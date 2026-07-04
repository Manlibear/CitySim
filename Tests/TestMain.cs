using System.Reflection;
using Godot;
using Chickensoft.GoDotTest;

namespace CitySim.Tests;

// Dedicated test-runner entry point, decoupled from the game's real Main.cs
// scene so tests don't depend on the full world bootstrapping successfully.
// Run with: godot --headless --run-tests --quit-on-finish Tests/TestMain.tscn
public partial class TestMain : Node
{
    public TestEnvironment Environment = default!;

    public override void _Ready()
    {
        GoTest.Adapter = new CompactTestAdapter();
        Environment = TestEnvironment.From(OS.GetCmdlineArgs());
        _ = GoTest.RunTests(Assembly.GetExecutingAssembly(), this, Environment);
    }
}
