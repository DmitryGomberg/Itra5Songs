using SongShowcase.Models.Generation;

namespace SongShowcase.Models;

/// <summary>
/// ViewModel главной страницы: содержит параметры тулбара, данные текущей страницы
/// и список доступных локалей для dropdown.
/// </summary>
public class SongTableViewModel
{
    // --- Параметры тулбара (отражают текущее состояние UI) ---
    public string Locale { get; set; } = "en-US";
    public long Seed { get; set; }
    public double AverageLikes { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // --- Данные ---
    public List<Song> Songs { get; set; } = new();

    // --- Для dropdown языка ---
    public List<(string Locale, string DisplayName)> AvailableLocales { get; set; } = new();

    // --- Вспомогательные свойства для пагинации ---
    /// <summary>Есть ли предыдущая страница.</summary>
    public bool HasPrevPage => Page > 1;

    /// <summary>Следующая страница всегда есть — данные бесконечны по ТЗ.</summary>
    public bool HasNextPage => true;
}
