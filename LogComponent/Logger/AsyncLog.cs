namespace LogComponent.Logger;

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
    private readonly ILogWriter _logWriter;
    private readonly IClock _clock;
    private readonly int _maxConsecutiveExceptions;

    private static readonly object _exitLock = new object();

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

    private bool _exit;
    private bool Exit
    {
        get
        {
            return _exit;
        }
        set
        {
            lock (_exitLock)
            {
                _exit = true; // Prevent reversal of an exit
            }
        }
    }

    private bool QuitWithFlush { get; set; }

    public void StopWithoutFlush()
    {
        Exit = true;
    }

    public void StopWithFlush()
    {
        QuitWithFlush = true;
        while (!IsBufferEmpty())
        {
        }
    }

    public void Write(string text)
    {
        if (!QuitWithFlush)
        {
            _lines.Enqueue(new LogLine() { Text = text, Timestamp = _clock.CurrentTime });
        }
    }

    private void MainLoop()
    {
        int exceptionCount = 0;
        while (!Exit)
        {
            lock (_exitLock)
            {
                WriteNextLog(ref exceptionCount);
                if (!Exit && QuitWithFlush && IsBufferEmpty())
                {
                    StopWithoutFlush();
                }
            }
        }
    }

    private void WriteNextLog(ref int exceptionCount)
    {
        var hasLog = _lines.TryPeek(out var logLine);

        if (!hasLog || !IsLogLineValid(logLine) || Exit)
        {
            return;
        }

        try
        {
            // INTERVIEW NOTE: A clear improvement here would be to write in batches for better performance
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

    public bool IsBufferEmpty()
    {
        return _lines.IsEmpty;
    }
}
