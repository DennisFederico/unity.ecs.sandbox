using AStar.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Utils.Narkdagas.PathFinding;


namespace AStar.MonoBehaviors {
    
    // IMPORTANT ... Use PathfindingEcsGridMono instead of this, as it handles the updates of obstacles
    public class GridSingletonComponentAuthoring : MonoBehaviour {
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private int cellSize;

        private class GridSingletonComponentAuthoringBaker : Baker<GridSingletonComponentAuthoring> {
            public override void Bake(GridSingletonComponentAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                
                AddComponent(entity, new GridSingletonComponent {
                    Width = authoring.width,
                    Height = authoring.height,
                    CellSize = authoring.cellSize,
                    Origin = authoring.transform.position
                });
                
                var gridBuffer = AddBuffer<PathNode>(entity);
                
                for (var i = 0; i < authoring.width * authoring.height; i++) {
                    gridBuffer.Add(new PathNode {
                        Index = i,
                        XY = new int2(i % authoring.width, i / authoring.width),
                        IsWalkable = true
                    });
                }

            }
        }
    }
}