using _test_common.Framework;
using Ans.Net10.Common;
using Microsoft.Extensions.DependencyInjection;

namespace _test_common.Demos;

public static class Exts__diDemo
{
    public static Task Run()
    {
        DemoConsole.Part("Exts__di");
        EmptyCollection();
        Registered();
        NotRegistered();
        Interface();
        return Task.CompletedTask;
    }

    private static void EmptyCollection()
    {
        DemoConsole.Note("пустая коллекция → false");
        var sc = new ServiceCollection();
        DemoConsole.Case("HasService(typeof(string)) == false", () => sc.HasService(typeof(string)), false);
    }

    private static void Registered()
    {
        DemoConsole.Note("тип зарегистрирован → true");
        var sc = new ServiceCollection();
        sc.AddSingleton<IDisposable, DummyDisposable>();
        DemoConsole.Case("HasService(IDisposable)", () => sc.HasService(typeof(IDisposable)), true);
        DemoConsole.Case("HasService(DummyDisposable)", () => sc.HasService(typeof(DummyDisposable)), false);
    }

    private static void NotRegistered()
    {
        DemoConsole.Note("тип не зарегистрирован → false");
        var sc = new ServiceCollection();
        sc.AddSingleton<IDisposable, DummyDisposable>();
        DemoConsole.Case("HasService(typeof(string))", () => sc.HasService(typeof(string)), false);
    }

    private static void Interface()
    {
        DemoConsole.Note("Регистрация по конкретному типу — интерфейс уже не считается");
        var sc = new ServiceCollection();
        sc.AddSingleton<DummyDisposable>();
        DemoConsole.Case("HasService(DummyDisposable) == true", () => sc.HasService(typeof(DummyDisposable)), true);
        DemoConsole.Case("HasService(IDisposable) == false", () => sc.HasService(typeof(IDisposable)), false);
    }

    private sealed class DummyDisposable : IDisposable { public void Dispose() { } }
}