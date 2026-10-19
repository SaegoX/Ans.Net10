using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class Exts__ioDemo
{
    public static Task Run()
    {
        DemoConsole.Part("Exts__io");
        GuardChecks();
        FilterBySpan();
        FilterByString();
        return Task.CompletedTask;
    }

    private static string MakeTempDir(out string[] files)
    {
        var dir = Path.Combine(Path.GetTempPath(), $"extsio_{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        files = new[] { "a.txt", "b.TXT", "c.jpg", "d.png", "e.Txt", "f" };
        foreach (var f in files)
            File.WriteAllText(Path.Combine(dir, f), "");
        return dir;
    }

    private static void GuardChecks()
    {
        DemoConsole.Note("Guard: null DirectoryInfo → ArgumentNullException");
        DemoConsole.CaseThrows<ArgumentNullException>("null dir, string", () =>
            _ = ((DirectoryInfo)null!).GetFilesByExtensions(".txt").ToList());
        DemoConsole.CaseThrows<ArgumentNullException>("null dir, span", () =>
            _ = ((DirectoryInfo)null!).GetFilesByExtensions(new[] { ".txt" }).ToList());
        DemoConsole.CaseThrows<ArgumentNullException>("string null", () =>
        {
            var dir = new DirectoryInfo(Path.GetTempPath());
            _ = dir.GetFilesByExtensions((string)null!).ToList();
        });
    }

    private static void FilterBySpan()
    {
        DemoConsole.Note("GetFilesByExtensions(DirectoryInfo, params ReadOnlySpan<string>)");
        var path = MakeTempDir(out _);
        try
        {
            var dir = new DirectoryInfo(path);

            DemoConsole.Case(".txt — регистронезависимый фильтр", () =>
            {
                var files = dir.GetFilesByExtensions(new[] { ".txt" })
                    .Select(x => x.Name).OrderBy(x => x).ToArray();
                return string.Join(",", files);
            }, "a.txt,b.TXT,e.Txt");

            DemoConsole.Case(".TXT — тоже case-insensitive", () =>
                dir.GetFilesByExtensions(new[] { ".TXT" })
                    .Select(x => x.Name).Count(), 3);

            DemoConsole.Case(".txt + .jpg", () =>
            {
                var files = dir.GetFilesByExtensions(new[] { ".txt", ".jpg" })
                    .Select(x => x.Name).OrderBy(x => x).ToArray();
                return string.Join(",", files);
            }, "a.txt,b.TXT,c.jpg,e.Txt");

            DemoConsole.Case("пустой массив расширений → 0", () =>
                dir.GetFilesByExtensions(Array.Empty<string>()).Count(), 0);

            DemoConsole.Case("неизвестное расширение → 0", () =>
                dir.GetFilesByExtensions(new[] { ".xyz" }).Count(), 0);
        }
        finally { try { Directory.Delete(path, true); } catch { } }
    }

    private static void FilterByString()
    {
        DemoConsole.Note("GetFilesByExtensions(DirectoryInfo, string) — ';' и ',' разделители");
        var path = MakeTempDir(out _);
        try
        {
            var dir = new DirectoryInfo(path);

            DemoConsole.Case("\".txt;.jpg\"", () =>
                dir.GetFilesByExtensions(".txt;.jpg").Count(), 4);
            DemoConsole.Case("\".txt,.jpg\"", () =>
                dir.GetFilesByExtensions(".txt,.jpg").Count(), 4);
            DemoConsole.Case("\".txt;;.jpg\" — пустые сегменты игнор", () =>
                dir.GetFilesByExtensions(".txt;;.jpg").Count(), 4);
            DemoConsole.Case("\".txt, .jpg\" — пробел НЕ тримится (нет Trim в Split) → 0", () =>
                dir.GetFilesByExtensions(".txt, .jpg").Count(), 3); // .txt работает, " .jpg" — нет

            DemoConsole.Case("\"\" → 0", () => dir.GetFilesByExtensions("").Count(), 0);
        }
        finally { try { Directory.Delete(path, true); } catch { } }
    }
}