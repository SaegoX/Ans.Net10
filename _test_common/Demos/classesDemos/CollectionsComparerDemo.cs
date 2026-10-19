using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class CollectionsComparerDemo
{
    private sealed class User
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public override string ToString() => $"{Id}:{Name}";
    }

    private static Func<User, int> Key => u => u.Id;
    private static Func<User, User, bool> Diff => (a, b) => a.Name != b.Name;

    private static CollectionsComparer<User, User, int> Cmp(
        IEnumerable<User>? cur,
        IEnumerable<User>? nw,
        Func<User, User, bool>? diff = null)
        => new(cur, nw, Key, Key, diff ?? Diff);

    public static Task Run()
    {
        DemoConsole.Part("CollectionsComparer<TCurrent, TNewest, TKey>");
        EmptyAll();
        NullInputs();
        AddedOnly();
        DeletedOnly();
        ActualOnly();
        ChangedOnly();
        Mixed();
        DiffAlwaysTrue();
        DiffAlwaysFalse();
        DuplicateKeys();
        TestDebugCall();
        return Task.CompletedTask;
    }

    private static void EmptyAll()
    {
        DemoConsole.Note("Обе коллекции пустые");
        var c = Cmp([], []);
        DemoConsole.Case("CurrentCount=0", () => c.CurrentCount, 0);
        DemoConsole.Case("NewestCount=0", () => c.NewestCount, 0);
        DemoConsole.Case("HasAdded=false", () => c.HasAdded, false);
        DemoConsole.Case("HasDeleted=false", () => c.HasDeleted, false);
        DemoConsole.Case("HasActual=false", () => c.HasActual, false);
        DemoConsole.Case("HasChanged=false", () => c.HasChanged, false);
    }

    private static void NullInputs()
    {
        DemoConsole.Note("null → трактуются как пустые");
        var c = new CollectionsComparer<User, User, int>(null, null, Key, Key, Diff);
        DemoConsole.Case("CurrentCount=0", () => c.CurrentCount, 0);
        DemoConsole.Case("NewestCount=0", () => c.NewestCount, 0);

        var c2 = new CollectionsComparer<User, User, int>(
            null, [new User { Id = 1, Name = "a" }], Key, Key, Diff);
        DemoConsole.Case("null current + 1 newest → Added=1", () => c2.AddedCount, 1);
    }

    private static void AddedOnly()
    {
        DemoConsole.Note("Только добавления");
        var c = Cmp(
            [new User { Id = 1, Name = "a" }],
            [new User { Id = 1, Name = "a" },
             new User { Id = 2, Name = "b" },
             new User { Id = 3, Name = "c" }]);
        DemoConsole.Case("Added=2", () => c.AddedCount, 2);
        DemoConsole.Case("Deleted=0", () => c.DeletedCount, 0);
        DemoConsole.Case("Actual=1", () => c.ActualCount, 1);
        DemoConsole.Case("Changed=0", () => c.ChangedCount, 0);
        DemoConsole.Case("Added ids", () => string.Join(",", c.Added.Select(u => u.Id).OrderBy(x => x)), "2,3");
    }

    private static void DeletedOnly()
    {
        DemoConsole.Note("Только удаления");
        var c = Cmp(
            [new User { Id = 1, Name = "a" },
             new User { Id = 2, Name = "b" },
             new User { Id = 3, Name = "c" }],
            [new User { Id = 1, Name = "a" }]);
        DemoConsole.Case("Deleted=2", () => c.DeletedCount, 2);
        DemoConsole.Case("Added=0", () => c.AddedCount, 0);
        DemoConsole.Case("Actual=1", () => c.ActualCount, 1);
        DemoConsole.Case("Deleted ids", () => string.Join(",", c.Deleted.Select(u => u.Id).OrderBy(x => x)), "2,3");
    }

    private static void ActualOnly()
    {
        DemoConsole.Note("Все совпадают по ключу и по данным");
        var c = Cmp(
            [new User { Id = 1, Name = "a" }, new User { Id = 2, Name = "b" }],
            [new User { Id = 1, Name = "a" }, new User { Id = 2, Name = "b" }]);
        DemoConsole.Case("Actual=2", () => c.ActualCount, 2);
        DemoConsole.Case("Changed=0", () => c.ChangedCount, 0);
    }

    private static void ChangedOnly()
    {
        DemoConsole.Note("Совпадают по ключу, различаются по Name");
        var c = Cmp(
            [new User { Id = 1, Name = "a" }, new User { Id = 2, Name = "b" }],
            [new User { Id = 1, Name = "a-changed" }, new User { Id = 2, Name = "b" }]);
        DemoConsole.Case("Actual=2", () => c.ActualCount, 2);
        DemoConsole.Case("Changed=1", () => c.ChangedCount, 1);
        DemoConsole.Case("Changed pair: a → a-changed", () =>
        {
            var pair = c.Changed.First();
            return $"{pair.Current.Name} → {pair.Newest.Name}";
        }, "a → a-changed");
    }

    private static void Mixed()
    {
        DemoConsole.Note("Смешанный сценарий");
        var c = Cmp(
            [new User { Id = 1, Name = "a" },
             new User { Id = 2, Name = "b" },
             new User { Id = 3, Name = "c" }],
            [new User { Id = 1, Name = "a" },
             new User { Id = 2, Name = "b-new" },
             new User { Id = 4, Name = "d" }]);
        DemoConsole.Case("Current=3", () => c.CurrentCount, 3);
        DemoConsole.Case("Newest=3", () => c.NewestCount, 3);
        DemoConsole.Case("Added=1", () => c.AddedCount, 1);
        DemoConsole.Case("Deleted=1", () => c.DeletedCount, 1);
        DemoConsole.Case("Actual=2", () => c.ActualCount, 2);
        DemoConsole.Case("Changed=1", () => c.ChangedCount, 1);
        DemoConsole.Case("ActualCurrent ids", () => string.Join(",", c.ActualCurrent.Select(u => u.Id).OrderBy(x => x)), "1,2");
        DemoConsole.Case("ActualNewest ids", () => string.Join(",", c.ActualNewest.Select(u => u.Id).OrderBy(x => x)), "1,2");
    }

    private static void DiffAlwaysTrue()
    {
        DemoConsole.Note("funcDataDiff всегда true → все Actual → Changed");
        var c = Cmp(
            [new User { Id = 1, Name = "a" }, new User { Id = 2, Name = "b" }],
            [new User { Id = 1, Name = "a" }, new User { Id = 2, Name = "b" }],
            (_, _) => true);
        DemoConsole.Case("Actual=2", () => c.ActualCount, 2);
        DemoConsole.Case("Changed=2", () => c.ChangedCount, 2);
    }

    private static void DiffAlwaysFalse()
    {
        DemoConsole.Note("funcDataDiff всегда false → Changed всегда пуст");
        var c = Cmp(
            [new User { Id = 1, Name = "a" }],
            [new User { Id = 1, Name = "totally-different" }],
            (_, _) => false);
        DemoConsole.Case("Actual=1", () => c.ActualCount, 1);
        DemoConsole.Case("Changed=0", () => c.ChangedCount, 0);
    }

    private static void DuplicateKeys()
    {
        DemoConsole.Note("Дубликаты ключей → ToDictionary бросает ArgumentException");
        DemoConsole.CaseThrows<ArgumentException>("дубликаты в current", () =>
            _ = Cmp(
                [new User { Id = 1, Name = "a" }, new User { Id = 1, Name = "b" }],
                Array.Empty<User>()));

        DemoConsole.CaseThrows<ArgumentException>("дубликаты в newest", () =>
            _ = Cmp(
                Array.Empty<User>(),
                [new User { Id = 1, Name = "a" }, new User { Id = 1, Name = "b" }]));
    }

    private static void TestDebugCall()
    {
        DemoConsole.Note("TestDebug() — печатает статистику в консоль");
        var c = Cmp(
            [new User { Id = 1, Name = "a" }, new User { Id = 2, Name = "b" }],
            [new User { Id = 1, Name = "a" }, new User { Id = 3, Name = "c" }]);
        DemoConsole.Case("вызов без ошибок", () => { c.TestDebug(); });
    }
}