using Unity.Mathematics;
using UnityEngine;
using Utils.Narkdagas.GridSystem;

namespace TowerDefenseHybrid.Mono {

    public class GridManager : MonoBehaviour {

        public static GridManager Instance;

        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private float cellSize = 2f;
        [SerializeField] private Vector3 origin;
        [SerializeField] private Transform pathWaypoints;
        [SerializeField] private Material pathwayMaterial;
        [SerializeField] private bool debugGrid;
        private Mesh _pathwayMesh;

        //TODO Should we have the cell center WorldPosition as part of the GridCellInfo?
        public struct GridObject {
            public int2 GridPosition;
            public bool IsBuildable;
            public string Name;

            public GridObject(int2 gridPosition, string name) {
                GridPosition = gridPosition;
                Name = name;
                IsBuildable = true;
            }

            public override string ToString() {
                return Name != null ? Name.Substring(0, 3) : IsBuildable ? $"[{GridPosition.x},{GridPosition.y}]" : "NB";
            }

            public bool IsOccupied() {
                return Name != null;
            }
        }

        //TODO EXTRA FUNCTIONALITY
        // - Clear Cell
        // - Check Cell
        // - Place Object of a given size/shape

        public GenericXZGrid<GridObject> Grid { get; private set; }

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                throw new System.Exception("An instance of this singleton already exists.");
            }

            Instance = this;
            Grid = new GenericXZGrid<GridObject>(origin, width, height, cellSize, (_, pos) => new GridObject(pos, null), debugGrid);

            _pathwayMesh = BuildPathwayMesh();
            if (debugGrid) {
                new GenericXZGridDebugVisual<GridObject>(Grid);
            }
        }

        private void Start() {
            PlaceNonBuildableCells();
        }

        private void PlaceNonBuildableCells() {
            if (pathWaypoints == null || pathWaypoints.childCount == 0) return;
            var prevWaypoint = pathWaypoints.GetChild(0).position;
            for (var i = 1; i < pathWaypoints.childCount; i++) {
                var waypoint = pathWaypoints.GetChild(i).position;
                var distance = Vector3.Distance(prevWaypoint, waypoint);
                var step = Grid.CellSize / 3;
                // Debug.Log($"from {prevWaypoint} to {waypoint} - distance {distance} - step {step}");
                var total = 0f;
                while (total <= distance) {
                    var worldPos = Vector3.Lerp(prevWaypoint, waypoint, (total / distance));
                    // Debug.Log($"total {total/distance}: worldPos {worldPos}");
                    var gridObject = Grid.GetGridObject(worldPos);
                    if (gridObject.IsBuildable) {
                        // Debug.Log($"Changing {gridObject.GridPosition} to NoBuild");
                        gridObject.IsBuildable = false;
                        Grid.SetGridObject(gridObject.GridPosition, gridObject);
                        //HACK BUILD THE PATHWAY
                        BuildPathwayTemplate(Grid.GetWorldPosition(gridObject.GridPosition, true) + Vector3.up * 0.001f);
                    }

                    total += step;
                }

                prevWaypoint = waypoint;
            }
        }
        
        private static Mesh BuildPathwayMesh() {
            var mesh = new Mesh {
                name = "SquareMesh",
                vertices = new[] { new Vector3(-1, 0, -1), new Vector3(-1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, -1) },
                uv = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) },
                triangles = new[] { 0, 1, 2, 0, 2, 3 }
            };
            mesh.RecalculateNormals();
            return mesh;
        }

        private void BuildPathwayTemplate(Vector3 position) {
            var go = new GameObject("Pathway", new[] { typeof(MeshRenderer), typeof(MeshFilter) });
            go.GetComponent<MeshFilter>().mesh = _pathwayMesh;
            go.GetComponent<MeshRenderer>().material = pathwayMaterial;
            go.transform.position = position;
        }
    }
}