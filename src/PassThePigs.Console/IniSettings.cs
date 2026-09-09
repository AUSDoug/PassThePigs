namespace PassThePigs.Console;

/// <summary>
/// The old Settings.ini, but with a portable parser instead of the Windows-only
/// kernel32 P/Invoke. Flat "Key=Value" lines under a single [Settings] section.
/// </summary>
internal sealed class IniSettings
{
    public int Games { get; set; } = 1;
    public int Cpu0Ai { get; set; }      // strategy id for player 1 (AI vs AI)
    public int Cpu1Ai { get; set; }      // strategy id for player 2 / the human's opponent
    public bool Verbose { get; set; }

    public static IniSettings Load(string path)
    {
        var s = new IniSettings();
        if (!File.Exists(path))
        {
            s.Save(path);
            return s;
        }

        foreach (string raw in File.ReadAllLines(path))
        {
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('[') || line.StartsWith(';') || line.StartsWith('#'))
                continue;
            int eq = line.IndexOf('=');
            if (eq < 0) continue;
            string key = line[..eq].Trim();
            string value = line[(eq + 1)..].Trim();

            switch (key.ToLowerInvariant())
            {
                case "games": if (int.TryParse(value, out int g)) s.Games = g; break;
                case "cpu 0 ai": if (int.TryParse(value, out int a0)) s.Cpu0Ai = a0; break;
                case "cpu 1 ai": if (int.TryParse(value, out int a1)) s.Cpu1Ai = a1; break;
                case "log mode": s.Verbose = value is "1" or "true" or "True"; break;
            }
        }
        return s;
    }

    public void Save(string path) => File.WriteAllText(path,
        $"[Settings]{Environment.NewLine}" +
        $"Games={Games}{Environment.NewLine}" +
        $"CPU 0 AI={Cpu0Ai}{Environment.NewLine}" +
        $"CPU 1 AI={Cpu1Ai}{Environment.NewLine}" +
        $"Log Mode={(Verbose ? 1 : 0)}{Environment.NewLine}");
}
