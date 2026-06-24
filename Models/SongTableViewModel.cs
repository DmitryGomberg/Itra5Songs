using SongShowcase.Models.Generation;

namespace SongShowcase.Models;
public class SongTableViewModel
{
    public string Locale { get; set; } = "en-US";
    public long Seed { get; set; }
    public double AverageLikes { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string ViewMode { get; set; } = "table";
    public bool IsGallery => ViewMode.Equals("gallery", StringComparison.OrdinalIgnoreCase);
    public List<Song> Songs { get; set; } = new();
    public List<(string Locale, string DisplayName)> AvailableLocales { get; set; } = new();

    public bool HasPrevPage => Page > 1;

    public bool HasNextPage => true;
}
