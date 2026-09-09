using PassThePigs.Core.Ai;

namespace PassThePigs.Core;

/// <summary>
/// A single two-player game of Pass the Pigs, played to <see cref="WinScore"/>.
///
/// Turn semantics match the original console game: the win is only decided when a
/// turn ends (a voluntary pass or a pig out), so a greedy player can roll past the
/// target and still bust.
/// </summary>
public sealed class PigGame
{
    private readonly Random _rng;
    private readonly PlayerState[] _players;

    public int WinScore { get; }
    public IReadOnlyList<PlayerState> Players => _players;

    /// <summary>Index (0 or 1) of the player whose turn it is.</summary>
    public int ActiveIndex { get; private set; }
    public PlayerState Active => _players[ActiveIndex];
    public PlayerState Opponent => _players[ActiveIndex ^ 1];

    /// <summary>Turns taken so far (both players), 1-based for the current turn.</summary>
    public int TurnNumber { get; private set; } = 1;

    public bool IsOver { get; private set; }
    public int WinnerIndex { get; private set; } = -1;
    public PlayerState? Winner => WinnerIndex < 0 ? null : _players[WinnerIndex];

    /// <summary>True while the active player may still roll or pass this turn.</summary>
    public bool ActiveCanContinue => !IsOver;

    public event Action<PlayerState, PigRoll>? Rolled;
    public event Action<PlayerState>? PiggedOut;
    public event Action<PlayerState>? Passed;
    public event Action<PlayerState>? TurnStarted;   // the new active player
    public event Action<PlayerState>? GameWon;

    public PigGame(string player1Name, string player2Name, int winScore = 100,
        Random? rng = null, int startingPlayer = 0)
    {
        _players = [new PlayerState(player1Name), new PlayerState(player2Name)];
        WinScore = winScore;
        _rng = rng ?? new Random();
        ActiveIndex = startingPlayer & 1;
    }

    /// <summary>The state visible to a strategy, from the active player's point of view.</summary>
    public GameView ActiveView =>
        new(Active.TotalScore, Active.TurnScore, Opponent.TotalScore, WinScore);

    /// <summary>Active player tosses the pigs. A pig out ends the turn.</summary>
    public PigRoll Roll()
    {
        if (IsOver) throw new InvalidOperationException("The game is over.");

        PlayerState roller = Active;
        PigRoll roll = Pigs.Roll(_rng);
        Rolled?.Invoke(roller, roll);

        if (roll.IsPigOut)
        {
            roller.TurnScore = 0;
            PiggedOut?.Invoke(roller);
            EndTurn();
        }
        else
        {
            Active.TurnScore += roll.Score;
        }

        return roll;
    }

    /// <summary>Active player banks the turn score and passes the pigs.</summary>
    public void Pass()
    {
        if (IsOver) throw new InvalidOperationException("The game is over.");
        Passed?.Invoke(Active);
        EndTurn();
    }

    /// <summary>Plays the active player's whole turn using a strategy. Returns the turn's rolls.</summary>
    public IReadOnlyList<PigRoll> PlayTurn(IRollStrategy strategy, Action<RollDecision>? onDecision = null)
    {
        var rolls = new List<PigRoll>();
        int actorAtStart = ActiveIndex;
        while (!IsOver && ActiveIndex == actorAtStart)
        {
            RollDecision d = strategy.Decide(ActiveView, _rng);
            onDecision?.Invoke(d);
            if (d.Roll) rolls.Add(Roll());
            else { Pass(); break; }
        }
        return rolls;
    }

    /// <summary>Runs a whole game with a strategy per player. Returns the winner's index.</summary>
    public int PlayToEnd(IRollStrategy player1, IRollStrategy player2,
        Action<int, RollDecision>? onDecision = null)
    {
        while (!IsOver)
        {
            int actor = ActiveIndex;
            IRollStrategy s = actor == 0 ? player1 : player2;
            PlayTurn(s, onDecision is null ? null : d => onDecision(actor, d));
        }
        return WinnerIndex;
    }

    private void EndTurn()
    {
        PlayerState p = Active;
        p.TotalScore += p.TurnScore;
        p.TurnScore = 0;

        if (p.TotalScore >= WinScore)
        {
            IsOver = true;
            WinnerIndex = ActiveIndex;
            GameWon?.Invoke(p);
            return;
        }

        ActiveIndex ^= 1;
        TurnNumber++;
        TurnStarted?.Invoke(Active);
    }
}
