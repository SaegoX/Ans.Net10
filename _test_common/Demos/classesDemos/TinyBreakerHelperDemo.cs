using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class TinyBreakerHelperDemo
{
    public static Task Run()
    {
        DemoConsole.Part("TinyBreakerHelper");
        CtorDefaults();
        SimpleCycle();
        StepOne();
        StepZeroOrNegative();
        return Task.CompletedTask;
    }

    private static void CtorDefaults()
    {
        DemoConsole.Note("ctor");
        DemoConsole.Case("new() → Step=1234", () => new TinyBreakerHelper().Step, 1234);
        DemoConsole.Case("new(10) → Step=10", () => new TinyBreakerHelper(10).Step, 10);
        DemoConsole.Case("начальный Current == 0", () => new TinyBreakerHelper(5).Current, 0);
    }

    private static void SimpleCycle()
    {
        DemoConsole.Note("Step=3: Next → false, false, true, false, false, true, ...");
        var b = new TinyBreakerHelper(3);
        DemoConsole.Case("1-й Next → false", () => b.Next(), false);
        DemoConsole.Case("2-й Next → false", () => b.Next(), false);
        DemoConsole.Case("3-й Next → true", () => b.Next(), true);
        DemoConsole.Case("Current сброшен в 0", () => b.Current, 0);

        DemoConsole.Note("15 итераций при Step=3 → ровно 5 срабатываний");
        var b2 = new TinyBreakerHelper(3);
        int triggers = 0;
        for (int i = 0; i < 15; i++) if (b2.Next()) triggers++;
        DemoConsole.Case("срабатываний == 5", () => triggers, 5);
    }

    private static void StepOne()
    {
        DemoConsole.Note("Step=1 → Next() всегда true, Current всегда 0");
        var b = new TinyBreakerHelper(1);
        DemoConsole.Case("1-й Next → true", () => b.Next(), true);
        DemoConsole.Case("2-й Next → true", () => b.Next(), true);
        DemoConsole.Case("Current всегда 0", () => b.Current, 0);
    }

    private static void StepZeroOrNegative()
    {
        DemoConsole.Note("BUG/пограничное: step<=0 не валидируется, Next() всегда true");
        DemoConsole.Case("Step=0 Next → true", () =>
        {
            var b = new TinyBreakerHelper(0);
            return b.Next();
        }, true);

        DemoConsole.Case("Step=-1 Next → true", () =>
        {
            var b = new TinyBreakerHelper(-1);
            return b.Next();
        }, true);

        DemoConsole.Note("TODO: step<=0 запрещён контрактом — надо ArgumentOutOfRangeException в ctor");
    }
}