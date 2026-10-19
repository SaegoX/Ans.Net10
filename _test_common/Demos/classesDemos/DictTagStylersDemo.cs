using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class DictTagStylersDemo
{
    public static Task Run()
    {
        DemoConsole.Part("DictTagStylers : Dictionary<string, TagStyler>");
        Ctor();
        Duplicates();
        SegmentCounts();
        NestedStylerWorks();
        return Task.CompletedTask;
    }

    private static void Ctor()
    {
        DemoConsole.Note("ctors и null/empty");
        DemoConsole.Case("null → пусто", () => new DictTagStylers(null!).Count, 0);
        DemoConsole.Case("без аргументов → пусто", () => new DictTagStylers().Count, 0);
        DemoConsole.Case("1 валидная строка", () =>
            new DictTagStylers("a|cls|color:red").ContainsKey("a"), true);
        DemoConsole.Case("2 строки", () =>
            new DictTagStylers("a|cls|color:red", "b|cls2|x:y").Count, 2);
        DemoConsole.Case("пустые строки игнор", () =>
            new DictTagStylers("a|cls|color:red", "", "   ", "b|cls2|x:y").Count, 2);
    }

    private static void Duplicates()
    {
        DemoConsole.Note("Дубликат ключа игнорируется (не исключение — так задумано)");
        DemoConsole.Case("a дважды → 1 элемент", () =>
            new DictTagStylers("a|cls1|x:1", "a|cls2|x:2").Count, 1);
        DemoConsole.Case("первое значение сохраняется", () =>
            new DictTagStylers("a|cls1|x:1", "a|cls2|x:2")["a"].Styles.Items["x"], "1");
    }

    private static void SegmentCounts()
    {
        DemoConsole.Note("SplitFix(\"|\", 3) — что при разном числе сегментов");
        DemoConsole.Case("3 сегмента — ок", () =>
            new DictTagStylers("a|cls|color:red").ContainsKey("a"), true);

        DemoConsole.Note("Меньше 3 сегментов — зависит от SplitFix; TODO: проверить с его исходником");
        DemoConsole.Case("2 сегмента (a|cls) — не падает?", () =>
        {
            try { var d = new DictTagStylers("a|cls"); return d.ContainsKey("a"); }
            catch { return false; }
        });
    }

    private static void NestedStylerWorks()
    {
        DemoConsole.Note("Вложенный TagStyler создаётся корректно");
        DemoConsole.Case("стили распарсились", () =>
            new DictTagStylers("a|cls1 cls2|color:red;padding:1px")["a"].Styles.Items.Count, 2);
        DemoConsole.Case("значение color", () =>
            new DictTagStylers("a|cls1|color:red")["a"].Styles.Items["color"], "red");
    }
}