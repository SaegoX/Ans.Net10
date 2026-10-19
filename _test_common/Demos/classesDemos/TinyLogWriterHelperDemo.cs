using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class TinyLogWriterHelperDemo
{
    public static async Task Run()
    {
        DemoConsole.Part("TinyLogWriterHelper");
        await CtorChecks();
        await AppendAndSave();
        await AppendLineAndFormat();
        await AppendLogToConsole();
        await RewriteChecks();
        await AutoFlushCheck();
    }

    private static string TempFile() => Path.Combine(Path.GetTempPath(), $"tln_{Guid.NewGuid():N}.log");

    private static async Task CtorChecks()
    {
        DemoConsole.Note("ctor — guard");
        DemoConsole.CaseThrows<ArgumentException>("filename=null", () => _ = new TinyLogWriterHelper(null!, false));
        DemoConsole.CaseThrows<ArgumentException>("filename=\"\"", () => _ = new TinyLogWriterHelper("", false));

        var f = TempFile();
        try
        {
            var w = new TinyLogWriterHelper(f, rewrite: true);
            DemoConsole.Case("Filename установлен", () => w.Filename, f);
            DemoConsole.Case("Length по умолчанию 300", () => w.Length, 300);
            DemoConsole.Case("файл создан", () => File.Exists(f), true);
        }
        finally { try { File.Delete(f); } catch { } }
    }

    private static async Task AppendAndSave()
    {
        DemoConsole.Note("Append + SaveAsync");
        var f = TempFile();
        try
        {
            var w = new TinyLogWriterHelper(f, rewrite: true);
            w.Append("hello ");
            w.Append("world");
            await w.SaveAsync();
            DemoConsole.Case("файл содержит \"hello world\"", () => File.ReadAllText(f), "hello world");

            w.Append(" | again");
            await w.SaveAsync();
            DemoConsole.Case("второй Save дописывает (Append-режим)", () =>
                File.ReadAllText(f).Contains("hello world | again"), true);
        }
        finally { try { File.Delete(f); } catch { } }
    }

    private static async Task AppendLineAndFormat()
    {
        DemoConsole.Note("AppendLine / AppendFormat");
        var f = TempFile();
        try
        {
            var w = new TinyLogWriterHelper(f, rewrite: true);
            w.AppendLine("line1");
            w.AppendLine("line2");
            w.Append("x={0},y={1}", 1, 2);
            w.AppendLine(" fmt={0}", "end");
            await w.SaveAsync();

            var text = File.ReadAllText(f);
            DemoConsole.Case("содержит \"line1\"", () => text.Contains("line1"), true);
            DemoConsole.Case("содержит \"line2\"", () => text.Contains("line2"), true);
            DemoConsole.Case("содержит \"x=1,y=2\"", () => text.Contains("x=1,y=2"), true);
            DemoConsole.Case("содержит \"fmt=end\"", () => text.Contains("fmt=end"), true);

            DemoConsole.Note("Append(null) не падает — StringBuilder.Append(null) ок");
            w.Append((string?)null);
            await w.SaveAsync();
        }
        finally { try { File.Delete(f); } catch { } }
    }

    private static async Task AppendLogToConsole()
    {
        DemoConsole.Note("AppendLog / AppendLineLog — дублируют в консоль (увидишь ниже)");
        var f = TempFile();
        try
        {
            var w = new TinyLogWriterHelper(f, rewrite: true);
            w.AppendLog("[LOG] {0}={1}", "k", 42);
            w.AppendLineLog("[LOG-LINE] {0}", "done");
            await w.SaveAsync();
            DemoConsole.Case("файл содержит \"[LOG] k=42\"", () => File.ReadAllText(f).Contains("[LOG] k=42"), true);
            DemoConsole.Case("файл содержит \"[LOG-LINE] done\"", () => File.ReadAllText(f).Contains("[LOG-LINE] done"), true);
        }
        finally { try { File.Delete(f); } catch { } }
    }

    private static async Task RewriteChecks()
    {
        DemoConsole.Note("rewrite: true — файл очищается в ctor");
        var f = TempFile();
        try
        {
            File.WriteAllText(f, "OLD CONTENT");
            var w = new TinyLogWriterHelper(f, rewrite: true);
            DemoConsole.Case("файл очищен при ctor", () => File.ReadAllText(f), "");
            w.Append("new");
            await w.SaveAsync();
            DemoConsole.Case("после записи только new", () => File.ReadAllText(f), "new");
        }
        finally { try { File.Delete(f); } catch { } }

        DemoConsole.Note("rewrite: false — существующий контент сохраняется");
        var f2 = TempFile();
        try
        {
            File.WriteAllText(f2, "OLD|");
            var w = new TinyLogWriterHelper(f2, rewrite: false);
            w.Append("new");
            await w.SaveAsync();
            DemoConsole.Case("содержит \"OLD|new\"", () => File.ReadAllText(f2), "OLD|new");
        }
        finally { try { File.Delete(f2); } catch { } }
    }

    private static async Task AutoFlushCheck()
    {
        DemoConsole.Note("_test() — при _count > Length запускает Task.Run(SaveAsync) в фоне. " +
                         "Гарантий порядка/завершения нет (fire-and-forget).");

        var f = TempFile();
        try
        {
            var w = new TinyLogWriterHelper(f, rewrite: true, length: 2);
            w.Append("a");   // count=1
            w.Append("b");   // count=2 — не больше Length
            w.Append("c");   // count=3 > 2 — фоновый SaveAsync
            DemoConsole.Note("Ждём 200мс, чтобы фоновый SaveAsync отработал");
            await Task.Delay(200);
            DemoConsole.Case("файл создан", () => File.Exists(f), true);

            DemoConsole.Note("ОСОБЕННОСТЬ: _test срабатывает по счётчику и НЕ сбрасывает _count сразу — " +
                             "фоновые SaveAsync могут гоняться. Это fire-and-forget.");
        }
        finally { try { File.Delete(f); } catch { } }
    }
}