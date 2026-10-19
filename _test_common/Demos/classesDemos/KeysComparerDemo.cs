using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class KeysComparerDemo
{
    public static Task Run()
    {
        DemoConsole.Part("KeysComparer");
        BothNull();
        OnlyNew();
        OnlyOld();
        Identical();
        Disjoint();
        Overlapping();
        FromStringArray();
        FromCsvString();
        return Task.CompletedTask;
    }

    private static void BothNull()
    {
        DemoConsole.Note("оба null → пусто");
        var c = new KeysComparer((IEnumerable<int>?)null, (IEnumerable<int>?)null);
        DemoConsole.Case("Added пусто", () => c.Added.Count, 0);
        DemoConsole.Case("Deleted пусто", () => c.Deleted.Count, 0);
        DemoConsole.Case("HasAdded false", () => c.HasAdded, false);
        DemoConsole.Case("HasDeleted false", () => c.HasDeleted, false);
    }

    private static void OnlyNew()
    {
        DemoConsole.Note("old=null, new=[1,2,3]");
        var c = new KeysComparer((IEnumerable<int>?)null, [1, 2, 3]);
        DemoConsole.Case("Added = 3", () => c.Added.Count, 3);
        DemoConsole.Case("AddedString", () => c.AddedString, "1,2,3");
        DemoConsole.Case("Deleted пусто", () => c.Deleted.Count, 0);
    }

    private static void OnlyOld()
    {
        DemoConsole.Note("old=[1,2,3], new=null");
        var c = new KeysComparer([1, 2, 3], (IEnumerable<int>?)null);
        DemoConsole.Case("Deleted = 3", () => c.Deleted.Count, 3);
        DemoConsole.Case("DeletedString", () => c.DeletedString, "1,2,3");
        DemoConsole.Case("Added пусто", () => c.Added.Count, 0);
    }

    private static void Identical()
    {
        DemoConsole.Note("идентичные");
        var c = new KeysComparer([1, 2, 3], [1, 2, 3]);
        DemoConsole.Case("Added пусто", () => c.Added.Count, 0);
        DemoConsole.Case("Deleted пусто", () => c.Deleted.Count, 0);
    }

    private static void Disjoint()
    {
        DemoConsole.Note("непересекающиеся");
        var c = new KeysComparer([1, 2, 3], [4, 5, 6]);
        DemoConsole.Case("Added = [4,5,6]", () => c.AddedString, "4,5,6");
        DemoConsole.Case("Deleted = [1,2,3]", () => c.DeletedString, "1,2,3");
    }

    private static void Overlapping()
    {
        DemoConsole.Note("частичное пересечение");
        var c = new KeysComparer([1, 2, 3], [3, 4, 5]);
        DemoConsole.Case("Added = [4,5]", () => c.AddedString, "4,5");
        DemoConsole.Case("Deleted = [1,2]", () => c.DeletedString, "1,2");
    }

    private static void FromStringArray()
    {
        DemoConsole.Note("ctor(string[], string[]) — валидные строки");
        DemoConsole.Case("\"1\",\"2\",\"3\" vs \"2\",\"4\"", () =>
        {
            var c = new KeysComparer(new[] { "1", "2", "3" }, new[] { "2", "4" });
            return $"{c.AddedString}|{c.DeletedString}";
        }, "4|1,3");

        DemoConsole.Case("null массивы → пусто", () =>
        {
            var c = new KeysComparer((string[]?)null, (string[]?)null);
            return c.Added.Count + c.Deleted.Count;
        }, 0);

        DemoConsole.Case("пустые массивы → пусто", () =>
        {
            var c = new KeysComparer(Array.Empty<string>(), Array.Empty<string>());
            return c.Added.Count + c.Deleted.Count;
        }, 0);

        DemoConsole.Note("BUG в doc-comment: обещано «невалидные отсекаются», " +
                         "а по факту ToIntArray даёт 0. Фиксируем реальное поведение.");
        DemoConsole.Case("[\"1\",\"abc\",\"3\"] vs [\"3\"] → Deleted = \"1,0\"", () =>
        {
            var c = new KeysComparer(new[] { "1", "abc", "3" }, new[] { "3" });
            return c.DeletedString;
        }, "1,0");
    }

    private static void FromCsvString()
    {
        DemoConsole.Note("ctor(string, string) — CSV валидные");
        DemoConsole.Case("\"1,2,3\" vs \"3,4,5\"", () =>
        {
            var c = new KeysComparer("1,2,3", "3,4,5");
            return $"{c.AddedString}|{c.DeletedString}";
        }, "4,5|1,2");

        DemoConsole.Case("null → пусто", () =>
        {
            var c = new KeysComparer((string?)null, (string?)null);
            return c.Added.Count + c.Deleted.Count;
        }, 0);

        DemoConsole.Case("\"\" → пусто", () =>
        {
            var c = new KeysComparer("", "");
            return c.Added.Count + c.Deleted.Count;
        }, 0);

        DemoConsole.Note("CSV с невалидной строкой — ToIntArray → 0, не отсекается");
        DemoConsole.Case("\"1,abc,3\" vs \"3\" → Deleted = \"1,0\"", () =>
        {
            var c = new KeysComparer("1,abc,3", "3");
            return c.DeletedString;
        }, "1,0");
    }
}