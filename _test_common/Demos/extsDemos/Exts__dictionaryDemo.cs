using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class Exts__dictionaryDemo
{
    public static Task Run()
    {
        DemoConsole.Part("Exts__dictionary");
        GetSerializationChecks();
        GetFromDictChecks();
        GetStringChecks();
        GetIntChecks();
        GetLongChecks();
        GetDoubleChecks();
        GetBoolChecks();
        GetDateTimeChecks();
        GetDateOnlyTimeOnlyChecks();
        return Task.CompletedTask;
    }

    private static void GetSerializationChecks()
    {
        DemoConsole.Note("GetSerialization — key + value (опционально с шаблоном)");
        var kvp = new KeyValuePair<string, string>("k", "v");
        DemoConsole.Case("без шаблона → \"kv\"", () => kvp.GetSerialization(), "kv");
        DemoConsole.Case("с шаблоном \"[{0}]\" → \"k[v]\"", () => kvp.GetSerialization("[{0}]"), "k[v]");
    }

    private static void GetFromDictChecks()
    {
        DemoConsole.Note("GetFromDict / Get — null/not found → default");
        IDictionary<string, int>? dict = new Dictionary<string, int> { ["a"] = 1 };
        DemoConsole.Case("найден", () => dict.GetFromDict("a"), 1);
        DemoConsole.Case("не найден → 0", () => dict.GetFromDict("zzz"), 0);
        DemoConsole.Case("null dict → 0", () => ((IDictionary<string, int>?)null).GetFromDict("a"), 0);

        IReadOnlyDictionary<string, int>? ro = new Dictionary<string, int> { ["a"] = 1 };
        DemoConsole.Case("RO найден", () => ro.Get("a"), 1);
        DemoConsole.Case("RO null dict → 0", () => ((IReadOnlyDictionary<string, int>?)null).Get("a"), 0);
    }

    private static void GetStringChecks()
    {
        DemoConsole.Note("GetStringFromDict / GetValueStringOrKey");
        var dictObj = new Dictionary<string, object> { ["k"] = "hello", ["n"] = 42, ["nil"] = null! };

        DemoConsole.Case("k=\"hello\"", () => dictObj.GetStringFromDict("k", "def"), "hello");
        DemoConsole.Case("n=42 (ToString)", () => dictObj.GetStringFromDict("n", "def"), "42");
        DemoConsole.Case("nil → def", () => dictObj.GetStringFromDict("nil", "def"), "def");
        DemoConsole.Case("missing → def", () => dictObj.GetStringFromDict("zzz", "def"), "def");

        DemoConsole.Note("GetValueStringOrKey — func + keyTemplate");
        DemoConsole.Case("найден", () => dictObj.GetValueStringOrKey("n", v => v.ToString()), "42");
        DemoConsole.Case("не найден → key", () => dictObj.GetValueStringOrKey("zzz", v => v.ToString()), "zzz");
        DemoConsole.Case("не найден + шаблон", () =>
            dictObj.GetValueStringOrKey("zzz", v => v.ToString(), "<{0}>"), "<zzz>");
    }

    private static void GetIntChecks()
    {
        DemoConsole.Note("GetIntFromDict — конверсии int/long/string/bool");
        var dict = new Dictionary<string, object>
        {
            ["i"] = 42,
            ["l"] = 100L,
            ["l_big"] = (long)int.MaxValue + 1,
            ["s"] = "123",
            ["s_bad"] = "abc",
            ["b"] = true,
            ["null"] = null!
        };

        DemoConsole.Case("int", () => dict.GetIntFromDict("i", -1), 42);
        DemoConsole.Case("long в диапазоне", () => dict.GetIntFromDict("l", -1), 100);
        DemoConsole.Case("long > int.Max → fallback по ToInt строки \"abc\" → -1", () =>
            dict.GetIntFromDict("l_big", -1));
        DemoConsole.Case("string \"123\"", () => dict.GetIntFromDict("s", -1), 123);
        DemoConsole.Case("string \"abc\" → -1", () => dict.GetIntFromDict("s_bad", -1), -1);
        DemoConsole.Case("bool true → \"True\" → ToInt → -1", () =>
            dict.GetIntFromDict("b", -1));
        DemoConsole.Case("null → -1", () => dict.GetIntFromDict("null", -1), -1);
        DemoConsole.Case("missing → -1", () => dict.GetIntFromDict("zzz", -1), -1);
    }

    private static void GetLongChecks()
    {
        DemoConsole.Note("GetLongFromDict — конверсии");
        var dict = new Dictionary<string, object>
        {
            ["l"] = long.MaxValue,
            ["i"] = 42,
            ["s"] = "9999999999",
            ["s_bad"] = "abc"
        };
        DemoConsole.Case("long.MaxValue", () => dict.GetLongFromDict("l", -1), long.MaxValue);
        DemoConsole.Case("int → long", () => dict.GetLongFromDict("i", -1), 42L);
        DemoConsole.Case("string large", () => dict.GetLongFromDict("s", -1), 9999999999L);
        DemoConsole.Case("string bad → -1", () => dict.GetLongFromDict("s_bad", -1), -1L);
    }

    private static void GetDoubleChecks()
    {
        DemoConsole.Note("GetDoubleFromDict");
        var dict = new Dictionary<string, object>
        {
            ["d"] = 1.5,
            ["i"] = 42,
            ["s"] = "2.71828",
            ["s_bad"] = "abc"
        };
        DemoConsole.Case("double", () => dict.GetDoubleFromDict("d", -1), 1.5);
        DemoConsole.Case("int → double", () => dict.GetDoubleFromDict("i", -1), 42.0);
        DemoConsole.Case("string", () => dict.GetDoubleFromDict("s", -1), 2.71828);
        DemoConsole.Case("string bad → -1", () => dict.GetDoubleFromDict("s_bad", -1), -1.0);
    }

    private static void GetBoolChecks()
    {
        DemoConsole.Note("GetBoolFromDict — int==1, bool, string");
        var dict = new Dictionary<string, object>
        {
            ["t"] = true,
            ["f"] = false,
            ["i1"] = 1,
            ["i0"] = 0,
            ["s_true"] = "true",
            ["s_yes"] = "yes",
            ["null"] = null!
        };

        DemoConsole.Case("true", () => dict.GetBoolFromDict("t"), true);
        DemoConsole.Case("false", () => dict.GetBoolFromDict("f"), false);
        DemoConsole.Case("int 1 → true", () => dict.GetBoolFromDict("i1"), true);
        DemoConsole.Case("int 0 → false", () => dict.GetBoolFromDict("i0"), false);
        DemoConsole.Case("string \"true\" → true", () => dict.GetBoolFromDict("s_true"), true);
        DemoConsole.Case("string \"yes\" → ToBool → false", () => dict.GetBoolFromDict("s_yes"), false);
        DemoConsole.Case("null → false (default)", () => dict.GetBoolFromDict("null"), false);
        DemoConsole.Case("null → true (default=true)", () => dict.GetBoolFromDict("null", true), true);
    }

    private static void GetDateTimeChecks()
    {
        DemoConsole.Note("GetDateTimeFromDict");
        var dict = new Dictionary<string, object>
        {
            ["dt"] = new DateTime(2026, 6, 15, 14, 30, 0),
            ["s_iso"] = "2026-06-15",
            ["s_bad"] = "abc",
            ["null"] = null!
        };
        DemoConsole.Case("DateTime напрямую", () => dict.GetDateTimeFromDict("dt", default), new DateTime(2026, 6, 15, 14, 30, 0));
        DemoConsole.Case("string \"2026-06-15\"", () =>
            dict.GetDateTimeFromDict("s_iso", default).Year, 2026);
        DemoConsole.Case("string bad → default", () =>
            dict.GetDateTimeFromDict("s_bad", new DateTime(2000, 1, 1)), new DateTime(2000, 1, 1));
        DemoConsole.Case("null → default", () =>
            dict.GetDateTimeFromDict("null", new DateTime(2000, 1, 1)), new DateTime(2000, 1, 1));
    }

    private static void GetDateOnlyTimeOnlyChecks()
    {
        DemoConsole.Note("GetDateOnlyFromDict / GetTimeOnlyFromDict");
        var dict = new Dictionary<string, object>
        {
            ["d"] = new DateOnly(2026, 6, 15),
            ["dt"] = new DateTime(2026, 6, 15, 14, 30, 0),
            ["t"] = new TimeOnly(14, 30, 0),
            ["ts"] = new TimeSpan(14, 30, 0)
        };

        DemoConsole.Case("DateOnly напрямую", () => dict.GetDateOnlyFromDict("d", default), new DateOnly(2026, 6, 15));
        DemoConsole.Case("DateTime → DateOnly", () => dict.GetDateOnlyFromDict("dt", default), new DateOnly(2026, 6, 15));
        DemoConsole.Case("TimeOnly напрямую", () => dict.GetTimeOnlyFromDict("t", default), new TimeOnly(14, 30, 0));
        DemoConsole.Case("DateTime → TimeOnly", () => dict.GetTimeOnlyFromDict("dt", default), new TimeOnly(14, 30, 0));
        DemoConsole.Case("TimeSpan → TimeOnly", () => dict.GetTimeOnlyFromDict("ts", default), new TimeOnly(14, 30, 0));
    }
}