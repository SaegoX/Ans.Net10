using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class Exts__datetimeDemo
{
    public static Task Run()
    {
        DemoConsole.Part("Exts__datetime");
        DateTimeConversions();
        GetTensesChecks();
        GetStartOfWeekChecks();
        GetStartOfMonthChecks();
        IsEqualChecks();
        HasTimeOfDayChecks();
        AnsFormats();
        DateOnlyChecks();
        TimeSpanChecks();
        return Task.CompletedTask;
    }

    private static void DateTimeConversions()
    {
        DemoConsole.Note("GetTimeOnly / GetDateOnly из DateTime");
        var dt = new DateTime(2026, 6, 15, 14, 30, 45);
        DemoConsole.Case("GetTimeOnly", () => dt.GetTimeOnly(), new TimeOnly(14, 30, 45));
        DemoConsole.Case("GetDateOnly", () => dt.GetDateOnly(), new DateOnly(2026, 6, 15));
    }

    private static void GetTensesChecks()
    {
        DemoConsole.Note("GetTenses — Past / Present / Future относительно Today");
        var today = DateTime.Now.Date;
        DemoConsole.Case("вчера → Past", () => today.AddDays(-1).GetTenses(), TensesEnum.Past);
        DemoConsole.Case("сегодня → Present", () => today.GetTenses(), TensesEnum.Present);
        DemoConsole.Case("завтра → Future", () => today.AddDays(1).GetTenses(), TensesEnum.Future);
        DemoConsole.Case("сегодня 23:59:59 → Present", () => today.AddHours(23).AddMinutes(59).GetTenses(), TensesEnum.Present);
    }

    private static void GetStartOfWeekChecks()
    {
        DemoConsole.Note("GetStartOfWeek — семантические проверки (не зависят от года)");

        // 2024-06-15 — гарантированно суббота
        var someDate = new DateTime(2024, 6, 15);

        DemoConsole.Case("result.DayOfWeek == startOfWeek (Monday)", () =>
            someDate.GetStartOfWeek(DayOfWeek.Monday).DayOfWeek, DayOfWeek.Monday);
        DemoConsole.Case("result.DayOfWeek == startOfWeek (Sunday)", () =>
            someDate.GetStartOfWeek(DayOfWeek.Sunday).DayOfWeek, DayOfWeek.Sunday);
        DemoConsole.Case("result.DayOfWeek == startOfWeek (Saturday)", () =>
            someDate.GetStartOfWeek(DayOfWeek.Saturday).DayOfWeek, DayOfWeek.Saturday);

        DemoConsole.Case("result <= someDate и разница < 7 дней", () =>
        {
            var r = someDate.GetStartOfWeek(DayOfWeek.Monday);
            return r <= someDate && (someDate - r).TotalDays < 7;
        }, true);

        DemoConsole.Case("result.TimeOfDay == 00:00", () =>
            someDate.GetStartOfWeek(DayOfWeek.Monday).TimeOfDay, TimeSpan.Zero);

        DemoConsole.Note("Конкретный пример: 2024-06-15 (суббота) + Monday → 2024-06-10");
        DemoConsole.Case("2024-06-15 (Sat), start=Monday → 2024-06-10", () =>
            someDate.GetStartOfWeek(DayOfWeek.Monday), new DateTime(2024, 6, 10));
        DemoConsole.Case("2024-06-15 (Sat), start=Saturday → 2024-06-15", () =>
            someDate.GetStartOfWeek(DayOfWeek.Saturday), new DateTime(2024, 6, 15));

        DemoConsole.Note("Boundary: DateTime.MinValue → overflow при вычитании дней");
        DemoConsole.CaseThrows<ArgumentOutOfRangeException>(
            "MinValue, start=Sunday → ArgumentOutOfRangeException (overflow)", () =>
            {
                var _ = DateTime.MinValue.GetStartOfWeek(DayOfWeek.Sunday);
            });
    }

    private static void GetStartOfMonthChecks()
    {
        DemoConsole.Note("GetStartOfMonth — на фиксированной дате");
        DemoConsole.Case("2024-06-15 → 2024-06-01", () =>
            new DateTime(2024, 6, 15).GetStartOfMonth(), new DateTime(2024, 6, 1));
        DemoConsole.Case("2024-12-31 → 2024-12-01", () =>
            new DateTime(2024, 12, 31).GetStartOfMonth(), new DateTime(2024, 12, 1));
        DemoConsole.Case("уже 1-е → тот же день (время сброшено)", () =>
            new DateTime(2024, 6, 1, 15, 30, 0).GetStartOfMonth(), new DateTime(2024, 6, 1));
        DemoConsole.Case("результат.Day == 1", () =>
            new DateTime(2024, 6, 15).GetStartOfMonth().Day, 1);
    }

    private static void IsEqualChecks()
    {
        DemoConsole.Note("IsEqual(value2, minutesDiff) — |diff| <= minutesDiff");
        var a = new DateTime(2024, 6, 15, 12, 0, 0);

        DemoConsole.Case("тот же → true (0 min)", () => a.IsEqual(a, 0), true);
        DemoConsole.Case("+5 мин, diff=5 → true (граница)", () =>
            a.IsEqual(a.AddMinutes(5), 5), true);
        DemoConsole.Case("+5 мин, diff=4 → false", () =>
            a.IsEqual(a.AddMinutes(5), 4), false);
        DemoConsole.Case("-5 мин, diff=5 → true (abs)", () =>
            a.IsEqual(a.AddMinutes(-5), 5), true);
        DemoConsole.Case("+5.5 мин, diff=5 → false", () =>
            a.IsEqual(a.AddSeconds(330), 5), false);
        DemoConsole.Note("Отрицательный minutesDiff — поведение нестандартное");
        DemoConsole.Case("diff=-1, dates равны → true (0 <= -1? нет)", () =>
            a.IsEqual(a, -1));
    }

    private static void HasTimeOfDayChecks()
    {
        DemoConsole.Note("HasTimeOfDay — TimeOfDay != 00:00:00");
        DemoConsole.Case("00:00:00 → false", () => new DateTime(2026, 1, 1, 0, 0, 0).HasTimeOfDay(), false);
        DemoConsole.Case("00:00:01 → true", () => new DateTime(2026, 1, 1, 0, 0, 1).HasTimeOfDay(), true);
        DemoConsole.Case("12:00:00 → true", () => new DateTime(2024, 1, 1, 12, 0, 0).HasTimeOfDay(), true);
        DemoConsole.Case("23:59:59.999 → true", () => new DateTime(2026, 1, 1, 23, 59, 59, 999).HasTimeOfDay(), true);
    }

    private static void AnsFormats()
    {
        DemoConsole.Note("GetAnsDate / GetAnsDateTime / GetAnsDateTimeForFile — на фиксированной дате 2024");
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);
        DemoConsole.Case("GetAnsDate", () => dt.GetAnsDate(), "2024-006-15");
        DemoConsole.Case("GetAnsDateTime", () => dt.GetAnsDateTime(), "2024-006-15 14:30:45");
        DemoConsole.Case("GetAnsDateTimeForFile", () => dt.GetAnsDateTimeForFile(), "2024-006-15_14-30-45");
    }

    private static void DateOnlyChecks()
    {
        DemoConsole.Note("DateOnly: GetDateTime / GetTenses");
        var d = new DateOnly(2024, 6, 15);
        DemoConsole.Case("GetDateTime → 00:00:00", () => d.GetDateTime(), new DateTime(2024, 6, 15, 0, 0, 0));

        var today = DateOnly.FromDateTime(DateTime.Now);
        DemoConsole.Case("вчера → Past", () => today.AddDays(-1).GetTenses(), TensesEnum.Past);
        DemoConsole.Case("сегодня → Present", () => today.GetTenses(), TensesEnum.Present);
        DemoConsole.Case("завтра → Future", () => today.AddDays(1).GetTenses(), TensesEnum.Future);
    }

    private static void TimeSpanChecks()
    {
        DemoConsole.Note("GetTotalMinutes — abs + RoundToInt");
        DemoConsole.Case("+90 мин → 90", () => TimeSpan.FromMinutes(90).GetTotalMinutes(), 90);
        DemoConsole.Case("-90 мин → 90 (abs)", () => TimeSpan.FromMinutes(-90).GetTotalMinutes(), 90);
        DemoConsole.Case("1ч 30м 45с → 91 (round)", () =>
            new TimeSpan(1, 30, 45).GetTotalMinutes(), 91);
        DemoConsole.Case("0 → 0", () => TimeSpan.Zero.GetTotalMinutes(), 0);
    }
}