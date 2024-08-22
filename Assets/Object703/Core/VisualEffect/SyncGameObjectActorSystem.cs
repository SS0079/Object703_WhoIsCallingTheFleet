using KittyHelpYouOut;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Object703.Core
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct SyncGameObjectActorSystem : ISystem
    {
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref SystemState state)
        {
            // sync position and rotation of entities and their game object
            foreach (var (gameObjectActor,ltw) in SystemAPI.Query<RefRW<GameObjectActor>,RefRO<LocalToWorld>>())
            {
                var actorTransform = gameObjectActor.ValueRW.Get().transform;
                actorTransform.position = ltw.ValueRO.Position;
                actorTransform.rotation = ltw.ValueRO.Rotation;
            }
            
            //sync the lineRenderer positions of entities
            foreach (var (line,weapon,ltw) in SystemAPI.Query<LineRendererActor,RefRO<Weapon_FanRange>,RefRO<LocalToWorld>>())
            {
                Vector3 fwd = ltw.ValueRO.Forward;
                Vector3 start = ltw.ValueRO.Position;
                var fanPoints = KittyMath.GetFanPoints(start,fwd,weapon.ValueRO.maxRange,weapon.ValueRO.minRange,weapon.ValueRO.ArcLimit,12,6);
                // var lineRenderer = line.go.GetComponent<LineRenderer>();
                var lineRenderer = line.value;
                lineRenderer.positionCount = fanPoints.Length;
                lineRenderer.SetPositions(fanPoints);
            }
        }

    }
}