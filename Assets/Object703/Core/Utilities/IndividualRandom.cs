using System;
using Unity.Entities;
using Unity.NetCode;
using Random = Unity.Mathematics.Random;

namespace Object703.Core
{
    [Serializable]
    public struct IndividualRandom : IComponentData
    {
        public Random value;
    }
}