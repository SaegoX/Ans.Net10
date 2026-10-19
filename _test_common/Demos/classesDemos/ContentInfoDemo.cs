using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class ContentInfoDemo
{
    public static Task Run()
    {
        DemoConsole.Part("ContentInfo / ContentGroupEnum");
        EnumValues();
        Ctor();
        ImageFlags();
        MediaTypeValid();
        MediaTypeInvalid();
        ToStringCheck();
        return Task.CompletedTask;
    }

    private static void EnumValues()
    {
        DemoConsole.Note("ContentGroupEnum — все значения");
        var names = Enum.GetNames<ContentGroupEnum>();
        DemoConsole.Case("количество значений = 9", () => names.Length, 9);
        DemoConsole.Case("содержит Archive", () => Enum.IsDefined(ContentGroupEnum.Archive), true);
        DemoConsole.Case("содержит Image", () => Enum.IsDefined(ContentGroupEnum.Image), true);
    }

    private static void Ctor()
    {
        DemoConsole.Note("Конструктор — базовые сценарии");
        var i = new ContentInfo(".jpg", "image/jpeg", ContentGroupEnum.Image);
        DemoConsole.Case("Extension", () => i.Extension, ".jpg");
        DemoConsole.Case("ContentType", () => i.ContentType, "image/jpeg");
        DemoConsole.Case("Group", () => i.Group, ContentGroupEnum.Image);

        DemoConsole.Note("Дефолтные флаги");
        DemoConsole.Case("IsWebImage по умолчанию false", () => i.IsWebImage, false);
        DemoConsole.Case("IsJpeg по умолчанию false", () => i.IsJpeg, false);
    }

    private static void ImageFlags()
    {
        DemoConsole.Note("IsImage — вычисляется из Group == Image");
        DemoConsole.Case("Image → IsImage=true", () => new ContentInfo(".png", "image/png", ContentGroupEnum.Image).IsImage, true);
        DemoConsole.Case("Audio → IsImage=false", () => new ContentInfo(".mp3", "audio/mp3", ContentGroupEnum.Audio).IsImage, false);
        DemoConsole.Case("Text  → IsImage=false", () => new ContentInfo(".txt", "text/plain", ContentGroupEnum.Text).IsImage, false);

        DemoConsole.Note("IsWebImage/IsJpeg — явные флаги");
        DemoConsole.Case("Web + Jpeg", () =>
            new ContentInfo(".jpg", "image/jpeg", ContentGroupEnum.Image, isWebImage: true, isJpeg: true).IsJpeg, true);
        DemoConsole.Case("Web без Jpeg (png)", () =>
            new ContentInfo(".png", "image/png", ContentGroupEnum.Image, isWebImage: true).IsJpeg, false);
    }

    private static void MediaTypeValid()
    {
        DemoConsole.Note("MediaType — валидные ContentType");
        DemoConsole.Case("image/jpeg → MediaType", () => new ContentInfo(".jpg", "image/jpeg", ContentGroupEnum.Image).MediaType.MediaType, "image/jpeg");
        DemoConsole.Case("text/plain → MediaType", () => new ContentInfo(".txt", "text/plain", ContentGroupEnum.Text).MediaType.MediaType, "text/plain");
        DemoConsole.Case("application/json → MediaType", () => new ContentInfo(".json", "application/json", ContentGroupEnum.Text).MediaType.MediaType, "application/json");

        DemoConsole.Note("С параметрами (charset и т.п.)");
        DemoConsole.Case("text/plain; charset=utf-8", () =>
            new ContentInfo(".txt", "text/plain; charset=utf-8", ContentGroupEnum.Text).MediaType.MediaType, "text/plain");
    }

    private static void MediaTypeInvalid()
    {
        DemoConsole.Note("MediaType — НЕвалидный ContentType → fallback application/octet-stream");
        DemoConsole.Case("пустая строка", () => new ContentInfo(".x", "", ContentGroupEnum.Bin).MediaType.MediaType, "application/octet-stream");
        DemoConsole.Case("\"garbage\"", () => new ContentInfo(".x", "garbage", ContentGroupEnum.Bin).MediaType.MediaType, "application/octet-stream");
        DemoConsole.Case("\"not a mime type\"", () => new ContentInfo(".x", "not a mime type", ContentGroupEnum.Bin).MediaType.MediaType, "application/octet-stream");

        DemoConsole.Note("Lazy: одно и то же значение при повторных обращениях (ссылка)");
        var c = new ContentInfo(".bin", "application/octet-stream", ContentGroupEnum.Bin);
        DemoConsole.Case("ReferenceEquals(MediaType, MediaType)", () => ReferenceEquals(c.MediaType, c.MediaType), true);
    }

    private static void ToStringCheck()
    {
        DemoConsole.Note("ToString() — формат с группой, MIME и флагами");
        var c = new ContentInfo(".jpg", "image/jpeg", ContentGroupEnum.Image, isWebImage: true, isJpeg: true);
        DemoConsole.Case("ToString().Contains(Group)", () => c.ToString().Contains("Image"), true);
        DemoConsole.Case("ToString().Contains(MIME)", () => c.ToString().Contains("image/jpeg"), true);
        DemoConsole.Case("ToString().Contains(IsJpeg:True)", () => c.ToString().Contains("IsJpeg:True"), true);
    }
}