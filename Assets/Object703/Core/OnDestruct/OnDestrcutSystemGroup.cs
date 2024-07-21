using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace Object703.Core
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup),OrderLast = true)]
    public partial class OnDestrcutSystemGroup : ComponentSystemGroup
    {
        
    }
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(PredictedSimulationSystemGroup))]
    public partial class AfterDestructSystemGroup : ComponentSystemGroup
    {
        
    }
    
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class BeforeDestructSystemGroup : ComponentSystemGroup
    {
        
    }
}