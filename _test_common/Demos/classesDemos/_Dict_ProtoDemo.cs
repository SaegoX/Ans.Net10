using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

/// <summary>Демка абстрактного _Dict_Proto&lt;TKey, TValue&gt;.</summary>
public static class _Dict_ProtoDemo
{
    /// <summary>Конкретная реализация: string→string, конверсии — тождественные.</summary>
    private sealed class StringDict : _Dict_Proto<string, string>
    {

        public StringDict() { }
        public StringDict(IEnumerable<string>? s) : base(s) { }
        public StringDict(string? s) : base(s) { }

        public override string StringToKey(string key) => key;
        public override string StringToValue(string value) => value;
        public override string KeyToString(string key) => key;
        public override string ValueToString(string value) => value;

    }

    public static Task Run()
    {

        DemoConsole.Part("_Dict_Proto<TKey, TValue>  (StringDict : string→string)");
        CtorEmpty();
        CtorFromEnumerable();
        CtorFromString();
        CtorFromStringEscaped();
        AddSingle();
        AddEdges();
        AddItemsMass();
        ToStringSimple();
        ToStringEscaping();
        RoundTrip();
        GetValueOrKey();
        return Task.CompletedTask;

    }

    private static void CtorEmpty()
    {

        DemoConsole.Note("ctors — пустой");
        DemoConsole.Case("new → Count",         () => new StringDict().Count, 0);
        DemoConsole.Case("new → ToString()",    () => new StringDict().ToString(), "");

    }

    private static void CtorFromEnumerable()
    {

        DemoConsole.Note("ctor(IEnumerable<string>)");
        DemoConsole.Case("null → пусто",        () => new StringDict((IEnumerable<string>?)null).Count, 0);
        DemoConsole.Case("[] → пусто",          () => new StringDict(Array.Empty<string>()).Count, 0);
        DemoConsole.Case("[a=1] → 1",           () => new StringDict(new[] { "a=1" }).Count, 1);
        DemoConsole.Case("[a=1,b=2,c=3] → 3",   () => new StringDict(new[] { "a=1", "b=2", "c=3" }).Count, 3);
        DemoConsole.Case("мусор без '=' игнор", () => new StringDict(new[] { "a=1", "мусор" }).Count, 1);

    }

    private static void CtorFromString()
    {

        DemoConsole.Note("ctor(string)");
        DemoConsole.Case("null → пусто",        () => new StringDict((string?)null).Count, 0);
        DemoConsole.Case("\"\" → пусто",        () => new StringDict("").Count, 0);
        DemoConsole.Case("\"a=1\" → a=1",       () => new StringDict("a=1")["a"], "1");
        DemoConsole.Case("\"a=1;b=2;c=3\" → 3", () => new StringDict("a=1;b=2;c=3").Count, 3);

    }

    private static void CtorFromStringEscaped()
    {

        DemoConsole.Note("ctor(string) — экранированные ';'");
        var d = new StringDict("a=1\\;2;b=3");
        DemoConsole.Case("Count",       () => d.Count, 2);
        DemoConsole.Case("d[\"a\"]",    () => d["a"], "1;2");
        DemoConsole.Case("d[\"b\"]",    () => d["b"], "3");

    }

    private static void AddSingle()
    {

        DemoConsole.Note("Add(string) — обычный");
        DemoConsole.Case("Add(\"a=1\")", () => { var d = new StringDict(); d.Add("a=1"); return d["a"]; }, "1");
        DemoConsole.Case("Add(\"k=\") → пустое знач", () => { var d = new StringDict(); d.Add("k="); return d["k"]; }, "");
        DemoConsole.Case("Add(\"=v\") → пустой ключ", () => { var d = new StringDict(); d.Add("=v"); return d[""]; }, "v");

    }

    private static void AddEdges()
    {
        DemoConsole.Note("Add(string) — краевые");

        DemoConsole.Case("Add(null) → 0", () => { var d = new StringDict(); d.Add(null!); return d.Count; }, 0);
        DemoConsole.Case("Add(\"\") → 0", () => { var d = new StringDict(); d.Add(""); return d.Count; }, 0);
        DemoConsole.Case("Add(\"abc\") без '=' → 0", () => { var d = new StringDict(); d.Add("abc"); return d.Count; }, 0);
        DemoConsole.Case("Add(\"a=b=c\") → key='a', val='b=c'",
            () => { var d = new StringDict(); d.Add("a=b=c"); return d["a"]; }, "b=c");
        DemoConsole.Case("Add(\"a=v\\\\;x\") → val 'v;x'",
            () => { var d = new StringDict(); d.Add("a=v\\;x"); return d["a"]; }, "v;x");

        DemoConsole.Note("Известная особенность Add: Split('=') не рассматривает '\\=', " +
                         "поэтому экранирование '=' в ключе не работает (баг библиотеки)");
        DemoConsole.Case("Add(\"a\\\\=b=v\") — ключ становится 'a\\', а не 'a=b'",
            () => { var d = new StringDict(); d.Add("a\\=b=v"); return d.ContainsKey("a\\"); }, true);
    }

    private static void AddItemsMass()
    {

        DemoConsole.Note("AddItems(IEnumerable<string>)");
        DemoConsole.Case("3 пары",          () => { var d = new StringDict(); d.AddItems(new[] { "a=1", "b=2", "c=3" }); return d.Count; }, 3);

    }

    private static void ToStringSimple()
    {

        DemoConsole.Note("ToString() — обычный");
        DemoConsole.Case("пустой → \"\"",   () => new StringDict().ToString(), "");
        DemoConsole.Case("a=1 → \"a=1\"",   () => { var d = new StringDict(); d["a"] = "1"; return d.ToString(); }, "a=1");
        DemoConsole.Case("3 пары → с ';'",  () => { var d = new StringDict(); d["a"] = "1"; d["b"] = "2"; d["c"] = "3"; return d.ToString(); }, "a=1;b=2;c=3");

    }

    private static void ToStringEscaping()
    {

        DemoConsole.Note("ToString() — экранирование '=' и ';'");
        DemoConsole.Case("'=' в значении",  () => { var d = new StringDict(); d["k"] = "a=b"; return d.ToString(); }, "k=a\\=b");
        DemoConsole.Case("';' в значении",  () => { var d = new StringDict(); d["k"] = "a;b"; return d.ToString(); }, "k=a\\;b");
        DemoConsole.Case("'=' в ключе",     () => { var d = new StringDict(); d["a=b"] = "v"; return d.ToString(); }, "a\\=b=v");

    }

    private static void RoundTrip()
    {

        DemoConsole.Note("Round-trip: parse → ToString → parse");
        const string src = "user=vasya;role=admin\\;super;pwd=a\\=b";
        DemoConsole.Case("исходная строка",         () => new StringDict(src).ToString(), src);
        DemoConsole.Case("двойной прогон стабилен", () =>
        {

            var once = new StringDict(src).ToString();
            var twice = new StringDict(once).ToString();
            return once == twice;

        }, true);

    }

    private static void GetValueOrKey()
    {

        DemoConsole.Note("GetValueOrKey(TKey)");
        var d = new StringDict("a=1;b=2");
        DemoConsole.Case("существующий ключ",   () => d.GetValueOrKey("a"), "1");
        DemoConsole.Case("отсутствующий ключ",  () => d.GetValueOrKey("zzz"), "zzz");
        DemoConsole.Case("null-ключ → default", () => d.GetValueOrKey(null!), null);

    }
}