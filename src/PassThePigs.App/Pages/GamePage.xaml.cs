using System.Diagnostics;
using System.Text;
using PassThePigs.App.Services;
using PassThePigs.App.ViewModels;
using PassThePigs.Core;

namespace PassThePigs.App.Pages;

public partial class GamePage : ContentPage
{
    private readonly GameViewModel _vm;

    public GamePage(GameViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        _vm.RollShown += OnRollShown;
        _vm.NewGame += () => PigImage.Source = null;   // don't carry the last game's render in
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Start();
    }

    private async void OnRollShown(PigRoll roll)
    {
        string js =
            $"renderRoll('{PigPoses.PoseId(roll.One)}',{Dot(roll.One)}," +
            $"'{PigPoses.PoseId(roll.Two)}',{Dot(roll.Two)})";

        // 1. Render + get the base64 length back (a small int survives the bridge).
        int total = 0;
        for (int attempt = 0; attempt < 5 && total == 0; attempt++)
        {
            try
            {
                string? lenStr = await PigView.EvaluateJavaScriptAsync(js);
                int.TryParse(Clean(lenStr), out total);
            }
            catch (Exception ex) { Debug.WriteLine($"[pig] render eval failed: {ex.Message}"); }

            if (total == 0) await Task.Delay(300);
        }
        Debug.WriteLine($"[pig] base64 length = {total}");
        if (total == 0) return;

        // 2. Pull the base64 back in slices small enough to marshal reliably.
        const int chunk = 24_000;
        var sb = new StringBuilder(total);
        for (int off = 0; off < total; off += chunk)
        {
            int take = Math.Min(chunk, total - off);
            string? part = await PigView.EvaluateJavaScriptAsync($"__b64Slice({off},{take})");
            sb.Append(Clean(part));
        }

        try
        {
            byte[] png = Convert.FromBase64String(sb.ToString());
            Debug.WriteLine($"[pig] decoded {png.Length} bytes");
            PigImage.Source = ImageSource.FromStream(() => new MemoryStream(png));
        }
        catch (Exception ex) { Debug.WriteLine($"[pig] decode failed: {ex.Message}"); }
    }

    private static string Dot(PigPosition p) => PigPoses.ShowDot(p) ? "true" : "false";

    // Android's WebView hands JS string results back JSON-quoted; base64 itself
    // contains nothing that gets escaped, so trimming the wrapping quotes is enough.
    private static string Clean(string? evalResult)
    {
        if (string.IsNullOrEmpty(evalResult)) return string.Empty;
        string s = evalResult.Trim();
        if (s.Length >= 2 && s[0] == '"' && s[^1] == '"') s = s[1..^1];
        return s == "null" ? string.Empty : s;
    }
}
