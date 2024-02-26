using UnityEngine;

namespace TowerDefenseBase.Helpers {

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
                case PlacementFacing.Up: return PlacementFacing.Right;
                case PlacementFacing.Right: return PlacementFacing.Down;
                case PlacementFacing.Down: return PlacementFacing.Left;
                case PlacementFacing.Left: return PlacementFacing.Up;
            }
        }

        public static PlacementFacing Prev(this PlacementFacing facing) {
            switch (facing) {
                default:
                case PlacementFacing.Up: return PlacementFacing.Left;
                case PlacementFacing.Right: return PlacementFacing.Up;
                case PlacementFacing.Down: return PlacementFacing.Right;
                case PlacementFacing.Left: return PlacementFacing.Down;
            }
        }

        public static PlacementFacing ChangeBy(this PlacementFacing facing, int value) {
            if (value == 0) return facing;
            if (value > 0) {
                for (var i = 0; i < value; i++) {
                    facing = facing.Next();
                }
            } else {
                for (var i = 0; i < -value; i++) {
                    facing = facing.Prev();
                }
            }
            return facing;
        }

        public static float RotationAngle(this PlacementFacing facing) {
            switch (facing) {
                default:
                case PlacementFacing.Up: return 0;
                case PlacementFacing.Right: return 90;
                case PlacementFacing.Down: return 180;
                case PlacementFacing.Left: return 270;
            }
        }

        public static Quaternion Rotation(this PlacementFacing facing) {
            return Quaternion.Euler(0, facing.RotationAngle(), 0);
        }
    }
}