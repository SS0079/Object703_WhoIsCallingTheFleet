using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Text;
using KittyHelpYouOut.ServiceClass;

namespace KittyHelpYouOut
{
    public class KittyPool : KittyMonoSingletonAuto<KittyPool>
    {
        public enum HideStyle
        {
            Faraway,
            Inactive
        }

        private class Pool
        {
            private Pool(){}

            public Pool(int maxSize)
            {
                available = new(maxSize);
                occupied = new(maxSize);
                max = maxSize;
            }
            
            private readonly KittyRingBuffer<GameObject> available;
            private readonly KittyRingBuffer<GameObject> occupied;
            private readonly int max;
            public int Count => available.Count + occupied.Count;

            public GameObject Get(GameObject prefab)
            {
                GameObject result = null;
                if (available.Count>0)
                {
                    //if there is available object, get object and move it to occupied
                    result = available.GetFromHead();
                    available.RemoveFromHead();
                    occupied.AddTail(result);
                }else if (Count < max)
                {
                    //if here is no available but count didnt reach max, instantiate game object and move it to occupied
                    result = Instantiate(prefab);
                    occupied.AddTail(result);
                }
                else
                {
                    //set eldest occupied object as result and return
                    result = occupied.GetFromHead();
                }
                return result;
            }

            public void Recycle(GameObject go)
            {
                for (int i = 0; i < occupied.Count; i++)
                {
                    var localGo = occupied.GetFromHead(i);
                    if (localGo==go)
                    {
                        occupied.RemoveFromHead(i);
                        available.AddTail(localGo);
                        return;
                    }
                }
                Debug.LogWarning($"{go.name} not found in occupied");
            }
        }
        private Dictionary<string, Pool> poolDic;
        public int poolMaxSize = 10000;
        private StringBuilder sb;
        private const char DASH = '_';
        private static Vector3 FAR_AWAY = new Vector3(0, 100000, 0);
        private const float HIT_DEP_DELY = 3f;
        private const float RIGHT_NOW = 0f;
        protected override void Awake()
        {
            base.Awake();
            poolDic = new(poolMaxSize);
        }

        //write a pool class that contain 2 list(or other collection),one for occupied pool object, the other for available object
        //the kitty pool should have a dictionary<string,pool>, to hold pools for all object, and differ them by game object name
        //every time try get a pool object, first check if there is a related pool in the dictionary
        //if pool exist, try get object from available list. if available list is empty, instantiate a new game object and add it to this list
        //if there is available object in the list, return that game object and move it from available to occupied
        //to recycle occupied object, move it from occupied to available
        //if sum count of occupied and available reach the maximum object count of the pool, fetch and return the eldest object in the occupied
        //if there is no such pool in the first place, add a pool to pool dic and populate it with available object, remember we should hide the populated fresh object
        
        private void AddPool(GameObject addObj)
        {
            var newPool = new Pool(poolMaxSize);
            poolDic.Add(addObj.gameObject.name, newPool);
        }

        // private GameObject AddPoolObject(GameObject prefab)
        // {
        //     if (!poolDic.TryGetValue(prefab.name, out var objectPool))
        //     {
        //         Debug.LogWarning($"Pool missing : {prefab.name}");
        //         return null;
        //     }
        //     GameObject newObj = Instantiate(prefab);
        //     if (!newObj.TryGetComponent(out KittyPoolObject poolObject))
        //     {
        //         poolObject=newObj.AddComponent<KittyPoolObject>();
        //     }
        //     sb.Clear();
        //     sb.Append(prefab.name);
        //     sb.Append(DASH);
        //     sb.Append(objectPool.Count);
        //     newObj.name = sb.ToString();
        //     occupationDic.Add(newObj.name,poolObject);
        //     objectPool.Enqueue(newObj);
        //     return newObj;
        // }

        /// <summary>
        /// 返回一个active的GameObject
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GameObject GetPoolObject(GameObject prefab, Vector3 pos = default, Quaternion rot = default, Transform parent=null)
        {
            GameObject objToSpawn;
            if (!poolDic.ContainsKey(prefab.name))
            {
                AddPool(prefab);
            }
            var pool = poolDic[prefab.name];
            objToSpawn=pool.Get(prefab);
            objToSpawn.transform.SetPositionAndRotation(pos, rot);
            objToSpawn.transform.SetParent(parent);
            objToSpawn.name = prefab.name;
            objToSpawn.SetActive(true);
            return objToSpawn;
        }

        public void RecyclePoolObject(GameObject go,HideStyle style)
        {
            if (!poolDic.TryGetValue(go.name, out var pool)) return;
            pool.Recycle(go);
            switch (style)
            {
                case HideStyle.Faraway:
                    go.transform.position = FAR_AWAY;
                    break;
                case HideStyle.Inactive:
                    go.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }

    }
    
    public static class PoolHelper
    {
        public static GameObject RecyclePoolObject(this GameObject go,KittyPool.HideStyle style=KittyPool.HideStyle.Inactive)
        {
            KittyPool.Instance.RecyclePoolObject(go,style);
            return go;
        }

        
        public static GameObject GetPoolObjectHere(this Transform reference, GameObject go,Transform parent=null)
        {
            return KittyPool.Instance.GetPoolObject(go, reference.position, reference.rotation);
        }


        public static GameObject GetPoolObject(this GameObject go,Vector3 pos=default,Quaternion rot=default,Transform parent=null)
        {
            return KittyPool.Instance.GetPoolObject(go, pos, rot, parent);
        }
    }
}


