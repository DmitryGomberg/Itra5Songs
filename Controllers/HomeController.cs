using Microsoft.AspNetCore.Mvc;
using SongShowcase.Models;
using SongShowcase.Models.Generation;
using SongShowcase.Services;

namespace SongShowcase.Controllers;

public class HomeController : Controller
{
    private readonly SongGeneratorService _generator;
    private readonly LocalizationService _localization;
    private readonly CoverGeneratorService _coverGenerator;
    private readonly MidiGeneratorService _midiGenerator;

    public HomeController(
        SongGeneratorService generator,
        LocalizationService localization,
        CoverGeneratorService coverGenerator,
        MidiGeneratorService midiGenerator)
    {
        _generator = generator;
        _localization = localization;
        _coverGenerator = coverGenerator;
        _midiGenerator = midiGenerator;
    }

    public IActionResult Index(
        string locale = "en-US",
        long seed = 0,
        double averageLikes = 0,
        int page = 1,
        int pageSize = 10)
    {
        if (seed == 0)
            seed = new Random().NextInt64(1, long.MaxValue);

        var request = new GenerationRequest
        {
            Locale = locale,
            Seed = seed,
            Page = page,
            PageSize = pageSize,
            AverageLikes = averageLikes
        };

        var songs = _generator.Generate(request);

        foreach (var song in songs)
        {
            song.CoverImageUrl = $"/Home/Cover?title={Uri.EscapeDataString(song.Album ?? "Single")}&artist={Uri.EscapeDataString(song.Artist)}&seed={song.Index + seed}";
            song.AudioUrl = $"/Home/Audio?seed={song.Index + seed}";
        }

        var vm = new SongTableViewModel
        {
            Locale = locale,
            Seed = seed,
            AverageLikes = averageLikes,
            Page = page,
            PageSize = pageSize,
            Songs = songs,
            AvailableLocales = _localization.GetAvailable().ToList()
        };

        return View(vm);
    }

    [HttpGet]
    [ResponseCache(Duration = 3600)]
    public IActionResult Cover(string title, string artist, int seed)
    {
        var svg = _coverGenerator.Generate(title, artist, seed);
        return Content(svg, "image/svg+xml");
    }

    [HttpGet]
    [ResponseCache(Duration = 3600)]
    public IActionResult Audio(long seed)
    {
        var midiBytes = _midiGenerator.Generate(seed);
        return File(midiBytes, "audio/midi");
    }

    [HttpGet]
    public IActionResult GalleryPage(string locale, long seed, double averageLikes, int page, int pageSize = 12)
    {
        var request = new GenerationRequest
        {
            Locale = locale,
            Seed = seed,
            Page = page,
            PageSize = pageSize,
            AverageLikes = averageLikes
        };

        var songs = _generator.Generate(request);

        foreach (var song in songs)
        {
            song.CoverImageUrl = $"/Home/Cover?title={Uri.EscapeDataString(song.Album ?? "Single")}&artist={Uri.EscapeDataString(song.Artist)}&seed={song.Index + seed}";
            song.AudioUrl = $"/Home/Audio?seed={song.Index + seed}";
        }

        return Json(songs);
    }
}
