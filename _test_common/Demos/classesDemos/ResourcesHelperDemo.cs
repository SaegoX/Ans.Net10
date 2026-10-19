using System.Resources;
using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class ResourcesHelperDemo
{
    public static Task Run()
    {
        DemoConsole.Part("ResourcesHelper");
        CtorChecks();
        GetFormResourceManagersStatic();
        GetCrudFaceGuard();
        return Task.CompletedTask;
    }

    private static void CtorChecks()
    {
        DemoConsole.Note("ctor — Resources = resources + Faces.ResourceManager");
        DemoConsole.Case("без аргументов → минимум 1 ресурс", () =>
            new ResourcesHelper().Resources.Length >= 1, true);

        DemoConsole.Note("Всегда содержит Faces.ResourceManager последним — проверяем через длину");
        DemoConsole.Case("с пользовательским ресурсом → ≥2", () =>
        {
            var fake = new ResourceManager("Fake", typeof(ResourcesHelper).Assembly);
            return new ResourcesHelper(fake).Resources.Length >= 2;
        }, true);
    }

    private static void GetFormResourceManagersStatic()
    {
        DemoConsole.Note("GetFormResourceManagers — static аналог ctor");
        DemoConsole.Case("возвращает непустой массив", () =>
            ResourcesHelper.GetFormResourceManagers().Length >= 1, true);
    }

    private static void GetCrudFaceGuard()
    {
        DemoConsole.Note("GetCrudFace(null) → null (guard)");
        DemoConsole.Case("null → null", () =>
            new ResourcesHelper().GetCrudFace(null!), (CrudFace?)null);

        DemoConsole.Note("GetCrudFace(\"nonexistent_xyz_key\") — обычно не падает, возвращает CrudFace с пустыми полями. " +
                         "TODO: точный кейс требует наличия .resx-ключей Faces.");
    }
}