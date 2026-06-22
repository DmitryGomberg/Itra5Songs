namespace SongShowcase.Models.Generation;

/// <summary>
/// Параметры одного запроса на генерацию страницы данных.
/// Все поля независимы друг от друга по требованию ТЗ:
/// смена Likes не должна влиять на Title/Artist/Album/Genre, и наоборот.
/// </summary>
public class GenerationRequest
{
    /// <summary>Код локали, например "en-US" или "ru-RU". Должен совпадать с именем JSON-словаря.</summary>
    public string Locale { get; set; } = "en-US";

    /// <summary>
    /// Пользовательский 64-битный сид. Определяет результат генерации текстовых полей
    /// (Title/Artist/Album/Genre) и музыки. Likes сидируются от него же, но отдельной веткой.
    /// </summary>
    public long Seed { get; set; }

    /// <summary>Номер страницы, начиная с 1. Комбинируется с Seed для получения сида конкретной страницы.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Размер страницы (число записей).</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>Среднее число лайков на песню, 0–10, дробное. Влияет только на Likes.</summary>
    public double AverageLikes { get; set; }
}
