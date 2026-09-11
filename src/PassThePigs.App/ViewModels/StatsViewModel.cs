using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassThePigs.App.Services;
using PassThePigs.Core.Ai;

namespace PassThePigs.App.ViewModels;

/// <summary>
/// Stats computed from <see cref="GameHistoryService"/>. Recomputed on open and again
/// after a reset, so the properties are observable rather than fixed at construction.
/// </summary>
public partial class StatsViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoHistory))]
    private bool _hasHistory;

    public bool NoHistory => !HasHistory;

    [ObservableProperty] private int _gamesPlayed;
    [ObservableProperty] private string _winLossRecord = "—";
    [ObservableProperty] private string _winPercentage = "—";

    [ObservableProperty] private string _currentStreakLabel = "No current streak";
    [ObservableProperty] private string _bestWinStreakLabel = "—";
    [ObservableProperty] private string _worstLoseStreakLabel = "—";

    [ObservableProperty] private string _mostCommonOpponent = "—";

    [ObservableProperty] private string _bestAgainstName = "—";
    [ObservableProperty] private string _bestAgainstDetail = "";
    [ObservableProperty] private string _worstAgainstName = "—";
    [ObservableProperty] private string _worstAgainstDetail = "";

    [ObservableProperty] private string _fastestWin = "—";

    public StatsViewModel() => Load();

    private void Load()
    {
        var games = GameHistoryService.All;
        GamesPlayed = games.Count;
        HasHistory = GamesPlayed > 0;
        if (!HasHistory)
        {
            WinLossRecord = WinPercentage = MostCommonOpponent = BestAgainstName
                = WorstAgainstName = BestWinStreakLabel = WorstLoseStreakLabel = FastestWin = "—";
            CurrentStreakLabel = "No current streak";
            BestAgainstDetail = WorstAgainstDetail = "";
            return;
        }

        int wins = games.Count(g => g.Won);
        int losses = GamesPlayed - wins;
        WinLossRecord = $"{wins}–{losses}";
        WinPercentage = $"{Math.Round(100.0 * wins / GamesPlayed)}%";

        (int streak, bool streakIsWin) = CurrentStreak(games);
        CurrentStreakLabel = streak == 0 ? "No current streak"
            : streakIsWin ? $"{streak}-game win streak"
            : $"{streak}-game losing streak";

        BestWinStreakLabel = Games(LongestStreak(games, won: true));
        WorstLoseStreakLabel = Games(LongestStreak(games, won: false));

        var byOpponent = games.GroupBy(g => g.OpponentId).ToList();
        MostCommonOpponent = NameFor(byOpponent.OrderByDescending(g => g.Count()).First().Key);

        var winRates = byOpponent
            .Select(g => (Id: g.Key, Games: g.Count(), WinRate: g.Count(r => r.Won) / (double)g.Count()))
            .ToList();
        var best = winRates.OrderByDescending(w => w.WinRate).ThenByDescending(w => w.Games).First();
        var worst = winRates.OrderBy(w => w.WinRate).ThenByDescending(w => w.Games).First();
        BestAgainstName = NameFor(best.Id);
        BestAgainstDetail = Detail(best.WinRate, best.Games);
        WorstAgainstName = NameFor(worst.Id);
        WorstAgainstDetail = Detail(worst.WinRate, worst.Games);

        GameRecord? fastest = games.Where(g => g.Won).OrderBy(g => g.Rounds).FirstOrDefault();
        FastestWin = fastest is null ? "—" : $"{Rounds(fastest.Rounds)} vs {NameFor(fastest.OpponentId)}";
    }

    [RelayCommand]
    private async Task ResetStatisticsAsync()
    {
        bool confirmed = await Shell.Current.DisplayAlert(
            "Reset statistics?",
            "This permanently deletes your game history. This can't be undone.",
            "Reset", "Cancel");
        if (!confirmed) return;

        GameHistoryService.ClearAll();
        Load();
    }

    [RelayCommand]
    private static Task DoneAsync() => Shell.Current.GoToAsync("..");

    /// <summary>Length and direction of the run at the end of the (chronological) list.</summary>
    private static (int length, bool isWin) CurrentStreak(IReadOnlyList<GameRecord> games)
    {
        bool last = games[^1].Won;
        int n = 0;
        for (int i = games.Count - 1; i >= 0 && games[i].Won == last; i--) n++;
        return (n, last);
    }

    private static int LongestStreak(IReadOnlyList<GameRecord> games, bool won)
    {
        int best = 0, run = 0;
        foreach (var g in games)
        {
            run = g.Won == won ? run + 1 : 0;
            best = Math.Max(best, run);
        }
        return best;
    }

    private static string NameFor(int id) =>
        Strategies.Names[Math.Clamp(id, 0, Strategies.Names.Length - 1)];

    private static string Games(int n) => $"{n} game{(n == 1 ? "" : "s")}";
    private static string Rounds(int n) => $"{n} round{(n == 1 ? "" : "s")}";

    private static string Detail(double winRate, int games) =>
        $"{Math.Round(winRate * 100)}% win rate · {Games(games)}";
}
