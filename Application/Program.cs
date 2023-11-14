using System;
using Microsoft.Extensions.Configuration;

namespace Application;

using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
{
    static void Main(string[] args)
    {
        var host = new HostBuilder()
                    .ConfigureHostConfiguration(configHost =>
                                {
                                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                                    configHost.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                                })
                    .ConfigureServices((hostContext, services) =>
                    {
                        new Startup(hostContext.Configuration).ConfigureServices(services);
                    })
                    .Build();

        using (var serviceScope = host.Services.CreateScope())
        {
            var services = serviceScope.ServiceProvider;
            var myService = services.GetRequiredService<MyService>();
            myService.Run();            
        }

        host.Run();
    }
}