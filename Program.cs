using RPA.StreamingNotification;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .UseSerilog()
    .ConfigureServices((hostContext, services) =>
    {
        Environment.SetEnvironmentVariable("BASEDIR", AppContext.BaseDirectory);
        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(hostContext.Configuration).CreateLogger();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
