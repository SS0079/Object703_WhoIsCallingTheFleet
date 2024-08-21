using System;
using System.Collections.Generic;
using System.Text;
using KittyHelpYouOut;
using QFramework;
using UnityEngine;

namespace Object703.Utility
{
    public class AssetLoaderManager : KittyMonoSingletonManual<AssetLoaderManager>
    {
        public GameObject[] prefabNames;
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
                var assetName = prefabNames[i].name;
                GameObject result=null;
                try
                {
                    result = prefabLoader.LoadSync<GameObject>(assetName);
                }
                catch (Exception _)
                {
                    KittyDebug.Instance.LogError($"res load error: {assetName}");
                }
                if (result==null) continue;
                
                // add a logic to distinct Local and Remote child, then add to dic with distinct key
                var childCount = result.transform.childCount;
                if (childCount<=0)
                {
                    KittyDebug.Instance.LogError($"game object asset for actor must have at lest 1 child as actual asset for Local and Remote effect display\n" +
                                        $"error game object: {assetName}");
                    continue;
                }
                var localName = $"{assetName}_Local";
                var otherName = $"{assetName}_Other";
                if (childCount==1)
                {
                    var child = result.transform.GetChild(0);
                    // add same asset to both Local and Remote key if there is only 1 child
                    if (prefabDic.TryAdd(localName, child.gameObject))
                    {
                        sb.Append($"{++prefabCount}:{localName} | {result.name} -> {child.name}\n");
                    }
                    if (prefabDic.TryAdd(otherName, child.gameObject))
                    {
                        sb.Append($"{++prefabCount}:{otherName} | {result.name} -> {child.name}\n");
                    }
                }
                else
                {
                    for (int j = 0; j < 2; j++)
                    {
                        var child = result.transform.GetChild(j);
                        if (child.name=="Local")
                        {
                            if (prefabDic.TryAdd(localName, child.gameObject))
                            {
                                sb.Append($"{++prefabCount}:{localName} | {result.name} -> {child.name}\n");
                            }
                        }
                        else
                        {
                            if (prefabDic.TryAdd(otherName, child.gameObject))
                            {
                                sb.Append($"{++prefabCount}:{otherName} | {result.name} -> {child.name}\n");
                            }
                        }
                    }
                }
                
            }
            KittyDebug.Instance.Log(sb.ToString());
        }
    }
}