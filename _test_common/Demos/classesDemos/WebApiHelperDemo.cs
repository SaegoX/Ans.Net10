using System.Buffers;
using System.Net;
using System.Text;
using System.Text.Json;
using _test_common.Framework;
using Ans.Net10.Common;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace _test_common.Demos;

public static class WebApiHelperDemo
{
    private sealed class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _factory;
        public int CallCount { get; private set; }
        public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> factory) => _factory = factory;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            CallCount++;
            try { return Task.FromResult(_factory(request)); }
            catch (Exception e) { return Task.FromException<HttpResponseMessage>(e); }
        }
    }

    private static HttpClient MakeClient(HttpMessageHandler handler) => new(handler);

    private static HttpResponseMessage Json<T>(HttpStatusCode code, T obj)
        => new(code) { Content = new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json") };

    private static HttpResponseMessage RawJson(HttpStatusCode code, string json)
        => new(code) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static HybridCache DefaultCache()
    {
        var sc = new ServiceCollection();
        sc.AddHybridCache();
        return sc.BuildServiceProvider().GetRequiredService<HybridCache>();
    }

    /// <summary>Обход бага: сериализатор, который игнорирует Headers.</summary>
    private sealed class WebApiResultSerializer : IHybridCacheSerializer<WebApiResult<UserDto>>
    {
        public void Serialize(WebApiResult<UserDto> value, IBufferWriter<byte> target)
        {
            using var w = new Utf8JsonWriter(target);
            w.WriteStartObject();
            w.WriteNumber("StatusCode", (int)value.StatusCode);
            if (value.Content != null)
            {
                w.WritePropertyName("Content");
                JsonSerializer.Serialize(w, value.Content);
            }
            if (value.ErrorBody != null) w.WriteString("ErrorBody", value.ErrorBody);
            w.WriteBoolean("IsDeserializationError", value.IsDeserializationError);
            w.WriteEndObject();
        }

        public WebApiResult<UserDto> Deserialize(ReadOnlySequence<byte> source)
        {
            var r = new WebApiResult<UserDto>();
            foreach (var seg in source)
            {
                using var doc = JsonDocument.Parse(seg);
                var root = doc.RootElement;
                if (root.TryGetProperty("StatusCode", out var sc)) r.StatusCode = (HttpStatusCode)sc.GetInt32();
                if (root.TryGetProperty("Content", out var ct)) r.Content = ct.Deserialize<UserDto>();
                if (root.TryGetProperty("ErrorBody", out var eb)) r.ErrorBody = eb.GetString();
                if (root.TryGetProperty("IsDeserializationError", out var ide)) r.IsDeserializationError = ide.GetBoolean();
                break; // последовательности обычно один сегмент
            }
            return r;
        }
    }

    private static HybridCache CacheWithWorkaround()
    {
        var sc = new ServiceCollection();
        sc.AddHybridCache()
          .AddSerializer<WebApiResult<UserDto>, WebApiResultSerializer>();
        return sc.BuildServiceProvider().GetRequiredService<HybridCache>();
    }

    public static async Task Run()
    {
        DemoConsole.Part("WebApiHelper<T>");

        DemoConsole.Note("ВАЖНО: WebApiHelper.SendGetAsync в текущей реализации библиотеки " +
                         "бросает NotSupportedException — HybridCache не может сериализовать " +
                         "HttpResponseHeaders" +
                         "POST/PUT/DELETE работают, потому что не кэшируются.");

        CtorGuards();
        await GetThrows_NotSupported();
        await Post_Works();
        await Put_Works();
        await Delete_Works();
        await Guards_PostPutDelete();
        await Invalidate_Works();
        await Workaround_WithCustomSerializer();
    }

    private static void CtorGuards()
    {
        DemoConsole.Note("ctor guards");
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var client = MakeClient(handler);
        var cache = DefaultCache();

        DemoConsole.CaseThrows<ArgumentNullException>("httpClient=null",
            () => _ = new WebApiHelper<UserDto>(null!, "https://t/", cache));
        DemoConsole.CaseThrows<ArgumentNullException>("baseUrl=null",
            () => _ = new WebApiHelper<UserDto>(client, null!, cache));
        DemoConsole.CaseThrows<ArgumentNullException>("cache=null",
            () => _ = new WebApiHelper<UserDto>(client, "https://t/", null!));

        var h = new WebApiHelper<UserDto>(client, "https://t/", cache);
        DemoConsole.Case("BaseUrl", () => h.BaseUrl, "https://t/");
        DemoConsole.Case("JsonOptions not null", () => h.JsonOptions is not null, true);
        DemoConsole.Case("JsonTypeInfo is null", () => h.JsonTypeInfo is null, true);
        DemoConsole.Case("CacheOptions is null", () => h.CacheOptions is null, true);
        DemoConsole.Case("Params not null", () => h.Params is not null, true);
    }

    private static async Task GetThrows_NotSupported()
    {
        DemoConsole.Note("БАГ: GET с успешным ответом → HybridCache пытается сериализовать " +
                         "WebApiResult (с HttpResponseHeaders) → NotSupportedException. " +
                         "HTTP-запрос при этом УСПЕВАЕТ уйти (handler вызван).");

        var handler = new FakeHandler(_ => Json(HttpStatusCode.OK, new UserDto { Id = 1, Name = "v" }));
        var h = new WebApiHelper<UserDto>(MakeClient(handler), "https://t/", DefaultCache());

        await DemoConsole.CaseThrowsAsync<NotSupportedException>(
            "SendGetAsync → NotSupportedException",
            () => h.SendGetAsync("?id=1"));

        DemoConsole.Case("handler был вызван 1 раз (HTTP ушёл до падения)", () => handler.CallCount, 1);

        DemoConsole.Note("Ошибка воспроизводится на ЛЮБОМ успешном GET, " +
                         "независимо от T — Headers не сериализуется.");
    }

    private static async Task Post_Works()
    {
        DemoConsole.Note("POST — не кэшируется, работает");
        var handler = new FakeHandler(_ =>
            Json(HttpStatusCode.OK, new UserDto { Id = 42, Name = "created" }));
        var h = new WebApiHelper<UserDto>(MakeClient(handler), "https://t/", DefaultCache());
        var payload = new UserDto { Name = "new" };

        var r = await h.SendPostAsync("?create=1", payload);
        DemoConsole.Case("StatusCode == 200", () => (int)r.StatusCode, 200);
        DemoConsole.Case("Content.Id == 42", () => r.Content?.Id, 42);
        DemoConsole.Case("Content.Name == created", () => r.Content?.Name, "created");
        DemoConsole.Case("handler вызван 1 раз", () => handler.CallCount, 1);

        await h.SendPostAsync("?create=1", payload);
        DemoConsole.Case("2-й POST — снова HTTP (нет кэша)", () => handler.CallCount, 2);
    }

    private static async Task Put_Works()
    {
        DemoConsole.Note("PUT — не кэшируется, работает");
        var handler = new FakeHandler(_ =>
            Json(HttpStatusCode.OK, new UserDto { Id = 7, Name = "updated" }));
        var h = new WebApiHelper<UserDto>(MakeClient(handler), "https://t/", DefaultCache());
        var r = await h.SendPutAsync("?u=1", new UserDto { Id = 7 });
        DemoConsole.Case("Content.Name == updated", () => r.Content?.Name, "updated");
    }

    private static async Task Delete_Works()
    {
        DemoConsole.Note("DELETE — не кэшируется, работает");
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var h = new WebApiHelper<UserDto>(MakeClient(handler), "https://t/", DefaultCache());
        var r = await h.SendDeleteAsync("?id=5");
        DemoConsole.Case("StatusCode == 204", () => (int)r.StatusCode, 204);
        DemoConsole.Case("Content is null", () => r.Content is null, true);
    }

    private static async Task Guards_PostPutDelete()
    {
        DemoConsole.Note("guards POST/PUT/DELETE");
        var h = new WebApiHelper<UserDto>(
            MakeClient(new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK))),
            "https://t/", DefaultCache());

        await DemoConsole.CaseThrowsAsync<ArgumentNullException>("POST(null qs)",
            () => h.SendPostAsync(null!, new UserDto()));
        await DemoConsole.CaseThrowsAsync<ArgumentNullException>("POST(null payload)",
            () => h.SendPostAsync("?x=1", null!));
        await DemoConsole.CaseThrowsAsync<ArgumentNullException>("PUT(null qs)",
            () => h.SendPutAsync(null!, new UserDto()));
        await DemoConsole.CaseThrowsAsync<ArgumentNullException>("PUT(null payload)",
            () => h.SendPutAsync("?x=1", null!));
        await DemoConsole.CaseThrowsAsync<ArgumentNullException>("DELETE(null qs)",
            () => h.SendDeleteAsync(null!));
    }

    private static async Task Invalidate_Works()
    {
        DemoConsole.Note("InvalidateHelperCacheAsync — Remove, без сериализации, работает");
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var h = new WebApiHelper<UserDto>(MakeClient(handler), "https://t/", DefaultCache());

        await h.InvalidateHelperCacheAsync("?x=1");
        DemoConsole.Case("не бросает", () => true, true);

        DemoConsole.CaseThrows<ArgumentNullException>("Invalidate(null) → ANE", () =>
            h.InvalidateHelperCacheAsync(null!).AsTask().GetAwaiter().GetResult());

        await h.InvalidateHelperCacheAsync();
        DemoConsole.Case("без параметров (использует Params)", () => true, true);
    }

    private static async Task Workaround_WithCustomSerializer()
    {
        DemoConsole.Note("ОБХОД БАГА: кастомный IHybridCacheSerializer<WebApiResult<UserDto>>, " +
                         "который не сериализует Headers. С таким кэшем GET работает.");

        var handler = new FakeHandler(_ =>
            Json(HttpStatusCode.OK, new UserDto { Id = 1, Name = "v" }));
        var h = new WebApiHelper<UserDto>(
            MakeClient(handler), "https://t/", CacheWithWorkaround());

        var r = await h.SendGetAsync("?id=1");
        DemoConsole.Case("StatusCode == 200", () => (int)r.StatusCode, 200);
        DemoConsole.Case("Content.Name == v", () => r.Content?.Name, "v");
        DemoConsole.Case("handler вызван 1 раз", () => handler.CallCount, 1);
        DemoConsole.Case("Headers == null (потеряны)", () => r.Headers is null, true);

        DemoConsole.Note("2-й GET → cache hit (handler НЕ вызывается)");
        await h.SendGetAsync("?id=1");
        DemoConsole.Case("handler всё ещё 1", () => handler.CallCount, 1);
    }
}