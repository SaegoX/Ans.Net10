using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class TypografHelperDemo
{
    public static Task Run()
    {
        DemoConsole.Part("TypografHelper");
        CtorGuard();
        EmptySource();
        TagsPreserved();
        PlainText();
        UnclosedTags();
        return Task.CompletedTask;
    }

    private static void CtorGuard()
    {
        DemoConsole.Note("ctor — null guard");
        DemoConsole.CaseThrows<ArgumentNullException>("null", () => _ = new TypografHelper(null!));
    }

    private static void EmptySource()
    {
        DemoConsole.Note("пустая строка");
        var t = new TypografHelper("");
        DemoConsole.Case("Source == \"\"", () => t.Source, "");
        DemoConsole.Case("Result == \"\"", () => t.Result, "");
    }

    private static void TagsPreserved()
    {
        DemoConsole.Note("HTML-теги сохраняются как есть (Typograf их не трогает)");
        var t = new TypografHelper("<p>hello</p>");
        DemoConsole.Case("Result содержит \"<p>\"", () => t.Result.Contains("<p>"), true);
        DemoConsole.Case("Result содержит \"</p>\"", () => t.Result.Contains("</p>"), true);
        DemoConsole.Case("Result содержит \"hello\"", () => t.Result.Contains("hello"), true);
        DemoConsole.Case("Source == Result.Source", () => t.Source, "<p>hello</p>");

        var t2 = new TypografHelper("<div><span>a</span><span>b</span></div>");
        DemoConsole.Case("несколько тегов сохранены", () =>
            t2.Result.Contains("<div>") && t2.Result.Contains("<span>"), true);
    }

    private static void PlainText()
    {
        DemoConsole.Note("Чистый текст без тегов — проходит через GetTypografElem");
        var t = new TypografHelper("Hello, world!");
        DemoConsole.Case("Source сохранён", () => t.Source, "Hello, world!");
        DemoConsole.Case("Result не null", () => t.Result is not null, true);
        DemoConsole.Note("TODO: точные типографские замены зависят от SuppTypograph.GetTypografElem " +
                         "(кавычки, тире, неразрывные пробелы) — проверить отдельно при наличии его исходника");
    }

    private static void UnclosedTags()
    {
        DemoConsole.Note("'<тег' без '>' — добавляется как есть, без типографики");
        var t = new TypografHelper("text <broken");
        DemoConsole.Case("Result содержит \"<broken\"", () => t.Result.Contains("<broken"), true);
        DemoConsole.Case("Result не падает", () => t.Result is not null, true);

        DemoConsole.Note("Тег без закрытия: \"before<p>\" — тег сохраняется");
        var t2 = new TypografHelper("before<p>");
        DemoConsole.Case("Result содержит \"<p>\"", () => t2.Result.Contains("<p>"), true);
    }
}