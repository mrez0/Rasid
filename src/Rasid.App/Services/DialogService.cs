using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Rasid.Core.Abstractions;

namespace Rasid.App.Services;

public class DialogService : IDialogService
{
    public async Task<bool> ConfirmAsync(string title, string message, string confirmText = "Yes")
    {
        Window? owner = GetMainWindow();

        if (owner is null)
        {
            return false;
        }

        bool result = false;

        Window dialog = new()
        {
            Title = title,
            Width = 420,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false
        };

        Button confirmButton = new()
        {
            Content = confirmText, IsDefault = true
        };

        Button cancelButton = new()
        {
            Content = "Cancel", IsCancel = true
        };

        confirmButton.Click += (_, _) =>
        {
            result = true;
            dialog.Close();
        };

        cancelButton.Click += (_, _) =>
        {
            result = false;
            dialog.Close();
        };

        StackPanel buttonsPanel = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8
        };

        buttonsPanel.Children.Add(confirmButton);
        buttonsPanel.Children.Add(cancelButton);

        StackPanel layout = new()
        {
            Margin = new Thickness(20),
            Spacing = 16
        };

        layout.Children.Add(new TextBlock
        {
            Text = message, TextWrapping = TextWrapping.Wrap
        });
        layout.Children.Add(buttonsPanel);

        dialog.Content = layout;

        await dialog.ShowDialog(owner);

        return result;
    }

    private Window? GetMainWindow() =>
        Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
}