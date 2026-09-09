#!/usr/bin/env python3
"""
BaseTarget sweep / hill-climb for the Expert decision tree.

Expert now has a single tunable knob: BaseTarget (the "stop at N" threshold).
This script plays each candidate threshold head-to-head against a champion
(both side orderings, first turn alternated inside the exe) and reports the
win rate, so you can see the plateau around Gorman's 23.

Usage:
    python tools/hillclimb.py [--games N] [--seed K]
                              [--champion T] [--range LO HI]
"""

import argparse
import math
import re
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
RESULT_RE = re.compile(r"(\w+)=(\S+)")


def find_exe() -> str:
    candidates = sorted(REPO.glob("src/PassThePigs.Console/bin/*/*/PassThePigs.Console.exe"))
    candidates += sorted(REPO.glob("src/PassThePigs.Console/bin/*/*/PassThePigs.Console"))
    if not candidates:
        sys.exit("No built exe found - run: dotnet build src/PassThePigs.Console")
    return str(candidates[-1])


def run_match(exe, games, seed, p1_spec, p2_spec):
    out = subprocess.check_output(
        [exe, "bench", f"games={games}", f"seed={seed}", f"p1={p1_spec}", f"p2={p2_spec}"],
        text=True,
    ).strip()
    d = dict(RESULT_RE.findall(out))
    return int(d["p1wins"]), int(d["p2wins"])


def evaluate(exe, games, seed, champion, challenger):
    """Win rate of BaseTarget `challenger` vs BaseTarget `champion`, both orderings."""
    cw1, mw1 = run_match(exe, games, seed, f"expert:{challenger}", f"expert:{champion}")
    mw2, cw2 = run_match(exe, games, seed + 1, f"expert:{champion}", f"expert:{challenger}")
    ch_wins = cw1 + cw2
    total = ch_wins + mw1 + mw2
    rate = ch_wins / total
    se = math.sqrt(rate * (1.0 - rate) / total)
    return rate, se


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--games", type=int, default=100000, help="games per ordering (x2 per comparison)")
    ap.add_argument("--seed", type=int, default=1)
    ap.add_argument("--champion", type=int, default=23)
    ap.add_argument("--range", type=int, nargs=2, default=[16, 34])
    args = ap.parse_args()

    exe = find_exe()
    print(f"exe      : {exe}")
    print(f"champion : BaseTarget={args.champion}   games/cmp: {args.games * 2}\n")
    print(f"  {'BaseTarget':<12} {'win rate vs champion':<22} {'edge':<10}")

    seed = args.seed
    best = (args.champion, 0.5)
    for t in range(args.range[0], args.range[1] + 1):
        if t == args.champion:
            continue
        rate, se = evaluate(exe, args.games, seed, args.champion, t)
        seed += 2
        edge = rate - 0.5
        flag = "  <-- best" if rate > best[1] and edge > 2 * se else ""
        print(f"  {t:<12} {rate:.4f} +/- {se:.4f}      {edge:+.4f}{flag}")
        if rate > best[1] and edge > 2 * se:
            best = (t, rate)

    if best[0] == args.champion:
        print(f"\nNo threshold significantly beats {args.champion}. Plateau confirmed.")
    else:
        print(f"\nBest: BaseTarget={best[0]} (win rate {best[1]:.4f} vs {args.champion})")


if __name__ == "__main__":
    main()
