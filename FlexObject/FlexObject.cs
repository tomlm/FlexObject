// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Flex
{
    /// <summary>
    /// DynamicObject which seamlessly blends dynamic properties with real typed properties
    /// </summary>
    /// <remarks>
    /// NOTE: Dyanmic properties will call PropertyChanged when changing, if you defined your own typed properties, you will need to call NotifyChanged() manually to get change notifications.
    /// NOTE 2: Override TrySetValue/TryGetValue/TryRemoveValue to customize.  All other methods call these 3 methods.
    /// </remarks>
    public class FlexObject : DynamicObject, INotifyPropertyChanged
    {
        private Dictionary<string, object> dynamicProperties = new Dictionary<string, object>();

        private Dictionary<string, PropertyInfo> objectProperties = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public FlexObject()
        {
            objectProperties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name != "Item")
                .ToDictionary(p => p.Name, p => p);
        }

        public IEnumerable<string> GetProperties() => GetDynamicMemberNames();

        #region DYNAMIC
        /// <summary>
        /// Dynamic property enumeration support for real/dynamic properties.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            foreach (var key in objectProperties.Keys)
                yield return key;

            foreach (var key in dynamicProperties.Keys)
                yield return key;

            yield break;
        }

        /// <summary>
        /// DynamicObject support for access value for real/dynamic properties.
        /// </summary>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetValue(binder.Name, out result);
        }

        /// <summary>
        /// DynamicObject support for setting value for real/dynamic properties.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return TrySetValue(binder.Name, value);
        }
        #endregion

        /// <summary>
        /// Helper to calls PropertyChanged for property name of caller
        /// </summary>
        /// <param name="property"></param>
        public void NotifyChanged([CallerMemberName] string property = null)
        {
            if (PropertyChanged != null)
            {
                if (property == null)
                    throw new ArgumentNullException(nameof(property));

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }

        /// <summary>
        /// Array access for both real and dynamic properties.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public dynamic this[string name]
        {
            get
            {
                this.TryGetValue(name, out object value);
                return value;
            }
            set
            {
                this.TrySetValue(name, value);
            }
        }

        /// <summary>
        /// Returns true if property exists (real or dynamic)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return this.objectProperties.ContainsKey(key) || this.dynamicProperties.ContainsKey(key);
        }

        /// <summary>
        /// Remove a dynamic property (for real properties, sets to null)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            return TryRemoveValue(key);
        }

        /// <summary>
        /// Remove each dynamic proeprty and set real properties to null.
        /// </summary>
        public void Clear()
        {
            foreach (var key in this.GetProperties())
            {
                Remove(key);
            }
        }

        public object GetValue(string propertyName)
        {
            TryGetValue(propertyName, out var result);
            return result;
        }

        public ValueT GetValue<ValueT>(string propertyName)
        {
            TryGetValue(propertyName, out var result);
            return (ValueT)result;
        }

        public bool TryGetValue<ValueT>(string propertyName, out ValueT result)
        {
            result = default(ValueT);
            if (TryGetValue(propertyName, out object obj))
            {
                result = (ValueT)obj;
                return true;
            }
            return false;
        }

        public void SetValue(string propertyName, object value)
        {
            TrySetValue(propertyName, value);
        }

        /// <summary>
        /// TrySetValue() for both real and dynamic properties.
        /// </summary>
        /// <remarks>override this to property setting (all other methods call this)</remarks>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool TrySetValue(string name, object value)
        {
            if (this.objectProperties.TryGetValue(name, out var prop))
            {
                prop.SetValue(this, value);
            }
            else
            {
                dynamicProperties[name] = value;
                NotifyChanged(name);
            }

            return true;
        }

        /// <summary>
        /// TryGetvalue() for both real and dynamic properties.
        /// </summary>
        /// <remarks>Override this to customize (all other methods call this)</remarks>
        /// <param name="name"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual bool TryGetValue(string name, out object result)
        {
            if (this.dynamicProperties.ContainsKey(name))
            {
                result = this.dynamicProperties[name];
                return true;
            }
            else if (this.objectProperties.TryGetValue(name, out var propertyInfo))
            {
                result = propertyInfo.GetValue(this);
                return true;
            }
            result = null;
            // we return true/null for unknown properties
            return true;
        }

        /// <summary>
        /// TryRemoveValue() for both real and dynamic properties.
        /// </summary>
        /// <remarks>Override this to change value behavior (All other methods call this)</remarks>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual bool TryRemoveValue(string name)
        {
            if (this.objectProperties.ContainsKey(name))
            {
                this.objectProperties[name].SetValue(this, null);
                return true;
            }
            else if (this.dynamicProperties.ContainsKey(name))
            {
                this.dynamicProperties.Remove(name);
                NotifyChanged(name);
            }
            return true;
        }
    }
}
