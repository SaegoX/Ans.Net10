using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class Exts__toDemo
{
    public static Task Run()
    {
        DemoConsole.Part("Exts__to (ToGrid / ToIntArray / ToIntArrayTrim)");
        ToGridChecks();
        SeparatorsCheck();
        ToIntArrayFromArray();
        ToIntArrayFromString();
        ToIntArrayTrimFromString();
        return Task.CompletedTask;
    }

    private static void ToGridChecks()
    {
        DemoConsole.Note("ToGrid — Chunk по count");
        DemoConsole.Case("null source → 0 батчей", () => ((IEnumerable<int>?)null).ToGrid(3).Count(), 0);
        DemoConsole.Case("count=0 → 0 батчей", () => new[] { 1, 2, 3 }.ToGrid(0).Count(), 0);
        DemoConsole.Case("count=-1 → 0 батчей", () => new[] { 1, 2, 3 }.ToGrid(-1).Count(), 0);
        DemoConsole.Case("[1..5] по 2 → [2,2,1]", () =>
            string.Join(",", new[] { 1, 2, 3, 4, 5 }.ToGrid(2).Select(b => b.Length)), "2,2,1");
        DemoConsole.Case("[1..6] по 3 → [3,3]", () =>
            string.Join(",", new[] { 1, 2, 3, 4, 5, 6 }.ToGrid(3).Select(b => b.Length)), "3,3");
        DemoConsole.Case("count больше длины → 1 батч", () =>
            new[] { 1, 2 }.ToGrid(100).Count(), 1);
    }
    private static void SeparatorsCheck()
    {
        DemoConsole.Note("SEP_ITEMS / SEP_ITEMS2 — разделители для ToIntArray");
        DemoConsole.Case("SEP_ITEMS содержит ',' ';' '|'", () =>
            string.Join("", Exts__to.SEP_ITEMS), ",;|");
        DemoConsole.Case("SEP_ITEMS2 не null", () =>
            Exts__to.SEP_ITEMS2 is not null, true);

        DemoConsole.Note("Проверим ToIntArray на всех трёх разделителях");
        DemoConsole.Case("\"1,2,3\"", () => string.Join(",", "1,2,3".ToIntArray()), "1,2,3");
        DemoConsole.Case("\"1;2;3\"", () => string.Join(",", "1;2;3".ToIntArray()), "1,2,3");
        DemoConsole.Case("\"1|2|3\"", () => string.Join(",", "1|2|3".ToIntArray()), "1,2,3");
        DemoConsole.Case("\"1;2|3\" — смешанные", () =>
            string.Join(",", "1;2|3".ToIntArray()), "1,2,3");
    }

    private static void ToIntArrayFromArray()
    {
        DemoConsole.Note("ToIntArray(string[]) — БАГ: невалидная строка → 0, НЕ отбрасывается");
        DemoConsole.Case("null → []", () => ((string[]?)null).ToIntArray().Length, 0);
        DemoConsole.Case("[] → []", () => Array.Empty<string>().ToIntArray().Length, 0);
        DemoConsole.Case("[1,2,3] → [1,2,3]", () =>
            string.Join(",", new[] { "1", "2", "3" }.ToIntArray()), "1,2,3");
        DemoConsole.Case("[1,abc,3] → [1,0,3] (BUG)", () =>
            string.Join(",", new[] { "1", "abc", "3" }.ToIntArray()), "1,0,3");
        DemoConsole.Case("[\"\",\"7\"] → [0,7] (BUG)", () =>
            string.Join(",", new[] { "", "7" }.ToIntArray()), "0,7");
        DemoConsole.Case("[ 42 ] (с пробелами) → [42]", () =>
            string.Join(",", new[] { " 42 " }.ToIntArray()), "42");
        DemoConsole.Case("overflow → 0", () =>
            string.Join(",", new[] { "999999999999" }.ToIntArray()), "0");
    }

    private static void ToIntArrayFromString()
    {
        DemoConsole.Note("ToIntArray(string) — CSV через SEP_ITEMS2 (TODO: посмотреть что это)");
        DemoConsole.Case("null → []", () => ((string?)null).ToIntArray().Length, 0);
        DemoConsole.Case("\"\" → []", () => "".ToIntArray().Length, 0);
        DemoConsole.Case("\"1,2,3\" → [1,2,3]", () =>
            string.Join(",", "1,2,3".ToIntArray()), "1,2,3");
        DemoConsole.Case("\"1,abc,3\" → [1,0,3] (BUG)", () =>
            string.Join(",", "1,abc,3".ToIntArray()), "1,0,3");
        DemoConsole.Case("\"1;;3\" — SplitAny даёт пустой сегмент → 0", () =>
            string.Join(",", "1;;3".ToIntArray()), "1,0,3");
        DemoConsole.Note("TODO: уточнить у автора, какой именно набор символов в SEP_ITEMS2 " +
                         "— здесь мы предполагаем ',' и ';'");
    }

    private static void ToIntArrayTrimFromString()
    {
        DemoConsole.Note("ToIntArrayTrim — как ToIntArray, но пустые сегменты отбрасывает");
        DemoConsole.Case("null → []", () => ((string?)null).ToIntArrayTrim().Length, 0);
        DemoConsole.Case("\"\" → []", () => "".ToIntArrayTrim().Length, 0);
        DemoConsole.Case("\"   \" → []", () => "   ".ToIntArrayTrim().Length, 0);
        DemoConsole.Case("\"1,2,3\" → [1,2,3]", () =>
            string.Join(",", "1,2,3".ToIntArrayTrim()), "1,2,3");
        DemoConsole.Case("\"1;;3\" → [1,3] (пустые отброшены)", () =>
            string.Join(",", "1;;3".ToIntArrayTrim()), "1,3");
        DemoConsole.Case("\" 1 , 2 , 3 \" → [1,2,3] (Trim)", () =>
            string.Join(",", " 1 , 2 , 3 ".ToIntArrayTrim()), "1,2,3");
        DemoConsole.Case("\"1,abc,3\" → [1,0,3] (abc не отбрасывается, только пустые)", () =>
            string.Join(",", "1,abc,3".ToIntArrayTrim()), "1,0,3");
    }
}