using LogComponent.Clock;
using LogComponent.LogWriter;
using Moq;

namespace LogComponent.Test.LogWriter
{
    public class FileLogWriterTests
    {
        private Mock<IClock> _clockMock;
        private FileLogWriter _logWriter;
        private const string _logFolder = "C:/LogTest";
        private Guid _tempLogFolder;
        private string LogFolder => Path.Combine(_logFolder, _tempLogFolder.ToString());

        [SetUp]
        public void Setup()
        {
            _clockMock = new Mock<IClock>();
            _tempLogFolder = Guid.NewGuid();
            _logWriter = new FileLogWriter(_clockMock.Object, LogFolder);
        }

        [TearDown]
        public void Teardown()
        {
            new DirectoryInfo(LogFolder).Delete(true);
        }

        [TestCase(50, "2023-11-16 00:00")]
        [TestCase(100, "2023-11-16 02:50")]
        [TestCase(160, "2023-11-16 10:00")]
        [TestCase(200, "2023-11-16 08:25")]
        [TestCase(500, "2023-11-16 18:05")]
        [TestCase(180, "2023-11-16 23:59")]
        [TestCase(950, "2023-11-25 00:00")]
        [TestCase(600, "2023-11-25 13:00")]
        public void Write_NewFileIsCreated_OnlyAfterCurrentDayEnds(int logCount, string nextDayTime)
        {
            var tomorrow = DateTime.Parse(nextDayTime);
            var today = new DateTime(2023, 11, 15);
            var isNextDay = false;
            var expectedFileCount = 2;

            // Arrange
            _clockMock.Setup(c => c.CurrentTime).Returns(isNextDay ? tomorrow : today.RandomizeTime());


            // Act
            for (int i = 1; i <= logCount; i++)
            {
                _logWriter.Write(i.ToString());
            }

            var runEndOfDay = () => isNextDay = true;
            runEndOfDay();

            for (int i = 1; i <= logCount; i++)
            {
                _logWriter.Write(i.ToString());
            }

            // Assert
            Assert.That(Helpers.CountFilesInFolder(LogFolder), Is.EqualTo(expectedFileCount));
        }
    }
}
