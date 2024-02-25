using System;
using System.Collections;
using TowerDefenseBase.Helpers;
using Unity.Entities;
using UnityEngine;

namespace TowerDefenseHybrid.Mono {
    public class BuildingPlacementGhost : MonoBehaviour {
        [SerializeField] private Transform visualPrefab;
        [LayerField] [SerializeField] private int buildableLayer;
        [LayerField] [SerializeField] private int occupiedLayer;
        private Transform _currentVisual;

        private Transform Visual {
            get => _currentVisual;
            set {
                if (_currentVisual != null) {
                    Destroy(_currentVisual.gameObject);
                }
            
                visualPrefab = value;
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
            UpdateLayer(buildableLayer);
        }

        private void OnGridCellCandidateChange(PlayerInputManager.CellPlacementData newCellPlacementData) {
            //If the cell was not valid, change the transform to the mouse position, to avoid the ghost to "jump" when enabled again
            if (!_currentCellPlacementData.IsValid && newCellPlacementData.IsValid) {
                transform.position = newCellPlacementData.WorldPosition;
            }
            
            //If cell occupation changed or Buildable changed, update the layer
            if (_currentCellPlacementData.IsOccupied != newCellPlacementData.IsOccupied || _currentCellPlacementData.IsBuildable != newCellPlacementData.IsBuildable) {
                UpdateLayer(newCellPlacementData.IsOccupied || !newCellPlacementData.IsBuildable ? occupiedLayer : buildableLayer);
            }
            
            //Facing changed - TODO use SLERP during LateUpdate!
            if (_currentCellPlacementData.PlacementFacing != newCellPlacementData.PlacementFacing) {
                transform.rotation = newCellPlacementData.PlacementFacing.Rotation();
            }
            
            _currentCellPlacementData = newCellPlacementData;
            //offset target Y coordinate for the "placing" effect
            if (_currentCellPlacementData.IsValid) {
                var offsetPosition = _currentCellPlacementData.CellCenterWorldPosition + new Vector3(0f, .15f, 0f);
                _currentCellPlacementData.CellCenterWorldPosition = offsetPosition;
            }
            Visual.gameObject.SetActive(newCellPlacementData.IsValid);
            
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
                var targetPosition = _currentCellPlacementData.CellCenterWorldPosition;
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);
                //SLERP?
                // transform.rotation = Quaternion.Lerp(transform.rotation, GridBuildingSystem3D.Instance.GetPlacedObjectRotation(), Time.deltaTime * 15f);
            }
        }


        private void UpdateVisual() {
            _currentVisual = Instantiate(visualPrefab, Vector3.zero, Quaternion.identity);
            _currentVisual.parent = transform;
            _currentVisual.localPosition = Vector3.zero;
            _currentVisual.localEulerAngles = Vector3.zero;
            _currentVisual.gameObject.SetActive(false);
            SetLayerToParentRecursive(_currentVisual);
        }
        
        private void UpdateLayer(int layerMask) {
            transform.gameObject.layer = layerMask;
            SetLayerToParentRecursive(_currentVisual);
        }

        private static void SetLayerToParentRecursive(Transform childTransform) {
            childTransform.gameObject.layer = childTransform.parent.gameObject.layer;
            foreach (Transform child in childTransform) {
                SetLayerToParentRecursive(child);
            }
        }
    }
}