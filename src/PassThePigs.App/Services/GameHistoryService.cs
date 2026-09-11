using System.Text.Json;

namespace PassThePigs.App.Services;

/// <summary>
/// Persists finished-game records to a small JSON file in app storage, for the
/// Statistics page. No database dependency - this is a handful of KB even after
/// hundreds of games.
/// </summary>
public static class GameHistoryService
{
    private const string FileName = "game_history.json";

    private static List<GameRecord>? _cache;

    private static string FilePath => Path.Combine(FileSystem.AppDataDirectory, FileName);

    /// <summary>All recorded games, oldest first. Loaded once and cached.</summary>
    public static IReadOnlyList<GameRecord> All => _cache ??= Load();

    /// <summary>Appends a finished game and persists it immediately.</summary>
    public static void RecordGame(GameRecord record)
    {
        var list = new List<GameRecord>(All) { record };
        _cache = list;
        Save(list);
    }

    /// <summary>Permanently deletes all recorded games.</summary>
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

    private static List<GameRecord> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return [];
            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<GameRecord>>(json) ?? [];
        }
        catch
        {
            // Missing or corrupt file - start fresh rather than crash the app over stats.
            return [];
        }
    }

    private static void Save(List<GameRecord> list)
    {
        try
        {
            File.WriteAllText(FilePath, JsonSerializer.Serialize(list));
        }
        catch
        {
            // Best-effort: worst case this game's result doesn't make it into the stats.
        }
    }
}
