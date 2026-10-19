using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class ObjInfoBuilderDemo
{
    public class TestClass
    {
        public string Name { get; set; } = "default";
        public int Age { get; set; } = 42;
        public string ReadOnly => "ro-value";
        public string WriteOnly { set { } }

        public void DoSomething() { }
        public int Calculate(int x) => x * 2;
        public T Echo<T>(T val) => val;
    }

    public class EmptyClass { }

    public static Task Run()
    {
        DemoConsole.Part("ObjInfoBuilder / ObjInfoProperty / ObjInfoMethod / ObjInfoFunction");
        BasicParse();
        WithObjectValues();
        PropertiesCategorized();
        GenericsAndSignatures();
        EmptyClassChecks();
        return Task.CompletedTask;
    }

    private static void BasicParse()
    {
        DemoConsole.Note("Разбор TestClass без obj — значения null");
        var b = new ObjInfoBuilder(typeof(TestClass));
        DemoConsole.Case("InfoType == typeof(TestClass)", () => b.InfoType == typeof(TestClass), true);
        DemoConsole.Case("Ctors.Count ≥ 1", () => b.Ctors.Count >= 1, true);
        DemoConsole.Case("HasProperties", () => b.HasProperties, true);
    }

    private static void WithObjectValues()
    {
        DemoConsole.Note("Разбор TestClass с obj — свойства заполняются значениями");
        var obj = new TestClass { Name = "Вася", Age = 30 };
        var b = new ObjInfoBuilder(typeof(TestClass), obj);

        DemoConsole.Case("Name.Value == \"Вася\"", () =>
            b.ReadWriteProperties.First(p => p.Name == "Name").Value, "\"Вася\"");

        DemoConsole.Case("Age.Value == 30", () =>
            b.ReadWriteProperties.First(p => p.Name == "Age").Value, "30");

        DemoConsole.Note("ReadOnly тоже заполнено");
        DemoConsole.Case("ReadOnly.Value != null", () =>
            b.ReadOnlyProperties.First(p => p.Name == "ReadOnly").Value is not null, true);
    }

    private static void PropertiesCategorized()
    {
        DemoConsole.Note("Категории свойств");
        var b = new ObjInfoBuilder(typeof(TestClass));

        DemoConsole.Case("ReadWrite содержит Name", () =>
            b.ReadWriteProperties.Any(p => p.Name == "Name"), true);
        DemoConsole.Case("ReadWrite содержит Age", () =>
            b.ReadWriteProperties.Any(p => p.Name == "Age"), true);
        DemoConsole.Case("ReadOnly содержит ReadOnly", () =>
            b.ReadOnlyProperties.Any(p => p.Name == "ReadOnly"), true);
        DemoConsole.Case("WriteOnly содержит WriteOnly", () =>
            b.WriteOnlyProperties.Any(p => p.Name == "WriteOnly"), true);

        DemoConsole.Case("HasReadWriteProperties", () => b.HasReadWriteProperties, true);
        DemoConsole.Case("HasReadonlyProperties", () => b.HasReadonlyProperties, true);
        DemoConsole.Case("HasWriteonlyProperties", () => b.HasWriteonlyProperties, true);

        DemoConsole.Note("TypeName для Name — \"string\"");
        DemoConsole.Case("TypeName of Name", () =>
            b.ReadWriteProperties.First(p => p.Name == "Name").TypeName, "string");
    }

    private static void GenericsAndSignatures()
    {
        DemoConsole.Note("Functions vs Methods, Generics");
        var b = new ObjInfoBuilder(typeof(TestClass));

        DemoConsole.Case("DoSomething — в Methods (void)", () =>
            b.Methods.Any(m => m.Name == "DoSomething"), true);

        DemoConsole.Case("Calculate — в Functions", () =>
            b.Functions.Any(f => f.Name == "Calculate"), true);

        DemoConsole.Case("Echo<T> — в Functions", () =>
            b.Functions.Any(f => f.Name == "Echo"), true);

        DemoConsole.Case("Echo<T>.Generics != null", () =>
            b.Functions.First(f => f.Name == "Echo").Generics is not null, true);

        DemoConsole.Case("Calculate.Parameters.Count == 1", () =>
            b.Functions.First(f => f.Name == "Calculate").Parameters.Count(), 1);
    }

    private static void EmptyClassChecks()
    {
        DemoConsole.Note("Пустой класс — только ctor");
        var b = new ObjInfoBuilder(typeof(EmptyClass));
        DemoConsole.Case("Ctors.Count ≥ 1", () => b.Ctors.Count >= 1, true);
        DemoConsole.Case("HasProperties == false", () => b.HasProperties, false);
        DemoConsole.Case("Functions.Count == 0", () => b.Functions.Count, 0);
        DemoConsole.Case("Methods.Count == 0", () => b.Methods.Count, 0);

        DemoConsole.Note("typeof(object) — все методы унаследованы от object → отфильтрованы");
        var bObj = new ObjInfoBuilder(typeof(object));
        DemoConsole.Case("object Functions пусто", () => bObj.Functions.Count, 0);
        DemoConsole.Case("object Methods пусто", () => bObj.Methods.Count, 0);
    }
}