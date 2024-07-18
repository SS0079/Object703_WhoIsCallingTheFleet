using System;
using Object703.Core;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Object703.Authoring
{
    [DisallowMultipleComponent]
    public class CanMoveAuthoring : MonoBehaviour
    {
        public enum MoveStyle
        {
            Arrow,
            Ship,
            Hover
        }
        public NetCodeConfig netConfig;
        public MoveStyle style;
        public ShipMoveConfig.AuthoringBox shipMoveConfig;
        public ArrowMoveConfig.AuthoringBox arrowMoveConfig; 
        public bool isHoming;
    }
    public class CanMoveAuthoringBaker : Baker<CanMoveAuthoring>
    {
        public override void Bake(CanMoveAuthoring authoring)
        {
            if (authoring.netConfig == null)
            {
                Debug.LogWarning($"Net code config missing!",authoring.gameObject);
                return;
            };
            var tickRate = authoring.netConfig.ClientServerTickRate.SimulationTickRate;
            var self = GetEntity(TransformUsageFlags.Dynamic);
            switch (authoring.style)
            {
                case CanMoveAuthoring.MoveStyle.Arrow:
                    AddComponent(self,authoring.arrowMoveConfig.ToComponentData(tickRate));
                    break;
                case CanMoveAuthoring.MoveStyle.Ship:
                    AddComponent(self,authoring.shipMoveConfig.ToComponentData(tickRate));
                    AddComponent(self,new MoveAxis());
                    AddComponent(self,new RotateAxis());
                    AddComponent(self,new MoveSpeed());
                    AddComponent(self,new RotateSpeed());
                    break;
                case CanMoveAuthoring.MoveStyle.Hover:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (authoring.isHoming)
            {
                AddComponent(self,new HomingTarget());
            }
        }
    }
}