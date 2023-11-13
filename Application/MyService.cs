using Microsoft.Extensions.Configuration;

namespace Application
{
    using System.Threading;

    using LogComponent;

    public class MyService
    {
        private readonly IConfiguration _configuration;
        private readonly IClock _clock;

        public MyService(IClock clock, IConfiguration configuration)
        {
            _configuration = configuration;
            _clock = clock;
        }

        public void Run()
        {
            var logFolder = _configuration.GetValue<string>("LogFolder");

            ILog logger = new AsyncLog(new FileLogWriter(_clock, logFolder), _clock);
            for (int i = 0; i < 15; i++)
            {
                logger.Write("Number with Flush: " + i.ToString());
                Thread.Sleep(50);
            }

            logger.StopWithFlush();

            ILog logger2 = new AsyncLog(new FileLogWriter(_clock, logFolder), _clock);
            for (int i = 50; i > 0; i--)
            {
                logger2.Write("Number with No flush: " + i.ToString());
                Thread.Sleep(20);
            }

            logger2.StopWithoutFlush();
        }
    }
}
