using System;

namespace Bolsover
{
    public class ParameterChangeEventArgs : EventArgs
    {
        public readonly string Property;
        public readonly object Value;

        public ParameterChangeEventArgs(string property, object value)
        {
            Property = property;
            Value = value;
        }
    }
}