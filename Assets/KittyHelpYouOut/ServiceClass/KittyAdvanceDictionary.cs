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
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public class KittyAdvanceDictionary<TKey,TValue> : IDictionary<TKey,TValue>,ISerializationCallbackReceiver
    {
        private KittyAdvanceDictionary()
        {
        }
        public KittyAdvanceDictionary(int initSize)
        {
            data = new(initSize);
            keyIndices = new(initSize);
        }
        [Serializable]
        private struct SerializableKeyValuePair<TK,TK1>
        {
            public TK key;
            public TK1 value;

            public SerializableKeyValuePair(TK key, TK1 value)
            {
                this.key = key;
                this.value = value;
            }
        }
        private Dictionary<TKey, int> keyIndices;
        [SerializeField]
        private List<SerializableKeyValuePair<TKey,TValue>> data;
        public Action<TKey, TValue> onAddCallback;
        public Action<TKey, TValue> onRemoveCallback;
        public Action<TKey, TValue> onChangeCallback;

        #region obsolete
        // public void Add(T key, T1 value)
        // {
        //     if (data.ContainsKey(key))
        //     {
        //         Debug.LogWarning($"Key {key} has already been added");
        //         return;
        //     }
        //     data.Add(key,value);
        //     onAddCallback?.Invoke(key,value);
        // }
        //
        // public void Remove(T key)
        // {
        //     if (data.ContainsKey(key))
        //     {
        //         Debug.LogWarning($"Key {key} has already been added");
        //         return;
        //     }
        //     data.Add(key,value);
        //     onAddCallback?.Invoke(key,value);
        // }
        //
        // public void Clear()
        // {
        //     data.Clear();
        //     onRemoveCallback?.Invoke(default,default);
        // }
        //
        // public bool TryAdd(T key, T1 value)
        // {
        //     var success = data.TryAdd(key, value);
        //     if (success)
        //     {
        //         onAddCallback?.Invoke(key,value);
        //     }
        //     return success;
        // }
        //
        // public bool TryGetValue(T key, out T1 result)
        // {
        //     return data.TryGetValue(key, out result);
        // }
        #endregion

        private TValue GetValue(TKey key) => data[keyIndices[key]].value;

        private void SetValue(TKey key, TValue value)
        {
            if (keyIndices.TryGetValue(key,out var index))
            {
                var kvp = data[index];
                kvp.value = value;
                data[index] = kvp;
            }
            else
            {
                Debug.LogWarning($"key not found: {key}");
            }
        }
        
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!keyIndices.ContainsKey(key))
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
                if (keyIndices.ContainsKey(key))
                {
                    return GetValue(key);
                }
                Debug.LogWarning($"key not found: {key}");
                return default;
            }
            set
            {
                SetValue(key, value);
                onChangeCallback?.Invoke(key,value);
            }
        }
        public ICollection<TKey> Keys => data.Select(tuple => tuple.key).ToArray();
        public ICollection<TValue> Values => data.Select(tuple => tuple.value).ToArray();

        public void SetSilence(TKey key, TValue value)
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

        public void RemoveSilence(TKey key)
        {
            if (data.ContainsKey(key))
            {
                data.Remove(key);
            }else
            {
                Debug.LogWarning($"key not found : {key}");
            }
        }
        public void AddSilence(TKey key,TValue value)
        {
            if (!data.ContainsKey(key))
            {
                data.Add(key,value);
            }else
            {
                Debug.LogWarning($"key already exist : {key}");
            }
        }
        

        public void ForceInvokeAddWithKey(TKey key)
        {
            onAddCallback?.Invoke(key,data[key]);
        }
        
        public void ForceInvokeRemoveWithKey(TKey key)
        {
            onRemoveCallback?.Invoke(key,data[key]);
        }
        public void ForceInvokeChangeWithKey(TKey key)
        {
            onChangeCallback?.Invoke(key,data[key]);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (data.ContainsKey(item.Key))
            {
                Debug.LogWarning($"Key {item.Key} has already been added");
                return;
            }
            data.Add(item.Key,item.Value);
            keys.Add(item.Key);
            values.Add(item.Value);
            onAddCallback?.Invoke(item.Key,item.Value);
        }

        public void Clear()
        {
            data.Clear();
            keys.Clear();
            values.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return data.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (data.ContainsKey(item.Key))
            {
                onRemoveCallback?.Invoke(item.Key,item.Value);
                data.Remove(item.Key);
                return true;
            }
            return false;
        }

        public int Count => data?.Count ?? 0;
        public bool IsReadOnly { get; }

        public (TKey, TValue) GetRandom()
        {
            (TKey,TValue) result=default;
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

        public void Add(TKey key, TValue value)
        {
            if (data.ContainsKey(key))
            {
                Debug.LogWarning($"Key {key} has already been added");
                return;
            }
            data.Add(key,value);
            onAddCallback?.Invoke(key,value);
        }

        public bool ContainsKey(TKey key)
        {
            return false;
        }

        public bool Remove(TKey key)
        {
            if (data.ContainsKey(key))
            {
                onRemoveCallback?.Invoke(key,data[key]);
                data.Remove(key);
                return true;
            }
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
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

 
public class SerializableDictionary { }
 
[Serializable]
public class SerializableDictionary<TKey, TValue> :
    SerializableDictionary,
    ISerializationCallbackReceiver,
    IDictionary<TKey, TValue>
{
    [SerializeField] private List<SerializableKeyValuePair> list = new List<SerializableKeyValuePair>();
 
    [Serializable]
    private struct SerializableKeyValuePair
    {
        public TKey Key;
        public TValue Value;
 
        public SerializableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
 
    private Dictionary<TKey, int> KeyPositions => _keyPositions.Value;
    private Lazy<Dictionary<TKey, int>> _keyPositions;
    public SerializableDictionary()
    {
        _keyPositions = new Lazy<Dictionary<TKey, int>>(MakeKeyPositions);
    }
 
    private Dictionary<TKey, int> MakeKeyPositions()
    {
        var dictionary = new Dictionary<TKey, int>(list.Count);
        for (var i = 0; i < list.Count; i++)
        {
            dictionary[list[i].Key] = i;
        }
        return dictionary;
    }
 
    public void OnBeforeSerialize() { }
 
    public void OnAfterDeserialize()
    {
        _keyPositions = new Lazy<Dictionary<TKey, int>>(MakeKeyPositions);
    }
 
    #region IDictionary<TKey, TValue>
 
    public TValue this[TKey key]
    {
        get => list[KeyPositions[key]].Value;
        set
        {
            var pair = new SerializableKeyValuePair(key, value);
            if (KeyPositions.ContainsKey(key))
            {
                list[KeyPositions[key]] = pair;
            }
            else
            {
                KeyPositions[key] = list.Count;
                list.Add(pair);
            }
        }
    }
 
    public ICollection<TKey> Keys => list.Select(tuple => tuple.Key).ToArray();
    public ICollection<TValue> Values => list.Select(tuple => tuple.Value).ToArray();
 
    public void Add(TKey key, TValue value)
    {
        if (KeyPositions.ContainsKey(key))
            throw new ArgumentException("An element with the same key already exists in the dictionary.");
        else
        {
            KeyPositions[key] = list.Count;
            list.Add(new SerializableKeyValuePair(key, value));
        }
    }
 
    public bool ContainsKey(TKey key) => KeyPositions.ContainsKey(key);
 
    public bool Remove(TKey key)
    {
        if (KeyPositions.TryGetValue(key, out var index))
        {
            KeyPositions.Remove(key);
 
            list.RemoveAt(index);
            for (var i = index; i < list.Count; i++)
                KeyPositions[list[i].Key] = i;
 
            return true;
        }
        else
            return false;
    }
 
    public bool TryGetValue(TKey key, out TValue value)
    {
        if (KeyPositions.TryGetValue(key, out var index))
        {
            value = list[index].Value;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }
 
    #endregion
 
    #region ICollection <KeyValuePair<TKey, TValue>>
    public int Count => list.Count;
    public bool IsReadOnly => false;
 
    public void Add(KeyValuePair<TKey, TValue> kvp) => Add(kvp.Key, kvp.Value);
 
    public void Clear() => list.Clear();
    public bool Contains(KeyValuePair<TKey, TValue> kvp) => KeyPositions.ContainsKey(kvp.Key);
 
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        var numKeys = list.Count;
        if (array.Length - arrayIndex < numKeys)
            throw new ArgumentException("arrayIndex");
        for (var i = 0; i < numKeys; i++, arrayIndex++)
        {
            var entry = list[i];
            array[arrayIndex] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
        }
    }
 
    public bool Remove(KeyValuePair<TKey, TValue> kvp) => Remove(kvp.Key);
 
    #endregion
 
    #region IEnumerable <KeyValuePair<TKey, TValue>>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return list.Select(ToKeyValuePair).GetEnumerator();
 
 
    }
    static KeyValuePair<TKey, TValue> ToKeyValuePair(SerializableKeyValuePair skvp)
    {
        return new KeyValuePair<TKey, TValue>(skvp.Key, skvp.Value);
    }
 
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
 
    #endregion
}