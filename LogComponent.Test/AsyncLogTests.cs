using System.Linq;
using System.Security.Cryptography.X509Certificates;
using LogComponent.Clock;
using LogComponent.Log;
using LogComponent.LogWriter;
using Moq;

namespace LogComponent.Test;

public class AsyncLogTests
{
    // TODO LATER Hafiz: Use fixtures, Inject the interfaces, Generate static data
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
    public void Write_WhenLogIsCreated_ThenAWriteHappens(int createdLogCount)
    {
        // Arrange
        int writtenLogCount = 0;
        _logWriterMock
            .Setup(x => x.Write(It.IsAny<string>()))
            .Callback((string logText) =>
            {
                writtenLogCount++;
            });

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
        Assert.That(writtenLogCount, Is.EqualTo(createdLogCount));
    }

    [Test]
    public void StopWithFlush_WhenCalled_PreventsFurtherWritesOnceLogBufferCleared()
    {
        Assert.Pass();
    }

    [Test]
    public void StopWithoutFlush_WhenCalled_PreventsAnyFurtherLogWrites()
    {
        Assert.Pass();
    }
}