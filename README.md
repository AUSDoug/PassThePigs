# Pass the Pigs

A C# implementation of Hasbro's *Pass the Pigs*, forked from
[`PassThePigsConsole`](https://github.com/AUSDoug/PassThePigsConsole) and being
reshaped into an Android app.

| project | what it is |
|---|---|
| `src/PassThePigs.Core` | The game engine and AI — pure C#, no UI. Roll scoring, turn/score/win rules, and the five roll strategies. |
| `src/PassThePigs.Console` | Headless companion: AI-vs-AI runs and the `bench` harness for tuning the AI. |
| `src/PassThePigs.App` | .NET MAUI app (`net9.0-android`) — the playable GUI. |

## The Android app

```
dotnet build src/PassThePigs.App -f net9.0-android          # -> a signed APK under bin/
```

Or open `PassThePigs.sln` in Visual Studio 2022 and deploy to an emulator or
device. **Setup** screen picks the opponent ruleset and who goes first (saved via
MAUI `Preferences`); the **game** screen shows the turn, both scores, the two pigs
from the last roll, and Roll / Pass. The opponent's turn plays out with a pause
between rolls. Needs the `maui` / `android` workload and the Android SDK.

## AI rulesets

| id | name | summary |
|----|------|---------|
| 0 | Basic | situational heuristics, falling back on "stop at 23" |
| 1 | Random | coin flip each roll |
| 2 | Aggressive | banks at 50 per turn |
| 3 | Expert | "stop at 23" + endgame play (press / gamble for the win) |
| 4 | EV | pure "stop at 23", no opponent awareness |

Derived from Michael Gorman, *Analytics, Pedagogy and the Pass the Pigs Game*,
INFORMS Transactions on Education 13(1), 2012 (`docs-gorman-paper.pdf`).

## Console usage

```
dotnet run --project src/PassThePigs.Console -- bench games=20000 seed=1 p1=expert p2=ev
dotnet run --project src/PassThePigs.Console -- play p1=3 p2=0 games=5 verbose
dotnet run --project src/PassThePigs.Console                # reads Settings.ini
```

`bench` prints one `RESULT ...` line and writes no logs. `play` logs game flow to
the console and `logs/`, with the AI's reasoning in `logs/ai-commentary-*.log`.
`tools/hillclimb.py` sweeps Expert's stop threshold using `bench`.

## Requirements

- .NET 9 SDK (Core + Console)
- .NET MAUI workload + Android SDK + JDK 17 (App)

## License

GNU General Public License v3 — see `LICENSE`.
