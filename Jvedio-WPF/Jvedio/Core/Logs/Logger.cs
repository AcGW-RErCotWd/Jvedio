﻿using SuperUtils.Framework.Logger;
using SuperUtils.IO;
using System;
using System.IO;

namespace Jvedio.Core.Logs
{
    /// <summary>
    /// 日志记录
    /// </summary>
    public class Logger : AbstractLogger
    {

        private static string FilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        private static object LogLock { get; set; }

        private Logger() { }

        public static Logger Instance { get; }

        static Logger()
        {
            Instance = new Logger();
            Instance.LogLevel = Level.Debug;
            LogLock = new object();
        }

        public override void LogPrint(string str)
        {
            Console.Write(str);
            if (!Directory.Exists(FilePath))
                SuperUtils.IO.DirHelper.TryCreateDirectory(FilePath);
            string filepath = System.IO.Path.Combine(FilePath, DateTime.Now.ToString("yyyy-MM-dd") + ".log");
            lock (LogLock) {
                FileHelper.TryAppendToFile(filepath, str);
            }
        }

    }
}
