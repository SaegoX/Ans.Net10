using System.Diagnostics;
using Ans.Net10.Common;

namespace _test_common.Framework;

/// <summary>
/// Управляющий класс для запуска кейсов, ловли исключений, печатати результата и вердикта OK/FAIL
/// </summary>
public static class DemoConsole
{
    /// <summary>Секция отдела (папки библиотеки).</summary>
    public static void Section(string name)
        => SuppConsole.SectionStart(name);

    /// <summary>Подраздел (класс).</summary>
    public static void Part(string name)
        => SuppConsole.PartStart(name);

    /// <summary>Строка-комментарий внутри демки.</summary>
    public static void Note(string text)
        => SuppConsole.WriteColor(ConsoleColor.DarkGray, $"  · {text}\n");

    /// <summary>Просто действие (void).</summary>
    public static void Case(string label, Action action)
    {

        var sw = Stopwatch.StartNew();
        Exception? ex = null;
        try { action(); }
        catch (Exception e) { ex = e; }
        sw.Stop();
        Print(label, ex is null ? "OK" : "FAIL", null, null, ex, sw.Elapsed);

    }

    /// <summary>Функция — печатаем, что вернулось, без проверки.</summary>
    public static void Case<T>(string label, Func<T> func)
    {

        var sw = Stopwatch.StartNew();
        T? actual = default;
        Exception? ex = null;
        try { actual = func(); }
        catch (Exception e) { ex = e; }
        sw.Stop();
        Print(label, ex is null ? "OK" : "FAIL", Format(actual), null, ex, sw.Elapsed);

    }

    /// <summary>Функция — сравниваем с ожиданием.</summary>
    public static void Case<T>(string label, Func<T> func, T expected)
    {

        var sw = Stopwatch.StartNew();
        T? actual = default;
        Exception? ex = null;
        try { actual = func(); }
        catch (Exception e) { ex = e; }
        sw.Stop();
        bool ok = ex is null && Equals(actual, expected);
        Print(label, ok ? "OK" : "FAIL", Format(actual), Format(expected), ex, sw.Elapsed);

    }

    /// <summary>Асинхронная версия без ожидания.</summary>
    public static async Task CaseAsync<T>(string label, Func<Task<T>> func)
    {

        var sw = Stopwatch.StartNew();
        T? actual = default;
        Exception? ex = null;
        try { actual = await func(); }
        catch (Exception e) { ex = e; }
        sw.Stop();
        Print(label, ex is null ? "OK" : "FAIL", Format(actual), null, ex, sw.Elapsed);

    }

    /// <summary>Асинхронная версия с ожиданием.</summary>
    public static async Task CaseAsync<T>(string label, Func<Task<T>> func, T expected)
    {

        var sw = Stopwatch.StartNew();
        T? actual = default;
        Exception? ex = null;
        try { actual = await func(); }
        catch (Exception e) { ex = e; }
        sw.Stop();
        bool ok = ex is null && Equals(actual, expected);
        Print(label, ok ? "OK" : "FAIL", Format(actual), Format(expected), ex, sw.Elapsed);

    }

    private static void Print(string label, string mark, string? actual, string? expected, Exception? ex, TimeSpan dt)
    {

        var color = mark == "OK" ? ConsoleColor.Green : ConsoleColor.Red;
        Console.Write("    [");
        SuppConsole.WriteColor(color, mark.PadRight(4));
        Console.Write($"] {label,-34}");

        if (actual is not null) Console.Write($" -> {actual}");
        if (expected is not null) Console.Write($"   (ожидалось: {expected})");
        Console.WriteLine($"   {dt.TotalMilliseconds:F2} ms");

        if (ex is not null)
            SuppConsole.WriteColor(ConsoleColor.Red, $"             исключение: {ex.GetType().Name}: {ex.Message}\n");

    }

    private static string Format(object? v) => v switch
    {

        null => "null",
        string s => "\"" + s.Replace("\n", "\\n").Replace("\t", "\\t") + "\"",
        bool b => b ? "true" : "false",
        Array a => "[" + string.Join(", ", a.Cast<object?>().Select(Format)) + "]",
        _ => v.ToString() ?? ""

    };

    /// <summary>Ожидаем, что действие бросит исключение указанного типа.</summary>
    public static void CaseThrows<TException>(string label, Action action)
        where TException : Exception
    {
        var sw = Stopwatch.StartNew();
        Exception? ex = null;
        try { action(); }
        catch (Exception e) { ex = e; }
        sw.Stop();

        if (ex is TException)
            Print(label, "OK", $"<{typeof(TException).Name}>", null, null, sw.Elapsed);
        else if (ex is null)
            Print(label, "FAIL", "не брошено", $"<{typeof(TException).Name}>", null, sw.Elapsed);
        else
            Print(label, "FAIL", $"<{ex.GetType().Name}>", $"<{typeof(TException).Name}>", ex, sw.Elapsed);
    }

    /// <summary>Асинхронная версия.</summary>
    public static async Task CaseThrowsAsync<TException>(string label, Func<Task> func)
        where TException : Exception
    {
        var sw = Stopwatch.StartNew();
        Exception? ex = null;
        try { await func(); }
        catch (Exception e) { ex = e; }
        sw.Stop();

        if (ex is TException)
            Print(label, "OK", $"<{typeof(TException).Name}>", null, null, sw.Elapsed);
        else if (ex is null)
            Print(label, "FAIL", "не брошено", $"<{typeof(TException).Name}>", null, sw.Elapsed);
        else
            Print(label, "FAIL", $"<{ex.GetType().Name}>", $"<{typeof(TException).Name}>", ex, sw.Elapsed);
    }
}