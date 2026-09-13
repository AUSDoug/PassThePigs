using System.Text.Json;

namespace PassThePigs.App.Services;

/// <summary>
/// Persists every human roll-or-pass decision to its own JSON file, separate from
/// <see cref="GameHistoryService"/> - it grows much faster (many decisions per game
/// instead of one record), so it gets its own file and lifecycle even though the
/// storage pattern is identical.
/// </summary>
public static class DecisionHistoryService
{
    private const string FileName = "decision_history.json";

    private static List<DecisionRecord>? _cache;

    private static string FilePath => Path.Combine(FileSystem.AppDataDirectory, FileName);

    /// <summary>All logged decisions, oldest first. Loaded once and cached.</summary>
    public static IReadOnlyList<DecisionRecord> All => _cache ??= Load();

    /// <summary>Appends one decision and persists it immediately.</summary>
    public static void RecordDecision(DecisionRecord record)
    {
        var list = new List<DecisionRecord>(All) { record };
        _cache = list;
        Save(list);
    }

    /// <summary>Permanently deletes all logged decisions.</summary>
    public static void ClearAll()
    {
        _cache = [];
        try
        {
            if (File.Exists(FilePath)) File.Delete(FilePath);
        }
        catch
        {
            // Best-effort - the in-memory cache is already cleared either way.
        }
    }

    private static List<DecisionRecord> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return [];
            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<DecisionRecord>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static void Save(List<DecisionRecord> list)
    {
        try
        {
            File.WriteAllText(FilePath, JsonSerializer.Serialize(list));
        }
        catch
        {
            // Best-effort; worst case this decision doesn't make it into play-style stats.
        }
    }
}
