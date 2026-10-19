using MimeKit;
using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class MailMessageModelDemo
{
    public static Task Run()
    {
        DemoConsole.Part("MailMessageModel");
        Defaults();
        SetValues();
        EmptyCollections();
        return Task.CompletedTask;
    }

    private static void Defaults()
    {
        DemoConsole.Note("Значения по умолчанию");
        var m = new MailMessageModel();
        DemoConsole.Case("From is null", () => m.From is null, true);
        DemoConsole.Case("To is null", () => m.To is null, true);
        DemoConsole.Case("Cc is not null", () => m.Cc is not null, true);
        DemoConsole.Case("Cc.Length == 0", () => m.Cc.Length, 0);
        DemoConsole.Case("Bcc is not null", () => m.Bcc is not null, true);
        DemoConsole.Case("Bcc.Length == 0", () => m.Bcc.Length, 0);
        DemoConsole.Case("Subject == \"\"", () => m.Subject, "");
        DemoConsole.Case("ContentHtml == \"\"", () => m.ContentHtml, "");
    }

    private static void SetValues()
    {
        DemoConsole.Note("Установка значений");
        var m = new MailMessageModel
        {
            From = new MailboxAddress("Вова", "vova@example.com"),
            To = new MailboxAddress("Гоша", "gosha@example.com"),
            Cc = [new MailboxAddress("Копия", "cc@example.com")],
            Bcc = [new MailboxAddress("Скрытый", "bcc@example.com")],
            Subject = "Тема",
            ContentHtml = "<h1>Привет!</h1>"
        };

        DemoConsole.Case("From.Address", () => m.From!.Address, "vova@example.com");
        DemoConsole.Case("From.Name", () => m.From!.Name, "Вова");
        DemoConsole.Case("To.Address", () => m.To!.Address, "gosha@example.com");
        DemoConsole.Case("Cc.Length", () => m.Cc.Length, 1);
        DemoConsole.Case("Cc[0].Address", () => m.Cc[0].Address, "cc@example.com");
        DemoConsole.Case("Bcc.Length", () => m.Bcc.Length, 1);
        DemoConsole.Case("Subject", () => m.Subject, "Тема");
        DemoConsole.Case("ContentHtml", () => m.ContentHtml, "<h1>Привет!</h1>");
    }

    private static void EmptyCollections()
    {
        DemoConsole.Note("Пустые массивы Cc/Bcc — валидны");
        var m = new MailMessageModel { Cc = [], Bcc = [] };
        DemoConsole.Case("Cc.Length == 0", () => m.Cc.Length, 0);
        DemoConsole.Case("Bcc.Length == 0", () => m.Bcc.Length, 0);

        DemoConsole.Note("Установка null в Cc/Bcc — компилятор предупредит, но рантайм пропустит");
        // m.Cc = null!; // если очень надо — но это нарушит контракт модели
    }
}