using Chickensoft.GoDotTest;
using Chickensoft.Log;
using Godot;

namespace CitySim.Tests;

// Minimal ITestReporter: one line per [Test] method (green PASS / red FAIL / yellow SKIP),
// no per-Setup/Cleanup noise, no cutesy phrasing. Registered via CompactTestAdapter.
public class CompactTestReporter(ILog log) : ITestReporter
{
    private int _passed;
    private int _failed;
    private int _skipped;

    public bool HadError { get; private set; }

    public void Update(TestEvent testEvent) { }

    public void SuiteUpdate(ITestSuite suite, TestSuiteEvent suiteEvent) { }

    public void MethodUpdate(ITestSuite suite, ITestMethod method, TestMethodEvent methodEvent)
    {
        if (method.Type != TestMethodType.Test) return;

        switch (methodEvent)
        {
            case TestMethodPassedEvent:
                _passed++;
                GD.PrintRich($"[color=green]PASS[/color] {suite.Name}.{method.Name}");
                break;
            case TestMethodFailedEvent failed:
                _failed++;
                HadError = true;
                GD.PrintRich($"[color=red]FAIL[/color] {suite.Name}.{method.Name}");
                log.Err(failed.FailureException);
                break;
            case TestMethodSkippedEvent:
                _skipped++;
                GD.PrintRich($"[color=yellow]SKIP[/color] {suite.Name}.{method.Name}");
                break;
        }
    }

    // Keep the exact "Test results: Passed: N | Failed: N | Skipped: N" wording —
    // Tests/run_tests.sh greps for it to decide the process exit code.
    public void OutputFinalReport() =>
        log.Print($"Test results: Passed: {_passed} | Failed: {_failed} | Skipped: {_skipped}");
}
