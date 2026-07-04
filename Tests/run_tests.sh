#!/usr/bin/env bash
# Runs the GoDotTest suite headlessly and exits non-zero on any failure —
# GoDotTest's own process exit code isn't reliable for this, so we parse
# its "Test results: Passed: N | Failed: N | Skipped: N" summary line instead.
set -o pipefail
cd "$(dirname "$0")/.."

GODOT_BIN="${GODOT_BIN:-}"
if [ -z "$GODOT_BIN" ]; then
    if command -v godot >/dev/null 2>&1; then
        GODOT_BIN=godot
    elif command -v godot-mono >/dev/null 2>&1; then
        GODOT_BIN=godot-mono
    else
        echo "Could not find a 'godot' or 'godot-mono' binary on PATH. Set GODOT_BIN to override." >&2
        exit 1
    fi
fi

output=$("$GODOT_BIN" --headless --run-tests --quit-on-finish Tests/TestMain.tscn 2>&1)
echo "$output"

if echo "$output" | grep -qE "Failed: [1-9]"; then
    echo "TESTS FAILED"
    exit 1
fi

if ! echo "$output" | grep -q "Failed: 0"; then
    echo "TESTS DID NOT COMPLETE (no summary line found — check output above for a crash/exception)"
    exit 1
fi

echo "ALL TESTS PASSED"
exit 0
