using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Extensions.DatatStructures
{
    public class TwoWayDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _forward = new();
        private readonly Dictionary<TValue, TKey> _reverse = new();

        public void Add(TKey key, TValue value)
        {
            if (_forward.ContainsKey(key))
                throw new ArgumentException("Duplicate key", nameof(key));
            if (_reverse.ContainsKey(value))
                throw new ArgumentException("Duplicate value", nameof(value));

            _forward.Add(key, value);
            _reverse.Add(value, key);
        }

        public bool GetByKey(TKey key, out TValue value) => _forward.TryGetValue(key, out value);

        public bool GetByValue(TValue value, out TKey key) =>_reverse.TryGetValue(value, out key);


        public bool RemoveByKey(TKey key)
        {
            if (_forward.TryGetValue(key, out var value))
            {
                _forward.Remove(key);
                _reverse.Remove(value);
                return true;
            }
            return false;
        }

        public void UpdateByKey(TKey key, TValue newValue)
        {
            if (!_forward.TryGetValue(key, out var oldValue)) 
            {
                Add(key, newValue);
                return;
            }

            // Prevent duplicate values
            if (_reverse.ContainsKey(newValue) && !_reverse[newValue]!.Equals(key)) return;

            _forward[key] = newValue;
            _reverse.Remove(oldValue);
            _reverse[newValue] = key;
        }

        public bool RemoveByValue(TValue value)
        {
            if (_reverse.TryGetValue(value, out var key))
            {
                _reverse.Remove(value);
                _forward.Remove(key);
                return true;
            }
            return false;
        }

        public bool ContainsKey(TKey key) => _forward.ContainsKey(key);

        public bool ContainsValue(TValue value) => _reverse.ContainsKey(value);

        public IEnumerable<TKey> Keys => _forward.Keys;

        public IEnumerable<TValue> Values => _reverse.Keys;

        public void Clear()
        {
            _forward.Clear();
            _reverse.Clear();
        }
    }
}
