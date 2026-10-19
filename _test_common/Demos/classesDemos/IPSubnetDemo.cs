using System.Net;
using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class IPSubnetDemo
{
    public static Task Run()
    {
        DemoConsole.Part("IPSubnet / IPSubnetsList");
        SubnetV4Boundaries();
        SubnetV6Boundaries();
        SubnetParseErrors();
        SubnetToStringNotCanonical();
        SubnetsList();
        return Task.CompletedTask;
    }

    private static void SubnetV4Boundaries()
    {
        DemoConsole.Note("V4 — граничные длины маски");
        DemoConsole.Case("192.168.1.0/0  → MaskV4Template=0", () =>
            new IPSubnet("192.168.1.0/0").MaskV4Template, 0u);
        DemoConsole.Case("192.168.1.0/32 → MaskV4Template=uint.MaxValue", () =>
            new IPSubnet("192.168.1.0/32").MaskV4Template, uint.MaxValue);
        DemoConsole.Case("192.168.1.0/24 MaskV4Result", () =>
            new IPSubnet("192.168.1.0/24").MaskV4Result, 0xC0A80100u);
        DemoConsole.CaseThrows<NotSupportedException>("/33", () => _ = new IPSubnet("1.2.3.4/33"));
        DemoConsole.CaseThrows<NotSupportedException>("/-1 (via IPAddress ctor)", () =>
            _ = new IPSubnet(IPAddress.Parse("1.2.3.4"), -1));
    }

    private static void SubnetV6Boundaries()
    {
        DemoConsole.Note("V6 — /0, /10, /128, /129");
        DemoConsole.Case("/0 MaskV6Bytes all zero", () =>
            new IPSubnet("::/0").MaskV6Bytes.All(b => b == 0), true);
        DemoConsole.Case("/128 fe80:: — IsV6", () =>
            new IPSubnet("fe80::/128").IsV6, true);
        DemoConsole.CaseThrows<NotSupportedException>("/129", () =>
            _ = new IPSubnet("::/129"));
    }

    private static void SubnetParseErrors()
    {
        DemoConsole.Note("Ошибки парсинга");
        DemoConsole.CaseThrows<NotSupportedException>("без '/'", () => _ = new IPSubnet("192.168.1.0"));
        DemoConsole.CaseThrows<NullReferenceException>("cidr=null → NRE (баг; в doc — ANE)", () =>
            _ = new IPSubnet((string)null!));
        DemoConsole.CaseThrows<NotSupportedException>("cidr=\"\" → NotSupported", () =>
            _ = new IPSubnet(""));
        DemoConsole.CaseThrows<FormatException>("garbage/24", () => _ = new IPSubnet("garbage/24"));
        DemoConsole.CaseThrows<FormatException>("1.2.3.4/abc", () => _ = new IPSubnet("1.2.3.4/abc"));
    }

    private static void SubnetToStringNotCanonical()
    {
        DemoConsole.Note("ToString — не канонизирует адрес к сетевому");
        DemoConsole.Case("192.168.1.100/24 → \"192.168.1.100/24\" (не .0!)", () =>
            new IPSubnet("192.168.1.100/24").ToString(), "192.168.1.100/24");
        DemoConsole.Case("но MaskV4Result уже маскированный (.0)", () =>
            new IPSubnet("192.168.1.100/24").MaskV4Result, 0xC0A80100u);
    }

    private static void SubnetsList()
    {
        DemoConsole.Note("IPSubnetsList");
        DemoConsole.Case("null → 0", () => new IPSubnetsList((string)null!).Count, 0);  // При корректировки логики в самом классе правильно будет написать так:
                                                                                        // DemoConsole.Case("null → 0", () => new IPSubnetsList((string?)null).Count, 0);
        DemoConsole.Case("\"\" → 0", () => new IPSubnetsList("").Count, 0);
        DemoConsole.Case("\"   \" → 0", () => new IPSubnetsList("   ").Count, 0);
        DemoConsole.Case("один cidr → 1", () => new IPSubnetsList("192.168.1.0/24").Count, 1);
        DemoConsole.Case("два через ';' → 2", () => new IPSubnetsList("192.168.1.0/24;10.0.0.0/8").Count, 2);
        DemoConsole.Case("два через ',' → 2", () => new IPSubnetsList("192.168.1.0/24,10.0.0.0/8").Count, 2);
        DemoConsole.Case("пробелы обрезаются", () => new IPSubnetsList("  10.0.0.0/8  ;  192.168.1.0/24  ").Count, 2);
        DemoConsole.Case("params с пустыми", () => new IPSubnetsList(new[] { "10.0.0.0/8", "", "192.168.1.0/24" }).Count, 2);
        DemoConsole.Case("ToString через ';'", () =>
            new IPSubnetsList("192.168.1.0/24", "10.0.0.0/8").ToString().Contains(';'), true);
        DemoConsole.CaseThrows<NotSupportedException>("невалидный элемент бросает", () =>
            _ = new IPSubnetsList("192.168.1.0/24;garbage"));
    }
}