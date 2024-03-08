using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BetterRefillGemsPlus
{
    internal class CacheDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary<TKey, TValue>,  IDeserializationCallback, ISerializable where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _cache = [];
        public readonly Func<TKey, TValue> DefaultValue;
        public CacheDictionary(Func<TKey, TValue> defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!_cache.TryGetValue(key, out var value))
                {
                    value = _cache[key] = DefaultValue(key);
                }
                return value;
            }
            set
            {
                ((IDictionary<TKey, TValue>)_cache)[key] = value;
            }
        }

        public int Count => ((ICollection<KeyValuePair<TKey, TValue>>)_cache).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)_cache).IsReadOnly;

        public IEnumerable<TKey> Keys => ((IReadOnlyDictionary<TKey, TValue>)_cache).Keys;

        public IEnumerable<TValue> Values => ((IReadOnlyDictionary<TKey, TValue>)_cache).Values;

        public bool IsSynchronized => ((ICollection)_cache).IsSynchronized;

        public object SyncRoot => ((ICollection)_cache).SyncRoot;

        public bool IsFixedSize => ((IDictionary)_cache).IsFixedSize;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => ((IDictionary<TKey, TValue>)_cache).Keys;


        ICollection<TValue> IDictionary<TKey, TValue>.Values => ((IDictionary<TKey, TValue>)_cache).Values;


        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_cache).Add(item);
        }

        public void Add(TKey key, TValue value)
        {
            ((IDictionary<TKey, TValue>)_cache).Add(key, value);
        }

        public void Add(object key, object value)
        {
            ((IDictionary)_cache).Add(key, value);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_cache).Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_cache).Contains(item);
        }

        public bool Contains(object key)
        {
            return ((IDictionary)_cache).Contains(key);
        }

        public bool ContainsKey(TKey key)
        {
            return ((IReadOnlyDictionary<TKey, TValue>)_cache).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_cache).CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_cache).CopyTo(array, index);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>)_cache).GetEnumerator();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ((ISerializable)_cache).GetObjectData(info, context);
        }

        public void OnDeserialization(object sender)
        {
            ((IDeserializationCallback)_cache).OnDeserialization(sender);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_cache).Remove(item);
        }

        public bool Remove(TKey key)
        {
            return ((IDictionary<TKey, TValue>)_cache).Remove(key);
        }

        public void Remove(object key)
        {
            ((IDictionary)_cache).Remove(key);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return ((IReadOnlyDictionary<TKey, TValue>)_cache).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_cache).GetEnumerator();
        }

    }
}
