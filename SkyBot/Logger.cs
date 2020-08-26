using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace SkyBot
{
    public static class Logger
    {
        public const string LOG_FILE = "log.txt";
        public static object SyncRoot { get; } = new object();

        /// <summary>
        /// Log a message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="level">Log Level</param>
        /// <param name="member">Leave empty</param>
        public static void Log(object message, LogLevel level = LogLevel.Debug, [CallerMemberName] string member = null)
        {
            DateTime date = DateTime.UtcNow;

            ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
            {
                lock(SyncRoot)
                {
                    string msg = $"{date} *{level}* {member ?? "unkown member"}: {message}\n";

                    File.AppendAllText(LOG_FILE, msg);

                    switch(level)
                    {
                        default:
                            break;

                        case LogLevel.Debug:
#if RELEASE
                            return;
#endif
#if DEBUG
                            break;
#endif

                        case LogLevel.Warning:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        case LogLevel.Error:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                    }

                    Console.Write(msg);

                    switch(level)
                    {
                        default:
                            break;
                        case LogLevel.Warning:
                        case LogLevel.Error:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            break;
                    }
                }
            }));
        }
    }

    public enum LogLevel
    {
        Info,
        /// <summary>
        /// Hidden from console when in release build
        /// </summary>
        Debug,
        Warning,
        Error
    }
}
