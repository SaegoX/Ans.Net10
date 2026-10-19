using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class TagAttributesBuilderDemo
{
    public static Task Run()
    {
        DemoConsole.Part("TagAttributesBuilder");
        Ctor();
        AppendBasic();
        AppendFlag();
        AppendQuoted();
        AppendEdgeCases();
        AppendIfCheck();
        ToStringCheck();
        return Task.CompletedTask;
    }

    private static void Ctor()
    {
        DemoConsole.Note("ctors");
        DemoConsole.Case("new → пусто", () => new TagAttributesBuilder().Items.Count, 0);
        DemoConsole.Case("new(null) → пусто", () => new TagAttributesBuilder(null).Items.Count, 0);
        DemoConsole.Case("new(\"\") → пусто", () => new TagAttributesBuilder("").Items.Count, 0);
        DemoConsole.Case("new(\"   \") → пусто", () => new TagAttributesBuilder("   ").Items.Count, 0);
    }

    private static void AppendBasic()
    {
        DemoConsole.Note("Append — k=\"v\"");
        DemoConsole.Case("id=\"main\"", () =>
        {
            var b = new TagAttributesBuilder("id=\"main\"");
            return b.Items["id"];
        }, "main");

        DemoConsole.Case("a=\"1\" b=\"2\" → 2", () =>
        {
            var b = new TagAttributesBuilder("a=\"1\" b=\"2\"");
            return $"{b.Items["a"]}|{b.Items["b"]}";
        }, "1|2");

        DemoConsole.Case("Append(null) no-op", () =>
        {
            var b = new TagAttributesBuilder();
            b.Append(null);
            return b.Items.Count;
        }, 0);
    }

    private static void AppendFlag()
    {
        DemoConsole.Note("Flag-атрибут без '=' → value = null");
        DemoConsole.Case("disabled", () =>
        {
            var b = new TagAttributesBuilder("disabled");
            return (b.Items.ContainsKey("disabled"), b.Items["disabled"]);
        }, (true, (string?)null));

        DemoConsole.Case("disabled checked → 2", () =>
        {
            var b = new TagAttributesBuilder("disabled checked");
            return b.Items.Count;
        }, 2);
    }

    private static void AppendQuoted()
    {
        DemoConsole.Note("Trim('\"') с обеих сторон");
        DemoConsole.Case("a=\"x\" → x", () => new TagAttributesBuilder("a=\"x\"").Items["a"], "x");
        DemoConsole.Case("a=x → x", () => new TagAttributesBuilder("a=x").Items["a"], "x");
        DemoConsole.Case("a='x' → 'x' (одинарные НЕ снимаются)", () =>
            new TagAttributesBuilder("a='x'").Items["a"], "'x'");
    }

    private static void AppendEdgeCases()
    {
        DemoConsole.Note("Edge: несколько '='");
        DemoConsole.Case("a=b=c → key=\"a=b\", val=\"c\" (LastIndexOf)", () =>
        {
            var b = new TagAttributesBuilder("a=b=c");
            return (b.Items.ContainsKey("a=b"), b.Items["a=b"]);
        }, (true, "c"));

        DemoConsole.Note("Ограничение: значения с пробелами не поддерживаются — Split(' ') раньше '='");
        DemoConsole.Case("a=\"x y\" → 2 записи", () =>
            new TagAttributesBuilder("a=\"x y\"").Items.Count, 2);
    }

    private static void AppendIfCheck()
    {
        DemoConsole.Note("AppendIf(check, ser)");
        DemoConsole.Case("true → добавляет", () =>
        {
            var b = new TagAttributesBuilder();
            b.AppendIf(true, "a=1");
            return b.Items.ContainsKey("a");
        }, true);

        DemoConsole.Case("false → нет", () =>
        {
            var b = new TagAttributesBuilder();
            b.AppendIf(false, "a=1");
            return b.Items.ContainsKey("a");
        }, false);
    }

    private static void ToStringCheck()
    {
        DemoConsole.Note("ToString — с ведущим пробелом; flag без '=...'");
        DemoConsole.Case("пустой → \"\"", () => new TagAttributesBuilder().ToString(), "");

        DemoConsole.Case("a=1 → \" a=\\\"1\\\"\"", () =>
        {
            var b = new TagAttributesBuilder();
            b.Append("a=1");
            return b.ToString();
        }, " a=\"1\"");

        DemoConsole.Case("disabled → \" disabled\"", () =>
        {
            var b = new TagAttributesBuilder();
            b.Append("disabled");
            return b.ToString();
        }, " disabled");

        DemoConsole.Case("a=1 disabled — содержит оба", () =>
        {
            var b = new TagAttributesBuilder();
            b.Append("a=1 disabled");
            var s = b.ToString();
            return s.Contains("a=\"1\"") && s.Contains("disabled");
        }, true);
    }
}