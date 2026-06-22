namespace SongShowcase.Models.Generation;

/// <summary>
/// Структура одного JSON-словаря локализации (например, en-US.json).
/// Все слова, из которых "собираются" названия песен/альбомов, артисты и жанры,
/// живут только здесь — в коде никаких регион-специфичных строк по требованию ТЗ.
/// </summary>
public class LocaleDictionary
{
    /// <summary>Код локали, должен совпадать с именем файла без расширения (напр. "en-US").</summary>
    public string Locale { get; set; } = string.Empty;

    /// <summary>Человекочитаемое название языка для UI (напр. "English (US)").</summary>
    public string DisplayName { get; set; } = string.Empty;

    // --- Строительные блоки для генерации названий песен ---
    // Названия собираем по шаблонам вида "{Adjective} {Noun}", поэтому нужны отдельные списки.
    public List<string> Adjectives { get; set; } = new();
    public List<string> Nouns { get; set; } = new();
    public List<string> TitleTemplates { get; set; } = new(); // напр. "{adj} {noun}", "{noun} of {noun}"

    // --- Артисты ---
    public List<string> FirstNames { get; set; } = new();
    public List<string> LastNames { get; set; } = new();
    public List<string> BandNamePrefixes { get; set; } = new(); // напр. "The", "Sgt."
    public List<string> BandNameNouns { get; set; } = new();    // напр. "Wolves", "Echoes"

    // --- Альбомы и жанры ---
    public List<string> AlbumWords { get; set; } = new();
    public List<string> Genres { get; set; } = new();

    // --- Отзывы (для деталей строки) ---
    public List<string> ReviewSentences { get; set; } = new();
}
