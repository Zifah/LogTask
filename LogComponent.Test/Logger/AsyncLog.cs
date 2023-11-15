using LogComponent.Clock;
using LogComponent.LogWriter;
using Moq;
using Log = LogComponent.Logger;

namespace LogComponent.Test.Logger;

public class AsyncLog
{
    private Log.AsyncLog _asyncLog;
    private Mock<IClock> _clockMock;
    private Mock<ILogWriter> _logWriterMock;

    [SetUp]
    public void Setup()
    {
        _clockMock = new Mock<IClock>();
        _logWriterMock = new Mock<ILogWriter>();
        _asyncLog = new Log.AsyncLog(_logWriterMock.Object, _clockMock.Object);
    }

    [TestCase(100)]
    [TestCase(200)]
    [TestCase(300)]
    public void Write_EachCallResultsInAWrite(int createdLogCount)
    {
        // Arrange
        _logWriterMock
            .Setup(x => x.Write(It.IsAny<string>()));
        var expectedWriteCount = createdLogCount * 3;

        // Act
        var task1 = Task.Run(() => LogNumbers(createdLogCount));
        var task2 = Task.Run(() => LogNumbers(createdLogCount));
        var task3 = Task.Run(() => LogNumbers(createdLogCount));

        Task.WaitAll(task1, task2, task3);
        WaitForBufferEmpty();

        // Assert
        _logWriterMock.Verify(x => x.Write(It.IsAny<string>()), Times.Exactly(expectedWriteCount));
    }

    [TestCase(340)]
    [TestCase(1000)]
    [TestCase(746)]
    public void StopWithFlush_PreventsFurtherWritesOnceLogBufferCleared
        (int createdLogCount)
    {
        _logWriterMock
            .Setup(x => x.Write(It.IsAny<string>()));

        var minExpectedWrites = createdLogCount;
        var maxExpectedWrites = createdLogCount + createdLogCount;

        LogNumbers(createdLogCount);
        Task.Run(() => LogNumbers(createdLogCount));
        _asyncLog.StopWithFlush();
        LogNumbers(createdLogCount);

        Thread.Sleep(50); // Allow time for logger to write the second batch of logs

        // Assert
        _logWriterMock.Verify(x => x.Write(It.IsAny<string>()), 
            Times.Between(minExpectedWrites, maxExpectedWrites, Moq.Range.Inclusive));
    }

    private void LogNumbers(int createdLogCount)
    {
        for (int i = 1; i <= createdLogCount; i++)
        {
            _asyncLog.Write($"{i}");
        }
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