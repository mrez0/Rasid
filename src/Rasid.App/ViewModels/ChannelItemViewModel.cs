using CommunityToolkit.Mvvm.ComponentModel;
using Rasid.Core.Models;

namespace Rasid.App.ViewModels;

public partial class ChannelItemViewModel : ViewModelBase
{
    private readonly Channel _channel;

    public ChannelItemViewModel(Channel channel)
    {
        _channel = channel;
        Name = channel.Name;
        Handle = channel.Handle ?? "";
    }

    public string Id => _channel.Id;

    public string LastChecked => _channel.LastCheckedUtc is null
        ? "Never checked"
        : _channel.LastCheckedUtc.Value.ToLocalTime().ToString("g");

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string Handle { get; set; }

    [ObservableProperty]
    public partial bool IsChecking { get; set; }

    public void Refresh()
    {
        OnPropertyChanged(nameof(LastChecked));
    }
}