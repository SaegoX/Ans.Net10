using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class ClassIdWrapperDemo
{
    private sealed class User
    {
        public string Name { get; }
        public User(string n) => Name = n;
        public override string ToString() => $"User({Name})";
    }

    public static Task Run()
    {
        DemoConsole.Part("ClassIdWrapper<T> (readonly record struct)");
        Ctor();
        ValueSemantics();
        NullItem();
        WithStructType();
        return Task.CompletedTask;
    }

    private static void Ctor()
    {
        DemoConsole.Note("Конструктор и свойства");
        var w = new ClassIdWrapper<int>(1, 42);
        DemoConsole.Case("Id", () => w.Id, 1);
        DemoConsole.Case("Item", () => w.Item, 42);

        var w2 = new ClassIdWrapper<string>(2, "hello");
        DemoConsole.Case("string Id=2", () => w2.Id, 2);
        DemoConsole.Case("string Item=\"hello\"", () => w2.Item, "hello");
    }

    private static void ValueSemantics()
    {
        DemoConsole.Note("Value-семантика record struct");
        var a = new ClassIdWrapper<int>(1, 42);
        var b = new ClassIdWrapper<int>(1, 42);
        var c = new ClassIdWrapper<int>(2, 42);
        DemoConsole.Case("Equals(a,b)", () => a.Equals(b), true);
        DemoConsole.Case("Equals(a,c)", () => a.Equals(c), false);
        DemoConsole.Case("a == b (op==)", () => a == b, true);
        DemoConsole.Case("hash(a)==hash(b)", () => a.GetHashCode() == b.GetHashCode(), true);
    }

    private static void NullItem()
    {
        DemoConsole.Note("null Item для reference-type T");
        var w = new ClassIdWrapper<string?>(1, null);
        DemoConsole.Case("string? Item is null", () => w.Item is null, true);
        DemoConsole.Case("Id всё ещё 1", () => w.Id, 1);

        var user = new User("vasya");
        var w2 = new ClassIdWrapper<User>(1, user);
        DemoConsole.Case("Item == user по ссылке", () => ReferenceEquals(w2.Item, user), true);
        DemoConsole.Case("Item.Name", () => w2.Item!.Name, "vasya");
    }

    private static void WithStructType()
    {
        DemoConsole.Note("Value-type T: T? = T (не Nullable<T>) — null присвоить нельзя");
        var w = new ClassIdWrapper<int>(1, 42);
        DemoConsole.Case("Item = 42", () => w.Item, 42);

        var w2 = new ClassIdWrapper<DateTime>(1, new DateTime(2026, 10, 10));
        DemoConsole.Case("Item.Year = 2026", () => w2.Item.Year, 2026);

        DemoConsole.Note("new ClassIdWrapper<int>(1, null) НЕ скомпилируется — Item это обычный int, а не int?");
    }
}