using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class PaginationHelperDemo
{
    public static Task Run()
    {
        DemoConsole.Part("PaginationHelper (ядро расчёта PaginationModel)");
        CtorBounds();
        OffsetBounds();
        CurrentPageNavigation();
        WindowBoundaries();
        ZeroTotalPages();
        ToStringCheck();
        return Task.CompletedTask;
    }

    private static void CtorBounds()
    {
        DemoConsole.Note("ctor: itemsOnPage<1 → 1; totalItems<0 → 0");
        DemoConsole.Case("itemsOnPage=0 → 1", () => new PaginationHelper(0, 100).ItemsOnPage, 1);
        DemoConsole.Case("itemsOnPage=-5 → 1", () => new PaginationHelper(-5, 100).ItemsOnPage, 1);
        DemoConsole.Case("totalItems=-5 → 0", () => new PaginationHelper(10, -5).TotalItems, 0);
        DemoConsole.Case("totalItems=-5 → TotalPages=0", () => new PaginationHelper(10, -5).TotalPages, 0);
    }

    private static void OffsetBounds()
    {
        DemoConsole.Note("Offset clamp [1..9]");
        DemoConsole.Case("offset=0 → 1", () => new PaginationHelper(10, 100, 1, 0).Offset, 1);
        DemoConsole.Case("offset=-5 → 1", () => new PaginationHelper(10, 100, 1, -5).Offset, 1);
        DemoConsole.Case("offset=4 → 4", () => new PaginationHelper(10, 100, 1, 4).Offset, 4);
        DemoConsole.Case("offset=15 → 9", () => new PaginationHelper(10, 100, 1, 15).Offset, 9);
    }

    private static void CurrentPageNavigation()
    {
        DemoConsole.Note("CurrentPage: границы и NotValidIndex");
        DemoConsole.Case("currentPage=0 → 1, NotValidIndex", () =>
        {
            var h = new PaginationHelper(10, 100, 0);
            return (h.CurrentPage, h.NotValidIndex);
        }, (1, true));

        DemoConsole.Case("currentPage=-5 → 1, NotValidIndex", () =>
        {
            var h = new PaginationHelper(10, 100, -5);
            return (h.CurrentPage, h.NotValidIndex);
        }, (1, true));

        DemoConsole.Case("currentPage=999 → TotalPages(10)", () =>
        {
            var h = new PaginationHelper(10, 100, 999);
            return (h.CurrentPage, h.NotValidIndex);
        }, (10, true));

        DemoConsole.Case("currentPage=5 (в середине) — валидная", () =>
        {
            var h = new PaginationHelper(10, 100, 5);
            return (h.CurrentPage, h.NotValidIndex);
        }, (5, false));

        DemoConsole.Case("SkipItems для page=3, size=10 == 20", () =>
            new PaginationHelper(10, 100, 3).SkipItems, 20);
    }

    private static void WindowBoundaries()
    {
        DemoConsole.Note("Окно StartPage..EndPage — граница Offset+1");
        DemoConsole.Case("TotalPages=10, Offset=4, currentPage=1 → [1..9]", () =>
        {
            var h = new PaginationHelper(10, 100, 1, 4);
            return $"{h.StartPage}..{h.EndPage}";
        }, "1..9");

        DemoConsole.Case("... currentPage=4 → [1..9]", () =>
        {
            var h = new PaginationHelper(10, 100, 4, 4);
            return $"{h.StartPage}..{h.EndPage}";
        }, "1..9");

        DemoConsole.Case("... currentPage=5 → [1..9]", () =>
        {
            var h = new PaginationHelper(10, 100, 5, 4);
            return $"{h.StartPage}..{h.EndPage}";
        }, "1..9");

        DemoConsole.Case("... currentPage=6 → [2..10]", () =>
        {
            var h = new PaginationHelper(10, 100, 6, 4);
            return $"{h.StartPage}..{h.EndPage}";
        }, "2..10");

        DemoConsole.Case("... currentPage=10 → [2..10]", () =>
        {
            var h = new PaginationHelper(10, 100, 10, 4);
            return $"{h.StartPage}..{h.EndPage}";
        }, "2..10");

        DemoConsole.Note("Флаги HasItemsBefore/After на краях и в середине");
        DemoConsole.Case("currentPage=1 → Before=False, After=True", () =>
        {
            var h = new PaginationHelper(10, 100, 1, 4);
            return (h.HasItemsBefore, h.HasItemsAfter);
        }, (false, true));

        DemoConsole.Case("currentPage=5 → Before=True, After=True", () =>
        {
            var h = new PaginationHelper(10, 100, 5, 4);
            return (h.HasItemsBefore, h.HasItemsAfter);
        }, (false, true));  // window [1..9], StartPage=1 → Before=false, EndPage=9<10 → After=true

        DemoConsole.Case("currentPage=10 → Before=True, After=False", () =>
        {
            var h = new PaginationHelper(10, 100, 10, 4);
            return (h.HasItemsBefore, h.HasItemsAfter);
        }, (true, false));
    }

    private static void ZeroTotalPages()
    {
        DemoConsole.Note("totalItems=0 → TotalPages=0 — интересный крайний случай");
        DemoConsole.Case("TotalPages=0", () =>
            new PaginationHelper(10, 0).TotalPages, 0);

        DemoConsole.Case("TotalPages=0, currentPage=1 → Start=1, End=0", () =>
        {
            var h = new PaginationHelper(10, 0, 1);
            return (h.StartPage, h.EndPage);
        }, (1, 0));

        DemoConsole.Note("Наблюдение: при TotalPages=0 в setter нет верхней границы " +
                         "(guard `TotalPages > 0 &&`), поэтому CurrentPage можно выставить в 999 — не скорректируется");
        DemoConsole.Case("TotalPages=0, CurrentPage=999 (ручной set) → 999", () =>
        {
            var h = new PaginationHelper(10, 0, 1);
            h.CurrentPage = 999;
            return h.CurrentPage;
        }, 999);
    }

    private static void ToStringCheck()
    {
        DemoConsole.Note("ToString — диагностическая строка");
        DemoConsole.Case("page=1 → содержит \"Items\"", () =>
            new PaginationHelper(10, 100, 1).ToString().Contains("Items"), true);
        DemoConsole.Case("page=1 → содержит \"FirstPage:True\"", () =>
            new PaginationHelper(10, 100, 1).ToString().Contains("FirstPage:True"), true);
        DemoConsole.Case("page=10 → содержит \"LastPage:True\"", () =>
            new PaginationHelper(10, 100, 10).ToString().Contains("LastPage:True"), true);
    }
}