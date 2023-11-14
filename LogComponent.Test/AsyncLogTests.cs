using LogComponent.Clock;
using LogComponent.Log;
using LogComponent.LogWriter;
using Moq;

namespace LogComponent.Test;

public class AsyncLogTests
{
    private AsyncLog _asyncLog;
    private Mock<IClock> _clockMock;
    private Mock<ILogWriter> _logWriterMock;

    [SetUp]
    public void Setup()
    {
        _clockMock = new Mock<IClock>();
        _logWriterMock = new Mock<ILogWriter>();
        _asyncLog = new AsyncLog(_logWriterMock.Object, _clockMock.Object);
    }

    [TestCase(100)]
    [TestCase(200)]
    [TestCase(300)]
    public void Write_EachCallResultsInAWrite(int createdLogCount)
    {
        // Arrange
        _logWriterMock
            .Setup(x => x.Write(It.IsAny<string>()));

        // Act
        for (int i = 1; i <= createdLogCount; i++)
        {
            _asyncLog.Write($"{i}");
        }

        while (!_asyncLog.IsBufferEmpty())
        {
            // Wait for writes to complete
            Thread.Sleep(10);
        }

        // Assert
        _logWriterMock.Verify(x => x.Write(It.IsAny<string>()), Times.Exactly(createdLogCount));
    }

    [TestCase(340, 1000)]
    [TestCase(1000, 500)]
    [TestCase(746, 20)]
    public void StopWithFlush_PreventsFurtherWritesOnceLogBufferCleared
        (int createdLogCount, int unwrittenLogCount)
    {
        _logWriterMock
            .Setup(x => x.Write(It.IsAny<string>()));

        // Act
        for (int i = 1; i <= createdLogCount; i++)
        {
            _asyncLog.Write($"{i}");
            if (i == createdLogCount / 2)
            {
                _asyncLog.StopWithFlush();
            }
        }

        WaitForBufferEmpty();

        for (int i = 1; i <= unwrittenLogCount; i++)
        {
            _asyncLog.Write($"{i}");
        }

        Thread.Sleep(50); // Allow time for logger to write the second batch of logs

        // Assert
        _logWriterMock.Verify(x => x.Write(It.IsAny<string>()), Times.Exactly(createdLogCount));
        Assert.That(_asyncLog.IsBufferEmpty(), Is.False);
    }

    [TestCase(340)]
    [TestCase(1000)]
    [TestCase(746)]
    public void StopWithoutFlush_WhenCalled_PreventsAnyFurtherLogWrites
        (int createdLogCount)
    {
        // Arrange
        DateTime cutoffTime = DateTime.Now, lastWriteTime = DateTime.Now;
        _logWriterMock
            .Setup(x => x.Write(It.IsAny<string>()))
            .Callback(() => { lastWriteTime = DateTime.Now; });
        

        // Act
        for (int i = 1; i <= createdLogCount; i++)
        {
            _asyncLog.Write($"{i}");
            if (i == createdLogCount / 2)
            {
                _asyncLog.StopWithoutFlush();
                cutoffTime = DateTime.Now;
            }
        }

        Thread.Sleep(100); // Allow time for logger to write unwritten logs

        // Assert
        Assert.That(lastWriteTime, Is.LessThan(cutoffTime));
        _logWriterMock.Verify(x => x.Write(It.IsAny<string>()), Times.AtMost(createdLogCount / 2));
        Assert.That(_asyncLog.IsBufferEmpty(), Is.False);
    }

    private bool WaitForBufferEmpty()
    {
        var waited = false;
        while (!_asyncLog.IsBufferEmpty())
        {
            waited = true;
            Thread.Sleep(10);
        }
        return waited;
    }
}