using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rasid.Core.Abstractions;
using Rasid.Core.Data;
using Rasid.Core.Models;

namespace Rasid.App.ViewModels;

public partial class MainViewModel(
    IDbContextFactory<RasidDbContext> dbContextFactory,
    IChannelResolver channelResolver,
    ILogger<MainViewModel> logger)
    : ViewModelBase
{
    private readonly IDbContextFactory<RasidDbContext> _dbContextFactory = dbContextFactory;
    private readonly IChannelResolver _channelResolver = channelResolver;
    private readonly ILogger<MainViewModel> _logger = logger;

    public ObservableCollection<ChannelItemViewModel> Channels { get; } = [];

    [ObservableProperty]
    public partial ChannelItemViewModel? SelectedChannel { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string NewChannelUrl { get; set; } = "";

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "";

    public async Task LoadChannelsAsync()
    {
        IsLoading = true;

        try
        {
            await using RasidDbContext db = await _dbContextFactory.CreateDbContextAsync();
            List<Channel> channels = await db.Channels.OrderBy(c => c.Name).ToListAsync();

            Channels.Clear();

            foreach (Channel channel in channels)
            {
                Channels.Add(new ChannelItemViewModel(channel));
            }

            _logger.LogInformation("Loaded {Count} channels", Channels.Count);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load channels");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddChannelAsync()
    {
        string url = NewChannelUrl.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            StatusMessage = "Paste a channel URL first.";
        }

        StatusMessage = "Looking up channel...";

        try
        {
            ChannelInfo? info = await _channelResolver.ResolveAsync(url);

            if (info is null)
            {
                StatusMessage = "Could not read that URL. Is it a YouTube channel?";
                return;
            }

            await using RasidDbContext db = await _dbContextFactory.CreateDbContextAsync();

            if (await db.Channels.AnyAsync(c => c.Id == info.Id))
            {
                StatusMessage = $"{info.Name} is already in the list.";
                return;
            }

            Channel channel = new()
            {
                Id = info.Id,
                Name = info.Name,
                Handle = info.Handle,
                FolderName = MakeFolderName(info.Name),
                AddedUtc = DateTime.UtcNow,
                IsEnabled = true
            };

            db.Channels.Add(channel);
            await db.SaveChangesAsync();

            Channels.Add(new ChannelItemViewModel(channel));

            NewChannelUrl = "";
            StatusMessage = $"Added {info.Name}";
            _logger.LogInformation("Added channel {Id} ({Name})", info.Id, info.Name);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to add channel from {Url}", url);
            StatusMessage = "Something went wrong. Check the log.";
        }
    }

    private string MakeFolderName(string name)
    {
        char[] illegal = Path.GetInvalidFileNameChars();
        string cleaned = new(name.Where(c => !illegal.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "channel" : cleaned.Trim();
    }
}