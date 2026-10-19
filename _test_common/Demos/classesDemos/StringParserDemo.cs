using System.Globalization;
using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class StringParserDemo
{
    public static Task Run()
    {
        DemoConsole.Part("StringParser");
        Ctors();
        GetBounds();
        GetIntChecks();
        GetBoolChecks();
        GetDateTimeChecks();
        GetDateOnlyChecks();
        GetTimeOnlyChecks();
        Separators();
        return Task.CompletedTask;
    }

    private static void Ctors()
    {
        DemoConsole.Note("ctors — null / пустой / с разделителями");
        DemoConsole.Case("null source → 0", () => new StringParser(null).Get(0, "x"), "x");
        DemoConsole.Case("\"\" → 0", () => new StringParser("").Get(0, "x"), "x");
        DemoConsole.Case("\"a|b|c\" → 3", () =>
            new[] { new StringParser("a|b|c").Get(0), new StringParser("a|b|c").Get(1), new StringParser("a|b|c").Get(2) }.Length, 3);
        DemoConsole.Case("char ';'", () => new StringParser("a;b;c", ';').Get(1), "b");
        DemoConsole.Case("string sep", () => new StringParser("a::b::c", "::").Get(1), "b");
    }

    private static void GetBounds()
    {
        DemoConsole.Note("Get(index, default) — границы");
        var p = new StringParser("a||c"); // [a, "", c]
        DemoConsole.Case("index=-1", () => p.Get(-1, "DEF"), "DEF");
        DemoConsole.Case("index=0", () => p.Get(0), "a");
        DemoConsole.Case("index=1 (пусто)", () => p.Get(1), (string?)null);
        DemoConsole.Case("index=1 (пусто) + DEF", () => p.Get(1, "DEF"), "DEF");
        DemoConsole.Case("index=2", () => p.Get(2), "c");
        DemoConsole.Case("index=3 (out)", () => p.Get(3, "DEF"), "DEF");
        DemoConsole.Case("index=int.MaxValue", () => p.Get(int.MaxValue, "DEF"), "DEF");
    }

    private static void GetIntChecks()
    {
        DemoConsole.Note("GetInt — int? и с default");
        var p = new StringParser("42|-1|0|abc||" + int.MaxValue + "|" + int.MinValue + "|999999999999|  42  ");

        DemoConsole.Case("GetInt(0) 42", () => p.GetInt(0), 42);
        DemoConsole.Case("GetInt(1) -1", () => p.GetInt(1), -1);
        DemoConsole.Case("GetInt(2) 0", () => p.GetInt(2), 0);
        DemoConsole.Case("GetInt(3) abc → null", () => p.GetInt(3), (int?)null);
        DemoConsole.Case("GetInt(4) пусто → null", () => p.GetInt(4), (int?)null);
        DemoConsole.Case("GetInt(5) int.MaxValue", () => p.GetInt(5), int.MaxValue);
        DemoConsole.Case("GetInt(6) int.MinValue", () => p.GetInt(6), int.MinValue);
        DemoConsole.Case("GetInt(7) overflow → null", () => p.GetInt(7), (int?)null);
        DemoConsole.Case("GetInt(8) \"  42  \" → 42", () => p.GetInt(8), 42);

        DemoConsole.Case("GetInt(3, -1) abc → -1", () => p.GetInt(3, -1), -1);
        DemoConsole.Case("GetInt(100, 7) out → 7", () => p.GetInt(100, 7), 7);
    }

    private static void GetBoolChecks()
    {
        DemoConsole.Note("GetBool — \"1\", \"+\", \"true\" (без регистра)");
        var p = new StringParser("1|+|true|TRUE|True|0|false|yes||");
        DemoConsole.Case("\"1\"", () => p.GetBool(0), true);
        DemoConsole.Case("\"+\"", () => p.GetBool(1), true);   // doc: "+" тоже true
        DemoConsole.Case("\"true\"", () => p.GetBool(2), true);
        DemoConsole.Case("\"TRUE\"", () => p.GetBool(3), true);
        DemoConsole.Case("\"True\"", () => p.GetBool(4), true);
        DemoConsole.Case("\"0\"", () => p.GetBool(5), false);
        DemoConsole.Case("\"false\"", () => p.GetBool(6), false);
        DemoConsole.Case("\"yes\"", () => p.GetBool(7), false);
        DemoConsole.Case("\"\"", () => p.GetBool(8), false);
        DemoConsole.Case("out", () => p.GetBool(100), false);
    }

    private static void GetDateTimeChecks()
    {
        DemoConsole.Note("GetDateTime — ru-RU культура");
        var p = new StringParser("01.01.2024|not-a-date||2024-12-31 23:59:59");
        DemoConsole.Case("01.01.2024 → year 2024", () => p.GetDateTime(0)?.Year, 2024);
        DemoConsole.Case("not-a-date → null", () => p.GetDateTime(1), (DateTime?)null);
        DemoConsole.Case("\"\" → null", () => p.GetDateTime(2), (DateTime?)null);
        DemoConsole.Case("out → null", () => p.GetDateTime(100), (DateTime?)null);
        DemoConsole.Case("default 2000-01-01", () => p.GetDateTime(1, new DateTime(2000, 1, 1)).Year, 2000);
        DemoConsole.Case("format \"yyyy-MM-dd\"", () => p.GetDateTime(3, "yyyy-MM-dd"), "2024-12-31");
    }

    private static void GetDateOnlyChecks()
    {
        DemoConsole.Note("GetDateOnly");
        var p = new StringParser("01.01.2024|not-a-date|");
        DemoConsole.Case("01.01.2024 → month 1", () => p.GetDateOnly(0)?.Month, 1);
        DemoConsole.Case("invalid → null", () => p.GetDateOnly(1), (DateOnly?)null);
        DemoConsole.Case("default 2000-01-01", () => p.GetDateOnly(1, new DateOnly(2000, 1, 1)).Year, 2000);
        DemoConsole.Case("format \"yyyy-MM-dd\"", () => p.GetDateOnly(0, "yyyy-MM-dd"), "2024-01-01");
    }

    private static void GetTimeOnlyChecks()
    {
        DemoConsole.Note("GetTimeOnly");
        var p = new StringParser("14:30:00|not-a-time|");
        DemoConsole.Case("14:30:00 → hour 14", () => p.GetTimeOnly(0)?.Hour, 14);
        DemoConsole.Case("invalid → null", () => p.GetTimeOnly(1), (TimeOnly?)null);
        DemoConsole.Case("default 00:00", () => p.GetTimeOnly(1, new TimeOnly(0, 0)).Hour, 0);
        DemoConsole.Case("format \"HH-mm\"", () => p.GetTimeOnly(0, "HH-mm"), "14-30");
    }

    private static void Separators()
    {
        DemoConsole.Note("Кастомные разделители — char и string");
        DemoConsole.Case("char ';'", () => new StringParser("a;b;c", ';').Get(1), "b");
        DemoConsole.Case("string \"::\"", () => new StringParser("a::b::c", "::").Get(1), "b");
        DemoConsole.Note("Разделитель строка null → по умолчанию '|'");
        DemoConsole.Case("null string sep → '|'", () => new StringParser("a|b", (string?)null).Get(1), "b");
    }
}