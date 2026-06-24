using SongShowcase.Models.Generation;

namespace SongShowcase.Services;

/// <summary>
/// Генерирует список песен по параметрам запроса (locale, seed, page, likes).
///
/// Ключевые принципы (из требований ТЗ и подсказок преподавателя):
///
/// 1. ВОСПРОИЗВОДИМОСТЬ: тот же seed + page + locale → те же данные всегда.
///
/// 2. ВЛОЖЕННЫЕ ГЕНЕРАТОРЫ: каждая песня получает свой Random, засеянный от
///    "родительского" генератора страницы. Это гарантирует, что:
///    — изменение числа лайков не меняет тексты (лайки сидируются отдельно)
///    — добавление полей к одной записи не ломает соседние
///
/// 3. НЕЗАВИСИМОСТЬ ПАРАМЕТРОВ: Likes зависят только от seed+index, но не от
///    порядка генерации текстовых полей (отдельная ветка Random).
///
/// 4. ДРОБНЫЕ ЛАЙКИ: "4.7 лайка" = всегда 4, плюс 5-й с вероятностью 0.7.
/// </summary>
public class SongGeneratorService
{
    private readonly LocalizationService _localization;

    // Вероятность того, что песня будет синглом, а не частью альбома.
    private const double SingleProbability = 0.25;

    public SongGeneratorService(LocalizationService localization)
    {
        _localization = localization;
    }

    /// <summary>Генерирует одну страницу песен.</summary>
    public List<Song> Generate(GenerationRequest request)
    {
        var dict = _localization.Get(request.Locale);

        // Комбинируем seed пользователя и номер страницы (MAD-операция как в ТЗ).
        // Умножение + сложение даёт разные биты для разных страниц при одном seed.
        var pageSeed = CombineSeedAndPage(request.Seed, request.Page);

        // Родительский генератор для этой страницы — управляет порядком записей.
        var pageRandom = new Random(pageSeed);

        var songs = new List<Song>();

        for (var i = 0; i < request.PageSize; i++)
        {
            // Глобальный индекс записи (1, 2, 3, ... не сбрасывается при смене страницы).
            var globalIndex = (request.Page - 1) * request.PageSize + i + 1;

            // Каждая песня получает свой Random, засеянный от родительского.
            // Это ключевой паттерн из description.txt:
            // var songRandom = new Random(pageRandom.Next());
            // Теперь любые изменения соседних записей не влияют на эту.
            var songSeed = pageRandom.Next();
            var songRandom = new Random(songSeed);

            // Отдельный генератор для лайков — засеян тоже от pageRandom,
            // но независим от songRandom. Меняем AverageLikes — меняются только лайки.
            var likesSeed = pageRandom.Next();
            var likesRandom = new Random(likesSeed);

            var song = new Song
            {
                Index = globalIndex,
                Title = GenerateTitle(songRandom, dict),
                Artist = GenerateArtist(songRandom, dict),
                Album = GenerateAlbum(songRandom, dict),
                Genre = Pick(songRandom, dict.Genres),
                Likes = GenerateLikes(likesRandom, request.AverageLikes),
                ReviewText = GenerateReview(songRandom, dict)
            };

            songs.Add(song);
        }

        return songs;
    }

    // --- Генерация отдельных полей ---

    private static string GenerateTitle(Random rng, LocaleDictionary dict)
    {
        var template = Pick(rng, dict.TitleTemplates);

        // Заменяем плейсхолдеры {adj} и {noun} случайными словами из словаря.
        var result = template
            .Replace("{adj}", Pick(rng, dict.Adjectives))
            .Replace("{noun}", Pick(rng, dict.Nouns));

        return result;
    }

    private static string GenerateArtist(Random rng, LocaleDictionary dict)
    {
        // С вероятностью 50% генерируем имя человека, иначе — название группы.
        return rng.NextDouble() < 0.5
            ? GeneratePersonName(rng, dict)
            : GenerateBandName(rng, dict);
    }

    private static string GeneratePersonName(Random rng, LocaleDictionary dict)
    {
        var first = Pick(rng, dict.FirstNames);
        var last = Pick(rng, dict.LastNames);
        return $"{first} {last}";
    }

    private static string GenerateBandName(Random rng, LocaleDictionary dict)
    {
        // С вероятностью 40% добавляем префикс ("The", "Dr." и т.д.).
        var prefix = rng.NextDouble() < 0.4 ? Pick(rng, dict.BandNamePrefixes) + " " : "";
        var noun = Pick(rng, dict.BandNameNouns);
        return $"{prefix}{noun}".Trim();
    }

    private static string? GenerateAlbum(Random rng, LocaleDictionary dict)
    {
        // Часть песен — синглы, у них альбома нет.
        if (rng.NextDouble() < SingleProbability)
            return null;

        return Pick(rng, dict.AlbumWords);
    }

    private static string GenerateReview(Random rng, LocaleDictionary dict)
    {
        // Случайное количество предложений (2–4) из словаря отзывов.
        var count = rng.Next(2, 5);
        var sentences = dict.ReviewSentences.OrderBy(_ => rng.Next()).Take(count);
        return string.Join(" ", sentences);
    }

    /// <summary>
    /// Реализация дробных лайков из подсказки преподавателя.
    /// times(4.7, ...) = всегда 4 раза, плюс 5-й с вероятностью 0.7.
    /// </summary>
    private static int GenerateLikes(Random rng, double averageLikes)
    {
        if (averageLikes <= 0) return 0;

        var whole = (int)Math.Floor(averageLikes);
        var fraction = averageLikes - whole;

        // Целая часть — гарантированные лайки.
        var likes = whole;

        // Дробная часть — вероятностный дополнительный лайк.
        if (fraction > 0 && rng.NextDouble() < fraction)
            likes++;

        return likes;
    }

    /// <summary>Picks a random element from a list using the provided RNG.</summary>
    private static T Pick<T>(Random rng, List<T> list)
    {
        if (list.Count == 0)
            throw new InvalidOperationException($"Cannot pick from an empty list of {typeof(T).Name}.");
        return list[rng.Next(list.Count)];
    }

    /// <summary>
    /// Комбинирует пользовательский seed и номер страницы.
    /// MAD (Multiply-Add) обеспечивает разные биты при разных page даже при малом seed.
    /// Unchecked позволяет переполнение без исключений (это нормально для seed).
    /// </summary>
    private static int CombineSeedAndPage(long seed, int page)
    {
        unchecked
        {
            return (int)(seed * 1664525L + page * 1013904223L);
        }
    }
}
