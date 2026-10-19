using _test_common.Framework;
using Ans.Net10.Common;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace _test_common.Demos;

public static class HybridCacheHelperDemo
{
    public static async Task Run()
    {
        DemoConsole.Part("HybridCacheHelper");

        var services = new ServiceCollection();
        services.AddHybridCache();
        using var sp = services.BuildServiceProvider();
        var cache = sp.GetRequiredService<HybridCache>();

        CtorChecks(cache);
        await GetAsyncChecks(cache);
        await CacheHitChecks(cache);
        await RemoveChecks(cache);
    }

    private static void CtorChecks(HybridCache cache)
    {
        DemoConsole.Note("ctor — guard и сохранение options");
        DemoConsole.CaseThrows<ArgumentNullException>("ctor(null)", () => _ = new HybridCacheHelper(null!));

        var h1 = new HybridCacheHelper(cache);
        DemoConsole.Case("DefaultCacheOptions is null", () => h1.DefaultCacheOptions is null, true);

        var opts = new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(1) };
        var h2 = new HybridCacheHelper(cache, opts);
        DemoConsole.Case("DefaultCacheOptions == opts", () => ReferenceEquals(h2.DefaultCacheOptions, opts), true);
    }

    private static async Task GetAsyncChecks(HybridCache cache)
    {
        DemoConsole.Note("GetAsync — guard параметров");
        var h = new HybridCacheHelper(cache);
        await DemoConsole.CaseThrowsAsync<ArgumentNullException>("GetAsync(null key)",
            () => h.GetAsync<string>(null!, _ => ValueTask.FromResult("x")).AsTask());
        await DemoConsole.CaseThrowsAsync<ArgumentNullException>("GetAsync(null factory)",
            () => h.GetAsync<string>("k", null!).AsTask());

        DemoConsole.Note("GetAsync — нормальный промах + попадание");
        int calls = 0;
        Func<CancellationToken, ValueTask<string>> factory = _ =>
        {
            calls++;
            return ValueTask.FromResult("v");
        };
        var first = await h.GetAsync("get-key-1", factory);
        DemoConsole.Case("первый вызов вернул \"v\"", () => first, "v");
        DemoConsole.Case("фабрика вызвана 1 раз", () => calls, 1);

        DemoConsole.Note("Пустой ключ — HybridCache обычно разрешает");
        var emptyKeyResult = await h.GetAsync("", _ => ValueTask.FromResult("empty"));
        DemoConsole.Case("key=\"\" → \"empty\"", () => emptyKeyResult, "empty");
    }

    private static async Task CacheHitChecks(HybridCache cache)
    {
        DemoConsole.Note("Cache hit — фабрика не должна вызываться повторно");
        var h = new HybridCacheHelper(cache);
        int calls = 0;
        Func<CancellationToken, ValueTask<string>> factory = _ =>
        {
            calls++;
            return ValueTask.FromResult("v");
        };
        await h.GetAsync("hit-key-1", factory);
        await h.GetAsync("hit-key-1", factory);
        await h.GetAsync("hit-key-1", factory);
        DemoConsole.Case("3 запроса → фабрика вызвана 1 раз", () => calls, 1);
    }

    private static async Task RemoveChecks(HybridCache cache)
    {
        DemoConsole.Note("RemoveAsync — инвалидация");
        var h = new HybridCacheHelper(cache);
        int calls = 0;
        Func<CancellationToken, ValueTask<string>> factory = _ =>
        {
            calls++;
            return ValueTask.FromResult("v");
        };
        await h.GetAsync("rem-key-1", factory);
        await h.RemoveAsync("rem-key-1");
        await h.GetAsync("rem-key-1", factory);
        DemoConsole.Case("после Remove фабрика вызвана 2 раза", () => calls, 2);

        await DemoConsole.CaseThrowsAsync<ArgumentNullException>("RemoveAsync(null)",
            () => h.RemoveAsync(null!).AsTask());
    }
}