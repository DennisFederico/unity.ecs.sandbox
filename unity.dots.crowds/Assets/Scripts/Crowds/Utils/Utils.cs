using Unity.Mathematics;

namespace Crowds.Utils {
    public static class Utils {
        public static float3 NewRandomPosition(Random random) {
            return new float3 {
                x = random.NextFloat(-25, 25f),
                y = 0f,
                z = random.NextFloat(-25f, 25f)
            };
        }
    }
}