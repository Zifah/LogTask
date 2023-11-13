﻿namespace LogComponent
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Text;
    using System.Threading;

    public class AsyncLog : ILog
    {
        private readonly Thread _runThread;
        private readonly ConcurrentQueue<LogLine> _lines = new();
        private bool _exit;
        private bool _quitWithFlush = false;
        private readonly ILogWriter _logWriter;

        public AsyncLog(ILogWriter logWriter)
        {
            _logWriter = logWriter;
            _runThread = new Thread(MainLoop);
            _runThread.Start();
        }

        private void MainLoop()
        {
            while (!_exit)
            {
                for (int i = 0; i < 5; i++)
                {
                    var hasLog = _lines.TryPeek(out var logLine);

                    if (!hasLog)
                    {
                        break;
                    }

                    if (logLine == null)
                    {
                        // Prevent null reference exceptions
                        _lines.TryDequeue(out _);
                        Write("Found null log entry 😱");
                        continue;
                    }

                    if (_exit)
                    {
                        break;
                    }

                    _logWriter.Write(BuildLogLine(logLine.Timestamp, logLine.LineText()).ToString());
                    _lines.TryDequeue(out _);
                }
            }

            _exit = _exit || (_quitWithFlush == true && _lines.IsEmpty);
            Thread.Sleep(50);
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

        public void StopWithoutFlush()
        {
            _exit = true;
        }

        public void StopWithFlush()
        {
            _quitWithFlush = true;
        }

        public void Write(string text)
        {
            _lines.Enqueue(new LogLine() { Text = text, Timestamp = DateTime.Now });
        }
    }


}
