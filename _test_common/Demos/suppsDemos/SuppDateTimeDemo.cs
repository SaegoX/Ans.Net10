using System.Globalization;
using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class SuppDateTimeDemo
{
    public static Task Run()
    {
        DemoConsole.Part("SuppDateTime (static)");
        Environment();
        Properties();
        Formats();
        return Task.CompletedTask;
    }

    private static void Environment()
    {
        DemoConsole.Note("Где запущено — TimeZoneInfo и Culture");
        DemoConsole.Case("Culture = " + CultureInfo.CurrentCulture.Name, () => CultureInfo.CurrentCulture.Name);
        DemoConsole.Case("TimeZone = " + TimeZoneInfo.Local.Id, () => TimeZoneInfo.Local.Id);
        DemoConsole.Note("TODO: инъекция времени (IClock) отсутствует — Current = DateTime.Now всегда");
    }

    private static void Properties()
    {
        DemoConsole.Note("Свойства SuppDateTime — относительно локального времени");
        DemoConsole.Case("Current != default", () => SuppDateTime.Current != default, true);
        DemoConsole.Case("Today == Current.Date", () => SuppDateTime.Today == SuppDateTime.Current.Date, true);
        DemoConsole.Case("Yesterday == Today.AddDays(-1)", () => SuppDateTime.Yesterday == SuppDateTime.Today.AddDays(-1), true);
        DemoConsole.Case("Tomorrow == Today.AddDays(1)", () => SuppDateTime.Tomorrow == SuppDateTime.Today.AddDays(1), true);
        DemoConsole.Case("TomorrowAfter == Today.AddDays(2)", () => SuppDateTime.TomorrowAfter == SuppDateTime.Today.AddDays(2), true);
        DemoConsole.Case("CurrentYearBegin.Month == 1", () => SuppDateTime.CurrentYearBegin.Month, 1);
        DemoConsole.Case("CurrentYearBegin.Day == 1", () => SuppDateTime.CurrentYearBegin.Day, 1);
        DemoConsole.Case("CurrentYearBegin.TimeOfDay == 0", () => SuppDateTime.CurrentYearBegin.TimeOfDay, TimeSpan.Zero);
        DemoConsole.Case("NextYearBegin.Year == CurrentYearBegin.Year + 1", () =>
            SuppDateTime.NextYearBegin.Year == SuppDateTime.CurrentYearBegin.Year + 1, true);
        DemoConsole.Note("Граница года: 31.12.23:59:59 → 01.01.00:00:00 (проверить вручную в новогоднюю ночь)");
    }

    private static void Formats()
    {
        DemoConsole.Note("Константы и RTC3339FORMATS");
        DemoConsole.Case("ANS_DATETIME_FORMAT", () => SuppDateTime.ANS_DATETIME_FORMAT, "yyyy-'0'MM-dd HH:mm:ss");
        DemoConsole.Case("ANS_DATE_FORMAT", () => SuppDateTime.ANS_DATE_FORMAT, "yyyy-'0'MM-dd");
        DemoConsole.Case("ANS_DATETIME_FILE_FORMAT", () => SuppDateTime.ANS_DATETIME_FILE_FORMAT, "yyyy-'0'MM-dd_HH-mm-ss");

        DemoConsole.Case("RTC3339FORMATS.Count == 4", () => SuppDateTime.RTC3339FORMATS.Count, 4);
        DemoConsole.Case("содержит \"date\"", () => SuppDateTime.RTC3339FORMATS.ContainsKey("date"), true);
        DemoConsole.Case("содержит \"datetime\"", () => SuppDateTime.RTC3339FORMATS.ContainsKey("datetime"), true);
        DemoConsole.Case("содержит \"datetime-local\"", () => SuppDateTime.RTC3339FORMATS.ContainsKey("datetime-local"), true);
        DemoConsole.Case("содержит \"time\"", () => SuppDateTime.RTC3339FORMATS.ContainsKey("time"), true);

        DemoConsole.Note("RTC3339FORMATS — статический мутабельный Dictionary. " +
                         "Технически можно мутировать извне → не потокобезопасно. " +
                         "Идеально было бы FrozenDictionary / ReadOnlyDictionary.");
    }
}