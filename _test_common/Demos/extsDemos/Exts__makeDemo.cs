using _test_common.Framework;
using Ans.Net10.Common;
using System.ComponentModel;

namespace _test_common.Demos;

public static class Exts__makeDemo
{
    private enum Color
    {
        None,
        [Description("Красный цвет")] Red,
        Green
    }

    public static Task Run()
    {
        DemoConsole.Part("Exts__make");
        MakeString();
        MakeStringWithNullValue();
        MakeMore();
        MakeStruct();
        MakeNullableStruct();
        MakeEnum();
        MakeBool();
        MakeRepeatsChecks();
        MakeFromCollectionChecks();
        MakeOverDictChecks();
        MakeUrlWrapperChecks();
        return Task.CompletedTask;
    }

    private static void MakeString()
    {
        DemoConsole.Note("Make(string, template)");
        DemoConsole.Case("\"x\" + \"[{0}]\" → \"[x]\"", () => "x".Make("[{0}]"), "[x]");
        DemoConsole.Case("\"x\" + null → \"x\"", () => "x".Make(null), "x");
        DemoConsole.Case("null + \"[{0}]\" → \"\"", () => ((string?)null).Make("[{0}]"), "");
        DemoConsole.Case("\"\" + \"[{0}]\" → \"\"", () => "".Make("[{0}]"), "");
    }

    private static void MakeStringWithNullValue()
    {
        DemoConsole.Note("Make(string, template, nullValue)");
        DemoConsole.Case("\"x\"=\"N/A\", nullValue=\"N/A\" → \"\"", () =>
            "x".Make("[{0}]", "x"), "");
        DemoConsole.Case("\"y\" != \"x\", expected \"[y]\"", () =>
            "y".Make("[{0}]", "x"), "[y]");
        DemoConsole.Case("null == null → \"\"", () =>
            ((string?)null).Make("[{0}]", null), "");
    }

    private static void MakeMore()
    {
        DemoConsole.Note("MakeMore — value + доп. args");
        DemoConsole.Case("\"x\", \"{0}-{1}\", \"y\" → \"x-y\"", () =>
            "x".MakeMore("{0}-{1}", "y"), "x-y");
        DemoConsole.Case("null → \"\"", () =>
            ((string?)null).MakeMore("{0}"), "");
        DemoConsole.Case("null template → value", () =>
            "x".MakeMore(null), "x");
    }

    private static void MakeStruct()
    {
        DemoConsole.Note("Make<T> для struct (int) с nullValue");
        DemoConsole.Case("42 + \"[{0}]\" + nullValue=0 → \"[42]\"", () =>
            42.Make("[{0}]", null, 0), "[42]");
        DemoConsole.Case("0 + nullValue=0 → \"\"", () =>
            0.Make("[{0}]", null, 0), "");
        DemoConsole.Case("42 + format=\"X\" → \"[2A]\"", () =>
            42.Make("[{0}]", "X", 0), "[2A]");
    }

    private static void MakeNullableStruct()
    {
        DemoConsole.Note("Make<T> для nullable");
        DemoConsole.Case("42 (int?) + \"[{0}]\" → \"[42]\"", () =>
            ((int?)42).Make("[{0}]"), "[42]");
        DemoConsole.Case("null (int?) → \"\"", () =>
            ((int?)null).Make("[{0}]"), "");
    }

    private static void MakeEnum()
    {
        DemoConsole.Note("Make для enum с DescriptionAttribute");
        DemoConsole.Case("Color.Red → \"Красный цвет\" (из Description)", () =>
            Color.Red.Make("{0}"), "Красный цвет");
        DemoConsole.Case("Color.Green → \"Green\" (нет Description)", () =>
            Color.Green.Make("{0}"), "Green");
        DemoConsole.Case("Color.None + nullValue=None → \"\"", () =>
            Color.None.Make("{0}", Color.None), "");
        DemoConsole.Case("Color.Red + nullValue=None → \"Красный цвет\"", () =>
            Color.Red.Make("{0}", Color.None), "Красный цвет");
    }

    private static void MakeBool()
    {
        DemoConsole.Note("bool.Make(trueText, falseText)");
        DemoConsole.Case("true → \"yes\"", () => true.Make("yes", "no"), "yes");
        DemoConsole.Case("false → \"no\"", () => false.Make("yes", "no"), "no");
        DemoConsole.Case("false, нет falseText → \"\"", () => false.Make("yes"), "");
    }

    private static void MakeRepeatsChecks()
    {
        DemoConsole.Note("MakeRepeats(string, count, ...)");
        DemoConsole.Case("\"a\", 3 → \"aaa\"", () => "a".MakeRepeats(3), "aaa");
        DemoConsole.Case("\"a\", 1 → \"a\"", () => "a".MakeRepeats(1), "a");
        DemoConsole.Case("\"a\", 0 → \"\"", () => "a".MakeRepeats(0), "");
        DemoConsole.Case("\"a\", -1 → \"\"", () => "a".MakeRepeats(-1), "");
        DemoConsole.Case("null → \"\"", () => ((string?)null).MakeRepeats(3), "");
        DemoConsole.Case("\"a\", 3, separator=\",\" → \"a,a,a\"", () => "a".MakeRepeats(3, null, null, ","), "a,a,a");
        DemoConsole.Case("\"a\", 2, itemTemplate=\"[{0}]\" → \"[a][a]\"", () =>
            "a".MakeRepeats(2, null, "[{0}]", null), "[a][a]");
    }

    private static void MakeFromCollectionChecks()
    {
        DemoConsole.Note("MakeFromCollection — склейка через разделитель");
        DemoConsole.Case("[\"a\",\"b\",\"c\"], sep=\",\" → \"a,b,c\"", () =>
            new[] { "a", "b", "c" }.MakeFromCollection(null, null, ","), "a,b,c");
        DemoConsole.Case("[null,\"\",\"a\",\"b\"], sep=\",\" → \"a,b\"", () =>
            new[] { null, "", "a", "b" }.MakeFromCollection(null, null, ","), "a,b");
        DemoConsole.Case("null → \"\"", () =>
            ((IEnumerable<string>?)null).MakeFromCollection(), "");

        DemoConsole.Note("С itemExtractor (Func<T,string?>)");
        DemoConsole.Case("[1,2,3], sep=\"-\" → \"1-2-3\"", () =>
            new[] { 1, 2, 3 }.MakeFromCollection(x => x.ToString(), null, null, "-"), "1-2-3");
        DemoConsole.Case("пустой → \"\"", () =>
            Array.Empty<int>().MakeFromCollection(x => x.ToString(), null, null, "-"), "");
    }

    private static void MakeOverDictChecks()
    {
        DemoConsole.Note("MakeOverDict — по ключу из словаря, иначе — сам ключ");
        var dict = new Dictionary<string, string> { ["k"] = "VALUE" };
        DemoConsole.Case("k в словаре → \"VALUE\"", () => "k".MakeOverDict(dict, null), "VALUE");
        DemoConsole.Case("missing → сам ключ", () => "zzz".MakeOverDict(dict, null), "zzz");
        DemoConsole.Case("template \"[{0}]\" → \"[VALUE]\"", () =>
            "k".MakeOverDict(dict, "[{0}]"), "[VALUE]");
        DemoConsole.Case("template + missing → \"[zzz]\"", () =>
            "zzz".MakeOverDict(dict, "[{0}]"), "[zzz]");
        DemoConsole.Case("null key → \"\"", () => ((string?)null).MakeOverDict(dict, null), "");
    }

    private static void MakeUrlWrapperChecks()
    {
        DemoConsole.Note("Make_UrlWrapper — добавляет https:// если нет протокола");
        DemoConsole.Case("https://x → как есть", () => "https://x".Make_UrlWrapper(null), "https://x");
        DemoConsole.Case("http://x → как есть", () => "http://x".Make_UrlWrapper(null), "http://x");
        DemoConsole.Case("/path → как есть (relative)", () => "/path".Make_UrlWrapper(null), "/path");
        DemoConsole.Case("example.com → https://example.com", () =>
            "example.com".Make_UrlWrapper(null), "https://example.com");
        DemoConsole.Case("с шаблоном → <a href=\"...\">", () =>
            "example.com".Make_UrlWrapper("<a href=\"{0}\">link</a>"),
            "<a href=\"https://example.com\">link</a>");
        DemoConsole.Case("null → \"\"", () => ((string?)null).Make_UrlWrapper(null), "");
    }
}