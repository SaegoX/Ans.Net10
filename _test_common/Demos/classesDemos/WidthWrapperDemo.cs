using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class WidthWrapperDemo
{
    public static Task Run()
    {
        DemoConsole.Part("WidthsEnum / WidthWrapper");
        EnumValues();
        AutoStylePerEnum();
        NullHandling();
        ToStringCheck();
        return Task.CompletedTask;
    }

    private static void EnumValues()
    {
        DemoConsole.Note("WidthsEnum — все значения");
        DemoConsole.Case("кол-во = 7", () => Enum.GetValues<WidthsEnum>().Length, 7);
        DemoConsole.Case("Full = 0", () => (int)WidthsEnum.Full, 0);
        DemoConsole.Case("Nothing = 6", () => (int)WidthsEnum.Nothing, 6);
    }

    private static void AutoStylePerEnum()
    {
        DemoConsole.Note("AutoStyle — все варианты ширины");
        DemoConsole.Case("Full → width:100%", () => new WidthWrapper("x", WidthsEnum.Full, "").AutoStyle, "width:100%");
        DemoConsole.Case("ExtraLarge → width:40rem", () => new WidthWrapper("x", WidthsEnum.ExtraLarge, "").AutoStyle, "width:40rem");
        DemoConsole.Case("Large → width:30rem", () => new WidthWrapper("x", WidthsEnum.Large, "").AutoStyle, "width:30rem");
        DemoConsole.Case("Medium → width:20rem", () => new WidthWrapper("x", WidthsEnum.Medium, "").AutoStyle, "width:20rem");
        DemoConsole.Case("Small → width:15rem", () => new WidthWrapper("x", WidthsEnum.Small, "").AutoStyle, "width:15rem");
        DemoConsole.Case("ExtraSmall → width:10rem", () => new WidthWrapper("x", WidthsEnum.ExtraSmall, "").AutoStyle, "width:10rem");
        DemoConsole.Case("Nothing → null", () => new WidthWrapper("x", WidthsEnum.Nothing, "").AutoStyle, (string?)null);

        DemoConsole.Note("Невалидное значение enum → fallback width:6rem");
        DemoConsole.Case("(WidthsEnum)999 → width:6rem", () =>
            new WidthWrapper("x", (WidthsEnum)999, "").AutoStyle, "width:6rem");
    }

    private static void NullHandling()
    {
        DemoConsole.Note("html=null / cssClass=null → пустые строки");
        var w = new WidthWrapper(null, WidthsEnum.Full, null);
        DemoConsole.Case("Html == \"\"", () => w.Html, "");
        DemoConsole.Case("CssClass == \"\"", () => w.CssClass, "");
        DemoConsole.Case("Width сохранён", () => w.Width, WidthsEnum.Full);
    }

    private static void ToStringCheck()
    {
        DemoConsole.Note("ToString — <div ...>...</div>");
        DemoConsole.Case("Full + class=\"c\"", () =>
            new WidthWrapper("hi", WidthsEnum.Full, "c").ToString(),
            "<div class=\"c\" style=\"width:100%\">hi</div>");

        DemoConsole.Case("Medium без cssClass", () =>
            new WidthWrapper("hi", WidthsEnum.Medium, null).ToString(),
            "<div style=\"width:20rem\">hi</div>");

        DemoConsole.Case("Nothing без style", () =>
            new WidthWrapper("hi", WidthsEnum.Nothing, null).ToString(),
            "<div>hi</div>");

        DemoConsole.Case("class без style — только class", () =>
            new WidthWrapper("hi", WidthsEnum.Nothing, "c").ToString(),
            "<div class=\"c\">hi</div>");
    }
}