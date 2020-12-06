using AutoRefTypes;
using SkyBot.Osu.AutoRef.Events;
using SkyBot.Osu.AutoRef.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace SkyBot.Osu.AutoRef
{
    public class AutoRefEngine : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public bool ValidSetup { get => _lc != null && _eventRunner != null && _pluginContext != null && !IsDisposed; }
        public LobbyController LC { get => _lc; }

        LobbyController _lc;
        EventRunner _eventRunner;
        ScriptingPluginContext _pluginContext;
        CancellationTokenSource _tickToken;
        Task _tickTask;

        public AutoRefEngine()
        {
        }

        ~AutoRefEngine()
        {
            Dispose(false);
        }

        public bool LoadScriptsFromLibrary(string dll)
        {
            _pluginContext = new ScriptingPluginContext();
            return _pluginContext.LoadFromFile(dll);
        }

        public bool LoadScriptFromFile(string file)
        {
            if (!File.Exists(file))
                return false;

            string script = File.ReadAllText(file);

            if (string.IsNullOrEmpty(script))
                return false;

            return LoadScript(script);
        }

        public bool LoadScript(string script)
        {
            if (_pluginContext != null && _pluginContext.IsAlive)
                throw new Exception("Cannot load new plugin when old plugin context is still alive");


            if (!ScriptingCompiler.TryCompile(script, out MemoryStream assemblyStream, out List<Exception> exceptions) ||
                exceptions.Count > 0)
            {
                assemblyStream?.Dispose();
                throw new AggregateException(exceptions);
            }

            try
            {
                _pluginContext = new ScriptingPluginContext();
                _pluginContext.LoadFromStream(assemblyStream);
            }
            finally
            {
                assemblyStream?.Dispose();
            }

            return true;
        }

        public bool Setup(string script, bool isFile, bool isDll)
        {
            _tickToken = new CancellationTokenSource();
            _tickTask = new Task(async () =>
            {
                while(!_tickToken.IsCancellationRequested)
                {
                    RefTick();
                    await Task.Delay(50).ConfigureAwait(false);
                }
            }, _tickToken.Token);

            _eventRunner = new EventRunner();
            _lc = new LobbyController(Program.IRC, _eventRunner);

            if (isDll)
            {
                return LoadScriptsFromLibrary(script);
            }
            else
            {
                if (isFile)
                    return LoadScriptFromFile(script);
                else
                    return LoadScript(script);
            }
        }

        public void Run(string lobbyName)
        {
            if (!ValidSetup)
                throw new Exception("Invalid Setup");

            Type[] types = _pluginContext.LoadedAssembly.GetTypes();
            Type entryType = types.First(t => t.GetInterfaces().Any(i => i.Equals(typeof(IEntryPoint))));

            IEntryPoint entryPoint = Activator.CreateInstance(entryType) as IEntryPoint;
            entryPoint.OnLoad(_lc, _eventRunner);

            _tickTask.Start();
            _lc.CreateLobby(lobbyName);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                IsDisposed = true;

                if (_lc != null && (_lc.DataHandler.Status != LobbyStatus.Closed ||_lc.DataHandler.Status != LobbyStatus.None))
                    _lc.CloseLobby();

                _lc = null;

                _pluginContext?.Unload();

                _tickToken?.Cancel();
            }
        }
    
        void RefTick()
        {
            _lc.ProcessIncomingMessages();

            switch (_lc.DataHandler.Status)
            {
                case LobbyStatus.Created:
                case LobbyStatus.Playing:
                    _eventRunner.OnTick();
                    break;

                default:
                    break;
            }
        }
    }
}
