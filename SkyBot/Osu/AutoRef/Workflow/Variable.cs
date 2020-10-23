using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Workflow
{
    public class Variable
    {
        public int Id { get; set; }
        public object Value
        {
            get => _value;
            set
            {
                if (value is bool)
                    VariableType = VariableType.Bool;
                else if (value is short ||
                         value is int ||
                         value is ushort ||
                         value is uint ||
                         value is ulong)
                {
                    _value = (long)value;
                    VariableType = VariableType.Number;
                    return;
                }
                else if (value is long)
                    VariableType = VariableType.Number;
                else if (value is char)
                {
                    _value = $"{value}";
                    VariableType = VariableType.Text;
                }
                else if (value is string)
                    VariableType = VariableType.Text;
                else
                {
                    _value = value.ToString();
                    VariableType = VariableType.Text;
                }
            }
        }
        public VariableType VariableType { get; private set; }

        object _value;

        public Variable(int id)
        {
            Id = id;
        }

        public Variable(int id, Expression expression) : this(id)
        {
            _value = expression;
            VariableType = VariableType.Expression;
        }

        public Variable(int id, object value) : this(id)
        {
            Value = value;
        }

        public static Variable TryParse(string s, int id)
        {
            if (bool.TryParse(s, out bool b))
                return new Variable(id, b);
            else if (long.TryParse(s, out long l))
                return new Variable(id, l);
            else
                return new Variable(id, s);
        }

        public override string ToString()
        {
            return $"ID: {Id}, Value: {Value}, Type: {VariableType}";
        }
    }
}
