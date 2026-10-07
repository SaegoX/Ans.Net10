using System.Diagnostics;
using System.Reflection;

namespace _test_common.Framework;

/// <summary>Запускает один кейс, сравнивает с ожиданием.</summary>
public static class DemoRunner
{
    public static DemoResult Run(DemoEntry entry, DemoCaseAttribute testCase)
    {
        var sw = Stopwatch.StartNew();
        object? actual = null;
        Exception? error = null;

        try
        {
            actual = entry.Method.Invoke(null, testCase.Args);
        }
        catch (TargetInvocationException tie)
        {
            error = tie.InnerException ?? tie;
        }
        catch (Exception ex)
        {
            error = ex;
        }

        sw.Stop();

        bool success;
        if (testCase.ExpectException)
            success = error is not null;
        else if (error is not null)
            success = false;
        else if (testCase.ShouldVerify)
            success = Equals(actual, testCase.Expected);
        else
            success = true;

        return new DemoResult
        {
            CaseLabel = testCase.Label,
            Success = success,
            Actual = actual,
            Expected = testCase.Expected,
            ExpectException = testCase.ExpectException,
            ShouldVerify = testCase.ShouldVerify,
            Exception = error,
            Duration = sw.Elapsed,
        };
    }
}