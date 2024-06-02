using System;
using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Object703.Core.Utilities
{
    [Serializable]
    public struct IndividualRandom : IComponentData
    {
        public Random value;
    }
}