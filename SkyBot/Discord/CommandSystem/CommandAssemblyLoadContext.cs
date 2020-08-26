using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace SkyBot.Discord.CommandSystem
{
    public class CommandAssemblyLoadContext : AssemblyLoadContext
    {
        public Assembly Assembly { get; set; }

        public CommandAssemblyLoadContext() : base(true)
        {
            
        }

        public bool Load(string file)
        {
            try
            {
                FileInfo fi = new FileInfo(file);

                if (!fi.Exists)
                    return false;

                Assembly = LoadFromAssemblyPath(fi.FullName);

                if (Assembly == null)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex, LogLevel.Error);
            }

            return false;
        }

        public new bool Unload()
        {
            WeakReference wr = new WeakReference(Assembly);
            Assembly = null;

            base.Unload();

            GC.Collect(); //lgtm [cs/call-to-gc]
            GC.SuppressFinalize(this);

            int waitCounter = 0;

            while(wr.IsAlive || waitCounter < 5000)
            {
                Task.Delay(1).Wait();
                waitCounter++;
            }

            if (wr.IsAlive)
                return false;

            return true;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            return IntPtr.Zero;
        }
    }
}
