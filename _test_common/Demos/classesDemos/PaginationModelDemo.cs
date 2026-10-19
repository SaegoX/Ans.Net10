using _test_common.Framework;
using Ans.Net10.Common;
using Microsoft.EntityFrameworkCore;

namespace _test_common.Demos;

public static class PaginationModelDemo
{
    private sealed class Item { public int Id { get; set; } }
    private sealed class Db : DbContext
    {
        public DbSet<Item> Items => Set<Item>();
        public Db(DbContextOptions<Db> o) : base(o) { }
    }

    public static async Task Run()
    {
        DemoConsole.Part("PaginationModel (readonly record struct)");

        DemoConsole.Note("Конструирование напрямую (позиционный ctor)");

        var m1 = new PaginationModel(
            CurrentPage: 3, TotalItems: 25, TotalPages: 3, SkipItems: 20, ItemsOnPage: 10,
            StartPage: 1, PreviousPage: 1, NextPage: 3, EndPage: 3,
            ActiveFirstPage: false, ActiveLastPage: true,
            HasItemsBefore: true, HasItemsAfter: false,
            NotValidIndex: false, Offset: 2);

        DemoConsole.Case("ToString()", () => m1.ToString().Contains("CurrentPage"), true);
        DemoConsole.Case("CurrentPage", () => m1.CurrentPage, 3);
        DemoConsole.Case("TotalPages", () => m1.TotalPages, 3);

        DemoConsole.Note("Value-семантика record struct");

        var m2 = m1 with { CurrentPage = 3 };
        DemoConsole.Case("Equals с одинаковым содержимым", () => m1.Equals(m2), true);

        var m3 = m1 with { CurrentPage = 99 };
        DemoConsole.Case("Equals с отличающимся полем", () => m1.Equals(m3), false);
        DemoConsole.Case("GetHashCode совпадает для равных", () => m1.GetHashCode() == m2.GetHashCode(), true);

        DemoConsole.Note("Конструирование из PaginationHelper (через хелпер)");

        var opts = new DbContextOptionsBuilder<Db>()
            .UseInMemoryDatabase($"pm_{Guid.NewGuid()}")
            .Options;
        await using var db = new Db(opts);
        for (int i = 1; i <= 25; i++) db.Items.Add(new Item { Id = i });
        await db.SaveChangesAsync();

        await DemoConsole.CaseAsync("ctor(PaginationHelper) — копирует все поля", async () =>
        {
            var h = await PaginatedQueryableHelper<Item>.CreateAsync(db.Items, 2, 10);
            var src = h.PaginationHelper;
            var model = new PaginationModel(src);

            return model.CurrentPage == src.CurrentPage
                && model.TotalItems == src.TotalItems
                && model.TotalPages == src.TotalPages
                && model.Offset == src.Offset;
        }, true);

        DemoConsole.Note("Передача как DTO: value-тип, можно спокойно хранить и сравнивать");
    }
}