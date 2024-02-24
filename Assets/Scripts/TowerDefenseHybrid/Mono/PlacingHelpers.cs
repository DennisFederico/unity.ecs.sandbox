using UnityEngine;

namespace TowerDefenseHybrid.Mono {
    
    public enum PlacementFacing {
        Up, //Z+
        Right, //X+
        Down, //Z-
        Left //X-
    }
    
    public static class PlacementFacingExtensions {
        
        public static PlacementFacing DefaultFacing = PlacementFacing.Up;
        
        public static PlacementFacing Next(this PlacementFacing facing) {
            switch (facing) {
                default:
                case PlacementFacing.Up:    return PlacementFacing.Right;
                case PlacementFacing.Right: return PlacementFacing.Down;
                case PlacementFacing.Down:  return PlacementFacing.Left;
                case PlacementFacing.Left:  return PlacementFacing.Up;
            }
        }
        
        public static float RotationAngle(this PlacementFacing facing) {
            switch (facing) {
                default:
                case PlacementFacing.Up:    return 0;
                case PlacementFacing.Right: return 90;
                case PlacementFacing.Down:  return 180;
                case PlacementFacing.Left:  return 270; 
            }
        }
        
        public static Quaternion Rotation(this PlacementFacing facing) {
            return Quaternion.Euler(0, facing.RotationAngle(), 0);
        }
    }
}