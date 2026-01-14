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
                services.AddSingleton<IReportService, ReportServiceConsole>();
                services.AddSingleton<IProgressReporter, ProgressReporterConsole>();
                services.AddSingleton<FilePairProcessor>();
                services.AddSingleton<DirectoryProcessor>();
                services.AddSingleton<CompareApp>();
            })
            .Build();

        var app = host.Services.GetRequiredService<CompareApp>();
        return app.Run(args);
    }
}
