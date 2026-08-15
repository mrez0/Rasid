using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rasid.Core.Data;
using Rasid.Core.Models;

namespace Rasid.App.ViewModels;

public partial class MainViewModel(IDbContextFactory<RasidDbContext> dbContextFactory, ILogger<MainViewModel> logger)
    : ViewModelBase
{
    private readonly IDbContextFactory<RasidDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<MainViewModel> _logger = logger;

    public ObservableCollection<ChannelItemViewModel> Channels { get; } = [];

    [ObservableProperty]
    public partial ChannelItemViewModel? SelectedChannel { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

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
}