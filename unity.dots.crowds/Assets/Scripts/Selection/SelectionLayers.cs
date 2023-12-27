using System;
using UnityEngine;

namespace Selection {
    
    [Flags]
    public enum SelectionLayers {
        [InspectorName("Input Layer")]
        Input = 1 << 0,
        [InspectorName("Terrain Layer")]
        Terrain = 1 << 1,
        Tower = 1 << 2,
        Enemy = 1 << 3,
        Projectile = 1 << 4,
        Unit = 1 << 5,
        Selection = 1 << 6,
    }
}