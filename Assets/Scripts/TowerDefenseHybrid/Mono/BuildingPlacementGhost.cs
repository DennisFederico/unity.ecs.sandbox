using System;
using System.Collections;
using TowerDefenseBase.Helpers;
using TowerDefenseBase.Scriptables;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TowerDefenseHybrid.Mono {
    public class BuildingPlacementGhost : MonoBehaviour {
        [SerializeField] private TurretDataSO[] turrets;
        [SerializeField] [LayerField] private int buildableLayer;
        [SerializeField] [LayerField] private int occupiedLayer;
        [SerializeField] private Material ghostRangeMaterial;
        private Transform _currentVisual;
        private TurretDataSO _currentTurretData;
        private Mesh _fovMesh;

        private TurretDataSO SelectedTurret {
            set {
                if (_currentVisual != null) {
                    Destroy(_currentVisual.gameObject);
                }
                _currentTurretData = value;
                UpdateVisual();
            }
        }

        private PlayerInputManager.CellPlacementData _currentCellPlacementData;
        
        private void OnEnable() {
            //HACK TO WAIT UNTIL THE MANAGER IS READY (WATCH OUT FOR INFINITE LOOPS)
            StartCoroutine(WaitEnable(
                () => PlayerInputManager.Instance,
                () => PlayerInputManager.Instance.OnGridCellCandidateChange += OnGridCellCandidateChange
            ));
        }

        private static IEnumerator WaitEnable(Func<bool> condition, Action action) {
            yield return new WaitUntil(condition);
            action();
            yield return action;
        }

        private void OnDisable() {
            PlayerInputManager.Instance.OnGridCellCandidateChange -= OnGridCellCandidateChange;
        }

        private void Start() {
            UpdateVisual();
            UpdateVisualLayer(buildableLayer);
            _fovMesh = new Mesh() {
                name = "FOVGhost"
            };
        }

        private void OnGridCellCandidateChange(PlayerInputManager.CellPlacementData newCellPlacementData) {
            if (_currentCellPlacementData.BuildingIndex != newCellPlacementData.BuildingIndex) {
                if (newCellPlacementData.BuildingIndex < 1 || newCellPlacementData.BuildingIndex > turrets.Length) {
                    SelectedTurret = null;
                } else {
                    SelectedTurret = turrets[newCellPlacementData.BuildingIndex - 1];
                }
            }
            
            //If the cell was not valid, change the transform to the mouse position, to avoid the ghost to "jump" when enabled again
            if (!_currentCellPlacementData.IsValid && newCellPlacementData.IsValid) {
                transform.position = newCellPlacementData.WorldPosition;
            }
            
            //If cell occupation changed or Buildable changed, update the layer
            if (_currentCellPlacementData.IsOccupied != newCellPlacementData.IsOccupied || _currentCellPlacementData.IsBuildable != newCellPlacementData.IsBuildable) {
                UpdateVisualLayer(newCellPlacementData.IsOccupied || !newCellPlacementData.IsBuildable ? occupiedLayer : buildableLayer);
            }
            
            _currentCellPlacementData = newCellPlacementData;
            //offset target Y coordinate for the "placing" effect
            if (_currentCellPlacementData.IsValid) {
                var offsetPosition = _currentCellPlacementData.CellCenterWorldPosition + new Vector3(0f, .15f, 0f);
                _currentCellPlacementData.CellCenterWorldPosition = offsetPosition;
            }
            if (_currentVisual) _currentVisual.gameObject.SetActive(newCellPlacementData.IsValid);
            
            //Like this is more snappy, less smooth and more accurate/performant? (no LateUpdate)
            //A middle ground would be to flag the LateUpdate for only a few frames after the candidate cell changes (coroutine?)
            //But need to handle the cancel/restart of the coroutine when the candidate cell changes again
            // if (candidateCellData.IsValid) {
            //     transform.position = candidateCellData.CellCenterWorldPosition;
            //     transform.rotation = Quaternion.identity;
            // }
        }

        private void LateUpdate() {
            if (_currentCellPlacementData.IsValid) {
                var currentTransform = transform;
                transform.position = Vector3.Lerp(currentTransform.position, _currentCellPlacementData.CellCenterWorldPosition, Time.deltaTime * 15f);
                //Spherical lerp for rotation
                transform.rotation = Quaternion.Slerp(currentTransform.rotation, _currentCellPlacementData.PlacementFacing.Rotation(), Time.deltaTime * 15f);
            }
        }


        private void UpdateVisual() {
            if (_currentTurretData == null)  return;
            _currentVisual = Instantiate(_currentTurretData.ghostPrefab, Vector3.zero, Quaternion.identity);
            _currentVisual.parent = transform;
            _currentVisual.localPosition = Vector3.zero;
            _currentVisual.localEulerAngles = Vector3.zero;
            _currentVisual.gameObject.SetActive(false);
            SetVisualLayerToParentRecursive(_currentVisual);
            BuildFoV(_currentVisual, _currentTurretData.fovAngle, _currentTurretData.fovRange);
        }

        private void BuildFoV(Transform currentVisual, float fovAngle, float fovRange) {
            var fovGo = new GameObject($"FOV-{currentVisual.name}");
            fovGo.AddComponent<MeshFilter>().mesh = RebuildFovMesh(fovAngle, fovRange);
            fovGo.AddComponent<MeshRenderer>().material = ghostRangeMaterial;
            fovGo.transform.parent = currentVisual;
            fovGo.transform.localPosition = Vector3.zero + Vector3.up * 1f;
            fovGo.transform.localEulerAngles = Vector3.zero;
        }

        private Mesh RebuildFovMesh(float fovAngle, float fovRange) {
            //More vertices gives a rounder shape
            var fovMeshResolution = fovRange;
            var numTriangles = (int) math.round(fovAngle * fovMeshResolution);
            var angleStep =  math.radians(fovAngle) / numTriangles;
            var vertices = new Vector3[numTriangles + 2];
            var triangles = new int[numTriangles * 3];
            var halfFov = math.radians(fovAngle/2);
            
            //Add the first vertex
            vertices[0] = Vector3.zero;
            //Add the vertex & triangles in between
            for (int i = 0; i < numTriangles; i++) {
                var angle = -halfFov + angleStep * (i-1); //Start from the left side of the FoV
                vertices[i+1] = new Vector3(math.sin(angle) * fovRange, 0, math.cos(angle) * fovRange);
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
            //Add the last vertex
            vertices[numTriangles+1] = new Vector3(math.sin(math.radians(halfFov)) * fovRange, 0, math.cos(math.radians(halfFov)) * fovRange);
            _fovMesh.Clear();
            _fovMesh.vertices = vertices;
            _fovMesh.triangles = triangles;
            _fovMesh.RecalculateNormals();
            return _fovMesh;
        }

        private void UpdateVisualLayer(int layerMask) {
            transform.gameObject.layer = layerMask;
            if (_currentVisual != null) SetVisualLayerToParentRecursive(_currentVisual);
        }

        private static void SetVisualLayerToParentRecursive(Transform childTransform) {
            childTransform.gameObject.layer = childTransform.parent.gameObject.layer;
            foreach (Transform child in childTransform) {
                SetVisualLayerToParentRecursive(child);
            }
        }
    }
}