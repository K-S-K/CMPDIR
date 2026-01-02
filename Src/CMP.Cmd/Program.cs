using CMP.Lib;
using CMP.Lib.Analysis;
using CMP.Lib.Diagnostics;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

internal static class Program
{
    public static int Main(string[] args)
    {
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton<IReportService, ConsoleReportService>();
                services.AddSingleton<IProgressReporter, ConsoleProgressReporter>();
                services.AddSingleton<DirectoryProcessor>();
                services.AddSingleton<DirectoryCompareApp>();
            })
            .Build();

        var app = host.Services.GetRequiredService<DirectoryCompareApp>();
        return app.Run(args);
    }
}
