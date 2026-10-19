using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class TagStylerDemo
{
    public static Task Run()
    {
        DemoConsole.Part("TagStyler");
        CtorNull();
        CtorStrings();
        CtorEmpty();
        ApplyBase();
        return Task.CompletedTask;
    }

    private static void CtorNull()
    {
        DemoConsole.Note("ctor с null-вложенными — создаются пустые");
        DemoConsole.Case("(TagClassesBuilder?)null, null → оба созданы", () =>
        {
            var s = new TagStyler((TagClassesBuilder?)null, null);
            return s.Classes is not null && s.Styles is not null;
        }, true);
        DemoConsole.Case("Styles.Items пуст", () =>
            new TagStyler((TagClassesBuilder?)null, null).Styles.Items.Count, 0);
    }

    private static void CtorStrings()
    {
        DemoConsole.Note("ctor(string, string) — через строки");
        DemoConsole.Case("пустые строки → пусто", () =>
        {
            var s = new TagStyler("", "");
            return s.Styles.Items.Count;
        }, 0);

        DemoConsole.Case("стили распарсились", () =>
            new TagStyler("c1 c2", "color:red;padding:1px").Styles.Items.Count, 2);

        DemoConsole.Case("CSS-классы есть (Items.Count > 0)", () =>
        {
            var s = new TagStyler("c1 c2", "");
            return s.Classes.Items.Count > 0;
        }, true);

        DemoConsole.Case("null стили — без исключения", () =>
            new TagStyler("c1", null).Styles.Items.Count, 0);
    }

    private static void CtorEmpty()
    {
        DemoConsole.Note("ctor() — оба пустых");
        DemoConsole.Case("Classes и Styles созданы", () =>
        {
            var s = new TagStyler();
            return s.Classes is not null && s.Styles is not null;
        }, true);
        DemoConsole.Case("Styles.Items.Count == 0", () =>
            new TagStyler().Styles.Items.Count, 0);
    }

    private static void ApplyBase()
    {
        DemoConsole.Note("ApplyBase — не перезаписывает текущее");
        DemoConsole.Case("own 'color' сохраняется, 'padding' донора добавляется", () =>
        {
            var baseStyler = new TagStyler("base-c", "color:red;padding:1px");
            var own = new TagStyler("own-c", "color:blue");
            own.ApplyBase(baseStyler);
            return $"color={own.Styles.Items["color"]}|padding={own.Styles.Items["padding"]}";
        }, "color=blue|padding=1px");

        DemoConsole.Note("Если у TagClassesBuilder нет 'Items' — компилятор подскажет, скажи — поправим");
    }
}