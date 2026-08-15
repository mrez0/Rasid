using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Rasid.Core.Abstractions;

namespace Rasid.Core.Services;

public class YtDlpChannelResolver(ILogger<YtDlpChannelResolver> logger) : IChannelResolver
{
    private const string YtDlpCommand = "yt-dlp";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

    public async Task<ChannelInfo?> ResolveAsync(string url, CancellationToken token = default)
    {
        using CancellationTokenSource timeoutSource = new(Timeout);
        using CancellationTokenSource linked =
            CancellationTokenSource.CreateLinkedTokenSource(token, timeoutSource.Token);

        ProcessStartInfo startInfo = new()
        {
            FileName = YtDlpCommand,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("--flat-playlist");
        startInfo.ArgumentList.Add("--playlist-items");
        startInfo.ArgumentList.Add("1");
        startInfo.ArgumentList.Add("--dump-json");
        startInfo.ArgumentList.Add(url);

        try
        {
            using Process process = new();
            process.StartInfo = startInfo;
            process.Start();

            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync(linked.Token);
            Task<string> stderrTask = process.StandardError.ReadToEndAsync(linked.Token);

            await process.WaitForExitAsync(linked.Token);

            string stdout = await stdoutTask;
            string stderr = await stderrTask;

            if (process.ExitCode == 0)
            {
                return ParseFirstLine(stdout, url);
            }

            logger.LogWarning("yt-dlp failed for {Url}. Exit {Code}. {Error}", url, process.ExitCode,
                stderr.Trim());

            return null;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Resolving {Url} timed out or was cancelled", url);
            return null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Could not run yt-dlp for {url}", url);
            return null;
        }
    }

    private ChannelInfo? ParseFirstLine(string stdout, string url)
    {
        string firstLine = stdout.Split('\n')[0].Trim();

        if (string.IsNullOrWhiteSpace(firstLine))
        {
            logger.LogWarning("yt-dlp returned nothing for {Url}", url);
            return null;
        }

        using JsonDocument doc = JsonDocument.Parse(firstLine);
        JsonElement root = doc.RootElement;

        string? id = GetString(root, "playlist_channel_id");
        string? name = GetString(root, "playlist_channel");
        string? handle = GetString(root, "playlist_uploader_id");

        if (!string.IsNullOrWhiteSpace(id))
        {
            return new ChannelInfo(id, name ?? id, handle);
        }

        logger.LogWarning("No channel id in yt-dlp output for {Url}", url);
        return null;
    }

    private static string? GetString(JsonElement element, string property) =>
        element.TryGetProperty(property, out JsonElement value) ? value.GetString() : null;
}