using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class DictStringDemo
{
    public static Task Run()
    {
        DemoConsole.Part("DictString : _Dict_Proto<string, string>");
        Ctor();
        Keys();
        Values();
        RoundTrip();
        Duplicates();
        EscapeBug();
        return Task.CompletedTask;
    }

    private static void Ctor()
    {
        DemoConsole.Note("ctors");
        DemoConsole.Case("new → 0", () => new DictString().Count, 0);
        DemoConsole.Case("null(string) → 0", () => new DictString((string?)null).Count, 0);
        DemoConsole.Case("\"\" → 0", () => new DictString("").Count, 0);
        DemoConsole.Case("\"a=1;b=2\" → 2", () => new DictString("a=1;b=2").Count, 2);
        DemoConsole.Case("params \"a=1\",\"b=2\"", () => new DictString("a=1", "b=2").Count, 2);
        DemoConsole.Case("IEnumerable", () => new DictString(new[] { "a=1", "b=2" }).Count, 2);
    }

    private static void Keys()
    {
        DemoConsole.Note("Границы ключа string");
        DemoConsole.Case("ключ-пустая строка =v", () => new DictString("=v")[""], "v");
        DemoConsole.Case("ключ-юникод", () => new DictString("привет=v")["привет"], "v");
        DemoConsole.Case("ключ с пробелом внутри", () => new DictString("a b=v")["a b"], "v");
        DemoConsole.Case("ключ с '=' в значении", () => new DictString("a=b=c")["a"], "b=c");
    }

    private static void Values()
    {
        DemoConsole.Note("Границы значения string");
        DemoConsole.Case("пустое значение a=", () => new DictString("a=")["a"], "");
        DemoConsole.Case("длинное значение", () =>
            new DictString("a=" + new string('x', 1000))["a"].Length, 1000);
        DemoConsole.Case("юникод", () => new DictString("k=привет🎉")["k"], "привет🎉");
    }

    private static void RoundTrip()
    {
        DemoConsole.Note("Round-trip string → DictString → string");
        const string src = "a=1;b=2;c=3";
        DemoConsole.Case("ToString сохраняет", () => new DictString(src).ToString(), src);
        DemoConsole.Case("двойной прогон", () =>
        {
            var once = new DictString(src).ToString();
            var twice = new DictString(once).ToString();
            return once == twice;
        }, true);
    }

    private static void Duplicates()
    {
        DemoConsole.Note("Дубликат ключа в сериализации → ArgumentException (Dictionary.Add)");
        DemoConsole.CaseThrows<ArgumentException>("\"a=1;a=2\"", () => _ = new DictString("a=1;a=2"));
    }

    private static void EscapeBug()
    {
        DemoConsole.Note("Наследует баг base: Split('=') не уважает '\\=' в ключе");
        DemoConsole.Case("a\\=b=v — ключ становится 'a\\', а не 'a=b'", () =>
        {
            var d = new DictString();
            d.Add("a\\=b=v");
            return d.ContainsKey("a\\");
        }, true);
    }
}