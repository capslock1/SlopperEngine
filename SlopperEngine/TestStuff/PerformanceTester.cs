using System.Diagnostics;

namespace SlopperEngine.TestStuff;

/// <summary>
/// Quick class to test the performance of a function.
/// </summary>
public class PerformanceTester
{
    private Stopwatch _timer = new();
    public void StartTest()
    {
        _timer.Restart();
    }
    public void EndTest(string testName = "")
    {
        _timer.Stop();
        Console.WriteLine($"Test {testName} took {_timer.ElapsedTicks} ticks");
    }
}