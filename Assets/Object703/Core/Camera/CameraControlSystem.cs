using System;
using Object703.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Object703.Core
{
     [Serializable]
     [GhostComponent(PrefabType = GhostPrefabType.Client)]
     public class CameraTargetReference : IComponentData
     {
         public Transform value;
     }
     
     [Serializable]
     [GhostComponent(PrefabType = GhostPrefabType.Client)]
     public struct NewCameraTargetTag : IComponentData
     {
        
     }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct CameraControlSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            // state.RequireForUpdate<GeneralData>();
            state.RequireForUpdate<PlayerTag>();

        }

        public void OnUpdate(ref SystemState state)
        {
            //if found an entity with fresh camera target tag, remove this tag and add a camera target reference, with a value of transform from mono scene
            // that make a connection between this entity and a camera target game object in mono scene
            
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (tag,entity) in SystemAPI.Query<RefRO<NewCameraTargetTag>>().WithAll<GhostOwnerIsLocal>().WithEntityAccess())
            {
                var targetTrans = CameraTargetManager.Instance.GetTarget();
                var targetRef = new CameraTargetReference { value = targetTrans };
                MixCamController.Instance.MixCam.ChildCameras[0].Follow = targetTrans;
                MixCamController.Instance.MixCam.ChildCameras[0].LookAt = targetTrans;
                ecb.RemoveComponent(entity,typeof(NewCameraTargetTag));
                ecb.AddComponent(entity,targetRef);
            }
            // ecb.Playback(state.EntityManager);
            // ecb.Dispose();
            
            // var freshCameraQueryEntities = SystemAPI.QueryBuilder().WithAll<FreshCameraTargetTag,GhostOwnerIsLocal>().Build().ToEntityArray(Allocator.Temp);
            // for (int i = 0; i < freshCameraQueryEntities.Length; i++)
            // {
            //     var entity = freshCameraQueryEntities[i];
            //     var targetTrans = CameraTargetManager.Instance.GetTarget();
            //     var targetRef = new CameraTargetReference { value = targetTrans };
            //     MixCamController.Instance.MixCam.ChildCameras[0].Follow = targetTrans;
            //     MixCamController.Instance.MixCam.ChildCameras[0].LookAt = targetTrans;
            //     state.EntityManager.RemoveComponent<FreshCameraTargetTag>(entity);
            //     state.EntityManager.AddComponentData(entity, targetRef);
            // }

            // run a job that sync the position of entity been targeted and the game object in mono scene on the main thread
            new SyncCameraTargetPositionJob().Run();
            // state.Dependency.Complete();
            // var localCameraAxis = SystemAPI.GetComponent<ControlAxis_Camera>(inputGatheringSysHandle);
            // if (localCameraAxis.value.x>0)
            // {
            //     var generalData = SystemAPI.GetSingleton<GeneralData>();
            //     generalData.curVisionIndex = (generalData.curVisionIndex + 1) % MixCamController.Instance.MixCam.ChildCameras.Length;
            //     SystemAPI.SetSingleton(generalData);
            //     MixCamController.Instance.MixSwitcher.SetPriority(generalData.curVisionIndex);
            // }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        public partial struct SyncCameraTargetPositionJob : IJobEntity
        {
            public void Execute(
                [EntityIndexInQuery] int index,
                in LocalTransform localTransform,
                CameraTargetReference reference)
            {
                if (reference.value == null) return;
                reference.value.position = localTransform.Position;
            }
        }
    }
}