namespace PassThePigs.Core.Ai;

/// <summary>A "roll or pass" decision maker. Implementations must be pure (no game mutation).</summary>
public interface IRollStrategy
{
    /// <summary>Short lowercase id, e.g. "expert".</summary>
    string Name { get; }

    /// <summary>Decide whether the active player should roll again.</summary>
    /// <param name="view">The visible game state, from the active player's perspective.</param>
    /// <param name="rng">Shared RNG - only the Random strategy uses it.</param>
    RollDecision Decide(in GameView view, Random rng);
}
