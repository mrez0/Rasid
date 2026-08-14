using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rasid.App.ViewModels;
using Rasid.Core.Data;
using Serilog;

namespace Rasid.App;

public static class AppHost
{
    public static IHost Build()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .WriteTo.File(
                Path.Combine(AppPaths.LogFolder, "rasid-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14)
            .CreateLogger();

        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.Logging.ClearProviders();
        builder.Services.AddSerilog();

        builder.Services.AddDbContextFactory<RasidDbContext>(options =>
            options.UseSqlite($"Data Source={AppPaths.DatabaseFile}")
        );

        builder.Services.AddTransient<MainViewModel>();

        return builder.Build();
    }
}