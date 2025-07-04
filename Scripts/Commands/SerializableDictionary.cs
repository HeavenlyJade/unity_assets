using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiGame.Commands
{
    [Serializable]
    public abstract class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            // 当列表中存在默认值（对于引用类型即 null）的键时，
            // 通常意味着我们正在编辑器中进行修改（例如，刚刚添加了一个新条目，还未赋值）。
            // 在这种情况下，我们不应该用运行时字典的数据去覆盖序列化列表，
            // 否则用户刚添加的空行会立即消失。
            // 因此，我们跳过此次同步，以保留编辑器中的修改。
            if (keys.Contains(default(TKey)))
            {
                return;
            }

            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            this.Clear();

            if (keys.Count != values.Count)
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable.", keys.Count, values.Count));

            for (int i = 0; i < keys.Count; i++)
            {
                // Don't add entries with null keys or duplicate keys
                if (keys[i] != null && !this.ContainsKey(keys[i]))
                {
                    this.Add(keys[i], values[i]);
                }
            }
        }
    }
} 