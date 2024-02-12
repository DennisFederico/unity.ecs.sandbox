using Selection.Components;
using Selection.Systems;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Selection.MonoBehaviours {
    [AddComponentMenu("Unit Select/Deselect Manager")]
    public class UnitSelectManager : MonoBehaviour {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private PhysicsCategoryTags belongsTo;
        [SerializeField] private PhysicsCategoryTags collidesWith;
        // [SerializeField] private Material debugMaterial;
        [SerializeField] private TextMeshProUGUI counterText;

        private World _world;
        private SelectedCountEventSystem _selectedCountEventSystem;
        // private float3 _mouseStartPos;
        private Vector2 _mouseStartPos;
        private bool _isDragging;

        //A Singleton that keeps the buffer of ray-casts
        private Entity _rayCastBuffer;
        private Entity _meshVerticesBuffer;
        private Entity _meshBoxBuffer;

        private void OnEnable() {
            mainCamera = mainCamera == null ? Camera.main : mainCamera;
            _world = World.DefaultGameObjectInjectionWorld;

            // if (debugMaterial == null) {
            //     debugMaterial = new Material(Shader.Find($"Universal Render Pipeline/Unlit")) {
            //         color = new Color(1f, 150 / 255f, 0f, 145 / 255f)
            //     };
            // }

            if (_world.IsCreated) {
                if (!_world.EntityManager.Exists(_rayCastBuffer)) {
                    _rayCastBuffer = _world.EntityManager.CreateSingletonBuffer<RayCastBufferComponent>();
                }

                if (!_world.EntityManager.Exists(_meshVerticesBuffer)) {
                    _meshVerticesBuffer = _world.EntityManager.CreateSingletonBuffer<SelectionVerticesBufferComponent>();
                }

                if (!_world.EntityManager.Exists(_meshBoxBuffer)) {
                    _meshBoxBuffer = _world.EntityManager.CreateSingletonBuffer<SelectionBoxBufferComponent>();
                }

                _selectedCountEventSystem = _world.GetOrCreateSystemManaged<SelectedCountEventSystem>();
                _selectedCountEventSystem.OnSelectedCountChanged += OnSelectedCountChanged;
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

                if (_world.EntityManager.Exists(_meshBoxBuffer)) {
                    _world.EntityManager.DestroyEntity(_meshBoxBuffer);
                }
                
                if (_selectedCountEventSystem != null) {
                    _selectedCountEventSystem.OnSelectedCountChanged -= OnSelectedCountChanged;
                }
            }
        }

        private void OnSelectedCountChanged(int count) {
            counterText.text = $"Selected: {count}";
        }

        private void Update() {
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                _mouseStartPos = Mouse.current.position.ReadValue();
            }

            if (Mouse.current.leftButton.isPressed && !_isDragging) {
                if (math.distance(_mouseStartPos, Mouse.current.position.ReadValue()) > 5) {
                    _isDragging = true;
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame) {
                if (!_isDragging) {
                    SelectSingleUnit(Keyboard.current.leftShiftKey.isPressed);
                } else {
                    SelectMultipleUnitsUsingPrism(Keyboard.current.leftShiftKey.isPressed);
                    // SelectMultipleUnitsUsingBox(Keyboard.current.leftShiftKey.isPressed);
                }
                _isDragging = false;
            }
        }

        private void SelectSingleUnit(bool additive = false) {
            // Debug.Log("Send Selection for single unit");
            var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            _world.EntityManager.GetBuffer<RayCastBufferComponent>(_rayCastBuffer).Add(new RayCastBufferComponent() {
                Value = new RaycastInput() {
                    Start = ray.origin,
                    End = ray.GetPoint(mainCamera.farClipPlane),
                    Filter = new CollisionFilter() {
                        BelongsTo = belongsTo.Value,
                        CollidesWith = collidesWith.Value,
                    }
                },
                Additive = additive
            });
        }

        // ReSharper disable once UnusedMember.Local
        private void SelectMultipleUnitsUsingBox(bool additive = false) {
            // Debug.Log("Send Box Data for multiple units selection");
            var topLeft = math.min(_mouseStartPos, Mouse.current.position.ReadValue());
            var botRight = math.max(_mouseStartPos, Mouse.current.position.ReadValue());
            var rect = Rect.MinMaxRect(topLeft.x, topLeft.y, botRight.x, botRight.y);
            
            var zSize = mainCamera.farClipPlane - mainCamera.nearClipPlane;
            var centerRay = mainCamera.ScreenPointToRay(rect.center);
            var boxCenter = centerRay.GetPoint(zSize / 2);

            //Using nearClip "smaller" box
            var leftBottom = mainCamera.ScreenToWorldPoint(new Vector3(rect.min.x, rect.min.y, mainCamera.farClipPlane));
            var rightTop = mainCamera.ScreenToWorldPoint(new Vector3(rect.max.x, rect.max.y, mainCamera.farClipPlane));
            var distance = leftBottom - rightTop;
            var xSize = math.abs(distance.x);
            var ySize = math.abs(distance.y);
                
            //Center adjustment?
            //boxCenter -= mainCamera.transform.forward * mainCamera.nearClipPlane;
            var transformPosition = boxCenter - mainCamera.transform.position;
            var orientation = Quaternion.LookRotation(transformPosition, Vector3.up);


            // PUSH THE DATA TO THE BUFFER
            _world.EntityManager.GetBuffer<SelectionBoxBufferComponent>(_meshBoxBuffer).Add(new SelectionBoxBufferComponent() {
                BoxCenter = boxCenter,
                BoxSize = new float3(xSize, ySize, zSize),
                BoxOrientation = orientation,
                Additive = additive,
                BelongsTo = belongsTo,
                CollidesWith = collidesWith
            });
        }

        //DebugBoxMesh(boxCenter, new float3(xSize, ySize, zSize), orientation);

        private void SelectMultipleUnitsUsingPrism(bool additive = false) {
            // Debug.Log("Send Selection for multiple units");
            var topLeft = math.min(_mouseStartPos, Mouse.current.position.ReadValue());
            var botRight = math.max(_mouseStartPos, Mouse.current.position.ReadValue());

            var rect = Rect.MinMaxRect(topLeft.x, topLeft.y, botRight.x, botRight.y);
            var cornerRays = new[] {
                mainCamera.ScreenPointToRay(rect.min),
                mainCamera.ScreenPointToRay(rect.max),
                mainCamera.ScreenPointToRay(new Vector2(rect.xMin, rect.yMax)),
                mainCamera.ScreenPointToRay(new Vector2(rect.xMax, rect.yMin))
            };

            //CREATE THE VERTICES FOR THE MESH
            var rayDistance = mainCamera.farClipPlane;
            var nVertices = new NativeArray<float3>(5, Allocator.TempJob);
            for (int i = 0; i < cornerRays.Length; i++) {
                nVertices[i] = cornerRays[i].GetPoint(rayDistance);
                // Debug.DrawLine(cornerRays[i].GetPoint(mainCamera.nearClipPlane), cornerRays[i].GetPoint(mainCamera.farClipPlane), Color.red, 5f);
            }

            //The center of the near clip plane, better if it is the intersection of the rays with the nearClip (4 more points)
            var cameraTransform = mainCamera.transform;
            nVertices[4] = cameraTransform.position + (cameraTransform.forward * mainCamera.nearClipPlane);

            // if (debugMesh) {
            //DebugCollisionMeshGo(nVertices);
            // BoxCollider();
            // DebugCollisionMeshEntity(nVertices);    
            // }

            // PUSH THE DATA TO THE BUFFER
            _world.EntityManager.GetBuffer<SelectionVerticesBufferComponent>(_meshVerticesBuffer).Add(new SelectionVerticesBufferComponent() {
                Vertices = nVertices,
                Additive = additive,
                BelongsTo = belongsTo,
                CollidesWith = collidesWith
            });
            // nVertices.Dispose();
        }

        // ReSharper disable once UnusedMember.Local
        // private void DebugCollisionMeshGo(NativeArray<float3> nVertices) {
        //     var material = new Material(Shader.Find($"Universal Render Pipeline/Unlit")) {
        //         color = Color.yellow
        //     };
        //
        //     //CREATE THE TRIANGLES FOR THE MESH
        //     var triangles = new[] {
        //         4, 0, 2,
        //         4, 2, 1,
        //         4, 1, 3,
        //         4, 3, 0,
        //         0, 1, 2,
        //         0, 3, 1
        //     };
        //
        //     var mesh = new Mesh() {
        //         name = "Selection Mesh",
        //         vertices = new Vector3[] {
        //             nVertices[0],
        //             nVertices[1],
        //             nVertices[2],
        //             nVertices[3],
        //             nVertices[4],
        //         },
        //         triangles = triangles
        //     };
        //
        //     var go = new GameObject("CollidingMesh");
        //     var mf = go.AddComponent<MeshFilter>();
        //     mf.mesh = mesh;
        //     var mr = go.AddComponent<MeshRenderer>();
        //     material.doubleSidedGI = true;
        //     mr.material = material;
        // }

        // ReSharper disable once UnusedMember.Local
        // private void DebugBoxMesh(float3 center, float3 size, quaternion orientation) {
        //     GameObject.CreatePrimitive(PrimitiveType.Cube).transform.SetPositionAndRotation(center, orientation);
        //
        //     var entity = _world.EntityManager.CreateEntity();
        //     _world.EntityManager.AddSharedComponent(entity, new PhysicsWorldIndex());
        //     
        //     var physicsMaterial = Unity.Physics.Material.Default;
        //     physicsMaterial.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;
        //     var collisionFilter = new CollisionFilter {
        //         BelongsTo = belongsTo.Value,
        //         CollidesWith = collidesWith.Value
        //     };
        //     var boxCollider = Unity.Physics.BoxCollider.Create(new BoxGeometry() {
        //         Orientation = quaternion.identity,
        //         Size = new float3(size),
        //         BevelRadius = 0.0f
        //     }, collisionFilter, physicsMaterial);
        //     var colliderComponent = new PhysicsCollider { Value = boxCollider };
        //     
        //     _world.EntityManager.AddComponentData(entity, colliderComponent);
        // }

        // ReSharper disable once UnusedMember.Local
        // private void DebugCollisionMeshEntity(NativeArray<float3> nVertices, bool renderMesh = false) {
        //     var entity = _world.EntityManager.CreateEntity();
        //     _world.EntityManager.AddComponentData(entity, new LocalToWorld { Value = float4x4.identity });
        //     _world.EntityManager.AddSharedComponent(entity, new PhysicsWorldIndex());
        //
        //     if (renderMesh) {
        //         //CREATE THE TRIANGLES FOR THE MESH
        //         var triangles = new[] {
        //             4, 0, 2,
        //             4, 2, 1,
        //             4, 1, 3,
        //             4, 3, 0,
        //             0, 1, 2,
        //             0, 3, 1
        //         };
        //
        //         //MESH USING UNITY GRAPHICS
        //         var renderMeshDescription = new RenderMeshDescription(ShadowCastingMode.Off);
        //         var mesh = new Mesh() {
        //             name = "Selection Mesh",
        //             vertices = new Vector3[] {
        //                 nVertices[0],
        //                 nVertices[1],
        //                 nVertices[2],
        //                 nVertices[3],
        //                 nVertices[4],
        //             },
        //             triangles = triangles,
        //         };
        //         var renderMeshArray = new RenderMeshArray(new[] { debugMaterial }, new[] { mesh });
        //         RenderMeshUtility.AddComponents(entity,
        //             _world.EntityManager,
        //             renderMeshDescription,
        //             renderMeshArray,
        //             MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0)
        //         );
        //     }
        //
        //     var physicsMaterial = Unity.Physics.Material.Default;
        //     physicsMaterial.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;
        //     var collisionFilter = new CollisionFilter {
        //         BelongsTo = belongsTo.Value,
        //         CollidesWith = collidesWith.Value
        //     };
        //
        //     var selectionCollider = ConvexCollider.Create(nVertices, ConvexHullGenerationParameters.Default, collisionFilter, physicsMaterial);
        //     _world.EntityManager.AddComponentData(entity, new PhysicsCollider() { Value = selectionCollider });
        //     _world.EntityManager.AddComponentData(entity, new SelectionColliderDataComponent());
        // }

        private void OnGUI() {
            if (_isDragging) {
                var rect = SelectionGUI.GetScreenRect(_mouseStartPos, Mouse.current.position.ReadValue());
                SelectionGUI.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.1f));
                SelectionGUI.DrawScreenRectBorder(rect, 1, Color.blue);
            }
        }
    }
}