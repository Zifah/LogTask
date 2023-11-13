using System;
using System.IO;
using LogComponent;
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
        var logFolder = Configuration.GetRequiredSection("LogFolder");

        if (logFolder.Value == null || !Path.IsPathFullyQualified(logFolder.Value))
        {
            throw new InvalidOperationException("LogFolder is not correctly configured");
        }

        // Configure services here
        services.AddSingleton(Configuration);
        services.AddTransient<MyService>();
        services.AddTransient<IClock, RealClock>();
    }
}