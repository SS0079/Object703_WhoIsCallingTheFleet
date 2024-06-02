using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Object703.Authoring
{
    public struct SpawnSubControllerName : IComponentData
    {
        public FixedString512Bytes value;
    }
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GhostAuthoringComponent))]
    public class HaveSubControllerAuthoring : MonoBehaviour
    {
        public string[] names;
        class HaveSubControllerAuthoringBaker : Baker<HaveSubControllerAuthoring>
        {
            public override void Bake(HaveSubControllerAuthoring authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                if (authoring.names.Length>0)
                {
                    var sb = new StringBuilder();
                    
                    for (int i = 0; i < authoring.names.Length; i++)
                    {
                        if (i != 0) sb.Append('|');
                        sb.Append(authoring.names[i]);
                    }
                    AddComponent(self,new SpawnSubControllerName(){value = new FixedString512Bytes(sb.ToString())});
                    sb.Clear();
                }
            }
        }
    }
}