using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class Exts__reflectionDemo
{
    private sealed class Sample
    {
        public int Id { get; set; } = 42;
        public string Name { get; set; } = "vasya";
        public int PublicField = 7;
        public int ReadOnly => 100;
        private int Hidden { get; set; } = 999;
    }

    [AttributeUsage(AttributeTargets.Class)]
    private sealed class MarkAttribute : Attribute { public string Value { get; set; } = ""; }

    [Mark(Value = "hello")]
    private sealed class Marked { }

    public static Task Run()
    {
        DemoConsole.Part("Exts__reflection");
        ToDynamicChecks();
        GetAttributeChecks();
        GetCSharpTypeNameChecks();
        GetCSharpGenericsChecks();
        GetCSharpParamsChecks();
        GetPropertyNameChecks();
        GetPropertyValueChecks();
        DefaultObjectChecks();
        return Task.CompletedTask;
    }

    private static void ToDynamicChecks()
    {
        DemoConsole.Note("ToDynamic — ExpandoObject из публичных свойств");
        var dyn = new Sample().ToDynamic();
        var dict = (IDictionary<string, object?>)dyn;
        DemoConsole.Case("содержит Id", () => dict.ContainsKey("Id"), true);
        DemoConsole.Case("содержит Name", () => dict.ContainsKey("Name"), true);
        DemoConsole.Case("не содержит Hidden (private)", () => dict.ContainsKey("Hidden"), false);
        DemoConsole.Case("Id.Value == 42", () => dict["Id"], 42);

        DemoConsole.CaseThrows<ArgumentNullException>("null → ArgumentNullException", () =>
            _ = ((object)null!).ToDynamic());
    }

    private static void GetAttributeChecks()
    {
        DemoConsole.Note("GetAttribute<T>");
        DemoConsole.Case("Marked.GetAttribute<Mark>() != null", () =>
            typeof(Marked).GetAttribute<MarkAttribute>() is not null, true);
        DemoConsole.Case("Marked.Value == hello", () =>
            typeof(Marked).GetAttribute<MarkAttribute>()!.Value, "hello");
        DemoConsole.Case("Sample (нет атрибута) → null", () =>
            typeof(Sample).GetAttribute<MarkAttribute>(), (MarkAttribute?)null);
    }

    private static void GetCSharpTypeNameChecks()
    {
        DemoConsole.Note("GetCSharpTypeName");
        DemoConsole.Case("int", () => typeof(int).GetCSharpTypeName(), "int");
        DemoConsole.Case("string", () => typeof(string).GetCSharpTypeName(), "string");
        DemoConsole.Case("int?", () => typeof(int?).GetCSharpTypeName(), "int?");
        DemoConsole.Case("int[]", () => typeof(int[]).GetCSharpTypeName(), "int[]");
        DemoConsole.Case("List<int>", () => typeof(List<int>).GetCSharpTypeName(), "List<int>");
        DemoConsole.Case("Dictionary<string,int>", () =>
            typeof(Dictionary<string, int>).GetCSharpTypeName(), "Dictionary<string, int>");

        DemoConsole.Note("isDropNullable=true — убирает '?'");
        DemoConsole.Case("int? с dropNullable=true → int", () =>
            typeof(int?).GetCSharpTypeName(isDropNullable: true), "int");
    }

    private static void GetCSharpGenericsChecks()
    {
        DemoConsole.Note("GetCSharpGenerics");
        var generic = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Empty" && m.IsGenericMethodDefinition);
        var nonGeneric = typeof(Sample).GetProperty("Id")!.GetGetMethod()!;

        DemoConsole.Case("generic method → \"<T>\"", () =>
            generic.GetCSharpGenerics() is not null, true);
        DemoConsole.Case("non-generic → null", () =>
            nonGeneric.GetCSharpGenerics(), (string?)null);
    }

    private static void GetCSharpParamsChecks()
    {
        DemoConsole.Note("GetCSharpParams");
        var method = typeof(string).GetMethod("Substring", new[] { typeof(int), typeof(int) })!;
        var ps = method.GetCSharpParams().ToArray();

        DemoConsole.Case("2 параметра", () => ps.Length, 2);
        DemoConsole.Case("содержит \"int startIndex\"", () => ps[0].Contains("int") && ps[0].Contains("startIndex"), true);
        DemoConsole.Case("содержит \"int length\"", () => ps[1].Contains("int") && ps[1].Contains("length"), true);
    }

    private static void GetPropertyNameChecks()
    {
        DemoConsole.Note("GetPropertyName — чистит get_/set_");
        var getter = typeof(Sample).GetProperty("Id")!.GetGetMethod()!;
        var setter = typeof(Sample).GetProperty("Id")!.GetSetMethod()!;

        DemoConsole.Case("get_Id → Id", () => getter.GetPropertyName(), "Id");
        DemoConsole.Case("set_Id → Id", () => setter.GetPropertyName(), "Id");
        DemoConsole.Case("не-аксессор сохраняет имя", () =>
            typeof(Sample).GetMethod("ToString")!.GetPropertyName(), "ToString");
    }

    private static void GetPropertyValueChecks()
    {
        DemoConsole.Note("GetPropertyValue — object + name");
        var s = new Sample { Id = 42, Name = "vasya" };
        DemoConsole.Case("Id → 42", () => s.GetPropertyValue("Id"), 42);
        DemoConsole.Case("Name → vasya", () => s.GetPropertyValue("Name"), "vasya");
        DemoConsole.Case("ReadOnly → 100", () => s.GetPropertyValue("ReadOnly"), 100);

        DemoConsole.CaseThrows<InvalidOperationException>("missing → IOE", () =>
            _ = s.GetPropertyValue("zzz"));
    }

    private static void DefaultObjectChecks()
    {
        DemoConsole.Note("DefaultObject<T>");
        DemoConsole.Case("obj=42 (object), T=int, default=0 → 42", () =>
            ((object?)42).DefaultObject(0), 42);
        DemoConsole.Case("obj=null, default=99 → 99", () =>
            ((object?)null).DefaultObject(99), 99);

        DemoConsole.Case("obj=null, default(T) → 0 (для int)", () =>
            ((object?)null).DefaultObject<int>(), 0);
        DemoConsole.Case("obj=null, default(T) → null (для string)", () =>
            ((object?)null).DefaultObject<string?>(), (string?)null);
    }
}