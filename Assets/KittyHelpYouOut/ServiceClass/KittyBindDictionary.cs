using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KittyHelpYouOut
{
    /// <summary>
    /// 猫猫可绑定大字典！可以绑定增删改事件,大概能适应所有需要绑定数据的场合。不光可绑定，甚至可以序列化！
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public class KittyBindDictionary<TKey,TValue> : IEnumerable<KeyValuePair<TKey, TValue>>,ISerializationCallbackReceiver
    {
        private KittyBindDictionary()
        {
        }
        public KittyBindDictionary(int initSize)
        {
            data = new(initSize);
            keyIndices = new(initSize);
            unusedIndices = new(initSize);
        }
        [Serializable]
        private struct SerializableKeyValuePair
        {
            public TKey key;
            public TValue value;

            public SerializableKeyValuePair(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }
        }
        private Dictionary<TKey, int> keyIndices;
        [SerializeField]
        private List<SerializableKeyValuePair> data;
        private Queue<int> unusedIndices;
        public Action<TKey, TValue> onAddCallback;
        public Action<TKey, TValue> onRemoveCallback;
        public Action<TKey, TValue> onChangeCallback;
        public Action onClearCallback;

        public ICollection<TKey> Keys => data.Select(tuple => tuple.key).ToArray();
        public ICollection<TValue> Values => data.Select(tuple => tuple.value).ToArray();
        
        private TValue GetValue(TKey key) => data[keyIndices[key]].value;
        
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!ContainsKey(key))
            {
                value = default;
                Debug.LogWarning($"key not found: {key}");
                return false;
            }
            value = GetValue(key);
            return true;
        }

        public TValue this[TKey key]
        {
            get
            {
                if (ContainsKey(key))
                {
                    return GetValue(key);
                }
                Debug.LogWarning($"key not found: {key}");
                return default;
            }
            set
            {
                SetSilence(key, value);
                onChangeCallback?.Invoke(key,value);
            }
        }
        
        public void SetSilence(TKey key, TValue value)
        {
            if (keyIndices.TryGetValue(key,out var index))
            {
#if UNITY_EDITOR
            Debug.Log($"Silence set, key={key}, value={value}");
#endif
                var kvp = data[index];
                kvp.value = value;
                data[index] = kvp;
            }
            else
            {
                Debug.LogWarning($"key not found: {key}");
            }
        }
        
        public bool ContainsKey(TKey key)
        {
            return keyIndices.ContainsKey(key);
        }

        // we should make a queue that hold the unused indices of data list
        // every time adding a new item, first check if there is available unused indices in that queue
        // if hashset is empty, than we add new in the data list
        // when remove item, add a new unused index to that queue

        private void _Add(TKey k, TValue v)
        {
            //set to unused index if there is one. add when unusedIndices is empty
            if (unusedIndices.Count>0)
            {
                var availableIndex = unusedIndices.Dequeue();
                keyIndices.Add(k,availableIndex);
                data[availableIndex] = new SerializableKeyValuePair(k, v);
                Debug.Log($"Reusing index:{availableIndex}");
            }
            else
            {
                keyIndices.Add(k,data.Count);
                data.Add(new SerializableKeyValuePair(k,v));
            }
        }
        
        public void Add(TKey key, TValue value)
        {
            if (!ContainsKey(key))
            {
                _Add(key,value);
                onAddCallback?.Invoke(key,value);
            }
            else
            {
                Debug.LogWarning($"key already exist: {key}");
            }
        }
        
        public void AddSilence(TKey key,TValue value)
        {
            
            if (!ContainsKey(key))
            {
#if UNITY_EDITOR
                Debug.Log($"Silence add, key={key}, value={value}");
#endif
                _Add(key,value);
            }
            else
            {
                Debug.LogWarning($"key already exist: {key}");
            }
        }

        private void _Remove(TKey k)
        {
            //queue the relative index , then remove that item
            unusedIndices.Enqueue(keyIndices[k]);
            Debug.Log($"Recycling index:{keyIndices[k]}");
            keyIndices.Remove(k);
        }
        
        public void Remove(TKey key)
        {
            if (ContainsKey(key))
            {
                onRemoveCallback?.Invoke(key,GetValue(key));
                _Remove(key);
            }
            else
            {
                Debug.LogWarning($"key not found: {key}");
            }
        }
        
        public void RemoveSilence(TKey key)
        {
            if (ContainsKey(key))
            {
#if UNITY_EDITOR
                Debug.Log($"Silence remove, key={key}, value={GetValue(key)}");
#endif
                _Remove(key);
            }
            else
            {
                Debug.LogWarning($"key not found: {key}");
            }
        }
        
        public void Clear()
        {
            data.Clear();
            keyIndices.Clear();
            unusedIndices.Clear();
            onClearCallback?.Invoke();
        }
        
        public void ClearSilence()
        {
            data.Clear();
            keyIndices.Clear();
            unusedIndices.Clear();
#if UNITY_EDITOR
            Debug.Log($"Silence clear");
#endif
        }
        

        public void ForceInvokeAddWithKey(TKey key)
        {
            onAddCallback?.Invoke(key,GetValue(key));
        }
        
        public void ForceInvokeRemoveWithKey(TKey key)
        {
            onRemoveCallback?.Invoke(key,GetValue(key));
        }
        public void ForceInvokeChangeWithKey(TKey key)
        {
            onChangeCallback?.Invoke(key,GetValue(key));
        }

        public int Count => data?.Count ?? 0;

        public (TKey, TValue) GetRandom()
        {
            (TKey,TValue) result=default;
            if (data==null || data.Count==0)
            {
                Debug.LogWarning($"Data not init or is empty");
            }
            else
            {
                result = (data[0].key, data[0].value);
            }
            return result;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return data.Select(ToKeyValuePair).GetEnumerator();
        }
        static KeyValuePair<TKey, TValue> ToKeyValuePair(SerializableKeyValuePair skvp)
        {
            return new KeyValuePair<TKey, TValue>(skvp.key, skvp.value);
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
        }
    }
}
