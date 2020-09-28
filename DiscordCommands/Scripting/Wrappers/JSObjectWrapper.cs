using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordCommands.Scripting.Wrappers
{
    /// <summary>
    /// Abstract js object wrapper, cannot be used for scripting
    /// </summary>
    public abstract class JSObjectWrapper<T>
    {
        protected T _value { get; set; }

        /// <summary>
        /// Abstract js object wrapper, cannot be used for scripting
        /// </summary>
        public JSObjectWrapper(T value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
