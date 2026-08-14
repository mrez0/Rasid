using System.ComponentModel.DataAnnotations;

namespace Rasid.Core.Models;

public class Video
{
    [Key]
    [MaxLength(32)]
    public string Id { get; set; } = "";

    [MaxLength(64)]
    public string ChannelId { get; set; } = "";

    [MaxLength(500)]
    public string Title { get; set; } = "";

    public DateTime PublishedUtc { get; set; }
    public double? DurationSeconds { get; set; }
    public VideoStatus Status { get; set; } = VideoStatus.Pending;
    public string? FilePath { get; set; }
    public long? FileSizeBytes { get; set; }
    public DateTime? DownloadedUtc { get; set; }
    public DateTime? DeletedUtc { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
    public Channel? Channel { get; set; }
}