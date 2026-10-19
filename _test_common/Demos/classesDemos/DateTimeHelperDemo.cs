using _test_common.Framework;
using Ans.Net10.Common;
using System.Globalization;

namespace _test_common.Demos;

public static class DateTimeHelperDemo
{
    public static Task Run()
    {
        DemoConsole.Note("Culture: " + CultureInfo.CurrentCulture.Name);
        DemoConsole.Part("DateTimeHelper");
        Snapshot();
        Boundaries();
        GetPassed_YesterdayTodayTomorrow();
        GetPassed_NearPastAndFuture();
        GetPassed_FarPastAndFuture();
        GetPassed_AddTimeBehavior();
        GetPassed_Null();
        GetPassed_DateOnly();
        return Task.CompletedTask;
    }

    private static void Snapshot()
    {
        DemoConsole.Note("Снимок времени фиксируется при создании");
        var h = new DateTimeHelper();
        DemoConsole.Case("Current != default", () => h.Current != default, true);
        DemoConsole.Case("Today = Current.Date", () => h.Today == h.Current.Date, true);
        DemoConsole.Case("Yesterday = Today - 1", () => h.Yesterday == h.Today.AddDays(-1), true);
        DemoConsole.Case("Tomorrow = Today + 1", () => h.Tomorrow == h.Today.AddDays(1), true);
        DemoConsole.Case("TomorrowAfter = Today + 2", () => h.TomorrowAfter == h.Today.AddDays(2), true);
        DemoConsole.Case("CurrentYearBegin = 1 Jan", () => h.CurrentYearBegin == new DateTime(h.Current.Year, 1, 1), true);
        DemoConsole.Case("NextYearBegin = 1 Jan next", () => h.NextYearBegin == h.CurrentYearBegin.AddYears(1), true);
    }

    private static void Boundaries()
    {
        DemoConsole.Note("Пограничные даты календаря — что конструктор их принимает?");

        DemoConsole.CaseThrows<ArgumentOutOfRangeException>("new DateTime(y, 13, 1) — 13-й месяц", () =>
            _ = new DateTime(2024, 13, 1));

        DemoConsole.CaseThrows<ArgumentOutOfRangeException>("new DateTime(y, 1, 32) — 32-й день", () =>
            _ = new DateTime(2024, 1, 32));

        DemoConsole.CaseThrows<ArgumentOutOfRangeException>("29 февраля НЕвисокосного года", () =>
            _ = new DateTime(2023, 2, 29));

        DemoConsole.Case("29 февраля ВИСОКОСНОГО года — валидно", () =>
        {
            var d = new DateTime(2024, 2, 29);
            return d.Day == 29 && d.Month == 2;
        }, true);

        DemoConsole.Case("31 декабря 23:59:59.999", () =>
        {
            var d = new DateTime(2024, 12, 31, 23, 59, 59, 999);
            return $"{d.Day:D2}.{d.Month:D2}.{d.Year} {d.TimeOfDay}";
        });

        DemoConsole.Case("1 января 00:00:00", () =>
        {
            var d = new DateTime(2024, 1, 1, 0, 0, 0);
            return d.TimeOfDay == TimeSpan.Zero;
        }, true);
    }

    private static void GetPassed_YesterdayTodayTomorrow()
    {
        DemoConsole.Note("GetPassed для сегодня/вчера/завтра/послезавтра (addTime=false, с подстановками)");
        var h = new DateTimeHelper();
        DemoConsole.Case("вчера", () => h.GetPassed(h.Yesterday, false, true));
        DemoConsole.Case("сегодня", () => h.GetPassed(h.Today, false, true));
        DemoConsole.Case("завтра", () => h.GetPassed(h.Tomorrow, false, true));
        DemoConsole.Case("послезавтра", () => h.GetPassed(h.TomorrowAfter, false, true));
    }

    private static void GetPassed_NearPastAndFuture()
    {
        DemoConsole.Note("GetPassed — этот год, но не вчера/сегодня/завтра");
        var h = new DateTimeHelper();

        // +5 дней — гарантированно будущее (но в этом году, если сегодня не 27 декабря+)
        var fiveDaysAhead = h.Today.AddDays(5);
        DemoConsole.Case("+5 дней — не пустая строка", () => !string.IsNullOrEmpty(h.GetPassed(fiveDaysAhead, false, true)), true);

        // -5 дней — точно в прошлом, но в этом году (если сегодня не 5 января-)
        var fiveDaysBack = h.Today.AddDays(-5);
        DemoConsole.Case("-5 дней — не пустая строка", () => !string.IsNullOrEmpty(h.GetPassed(fiveDaysBack, false, true)), true);
    }

    private static void GetPassed_FarPastAndFuture()
    {
        DemoConsole.Note("GetPassed — далёкое прошлое и будущее (прошлые/следующие года)");
        var h = new DateTimeHelper();
        DemoConsole.Case("год назад", () => h.GetPassed(new DateTime(h.Current.Year - 1, 6, 15), false, true));
        DemoConsole.Case("год вперёд", () => h.GetPassed(new DateTime(h.Current.Year + 1, 6, 15), false, true));
        DemoConsole.Case("DateTime.MinValue", () => h.GetPassed(DateTime.MinValue, false, true));
        DemoConsole.Case("DateTime.MaxValue", () => h.GetPassed(DateTime.MaxValue, false, true));
    }

    private static void GetPassed_AddTimeBehavior()
    {
        DemoConsole.Note("addTime=true при отсутствии времени — сбрасывается автоматически");
        var h = new DateTimeHelper();

        DemoConsole.Case("сегодня, addTime=true — без времени", () => !string.IsNullOrEmpty(h.GetPassed(h.Today, true, true)), true);

        var withTime = h.Today.AddHours(14).AddMinutes(30);
        DemoConsole.Case("сегодня 14:30, addTime=true — со временем", () => h.GetPassed(withTime, true, true));

        DemoConsole.Note("useYesterdayTodayTomorrow=false — без словесных подстановок");
        DemoConsole.Case("сегодня без подстановок", () => h.GetPassed(h.Today, false, false));
    }

    private static void GetPassed_Null()
    {
        DemoConsole.Note("null → Resources.Common.Text_Never");
        var h = new DateTimeHelper();
        DemoConsole.Case("(DateTime?)null", () => !string.IsNullOrEmpty(h.GetPassed((DateTime?)null, false, true)), true);
        DemoConsole.Case("(DateOnly?)null", () => !string.IsNullOrEmpty(h.GetPassed((DateOnly?)null, true)), true);
    }

    private static void GetPassed_DateOnly()
    {
        DemoConsole.Note("DateOnly перегрузки");
        var h = new DateTimeHelper();
        DemoConsole.Case("сегодня (факт)", () => h.GetPassed(h.Today, false, true));
        DemoConsole.Case("вчера (факт)", () => h.GetPassed(h.Yesterday, false, true));
        DemoConsole.Case("завтра (факт)", () => h.GetPassed(h.Tomorrow, false, true));
    }
}