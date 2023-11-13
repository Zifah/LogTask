namespace LogComponent.Log
{
    using System;
    using System.Collections.Concurrent;
    using System.Text;
    using System.Threading;
    using LogComponent.Clock;
    using LogComponent.Helpers;
    using LogComponent.LogWriter;

    public class AsyncLog : ILog
    {
        private readonly Thread _runThread;
        private readonly ConcurrentQueue<LogLine> _lines;
        private bool _exit;
        private bool _quitWithFlush = false;
        private readonly ILogWriter _logWriter;
        private readonly IClock _clock;
        private readonly int _maxConsecutiveExceptions;

        public AsyncLog(ILogWriter logWriter, IClock clock)
        {
            Require.NotNull(clock, nameof(clock));
            Require.NotNull(logWriter, nameof(logWriter));
            _lines = new ConcurrentQueue<LogLine>();
            _maxConsecutiveExceptions = 100;

            _clock = clock;
            _logWriter = logWriter;
            _runThread = new Thread(MainLoop);
            _runThread.Start();
        }

        private static StringBuilder BuildLogLine(DateTime timestamp, string lineText)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append(timestamp.ToString("yyyy-MM-dd HH:mm:ss:fff"));
            stringBuilder.Append('\t');
            stringBuilder.Append(lineText);
            stringBuilder.Append('\t');
            stringBuilder.Append(Environment.NewLine);
            return stringBuilder;
        }

        public void StopWithoutFlush() => _exit = true;

        public void StopWithFlush() => _quitWithFlush = true;

        public void Write(string text) => _lines.Enqueue(new LogLine() { Text = text, Timestamp = _clock.CurrentTime });

        private void MainLoop()
        {
            int exceptionCount = 0;
            while (!_exit)
            {
                ProcessNextBatch(ref exceptionCount);
                Thread.Sleep(50);
                _exit = _exit || (_quitWithFlush == true && _lines.IsEmpty);
            }
        }

        private void ProcessNextBatch(ref int exceptionCount)
        {
            for (int i = 0; i < 5; i++)
            {
                var hasLog = _lines.TryPeek(out var logLine);

                if (!hasLog)
                {
                    break;
                }

                if (!IsLogLineValid(logLine))
                {
                    continue;
                }

                if (_exit)
                {
                    break;
                }

                try
                {
                    _logWriter.Write(BuildLogLine(logLine.Timestamp, logLine.LineText()).ToString());
                    _lines.TryDequeue(out _);
                    exceptionCount = 0;
                }
                catch (Exception ex)
                {
                    exceptionCount++;
                    LogExceptionOrPreventOverflow(ex, exceptionCount);
                }
            }
        }

        private void LogExceptionOrPreventOverflow(Exception ex, int exceptionCount)
        {
            if (exceptionCount > _maxConsecutiveExceptions)
            {
                StopWithoutFlush();
            }
            else
            {
                Write($"An {ex.GetType()} exception occurred; Message: {ex.Message}");
            }
        }

        private bool IsLogLineValid(LogLine? logLine)
        {
            if (logLine == null)
            {
                // Prevent null reference exceptions
                _lines.TryDequeue(out _);
                Write("Found null log entry 😱");
                return false;
            }
            return true;
        }
    }
}
