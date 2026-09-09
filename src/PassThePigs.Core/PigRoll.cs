namespace PassThePigs.Core;

/// <summary>The outcome of tossing both pigs.</summary>
/// <param name="One">Position of the first pig.</param>
/// <param name="Two">Position of the second pig.</param>
/// <param name="Score">Points for this roll; 0 means a pig out.</param>
public readonly record struct PigRoll(PigPosition One, PigPosition Two, int Score)
{
    public bool IsPigOut => Score == 0;
}
