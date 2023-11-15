using LogComponent.Clock;
using LogComponent.LogWriter;
using Moq;

namespace LogComponent.Test.LogWriter
{
    public class FileLogWriterTests
    {
        private const string _logFolder = "C:/LogTest";
        private Mock<IClock> _clockMock;
        private FileLogWriter _logWriter;
        private Guid _tempLogFolder;
        private string LogFolder => Path.Combine(_logFolder, _tempLogFolder.ToString());


        [SetUp]
        public void Setup()
        {
            _tempLogFolder = Guid.NewGuid();
            _clockMock = new Mock<IClock>();
            _logWriter = new FileLogWriter(_clockMock.Object, LogFolder);
        }

        [TearDown]
        public void Teardown()
        {
            new DirectoryInfo(LogFolder).Delete(true);
        }

        // INTERVIEW NOTE: I would use a test data generation library here to improve readability
        [TestCase(3, "2023-11-15 14:05", "2023-11-16 00:00", "2023-11-17 00:00")]
        [TestCase(3, "2023-11-15 18:00", "2023-11-15 03:00", "2023-11-16 00:00", "2023-11-16 23:43", "2023-11-17 00:05")]
        [TestCase(2, "2023-11-15 18:00", "2023-11-15 03:00", "2023-11-16 00:00", "2023-11-16 23:43")]
        [TestCase(1, "2023-11-15 00:00", "2023-11-15 00:00", "2023-11-15 00:00")]
        [TestCase(1, "2023-11-16 00:00", "2023-11-16 00:00", "2023-11-16 00:00")]
        public void Write_NewFileIsCreated_OnlyAfterCurrentDayEnds(int expectedFileCount, params string[] writeTimes)
        {
            // Act
            foreach(var time in writeTimes.Select(DateTime.Parse))
            {
                _clockMock.SetupGet(x => x.CurrentTime).Returns(time.RandomizeTime());
                _logWriter.Write("TestLog\n");
            }

            // Assert
            Assert.That(Helpers.CountFilesInFolder(LogFolder), Is.EqualTo(expectedFileCount));
        }
    }
}
