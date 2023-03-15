using System;

namespace Zephyr.Interpreting
{
    public class RuntimeValue : IComparable<RuntimeValue>
    {
        public static RuntimeValue None => _none;

        private static readonly RuntimeValue _none = new(null);

        public object Value
        {
            get
            {
                if (IsNone)
                    throw new ArgumentException("Value was null: thrown by RuntimeValue");

                return _value;
            }
        }

        private readonly object _value;
        public bool IsNone => _value is null;

        public RuntimeValue(object value)
        {
            if (value is RuntimeValue v)
                _value = v._value;
            else
                _value = value;
        }

        public int CompareTo(RuntimeValue other)
        {
            return GetDouble().CompareTo(other.GetDouble());
        }

        private double GetDouble()
        {
            return Value switch
            {
                int value => value,
                double value => value,
                _ => throw new ArgumentException($"Cannot cast {Value.GetType().Name} to double")
            };
        }

        public override string ToString()
        {
            return IsNone ? "None" : Value.ToString();
        }

        public static RuntimeValue operator +(RuntimeValue val1, RuntimeValue val2)
        {
            return (val1.Value, val2.Value) switch
            {
                (int v1, int v2) => v1 + v2,
                (double v1, int v2) => v1 + v2,
                (int v1, double v2) => v1 + v2,
                (double v1, double v2) => v1 + v2,
                (string v1, string v2) => v1 + v2,
                (string v1, int v2) => v1 + v2,
                (string v1, double v2) => v1 + v2,
                (int v1, string v2) => v1 + v2,
                (double v1, string v2) => v1 + v2
            };
        }
        
        public static RuntimeValue operator -(RuntimeValue val1, RuntimeValue val2)
        {
            return (val1.Value, val2.Value) switch
            {
                (int v1, int v2) => v1 - v2,
                (double v1, int v2) => v1 - v2,
                (int v1, double v2) => v1 - v2,
                (double v1, double v2) => v1 - v2
            };
        }
        
        public static RuntimeValue operator *(RuntimeValue val1, RuntimeValue val2)
        {
            return (val1.Value, val2.Value) switch
            {
                (int v1, int v2) => v1 * v2,
                (double v1, int v2) => v1 * v2,
                (int v1, double v2) => v1 * v2,
                (double v1, double v2) => v1 * v2
            };
        }
        
        public static RuntimeValue operator /(RuntimeValue val1, RuntimeValue val2)
        {
            return (val1.Value, val2.Value) switch
            {
                (int v1, int v2) => v1 / v2,
                (double v1, int v2) => v1 / v2,
                (int v1, double v2) => v1 / v2,
                (double v1, double v2) => v1 / v2
            };
        }

        public static RuntimeValue operator -(RuntimeValue val1)
        {
            return val1.Value switch
            {
                int v1 => -v1,
                double v1 => -v1
            };
        }

        public static implicit operator RuntimeValue(bool val)
        {
            return new((object)val);
        }
        
        public static implicit operator RuntimeValue(int val)
        {
            return new(val);
        }
        
        public static implicit operator RuntimeValue(double val)
        {
            return new(val);
        }

        public static implicit operator RuntimeValue(string val)
        {
            return new(val);
        }
    }
}