using System;
using System.IO;
using LogComponent.Clock;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var logFolder = Configuration.GetRequiredSection(Constants.ConfigKeyLogFolder);

        if (logFolder.Value == null || !Path.IsPathFullyQualified(logFolder.Value))
        {
            throw new InvalidOperationException($"{Constants.ConfigKeyLogFolder} is not correctly configured");
        }

        // Configure services here
        services.AddSingleton(Configuration);
        services.AddTransient<MyService>();
        services.AddTransient<IClock, RealClock>();
    }
}