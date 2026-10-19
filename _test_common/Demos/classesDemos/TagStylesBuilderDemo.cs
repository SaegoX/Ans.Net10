using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class TagStylesBuilderDemo
{
    public static Task Run()
    {
        DemoConsole.Part("TagStylesBuilder");
        Ctor();
        AppendString();
        AppendDict();
        AppendIf();
        ApplyOriginal();
        ToStringCheck();
        ParseBugs();
        return Task.CompletedTask;
    }

    private static void Ctor()
    {
        DemoConsole.Note("ctors");
        DemoConsole.Case("new → пусто", () => new TagStylesBuilder().Items.Count, 0);
        DemoConsole.Case("new(null) → пусто", () => new TagStylesBuilder(null).Items.Count, 0);
        DemoConsole.Case("new(\"\") → пусто", () => new TagStylesBuilder("").Items.Count, 0);
        DemoConsole.Case("new(\"   \") → пусто", () => new TagStylesBuilder("   ").Items.Count, 0);
        DemoConsole.Case("new(\"a:1\") → 1", () => new TagStylesBuilder("a:1").Items.Count, 1);
        DemoConsole.Case("new(\"a:1;b:2\") → 2", () => new TagStylesBuilder("a:1;b:2").Items.Count, 2);
    }

    private static void AppendString()
    {
        DemoConsole.Note("Append(string) — перезаписывает");
        DemoConsole.Case("Append(\"a:1\")", () =>
        {
            var b = new TagStylesBuilder();
            b.Append("a:1");
            return b.Items["a"];
        }, "1");
        DemoConsole.Case("Append(null) no-op", () =>
        {
            var b = new TagStylesBuilder();
            b.Append((string?)null);
            return b.Items.Count;
        }, 0);
        DemoConsole.Case("Append(\"\") no-op", () =>
        {
            var b = new TagStylesBuilder();
            b.Append("");
            return b.Items.Count;
        }, 0);
        DemoConsole.Case("Append перезаписывает существующее", () =>
        {
            var b = new TagStylesBuilder("a:1");
            b.Append("a:2");
            return b.Items["a"];
        }, "2");
    }

    private static void AppendDict()
    {
        DemoConsole.Note("Append(Dictionary)");
        DemoConsole.Case("Append(dict) добавляет", () =>
        {
            var b = new TagStylesBuilder();
            b.Append(new Dictionary<string, string> { ["a"] = "1", ["b"] = "2" });
            return b.Items.Count;
        }, 2);
        DemoConsole.Case("Append((dict)null) no-op", () =>
        {
            var b = new TagStylesBuilder();
            b.Append((Dictionary<string, string>?)null);
            return b.Items.Count;
        }, 0);
    }

    private static void AppendIf()
    {
        DemoConsole.Note("AppendIf(check, styles)");
        DemoConsole.Case("true → добавляет", () =>
        {
            var b = new TagStylesBuilder();
            b.AppendIf(true, "a:1");
            return b.Items.ContainsKey("a");
        }, true);
        DemoConsole.Case("false → не добавляет", () =>
        {
            var b = new TagStylesBuilder();
            b.AppendIf(false, "a:1");
            return b.Items.ContainsKey("a");
        }, false);
    }

    private static void ApplyOriginal()
    {
        DemoConsole.Note("ApplyOriginal — НЕ перезаписывает существующие");
        DemoConsole.Case("string: color остаётся '1', padding добавляется '3'", () =>
        {
            var b = new TagStylesBuilder("a:1");
            b.ApplyOriginal("a:2;b:3");
            return $"{b.Items["a"]}|{b.Items["b"]}";
        }, "1|3");
        DemoConsole.Case("string: null no-op", () =>
        {
            var b = new TagStylesBuilder();
            b.ApplyOriginal((string?)null);
            return b.Items.Count;
        }, 0);
        DemoConsole.Case("dict: не перезаписать", () =>
        {
            var b = new TagStylesBuilder("a:1");
            b.ApplyOriginal(new Dictionary<string, string> { ["a"] = "2", ["b"] = "3" });
            return $"{b.Items["a"]}|{b.Items["b"]}";
        }, "1|3");
        DemoConsole.Case("dict: null no-op", () =>
        {
            var b = new TagStylesBuilder();
            b.ApplyOriginal((Dictionary<string, string>?)null);
            return b.Items.Count;
        }, 0);
    }

    private static void ToStringCheck()
    {
        DemoConsole.Note("ToString — с завершающей ';'");
        DemoConsole.Case("пустой → \"\"", () => new TagStylesBuilder().ToString(), "");
        DemoConsole.Case("один стиль", () => new TagStylesBuilder("a:1").ToString(), "a:1;");
        DemoConsole.Case("два стиля", () => new TagStylesBuilder("a:1;b:2").ToString(), "a:1;b:2;");
    }

    private static void ParseBugs()
    {
        DemoConsole.Note("Баг: ':' в значении теряется (Split(':') без limit)");
        DemoConsole.Case("background:url(http://x) → теряется", () =>
        {
            var b = new TagStylesBuilder("background:url(http://x)");
            return b.Items.Count;
        }, 0);

        DemoConsole.Note("Пустые сегменты ';;' игнорируются (RemoveEmptyEntries)");
        DemoConsole.Case("a:1;;b:2;", () =>
        {
            var b = new TagStylesBuilder("a:1;;b:2;");
            return b.Items.Count;
        }, 2);

        DemoConsole.Note("TrimEntries — пробелы по краям у ключей и значений");
        DemoConsole.Case("  a  :  1  → 1", () =>
        {
            var b = new TagStylesBuilder("  a  :  1  ");
            return b.Items["a"];
        }, "1");

        DemoConsole.Note("Сегмент без ':' игнорируется");
        DemoConsole.Case("garbage → 0", () =>
        {
            var b = new TagStylesBuilder("garbage");
            return b.Items.Count;
        }, 0);

        DemoConsole.Note("Второе ':' в значении → parts.Length != 2 → пропуск");
        DemoConsole.Case("a:1:2 → пропуск", () =>
        {
            var b = new TagStylesBuilder("a:1:2");
            return b.Items.Count;
        }, 0);
    }
}