using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace LogComponent
{
    public class FileLogWriter : ILogWriter
    {
        private readonly IClock _clock;

        private DateTime _curDate;
        private const string LogFolder = @"C:\LogTest";
        private const int WriterIdleTimeoutMs = 100;
        private string LogFilePath => Path.Combine(LogFolder, $"Log{_curDate:yyyyMMdd HHmmss fff}.log");

        /// <summary>
        /// Prevents <see cref="_currentWriter"> from being used for writing while it is being disposed
        /// </summary>
        private static readonly object _lockObject = new();
        public FileLogWriter(IClock clock)
        {
            _clock = clock;
            _curDate = _clock.CurrentTime;

            if (!Directory.Exists(LogFolder))
                Directory.CreateDirectory(LogFolder);
            InitializeLogFile();
        }

        private StreamWriter Writer
        {
            get
            {
                EnsureFileRollOver();
                var writer = File.AppendText(LogFilePath);
                writer.AutoFlush = true;
                return writer;
            }
        }
        public void Write(string logText)
        {
            using var writer = Writer;
            writer.AutoFlush = true;
            writer.Write(logText);
        }

        private void InitializeLogFile()
        {
            if (!File.Exists(LogFilePath))
            {
                lock (_lockObject)
                {
                    if (!File.Exists(LogFilePath))
                    {
                        File.WriteAllText(LogFilePath,
                       "Timestamp".PadRight(25, ' ') + '\t' + "Data".PadRight(15, ' ') + '\t' + Environment.NewLine);
                    }
                }
            }
        }

        private bool EnsureFileRollOver()
        {
            if (_clock.CurrentTime.Day > _curDate.Day)
            {
                _curDate = _clock.CurrentTime;
                InitializeLogFile();
                return true;
            }
            return false;
        }
    }
}