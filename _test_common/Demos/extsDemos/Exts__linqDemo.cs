using System.Linq.Expressions;
using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class Exts__linqDemo
{
    private sealed class Item
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public Item? Parent { get; init; }
    }

    private sealed class Nested
    {
        public Item? Child { get; init; }
    }

    public static Task Run()
    {
        DemoConsole.Part("Exts__linq");
        ApplyOrderChecks();
        ApplyOrderByChecks();
        ApplyThenByChecks();
        OrAndChecks();
        return Task.CompletedTask;
    }

    private static List<Item> Sample() =>
    [
        new() { Id = 3, Name = "c" },
        new() { Id = 1, Name = "a" },
        new() { Id = 2, Name = "b" }
    ];

    private static void ApplyOrderChecks()
    {
        DemoConsole.Note("ApplyOrder — цепочка сортировок через OrderBuilder");
        var q = Sample().AsQueryable();

        DemoConsole.Case("пустой OrderBuilder → тот же порядок", () =>
            string.Join(",", q.ApplyOrder(new OrderBuilder("")).Select(x => x.Id)), "3,1,2");

        DemoConsole.Case("\"Id\" → 1,2,3", () =>
            string.Join(",", q.ApplyOrder(new OrderBuilder("Id")).Select(x => x.Id)), "1,2,3");

        DemoConsole.Case("\"-Id\" → 3,2,1", () =>
            string.Join(",", q.ApplyOrder(new OrderBuilder("-Id")).Select(x => x.Id)), "3,2,1");

        DemoConsole.Case("\"Name,-Id\" (мульти)", () =>
            string.Join(",", q.ApplyOrder(new OrderBuilder("Name,-Id")).Select(x => x.Name)), "a,b,c");

        DemoConsole.CaseThrows<ArgumentNullException>("null query", () =>
            _ = ((IQueryable<Item>)null!).ApplyOrder(new OrderBuilder("Id")));

        DemoConsole.CaseThrows<ArgumentNullException>("null order", () =>
            _ = q.ApplyOrder(null!));
    }

    private static void ApplyOrderByChecks()
    {
        DemoConsole.Note("ApplyOrderBy / ApplyOrderByDescending");
        var q = Sample().AsQueryable();

        DemoConsole.Case("ApplyOrderBy(\"Id\") → 1,2,3", () =>
            string.Join(",", q.ApplyOrderBy("Id").Select(x => x.Id)), "1,2,3");
        DemoConsole.Case("ApplyOrderByDescending(\"Id\") → 3,2,1", () =>
            string.Join(",", q.ApplyOrderByDescending("Id").Select(x => x.Id)), "3,2,1");

        DemoConsole.Note("Вложенное свойство через точку — Parent.Id");
        var nested = new List<Nested>
        {
            new() { Child = new Item { Id = 2 } },
            new() { Child = new Item { Id = 1 } },
            new() { Child = new Item { Id = 3 } }
        }.AsQueryable();

        DemoConsole.Case("ApplyOrderBy(\"Child.Id\")", () =>
            string.Join(",", nested.ApplyOrderBy("Child.Id").Select(x => x.Child!.Id)), "1,2,3");

        DemoConsole.CaseThrows<InvalidOperationException>("нет свойства \"xxx\"", () =>
            _ = q.ApplyOrderBy("xxx").ToList());
        DemoConsole.CaseThrows<InvalidOperationException>("нет вложенного \"Child.xxx\"", () =>
            _ = nested.ApplyOrderBy("Child.xxx").ToList());
    }

    private static void ApplyThenByChecks()
    {
        DemoConsole.Note("ApplyThenBy / ApplyThenByDescending");
        var data = new List<Item>
        {
            new() { Id = 1, Name = "b" },
            new() { Id = 1, Name = "a" },
            new() { Id = 2, Name = "c" },
        }.AsQueryable();

        DemoConsole.Case("OrderBy Id, ThenBy Name → [a,b],[c]", () =>
        {
            var result = data.ApplyOrderBy("Id").ApplyThenBy("Name").ToList();
            return string.Join(",", result.Select(x => $"{x.Id}:{x.Name}"));
        }, "1:a,1:b,2:c");

        DemoConsole.Case("OrderBy Id, ThenByDescending Name → [b,a],[c]", () =>
        {
            var result = data.ApplyOrderBy("Id").ApplyThenByDescending("Name").ToList();
            return string.Join(",", result.Select(x => $"{x.Id}:{x.Name}"));
        }, "1:b,1:a,2:c");
    }

    private static void OrAndChecks()
    {
        DemoConsole.Note("Or / And — комбинирование деревьев выражений");
        Expression<Func<Item, bool>> e1 = x => x.Id == 1;
        Expression<Func<Item, bool>> e2 = x => x.Id == 2;

        var data = Sample();
        DemoConsole.Case("Or(Id=1, Id=2) → [1,2]", () =>
        {
            var combined = e1.Or(e2).Compile();
            return string.Join(",", data.Where(combined).Select(x => x.Id).OrderBy(x => x));
        }, "1,2");

        DemoConsole.Case("And(Id>0, Id<3) → [1,2]", () =>
        {
            Expression<Func<Item, bool>> a1 = x => x.Id > 0;
            Expression<Func<Item, bool>> a2 = x => x.Id < 3;
            var combined = a1.And(a2).Compile();
            return string.Join(",", data.Where(combined).Select(x => x.Id).OrderBy(x => x));
        }, "1,2");

        DemoConsole.CaseThrows<ArgumentNullException>("Or(null, e2)", () =>
            _ = Exts__linq.Or((Expression<Func<Item, bool>>)null!, e2));
        DemoConsole.CaseThrows<ArgumentNullException>("And(e1, null)", () =>
            _ = Exts__linq.And(e1, (Expression<Func<Item, bool>>)null!));

        DemoConsole.Note("Особенность: Or/And используют Expression.Invoke — " +
                         "это работает при Compile(), но EF Core не всегда переводит Invoke в SQL.");
    }
}