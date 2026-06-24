using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace SongShowcase.Services;

/// <summary>
/// Генерирует MIDI с тремя треками: мелодия, аккорды, бас.
/// Используется музыкальная теория: тональность, аккордовые прогрессии,
/// диатонические ноты — результат звучит связно, а не как случайный шум.
/// </summary>
public class MidiGeneratorService
{
    private const int TicksPerQuarter = 480;

    private static readonly int[][] ChordProgressions =
    [
        [0, 5, 7, 5],
        [0, 9, 5, 7],
        [0, 7, 9, 5],
        [9, 5, 0, 7],
        [0, 5, 9, 7],
        [0, 2, 5, 7],
    ];

    private static readonly int[] MajorScale = [0, 2, 4, 5, 7, 9, 11];
    private static readonly int[] MinorScale = [0, 2, 3, 5, 7, 8, 10];
    private static readonly int[] TriadIntervals = [0, 4, 7];
    private static readonly int[] MinorTriadIntervals = [0, 3, 7];

    public byte[] Generate(long seed)
    {
        var rng = new Random((int)(seed ^ (seed >> 32)));

        var isMinor = rng.NextDouble() < 0.4;
        var scale = isMinor ? MinorScale : MajorScale;
        var rootNote = rng.Next(48, 60);
        var bpm = rng.Next(72, 145);
        var bars = 8;
        var progressionIndex = rng.Next(ChordProgressions.Length);
        var progression = ChordProgressions[progressionIndex];
        var ticksPerBar = TicksPerQuarter * 4;

        var midiFile = new MidiFile();
        midiFile.TimeDivision = new TicksPerQuarterNoteTimeDivision(TicksPerQuarter);

        var tempoTrack = new TrackChunk(
            new SetTempoEvent((long)(60_000_000.0 / bpm))
        );
        midiFile.Chunks.Add(tempoTrack);

        var melodyTrack = new TrackChunk();
        using (var notes = melodyTrack.ManageNotes())
        {
            long time = 0;
            for (var bar = 0; bar < bars; bar++)
            {
                var chordRoot = rootNote + progression[bar % progression.Length];
                foreach (var n in GenerateMelodyBar(rng, scale, chordRoot, time, ticksPerBar))
                    notes.Objects.Add(n);
                time += ticksPerBar;
            }
        }
        midiFile.Chunks.Add(melodyTrack);

        var chordTrack = new TrackChunk();
        using (var notes = chordTrack.ManageNotes())
        {
            long time = 0;
            var intervals = isMinor ? MinorTriadIntervals : TriadIntervals;
            for (var bar = 0; bar < bars; bar++)
            {
                var chordRoot = rootNote - 12 + progression[bar % progression.Length];
                foreach (var interval in intervals)
                {
                    notes.Objects.Add(new Note((SevenBitNumber)Math.Clamp(chordRoot + interval, 24, 96))
                    {
                        Time = time,
                        Length = ticksPerBar - TicksPerQuarter / 2,
                        Velocity = (SevenBitNumber)55
                    });
                }
                time += ticksPerBar;
            }
        }
        midiFile.Chunks.Add(chordTrack);

        var bassTrack = new TrackChunk();
        using (var notes = bassTrack.ManageNotes())
        {
            long time = 0;
            for (var bar = 0; bar < bars; bar++)
            {
                var bassRoot = rootNote - 24 + progression[bar % progression.Length];
                foreach (var n in GenerateBassBar(rng, bassRoot, time, ticksPerBar))
                    notes.Objects.Add(n);
                time += ticksPerBar;
            }
        }
        midiFile.Chunks.Add(bassTrack);

        using var stream = new MemoryStream();
        midiFile.Write(stream);
        return stream.ToArray();
    }

    private static List<Note> GenerateMelodyBar(Random rng, int[] scale, int chordRoot, long startTime, long barLength)
    {
        var result = new List<Note>();
        var noteCount = 8;
        var noteLen = barLength / noteCount;
        var scalePosition = rng.Next(scale.Length);

        for (var i = 0; i < noteCount; i++)
        {
            var step = rng.NextDouble() < 0.7 ? rng.Next(-2, 3) : rng.Next(-4, 5);
            scalePosition = Math.Clamp(scalePosition + step, 0, scale.Length - 1);
            var midiNote = chordRoot + scale[scalePosition];

            while (midiNote > chordRoot + 14) midiNote -= 12;
            while (midiNote < chordRoot - 2) midiNote += 12;
            midiNote = Math.Clamp(midiNote, 36, 96);

            if (rng.NextDouble() < 0.1) { startTime += noteLen; continue; }

            var duration = rng.NextDouble() < 0.3 ? noteLen * 2 : noteLen;
            if (duration <= 0) duration = noteLen;

            result.Add(new Note((SevenBitNumber)midiNote)
            {
                Time = startTime,
                Length = duration,
                Velocity = (SevenBitNumber)rng.Next(72, 100)
            });

            startTime += noteLen;
        }
        return result;
    }

    private static List<Note> GenerateBassBar(Random rng, int bassRoot, long startTime, long barLength)
    {
        var result = new List<Note>();
        var pattern = rng.Next(3);

        switch (pattern)
        {
            case 0:
                result.Add(new Note((SevenBitNumber)Math.Clamp(bassRoot, 24, 60))
                {
                    Time = startTime,
                    Length = barLength - TicksPerQuarter / 4,
                    Velocity = (SevenBitNumber)70
                });
                break;

            case 1:
                result.Add(new Note((SevenBitNumber)Math.Clamp(bassRoot, 24, 60))
                {
                    Time = startTime,
                    Length = barLength / 2 - TicksPerQuarter / 4,
                    Velocity = (SevenBitNumber)70
                });
                result.Add(new Note((SevenBitNumber)Math.Clamp(bassRoot + 7, 24, 60))
                {
                    Time = startTime + barLength / 2,
                    Length = barLength / 2 - TicksPerQuarter / 4,
                    Velocity = (SevenBitNumber)65
                });
                break;

            case 2:
                var walkNotes = new[] { bassRoot, bassRoot + 4, bassRoot + 7, bassRoot + 4 };
                for (var i = 0; i < 4; i++)
                {
                    result.Add(new Note((SevenBitNumber)Math.Clamp(walkNotes[i], 24, 60))
                    {
                        Time = startTime + i * (barLength / 4),
                        Length = barLength / 4 - TicksPerQuarter / 8,
                        Velocity = (SevenBitNumber)(65 + rng.Next(-5, 6))
                    });
                }
                break;
        }

        return result;
    }
}
