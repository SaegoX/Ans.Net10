using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class OrderBuilderDemo
{
    public static Task Run()
    {
        DemoConsole.Part("OrderItem / OrderBuilder");
        OrderItemChecks();
        OrderBuilderBasic();
        OrderBuilderSeparators();
        OrderBuilderNullEmpty();
        ToStringRoundTrip();
        GetLinqCode();
        return Task.CompletedTask;
    }

    private static void OrderItemChecks()
    {
        DemoConsole.Note("OrderItem(string) — границы");
        DemoConsole.Case("\"Name\" → asc", () =>
        {
            var i = new OrderItem("Name");
            return (i.Column, i.IsDescending);
        }, ("Name", false));

        DemoConsole.Case("\"-Age\" → desc", () =>
        {
            var i = new OrderItem("-Age");
            return (i.Column, i.IsDescending);
        }, ("Age", true));

        DemoConsole.Case("\"\" → пусто", () =>
        {
            var i = new OrderItem("");
            return (i.Column, i.IsDescending);
        }, ("", false));

        DemoConsole.Case("null → пусто", () =>
        {
            var i = new OrderItem(null!);
            return (i.Column, i.IsDescending);
        }, ("", false));

        DemoConsole.Case("\"-\" (только дефис) → Column \"\", Desc=true", () =>
        {
            var i = new OrderItem("-");
            return (i.Column, i.IsDescending);
        }, ("", true));

        DemoConsole.Case("\"--x\" (двойной) → Column \"-x\", Desc=true", () =>
        {
            var i = new OrderItem("--x");
            return (i.Column, i.IsDescending);
        }, ("-x", true));

        DemoConsole.Case("\" -x\" (пробел впереди) → Column \" -x\", Desc=false", () =>
        {
            var i = new OrderItem(" -x");
            return (i.Column, i.IsDescending);
        }, (" -x", false));
    }

    private static void OrderBuilderBasic()
    {
        DemoConsole.Note("OrderBuilder(string)");
        DemoConsole.Case("\"Name\" → 1", () => new OrderBuilder("Name").Items.Length, 1);
        DemoConsole.Case("\"Name,-Age\" → 2", () => new OrderBuilder("Name,-Age").Items.Length, 2);
        DemoConsole.Case("\"a,-b,c\" → 3", () => new OrderBuilder("a,-b,c").Items.Length, 3);
    }

    private static void OrderBuilderSeparators()
    {
        DemoConsole.Note("Разделители: ',' и ';'");
        DemoConsole.Case("\"a,b\" → 2", () => new OrderBuilder("a,b").Items.Length, 2);
        DemoConsole.Case("\"a;b\" → 2", () => new OrderBuilder("a;b").Items.Length, 2);
        DemoConsole.Case("\"a,,b\" → 2", () => new OrderBuilder("a,,b").Items.Length, 2);
        DemoConsole.Case("\"a;,;b\" → 2", () => new OrderBuilder("a;,;b").Items.Length, 2);
    }

    private static void OrderBuilderNullEmpty()
    {
        DemoConsole.Note("null / пустая строка");
        DemoConsole.Case("null → 0", () => new OrderBuilder(null!).Items.Length, 0);
        DemoConsole.Case("\"\" → 0", () => new OrderBuilder("").Items.Length, 0);
    }

    private static void ToStringRoundTrip()
    {
        DemoConsole.Note("ToString — через ','");
        DemoConsole.Case("пустой → \"\"", () => new OrderBuilder("").ToString(), "");
        DemoConsole.Case("a,-b,c", () => new OrderBuilder("a,-b,c").ToString(), "a,-b,c");
        DemoConsole.Case("round-trip стабилен", () =>
        {
            var once = new OrderBuilder("a,-b,c").ToString();
            var twice = new OrderBuilder(once).ToString();
            return once == twice;
        }, true);
    }

    private static void GetLinqCode()
    {
        DemoConsole.Note("GetLinqCode(offset)");
        DemoConsole.Case("пустой → \"\"", () =>
            new OrderBuilder("").GetLinqCode("  "), "");

        DemoConsole.Case("1 элемент asc", () =>
        {
            var code = new OrderBuilder("Name").GetLinqCode("  ");
            return code.Contains(".OrderBy(x => x.Name)") && !code.Contains("ThenBy");
        }, true);

        DemoConsole.Case("1 элемент desc", () =>
        {
            var code = new OrderBuilder("-Age").GetLinqCode("  ");
            return code.Contains(".OrderByDescending(x => x.Age)");
        }, true);

        DemoConsole.Case("2 элемента: OrderBy + ThenBy", () =>
        {
            var code = new OrderBuilder("Name,-Age").GetLinqCode("  ");
            return code.Contains("OrderBy(x => x.Name)") && code.Contains("ThenByDescending(x => x.Age)");
        }, true);
    }
}