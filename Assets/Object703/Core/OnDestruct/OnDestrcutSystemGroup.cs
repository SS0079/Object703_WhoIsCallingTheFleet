using Unity.Entities;
using Unity.NetCode;

namespace Object703.Core
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup),OrderLast = true)]
    public partial class OnDestrcutSystemGroup : ComponentSystemGroup
    {
        
    }
    
    [UpdateInGroup(typeof(SimulationSystemGroup),OrderLast = true)]
    public partial class AfterDestructSystemGroup : ComponentSystemGroup
    {
        
    }
    
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class BeforeDestructSystemGroup : ComponentSystemGroup
    {
        
    }
}