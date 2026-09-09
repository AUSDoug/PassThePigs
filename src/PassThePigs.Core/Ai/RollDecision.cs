namespace PassThePigs.Core.Ai;

/// <summary>A strategy's choice plus a human-readable reason (for logging / commentary).</summary>
/// <param name="Roll">true to roll again, false to pass.</param>
/// <param name="Reason">Why - e.g. "reached the stop-at-23 threshold".</param>
public readonly record struct RollDecision(bool Roll, string Reason)
{
    public static RollDecision DoRoll(string reason) => new(true, reason);
    public static RollDecision Hold(string reason) => new(false, reason);
}
