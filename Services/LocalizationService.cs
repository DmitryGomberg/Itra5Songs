using System.Text.Json;
using SongShowcase.Models.Generation;

namespace SongShowcase.Services;

/// <summary>
/// Загружает JSON-словари из Data/Localization/ и кэширует их в памяти.
/// Добавление нового языка = просто положить новый .json файл, код менять не нужно.
/// </summary>
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

    /// <summary>Возвращает словарь для локали, или en-US как fallback.</summary>
    public LocaleDictionary Get(string locale)
    {
        if (_cache.TryGetValue(locale, out var dict))
            return dict;

        // fallback на первый доступный
        return _cache.Values.First();
    }

    /// <summary>Список всех доступных локалей для UI (dropdown).</summary>
    public IEnumerable<(string Locale, string DisplayName)> GetAvailable() =>
        _cache.Values
              .OrderBy(d => d.Locale)
              .Select(d => (d.Locale, d.DisplayName));
}
