using PassThePigs.Core;

namespace PassThePigs.App.Services;

/// <summary>
/// Maps a <see cref="PigPosition"/> to the landing-pose id used by the 3D model
/// (pig-model.js POSES, from Claude Design). The sider poses differ only in which
/// flank faces up, so the flank dot is visible for SideDot and hidden for SideNoDot.
/// </summary>
public static class PigPoses
{
    public static string PoseId(PigPosition p) => p switch
    {
        PigPosition.SideNoDot => "side-down",
        PigPosition.SideDot => "side-up",
        PigPosition.Razorback => "razorback",
        PigPosition.Trotter => "trotter",
        PigPosition.Snouter => "snouter",
        PigPosition.LeaningJowler => "jowler",
        _ => "trotter"
    };

    /// <summary>Whether the flank dot should render for this pose.</summary>
    public static bool ShowDot(PigPosition p) => p != PigPosition.SideNoDot;
}
