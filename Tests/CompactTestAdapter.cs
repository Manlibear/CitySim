using Chickensoft.GoDotTest;
using Chickensoft.Log;

namespace CitySim.Tests;

public class CompactTestAdapter : TestAdapter
{
    public override ITestReporter CreateReporter(ILog log) => new CompactTestReporter(log);
}
