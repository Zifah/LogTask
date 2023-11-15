using LogComponent.Clock;
using LogComponent.LogWriter;
using Moq;

namespace LogComponent.Test.LogWriter
{
    public class FileLogWriterTests
    {
        private const string _logFolder = "C:/LogTest";
        private Guid _tempLogFolder;
        private string LogFolder => Path.Combine(_logFolder, _tempLogFolder.ToString());


        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void Teardown()
        {
            new DirectoryInfo(LogFolder).Delete(true);
        }

        [TestCase(3, "2023-11-15 14:05", "2023-11-16 00:00", "2023-11-17 00:00")]
        [TestCase(3, "2023-11-15 18:00", "2023-11-15 03:00", "2023-11-16 00:00", "2023-11-16 23:43", "2023-11-17 00:05")]
        [TestCase(2, "2023-11-15 18:00", "2023-11-15 03:00", "2023-11-16 00:00", "2023-11-16 23:43")]
        [TestCase(1, "2023-11-15 00:00", "2023-11-15 00:00", "2023-11-15 00:00")]
        [TestCase(1, "2023-11-16 00:00", "2023-11-16 00:00", "2023-11-16 00:00")]
        public void Write_NewFileIsCreated_OnlyAfterCurrentDayEnds(int expectedFileCount, params string[] writeTimes)
        {
            // Arrange
            var clockMock = new Mock<IClock>();

            var tempLogFolder = Guid.NewGuid();
            var logWriter = new FileLogWriter(clockMock.Object, LogFolder);

            // Act
            foreach(var time in writeTimes.Select(DateTime.Parse))
            {
                clockMock.SetupGet(x => x.CurrentTime).Returns(time.RandomizeTime());
                logWriter.Write("TestLog");
            }

            // Assert
            Assert.That(Helpers.CountFilesInFolder(LogFolder), Is.EqualTo(expectedFileCount));
        }
    }
}
