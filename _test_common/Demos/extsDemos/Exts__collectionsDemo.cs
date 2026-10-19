using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class Exts__collectionsDemo
{
    private sealed class Item
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public override string ToString() => $"{Id}:{Name}";
    }

    public static Task Run()
    {
        DemoConsole.Part("Exts__collections");
        GetArrayAdd();
        GetArrayInsert();
        GetArrayRemoveAt();
        GetArrayRemove();
        GetDiffLazyChecks();
        GetDiffListChecks();
        GetActualUpdatedAddedRemoved();
        GetItemsClean();
        GetItemsTrim();
        GetItemsWithoutEmpty();
        GetItemsUnique();
        GetItemsCleanUnique();
        return Task.CompletedTask;
    }

    private static void GetArrayAdd()
    {
        DemoConsole.Note("GetArrayAdd — новый массив + элемент в конце");
        DemoConsole.Case("null + 5 → [5]", () => ((int[]?)null).GetArrayAdd(5).Length, 1);
        DemoConsole.Case("[] + 5 → [5]", () => Array.Empty<int>().GetArrayAdd(5).Length, 1);
        DemoConsole.Case("[1,2] + 3 → [1,2,3]", () =>
            string.Join(",", new[] { 1, 2 }.GetArrayAdd(3)), "1,2,3");
        DemoConsole.Case("не мутирует исходный", () =>
        {
            var src = new[] { 1, 2 };
            _ = src.GetArrayAdd(3);
            return src.Length;
        }, 2);
        DemoConsole.Case("null-элемент ок", () => ((string[]?)null).GetArrayAdd(null!).Length, 1);
    }

    private static void GetArrayInsert()
    {
        DemoConsole.Note("GetArrayInsert — вставка по индексу; index < 0 или > len → ArgumentOutOfRange");
        DemoConsole.Case("null + в index=0 → [5]", () => ((int[]?)null).GetArrayInsert(0, 5).Length, 1);
        DemoConsole.Case("[] + index=0 → [5]", () => Array.Empty<int>().GetArrayInsert(0, 5).Length, 1);
        DemoConsole.Case("[1,3] + index=1 → [1,2,3]", () =>
            string.Join(",", new[] { 1, 3 }.GetArrayInsert(1, 2)), "1,2,3");
        DemoConsole.Case("[1,2] + index=2 (в конец)", () =>
            string.Join(",", new[] { 1, 2 }.GetArrayInsert(2, 3)), "1,2,3");
        DemoConsole.Case("[1,2] + index=0 (в начало)", () =>
            string.Join(",", new[] { 1, 2 }.GetArrayInsert(0, 0)), "0,1,2");

        DemoConsole.CaseThrows<ArgumentOutOfRangeException>("index=-1", () =>
            _ = new[] { 1, 2 }.GetArrayInsert(-1, 5));
        DemoConsole.CaseThrows<ArgumentOutOfRangeException>("index=3 > len=2", () =>
            _ = new[] { 1, 2 }.GetArrayInsert(3, 5));
    }

    private static void GetArrayRemoveAt()
    {
        DemoConsole.Note("GetArrayRemoveAt — удаление по индексу; null/empty/out-of-range → AOORE");
        DemoConsole.Case("[1,2,3] - index=1 → [1,3]", () =>
            string.Join(",", new[] { 1, 2, 3 }.GetArrayRemoveAt(1)), "1,3");
        DemoConsole.Case("[1,2,3] - index=0 → [2,3]", () =>
            string.Join(",", new[] { 1, 2, 3 }.GetArrayRemoveAt(0)), "2,3");
        DemoConsole.Case("[1,2,3] - index=2 → [1,2]", () =>
            string.Join(",", new[] { 1, 2, 3 }.GetArrayRemoveAt(2)), "1,2");
        DemoConsole.Case("[x] - index=0 → []", () =>
            new[] { 42 }.GetArrayRemoveAt(0).Length, 0);

        DemoConsole.CaseThrows<ArgumentOutOfRangeException>("null", () => _ = ((int[]?)null).GetArrayRemoveAt(0));
        DemoConsole.CaseThrows<ArgumentOutOfRangeException>("empty", () => _ = Array.Empty<int>().GetArrayRemoveAt(0));
        DemoConsole.CaseThrows<ArgumentOutOfRangeException>("index=-1", () => _ = new[] { 1 }.GetArrayRemoveAt(-1));
        DemoConsole.CaseThrows<ArgumentOutOfRangeException>("index=len", () => _ = new[] { 1 }.GetArrayRemoveAt(1));
    }

    private static void GetArrayRemove()
    {
        DemoConsole.Note("GetArrayRemove — удаляет ПЕРВОЕ вхождение; нет совпадения → исходный массив");
        DemoConsole.Case("null → []", () => ((int[]?)null).GetArrayRemove(5).Length, 0);
        DemoConsole.Case("[] → []", () => Array.Empty<int>().GetArrayRemove(5).Length, 0);
        DemoConsole.Case("[1,2,3] - 2 → [1,3]", () =>
            string.Join(",", new[] { 1, 2, 3 }.GetArrayRemove(2)), "1,3");
        DemoConsole.Case("[1,2,1] - 1 → [2,1] (только первое)", () =>
            string.Join(",", new[] { 1, 2, 1 }.GetArrayRemove(1)), "2,1");
        DemoConsole.Case("[1,2] - 99 → тот же массив", () =>
        {
            var src = new[] { 1, 2 };
            var result = src.GetArrayRemove(99);
            return ReferenceEquals(result, src);
        }, true);
    }

    private static void GetDiffLazyChecks()
    {
        DemoConsole.Note("GetDiffLazy — ленивый (IEnumerable)");
        var cur = new[] { new Item { Id = 1, Name = "a" }, new Item { Id = 2, Name = "b" } };
        var nw = new[] { new Item { Id = 2, Name = "b2" }, new Item { Id = 3, Name = "c" } };

        var (added, removed, updated) = cur.GetDiffLazy(nw, x => x.Id);
        DemoConsole.Case("Added   = [3]", () => string.Join(",", added.Select(x => x.Id)), "3");
        DemoConsole.Case("Removed = [1]", () => string.Join(",", removed.Select(x => x.Id)), "1");
        DemoConsole.Case("Updated = [2]", () => string.Join(",", updated.Select(x => x.Id)), "2");

        DemoConsole.Note("Lazy — не перечислен, но типы ок");
        DemoConsole.Case("added is IEnumerable<Item>", () => added is IEnumerable<Item>, true);
    }

    private static void GetDiffListChecks()
    {
        DemoConsole.Note("GetDiffList — материализованный List");
        var cur = new[] { new Item { Id = 1, Name = "a" }, new Item { Id = 2, Name = "b" } };
        var nw = new[] { new Item { Id = 2, Name = "b2" }, new Item { Id = 3, Name = "c" } };

        var (added, removed, updated) = cur.GetDiffList(nw, x => x.Id);
        DemoConsole.Case("Added   = [3]", () => string.Join(",", added.Select(x => x.Id)), "3");
        DemoConsole.Case("Removed = [1]", () => string.Join(",", removed.Select(x => x.Id)), "1");
        DemoConsole.Case("Updated = [2:b2] (из newest!)", () =>
            string.Join(",", updated.Select(x => $"{x.Id}:{x.Name}")), "2:b2");
    }

    private static void GetActualUpdatedAddedRemoved()
    {
        DemoConsole.Note("GetActual / GetUpdated / GetAdded / GetRemoved");
        var cur = new[] { new Item { Id = 1, Name = "a" }, new Item { Id = 2, Name = "b" } };
        var nw = new[] { new Item { Id = 2, Name = "b2" }, new Item { Id = 3, Name = "c" } };

        DemoConsole.Note("GetActual — элементы current, чьи ключи есть в newest (пересечение)");
        DemoConsole.Case("GetActual = [2] (Id=1 нет в newest)", () =>
            string.Join(",", cur.GetActual(nw, x => x.Id).Select(x => x.Id)), "2");
        DemoConsole.Case("GetUpdated = [2] (из newest)", () =>
            string.Join(",", cur.GetUpdated(nw, x => x.Id).Select(x => x.Id)), "2");
        DemoConsole.Case("GetAdded = [3] (из newest)", () =>
            string.Join(",", cur.GetAdded(nw, x => x.Id).Select(x => x.Id)), "3");
        DemoConsole.Case("GetRemoved = [1] (из current)", () =>
            string.Join(",", cur.GetRemoved(nw, x => x.Id).Select(x => x.Id)), "1");

        DemoConsole.Note("ВАЖНО: GetActual возвращает ссылки на объекты из CURRENT, " +
                         "GetUpdated — из NEWEST. Проверим Name:");
        DemoConsole.Case("GetActual[0].Name == \"b\" (из cur)", () =>
            cur.GetActual(nw, x => x.Id).First().Name, "b");
        DemoConsole.Case("GetUpdated[0].Name == \"b2\" (из newest)", () =>
            cur.GetUpdated(nw, x => x.Id).First().Name, "b2");
    }

    private static void GetItemsClean()
    {
        DemoConsole.Note("GetItemsClean — Trim + убрать null/empty");
        DemoConsole.Case("[\"  a  \", null, \"\", \"  \", \"b\"] → [a, b]", () =>
            string.Join(",", new[] { "  a  ", null, "", "  ", "b" }.GetItemsClean()!), "a,b");
        DemoConsole.Case("пустой → []", () => Array.Empty<string?>().GetItemsClean().Count(), 0);
    }

    private static void GetItemsTrim()
    {
        DemoConsole.Note("GetItemsTrim — Trim, null остаётся null");
        DemoConsole.Case("[\"  a  \", null, \"\"] → [a, null, \"\"]", () =>
        {
            var r = new[] { "  a  ", null, "" }.GetItemsTrim().ToList();
            return string.Join("|", r.Select(x => x ?? "NULL"));
        }, "a|NULL|");
    }

    private static void GetItemsWithoutEmpty()
    {
        DemoConsole.Note("GetItemsWithoutEmpty — убрать null/empty, БЕЗ Trim");
        DemoConsole.Case("[\"  a  \", null, \"\", \"b\"] → [\"  a  \", b]", () =>
            string.Join("|", new[] { "  a  ", null, "", "b" }.GetItemsWithoutEmpty()!), "  a  |b");
    }

    private static void GetItemsUnique()
    {
        DemoConsole.Note("GetItemsUnique — Distinct, по умолчанию Ordinal");
        DemoConsole.Case("[a, A, a, b] Ordinal → [a, A, b]", () =>
            string.Join(",", new[] { "a", "A", "a", "b" }.GetItemsUnique()!), "a,A,b");
        DemoConsole.Case("[a, A, a] OrdinalIgnoreCase → [a]", () =>
            string.Join(",", new[] { "a", "A", "a" }.GetItemsUnique(StringComparer.OrdinalIgnoreCase)!), "a");
        DemoConsole.Case("null → содержит null", () =>
            new[] { "a", null, "a" }.GetItemsUnique()!.Count(), 2);
    }

    private static void GetItemsCleanUnique()
    {
        DemoConsole.Note("GetItemsCleanUnique — Trim + убрать null/empty + уникальные");
        DemoConsole.Case("[\"  a  \", \"a\", null, \"\"] Ordinal → [a]", () =>
            string.Join(",", new[] { "  a  ", "a", null, "" }.GetItemsCleanUnique()), "a");
        DemoConsole.Case("[A, a, B] OrdinalIgnoreCase → [A, B]", () =>
            string.Join(",", new[] { "A", "a", "B" }.GetItemsCleanUnique(StringComparer.OrdinalIgnoreCase)), "A,B");
        DemoConsole.Case("[a, a, a] → [a]", () =>
            string.Join(",", new[] { "a", "a", "a" }.GetItemsCleanUnique()), "a");
    }
}