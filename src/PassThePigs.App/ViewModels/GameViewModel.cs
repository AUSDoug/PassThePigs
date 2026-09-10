using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassThePigs.App.Services;
using PassThePigs.Core;
using PassThePigs.Core.Ai;

namespace PassThePigs.App.ViewModels;

public partial class GameViewModel : ObservableObject
{
    private const int HumanIndex = 0;

    private readonly Random _rng = new();
    private PigGame _game = null!;
    private IRollStrategy _opponent = null!;

    // The opponent's real name; only shown once "Random" mode reaches game over.
    private string _cpuRealName = "";

    [ObservableProperty] private int _roundNumber;
    [ObservableProperty] private int _targetScore;
    [ObservableProperty] private string _cpuName = "CPU";
    [ObservableProperty] private int _youTotal;
    [ObservableProperty] private int _cpuTotal;
    [ObservableProperty] private string _caption = "";
    [ObservableProperty] private string _resultText = "";

    /// <summary>Raised when a fresh game starts, so the page can clear the last render.</summary>
    public event Action? NewGame;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowHint))]
    private bool _hasRolled;

    /// <summary>Show the "swipe / tap to roll" hint until the first roll of the game.</summary>
    public bool ShowHint => !HasRolled;

    /// <summary>Raised for each roll so the page can play it in the 3D stage.</summary>
    public event Action<PigRoll>? RollShown;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(YouTurnLabel), nameof(CpuTurnLabel), nameof(YouCardStroke), nameof(CpuCardStroke))]
    [NotifyCanExecuteChangedFor(nameof(RollCommand), nameof(PassCommand))]
    private bool _isYourTurn;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(YouCardStroke), nameof(CpuCardStroke))]
    [NotifyCanExecuteChangedFor(nameof(RollCommand), nameof(PassCommand))]
    private bool _isGameOver;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RollCommand), nameof(PassCommand))]
    private bool _busy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(YouTurnLabel), nameof(CpuTurnLabel))]
    [NotifyCanExecuteChangedFor(nameof(PassCommand))]
    private int _turnScore;

    public string YouTurnLabel => IsYourTurn && TurnScore > 0 ? $"+{TurnScore}" : string.Empty;
    public string CpuTurnLabel => !IsYourTurn && TurnScore > 0 ? $"+{TurnScore}" : string.Empty;

    private static readonly Color Active = Color.FromArgb("#C2557A");
    private static readonly Color Inactive = Colors.Transparent;
    public Color YouCardStroke => IsYourTurn && !IsGameOver ? Active : Inactive;
    public Color CpuCardStroke => !IsYourTurn && !IsGameOver ? Active : Inactive;

    private bool CanRoll() => IsYourTurn && !IsGameOver && !Busy;
    private bool CanPass() => IsYourTurn && !IsGameOver && !Busy && TurnScore > 0;

    /// <summary>Begin a fresh game from the current settings. Call when the page appears.</summary>
    public void Start()
    {
        int aiId = GameSettings.RandomOpponent
            ? _rng.Next(Strategies.Names.Length)
            : GameSettings.OpponentAi;
        _opponent = Strategies.ById(aiId, GameSettings.ExactWin);
        _cpuRealName = Strategies.Names[aiId];
        CpuName = GameSettings.RandomOpponent ? "Opponent" : _cpuRealName;

        int start = GameSettings.FirstTurn switch
        {
            FirstTurn.Human => HumanIndex,
            FirstTurn.Ai => 1,
            _ => _rng.Next(2),
        };
        _game = new PigGame("You", _cpuRealName, GameSettings.WinScore, _rng, start, GameSettings.ExactWin);
        TargetScore = _game.WinScore;

        IsGameOver = false;
        ResultText = string.Empty;
        HasRolled = false;
        Caption = "Tap Roll to start your turn.";
        NewGame?.Invoke();
        Sync();

        if (!IsYourTurn)
            _ = RunOpponentTurnAsync();
    }

    [RelayCommand(CanExecute = nameof(CanRoll))]
    private async Task RollAsync()
    {
        PigRoll roll = _game.Roll();
        ShowRoll("You", roll);
        if (_game.ExactWin && !_game.IsOver && _game.ActiveIndex == HumanIndex
            && _game.Active.ProjectedTotal > _game.WinScore)
            Caption += $"  (over {_game.WinScore} - pass to try again next turn)";
        Sync();
        if (!IsYourTurn && !IsGameOver)
            await RunOpponentTurnAsync();
    }

    [RelayCommand(CanExecute = nameof(CanPass))]
    private async Task PassAsync()
    {
        int projected = _game.Active.ProjectedTotal;
        _game.Pass();
        Caption = _game.ExactWin && projected > _game.WinScore && !_game.IsOver
            ? $"Over {_game.WinScore} - you lose the turn."
            : "You hold.";
        Sync();
        if (!IsYourTurn && !IsGameOver)
            await RunOpponentTurnAsync();
    }

    [RelayCommand]
    private void PlayAgain() => Start();

    private async Task RunOpponentTurnAsync()
    {
        Busy = true;
        int delay = GameSettings.AiRollDelayMs;
        bool animate = delay > 0;   // 0 = play the whole turn instantly, no 3D render
        await Task.Delay(animate ? 500 : 0);

        while (!_game.IsOver && _game.ActiveIndex != HumanIndex)
        {
            RollDecision d = _opponent.Decide(_game.ActiveView, _rng);
            if (d.Roll)
            {
                PigRoll roll = _game.Roll();
                ShowRoll(CpuName, roll, animate);
                Sync();
                if (animate) await Task.Delay(delay);
            }
            else
            {
                int banked = _game.Active.TurnScore;
                int projected = _game.Active.ProjectedTotal;
                _game.Pass();
                Caption = _game.ExactWin && projected > _game.WinScore && !_game.IsOver
                    ? $"{CpuName} overshot {_game.WinScore}."
                    : $"{CpuName} holds on {banked}.";
                Sync();
                break;
            }
        }

        Busy = false;
        Sync();
    }

    private void ShowRoll(string who, PigRoll roll, bool render = true)
    {
        HasRolled = true;
        if (render) RollShown?.Invoke(roll);
        Caption = roll.IsPigOut
            ? $"{who}: {Nice(roll.One)} + {Nice(roll.Two)}  →  PIG OUT"
            : $"{who}: {Nice(roll.One)} + {Nice(roll.Two)}  →  {roll.Score}";
    }

    private void Sync()
    {
        RoundNumber = _game.RoundNumber;
        YouTotal = _game.Players[0].TotalScore;
        CpuTotal = _game.Players[1].TotalScore;
        IsYourTurn = _game.ActiveIndex == HumanIndex && !_game.IsOver;
        TurnScore = _game.Active.TurnScore;

        if (_game.IsOver && !IsGameOver)
        {
            IsGameOver = true;
            CpuName = _cpuRealName;   // reveal the "Random" opponent now the game's done
            ResultText = _game.WinnerIndex == HumanIndex
                ? $"You beat {_cpuRealName}! 🎉"
                : $"{_cpuRealName} wins.";
        }
    }

    private static string Nice(PigPosition p) => p switch
    {
        PigPosition.SideNoDot => "Sider",
        PigPosition.SideDot => "Sider (dot)",
        PigPosition.LeaningJowler => "Leaning Jowler",
        _ => p.ToString()
    };
}
