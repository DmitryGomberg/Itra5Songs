using SongShowcase.Models.Generation;

namespace SongShowcase.Services;

public class SongGeneratorService
{
    private readonly LocalizationService _localization;

    private const double SingleProbability = 0.25;

    public SongGeneratorService(LocalizationService localization)
    {
        _localization = localization;
    }

    public List<Song> Generate(GenerationRequest request)
    {
        var dict = _localization.Get(request.Locale);

        var pageSeed = CombineSeedAndPage(request.Seed, request.Page);

        var pageRandom = new Random(pageSeed);

        var songs = new List<Song>();

        for (var i = 0; i < request.PageSize; i++)
        {
            var globalIndex = (request.Page - 1) * request.PageSize + i + 1;

            var songSeed = pageRandom.Next();
            var songRandom = new Random(songSeed);

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


    private static string GenerateTitle(Random rng, LocaleDictionary dict)
    {
        var template = Pick(rng, dict.TitleTemplates);

        var result = template
            .Replace("{adj}", Pick(rng, dict.Adjectives))
            .Replace("{noun}", Pick(rng, dict.Nouns));

        return result;
    }

    private static string GenerateArtist(Random rng, LocaleDictionary dict)
    {
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
        var prefix = rng.NextDouble() < 0.4 ? Pick(rng, dict.BandNamePrefixes) + " " : "";
        var noun = Pick(rng, dict.BandNameNouns);
        return $"{prefix}{noun}".Trim();
    }

    private static string? GenerateAlbum(Random rng, LocaleDictionary dict)
    {
        if (rng.NextDouble() < SingleProbability)
            return null;

        return Pick(rng, dict.AlbumWords);
    }

    private static string GenerateReview(Random rng, LocaleDictionary dict)
    {
        var count = rng.Next(2, 5);
        var sentences = dict.ReviewSentences.OrderBy(_ => rng.Next()).Take(count);
        return string.Join(" ", sentences);
    }
    private static int GenerateLikes(Random rng, double averageLikes)
    {
        if (averageLikes <= 0) return 0;

        var whole = (int)Math.Floor(averageLikes);
        var fraction = averageLikes - whole;

        var likes = whole;

        if (fraction > 0 && rng.NextDouble() < fraction)
            likes++;

        return likes;
    }

    private static T Pick<T>(Random rng, List<T> list)
    {
        if (list.Count == 0)
            throw new InvalidOperationException($"Cannot pick from an empty list of {typeof(T).Name}.");
        return list[rng.Next(list.Count)];
    }

    private static int CombineSeedAndPage(long seed, int page)
    {
        unchecked
        {
            return (int)(seed * 1664525L + page * 1013904223L);
        }
    }
}
