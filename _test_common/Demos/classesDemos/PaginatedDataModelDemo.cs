using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class PaginatedDataModelDemo
{
    public static Task Run()
    {
        DemoConsole.Part("PaginatedDataModel");
        Order();
        PageBounds();
        ItemsOnPageBounds();
        MaxLimit();
        HasChecks();
        QueryTransform();
        return Task.CompletedTask;
    }

    private static void Order()
    {
        DemoConsole.Note("Order: null / пустая → Order=null, HasOrder=false");
        DemoConsole.Case("null → HasOrder=false", () => new PaginatedDataModel(null, 1, 10, 100, 10, 0).HasOrder, false);
        DemoConsole.Case("\"\" → HasOrder=false", () => new PaginatedDataModel("", 1, 10, 100, 10, 0).HasOrder, false);
        DemoConsole.Case("\"Name\" → HasOrder=true", () => new PaginatedDataModel("Name", 1, 10, 100, 10, 0).HasOrder, true);
        DemoConsole.Case("\"Name\" → Order.Items.Length=1", () =>
            new PaginatedDataModel("Name", 1, 10, 100, 10, 0).Order!.Items.Length, 1);
    }

    private static void PageBounds()
    {
        DemoConsole.Note("Page < 1 → коэрс в 1");
        DemoConsole.Case("page=0 → 1", () => new PaginatedDataModel(null, 0, 10, 100, 10, 0).Page, 1);
        DemoConsole.Case("page=-5 → 1", () => new PaginatedDataModel(null, -5, 10, 100, 10, 0).Page, 1);
        DemoConsole.Case("page=3 → 3", () => new PaginatedDataModel(null, 3, 10, 100, 10, 0).Page, 3);
    }

    private static void ItemsOnPageBounds()
    {
        DemoConsole.Note("itemsOnPage: 0 → default, <0 → max (или total если max<=0), >0 → max clamp");
        DemoConsole.Case("0 → defaultItemsOnPage=10", () =>
            new PaginatedDataModel(null, 1, 0, 100, 10, 0).ItemsOnPage, 10);
        DemoConsole.Case("-1, max=0 → totalItems=100", () =>
            new PaginatedDataModel(null, 1, -1, 100, 10, 0).ItemsOnPage, 100);
        DemoConsole.Case("-1, max=50 → 50", () =>
            new PaginatedDataModel(null, 1, -1, 100, 10, 50).ItemsOnPage, 50);
        DemoConsole.Case("30, max=100 → 30", () =>
            new PaginatedDataModel(null, 1, 30, 100, 10, 100).ItemsOnPage, 30);
        DemoConsole.Case("30, max=0 → 30 (no clamp)", () =>
            new PaginatedDataModel(null, 1, 30, 100, 10, 0).ItemsOnPage, 30);
    }

    private static void MaxLimit()
    {
        DemoConsole.Note("Жёсткий предел maxItemsOnPages");
        DemoConsole.Case("500, max=100 → 100", () =>
            new PaginatedDataModel(null, 1, 500, 1000, 10, 100).ItemsOnPage, 100);
        DemoConsole.Case("max=0 → no limit → 500", () =>
            new PaginatedDataModel(null, 1, 500, 1000, 10, 0).ItemsOnPage, 500);
    }

    private static void HasChecks()
    {
        DemoConsole.Note("HasPage");
        DemoConsole.Case("1, 10 → true", () => new PaginatedDataModel(null, 1, 10, 100, 10, 0).HasPage, true);
        DemoConsole.Case("0 (coerced 1), 10 → true", () => new PaginatedDataModel(null, 0, 10, 100, 10, 0).HasPage, true);
    }

    private static void QueryTransform()
    {
        DemoConsole.Note("GetQueryPaging / GetQueryOrdered");
        var data = Enumerable.Range(1, 25).ToList();
        var q = data.AsQueryable();

        DemoConsole.Case("page=1, size=10 → первые 10", () =>
            new PaginatedDataModel(null, 1, 10, 25, 10, 0).GetQueryPaging(q).ToList().First(), 1);
        DemoConsole.Case("page=2, size=10 → с 11-го", () =>
            new PaginatedDataModel(null, 2, 10, 25, 10, 0).GetQueryPaging(q).ToList().First(), 11);
        DemoConsole.Case("page=3, size=10 → 21..25", () =>
            new PaginatedDataModel(null, 3, 10, 25, 10, 0).GetQueryPaging(q).Count(), 5);
        DemoConsole.Case("page=10 (за пределами) → пусто", () =>
            new PaginatedDataModel(null, 10, 10, 25, 10, 0).GetQueryPaging(q).Count(), 0);
    }
}