using System;
using Avalonia;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Rasid.App;

internal sealed class Program
{
    public static IHost Host { get; private set; } = null!;

    [STAThread]
    public static void Main(string[] args)
    {
        Host = AppHost.Build();

        try
        {
            Log.Information("Rasid starting. Data folder: {Folder}", AppPaths.DataFolder);

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Rasid Crashed");
            throw;
        }
        finally
        {
            Log.Information("Rasid shutting down");
            Log.CloseAndFlush();
            Host.Dispose();
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}