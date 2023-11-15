using Microsoft.Extensions.Configuration;

using System;
using LogComponent.Clock;
using LogComponent.LogWriter;
using LogComponent.Logger;

namespace Application;
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
        var logFolder = _configuration.GetValue<string>(Constants.ConfigKeyLogFolder);

        if (string.IsNullOrWhiteSpace(logFolder))
        {
            throw new ArgumentNullException($"{Constants.ConfigKeyLogFolder} is not correctly configured");
        }

        ILog logger = new AsyncLog(new FileLogWriter(_clock, logFolder), _clock);
        for (int i = 0; i < 15; i++)
        {
            logger.Write("Number with Flush: " + i.ToString());
        }

        logger.StopWithFlush();

        ILog logger2 = new AsyncLog(new FileLogWriter(_clock, logFolder), _clock);
        for (int i = 50; i > 0; i--)
        {
            logger2.Write("Number with No flush: " + i.ToString());
        }

        logger2.StopWithoutFlush();
    }
}
