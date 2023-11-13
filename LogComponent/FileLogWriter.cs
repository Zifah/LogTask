using System;
using System.IO;

namespace LogComponent
{
    public class FileLogWriter : ILogWriter
    {
        private readonly IClock _clock;

        private DateTime _curDate;
        private const string LogFolder = @"C:\LogTest";
        private string LogFilePath => Path.Combine(LogFolder, $"Log{_curDate:yyyyMMdd HHmmss fff}.log");

        public FileLogWriter(IClock clock)
        {
            _clock = clock;
            _curDate = DateTime.Now;
            if (!Directory.Exists(LogFolder))
                Directory.CreateDirectory(LogFolder);
            InitializeLogFile();
        }

        public void Write(string logText)
        {
            EnsureFileRollOver();
            //TODO Hafiz: Make it possible to keep connection open for a few moments in case of rapid consecutive writes
            using var writer = File.AppendText(LogFilePath);
            writer.AutoFlush = true;
            writer.Write(logText);
        }

        private void InitializeLogFile()
        {
            if (!File.Exists(LogFilePath))
            {
                File.WriteAllText(LogFilePath,
               "Timestamp".PadRight(25, ' ') + '\t' + "Data".PadRight(15, ' ') + '\t' + Environment.NewLine);
            }
        }

        private void EnsureFileRollOver()
        {
            if (DateTime.Now.Day > _curDate.Day)
            {
                _curDate = _clock.CurrentTime;
                InitializeLogFile();
            }
        }
    }
}