using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Workflow
{
    public class Expression
    {
        public List<Delegate> Method { get; }
        public List<Variable> Variables { get; }

        public Expression(List<Delegate> method, List<Variable> variables)
        {
            Method = method;
            Variables = variables;
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder("Methods: ");

            for (int i = 0; i < Method.Count; i++)
                b.Append($"{Method[i].Method.Name} | ");

            b = b.Remove(b.Length - 3, 3);

            if (Variables != null && Variables.Count > 0)
            {
                b.AppendLine();
                b.Append("Variables: ");

                for (int i = 0; i < Variables.Count; i++)
                    b.Append($"{Variables} | ");

                b = b.Remove(b.Length - 3, 3);
            }

            return b.ToString();
        }
    }

}
