using System.Collections.Generic;
using System.Text;
using KittyHelpYouOut;
using QFramework;
using UnityEngine;

namespace Object703.Utility
{
    public class AssetLoaderManager : KittyMonoSingletonManual<AssetLoaderManager>
    {
        public string[] prefabNames;
        public Dictionary<string, GameObject> prefabDic = new(64);
        private ResLoader prefabLoader;
        private StringBuilder sb;
        private ushort prefabCount;
        private void Start()
        {
            prefabLoader = ResLoader.Allocate();
            sb=new StringBuilder(100);
            for (int i = 0; i < prefabNames.Length; i++)
            {
                var name = prefabNames[i];
                var result = prefabLoader.LoadSync<GameObject>(name);
                if (result==null) continue;
                var success = prefabDic.TryAdd(name, result);
                if (success)
                {
                    sb.Append($"{++prefabCount}:{name}\n");
                }
            }
            Debug.Log(sb.ToString());
        }
    }
}