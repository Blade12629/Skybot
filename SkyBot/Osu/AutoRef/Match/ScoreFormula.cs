using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SkyBot.Osu.AutoRef.Match
{
    public static class ScoreCalculator
    {

#pragma warning disable CA1305 // Specify IFormatProvider
        public static double CalculateRPN(Queue<string> rpn)
        {
            List<string> values = rpn.ToList();

            while(values.Count > 1)
            {
                string val = values[0];
                values.RemoveAt(0);

                if (char.IsNumber(val[0]))
                {
                    values.Add(val);
                    continue;
                }

                Operator op = FromChar(val[0]);

                if (val.Length == 1)
                    op = FromChar(val[0]);
                else
                    op = FuncFromChar(val);
                if (op != Operator.None ||
                        op != Operator.BrC ||
                        op != Operator.BrO)
                {
                    double right = double.Parse(Pop(values));

                    if ((int)op >= 10000 &&
                        (int)op < 100000)
                    {
                        values.Add(CalculateOperator(right, 0, op).ToString());
                    }
                    else
                    {
                        double left = double.Parse(Pop(values));
                        values.Add(CalculateOperator(left, right, op).ToString());
                    }
                }
            }

            return double.Parse(Pop(values));
        }

        static T Pop<T>(List<T> list)
        {
            T r = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);

            return r;
        }

        public static Queue<string> ParseRPN(string formula)
        {
            List<Operator> operators = new List<Operator>();
            Queue<string> outputQueue = new Queue<string>();

            for (int i = 0; i < formula.Length; i++)
            {
                char c = formula[i];

                if (char.IsWhiteSpace(c) || c == ',')
                    continue;
                else if (char.IsDigit(c))
                {
                    int start = i;

                    while(i < formula.Length && char.IsDigit(formula[i]))
                        i++;

                    outputQueue.Enqueue(formula.Substring(start, i - start));
                    i--;
                    continue;
                }
                if (char.IsWhiteSpace(c))
                    continue;


                Operator op = FromChar(c);

                //sin(50 + 10)
                if (op == Operator.None && char.IsLetter(c))
                {
                    int index = formula.IndexOf('(', i);

                    if (index == -1)
                        continue;

                    string fname = formula.Substring(i, index - i);
                    formula = formula.Remove(i, index - i);

                    op = Enum.Parse<Operator>(fname, true);
                }

                switch (op)
                {
                    case Operator.Sin:
                    case Operator.Cos:
                    case Operator.BrO:
                        {
                            operators.Add(op);
                        }
                        continue;
                }

                if (op != Operator.None &&
                    op != Operator.BrO &&
                    op != Operator.BrC)
                {
                    while (operators.Count > 0 &&
                          (GetPrecedence(operators[operators.Count - 1]) > GetPrecedence(op) ||
                          (GetPrecedence(operators[operators.Count - 1]) == GetPrecedence(op) && operators[operators.Count - 1] != Operator.Exponent)) &&
                          operators[operators.Count - 1] != Operator.BrO)
                    {
                        outputQueue.Enqueue(ToString(Pop(operators)));
                    }

                    operators.Add(op);
                }
                else if (op == Operator.BrC)
                {
                    while (operators.Count > 0 && operators[operators.Count - 1] != Operator.BrO)
                        outputQueue.Enqueue(ToString(Pop(operators)));

                    if (operators.Count > 0 && operators[operators.Count - 1] == Operator.BrO)
                        _ = Pop(operators);
                }
            }

            while (operators.Count > 0)
                outputQueue.Enqueue(ToString(Pop(operators)));

            return outputQueue;
        }

        public static double CalculateScore(long score, string formula, string scoreReplacePattern = "!s")
        {
            Queue<string> rpn = ParseRPN(formula.Replace(scoreReplacePattern, score.ToString(), StringComparison.CurrentCultureIgnoreCase));
            return CalculateRPN(rpn);
        }
#pragma warning restore CA1305 // Specify IFormatProvider

        static double CalculateOperator(double input, double input2, Operator op)
        {
            switch (op)
            {
                default:
                    return input;

                case Operator.Exponent:
                    return Math.Pow(input, input2);

                case Operator.Multiply:
                    return input * input2;

                case Operator.Divide:
                    return input / input2;

                case Operator.Plus:
                    return input + input2;

                case Operator.Minus:
                    return input - input2;

                case Operator.Modulo:
                    return input % input2;

                case Operator.Sin:
                    return Math.Sin(input);

                case Operator.Cos:
                    return Math.Cos(input);

                case Operator.Abs:
                    return Math.Abs(input);

                case Operator.Cbrt:
                    return Math.Cbrt(input);

                case Operator.Ceiling:
                    return Math.Ceiling(input);

                case Operator.Exp:
                    return Math.Exp(input);

                case Operator.Floor:
                    return Math.Floor(input);

                case Operator.IEEERemainder:
                    return Math.IEEERemainder(input, input2);

                case Operator.ILogB:
                    return Math.ILogB(input);

                case Operator.Log:
                    return Math.Log(input);

                case Operator.Sign:
                    return Math.Sign(input);

                case Operator.Sqrt:
                    return Math.Sqrt(input);

                case Operator.Tan:
                    return Math.Tan(input);

                case Operator.Truncate:
                    return Math.Truncate(input);

                case Operator.Atan2:
                    return Math.Atan2(input, input2);

                case Operator.LogDouble:
                    return Math.Log(input, input2);
                
                case Operator.Max:
                    return Math.Max(input, input2);
                
                case Operator.MaxMagnitude:
                    return Math.MaxMagnitude(input, input2);
                
                case Operator.Round:
                    return Math.Round(input, (int)input2, MidpointRounding.AwayFromZero);
                
                case Operator.ScaleB:
                    return Math.ScaleB(input, (int)input2);
            }
        }

        static Operator FuncFromChar(string func)
        {
            if (Enum.TryParse(func, true, out Operator op))
                return op;

            return Operator.None;
        }

        static Operator FromChar(char c)
        {
            Operator op;
            switch (c)
            {
                default:
                    op = Operator.None;
                    break;

                case '+':
                    op = Operator.Plus;
                    break;

                case '-':
                    op = Operator.Minus;
                    break;

                case '*':
                    op = Operator.Multiply;
                    break;

                case '/':
                    op = Operator.Divide;
                    break;

                case '^':
                    op = Operator.Exponent;
                    break;

                case '(':
                    op = Operator.BrO;
                    break;

                case ')':
                    op = Operator.BrC;
                    break;

                case '%':
                    op = Operator.Modulo;
                    break;
            }

            return op;
        }

        static string ToString(Operator op)
        {
            switch (op)
            {
                case Operator.None:
                    return null;

                default:
                    return op.ToString().ToLower(CultureInfo.CurrentCulture);

                case Operator.Exponent:
                    return "^";

                case Operator.Multiply:
                    return "*";

                case Operator.Divide:
                    return "/";

                case Operator.Plus:
                    return "+";

                case Operator.Minus:
                    return "-";

                case Operator.Modulo:
                    return "%";
            }
        }

        static int GetPrecedence(Operator op)
        {
            switch(op)
            {
                default:
                    return 0;

                case Operator.Exponent:
                    return 4;

                case Operator.Multiply:
                case Operator.Divide:
                case Operator.Modulo:
                    return 3;

                case Operator.Plus:
                case Operator.Minus:
                    return 2;
            }
        }
    }
}
