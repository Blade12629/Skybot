using AutoRefTypes;
using SkyBot.Osu.AutoRef.Events;
using SkyBot.Osu.AutoRef.Scripting;
using SkyBot.Osu.AutoRef.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace SkyBot.Osu.AutoRef
{
    public class AutoRefEngine : IDisposable, IEquatable<AutoRefEngine>
    {
        public Guid Id { get; }
        public bool IsDisposed { get; private set; }
        public bool ValidSetup { get => _lc != null && _eventRunner != null && _pluginContext != null && !IsDisposed; }
        public LobbyController LC { get => _lc; }

        GoogleAPI.SpreadSheets.SpreadSheet _sheet;
        AutoRefSettings _refSettings;
        ARDiscordHandler _discord;
        LobbyController _lc;
        EventRunner _eventRunner;
        ScriptingPluginContext _pluginContext;
        CancellationTokenSource _tickToken;
        Task _tickTask;

        System.Timers.Timer _creationTimer;
        bool _wasClosed;

        public AutoRefEngine()
        {
            Id = Guid.NewGuid();
        }

        ~AutoRefEngine()
        {
            Dispose(false);
        }

        public void StartCreationTimer()
        {
            double delay = (_refSettings.CreationDate - DateTime.UtcNow).TotalMilliseconds;

            if (delay <= 0)
            {
                CreationTimerElapsed(this, null);
                return;
            }

            _creationTimer = new System.Timers.Timer(delay);

            _creationTimer.Elapsed += CreationTimerElapsed;
            _creationTimer.Start();
        }

        public void StopCreationTimer()
        {
            if (!_creationTimer.Enabled)
                return;

            _creationTimer.Stop();
        }

        public bool Setup(AutoRefSettings settings)
        {
            _refSettings = settings;
            return Setup(settings.ScriptFileName, true, settings.IsLibrary, settings.DiscordGuildId, settings.DiscordLogChannelId);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        void OnLobbyClose()
        {
            Task.Delay(3000).ConfigureAwait(false).GetAwaiter().GetResult();

            _wasClosed = true;
            Dispose();
            Management.AutoRefManager.DeregisterInstance(this);
        }

        bool LoadScriptsFromLibrary(string dll)
        {
            _pluginContext = new ScriptingPluginContext();
            return _pluginContext.LoadFromFile(dll);
        }

        bool LoadScriptFromFile(string file)
        {
            if (!File.Exists(file))
                return false;

            string script = File.ReadAllText(file);

            if (string.IsNullOrEmpty(script))
                return false;

            return LoadScript(script);
        }

        bool LoadScript(string script)
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

        void CreationTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_refSettings.SpreadsheetId) && !string.IsNullOrEmpty(_refSettings.SpreadsheetTable))
                _sheet = new GoogleAPI.SpreadSheets.SpreadSheet(_refSettings.SpreadsheetId, _refSettings.SpreadsheetTable);

            Run();
        }

        bool Setup(string script, bool isFile, bool isDll, ulong discordGuildId, ulong logChannelId)
        {
            _tickToken = new CancellationTokenSource();
            _tickTask = new Task(async () =>
            {
                while (_tickToken != null && !_tickToken.IsCancellationRequested)
                {
                    RefTick();
                    await Task.Delay(50).ConfigureAwait(false);
                }
            }, _tickToken.Token);

            _eventRunner = new EventRunner();
            _lc = new LobbyController(Program.IRC, _eventRunner);
            _discord = new ARDiscordHandler(Program.DiscordHandler, discordGuildId, logChannelId);

            _lc.OnLobbyClose += OnLobbyClose;

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

        [MethodImpl(MethodImplOptions.NoInlining)]
        void Run()
        {
            if (!ValidSetup)
                throw new Exception("Invalid Setup");

            Type[] types = _pluginContext.LoadedAssembly.GetTypes();
            Type entryType = types.First(t => t.GetInterfaces().Any(i => i.Equals(typeof(IEntryPoint))));

            IEntryPoint entryPoint = Activator.CreateInstance(entryType) as IEntryPoint;
            entryPoint.OnLoad(_lc, _eventRunner, _discord, _sheet, _refSettings.ScriptInput);

            Task.Run(() =>
            {

                while (_lc.DataHandler.Status != LobbyStatus.Created ||
                    DateTime.UtcNow - _lc.DataHandler.CreationDate < TimeSpan.FromSeconds(3))
                    Task.Delay(20).ConfigureAwait(false).GetAwaiter().GetResult();

                _tickTask.Start();
            });
            _lc.CreateLobby(_refSettings.LobbyName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                IsDisposed = true;

                if (!_wasClosed && _lc != null && (_lc.DataHandler.Status != LobbyStatus.Closed ||_lc.DataHandler.Status != LobbyStatus.None))
                    _lc.Close();

                _lc = null;

                if (_creationTimer?.Enabled ?? false)
                    _creationTimer?.Stop();

                _creationTimer = null;


                _tickToken?.Cancel();
                _tickTask?.Wait();
                _tickTask = null;
                _tickToken = null;

                _eventRunner?.Clear();
                _eventRunner = null;

                _discord = null;

                _sheet?.Dispose();
                _sheet = null;

                _refSettings = null;

                _pluginContext?.Unload();
                _pluginContext = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    
        void RefTick()
        {
            try
            {
                if (_lc.DataHandler.Status == LobbyStatus.Closed)
                    return;

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
            catch (Exception ex)
            {
                Logger.Log($"AutoRefEngine tick error, error:\n" + ex, LogLevel.Error);
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AutoRefEngine);
        }

        public bool Equals([AllowNull] AutoRefEngine other)
        {
            return other != null &&
                   Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public static bool operator ==(AutoRefEngine left, AutoRefEngine right)
        {
            return EqualityComparer<AutoRefEngine>.Default.Equals(left, right);
        }

        public static bool operator !=(AutoRefEngine left, AutoRefEngine right)
        {
            return !(left == right);
        }
    }
}
