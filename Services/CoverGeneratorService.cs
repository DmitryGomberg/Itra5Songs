using System.Text;

namespace SongShowcase.Services;

public class CoverGeneratorService
{
    private static readonly (string From, string To)[] ColorPalettes =
    [
        ("#0f2027", "#2c5364"), 
        ("#1a1a2e", "#e94560"), 
        ("#0d0d0d", "#4ecdc4"), 
        ("#16213e", "#a8edea"),
        ("#1f1c2c", "#928dab"),
        ("#0f3460", "#533483"),
        ("#1a472a", "#a3e635"),
        ("#2d1b69", "#11998e"),
        ("#1e3c72", "#2a5298"),
        ("#4a0404", "#c0392b"),
        ("#0c3547", "#f7971e"),
        ("#1a0533", "#7b2ff7"),
    ];

    public string Generate(string title, string artist, int seed)
    {
        var rng = new Random(seed);

        var size = 300;
        var palette = ColorPalettes[rng.Next(ColorPalettes.Length)];

        var angle = rng.Next(4) * 45;

        var (gx1, gy1, gx2, gy2) = AngleToGradientCoords(angle);

        var sb = new StringBuilder();
        sb.AppendLine($"""<svg xmlns="http://www.w3.org/2000/svg" width="{size}" height="{size}" viewBox="0 0 {size} {size}">""");
        sb.AppendLine($"""  <defs>""");
        sb.AppendLine($"""    <linearGradient id="bg" x1="{gx1}" y1="{gy1}" x2="{gx2}" y2="{gy2}">""");
        sb.AppendLine($"""      <stop offset="0%" stop-color="{palette.From}"/>""");
        sb.AppendLine($"""      <stop offset="100%" stop-color="{palette.To}"/>""");
        sb.AppendLine($"""    </linearGradient>""");
        sb.AppendLine($"""    <clipPath id="clip"><rect width="{size}" height="{size}"/></clipPath>""");
        sb.AppendLine($"""  </defs>""");

        sb.AppendLine($"""  <rect width="{size}" height="{size}" fill="url(#bg)"/>""");

        sb.AppendLine("""  <g clip-path="url(#clip)" opacity="0.35">""");
        AppendShapes(sb, rng, size, palette.To);
        sb.AppendLine("""  </g>""");

        sb.AppendLine($"""  <rect x="0" y="{size - 100}" width="{size}" height="100" fill="rgba(0,0,0,0.55)"/>""");

        var safeTitle = EscapeXml(Truncate(title, 22));
        var safeArtist = EscapeXml(Truncate(artist, 26));

        sb.AppendLine($"""  <text x="14" y="{size - 52}" font-family="Arial, sans-serif" font-size="16" font-weight="bold" fill="white" dominant-baseline="middle">{safeTitle}</text>""");
        sb.AppendLine($"""  <text x="14" y="{size - 26}" font-family="Arial, sans-serif" font-size="13" fill="rgba(255,255,255,0.75)" dominant-baseline="middle">{safeArtist}</text>""");

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    private static void AppendShapes(StringBuilder sb, Random rng, int size, string accentColor)
    {
        var shapeCount = rng.Next(3, 7);

        for (var i = 0; i < shapeCount; i++)
        {
            var shapeType = rng.Next(3);
            var opacity = (rng.Next(30, 80) / 100.0).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            var color = rng.NextDouble() < 0.4 ? accentColor : "white";

            switch (shapeType)
            {
                case 0:
                    var cx = rng.Next(-50, size + 50);
                    var cy = rng.Next(-50, size + 50);
                    var r = rng.Next(30, 160);
                    sb.AppendLine($"""    <circle cx="{cx}" cy="{cy}" r="{r}" fill="none" stroke="{color}" stroke-width="{rng.Next(1, 4)}" opacity="{opacity}"/>""");
                    break;

                case 1:
                    var x1 = rng.Next(0, size);
                    var y1 = rng.Next(0, size);
                    var x2 = rng.Next(0, size);
                    var y2 = rng.Next(0, size);
                    sb.AppendLine($"""    <line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{color}" stroke-width="{rng.Next(1, 3)}" opacity="{opacity}"/>""");
                    break;

                case 2:
                    var rx = rng.Next(-20, size);
                    var ry = rng.Next(-20, size);
                    var rw = rng.Next(40, 200);
                    var rh = rng.Next(40, 200);
                    var rotate = rng.Next(0, 45);
                    sb.AppendLine($"""    <rect x="{rx}" y="{ry}" width="{rw}" height="{rh}" fill="none" stroke="{color}" stroke-width="{rng.Next(1, 3)}" opacity="{opacity}" transform="rotate({rotate},{rx + rw / 2},{ry + rh / 2})"/>""");
                    break;
            }
        }
    }

    private static (string, string, string, string) AngleToGradientCoords(int angle) => angle switch
    {
        45  => ("0", "1", "1", "0"),
        90  => ("0", "0", "1", "0"),
        135 => ("0", "0", "1", "1"),
        _   => ("0", "0", "0", "1"),
    };

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";

    private static string EscapeXml(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
