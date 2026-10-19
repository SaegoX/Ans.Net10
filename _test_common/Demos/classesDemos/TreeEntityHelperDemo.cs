using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class TreeEntityHelperDemo
{
    private sealed class Node : ITreeEntity
    {
        public int Id { get; set; }
        public int? ParentPtr { get; set; }
        public int Order { get; set; }
        public string Name { get; set; } = "";
        public override string ToString() => Name;
    }

    private static List<Node> BuildTree() =>
    [
        new() { Id = 1, ParentPtr = null, Order = 0, Name = "root1" },
        new() { Id = 2, ParentPtr = 1,    Order = 0, Name = "child1a" },
        new() { Id = 3, ParentPtr = 1,    Order = 1, Name = "child1b" },
        new() { Id = 4, ParentPtr = 2,    Order = 0, Name = "grand1" },
        new() { Id = 5, ParentPtr = null, Order = 1, Name = "root2" },
    ];

    private static Func<IEnumerable<Node>, IOrderedEnumerable<Node>> OrderByOrder
        => xs => xs.OrderBy(x => x.Order);

    public static Task Run()
    {
        DemoConsole.Part("TreeEntityHelper / TreeEntityWrapper / ITreeEntity");
        BuildBasic();
        ChildrenWiring();
        LevelsCheck();
        CurrentIdFilter();
        EmptySource();
        WrapperDefaults();
        return Task.CompletedTask;
    }

    private static void BuildBasic()
    {
        DemoConsole.Note("Построение из плоской коллекции");
        var h = new TreeEntityHelper<Node>(BuildTree(), null, OrderByOrder);

        DemoConsole.Case("TopItems = [root1, root2]", () =>
            string.Join(",", h.TopItems.Select(t => t.Value!.Name)), "root1,root2");

        DemoConsole.Case("AllItems pre-order = [root1,child1a,grand1,child1b,root2]", () =>
            string.Join(",", h.AllItems.Select(t => t.Value!.Name)),
            "root1,child1a,grand1,child1b,root2");

        DemoConsole.Case("AllItems.Count = 5", () => h.AllItems.Count(), 5);
    }

    private static void ChildrenWiring()
    {
        DemoConsole.Note("Children: null если пусто, иначе — [дети по Order]");
        var h = new TreeEntityHelper<Node>(BuildTree(), null, OrderByOrder);

        DemoConsole.Case("root1.Children.Count == 2", () =>
            h.TopItems.First(t => t.Id == 1).Children!.Count(), 2);
        DemoConsole.Case("root1.Children = [child1a, child1b]", () =>
            string.Join(",", h.TopItems.First(t => t.Id == 1).Children!.Select(x => x.Value!.Name)),
            "child1a,child1b");
        DemoConsole.Case("grand1.Children == null (нет детей)", () =>
            h.AllItems.First(t => t.Id == 4).Children, (IEnumerable<TreeEntityWrapper<Node>>?)null);
        DemoConsole.Case("root1.Parent == null", () =>
            h.TopItems.First(t => t.Id == 1).Parent, (TreeEntityWrapper<Node>?)null);
        DemoConsole.Case("child1a.Parent.Id == 1", () =>
            h.AllItems.First(t => t.Id == 2).Parent!.Id, 1);
    }

    private static void LevelsCheck()
    {
        DemoConsole.Note("Level по глубине");
        var h = new TreeEntityHelper<Node>(BuildTree(), null, OrderByOrder);

        DemoConsole.Case("root1.Level = 0", () => h.AllItems.First(t => t.Id == 1).Level, 0);
        DemoConsole.Case("child1a.Level = 1", () => h.AllItems.First(t => t.Id == 2).Level, 1);
        DemoConsole.Case("grand1.Level = 2", () => h.AllItems.First(t => t.Id == 4).Level, 2);
        DemoConsole.Case("root2.Level = 0", () => h.AllItems.First(t => t.Id == 5).Level, 0);
    }

    private static void CurrentIdFilter()
    {
        DemoConsole.Note("currentId=2 — исключает поддерево 2 (2 и его детей)");
        var h = new TreeEntityHelper<Node>(BuildTree(), 2, OrderByOrder);

        DemoConsole.Case("node 2 отсутствует", () =>
            h.AllItems.Any(t => t.Id == 2), false);
        DemoConsole.Case("node 4 (внук) тоже отсутствует", () =>
            h.AllItems.Any(t => t.Id == 4), false);
        DemoConsole.Case("root1.Children.Count == 1 (только child1b)", () =>
            h.TopItems.First(t => t.Id == 1).Children!.Count(), 1);
        DemoConsole.Case("root2 остаётся", () =>
            h.TopItems.Any(t => t.Id == 5), true);
    }

    private static void EmptySource()
    {
        DemoConsole.Note("Пустая коллекция → пустое дерево");
        var h = new TreeEntityHelper<Node>(Array.Empty<Node>(), null, OrderByOrder);
        DemoConsole.Case("TopItems пусто", () => h.TopItems.Count(), 0);
        DemoConsole.Case("AllItems пусто", () => h.AllItems.Count(), 0);
    }

    private static void WrapperDefaults()
    {
        DemoConsole.Note("TreeEntityWrapper — default значения");
        var w = new TreeEntityWrapper<Node>();
        DemoConsole.Case("Id = 0", () => w.Id, 0);
        DemoConsole.Case("ParentPtr = null", () => w.ParentPtr, (int?)null);
        DemoConsole.Case("Parent = null", () => w.Parent, (TreeEntityWrapper<Node>?)null);
        DemoConsole.Case("Children = null", () => w.Children, (IEnumerable<TreeEntityWrapper<Node>>?)null);
        DemoConsole.Case("Value = null", () => w.Value, (Node?)null);
    }
}