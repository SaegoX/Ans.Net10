namespace _test_common.Framework;

/// <summary>Меню по дереву демок: категория → класс → метод → кейс.</summary>
public sealed class DemoMenu
{
    private readonly DemoRegistry _registry;

    public DemoMenu(DemoRegistry registry) => _registry = registry;

    public void Run()
    {
        while (true)
        {
            var categories = _registry.Entries
                .Select(e => e.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            Console.WriteLine();
            Console.WriteLine("=== Демки библиотеки ===");
            for (int i = 0; i < categories.Count; i++)
                Console.WriteLine($"[{i + 1}] {categories[i]}");
            Console.WriteLine("[A] Прогнать всё");
            Console.WriteLine("[0] Выход");
            Console.Write("> ");

            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input)) continue;
            if (input == "0") return;

            if (input.Equals("A", StringComparison.OrdinalIgnoreCase))
            {
                RunAll();
                continue;
            }

            if (int.TryParse(input, out var idx) && idx >= 1 && idx <= categories.Count)
                ShowCategory(categories[idx - 1]);
            else
                Console.WriteLine("Не понял ввод.");
        }
    }

    private void ShowCategory(string category)
    {
        var entries = _registry.Entries.Where(e => e.Category == category).ToList();

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine($"=== {category} ===");
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                Console.WriteLine($"[{i + 1}] {e.TargetType.Name}.{e.DisplayName} ({e.Cases.Count} кейсов)");
            }
            Console.WriteLine("[A] Прогнать всю категорию");
            Console.WriteLine("[0] Назад");
            Console.Write("> ");

            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input)) continue;
            if (input == "0") return;

            if (input.Equals("A", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var e in entries) RunEntry(e);
                continue;
            }

            if (int.TryParse(input, out var idx) && idx >= 1 && idx <= entries.Count)
                ShowEntry(entries[idx - 1]);
            else
                Console.WriteLine("Не понял ввод.");
        }
    }

    private void ShowEntry(DemoEntry entry)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine($"=== {entry.TargetType.Name}.{entry.DisplayName} ===");
            if (!string.IsNullOrEmpty(entry.Description))
                Console.WriteLine(entry.Description);
            Console.WriteLine();

            for (int i = 0; i < entry.Cases.Count; i++)
            {
                var c = entry.Cases[i];
                var args = FormatArgs(c.Args);
                var exp = c.ExpectException
                    ? "исключение"
                    : (c.ShouldVerify ? FormatValue(c.Expected) : "—");
                Console.WriteLine($"[{i + 1}] {c.Label,-30} args={args,-30} exp={exp}");
            }
            Console.WriteLine("[A] Прогнать все кейсы");
            Console.WriteLine("[0] Назад");
            Console.Write("> ");

            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input)) continue;
            if (input == "0") return;

            if (input.Equals("A", StringComparison.OrdinalIgnoreCase))
            {
                RunEntry(entry);
                continue;
            }

            if (int.TryParse(input, out var idx) && idx >= 1 && idx <= entry.Cases.Count)
                PrintResult(entry.Cases[idx - 1].Label, DemoRunner.Run(entry, entry.Cases[idx - 1]));
            else
                Console.WriteLine("Не понял ввод.");
        }
    }

    private void RunEntry(DemoEntry entry)
    {
        int total = 0, ok = 0;
        foreach (var c in entry.Cases)
        {
            var r = DemoRunner.Run(entry, c);
            total++;
            if (r.Success) ok++;
            PrintResult(c.Label, r);
        }
        Console.WriteLine($"ИТОГО: {ok}/{total} прошло.");
    }

    private void RunAll()
    {
        int total = 0, ok = 0;
        foreach (var e in _registry.Entries)
        {
            Console.WriteLine();
            Console.WriteLine($"--- {e.TargetType.Name}.{e.DisplayName} ---");
            foreach (var c in e.Cases)
            {
                var r = DemoRunner.Run(e, c);
                total++;
                if (r.Success) ok++;
                PrintResult(c.Label, r);
            }
        }
        Console.WriteLine();
        Console.WriteLine($"=== ИТОГО: {ok}/{total} ===");
    }

    private static void PrintResult(string label, DemoResult r)
    {
        var mark = r.Success ? "OK  " : "FAIL";
        Console.WriteLine($"  [{mark}] {label} ({r.Duration.TotalMilliseconds:F2} ms)");
        Console.WriteLine($"         actual   = {FormatValue(r.Actual)}");
        if (r.ExpectException)
            Console.WriteLine("         expected = исключение");
        else if (r.ShouldVerify)
            Console.WriteLine($"         expected = {FormatValue(r.Expected)}");
        if (r.Exception is not null)
            Console.WriteLine($"         exception= {r.Exception.GetType().Name}: {r.Exception.Message}");
    }

    private static string FormatArgs(object?[] args)
        => "(" + string.Join(", ", args.Select(FormatValue)) + ")";

    private static string FormatValue(object? v)
    {
        if (v is null) return "null";
        if (v is string s) return "\"" + s.Replace("\n", "\\n").Replace("\t", "\\t") + "\"";
        if (v is Array arr) return "[" + string.Join(", ", arr.Cast<object?>().Select(FormatValue)) + "]";
        return v.ToString() ?? "";
    }
}