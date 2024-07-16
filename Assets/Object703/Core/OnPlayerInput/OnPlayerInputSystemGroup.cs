using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace Object703.Core.OnPlayerInput
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class OnPlayerInputSystemGroup : ComponentSystemGroup
    {
        
    }
}