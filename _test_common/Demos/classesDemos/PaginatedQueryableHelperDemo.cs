using _test_common.Framework;
using Ans.Net10.Common;
using Microsoft.EntityFrameworkCore;

namespace _test_common.Demos;

public static class PaginatedQueryableHelperDemo
{
    public sealed class Item { public int Id { get; set; } public string Name { get; set; } = ""; }

    private sealed class Db : DbContext
    {
        public DbSet<Item> Items => Set<Item>();
        public Db(DbContextOptions<Db> o) : base(o) { }
    }

    public static async Task Run()
    {
        DemoConsole.Part("PaginatedQueryableHelper<TEntity>");

        var opts = new DbContextOptionsBuilder<Db>()
            .UseInMemoryDatabase($"pq_{Guid.NewGuid()}")
            .Options;
        await using var db = new Db(opts);
        for (int i = 1; i <= 25; i++)
            db.Items.Add(new Item { Id = i, Name = $"Item{i:D2}" });
        await db.SaveChangesAsync();

        DemoConsole.Note("25 записей в БД. Тестируем page/size + границы");

        await DemoConsole.CaseAsync("page=1, size=10", async () =>
        {
            var h = await PaginatedQueryableHelper<Item>.CreateAsync(db.Items, 1, 10);
            var p = h.PaginationHelper;
            var real = await h.Query.CountAsync();
            return $"Cur={p.CurrentPage} Tot={p.TotalItems} Pages={p.TotalPages} Skip={p.SkipItems} real={real}";
        });

        await DemoConsole.CaseAsync("page=2, size=10", async () =>
        {
            var h = await PaginatedQueryableHelper<Item>.CreateAsync(db.Items, 2, 10);
            var p = h.PaginationHelper;
            var real = await h.Query.CountAsync();
            return $"Cur={p.CurrentPage} Skip={p.SkipItems} real={real}";
        });

        await DemoConsole.CaseAsync("page=3, size=10 (неполная страница)", async () =>
        {
            var h = await PaginatedQueryableHelper<Item>.CreateAsync(db.Items, 3, 10);
            var p = h.PaginationHelper;
            var real = await h.Query.CountAsync();
            return $"Cur={p.CurrentPage} real={real} ActiveLast={p.ActiveLastPage}";
        });

        await DemoConsole.CaseAsync("page=100 (за диапазоном)", async () =>
        {
            var h = await PaginatedQueryableHelper<Item>.CreateAsync(db.Items, 100, 10);
            var p = h.PaginationHelper;
            var real = await h.Query.CountAsync();
            return $"Cur={p.CurrentPage} NotValid={p.NotValidIndex} real={real}";
        });

        await DemoConsole.CaseAsync("page=0 (корректируется в 1)", async () =>
        {
            var h = await PaginatedQueryableHelper<Item>.CreateAsync(db.Items, 0, 10);
            return $"Cur={h.PaginationHelper.CurrentPage}";
        });

        await DemoConsole.CaseAsync("page=-5 (корректируется в 1)", async () =>
        {
            var h = await PaginatedQueryableHelper<Item>.CreateAsync(db.Items, -5, 10);
            return $"Cur={h.PaginationHelper.CurrentPage}";
        });

        await DemoConsole.CaseAsync("size=0 (корректируется в 1)", async () =>
        {
            var h = await PaginatedQueryableHelper<Item>.CreateAsync(db.Items, 1, 0);
            var p = h.PaginationHelper;
            var real = await h.Query.CountAsync();
            return $"ItemsOnPage={p.ItemsOnPage} real={real}";
        });

        await DemoConsole.CaseAsync("size=-5 (корректируется в 1)", async () =>
        {
            var h = await PaginatedQueryableHelper<Item>.CreateAsync(db.Items, 1, -5);
            return $"ItemsOnPage={h.PaginationHelper.ItemsOnPage}";
        });

        await DemoConsole.CaseAsync("size=1000 (больше данных)", async () =>
        {
            var h = await PaginatedQueryableHelper<Item>.CreateAsync(db.Items, 1, 1000);
            var real = await h.Query.CountAsync();
            return $"real={real}";
        });

        await DemoConsole.CaseThrowsAsync<ArgumentNullException>("null query",
            () => PaginatedQueryableHelper<Item>.CreateAsync(null!, 1, 10));

        DemoConsole.Note("Проверка навигационных полей PaginationModel");

        await DemoConsole.CaseAsync("Offset / StartPage / EndPage", async () =>
        {
            var h = await PaginatedQueryableHelper<Item>.CreateAsync(db.Items, 5, 10);
            var p = h.PaginationHelper;
            return $"Offset={p.Offset} Start={p.StartPage} End={p.EndPage} Prev={p.PreviousPage} Next={p.NextPage}";
        });

        await DemoConsole.CaseAsync("HasItemsBefore / HasItemsAfter на середине", async () =>
        {
            var h = await PaginatedQueryableHelper<Item>.CreateAsync(db.Items, 5, 10);
            var p = h.PaginationHelper;
            return $"Before={p.HasItemsBefore} After={p.HasItemsAfter}";
        });

        await DemoConsole.CaseAsync("HasItemsBefore/After на краях", async () =>
        {
            var first = (await PaginatedQueryableHelper<Item>.CreateAsync(db.Items, 1, 10)).PaginationHelper;
            var last = (await PaginatedQueryableHelper<Item>.CreateAsync(db.Items, 3, 10)).PaginationHelper;
            return $"first.Before={first.HasItemsBefore} first.After={first.HasItemsAfter} | last.Before={last.HasItemsBefore} last.After={last.HasItemsAfter}";
        });
    }
}