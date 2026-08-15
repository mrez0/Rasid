using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rasid.App.ViewModels;
using Rasid.Core.Abstractions;
using Rasid.Core.Data;
using Rasid.Core.Services;
using Serilog;
using Serilog.Events;

namespace Rasid.App;

public static class AppHost
{
    public static IHost Build()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
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
        builder.Services.AddSingleton<IChannelResolver, YtDlpChannelResolver>();

        return builder.Build();
    }

    public static async Task MigrateDatabaseAsync(IHost host)
    {
        IDbContextFactory<RasidDbContext> factory =
            host.Services.GetRequiredService<IDbContextFactory<RasidDbContext>>();
        await using RasidDbContext db = await factory.CreateDbContextAsync();
        await db.Database.MigrateAsync();
    }
}