using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class ConsoleMenuDemo
{
    public static Task Run()
    {
        DemoConsole.Part("ConsoleMenu / ConsoleMenuItem");

        DemoConsole.Note("ConsoleMenu.Run() интерактивен и ждёт ввода с клавиатуры. " +
                         "Во избежания зависания тестируется всё кроме этого");

        MenuItem();
        MenuState();
        AddItems();
        ManualInvocation();
        return Task.CompletedTask;
    }

    private static void MenuItem()
    {
        DemoConsole.Note("ConsoleMenuItem — свойства");
        var counter = 0;
        var item = new ConsoleMenuItem(ConsoleKey.A, "Alpha", () => counter++);
        DemoConsole.Case("Key == ConsoleKey.A", () => item.Key, ConsoleKey.A);
        DemoConsole.Case("Title == \"Alpha\"", () => item.Title, "Alpha");
        DemoConsole.Case("Action != null", () => item.Action is not null, true);
        DemoConsole.Case("Action вызывает инкремент", () => { item.Action(); return counter; }, 1);
    }

    private static void MenuState()
    {
        DemoConsole.Note("ConsoleMenu — начальное состояние");
        var m1 = new ConsoleMenu();
        DemoConsole.Case("Title == null (без ctor)", () => m1.Title, (string?)null);
        DemoConsole.Case("IsExitRequested == false", () => m1.IsExitRequested, false);
        DemoConsole.Case("Items.Count == 0", () => m1.Items.Count, 0);

        var m2 = new ConsoleMenu("My Title");
        DemoConsole.Case("Title из ctor(string)", () => m2.Title, "My Title");
    }

    private static void AddItems()
    {
        DemoConsole.Note("Add(ConsoleMenuItem) и Add(key, title, action)");
        var menu = new ConsoleMenu("Test");

        menu.Add(new ConsoleMenuItem(ConsoleKey.A, "A", () => { }));
        menu.Add(new ConsoleMenuItem(ConsoleKey.B, "B", () => { }));
        DemoConsole.Case("после 2× Add — Items.Count == 2", () => menu.Items.Count, 2);
        DemoConsole.Case("Items[0].Key", () => menu.Items[0].Key, ConsoleKey.A);
        DemoConsole.Case("Items[1].Title", () => menu.Items[1].Title, "B");

        menu.Add(ConsoleKey.C, "C", () => { });
        DemoConsole.Case("после Add(key,title,action) — 3", () => menu.Items.Count, 3);
        DemoConsole.Case("Items[2].Key", () => menu.Items[2].Key, ConsoleKey.C);
        DemoConsole.Case("Items[2].Title", () => menu.Items[2].Title, "C");
    }

    private static void ManualInvocation()
    {
        DemoConsole.Note("Симуляция «нажатий» — вручную вызываем Action у пунктов");
        var counter = 0;
        var menu = new ConsoleMenu();
        menu.Add(ConsoleKey.A, "A", () => counter += 1);
        menu.Add(ConsoleKey.B, "B", () => counter += 10);
        menu.Add(ConsoleKey.C, "C", () => counter += 100);

        DemoConsole.Case("сумма всех Action == 111", () =>
        {
            foreach (var item in menu.Items) item.Action();
            return counter;
        }, 111);

        DemoConsole.Note("IsExitRequested можно ставить изнутри Action — тогда Run() завершится");
        var m = new ConsoleMenu();
        DemoConsole.Case("по умолчанию false", () => m.IsExitRequested, false);
        DemoConsole.Case("set → true", () => { m.IsExitRequested = true; return m.IsExitRequested; }, true);
        DemoConsole.Case("set → false", () => { m.IsExitRequested = false; return m.IsExitRequested; }, false);
    }
}