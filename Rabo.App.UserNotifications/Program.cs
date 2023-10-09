using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rabo.App.UserNotifications.Services;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<UserNotifications>();
                    services.AddTransient<IServiceBusQueueProvider, ServiceBusQueueProvider>();
                })
                .Build();

        host.Run();
    }
}