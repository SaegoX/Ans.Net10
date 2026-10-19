using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class CrudFaceDemo
{
    public static Task Run()
    {
        DemoConsole.Part("CrudFace / ICrudFace");
        CtorExplicit();
        CtorFromString_Full();
        CtorFromString_PartialSegments();
        FallbackTitles();
        HasFlags();
        ToStringCheck();
        return Task.CompletedTask;
    }

    private static void CtorExplicit()
    {
        DemoConsole.Note("ctor с явными параметрами");
        var f = new CrudFace("name", "Title", "ShortT", "Desc", "Sample", "https://help");
        DemoConsole.Case("Name", () => f.Name, "name");
        DemoConsole.Case("Title", () => f.Title, "Title");
        DemoConsole.Case("ShortTitle", () => f.ShortTitle, "ShortT");
        DemoConsole.Case("Description", () => f.Description, "Desc");
        DemoConsole.Case("Sample", () => f.Sample, "Sample");
        DemoConsole.Case("HelpLink", () => f.HelpLink, "https://help");
    }

    private static void CtorFromString_Full()
    {
        DemoConsole.Note("ctor из строки: 5 сегментов через '|'");
        var f = new CrudFace("name", "Title|ShortT|Desc|Sample|https://help");
        DemoConsole.Case("TitleRaw", () => f.TitleRaw, "Title");
        DemoConsole.Case("ShortTitleRaw", () => f.ShortTitleRaw, "ShortT");
        DemoConsole.Case("Description", () => f.Description, "Desc");
        DemoConsole.Case("Sample", () => f.Sample, "Sample");
        DemoConsole.Case("HelpLink", () => f.HelpLink, "https://help");
    }

    private static void CtorFromString_PartialSegments()
    {
        DemoConsole.Note("ctor из строки — меньше 5 сегментов (недостающие → \"\")");
        DemoConsole.Case("0 сегментов: \"\"", () =>
        {
            var f = new CrudFace("n", "");
            return $"{f.TitleRaw.Length}|{f.Description.Length}|{f.HelpLink.Length}";
        }, "0|0|0");
        DemoConsole.Case("1 сегмент: \"T\"", () => new CrudFace("n", "T").TitleRaw, "T");
        DemoConsole.Case("2 сегмента: \"T|S\"", () => new CrudFace("n", "T|S").ShortTitleRaw, "S");
        DemoConsole.Case("3 сегмента: \"T|S|D\"", () => new CrudFace("n", "T|S|D").Description, "D");
        DemoConsole.Case("4 сегмента: \"T|S|D|Sm\"", () => new CrudFace("n", "T|S|D|Sm").Sample, "Sm");

        DemoConsole.Note("ctor из строки — больше 5 сегментов (лишние игнорируются)");
        DemoConsole.Case("\"T|S|D|Sm|H|extra|more\"", () =>
        {
            var f = new CrudFace("n", "T|S|D|Sm|H|extra|more");
            return f.HelpLink; // ожидаем "H", а не "H|extra|more"
        }, "H");
    }

    private static void FallbackTitles()
    {
        DemoConsole.Note("Fallback: пустой TitleRaw → Title = Name; пустой ShortTitleRaw → Title");
        var f1 = new CrudFace("userName", "||desc||"); 
        DemoConsole.Case("Title пустой → = Name", () => f1.Title, "userName");
        DemoConsole.Case("ShortTitle пустой → = Title", () => f1.ShortTitle, "userName");
        DemoConsole.Case("Description задан", () => f1.Description, "desc");

        var f2 = new CrudFace("x", "T||");
        DemoConsole.Case("Title задан, ShortTitle пуст", () => f2.ShortTitle, "T");

        var f3 = new CrudFace("x", "T|S");
        DemoConsole.Case("оба заданы", () => f3.Title + "/" + f3.ShortTitle, "T/S");
    }

    private static void HasFlags()
    {
        DemoConsole.Note("Has* флаги");
        var empty = new CrudFace("n", "");
        DemoConsole.Case("HasTitle=false", () => empty.HasTitle, false);
        DemoConsole.Case("HasShortTitle=false", () => empty.HasShortTitle, false);
        DemoConsole.Case("HasDescription=false", () => empty.HasDescription, false);
        DemoConsole.Case("HasSample=false", () => empty.HasSample, false);
        DemoConsole.Case("HasHelpLink=false", () => empty.HasHelpLink, false);
        DemoConsole.Case("HasFace=false", () => empty.HasFace, false);

        var full = new CrudFace("n", "T|S|D|Sm|H");
        DemoConsole.Case("full.HasTitle=true", () => full.HasTitle, true);
        DemoConsole.Case("full.HasFace=true", () => full.HasFace, true);

        var onlyDesc = new CrudFace("n", "||D");
        DemoConsole.Case("только Desc → HasFace=true", () => onlyDesc.HasFace, true);
    }

    private static void ToStringCheck()
    {
        DemoConsole.Note("ToString() — собирает обратно через '|'");
        var f = new CrudFace("n", "T|S|D|Sm|H");
        DemoConsole.Case("ToString() == исходная строка", () => f.ToString(), "T|S|D|Sm|H");

        var empty = new CrudFace("n", "");
        DemoConsole.Case("пустой → \"||||\"", () => empty.ToString(), "||||");
    }
}