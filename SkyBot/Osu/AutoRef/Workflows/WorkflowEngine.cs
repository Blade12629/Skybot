using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Jint;
using Jint.Runtime.Interop;
using SkyBot.Osu.AutoRef.Workflows.Wrappers;

namespace SkyBot.Osu.AutoRef.Workflows
{
    public class WorkflowEngine
    {
        public CancellationTokenSource CancellationTokenSource { get; private set; }

        Engine _engine;
        LobbyController _lc;
        AutoRefController _arc;

        public WorkflowEngine(LobbyController lc, AutoRefController arc)
        {
            _lc = lc;
            _arc = arc;
        }

        public List<Exception> GetErrors()
        {
            List<Exception> result = new List<Exception>();

            return result;
        }

        public WorkflowWrapper Interpret(string script, out Exception ex)
        {
            WorkflowWrapper workflow = new WorkflowWrapper();

            try
            {
                if (!SecurityCheck(script, out Exception ex__))
                    throw ex__;

                Setup(workflow);
                _engine.Execute(script);
            }
            catch (Exception ex_)
            {
                ex = ex_;
                return null;
            }

            ex = null;
            return workflow;
        }

        bool SecurityCheck(string script, out Exception ex)
        {
            const string WORKFLOW_START = "Workflow.AddStep(";
            if (string.IsNullOrEmpty(script))
            {
                ex = new InterpreterException("General", "Script is null or empty");
                return false;
            }

            script = script.Trim(' ', '\n');

            if (script.Contains("require(", StringComparison.CurrentCultureIgnoreCase))
            {
                ex = new InterpreterException("Security", "Use of require() detected");
                return false;
            }

            //Make sure user won't accidently invoke the function instead of adding it
            #region addstep check
            //Workflow.AddStep(() => Msg("Inviting players"));
            //Workflow.AddStep(InvitePlayers);

            for(int prevIndex = 0; ; )
            {
                int index = script.IndexOf(WORKFLOW_START, prevIndex, StringComparison.CurrentCultureIgnoreCase);

                if (index == -1)
                    break;

                prevIndex = index;
                int end = script.IndexOf(");", index, StringComparison.CurrentCultureIgnoreCase);

                if (end == -1)
                {
                    ex = new InterpreterException("SecurityCheck", "Unable to find end of Workflow.AddStep");
                    return false;
                }

                //()=>Msg("Invitingplayers")
                //InvitePlayers
                string sub = script.Substring(index + WORKFLOW_START.Length, end - index - WORKFLOW_START.Length).Trim(' ');

                if (string.IsNullOrEmpty(sub))
                {
                    ex = new InterpreterException("SecurityCheck", "Unable to find Workflow.AddStep parameters");
                    return false;
                }
                else if (sub.StartsWith("()=>", StringComparison.CurrentCultureIgnoreCase))
                {
                    index = script.IndexOf(WORKFLOW_START, end, StringComparison.CurrentCultureIgnoreCase);
                    continue;
                }
                else
                {
                    if (sub.EndsWith(')'))
                    {
                        ex = new InterpreterException("Invocation", "You cannot invoke functions through Workflow.AddStep(Function());");
                        return false;
                    }
                }
            }
            #endregion

            ex = null;
            return true;
        }

        void Setup(WorkflowWrapper workflow)
        {
            CancellationTokenSource = new CancellationTokenSource();

            _engine = new Engine(o =>
            {
                o.LimitMemory(200_000);
                o.AllowClrWrite(false);
                o.CancellationToken(CancellationTokenSource.Token);
                o.CatchClrExceptions();
                o.Culture(CultureInfo.CurrentCulture);
                o.DebugMode(false);
                o.AllowDebuggerStatement(false);
                o.LimitRecursion(50);
            });

            AddReferences();
            AddObjects(workflow);
        }

        void AddReferences()
        {
            _engine = _engine.SetValue("Uri", TypeReference.CreateTypeReference(_engine, typeof(Uri)))
                             .SetValue("Enum", TypeReference.CreateTypeReference(_engine, typeof(Enum)))
                             .SetValue("Math", TypeReference.CreateTypeReference(_engine, typeof(Math)))
                             .SetValue("MathF", TypeReference.CreateTypeReference(_engine, typeof(MathF)))
                             .SetValue("___RandomWrapper___", TypeReference.CreateTypeReference(_engine, typeof(Random)));

            _engine = _engine.SetValue("___ConvertWrapper___", TypeReference.CreateTypeReference(_engine, typeof(ConvertWrapper)))
                             .SetValue("___LobbyWrapper___", TypeReference.CreateTypeReference(_engine, typeof(LobbyWrapper)))
                             .SetValue("___RefWrapper___", TypeReference.CreateTypeReference(_engine, typeof(RefWrapper)))
                             .SetValue("___WorkflowWrapper___", TypeReference.CreateTypeReference(_engine, typeof(WorkflowWrapper)));

            _engine = _engine.SetValue("WinCondition", TypeReference.CreateTypeReference(_engine, typeof(WinCondition)))
                             .SetValue("SlotColor", TypeReference.CreateTypeReference(_engine, typeof(SlotColor)))
                             .SetValue("TeamMode", TypeReference.CreateTypeReference(_engine, typeof(TeamMode)))
                             .SetValue("Score", TypeReference.CreateTypeReference(_engine, typeof(ScoreWrapper)))
                             .SetValue("Roll", TypeReference.CreateTypeReference(_engine, typeof(RollWrapper)))
                             .SetValue("Slot", TypeReference.CreateTypeReference(_engine, typeof(SlotWrapper)));
        }

        void AddObjects(WorkflowWrapper workflow)
        {
            _engine = _engine.SetValue("Lobby", new LobbyWrapper(_lc))
                             .SetValue("Ref", new RefWrapper(_arc))
                             .SetValue("Convert", new ConvertWrapper())
                             .SetValue("Workflow", workflow)
                             .SetValue("Random", Program.Random)
                             .SetValue("RND", Program.Random);
        }

        void AddFunctions()
        {
            _engine = _engine.SetValue("ToString", new Func<object, string>(o => o.ToString()));
            _engine = _engine.SetValue("GetType", new Func<object, Type>(o => o.GetType()));
            _engine = _engine.SetValue("Equals", new Func<object, object, bool>((a, b) => a == null ? false : b == null ? false : a.Equals(b)));
            _engine = _engine.SetValue("CurrentTimeUtc", new Func<DateTime>(() => DateTime.UtcNow));
        }
    }
}
