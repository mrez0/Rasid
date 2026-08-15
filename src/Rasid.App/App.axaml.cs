using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Rasid.App.ViewModels;
using Rasid.App.Views;

namespace Rasid.App;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainViewModel viewModel = Program.Host.Services.GetRequiredService<MainViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel
            };

            _ = viewModel.LoadChannelsAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}