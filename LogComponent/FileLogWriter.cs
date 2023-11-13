using System;
using System.IO;

namespace LogComponent
{
    public class FileLogWriter : ILogWriter
    {
        private readonly IClock _clock;

        private DateTime _curDate;
        private readonly string _logFolder;
        private string LogFilePath => Path.Combine(_logFolder, $"Log{_curDate:yyyyMMdd HHmmss fff}.log");
        private static readonly object _lockObject = new();
        public FileLogWriter(IClock clock, string logFolder)
        {
            Require.NotNull(clock, nameof(clock));
            Require.NotNull(logFolder, nameof(logFolder));
            _clock = clock;
            _curDate = _clock.CurrentTime;

            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);

            _logFolder = logFolder;
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