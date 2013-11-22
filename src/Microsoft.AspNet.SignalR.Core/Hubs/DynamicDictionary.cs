// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public class DynamicDictionary : DynamicObject, IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _obj;

        public DynamicDictionary(IDictionary<string, object> obj)
        {
            _obj = obj;
        }

        public object this[string key]
        {
            get
            {
                object result;
                _obj.TryGetValue(key, out result);
                return Wrap(result);
            }
            set
            {
                _obj[key] = Unwrap(value);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "The compiler generates calls to invoke this")]
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];
            return true;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "The compiler generates calls to invoke this")]
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.Name] = value;
            return true;
        }

        public static object Wrap(object value)
        {
            var obj = value as IDictionary<string, object>;
            if (obj != null)
            {
                return new DynamicDictionary(obj);
            }

            return value;
        }

        public static object Unwrap(object value)
        {
            var dictWrapper = value as DynamicDictionary;
            if (dictWrapper != null)
            {
                return dictWrapper._obj;
            }

            return value;
        }

        public void Add(string key, object value)
        {
            _obj.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _obj.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return _obj.Keys; }
        }

        public bool Remove(string key)
        {
            return _obj.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _obj.TryGetValue(key, out value);
        }

        public ICollection<object> Values
        {
            get { return _obj.Values; }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _obj.Add(item);
        }

        public void Clear()
        {
            _obj.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _obj.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _obj.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _obj.Count; }
        }

        public bool IsReadOnly
        {
            get { return _obj.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _obj.Remove(item);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _obj.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
