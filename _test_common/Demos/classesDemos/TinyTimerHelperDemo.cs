using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class TinyTimerHelperDemo
{
    public static Task Run()
    {
        DemoConsole.Part("TinyTimerHelper");
        CtorChecks();
        FirstTrigger();
        GridCatchUp();
        GridVsFromNow();
        ZeroSpanBehavior();
        return Task.CompletedTask;
    }

    private static void CtorChecks()
    {
        DemoConsole.Note("ctor — NextStop ≈ Now + Span");
        var before = DateTime.Now;
        var t = new TinyTimerHelper(TimeSpan.FromSeconds(10), true);
        var after = DateTime.Now;

        DemoConsole.Case("Span установлен", () => t.Span, TimeSpan.FromSeconds(10));
        DemoConsole.Case("UseEqualIntervals", () => t.UseEqualIntervals, true);
        DemoConsole.Case("NextStop в [before, after] + 10s", () =>
            t.NextStop >= before.AddSeconds(10) && t.NextStop <= after.AddSeconds(10), true);
    }

    private static void FirstTrigger()
    {
        DemoConsole.Note("Test() — false до достижения NextStop, true после");

        DemoConsole.Note("Режим UseEqualIntervals=false — NextStop = now + Span");
        var t = new TinyTimerHelper(TimeSpan.FromMilliseconds(30), false);

        DemoConsole.Case("Test сразу → false", () => t.Test(), false);
        Thread.Sleep(150);
        DemoConsole.Case("после Sleep(150ms) → true", () => t.Test(), true);
        DemoConsole.Case("сразу повторно → false", () => t.Test(), false);
    }

    private static void GridCatchUp()
    {
        DemoConsole.Note("UseEqualIntervals=true + большой Sleep — сетка догоняет, " +
                         "несколько Test() подряд = true (это правильно для сетки)");
        var t = new TinyTimerHelper(TimeSpan.FromMilliseconds(30), true);
        Thread.Sleep(200);

        DemoConsole.Case("1-й Test = true", () => t.Test(), true);
        DemoConsole.Case("2-й Test = true", () => t.Test(), true);
        DemoConsole.Case("3-й Test = true", () => t.Test(), true);
        DemoConsole.Note("После исчерпания «пропущенных» интервалов Test начнёт возвращать false до следующего Span");
    }

    private static void GridVsFromNow()
    {
        DemoConsole.Note("UseEqualIntervals=true — сетка от предыдущего NextStop");

        var t1 = new TinyTimerHelper(TimeSpan.FromMilliseconds(30), true);
        Thread.Sleep(200);
        var first = t1.Test();          // true, NextStop = prevNextStop + 30
        var nextGrid = t1.NextStop;

        Thread.Sleep(50);
        var second = t1.Test();          // true, NextStop = nextGrid + 30
        var nextGrid2 = t1.NextStop;

        DemoConsole.Case("первый Test = true", () => first, true);
        DemoConsole.Case("второй Test = true", () => second, true);
        DemoConsole.Case("шаг между стопами == Span (сетка)", () =>
            (nextGrid2 - nextGrid).TotalMilliseconds == 30.0, true);

        DemoConsole.Note("UseEqualIntervals=false — от текущего момента");
        var t2 = new TinyTimerHelper(TimeSpan.FromMilliseconds(30), false);
        Thread.Sleep(200);
        t2.Test();
        var justAfter = DateTime.Now;
        var nextNow = t2.NextStop;
        DemoConsole.Case("NextStop ≈ Now + 30ms (в пределах ±100ms)", () =>
            Math.Abs((nextNow - justAfter.AddMilliseconds(30)).TotalMilliseconds) < 100, true);
    }

    private static void ZeroSpanBehavior()
    {
        DemoConsole.Note("Span=0 — NextStop == Now при ctor → Test() сразу проверяет >= now");
        var t = new TinyTimerHelper(TimeSpan.Zero, true);
        Thread.Sleep(5);
        DemoConsole.Case("первый Test = true", () => t.Test(), true);

        DemoConsole.Note("Span отрицательный — NextStop в прошлом; Test = true всегда");
        var tn = new TinyTimerHelper(TimeSpan.FromMilliseconds(-100), true);
        DemoConsole.Case("первый Test = true", () => tn.Test(), true);
        DemoConsole.Case("второй Test = true", () => tn.Test(), true);
    }
}