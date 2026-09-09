using PassThePigs.Core;

namespace PassThePigs.App.Services;

/// <summary>Maps a pig's landing position to a sprite in Resources/Images.</summary>
public static class PigImages
{
    /// <summary>e.g. "razorback_blank.png". The dot / facing variant is cosmetic and random.</summary>
    public static string For(PigPosition position, Random rng) => position switch
    {
        PigPosition.SideNoDot => Sider(rng, dot: false),
        PigPosition.SideDot => Sider(rng, dot: true),
        PigPosition.Razorback => "razorback" + Variant(rng),
        PigPosition.Trotter => "trotter" + Variant(rng),
        PigPosition.Snouter => "snouter" + Variant(rng),
        PigPosition.LeaningJowler => "jowler" + Variant(rng),
        _ => "trotter_blank.png"
    };

    private static string Sider(Random rng, bool dot) =>
        (rng.Next(2) == 0 ? "side" : "side_mirrored") + (dot ? "_dot.png" : "_blank.png");

    private static string Variant(Random rng) => rng.Next(2) == 0 ? "_blank.png" : "_dot.png";
}
