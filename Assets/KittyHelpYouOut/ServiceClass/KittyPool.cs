using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace KittyHelpYouOut
{
    public class KittyPool : KittyMonoSingletonAuto<KittyPool>
    {
        private Dictionary<string, Queue<GameObject>> GameObjectPoolDictionary=new Dictionary<string, Queue<GameObject>>();
        public int poolIniSize = 1;
        public int poolMaxSize = 10000;


        private void AddGameObjectPool(GameObject addObj)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < poolIniSize; i++)
            {
                GameObject obj = Instantiate(addObj);
                obj.gameObject.SetActive(false);
                obj.name = addObj.name;
                objectPool.Enqueue(obj);
            }
            GameObjectPoolDictionary.Add(addObj.gameObject.name, objectPool);
        }


        /// <summary>
        /// 返回一个active的GameObject
        /// </summary>
        /// <param name="go"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="parent"></param>
        /// <param name="setActive"></param>
        /// <returns></returns>
        public GameObject GetPoolObject(GameObject go, Vector3 pos = default, Quaternion rot = default, Transform parent=null)
        {
            GameObject objToSpawn;
            if (!GameObjectPoolDictionary.ContainsKey(go.name))
            {
                AddGameObjectPool(go);
            }
            Queue<GameObject> queue = GameObjectPoolDictionary[go.name];
            objToSpawn = Bubble(queue);
            if (objToSpawn==null)
            {
                if (queue.Count<poolMaxSize)
                {
                    objToSpawn = Instantiate(go);
                }
                else
                {
                    objToSpawn = queue.Dequeue();
                    objToSpawn.Recycle();
                }
            }
            queue.Enqueue(objToSpawn);
            objToSpawn.transform.SetPositionAndRotation(pos, rot);
            objToSpawn.transform.SetParent(parent);
            objToSpawn.name = go.name;
            objToSpawn.SetActive(true);
            return objToSpawn;
        }
        //================================================================================

        private GameObject Bubble(Queue<GameObject> queue)
        {
            for (int i = 0; i < queue.Count; i++)
            {
                while (queue.Count>0 && queue.Peek() == null)
                {
                    queue.Dequeue();
                }
                if (queue.Count==0)
                {
                    return null;
                }
                if (queue.Peek().activeSelf)
                {
                    queue.Enqueue(queue.Dequeue());
                }
                else
                {
                    return queue.Dequeue();
                }
            }
            return null;
        }

    }
    
    public static class PoolHelper
    {
        private static Vector3 FAR_AWAY = new Vector3(0, -100000, 0);
        private const float HIT_DEP_DELY = 3f;
        private const float RIGHT_NOW = 0f;
  

        public static GameObject Recycle(this GameObject go)
        {
            ExecuteRecycle(go);
            return go;
        }

        public static GameObject RecycleOnTime(this GameObject go,float second)
        {
            KittyCoroutine.Instance.StartCoroutine(ExecuteRecycleOnTime(go, second));
            return go;
        }


        public static MonoBehaviour RecycleOnTime(this MonoBehaviour cp, float second)
        {
            cp.StartCoroutine(ExecuteRecycleOnTime(cp.gameObject, second));
            return cp;
        }

        public static T Recycle<T>(this T cpt)where T : MonoBehaviour
        {
            ExecuteRecycle(cpt.gameObject);
            return cpt;
        }

        public static MonoBehaviour DelayedRecycle(this MonoBehaviour mono, float second=3f)
        {
            mono.StartCoroutine(ExecuteDelayedRecycle(mono.gameObject,second));
            return mono;
        }

        public static GameObject DelayedRecycle(this GameObject go, float second=3f)
        {
            KittyCoroutine.Instance.StartCoroutine(ExecuteDelayedRecycle(go, second));
            return go;
        }

        private static void ExecuteRecycle(GameObject go)
        {
            go.SetActive(false);
            go.transform.position = FAR_AWAY;
        }

        private static IEnumerator ExecuteDelayedRecycle(GameObject go, float second)
        {
            go.transform.position = FAR_AWAY;
            yield return new WaitForSeconds(second);
            go.SetActive(false); 
        }

        private static IEnumerator ExecuteRecycleOnTime(GameObject go,float second)
        {
            yield return new WaitForSeconds(second);
            go.SetActive(false);
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


