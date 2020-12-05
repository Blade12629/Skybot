using AutoRefTypes;
using SkyBot.Osu.AutoRef.Events;
using SkyBot.Osu.AutoRef.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SkyBot.Osu.AutoRef
{
    public class AutoRefEngine : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public bool ValidSetup { get; private set; }

        LobbyController _lc;
        AutoRefController _arc;
        EventRunner _eventRunner;
        ScriptingPluginContext _pluginContext;

        public AutoRefEngine()
        {
            _eventRunner = new EventRunner();
        }

        ~AutoRefEngine()
        {
            Dispose(false);
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

        public void Setup(AutoRefBuilder builder)
        {
            _lc = new LobbyController(Program.IRC);
            _arc = new AutoRefController(_lc);
            builder.Apply(_arc);

            LoadScript(builder.Script);
        }

        public void Run(string lobbyName)
        {
            if (!ValidSetup)
                throw new Exception("Invalid Setup");

            Type[] types = _pluginContext.LoadedAssembly.GetTypes();
            Type entryType = types.First(t => t.GetInterfaces().Any(i => i.Equals(typeof(IEntryPoint))));

            IEntryPoint entryPoint = Activator.CreateInstance(entryType) as IEntryPoint;

            TickEvent.Initialize(_eventRunner, _arc, _lc);
            entryPoint.OnLoad(_arc, _lc, _eventRunner);
            _arc.Start(lobbyName);
            _eventRunner.Start();
        }

        public void Stop()
        {
            _eventRunner.Stop();
            _lc.EnqueueCloseLobby();
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

                _eventRunner?.Stop();
                _eventRunner = null;

                if (_lc != null && _lc.IsLobbyClosed)
                    _lc.EnqueueCloseLobby();

                _arc = null;
                _lc = null;

                _pluginContext?.Unload();
            }
        }
    }
}
