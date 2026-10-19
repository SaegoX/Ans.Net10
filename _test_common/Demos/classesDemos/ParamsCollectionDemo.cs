using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class ParamsCollectionDemo
{
    public static Task Run()
    {
        DemoConsole.Part("ParamsCollection");
        Empty();
        AppendString();
        AppendBool();
        AppendNumbers();
        AppendDateTimes();
        ToStringCheck();
        return Task.CompletedTask;
    }

    private static void Empty()
    {
        DemoConsole.Note("Пустая коллекция");
        DemoConsole.Case("Items.Count == 0", () => new ParamsCollection().Items.Count, 0);
        DemoConsole.Case("ToString() == \"\"", () => new ParamsCollection().ToString(), "");
    }

    private static void AppendString()
    {
        DemoConsole.Note("Append(string, string) — БЕЗ фильтрации");
        DemoConsole.Case("k=v → 1", () =>
        {
            var p = new ParamsCollection(); p.Append("k", "v");
            return p.Items.Count;
        }, 1);
        DemoConsole.Case("k=\"\" — сохраняется", () =>
        {
            var p = new ParamsCollection(); p.Append("k", "");
            return p.Items["k"];
        }, "");
        DemoConsole.Case("k=null — сохраняется", () =>
        {
            var p = new ParamsCollection(); p.Append("k", (string)null!);
            return p.Items["k"];
        }, (string?)null);
    }

    private static void AppendBool()
    {
        DemoConsole.Note("Append(bool): true → \"1\", false → нет записи");
        DemoConsole.Case("true → Items[k]==\"1\"", () =>
        {
            var p = new ParamsCollection(); p.Append("k", true);
            return p.Items["k"];
        }, "1");
        DemoConsole.Case("false → 0 записей", () =>
        {
            var p = new ParamsCollection(); p.Append("k", false);
            return p.Items.Count;
        }, 0);
    }

    private static void AppendNumbers()
    {
        DemoConsole.Note("Append(int/long/double/float/decimal): 0 → не добавляет");
        DemoConsole.Case("int 42 → \"42\"", () =>
        {
            var p = new ParamsCollection(); p.Append("k", 42);
            return p.Items["k"];
        }, "42");
        DemoConsole.Case("int 0 → 0 записей", () =>
        {
            var p = new ParamsCollection(); p.Append("k", 0);
            return p.Items.Count;
        }, 0);
        DemoConsole.Case("double 1.5 → \"1,5\" (ru-RU!)", () =>
        {
            var p = new ParamsCollection(); p.Append("k", 1.5);
            return p.Items["k"];
        });
        DemoConsole.Case("double 0.0 → 0 записей", () =>
        {
            var p = new ParamsCollection(); p.Append("k", 0.0);
            return p.Items.Count;
        }, 0);
    }

    private static void AppendDateTimes()
    {
        DemoConsole.Note("Append(DateTime?): null → нет, иначе формат \"u\"");
        DemoConsole.Case("null → 0 записей", () =>
        {
            var p = new ParamsCollection(); p.Append("k", (DateTime?)null);
            return p.Items.Count;
        }, 0);

        DemoConsole.Case("2024-01-01 12:00:00 → \"2024-01-01 12:00:00Z\"", () =>
        {
            var p = new ParamsCollection(); p.Append("k", new DateTime(2024, 1, 1, 12, 0, 0));
            return p.Items["k"];
        }, "2024-01-01 12:00:00Z");

        DemoConsole.Note("DateOnly → \"yyyy-MM-dd\"");
        DemoConsole.Case("2024-01-01", () =>
        {
            var p = new ParamsCollection(); p.Append("k", (DateOnly?)new DateOnly(2024, 1, 1));
            return p.Items["k"];
        }, "2024-01-01");

        DemoConsole.Note("TimeOnly → \"HH:mm:ss\"");
        DemoConsole.Case("14:30:00", () =>
        {
            var p = new ParamsCollection(); p.Append("k", (TimeOnly?)new TimeOnly(14, 30, 0));
            return p.Items["k"];
        }, "14:30:00");
    }

    private static void ToStringCheck()
    {
        DemoConsole.Note("ToString — префикс '?', разделитель '&'");
        DemoConsole.Case("пустой → \"\"", () => new ParamsCollection().ToString(), "");
        DemoConsole.Case("один", () =>
        {
            var p = new ParamsCollection(); p.Append("a", "1");
            return p.ToString();
        }, "?a=1");
        DemoConsole.Case("два", () =>
        {
            var p = new ParamsCollection(); p.Append("a", "1"); p.Append("b", "2");
            return p.ToString();
        }, "?a=1&b=2");

        DemoConsole.Note("БАГ/особенность: нет URL-encoding — пробелы и '&', '=' в значениях ломают строку");
        DemoConsole.Case("k=\"hello world\" — пробел НЕ кодируется", () =>
        {
            var p = new ParamsCollection(); p.Append("k", "hello world");
            return p.ToString();
        }, "?k=hello world");
    }
}