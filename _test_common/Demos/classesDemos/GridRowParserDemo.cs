using System.Globalization;
using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class GridRowParserDemo
{
    public static Task Run()
    {
        DemoConsole.Part("GridRowParser");
        CtorNullGuard();
        TextBoundaries();
        IntBoundaries();
        RealBoundaries();
        DateBoundaries();
        BoolBoundaries();
        ProviderDiff();
        return Task.CompletedTask;
    }

    private static void CtorNullGuard()
    {
        DemoConsole.Note("_fields = null — нет guard, ждём NRE при обращении");
        DemoConsole.CaseThrows<NullReferenceException>("new GridRowParser(null, null).GetText(0)", () =>
            _ = new GridRowParser(null!, null).GetText(0));
    }

    private static void TextBoundaries()
    {
        DemoConsole.Note("GetText — границы индекса и содержимого");
        var r = new GridRowParser(new[] { "  a  ", "", "   ", null!, "b" }, null);
        DemoConsole.Case("index=-1", () => r.GetText(-1), "");
        DemoConsole.Case("index=0 (\"  a  \")", () => r.GetText(0), "a");
        DemoConsole.Case("index=1 (\"\")", () => r.GetText(1), "");
        DemoConsole.Case("index=2 (\"   \")", () => r.GetText(2), "");
        DemoConsole.Case("index=3 (null)", () => r.GetText(3), "");
        DemoConsole.Case("index=4 (\"b\")", () => r.GetText(4), "b");
        DemoConsole.Case("index=5 (out)", () => r.GetText(5), "");
        DemoConsole.Case("index=int.MaxValue", () => r.GetText(int.MaxValue), "");
    }

    private static void IntBoundaries()
    {
        DemoConsole.Note("GetInt — границы значений");
        var r = new GridRowParser(new[]
        {
            "42", "-1", "0", int.MaxValue.ToString(), int.MinValue.ToString(),
            " 42 ", "abc", "", "999999999999"
        }, CultureInfo.InvariantCulture);

        DemoConsole.Case("42", () => r.GetInt(0), 42);
        DemoConsole.Case("-1", () => r.GetInt(1), -1);
        DemoConsole.Case("0", () => r.GetInt(2), 0);
        DemoConsole.Case("int.MaxValue", () => r.GetInt(3), int.MaxValue);
        DemoConsole.Case("int.MinValue", () => r.GetInt(4), int.MinValue);
        DemoConsole.Case("\" 42 \"", () => r.GetInt(5), 42);
        DemoConsole.Case("\"abc\" → 0", () => r.GetInt(6), 0);
        DemoConsole.Case("\"\" → 0", () => r.GetInt(7), 0);
        DemoConsole.Case("overflow → 0", () => r.GetInt(8), 0);
    }

    private static void RealBoundaries()
    {
        DemoConsole.Note("GetReal — double");
        var r = new GridRowParser(new[]
        {
            "1.5", "-2.25", "0", "NaN", "Infinity", "abc", ""
        }, CultureInfo.InvariantCulture);

        DemoConsole.Case("1.5", () => r.GetReal(0), 1.5);
        DemoConsole.Case("-2.25", () => r.GetReal(1), -2.25);
        DemoConsole.Case("0", () => r.GetReal(2), 0.0);
        DemoConsole.Case("NaN", () => double.IsNaN(r.GetReal(3)), true);
        DemoConsole.Case("Infinity", () => double.IsPositiveInfinity(r.GetReal(4)), true);
        DemoConsole.Case("abc → 0", () => r.GetReal(5), 0.0);
        DemoConsole.Case("\"\" → 0", () => r.GetReal(6), 0.0);
    }

    private static void DateBoundaries()
    {
        DemoConsole.Note("GetDate — культура важна");
        var ru = new GridRowParser(new[] { "01.01.2024", "not-a-date", "" }, new CultureInfo("ru-RU"));
        DemoConsole.Case("01.01.2024 ru → 2024-01-01", () =>
            ru.GetDate(0)?.Date == new DateTime(2024, 1, 1), true);
        DemoConsole.Case("not-a-date → null", () => ru.GetDate(1), (DateTime?)null);
        DemoConsole.Case("\"\" → null", () => ru.GetDate(2), (DateTime?)null);
        DemoConsole.Case("index out → null", () => ru.GetDate(99), (DateTime?)null);

        var inv = new GridRowParser(new[] { "2024-01-01" }, CultureInfo.InvariantCulture);
        DemoConsole.Case("2024-01-01 invariant", () => inv.GetDate(0)?.Date, new DateTime(2024, 1, 1));
    }

    private static void BoolBoundaries()
    {
        DemoConsole.Note("GetBool — \"1\" / \"true\" (case-insensitive), остальное false");
        var r = new GridRowParser(new[]
        {
            "1", "true", "TRUE", "True", "yes", "0", "false", "", " "
        }, CultureInfo.InvariantCulture);
        DemoConsole.Case("\"1\" → true", () => r.GetBool(0), true);
        DemoConsole.Case("\"true\" → true", () => r.GetBool(1), true);
        DemoConsole.Case("\"TRUE\" → true", () => r.GetBool(2), true);
        DemoConsole.Case("\"True\" → true", () => r.GetBool(3), true);
        DemoConsole.Case("\"yes\" → false", () => r.GetBool(4), false);
        DemoConsole.Case("\"0\" → false", () => r.GetBool(5), false);
        DemoConsole.Case("\"false\" → false", () => r.GetBool(6), false);
        DemoConsole.Case("\"\" → false", () => r.GetBool(7), false);
        DemoConsole.Case("\"  \" → false", () => r.GetBool(8), false);
        DemoConsole.Case("index out → false", () => r.GetBool(99), false);
    }

    private static void ProviderDiff()
    {
        DemoConsole.Note("Разница провайдеров: '1,5' vs '1.5'");
        var ru = new GridRowParser(new[] { "1,5" }, new CultureInfo("ru-RU"));
        var inv = new GridRowParser(new[] { "1.5" }, CultureInfo.InvariantCulture);
        DemoConsole.Case("ru: \"1,5\" → 1.5", () => ru.GetReal(0), 1.5);
        DemoConsole.Case("inv: \"1.5\" → 1.5", () => inv.GetReal(0), 1.5);
    }
}