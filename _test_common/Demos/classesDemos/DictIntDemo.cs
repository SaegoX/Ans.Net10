using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class DictIntDemo
{
    public static Task Run()
    {
        DemoConsole.Part("DictInt : _Dict_Proto<int, string>  (реальный наследник)");
        CtorEmpty();
        CtorFromString();
        CtorFromParams();
        CtorFromEnumerable();
        AddSingle();
        KeyBoundaries();
        ValueBoundaries();
        ToStringCheck();
        RoundTrip();
        GetValueOrKeyCheck();
        return Task.CompletedTask;
    }

    private static void CtorEmpty()
    {
        DemoConsole.Note("ctor()");
        DemoConsole.Case("Count == 0", () => new DictInt().Count, 0);
        DemoConsole.Case("ToString() == \"\"", () => new DictInt().ToString(), "");
    }

    private static void CtorFromString()
    {
        DemoConsole.Note("ctor(string)");
        DemoConsole.Case("\"1=one;2=two\" → 2", () => new DictInt("1=one;2=two").Count, 2);
        DemoConsole.Case("\"1=one\" → [1]=\"one\"", () => new DictInt("1=one")[1], "one");
        DemoConsole.Case("\"\" → 0", () => new DictInt("").Count, 0);
        DemoConsole.Case("null → 0", () => new DictInt((string?)null).Count, 0);
    }

    private static void CtorFromParams()
    {
        DemoConsole.Note("ctor(params string[])");
        DemoConsole.Case("\"1=a\", \"2=b\" → 2", () => new DictInt("1=a", "2=b").Count, 2);
        DemoConsole.Case("пустой → 0", () => new DictInt().Count, 0);
    }

    private static void CtorFromEnumerable()
    {
        DemoConsole.Note("ctor(IEnumerable<string>)");
        DemoConsole.Case("[\"1=a\", \"2=b\"]", () => new DictInt(new[] { "1=a", "2=b" }).Count, 2);
    }

    private static void AddSingle()
    {
        DemoConsole.Note("Add(string)");
        DemoConsole.Case("Add(\"1=v\") → [1]=\"v\"", () => { var d = new DictInt(); d.Add("1=v"); return d[1]; }, "v");
        DemoConsole.Case("Add(null) — no-op", () => { var d = new DictInt(); d.Add(null!); return d.Count; }, 0);
        DemoConsole.Case("Add(\"\") — no-op", () => { var d = new DictInt(); d.Add(""); return d.Count; }, 0);
    }

    private static void KeyBoundaries()
    {
        DemoConsole.Note("Границы int-ключа");
        DemoConsole.Case("ключ int.MinValue", () => new DictInt($"{int.MinValue}=min")[int.MinValue], "min");
        DemoConsole.Case("ключ int.MaxValue", () => new DictInt($"{int.MaxValue}=max")[int.MaxValue], "max");
        DemoConsole.Case("ключ 0", () => new DictInt("0=zero")[0], "zero");
        DemoConsole.Case("ключ -1", () => new DictInt("-1=minus")[-1], "minus");

        DemoConsole.Note("НЕвалидный ключ → FormatException (StringToKey → key.ToInt() ?? throw)");
        DemoConsole.CaseThrows<FormatException>("ключ \"abc\"", () => _ = new DictInt("abc=1"));
        DemoConsole.CaseThrows<FormatException>("ключ \"\"", () => _ = new DictInt("=v"));
        DemoConsole.CaseThrows<FormatException>("ключ \"12.5\"", () => _ = new DictInt("12.5=v"));
        DemoConsole.CaseThrows<FormatException>("ключ за пределами int", () => _ = new DictInt("99999999999=v"));
    }

    private static void ValueBoundaries()
    {
        DemoConsole.Note("Границы значения string");
        DemoConsole.Case("пустое значение \"1=\"", () => new DictInt("1=")[1], "");
        DemoConsole.Case("длинное значение", () =>
            new DictInt("1=" + new string('x', 1000))[1].Length, 1000);
        DemoConsole.Case("значение с пробелами", () => new DictInt("1= hello ")[1], " hello ");
        DemoConsole.Case("значение с юникодом", () => new DictInt("1=привет🎉")[1], "привет🎉");
    }

    private static void ToStringCheck()
    {
        DemoConsole.Note("ToString() — сериализация");
        DemoConsole.Case("пустой → \"\"", () => new DictInt().ToString(), "");
        DemoConsole.Case("одна пара", () =>
        {
            var d = new DictInt(); d[5] = "five"; return d.ToString();
        }, "5=five");
        DemoConsole.Case("несколько пар через ';'", () =>
        {
            var d = new DictInt(); d[1] = "a"; d[2] = "b"; d[3] = "c"; return d.ToString();
        }, "1=a;2=b;3=c");
    }

    private static void RoundTrip()
    {
        DemoConsole.Note("Round-trip: string → DictInt → string");
        const string src = "1=one;2=two;3=three";
        DemoConsole.Case("ToString сохраняет строку", () => new DictInt(src).ToString(), src);
        DemoConsole.Case("двойной прогон стабилен", () =>
        {
            var once = new DictInt(src).ToString();
            var twice = new DictInt(once).ToString();
            return once == twice;
        }, true);

        DemoConsole.Note("Экранирование ';' в значении (работает, т.к. ';' — разделитель пар)");
        DemoConsole.Case("значение с ';'", () =>
        {
            var d = new DictInt(); d[1] = "a;b"; return d.ToString();
        }, "1=a\\;b");
    }

    private static void GetValueOrKeyCheck()
    {
        DemoConsole.Note("GetValueOrKey(int)");
        var d = new DictInt("1=one;2=two");
        DemoConsole.Case("существующий ключ", () => d.GetValueOrKey(1), "one");
        DemoConsole.Case("отсутствующий ключ → строка ключа", () => d.GetValueOrKey(999), "999");
        DemoConsole.Case("int.MaxValue → строка ключа", () => d.GetValueOrKey(int.MaxValue), int.MaxValue.ToString());
    }
}