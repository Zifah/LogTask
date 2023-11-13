using System.IO;

namespace LogComponent
{
    public interface ILogWriter
    {
        public void Write(string logText);
    }
}