using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace SkyBot.Osu.AutoRef.Scripting
{
    public class ScriptingPluginContext : AssemblyLoadContext
    {

        public bool IsAlive { get => _reference != null && _reference.IsAlive; }

        public Assembly LoadedAssembly
        {
            get
            {
                if (_reference == null || !_reference.IsAlive)
                    return null;

                return _reference.Target as Assembly;
            }
        }

        public event Action OnBeforeUnload;
        public event Action OnAfterUnload;

        WeakReference _reference;

        public ScriptingPluginContext() : base(true)
        {
            
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool LoadFromFile(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException("File not found", file);

            _reference = new WeakReference(LoadFromAssemblyPath(new FileInfo(file).FullName));

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public new void Unload()
        {
            OnBeforeUnload?.Invoke();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            base.Unload();

            OnAfterUnload?.Invoke();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void LoadFromStream(MemoryStream mstream)
        {
            _reference = new WeakReference(LoadFromStream((Stream)mstream));
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            return IntPtr.Zero;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return base.Load(assemblyName);
        }
    }
}
