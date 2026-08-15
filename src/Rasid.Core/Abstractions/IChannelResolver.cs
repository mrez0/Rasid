namespace Rasid.Core.Abstractions;

public interface IChannelResolver
{
    Task<ChannelInfo?> ResolveAsync(string url, CancellationToken token = default);
}