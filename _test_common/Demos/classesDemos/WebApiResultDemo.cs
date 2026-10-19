using _test_common.Framework;
using Ans.Net10.Common;
using System.Net;

namespace _test_common.Demos;

public static class WebApiResultDemo
{
    public static Task Run()
    {
        DemoConsole.Part("WebApiResult<T> : _WebResult_Base<T>");
        Inheritance();
        Defaults();
        InheritedSetGet();
        return Task.CompletedTask;
    }

    private static void Inheritance()
    {
        DemoConsole.Note("WebApiResult<T> — пустой наследник, вся логика в base");
        DemoConsole.Case("is _WebResult_Base<string>", () => new WebApiResult<string>() is _WebResult_Base<string>, true);
        DemoConsole.Case("is _WebResult_Base<int[]>", () => new WebApiResult<int[]>() is _WebResult_Base<int[]>, true);
    }

    private static void Defaults()
    {
        DemoConsole.Note("Наследует все дефолты base");
        var r = new WebApiResult<string>();
        DemoConsole.Case("Content is null", () => r.Content is null, true);
        DemoConsole.Case("StatusCode == 0", () => (int)r.StatusCode, 0);
        DemoConsole.Case("Headers is null", () => r.Headers is null, true);
        DemoConsole.Case("IsDeserializationError false", () => r.IsDeserializationError, false);
        DemoConsole.Case("DeserializationException null", () => r.DeserializationException is null, true);
        DemoConsole.Case("ErrorBody is null", () => r.ErrorBody is null, true);
    }

    private static void InheritedSetGet()
    {
        DemoConsole.Note("Присвоение через базовые свойства");
        var r = new WebApiResult<string>
        {
            Content = "ok",
            StatusCode = HttpStatusCode.Created,
            ErrorBody = "err"
        };
        DemoConsole.Case("Content", () => r.Content, "ok");
        DemoConsole.Case("StatusCode == 201", () => (int)r.StatusCode, 201);
        DemoConsole.Case("ErrorBody", () => r.ErrorBody, "err");
    }
}