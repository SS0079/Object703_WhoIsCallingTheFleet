using System.Collections.Generic;
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
        protected override void Awake()
        {
            base.Awake();
            ResKit.Init();
            prefabLoader = ResLoader.Allocate();
            for (int i = 0; i < prefabNames.Length; i++)
            {
                var name = prefabNames[i];
                var result = prefabLoader.LoadSync<GameObject>(name);
                if (result==null) continue;
                prefabDic.TryAdd(name, result);
            }
        }
    }
}