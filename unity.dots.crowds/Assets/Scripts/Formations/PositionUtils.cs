using Unity.Mathematics;

namespace Formations {
    public static class PositionUtils {

        public static float3 Position(this Formation formation, float radius, float fraction) {
            
            switch (formation) {
                case Formation.Circle:
                    return PointOnCircle(radius, fraction);
                case Formation.Square:
                    return InterpolateOverSquare(radius, fraction);
                case Formation.Line:
                    return PointOnLine(radius, fraction);
                default: return default;
            }
        }
        
        public static float3 Position(this Formation formation, float radius, float fraction, float3 origin) {
            
            switch (formation) {
                case Formation.Circle:
                    return PointOnCircle(radius, fraction, origin);
                case Formation.Square:
                    return InterpolateOverSquare(radius, fraction, origin);
                case Formation.Line:
                    return PointOnLine(radius, fraction, origin);
                default: return default;
            }
        }
        
        public static float3 PointOnCircle(float radius, float circleFraction) {
            return PointOnCircle(radius, circleFraction, float3.zero);
        }
        
        public static float3 PointOnCircle(float radius, float circleFraction, float3 origin) {
            
            circleFraction *= 360;
            // Convert from degrees to radians via multiplication by PI/180        
            float x = radius * math.cos(circleFraction * math.PI / 180F) + origin.x;
            float z = radius * math.sin(circleFraction * math.PI / 180F) + origin.z;

            return new float3(x, origin.y, z);
        }
        
        public static float3 PointOnSquare(float rectDimensions, float rectFraction) {
            return PointOnSquare(rectDimensions, rectFraction, float3.zero);
        }
        
        public static float3 InterpolateOverSquare(float side, float fraction) {
            return InterpolateOverSquare(side, fraction, float3.zero);
        }

        public static float3 InterpolateOverSquare(float side, float fraction, float3 origin) {
            
            float halfSide = side / 2;

            // Normalize t to the range of [0, 4)
            var clamp = math.clamp(fraction * 4, 0, 4);

            if (clamp < 1){
                // Top side (going left)
                return new float3(halfSide - (clamp * side) + origin.x, origin.y, -halfSide + origin.z);
            } else if (clamp < 2) {
                // Left side (going up)
                return new float3(-halfSide + origin.x, origin.y, (-halfSide + (clamp - 1) * side) + origin.z);
            } else if (clamp < 3) {
                // Bottom side (going right)
                return new float3(-halfSide + (clamp - 2) * side + origin.x, origin.y, halfSide + origin.z);
            } else {
                // Right side (going down)
                return new float3(halfSide + origin.x, origin.y, (halfSide - (clamp - 3) * side) + origin.z);
            }
        }
        
        
        public static float3 PointOnSquare(float rectDimensions, float rectFraction, float3 origin) {
            var target = PointOnCircle(rectDimensions * 2, rectFraction, origin);
            var minX = origin.x - rectDimensions;
            var minZ = origin.z - rectDimensions;
            var maxX = origin.x + rectDimensions;
            var maxZ = origin.z + rectDimensions;
            var x = target.x;
            var z = target.z;
            var midX = (minX + maxX) / 2;
            var midZ = (minZ + maxZ) / 2;
            // if (midX - x == 0) -> m == ±Inf -> minYx/maxYx == x (because value / ±Inf = ±0)
            var m = (midZ - z) / (midX - x);

            if (x <= midX) { // check "left" side
                var minXz = m * (minX - x) + z;
                if (minZ <= minXz && minXz <= maxZ)
                    return new float3(minX, 0 ,minXz);
            }

            if (x >= midX) { // check "right" side
                var maxXz = m * (maxX - x) + z;
                if (minZ <= maxXz && maxXz <= maxZ)
                    return new float3(maxX, 0, maxXz);
            }

            if (z <= midZ) { // check "top" side
                var minZx = (minZ - z) / m + x;
                if (minX <= minZx && minZx <= maxX)
                    return new float3(minZx, 0, minZ);
            }

            if (z >= midZ) { // check "bottom" side
                var maxZx = (maxZ - z) / m + x;
                if (minX <= maxZx && maxZx <= maxX)
                    return new float3(maxZx, 0,maxZ);
            }

            return new float3(x, origin.y, z);
        }

        public static float3 PointOnLine(float length, float lineFraction) {
            return PointOnLine(length, lineFraction, float3.zero);
        }
        
        public static float3 PointOnLine(float length, float lineFraction, float3 origin) {
            //TODO ADD DIRECTION VECTOR - ASSUMING FORWARD (Z)
            float interpolation = (lineFraction * length) - length/2;
            return new(origin.x, origin.y, origin.z + interpolation);
        }
    }
}