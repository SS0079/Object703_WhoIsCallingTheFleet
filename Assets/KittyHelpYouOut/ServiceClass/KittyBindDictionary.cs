using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KittyHelpYouOut.ServiceClass
{
    /// <summary>
    /// 猫猫可绑定大字典！大概可以适应所有需要绑定数据的场合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T1"></typeparam>
    [Serializable]
    public class KittyBindDictionary<T,T1> : IEnumerable<KeyValuePair<T,T1>>,ISerializationCallbackReceiver
    {
        private KittyBindDictionary()
        {
        }

        public KittyBindDictionary(int initSize)
        {
            this.data = new(initSize);
            keys = new(initSize);
            values = new(initSize);
        }

        [SerializeField]
        private List<T> keys;
        [SerializeField]
        private List<T1> values;
        private Dictionary<T, T1> data;
        public Action<T, T1> onAddCallback;
        public Action<T, T1> onRemoveCallback;
        public Action<T, T1> onChangeCallback;

        public void Add(T key, T1 value)
        {
            if (data.ContainsKey(key))
            {
                Debug.LogWarning($"Key {key} has already been added");
                return;
            }
            data.Add(key,value);
            onAddCallback?.Invoke(key,value);
        }

        public void Remove(T key)
        {
            if (!data.ContainsKey(key))
            {
                Debug.LogWarning($"Key {key} not found");
                return;
            }
            var value = data[key];
            data.Remove(key);
            onRemoveCallback?.Invoke(key,value);
        }

        public void Clear()
        {
            data.Clear();
            onRemoveCallback?.Invoke(default,default);
        }

        public bool TryAdd(T key, T1 value)
        {
            var success = data.TryAdd(key, value);
            if (success)
            {
                onAddCallback?.Invoke(key,value);
            }
            return success;
        }

        public bool TryGetValue(T key, out T1 result)
        {
            return data.TryGetValue(key, out result);
        }

        public T1 this[T key]
        {
            get
            {
                if (data.TryGetValue(key,out T1 result))
                {
                    return result;
                }
                else
                {
                    Debug.LogWarning($"Key {key} not found");
                    return default;
                }
            }
            set
            {
                if (data.ContainsKey(key))
                {
                    data[key] = value;
                    onChangeCallback?.Invoke(key,value);
                }
                else
                {
                    Debug.LogWarning($"Key {key} not found");
                }
            }
        }

        public void SetSilence(T key, T1 value)
        {
            if (data.ContainsKey(key))
            {
                data[key] = value;
            }
            else
            {
                Debug.LogWarning($"key not found : {key}");
            }
        }

        public void RemoveSilence(T key)
        {
            if (data.ContainsKey(key))
            {
                data.Remove(key);
            }else
            {
                Debug.LogWarning($"key not found : {key}");
            }
        }
        public void AddSilence(T key,T1 value)
        {
            if (!data.ContainsKey(key))
            {
                data.Add(key,value);
            }else
            {
                Debug.LogWarning($"key already exist : {key}");
            }
        }
        

        public void ForceInvokeAddWithKey(T key)
        {
            onAddCallback?.Invoke(key,data[key]);
        }
        
        public void ForceInvokeRemoveWithKey(T key)
        {
            onRemoveCallback?.Invoke(key,data[key]);
        }
        public void ForceInvokeChangeWithKey(T key)
        {
            onChangeCallback?.Invoke(key,data[key]);
        }

        public int Count => data==null? 0 : data.Count;

        public (T, T1) GetRandom()
        {
            (T,T1) result=default;
            if (data==null || data.Count==0)
            {
                Debug.LogWarning($"Data not init or is empty");
            }
            else
            {
                foreach (var item in data)
                {
                    result = (item.Key, item.Value);
                    break;
                }
            }
            return result;
        }

        public bool ContainsKey(T key)
        {
            return data.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<T, T1>> GetEnumerator()
        {
            // return new DataEnumerator(data);
            foreach (var item in data)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var kvp in data)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            for (int i = 0; i < keys.Count; i++)
            {
                data.Add(keys[i], values[i]);
            }
        }
    }
}