using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class TreeItemDemo
{
    /// <summary>Минимальный наследник с именем — чтобы отличать узлы в выводе.</summary>
    private sealed class Node : _TreeItem_Base
    {
        public string Name { get; }
        public Node(string name) => Name = name;
        public override string ToString() => Name;
    }

    /// <summary>Другой подтип — чтобы проверить FindItem&lt;T&gt; по типу.</summary>
    private sealed class SpecialNode : _TreeItem_Base
    {
        public string Tag { get; }
        public SpecialNode(string tag) => Tag = tag;
        public override string ToString() => $"Special({Tag})";
    }

    public static Task Run()
    {
        DemoConsole.Part("ITreeItem / _TreeItem_Base");
        Props();
        Append();
        AppendChildren();
        ParentsChain();
        FindItemSimple();
        FindItemRecursive();
        FindItemTyped();
        FindItemNotFound();
        CycleProtection();
        KnownLimitations();
        return Task.CompletedTask;
    }

    private static void Props()
    {
        DemoConsole.Note("Базовые свойства нового узла");

        DemoConsole.Case("HasParent у корня", () => new Node("root").HasParent, false);
        DemoConsole.Case("HasChildren у корня", () => new Node("root").HasChildren, false);
        DemoConsole.Case("Parent у корня — null", () => new Node("root").Parent, null);
        DemoConsole.Case("Children у корня — пусто", () => new Node("root").Children.Count(), 0);
        DemoConsole.Case("Parents у корня — пусто", () => new Node("root").Parents.Count(), 0);
    }

    private static void Append()
    {
        DemoConsole.Note("AppendChild(ITreeItem)");

        DemoConsole.Case("AppendChild(child) → HasChildren=true", () =>
        {
            var root = new Node("root");
            root.AppendChild(new Node("c1"));
            return root.HasChildren;
        }, true);

        DemoConsole.Case("AppendChild(child) → child.HasParent=true", () =>
        {
            var root = new Node("root");
            var c1 = new Node("c1");
            root.AppendChild(c1);
            return c1.HasParent && ReferenceEquals(c1.Parent, root);
        }, true);

        DemoConsole.Case("AppendChild(null) → no-op", () =>
        {
            var root = new Node("root");
            root.AppendChild(null!);
            return root.HasChildren;
        }, false);

        DemoConsole.Case("Порядок детей сохраняется", () =>
        {
            var root = new Node("root");
            root.AppendChild(new Node("a"));
            root.AppendChild(new Node("b"));
            root.AppendChild(new Node("c"));
            return string.Join(",", root.Children.Cast<Node>().Select(n => n.Name));
        }, "a,b,c");
    }

    private static void AppendChildren()
    {
        DemoConsole.Note("AppendChildren(params ITreeItem[])");

        DemoConsole.Case("AppendChildren(a, b, c) → 3", () =>
        {
            var root = new Node("root");
            root.AppendChildren(new Node("a"), new Node("b"), new Node("c"));
            return root.Children.Count();
        }, 3);

        DemoConsole.Case("AppendChildren с null в середине → null игнор", () =>
        {
            var root = new Node("root");
            root.AppendChildren(new Node("a"), null!, new Node("b"));
            return root.Children.Count();
        }, 2);

        DemoConsole.Case("AppendChildren(null-массив) → no-op", () =>
        {
            var root = new Node("root");
            root.AppendChildren((ITreeItem[])null!);
            return root.HasChildren;
        }, false);
    }

    private static void ParentsChain()
    {
        DemoConsole.Note("Parents — цепочка вверх до корня");

        DemoConsole.Case("root → child → grandchild, родители grandchild", () =>
        {
            var root = new Node("root");
            var c = new Node("child");
            var g = new Node("grand");
            root.AppendChild(c);
            c.AppendChild(g);
            return string.Join(" > ", g.Parents.Cast<Node>().Select(n => n.Name));
        }, "child > root");

        DemoConsole.Case("Parents у корня — пусто", () =>
        {
            var root = new Node("root");
            return root.Parents.Count();
        }, 0);
    }

    private static void FindItemSimple()
    {
        DemoConsole.Note("FindItem<T>(predicate) — прямой ребёнок");

        DemoConsole.Case("нашли 'b'", () =>
        {
            var root = new Node("root");
            root.AppendChildren(new Node("a"), new Node("b"), new Node("c"));
            return root.FindItem<Node>(n => n.Name == "b")?.Name;
        }, "b");

        DemoConsole.Case("не нашли 'zz'", () =>
        {
            var root = new Node("root");
            root.AppendChildren(new Node("a"), new Node("b"));
            return root.FindItem<Node>(n => n.Name == "zz");
        }, null);
    }

    private static void FindItemRecursive()
    {
        DemoConsole.Note("FindItem<T> — рекурсивный спуск (DFS)");

        DemoConsole.Case("нашли в глубине (3 уровня)", () =>
        {
            var root = new Node("root");
            var l1 = new Node("L1");
            var l2 = new Node("L2");
            var deep = new Node("deep");
            root.AppendChild(l1);
            l1.AppendChild(l2);
            l2.AppendChild(deep);
            return root.FindItem<Node>(n => n.Name == "deep")?.Name;
        }, "deep");

        DemoConsole.Case("порядок DFS: первый найденный слева", () =>
        {
            var root = new Node("root");
            var a = new Node("a");
            var b = new Node("b");
            a.AppendChild(new Node("target"));
            b.AppendChild(new Node("target"));
            root.AppendChildren(a, b);
            return ((Node)root.FindItem<Node>(n => n.Name == "target")!.Parent!).Name;
        }, "a");
    }

    private static void FindItemTyped()
    {
        DemoConsole.Note("FindItem<T> — фильтр по типу");

        DemoConsole.Case("ищем SpecialNode среди Node", () =>
        {
            var root = new Node("root");
            root.AppendChildren(new Node("a"), new Node("b"));
            var l1 = new Node("L1");
            l1.AppendChild(new SpecialNode("X"));
            root.AppendChild(l1);
            return root.FindItem<SpecialNode>(_ => true)?.Tag;
        }, "X");

        DemoConsole.Case("Node-тип найдёт SpecialNode тоже (наследник)", () =>
        {
            var root = new Node("root");
            var l1 = new Node("L1");
            l1.AppendChild(new SpecialNode("X"));
            root.AppendChild(l1);
            return root.FindItem<Node>(_ => true)?.ToString();
        }, "L1");
    }

    private static void FindItemNotFound()
    {
        DemoConsole.Note("FindItem<T> — негативные сценарии");

        DemoConsole.Case("у корня без детей", () =>
        {
            var root = new Node("root");
            return root.FindItem<Node>(_ => true);
        }, null);

        DemoConsole.Case("null-предикат → null", () =>
        {
            var root = new Node("root");
            root.AppendChild(new Node("a"));
            return root.FindItem<Node>(null!);
        }, null);

        DemoConsole.Case("предикат всегда false", () =>
        {
            var root = new Node("root");
            root.AppendChildren(new Node("a"), new Node("b"));
            return root.FindItem<Node>(_ => false);
        }, null);
    }

    private static void CycleProtection()
    {
        DemoConsole.Note("Защита от циклов — важное!");

        DemoConsole.CaseThrows<ArgumentException>("AppendChild(this) → ArgumentException", () =>
        {
            var a = new Node("a");
            a.AppendChild(a);
        });

        DemoConsole.CaseThrows<InvalidOperationException>("AppendChild(ancestor) → InvalidOperationException", () =>
        {
            var root = new Node("root");
            var child = new Node("child");
            root.AppendChild(child);
            child.AppendChild(root);
        });

        DemoConsole.CaseThrows<InvalidOperationException>("AppendChild(бабушка) с 3 уровней", () =>
        {
            var root = new Node("root");
            var l1 = new Node("L1");
            var l2 = new Node("L2");
            root.AppendChild(l1);
            l1.AppendChild(l2);
            l2.AppendChild(root);
        });
    }

    private static void KnownLimitations()
    {
        DemoConsole.Note("Известные ограничения: проверка циклов идёт только ВВЕРХ по предкам");

        DemoConsole.Case("Ребёнок может стать ребёнком своего сиблинга (не поймается)", () =>
        {
            var root = new Node("root");
            var a = new Node("a");
            var b = new Node("b");
            root.AppendChildren(a, b);
            a.AppendChild(b);
            return ReferenceEquals(b.Parent, a);
        }, true);

        DemoConsole.Note("То есть это не «настоящее» дерево, а ориентированный граф. " +
                         "Если нужна строгая структура — надо доп. проверка на «уже где-то в дереве».");
    }
}