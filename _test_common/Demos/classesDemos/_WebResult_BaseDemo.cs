using _test_common.Framework;
using Ans.Net10.Common;
using System.Net;

namespace _test_common.Demos;

public static class _WebResult_BaseDemo
{
    public static Task Run()
    {
        DemoConsole.Part("_WebResult_Base<T>");
        Defaults();
        SetContent();
        StatusCodes();
        DeserializationError();
        Headers();
        ErrorBody();
        return Task.CompletedTask;
    }

    private static void Defaults()
    {
        DemoConsole.Note("Значения по умолчанию нового экземпляра");
        var r = new _WebResult_Base<string>();
        DemoConsole.Case("Content is null", () => r.Content is null, true);
        DemoConsole.Case("StatusCode == 0", () => (int)r.StatusCode, 0);
        DemoConsole.Case("Headers is null", () => r.Headers is null, true);
        DemoConsole.Case("IsDeserializationError false", () => r.IsDeserializationError, false);
        DemoConsole.Case("DeserializationException null", () => r.DeserializationException is null, true);
        DemoConsole.Case("ErrorBody is null", () => r.ErrorBody is null, true);
    }

    private static void SetContent()
    {
        DemoConsole.Note("Установка Content");
        DemoConsole.Case("Content = \"hello\"", () => new _WebResult_Base<string> { Content = "hello" }.Content, "hello");
        DemoConsole.Case("Content = \"\"", () => new _WebResult_Base<string> { Content = "" }.Content, "");
        DemoConsole.Case("Content int[] length", () => new _WebResult_Base<int[]> { Content = [1, 2, 3] }.Content!.Length, 3);
    }

    private static void StatusCodes()
    {
        DemoConsole.Note("StatusCode (HttpStatusCode)");
        var r = new _WebResult_Base<string>();
        r.StatusCode = HttpStatusCode.OK; DemoConsole.Case("OK (200)", () => (int)r.StatusCode, 200);
        r.StatusCode = HttpStatusCode.NotFound; DemoConsole.Case("NotFound (404)", () => (int)r.StatusCode, 404);
        r.StatusCode = HttpStatusCode.InternalServerError; DemoConsole.Case("InternalServerError (500)", () => (int)r.StatusCode, 500);
    }

    private static void DeserializationError()
    {
        DemoConsole.Note("IsDeserializationError + DeserializationException");
        var r = new _WebResult_Base<string> { IsDeserializationError = true };
        DemoConsole.Case("IsDeserializationError == true", () => r.IsDeserializationError, true);
        r.DeserializationException = new FormatException("bad json");
        DemoConsole.Case("DeserializationException задан", () => r.DeserializationException is FormatException, true);
    }

    private static void Headers()
    {
        DemoConsole.Note("Headers — HttpResponseHeaders, реальный объект через HttpResponseMessage");
        var r = new _WebResult_Base<string>();
        DemoConsole.Case("Headers is null по умолчанию", () => r.Headers is null, true);

        using var msg = new HttpResponseMessage(HttpStatusCode.OK);
        msg.Headers.Add("X-Test", "value");
        r.Headers = msg.Headers;
        DemoConsole.Case("Headers содержит X-Test", () => r.Headers!.Contains("X-Test"), true);
    }

    private static void ErrorBody()
    {
        DemoConsole.Note("ErrorBody — тело ошибки от сервера");
        DemoConsole.Case("ErrorBody содержит '404'", () =>
            new _WebResult_Base<string> { ErrorBody = "<html>404 Not Found</html>" }.ErrorBody!.Contains("404"), true);
    }
}