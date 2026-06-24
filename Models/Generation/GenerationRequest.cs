namespace SongShowcase.Models.Generation;

public class GenerationRequest
{    public string Locale { get; set; } = "en-US";

    public long Seed { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public double AverageLikes { get; set; }
}
