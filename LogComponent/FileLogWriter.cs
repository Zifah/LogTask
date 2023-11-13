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
        private const int WriterIdleTimeoutMs = 5;
        private string LogFilePath => Path.Combine(LogFolder, $"Log{_curDate:yyyyMMdd HHmmss fff}.log");

        /// <summary>
        /// Prevents <see cref="_currentWriter"> from being used for writing while it is being disposed
        /// </summary>
        private static readonly object _writerManagementLock = new();

        private readonly ConcurrentDictionary<StreamWriter, List<Guid>> _writeLocks;

        public FileLogWriter(IClock clock)
        {
            _clock = clock;
            _curDate = _clock.CurrentTime;
            _writeLocks = new ConcurrentDictionary<StreamWriter, List<Guid>>();

            if (!Directory.Exists(LogFolder))
                Directory.CreateDirectory(LogFolder);
            InitializeLogFile();
        }

        private bool _isCurrentWriterDisposed = false;
        private StreamWriter? _currentWriter;
        private StreamWriter GetWriter(Guid lockKey)
        {
            lock (_writerManagementLock)
            {
                var createNewWriter = _currentWriter == null || _isCurrentWriterDisposed || EnsureFileRollOver();
                if (createNewWriter)
                {
                    var writer = File.AppendText(LogFilePath);
                    writer.AutoFlush = true;
                    TakeWriteLock(writer, lockKey);

                    Timer countdownTimer = new(o => TryDisposeWriter(o));
                    _currentWriter = writer;
                    _isCurrentWriterDisposed = false;
                    countdownTimer.Change(WriterIdleTimeoutMs, WriterIdleTimeoutMs);
                }
                return _currentWriter;
            }
        }

        private void TryDisposeWriter(object? state)
        {
            if (state is not Timer timer || _currentWriter is null)
            {
                return; // Unexpected, should log
            }

            if (!_writeLocks[_currentWriter].Any() && !_isCurrentWriterDisposed)
            {
                lock (_writerManagementLock)
                {
                    using (timer)
                    {
                        using (_currentWriter)
                        {
                            _isCurrentWriterDisposed = true;
                        }

                        timer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                }
                Console.WriteLine("Successfully disposed of current writer and timer");
            }
            else
            {
                Console.WriteLine("Writer is busy");
            }
        }

        public void Write(string logText)
        {
            var lockKey = Guid.NewGuid();
            var writer = GetWriter(lockKey);

            try
            {
                writer.Write(logText);
            }
            finally
            {
                ReleaseWriteLock(writer, lockKey);
            }
        }

        private void ReleaseWriteLock(StreamWriter writer, Guid lockKey)
        {
            _writeLocks[writer].Remove(lockKey);
        }

        private void TakeWriteLock(StreamWriter writer, Guid lockKey)
        {
            // Implement your logic to generate and return a GUID here
            _writeLocks.TryAdd(writer, new List<Guid>());
            _writeLocks[writer].Add(lockKey);
        }

        private void InitializeLogFile()
        {
            // TODO Hafiz: Make thread-safe
            if (!File.Exists(LogFilePath))
            {
                File.WriteAllText(LogFilePath,
               "Timestamp".PadRight(25, ' ') + '\t' + "Data".PadRight(15, ' ') + '\t' + Environment.NewLine);
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