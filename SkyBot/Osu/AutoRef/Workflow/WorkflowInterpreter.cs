using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyBot.Osu.AutoRef.Workflow
{
    public class WorkflowInterpreter
    {
        Dictionary<string, List<Delegate>> _methods;

        public void RegisterDefaultMethods(LobbyController lc, AutoRefController arc)
        {
            RegisterMethod("RequestRoll", new Action<string>(arc.RequestRoll));
            RegisterMethod("RequestPick", new Action<string>(arc.RequestPick));

            RegisterMethods("SendCommand", new Action<int>(i => lc.SendCommand((MPCommand)i)),
                                           new Action<int, object>((i, o) => lc.SendCommand((MPCommand)i, o)),
                                           new Action<int, object, object>((i, o, o2) => lc.SendCommand((MPCommand)i, o, o2)),
                                           new Action<int, object, object, object>((i, o, o2, o3) => lc.SendCommand((MPCommand)i, o, o2, o3)));

            RegisterMethods("Set", new Action<int>(i => lc.SetLobby((TeamMode)i, null, null)),
                                   new Action<int, int>((i, i2) => lc.SetLobby((TeamMode)i, (WinCondition)i2, null)),
                                   new Action<int, int, int>((i, i2, i3) => lc.SetLobby((TeamMode)i, (WinCondition)i2, i3)));
            RegisterMethods("Start", new Action(() => lc.SendCommand(MPCommand.Start)),
                                     new Action<int>(i => lc.SendCommand(MPCommand.Start, i)));
            RegisterMethods("Map", new Action<int>(i => lc.SetMap(i)),
                                   new Action<int, int>((i, i2) => lc.SetMap(i, i2)));
            RegisterMethods("Mods", new Action(() => lc.SetMods()),
                                    new Action<string>(s => lc.SetMods(s)),
                                    new Action<string, bool>((s, b) => lc.SetMods(s, b)),
                                    new Action<bool>(b => lc.SetMods(null, b)));
            RegisterMethods("Password", new Action(() => lc.SendCommand(MPCommand.Password, "")),
                                        new Action<string>(s => lc.SendCommand(MPCommand.Password, s)));

            RegisterMethod("EnqueueCloseLobby", new Action(lc.EnqueueCloseLobby));
            RegisterMethod("Invite", new Action<string>(lc.Invite));
            RegisterMethod("Lock", new Action(lc.LockMatch));
            RegisterMethod("Unlock", new Action(lc.UnlockMatch));
            RegisterMethod("Move", new Action<string, int>(lc.MovePlayer));
            RegisterMethod("Host", new Action<string>(lc.SetHost));
            RegisterMethod("ClearHost", new Action(() => lc.SetHost()));
            RegisterMethod("Settings", new Action(() => lc.SendCommand(MPCommand.Settings)));
            RegisterMethod("Abort", new Action(() => lc.SendCommand(MPCommand.Abort)));
            RegisterMethod("Team", new Action<string, int>((s, i) => lc.SendCommand(MPCommand.Team, s, ((SlotColor)i).ToString())));
            RegisterMethod("Timer", new Action<int>(i => lc.SendCommand(MPCommand.Timer, i)));
            RegisterMethod("AbortTimer", new Action(() => lc.SendCommand(MPCommand.Aborttimer)));
            RegisterMethod("Kick", new Action<string>(s => lc.SendCommand(MPCommand.Kick, s)));
            RegisterMethod("AddRef", new Action<string>(s => lc.SendCommand(MPCommand.AddRef, s)));
            RegisterMethod("RemoveRef", new Action<string>(s => lc.SendCommand(MPCommand.RemoveRef, s)));
            RegisterMethod("ListRefs", new Action(() => lc.SendCommand(MPCommand.ListRefs)));
            RegisterMethod("SortPlayers", new Action(arc.SortPlayers));

            RegisterMethods("SetSetting", new Action<string, bool>(arc.SetSetting),
                                          new Action<string, long>(arc.SetSetting),
                                          new Action<string, string>(arc.SetSetting));


        }

        void Invoke(Expression expr)
        {
            for (int i = 0; i < expr.Variables.Count; i++)
            {
                if (expr.Variables[i].VariableType == VariableType.Expression)
                {
                    Expression exp = (Expression)expr.Variables[i].Value;
                    Invoke(exp);

                    expr.Variables.RemoveAt(i);
                    i--;
                }
            }

            Delegate method = GetDelegateForSignature(expr.Variables, expr.Method);

            if (method == null)
                return;

            Invoke(method, expr.Variables);
        }

        void Invoke(Delegate del, List<Variable> variables)
        {
            del.DynamicInvoke(variables == null ? null : variables.Count == 0 ? null : variables.ToArray());
        }

        Delegate GetDelegateForSignature(List<Variable> variables, List<Delegate> delegates)
        {
            List<VariableType> types = variables.Select(v => v.VariableType).ToList();

            if (types.Any(t => t == VariableType.Expression))
                return null;

            foreach(Delegate del in delegates)
            {
                var parameters = del.Method.GetParameters();

                if (parameters.Length != variables.Count)
                    continue;

                bool foundSignature = false;
                for (int i = 0; i < types.Count; i++)
                {
                    switch(types[i])
                    {
                        case VariableType.Bool:
                            if (parameters[i].ParameterType != typeof(bool))
                                continue;
                            break;

                        case VariableType.Number:
                            if (parameters[i].ParameterType != typeof(long))
                                continue;
                            break;

                        case VariableType.Text:
                            if (parameters[i].ParameterType != typeof(string))
                                continue;
                            break;
                    }

                    if (i == types.Count - 1)
                        foundSignature = true;
                }

                if (foundSignature)
                    return del;
            }

            return null;
        }

        public void RegisterMethods(string method, params Delegate[] del)
        {
            if (!_methods.ContainsKey(method))
                _methods.Add(method, new List<Delegate>());

            _methods[method].AddRange(del);
        }

        public void RegisterMethod(string method, Delegate del)
        {
            RegisterMethods(method, del);
        }

        public List<Expression> Interpret(string script)
        {
            List<string> split = script.Trim(' ').Split(';').ToList();
            split.RemoveAll(s => string.IsNullOrEmpty(s));

            List<Expression> expressions = new List<Expression>();

            for (int i = 0; i < split.Count; i++)
                expressions.Add(InterpretExpression(split[i]));

            return expressions;
        }

        public string DumpScript(string script, bool indented)
        {
            List<Expression> expressions = Interpret(script);
            StringBuilder b = new StringBuilder();

            for (int i = 0; i < expressions.Count; i++)
            {
                b.AppendLine(expressions[i].ToString());

                if (indented && i != expressions.Count - 1)
                    b.AppendLine();
            }

            return b.ToString();
        }

        public Expression InterpretExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression))
                return null;

            expression = expression.TrimEnd(')', ';');

            //DoStuff()
            //a(5)
            //a("t")
            //a(true)
            //a(b(5))

            string methodName = null;
            List<Variable> variables = new List<Variable>();

            for (int i = 0; i < expression.Length; i++)
            {
                if (methodName == null)
                {
                    if (expression[i] == '(')
                    {
                        methodName = expression.Substring(0, i);
                        expression = expression.Remove(0, i + 1).Remove(expression.Length - 1);
                        i = 0;
                        continue;
                    }
                }
                else
                {
                    if (expression[i] == '"')
                    {
                        for (int x = i + 1; x < expression.Length; x++)
                        {
                            if (expression[x] == '"')
                            {
                                string sub = expression.Substring(i + 1, i - x - 1);
                                variables.Add(Variable.TryParse(sub, variables.Count + 1));
                                expression = expression.Remove(i, i - x);
                                break;
                            }
                        }

                        continue;
                    }
                    else if (char.IsNumber(expression[i]))
                    {
                        for (int x = i + 1; x < expression.Length; x++)
                        {
                            if (!char.IsNumber(expression[x]))
                            {
                                string sub = expression.Substring(i + 1, i - x - 1);
                                variables.Add(Variable.TryParse(sub, variables.Count + 1));
                                expression = expression.Remove(i, i - x);
                            }
                        }

                        continue;
                    }
                    else if (expression.StartsWith("true", StringComparison.CurrentCultureIgnoreCase))
                    {
                        variables.Add(new Variable(variables.Count + 1, true));
                        expression = expression.Remove(0, "true".Length);
                        continue;
                    }
                    else if (expression.StartsWith("false", StringComparison.CurrentCultureIgnoreCase))
                    {
                        variables.Add(new Variable(variables.Count + 1, true));
                        expression = expression.Remove(0, "false".Length);
                        continue;
                    }
                    else
                    {
                        int index = expression.IndexOf(')', StringComparison.CurrentCultureIgnoreCase);
                        string subExpr = expression.Substring(0, index);
                        expression = expression.Remove(0, index + 1);

                        variables.Add(new Variable(variables.Count, InterpretExpression(subExpr)));
                        continue;
                    }
                }
            }

            if (methodName == null)
                return null;

            _ = _methods.TryGetValue(methodName, out List<Delegate> del);
            return new Expression(del, variables);
        }
    }
}
