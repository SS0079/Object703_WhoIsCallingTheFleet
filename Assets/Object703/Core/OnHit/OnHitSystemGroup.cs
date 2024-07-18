using Unity.Entities;
using Unity.NetCode;

namespace Object703.Core
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
    
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class BeforeHitSystemGroup : ComponentSystemGroup
    {
        
    }
}