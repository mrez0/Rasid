using System.ComponentModel.DataAnnotations;

namespace Rasid.Core.Models;

public class Channel
{
    [Key]
    [MaxLength(64)]
    public string Id { get; set; } = "";

    [MaxLength(100)]
    public string? Handle { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = "";

    [MaxLength(200)]
    public string FolderName { get; set; } = "";

    public DateTime AddedUtc { get; set; }
    public DateTime? LastCheckedUtc { get; set; }
    public DateTime? LastRssPublishedUtc { get; set; }
    public int? KeepCount { get; set; }
    public bool IsEnabled { get; set; } = true;
    public List<Video> Videos { get; set; } = [];
}