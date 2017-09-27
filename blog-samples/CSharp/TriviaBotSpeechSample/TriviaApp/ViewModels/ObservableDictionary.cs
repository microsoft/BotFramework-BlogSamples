// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Windows.Foundation.Collections;

namespace TrivaApp
{
    /// <summary>
    /// Implementation of IObservableMap that supports reentrancy for use as a default view
    /// model.
    /// </summary>
    public class ObservableDictionary : IObservableMap<string, object>
    {
        /// <summary>
        /// The underlying dictionary to build on
        /// </summary>
        private Dictionary<string, object> _dictionary = new Dictionary<string, object>();

        /// <summary>
        /// Triggers when the map is changed
        /// </summary>
        public event MapChangedEventHandler<string, object> MapChanged;

        /// <summary>
        /// Gets the keys that exist in this map
        /// </summary>
        public ICollection<string> Keys
        {
            get
            {
                return _dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets the values in this dictionary
        /// </summary>
        public ICollection<object> Values
        {
            get
            {
                return _dictionary.Values;
            }
        }

        /// <summary>
        /// Gets how many elements are in the dictionary
        /// </summary>
        public int Count
        {
            get
            {
                return _dictionary.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this dictionary is readonly or not
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets and sets elements in this dictionary using the [] indexer
        /// </summary>
        /// <param name="key">The key value to access</param>
        /// <returns>The object that is identified by key</returns>
        public object this[string key]
        {
            get
            {
                return _dictionary[key];
            }

            set
            {
                _dictionary[key] = value;
                InvokeMapChanged(CollectionChange.ItemChanged, key);
            }
        }

        /// <summary>
        /// Does this dictionary contain item
        /// </summary>
        /// <param name="item">The element to look up</param>
        /// <returns>true if the dictionary contains item, otherwise false</returns>
        public bool Contains(KeyValuePair<string, object> item)
        {
            return _dictionary.Contains(item);
        }

        /// <summary>
        /// Does this dictionary contain key
        /// </summary>
        /// <param name="key">The key to lookup in the dictionary</param>
        /// <returns>true if the dictionary contains key, otherwise false</returns>
        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Looks up key in the dictionary and sets value to the result
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <param name="value">The reference to store the result in</param>
        /// <returns>true if successful, otherwise false</returns>
        public bool TryGetValue(string key, out object value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Adds the key/value pair to the dictionary
        /// </summary>
        /// <param name="key">The key to use</param>
        /// <param name="value">The value to store</param>
        public void Add(string key, object value)
        {
            _dictionary.Add(key, value);
            InvokeMapChanged(CollectionChange.ItemInserted, key);
        }

        /// <summary>
        /// Adds the key/value pair to the dictionary
        /// </summary>
        /// <param name="item">The key/value pair to store</param>
        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes the key and its associated value from the dictionary
        /// </summary>
        /// <param name="key">The key to remove</param>
        /// <returns>true if key existed, otherwise false</returns>
        public bool Remove(string key)
        {
            if (_dictionary.Remove(key))
            {
                InvokeMapChanged(CollectionChange.ItemRemoved, key);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the key/value pair from the dictionary
        /// </summary>
        /// <param name="item">The key/value pair to remove</param>
        /// <returns>true if the key/value pair existed in the dictionary</returns>
        public bool Remove(KeyValuePair<string, object> item)
        {
            object currentValue;
            if (_dictionary.TryGetValue(item.Key, out currentValue) &&
                object.Equals(item.Value, currentValue) && _dictionary.Remove(item.Key))
            {
                InvokeMapChanged(CollectionChange.ItemRemoved, item.Key);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes all key/value pairs from this dictionary
        /// </summary>
        public void Clear()
        {
            var priorKeys = _dictionary.Keys.ToArray();
            _dictionary.Clear();

            foreach (var key in priorKeys)
            {
                InvokeMapChanged(CollectionChange.ItemRemoved, key);
            }
        }

        /// <summary>
        /// Gets an enumerator for this dictionary
        /// </summary>
        /// <returns>The enumerator</returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        /// <summary>
        /// Gets an enumerator for this dictionary
        /// </summary>
        /// <returns>The enumerator</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        /// <summary>
        /// Copy all elements from this dictionary into array at the specified index
        /// </summary>
        /// <param name="array">The array to copy elements into</param>
        /// <param name="arrayIndex">The starting index to copy elements at</param>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            int arraySize = array.Length;
            foreach (var pair in _dictionary)
            {
                if (arrayIndex >= arraySize)
                {
                    break;
                }

                array[arrayIndex++] = pair;
            }
        }

        /// <summary>
        /// Triggers when a key is changed in the map
        /// </summary>
        /// <param name="change">How the key was changed</param>
        /// <param name="key">What key was changed</param>
        private void InvokeMapChanged(CollectionChange change, string key)
        {
            MapChanged?.Invoke(this, new ObservableDictionaryChangedEventArgs(change, key));
        }

        /// <summary>
        /// The event args containing the key that was changed, and how it was changed, when a change takes place
        /// </summary>
        private class ObservableDictionaryChangedEventArgs : IMapChangedEventArgs<string>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ObservableDictionaryChangedEventArgs" /> class.
            /// </summary>
            /// <param name="change">How the key was changed</param>
            /// <param name="key">What key was changed</param>
            public ObservableDictionaryChangedEventArgs(CollectionChange change, string key)
            {
                CollectionChange = change;
                Key = key;
            }

            /// <summary>
            /// Gets how the key was changed
            /// </summary>
            public CollectionChange CollectionChange { get; private set; }

            /// <summary>
            /// Gets the key that was changed
            /// </summary>
            public string Key
            {
                get;
                private set;
            }
        }
    }
}
