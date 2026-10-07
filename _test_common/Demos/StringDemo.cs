using _test_common.Framework;

namespace _test_common.Demos;

[DemoClass(typeof(string), "Примеры (BCL)")]
public static class StringDemo
{
    [DemoMethod(Name = "Trim", Description = "Убирает пробелы с обоих концов строки")]
    [DemoCase("обычная строка", "  hello  ", Expected = "hello")]
    [DemoCase("пустая", "", Expected = "")]
    [DemoCase("только пробелы", "   ", Expected = "")]
    [DemoCase("юникод", "  привет  ", Expected = "привет")]
    [DemoCase("null → исключение", null, ExpectException = true)]
    public static object? Trim(string? s) => s!.Trim();

    [DemoMethod(Name = "StartsWithOrdinal", Description = "Проверка префикса без учёта культуры")]
    [DemoCase("совпадает", "hello", "he", Expected = true)]
    [DemoCase("не совпадает", "hello", "xx", Expected = false)]
    [DemoCase("пустой префикс", "hello", "", Expected = true)]
    [DemoCase("префикс длиннее", "hi", "hello", Expected = false)]
    [DemoCase("юникод", "привет", "при", Expected = true)]
    public static object? StartsWithOrdinal(string s, string prefix)
        => s.StartsWith(prefix, StringComparison.Ordinal);
}