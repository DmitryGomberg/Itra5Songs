namespace SongShowcase.Models.Generation;

public class LocaleDictionary
{
    public string Locale { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public List<string> Adjectives { get; set; } = new();
    public List<string> Nouns { get; set; } = new();
    public List<string> TitleTemplates { get; set; } = new();
    public List<string> FirstNames { get; set; } = new();
    public List<string> LastNames { get; set; } = new();
    public List<string> BandNamePrefixes { get; set; } = new(); 
    public List<string> BandNameNouns { get; set; } = new();   
    public List<string> AlbumWords { get; set; } = new();
    public List<string> Genres { get; set; } = new();
    public List<string> ReviewSentences { get; set; } = new();
}
