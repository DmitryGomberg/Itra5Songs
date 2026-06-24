using System.Text.Json;
using SongShowcase.Models.Generation;

namespace SongShowcase.Services;
public class LocalizationService
{
    private readonly Dictionary<string, LocaleDictionary> _cache = new();
    private readonly string _dataPath;

    public LocalizationService(IWebHostEnvironment env)
    {
        _dataPath = Path.Combine(env.ContentRootPath, "Data", "Localization");
        LoadAll();
    }

    private void LoadAll()
    {
        if (!Directory.Exists(_dataPath)) return;

        foreach (var file in Directory.GetFiles(_dataPath, "*.json"))
        {
            var json = File.ReadAllText(file);
            var dict = JsonSerializer.Deserialize<LocaleDictionary>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (dict != null)
                _cache[dict.Locale] = dict;
        }
    }

    public LocaleDictionary Get(string locale)
    {
        if (_cache.TryGetValue(locale, out var dict))
            return dict;

        return _cache.Values.First();
    }

    public IEnumerable<(string Locale, string DisplayName)> GetAvailable() =>
        _cache.Values
              .OrderBy(d => d.Locale)
              .Select(d => (d.Locale, d.DisplayName));
}
