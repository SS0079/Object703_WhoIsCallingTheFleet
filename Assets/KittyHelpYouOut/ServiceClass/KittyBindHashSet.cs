using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KittyHelpYouOut.ServiceClass
{
    /// <summary>
    /// 猫猫可绑定哈希表！
    /// </summary>
    public class KittyBindHashSet<T> : IEnumerable<T>
    {
        private KittyBindHashSet()
        {
        }

        public KittyBindHashSet (int initSize)
        {
            this.data = new HashSet<T>(initSize);
        }
        
        private HashSet<T> data;
        public Action<T> onAddCallback;
        public Action<T> onRemoveCallback;
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in data)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        
        public bool Remove(params T[] keys)
        {
            var success = RemoveSilence(keys);
            onAddCallback?.Invoke(keys[0]);
            return success;
            
        }

        public bool RemoveSilence(params T[] keys)
        {
            var success = true;
            // var success = !data.Contains(key);
            for (int i = 0; i < keys.Length; i++)
            {
                var item = keys[i];
                var exist = data.Contains(item);
                success &= exist;
                if (exist)
                {
                    data.Remove(item);
                }
            }
            return success;
        }

        public void Clear()
        {
            ClearSilence();
            onRemoveCallback?.Invoke(default);
        }

        public void ClearSilence()
        {
            data.Clear();
        }

        public bool Add(params T[] keys)
        {
            var success = AddSilence(keys);
            onAddCallback?.Invoke(keys[0]);
            return success;
        }

        public bool AddSilence(params T[] keys)
        {
            var success = true;
            // var success = !data.Contains(key);
            for (int i = 0; i < keys.Length; i++)
            {
                var item = keys[i];
                var isNew = !data.Contains(item);
                success &= isNew;
                if (isNew)
                {
                    data.Add(item);
                }
            }
            return success;
        }

        public bool Contains(T key)
        {
            return data.Contains(key);
        }
        
        public void ForceInvokeAddWithKey(T key)
        {
            onAddCallback?.Invoke(key);
        }
        public void ForceInvokeRemoveWithKey(T key)
        {
            onRemoveCallback?.Invoke(key);
        }
        public int Count => data==null? 0 : data.Count;
        
        public T GetRandom()
        {
            T result=default;
            if (data==null || data.Count==0)
            {
                Debug.LogWarning($"Data not init or is empty");
            }
            else
            {
                foreach (var item in data)
                {
                    result = item;
                    break;
                }
            }
            return result;
        }
    }
}