using _test_common.Framework;
using Ans.Net10.Common;
using Microsoft.EntityFrameworkCore;

namespace _test_common.Demos;

/// <summary>Демо _ListPaginatedModel_Base</summary>
public static class _ListPaginatedModelDemo
{
    // Доменная сущность
    public sealed class UserEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    // UI-модель
    public sealed class UserModel
    {
        public string Name { get; }
        public UserModel(string name) => Name = name;
        public override string ToString() => $"UserModel({Name})";
    }

    // EF-контекст
    private sealed class TestDbContext : DbContext
    {
        public DbSet<UserEntity> Users => Set<UserEntity>();
        public TestDbContext(DbContextOptions<TestDbContext> o) : base(o) { }
    }

    // Наследник базового класса — по образцу из doc-comment
    private sealed class UserCatalogViewModel : _ListPaginatedModel_Base<UserEntity, UserModel>
    {
        private UserCatalogViewModel(
            PaginationModel pagination,
            IReadOnlyCollection<UserModel> items)
            : base(pagination, items) { }

        public static async Task<UserCatalogViewModel> CreateAsync(
            IQueryable<UserEntity> query, int page, int size, CancellationToken ct = default)
        {
            var data = await LoadDataAsync(query, x => new UserModel(x.Name), page, size, ct);
            return new UserCatalogViewModel(data.Pagination, data.Items);
        }
    }

    public static async Task Run()
    {
        DemoConsole.Part("_ListPaginatedModel_Base<TEntity, TModel>");

        var opts = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase($"pagedemo_{Guid.NewGuid()}")
            .Options;

        await using var db = new TestDbContext(opts);
        for (int i = 1; i <= 25; i++)
            db.Users.Add(new UserEntity { Id = i, Name = $"User{i:D2}" });
        await db.SaveChangesAsync();

        DemoConsole.Note("Сценарии пагинации по 25 записям");

        await DemoConsole.CaseAsync("page=1, size=10", async () =>
        {
            var vm = await UserCatalogViewModel.CreateAsync(db.Users, 1, 10);
            return $"Items={vm.ItemsCount}, HasItems={vm.HasItems}, first={vm.Items.First().Name}";
        });

        await DemoConsole.CaseAsync("page=2, size=10", async () =>
        {
            var vm = await UserCatalogViewModel.CreateAsync(db.Users, 2, 10);
            return $"Items={vm.ItemsCount}, first={vm.Items.First().Name}";
        });

        await DemoConsole.CaseAsync("page=3, size=10 (неполная)", async () =>
        {
            var vm = await UserCatalogViewModel.CreateAsync(db.Users, 3, 10);
            return $"Items={vm.ItemsCount}, first={vm.Items.First().Name}";
        });

        await DemoConsole.CaseAsync("page=100 (вне диапазона)", async () =>
        {
            var vm = await UserCatalogViewModel.CreateAsync(db.Users, 100, 10);
            return $"Items={vm.ItemsCount}, HasItems={vm.HasItems}";
        });

        await DemoConsole.CaseAsync("size=0", async () =>
        {
            var vm = await UserCatalogViewModel.CreateAsync(db.Users, 1, 0);
            return $"Items={vm.ItemsCount}, HasItems={vm.HasItems}";
        });

        await DemoConsole.CaseAsync("size=1000 (больше данных)", async () =>
        {
            var vm = await UserCatalogViewModel.CreateAsync(db.Users, 1, 1000);
            return $"Items={vm.ItemsCount}";
        });

        await DemoConsole.CaseAsync("page=0", async () =>
        {
            var vm = await UserCatalogViewModel.CreateAsync(db.Users, 0, 10);
            return $"Items={vm.ItemsCount}";
        });

        DemoConsole.Note("Пустой результат и защита от null");

        await DemoConsole.CaseAsync("пустой запрос", async () =>
        {
            var vm = await UserCatalogViewModel.CreateAsync(
                db.Users.Where(x => x.Id > 1000), 1, 10);
            return $"Items={vm.ItemsCount}, HasItems={vm.HasItems}";
        });

        await DemoConsole.CaseThrowsAsync<ArgumentNullException>("null query → ArgumentNullException",
            () => UserCatalogViewModel.CreateAsync(null!, 1, 10));

        await DemoConsole.CaseThrowsAsync<ArgumentNullException>("null func → ArgumentNullException",
            () => _ListPaginatedModel_Base<UserEntity, UserModel>.LoadDataAsync(db.Users, null!, 1, 10));
    }
}