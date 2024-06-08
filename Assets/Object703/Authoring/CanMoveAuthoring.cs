using System;
using Object703.Core.Combat;
using Object703.Core.Moving;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.Serialization;

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
        public MoveStyle style;
        public ShipMoveConfig.AuthoringBox shipMoveConfig;
        public ArrowMoveConfig.AuthoringBox arrowMoveConfig; 
        public bool isHoming;
    }
    public class CanMoveAuthoringBaker : Baker<CanMoveAuthoring>
    {
        public override void Bake(CanMoveAuthoring authoring)
        {
            if (NetCodeConfig.Global == null) return;
            var tickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
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