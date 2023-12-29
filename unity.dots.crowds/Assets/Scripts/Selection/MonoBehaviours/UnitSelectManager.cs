using Selection.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Material = UnityEngine.Material;

namespace Selection.MonoBehaviours {
    [AddComponentMenu("Unit Select/Deselect Manager")]
    public class UnitSelectManager : MonoBehaviour {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private PhysicsCategoryTags belongsTo;
        [SerializeField] private PhysicsCategoryTags collidesWith;
        [SerializeField] private Material debugMaterial;

        private World _world;
        private float3 _mouseStartPos;
        private bool _isDragging;

        //A Singleton that keeps the buffer of ray-casts
        private Entity _rayCastBuffer;
        private Entity _meshVerticesBuffer;

        private void OnEnable() {
            mainCamera = mainCamera == null ? Camera.main : mainCamera;
            _world = World.DefaultGameObjectInjectionWorld;
            
            if (debugMaterial == null) {
                debugMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit")) {
                    color = new Color(1f,150/255f, 0f, 145/255f)
                };
            }

            if (_world.IsCreated) {
                if (!_world.EntityManager.Exists(_rayCastBuffer)) {
                    _rayCastBuffer = _world.EntityManager.CreateSingletonBuffer<RayCastBufferComponent>();
                }

                if (!_world.EntityManager.Exists(_meshVerticesBuffer)) {
                    _meshVerticesBuffer = _world.EntityManager.CreateSingletonBuffer<SelectionVerticesBufferComponent>();
                }
            }
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                _mouseStartPos = Input.mousePosition;
            }

            if (Input.GetMouseButton(0) && !_isDragging) {
                if (math.distance(_mouseStartPos, Input.mousePosition) > 25) {
                    _isDragging = true;
                }
            }

            if (Input.GetMouseButtonUp(0)) {
                if (!_isDragging) {
                    SelectSingleUnit();
                } else {
                    SelectMultipleUnits();
                    _isDragging = false;
                }
            }
        }

        private void SelectSingleUnit() {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            _world.EntityManager.GetBuffer<RayCastBufferComponent>(_rayCastBuffer).Add(new RayCastBufferComponent() {
                Value = new RaycastInput() {
                    Start = ray.origin,
                    End = ray.GetPoint(mainCamera.farClipPlane),
                    Filter = new CollisionFilter() {
                        BelongsTo = belongsTo.Value,
                        CollidesWith = collidesWith.Value,
                    }
                },
                Additive = Input.GetKey(KeyCode.LeftShift)
            });
        }

        private void SelectMultipleUnits() {
            var topLeft = math.min(_mouseStartPos, Input.mousePosition);
            var botRight = math.max(_mouseStartPos, Input.mousePosition);

            var rect = Rect.MinMaxRect(topLeft.x, topLeft.y, botRight.x, botRight.y);
            var cornerRays = new[] {
                mainCamera.ScreenPointToRay(rect.min),
                mainCamera.ScreenPointToRay(rect.max),
                mainCamera.ScreenPointToRay(new Vector2(rect.xMin, rect.yMax)),
                mainCamera.ScreenPointToRay(new Vector2(rect.xMax, rect.yMin))
            };

            //CREATE THE VERTICES FOR THE MESH
            var rayDistance = 50f; //mainCamera.farClipPlane;
            var nVertices = new NativeArray<float3>(5, Allocator.TempJob);
            for (int i = 0; i < cornerRays.Length; i++) {
                nVertices[i] = cornerRays[i].GetPoint(rayDistance);
                Debug.DrawLine(cornerRays[i].GetPoint(mainCamera.nearClipPlane), cornerRays[i].GetPoint(mainCamera.farClipPlane), Color.red, 5f);
            }
            //The center of the near clip plane, better if it is the intersection of the rays with the nearClip (4 more points)
            var cameraTransform = mainCamera.transform;
            nVertices[4] = cameraTransform.position + (cameraTransform.forward * mainCamera.nearClipPlane);
            
            //DebugCollisionMeshGo(nVertices);
            //BoxCollider();
            //DebugCollisionMeshEntity(nVertices);
            
            // PUSH THE DATA TO THE BUFFER
             _world.EntityManager.GetBuffer<SelectionVerticesBufferComponent>(_meshVerticesBuffer).Add(new SelectionVerticesBufferComponent() {
                 Vertices = nVertices,
                 Additive = Input.GetKey(KeyCode.LeftShift),
                 belongsTo = belongsTo,
                 collidesWith = collidesWith
             });
            // nVertices.Dispose();
        }

        private void DebugCollisionMeshGo(NativeArray<float3> nVertices) {
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit")) {
                color = Color.yellow
            };
            
            //CREATE THE TRIANGLES FOR THE MESH
            var triangles = new[] {
                4, 0, 2,
                4, 2, 1,
                4, 1, 3,
                4, 3, 0,
                0, 1, 2,
                0, 3, 1
            };

            var mesh = new Mesh() {
                name = "Selection Mesh",
                vertices = new Vector3[] {
                    nVertices[0],
                    nVertices[1],
                    nVertices[2],
                    nVertices[3],
                    nVertices[4],
                },
                triangles = triangles
            };

            var go = new GameObject("CollidingMesh");
            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            material.doubleSidedGI = true;
            mr.material = material;
        }

        private void BoxCollider() {
            
            var entity = _world.EntityManager.CreateEntity();
            _world.EntityManager.AddSharedComponent(entity, new PhysicsWorldIndex());

            var physicsMaterial = Unity.Physics.Material.Default;
            physicsMaterial.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;
            var collisionFilter = new CollisionFilter {
                BelongsTo = belongsTo.Value,
                CollidesWith = collidesWith.Value
            };
            var boxCollider = Unity.Physics.BoxCollider.Create(new BoxGeometry() {
                Orientation = quaternion.identity,
                Size = new float3(1.0f),
                BevelRadius = 0.0f
            }, collisionFilter, physicsMaterial);
            var colliderComponent = new PhysicsCollider { Value = boxCollider };
            
            _world.EntityManager.AddComponentData(entity, colliderComponent);
        }
        
        private void DebugCollisionMeshEntity(NativeArray<float3> nVertices, bool renderMesh = false) {
            
            var entity = _world.EntityManager.CreateEntity();
            _world.EntityManager.AddComponentData(entity, new LocalToWorld {Value = float4x4.identity});
            _world.EntityManager.AddSharedComponent(entity, new PhysicsWorldIndex());
            
            if (renderMesh) {
                //CREATE THE TRIANGLES FOR THE MESH
                var triangles = new[] {
                    4, 0, 2,
                    4, 2, 1,
                    4, 1, 3,
                    4, 3, 0,
                    0, 1, 2,
                    0, 3, 1
                };
                
                //MESH USING UNITY GRAPHICS
                var renderMeshDescription = new RenderMeshDescription(ShadowCastingMode.Off);
                var mesh = new Mesh() {
                    name = "Selection Mesh",
                    vertices = new Vector3[] {
                        nVertices[0],
                        nVertices[1],
                        nVertices[2],
                        nVertices[3],
                        nVertices[4],
                    },
                    triangles = triangles,
                };
                var renderMeshArray = new RenderMeshArray(new[] { debugMaterial }, new[] { mesh });
                RenderMeshUtility.AddComponents(entity,
                    _world.EntityManager,
                    renderMeshDescription,
                    renderMeshArray,
                    MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0)
                );
            }
            
            var physicsMaterial = Unity.Physics.Material.Default;
            physicsMaterial.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;
            var collisionFilter = new CollisionFilter {
                BelongsTo = belongsTo.Value,
                CollidesWith = collidesWith.Value
            };
            
            var selectionCollider = ConvexCollider.Create(nVertices, ConvexHullGenerationParameters.Default, collisionFilter, physicsMaterial);
            _world.EntityManager.AddComponentData(entity, new PhysicsCollider() { Value = selectionCollider });
            _world.EntityManager.AddComponentData(entity, new SelectionColliderDataComponent());
        }

        private void OnGUI() {
            if (_isDragging) {
                var rect = SelectionGUI.GetScreenRect(_mouseStartPos, Input.mousePosition);
                SelectionGUI.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.1f));
                SelectionGUI.DrawScreenRectBorder(rect, 1, Color.blue);
            }
        }

        private void OnDisable() {
            if (_world.IsCreated) {
                if (_world.EntityManager.Exists(_rayCastBuffer)) {
                    _world.EntityManager.DestroyEntity(_rayCastBuffer);
                }

                if (_world.EntityManager.Exists(_meshVerticesBuffer)) {
                    _world.EntityManager.DestroyEntity(_meshVerticesBuffer);
                }
            }
        }
    }
}