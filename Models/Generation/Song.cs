namespace SongShowcase.Models.Generation;

public class Song
{
    public int Index { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;

    public string? Album { get; set; }

    public string Genre { get; set; } = string.Empty;

    public int Likes { get; set; }

    public string? CoverImageUrl { get; set; }
    public string? ReviewText { get; set; }
    public string? AudioUrl { get; set; }
}
