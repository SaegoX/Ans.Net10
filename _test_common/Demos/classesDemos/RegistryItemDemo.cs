using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class RegistryItemDemo
{
    public static Task Run()
    {
        DemoConsole.Part("RegistryItem");
        CtorEmpty();
        CtorExplicit();
        ValueSetter();
        CtorFromString_Basic();
        CtorFromString_LabelAndLevel();
        CtorFromString_Escaping();
        FillErrors();
        RoundTrip();
        LevelNotSerializedBug();
        return Task.CompletedTask;
    }

    private static void CtorEmpty()
    {
        DemoConsole.Note("ctor() — defaults");
        var r = new RegistryItem();
        DemoConsole.Case("Key == \"\"", () => r.Key, "");
        DemoConsole.Case("IsLabel == false", () => r.IsLabel, false);
        DemoConsole.Case("Level == 0", () => r.Level, 0);
        DemoConsole.Case("Value == \"\"", () => r.Value, "");
    }

    private static void CtorExplicit()
    {
        DemoConsole.Note("ctor(key, value, level, isLabel)");
        var r = new RegistryItem("k", "v", 3, true);
        DemoConsole.Case("Key", () => r.Key, "k");
        DemoConsole.Case("Value", () => r.Value, "v");
        DemoConsole.Case("Level", () => r.Level, 3);
        DemoConsole.Case("IsLabel", () => r.IsLabel, true);
    }

    private static void ValueSetter()
    {
        DemoConsole.Note("Value setter: null/\"\" → \"{Key}\". " +
                         "Key задаётся только через ctor или FillFromString (private set).");
        var r1 = new RegistryItem("user", "value", 0, false);

        r1.Value = "";
        DemoConsole.Case("\"\" → \"{user}\"", () => r1.Value, "{user}");

        r1.Value = null!;
        DemoConsole.Case("null → \"{user}\"", () => r1.Value, "{user}");

        r1.Value = "filled";
        DemoConsole.Case("\"filled\" остаётся", () => r1.Value, "filled");
    }

    private static void CtorFromString_Basic()
    {
        DemoConsole.Note("ctor(string) — базовые");
        DemoConsole.Case("\"a=b\"", () => (From("a=b").Key, From("a=b").Value), ("a", "b"));
        DemoConsole.Case("\"a=\" → Value \"{a}\"", () => From("a=").Value, "{a}");
        DemoConsole.Case("\"a==b\" → Value \"=b\"", () => From("a==b").Value, "=b");
    }

    private static void CtorFromString_LabelAndLevel()
    {
        DemoConsole.Note("Label и Level в сериализации");

        DemoConsole.Case("\"a=#b\" → IsLabel, Level=0, Value=\"b\"", () =>
        {
            var r = From("a=#b");
            return (r.IsLabel, r.Level, r.Value);
        }, (true, 0, "b"));

        DemoConsole.Case("\"a=#:3b\" → IsLabel, Level=3, Value=\"b\"", () =>
        {
            var r = From("a=#:3b");
            return (r.IsLabel, r.Level, r.Value);
        }, (true, 3, "b"));

        DemoConsole.Case("\"a=:3b\" → Level=3, Value=\"b\"", () =>
        {
            var r = From("a=:3b");
            return (r.IsLabel, r.Level, r.Value);
        }, (false, 3, "b"));

        DemoConsole.Note("Если после ':' не digit — ':' просто отрезается, остальное уходит в Value");
        DemoConsole.Case("\"a=:xb\" → Level=0, Value=\"xb\"", () =>
        {
            var r = From("a=:xb");
            return (r.IsLabel, r.Level, r.Value);
        }, (false, 0, "xb"));

        DemoConsole.Note("Уровень парсится как ОДНА цифра, остаток остаётся в Value");
        DemoConsole.Case("\"a=:10b\" → Level=1, Value=\"0b\"", () =>
        {
            var r = From("a=:10b");
            return (r.Level, r.Value);
        }, (1, "0b"));
    }

    private static void CtorFromString_Escaping()
    {
        DemoConsole.Note("Экранирование \\; \\= \\# \\:");
        DemoConsole.Case("\"k=a\\\\=b\" → Value \"a=b\"", () => From("k=a\\=b").Value, "a=b");
        DemoConsole.Case("\"k=a\\\\;b\" → Value \"a;b\"", () => From("k=a\\;b").Value, "a;b");
        DemoConsole.Case("\"k=a\\\\#b\" → Value \"a#b\"", () => From("k=a\\#b").Value, "a#b");
        DemoConsole.Case("\"k=a\\\\:b\" → Value \"a:b\"", () => From("k=a\\:b").Value, "a:b");
        DemoConsole.Case("\"a\\\\=b=v\" → Key=\"a=b\" (в отличие от _Dict_Proto!)", () =>
            From("a\\=b=v").Key, "a=b");
    }

    private static void FillErrors()
    {
        DemoConsole.Note("FillFromString — ошибки");
        DemoConsole.CaseThrows<ArgumentException>("null", () => _ = From(null!));
        DemoConsole.CaseThrows<ArgumentException>("\"\"", () => _ = From(""));
        DemoConsole.CaseThrows<ArgumentException>("\"   \"", () => _ = From("   "));
        DemoConsole.CaseThrows<ArgumentException>("без '='", () => _ = From("abc"));
        DemoConsole.CaseThrows<ArgumentException>("'=' в начале", () => _ = From("=v"));  // i1 < 1
    }

    private static void RoundTrip()
    {
        DemoConsole.Note("ToString → обратно");
        var cases = new[]
        {
            new RegistryItem("k1", "v1", 0, false),
            new RegistryItem("k2", "v2", 3, true),
            new RegistryItem("k3", "v3", 9, true),
        };
        foreach (var c in cases)
        {
            DemoConsole.Case($"round-trip \"{c}\"", () =>
            {
                var again = From(c.ToString());
                return (again.Key, again.Value, again.Level, again.IsLabel);
            }, (c.Key, c.Value, c.Level, c.IsLabel));
        }
    }

    private static void LevelNotSerializedBug()
    {
        DemoConsole.Note("BUG: Level >= 10 в ToString не пишется → при десериализации становится 0");
        DemoConsole.Case("Level=10 → ToString не содержит ':10'", () =>
            new RegistryItem("k", "v", 10, false).ToString(), "k=v");
        DemoConsole.Case("Level=10 после round-trip → 0", () =>
        {
            var again = From(new RegistryItem("k", "v", 10, false).ToString());
            return again.Level;
        }, 0);
        DemoConsole.Note("Также Level < 0 не сериализуется, после round-trip → 0");
    }

    private static RegistryItem From(string s) => new(s);
}