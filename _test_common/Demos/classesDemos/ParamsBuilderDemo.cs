using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class ParamsBuilderDemo
{
    public static Task Run()
    {
        DemoConsole.Part("ParamsBuilder");
        Basic();
        GetStringTemporary();
        AppendOverloads();
        return Task.CompletedTask;
    }

    private static void Basic()
    {
        DemoConsole.Note("Append + ToString");
        DemoConsole.Case("пустой → \"\"", () => new ParamsBuilder().ToString(), "");
        DemoConsole.Case("один", () =>
        {
            var b = new ParamsBuilder(); b.Append("a", "1");
            return b.ToString();
        }, "?a=1");
        DemoConsole.Case("два", () =>
        {
            var b = new ParamsBuilder(); b.Append("a", "1"); b.Append("b", "2");
            return b.ToString();
        }, "?a=1&b=2");
    }

    private static void GetStringTemporary()
    {
        DemoConsole.Note("GetString(name, value) — временно накладывает и НЕ мутирует Parameters");
        DemoConsole.Case("id=5 + временный mode=edit", () =>
        {
            var b = new ParamsBuilder();
            b.Append("id", 5);
            var s = b.GetString("mode", "edit");
            return s;
        }, "?id=5&mode=edit");

        DemoConsole.Case("GetString не меняет Parameters", () =>
        {
            var b = new ParamsBuilder();
            b.Append("id", 5);
            b.GetString("mode", "edit");
            return b.Parameters.Items.Count;
        }, 1);

        DemoConsole.Case("GetString(int) — page=2", () =>
        {
            var b = new ParamsBuilder();
            b.Append("q", "test");
            return b.GetString("page", 2);
        }, "?q=test&page=2");

        DemoConsole.Note("Переопределение: если key есть в Parameters — временный перезапишет");
        DemoConsole.Case("id=5, GetString(\"id\", 99) → ?id=99", () =>
        {
            var b = new ParamsBuilder();
            b.Append("id", 5);
            return b.GetString("id", 99);
        }, "?id=99");

        DemoConsole.Case("После GetString id в Parameters остаётся 5", () =>
        {
            var b = new ParamsBuilder();
            b.Append("id", 5);
            b.GetString("id", 99);
            return b.Parameters.Items["id"];
        }, "5");
    }

    private static void AppendOverloads()
    {
        DemoConsole.Note("Все перегрузки Append — делегируют в ParamsCollection (с фильтрацией)");
        DemoConsole.Case("bool true → \"?k=1\"", () =>
        {
            var b = new ParamsBuilder(); b.Append("k", true); return b.ToString();
        }, "?k=1");
        DemoConsole.Case("bool false → \"\"", () =>
        {
            var b = new ParamsBuilder(); b.Append("k", false); return b.ToString();
        }, "");
        DemoConsole.Case("int 0 → \"\"", () =>
        {
            var b = new ParamsBuilder(); b.Append("k", 0); return b.ToString();
        }, "");
        DemoConsole.Case("int 42 → \"?k=42\"", () =>
        {
            var b = new ParamsBuilder(); b.Append("k", 42); return b.ToString();
        }, "?k=42");
        DemoConsole.Case("DateTime null → \"\"", () =>
        {
            var b = new ParamsBuilder(); b.Append("k", (DateTime?)null); return b.ToString();
        }, "");
    }
}