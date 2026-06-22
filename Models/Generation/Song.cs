namespace SongShowcase.Models.Generation;

/// <summary>
/// Полное представление одной сгенерированной песни — то, что увидит пользователь
/// и в строке таблицы, и в развёрнутых деталях.
/// </summary>
public class Song
{
    /// <summary>Сквозной порядковый номер (1, 2, 3, ...), а не индекс на странице.</summary>
    public int Index { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;

    /// <summary>Название альбома, либо null/empty, если это сингл (тогда показываем "Single").</summary>
    public string? Album { get; set; }

    public string Genre { get; set; } = string.Empty;

    /// <summary>Число лайков — всегда целое, даже если задано дробное среднее значение.</summary>
    public int Likes { get; set; }

    // Поля ниже понадобятся на следующих шагах (обложка, плеер, отзыв) —
    // заводим их сразу, чтобы не переписывать модель посреди пути.
    public string? CoverImageUrl { get; set; }
    public string? ReviewText { get; set; }
    public string? AudioUrl { get; set; }
}
