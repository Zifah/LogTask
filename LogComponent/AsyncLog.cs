namespace LogTest
{
    using System;
    using System.Collections.Concurrent;

    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;

    public class AsyncLog : ILog
    {
        private Thread _runThread;
        private ConcurrentQueue<LogLine> _lines = new ConcurrentQueue<LogLine>();

        private StreamWriter _writer;

        private bool _exit;

        public AsyncLog()
        {
            if (!Directory.Exists(@"C:\LogTest"))
                Directory.CreateDirectory(@"C:\LogTest");

            _writer = File.AppendText(@"C:\LogTest\Log" + DateTime.Now.ToString("yyyyMMdd HHmmss fff") + ".log");

            _writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);

            _writer.AutoFlush = true;

            _runThread = new Thread(MainLoop);
            _runThread.Start();
        }

        private bool _QuitWithFlush = false;


        DateTime _curDate = DateTime.Now;

        private void MainLoop()
        {
            while (!_exit)
            {
                // TODO Hafiz: Open the file
                int maxWritesPerBatch = 5;

                while (!_exit && maxWritesPerBatch > 0)
                {
                    maxWritesPerBatch--; // TODO Hafiz: Why do we have this here?
                    var hasLog = _lines.TryPeek(out var logLine);

                    if (!hasLog || logLine == null)
                    {
                        continue;
                    }

                    StringBuilder stringBuilder = new();

                    if ((DateTime.Now - _curDate).Days != 0) // TODO Hafiz: Verify the correctness of this calculation
                    {
                        _curDate = DateTime.Now;

                        _writer = File.AppendText(@"C:\LogTest\Log" + DateTime.Now.ToString("yyyyMMdd HHmmss fff") + ".log");

                        _writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);

                        stringBuilder.Append(Environment.NewLine);

                        _writer.Write(stringBuilder.ToString());

                        _writer.AutoFlush = true;
                    }

                    stringBuilder.Append(logLine.Timestamp.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                    stringBuilder.Append('\t');
                    stringBuilder.Append(logLine.LineText());
                    stringBuilder.Append('\t');

                    stringBuilder.Append(Environment.NewLine);

                    if (_exit)
                    {
                        break;
                    }
                    _writer.Write(stringBuilder.ToString());
                    _lines.TryDequeue(out _);
                    // We can be sure that the last peeked item is the one we're dequeuing since only one thread dequeues from the queue
                    // We also ensure to dequeue from the log only after we have written the log entry
                }


                if (_QuitWithFlush == true && _lines.Count == 0)
                    _exit = true;

                Thread.Sleep(50);
            }
        }

        public void StopWithoutFlush()
        {
            _exit = true;
        }

        public void StopWithFlush()
        {
            _QuitWithFlush = true;
        }

        public void Write(string text)
        {
            _lines.Enqueue(new LogLine() { Text = text, Timestamp = DateTime.Now });
        }
    }
}