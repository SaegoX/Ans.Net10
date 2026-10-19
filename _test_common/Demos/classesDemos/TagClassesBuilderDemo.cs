using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class TagClassesBuilderDemo
{
    public static Task Run()
    {
        DemoConsole.Part("TagClassesBuilder");
        Ctor();
        AppendString();
        AppendDict();
        ApplyOriginal();
        AppendIf();
        ParsingBoundaries();
        AggregationInString();
        ToStringCheck();
        return Task.CompletedTask;
    }

    private static void Ctor()
    {
        DemoConsole.Note("ctors");
        DemoConsole.Case("new → 0", () => new TagClassesBuilder().Items.Count, 0);
        DemoConsole.Case("new(null) → 0", () => new TagClassesBuilder(null).Items.Count, 0);
        DemoConsole.Case("new(\"\") → 0", () => new TagClassesBuilder("").Items.Count, 0);
        DemoConsole.Case("new(\"   \") → 0", () => new TagClassesBuilder("   ").Items.Count, 0);
        DemoConsole.Case("new(\"btn-primary\")", () => new TagClassesBuilder("btn-primary").Items["btn-"][0], "primary");
    }

    private static void AppendString()
    {
        DemoConsole.Note("Append(string) — перезаписывает");
        DemoConsole.Case("Append(null) no-op", () =>
        {
            var b = new TagClassesBuilder();
            b.Append((string?)null);
            return b.Items.Count;
        }, 0);
        DemoConsole.Case("Append(\"\") no-op", () =>
        {
            var b = new TagClassesBuilder();
            b.Append("");
            return b.Items.Count;
        }, 0);
        DemoConsole.Case("Append(\"a\") — ключ \"a\", суффикс \"\"", () =>
            new TagClassesBuilder("a").ToString(), "a");
        DemoConsole.Case("Append перезаписывает префикс", () =>
        {
            var b = new TagClassesBuilder("btn-primary");
            b.Append("btn-large");
            return b.Items["btn-"].Length;
        }, 1); // был [primary], стал [large], длина 1
    }

    private static void AppendDict()
    {
        DemoConsole.Note("Append(Dictionary)");
        DemoConsole.Case("Append(dict) добавляет", () =>
        {
            var b = new TagClassesBuilder();
            b.Append(new Dictionary<string, string[]> { ["btn-"] = ["primary"] });
            return b.Items.Count;
        }, 1);
        DemoConsole.Case("Append(dict) перезаписывает", () =>
        {
            var b = new TagClassesBuilder("btn-primary btn-lg");
            b.Append(new Dictionary<string, string[]> { ["btn-"] = ["xl"] });
            return b.Items["btn-"][0];
        }, "xl");
        DemoConsole.Case("Append(null) no-op", () =>
        {
            var b = new TagClassesBuilder();
            b.Append((Dictionary<string, string[]>?)null);
            return b.Items.Count;
        }, 0);
    }

    private static void ApplyOriginal()
    {
        DemoConsole.Note("ApplyOriginal — НЕ перезаписывает");
        DemoConsole.Case("string: btn- сохраняется, link- добавляется", () =>
        {
            var b = new TagClassesBuilder("btn-primary");
            b.ApplyOriginal("btn-large link-info");
            return $"{b.Items["btn-"][0]}|{b.Items["link-"][0]}";
        }, "primary|info");
        DemoConsole.Case("dict: не перезаписать", () =>
        {
            var b = new TagClassesBuilder("btn-primary");
            b.ApplyOriginal(new Dictionary<string, string[]> { ["btn-"] = ["other"] });
            return b.Items["btn-"][0];
        }, "primary");
        DemoConsole.Case("null no-op", () =>
        {
            var b = new TagClassesBuilder();
            b.ApplyOriginal((Dictionary<string, string[]>?)null);
            return b.Items.Count;
        }, 0);
    }

    private static void AppendIf()
    {
        DemoConsole.Note("AppendIf(check, classes)");
        DemoConsole.Case("true → добавляет", () =>
        {
            var b = new TagClassesBuilder();
            b.AppendIf(true, "btn-primary");
            return b.Items.ContainsKey("btn-");
        }, true);
        DemoConsole.Case("false → не добавляет", () =>
        {
            var b = new TagClassesBuilder();
            b.AppendIf(false, "btn-primary");
            return b.Items.ContainsKey("btn-");
        }, false);
    }

    private static void ParsingBoundaries()
    {
        DemoConsole.Note("_getParts — LastIndexOf('-')");

        DemoConsole.Case("простой: btn-primary", () =>
        {
            var b = new TagClassesBuilder("btn-primary");
            return b.Items["btn-"][0];
        }, "primary");

        DemoConsole.Case("два дефиса: btn-primary-lg → ключ 'btn-primary-'", () =>
        {
            var b = new TagClassesBuilder("btn-primary-lg");
            return b.Items.ContainsKey("btn-primary-") && b.Items["btn-primary-"][0] == "lg";
        }, true);

        DemoConsole.Case("без дефиса: 'btn' → ключ 'btn', суффикс ''", () =>
        {
            var b = new TagClassesBuilder("btn");
            return b.Items["btn"].Length == 1 && b.Items["btn"][0] == "";
        }, true);

        DemoConsole.Case("дефис в конце: 'btn-' → ключ 'btn-', суффикс ''", () =>
        {
            var b = new TagClassesBuilder("btn-");
            return b.Items["btn-"].Length == 1 && b.Items["btn-"][0] == "";
        }, true);

        DemoConsole.Case("дефис в начале: '-btn' → ключ '-', суффикс 'btn'", () =>
        {
            var b = new TagClassesBuilder("-btn");
            return b.Items["-"][0] == "btn";
        }, true);
    }

    private static void AggregationInString()
    {
        DemoConsole.Note("Один префикс в строке — агрегирует суффиксы в массив");
        DemoConsole.Case("btn-primary btn-lg → ['primary', 'lg']", () =>
        {
            var b = new TagClassesBuilder("btn-primary btn-lg");
            return string.Join(",", b.Items["btn-"]);
        }, "primary,lg");

        DemoConsole.Note("Но если Append'ом добавить ещё раз — массив ЗАМЕНИТСЯ, не дополнится");
        DemoConsole.Case("btn-primary btn-lg + Append(\"btn-xl\") → ['xl']", () =>
        {
            var b = new TagClassesBuilder("btn-primary btn-lg");
            b.Append("btn-xl");
            return string.Join(",", b.Items["btn-"]);
        }, "xl");
    }

    private static void ToStringCheck()
    {
        DemoConsole.Note("ToString — key+suffix через пробел, с завершающим ничем");
        DemoConsole.Case("пустой → \"\"", () => new TagClassesBuilder().ToString(), "");
        DemoConsole.Case("btn-primary", () => new TagClassesBuilder("btn-primary").ToString(), "btn-primary");
        DemoConsole.Case("btn-primary btn-lg", () => new TagClassesBuilder("btn-primary btn-lg").ToString(), "btn-primary btn-lg");
        DemoConsole.Case("btn-primary link-info", () =>
            new TagClassesBuilder("btn-primary link-info").ToString(), "btn-primary link-info");
        DemoConsole.Case("отдельный 'btn' (без дефиса)", () => new TagClassesBuilder("btn").ToString(), "btn");
    }
}