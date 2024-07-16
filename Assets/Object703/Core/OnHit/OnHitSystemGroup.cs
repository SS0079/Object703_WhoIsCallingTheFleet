using Unity.Entities;
using Unity.NetCode;
using Random = Unity.Mathematics.Random;

namespace Object703.Core.OnHit
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial class OnHitSystemGroup : ComponentSystemGroup
    {
    }
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PredictedSimulationSystemGroup))]
    public partial class AfterHitSystemGroup : ComponentSystemGroup
    {
    }
}