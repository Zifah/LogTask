namespace LogTest
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Text;
    using System.Threading;

    public class AsyncLog : ILog
    {
        private readonly Thread _runThread;
        private readonly ConcurrentQueue<LogLine> _lines = new ConcurrentQueue<LogLine>();
        private const string LogFolder = @"C:\LogTest";
        private bool _exit;
        private bool _quitWithFlush = false;
        DateTime _curDate = DateTime.Now;

        private string LogFilePath => @$"{LogFolder}\Log" + _curDate.ToString("yyyyMMdd HHmmss fff") + ".log";
        private StreamWriter GetWriter
        {
            get
            {
                EnsureFileRollOver();
                return File.AppendText(LogFilePath);
            }
        }

        private void EnsureFileRollOver()
        {
            if (DateTime.Now.Day > _curDate.Day)
            {
                _curDate = DateTime.Now;
                InitializeLogFile();
            }

        }

        private void InitializeLogFile()
        {
            if (!File.Exists(LogFilePath))
            {
                File.WriteAllText(LogFilePath,
               "Timestamp".PadRight(25, ' ') + '\t' + "Data".PadRight(15, ' ') + '\t' + Environment.NewLine);

            }
        }

        public AsyncLog()
        {
            if (!Directory.Exists(LogFolder))
                Directory.CreateDirectory(LogFolder);

            InitializeLogFile();

            _runThread = new Thread(MainLoop);
            _runThread.Start();
        }

        private void MainLoop()
        {
            while (!_exit)
            {
                using (var writer = GetWriter)
                {
                    writer.AutoFlush = true;
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
                        writer.Write(BuildLogLine(logLine.Timestamp, logLine.LineText()).ToString());
                        _lines.TryDequeue(out _);
                        // We can be sure that the last peeked item is the one we're dequeuing since only this thread dequeues
                        // We also ensure to dequeue from the log only after we have written the log entry
                    }
                }

                _exit = _exit || (_quitWithFlush == true && _lines.IsEmpty);
                Thread.Sleep(50);
            }
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