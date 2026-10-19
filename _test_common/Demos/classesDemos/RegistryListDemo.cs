using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class RegistryListDemo
{
    public static Task Run()
    {
        DemoConsole.Part("RegistryList / RegistryItemEventArgs / RegistryModeEnum");
        Ctors();
        FillMarkers();
        AddInsertEvents();
        AddLabelAndSystem();
        Lookups();
        GetValueOrKeyChecks();
        GetKeyForValueChecks();
        InclusionExpansion();
        WidthAndMode();
        ToStringRoundTrip();
        LocalizationCheck();
        EventArgsAndEnum();
        return Task.CompletedTask;
    }

    private static RegistryItem Item(string key, string value, int lvl = 0, bool label = false)
        => new(key, value, lvl, label);

    private static void Ctors()
    {
        DemoConsole.Note("ctors");
        DemoConsole.Case("empty → 0", () => new RegistryList().Items.Count(), 0);
        DemoConsole.Case("IEnumerable → 2", () =>
            new RegistryList(new[] { Item("a", "1"), Item("b", "2") }).Items.Count(), 2);
        DemoConsole.Case("IEnumerable c null → 2 (null skip)", () =>
        {
            var arr = new RegistryItem[] { Item("a", "1"), null!, Item("b", "2") };
            return new RegistryList(arr).Items.Count();
        }, 2);
        DemoConsole.Case("params RegistryItem[] → 2", () =>
            new RegistryList(Item("a", "1"), Item("b", "2")).Items.Count(), 2);
        DemoConsole.Case("string \"a=1;b=2\" → 2", () =>
            new RegistryList("a=1;b=2").Items.Count(), 2);
        DemoConsole.Case("string с пустым сегментом", () =>
            new RegistryList("a=1;;b=2").Items.Count(), 2);
        DemoConsole.Case("params string[] → 2", () =>
            new RegistryList("a=1", "b=2").Items.Count(), 2);
        DemoConsole.Case("params string[] с пустым", () =>
            new RegistryList("a=1", "", "b=2").Items.Count(), 2);
    }

    private static void FillMarkers()
    {
        DemoConsole.Note("FillFromString: маркеры '!' (null-item) и '*' (all-item)");
        DemoConsole.Case("'!' → AddNullItem", () =>
            new RegistryList("a=1;!;b=2").Items.Count(), 3);
        DemoConsole.Case("'!' повторно игнор", () =>
            new RegistryList("a=1;!;b=2;!").Items.Count(), 3);
        DemoConsole.Case("'*' → AddAllItems", () =>
            new RegistryList("a=1;*;b=2").Items.Count(), 3);
        DemoConsole.Case("'*' повторно игнор", () =>
            new RegistryList("a=1;*;b=2;*").Items.Count(), 3);

        DemoConsole.Note("FillFromString(null) — просто очищает");
        DemoConsole.Case("null IEnumerable", () =>
        {
            var r = new RegistryList("a=1"); r.FillFromString((IEnumerable<string>?)null!);
            return r.Items.Count();
        }, 0);
        DemoConsole.Case("null string[]", () =>
        {
            var r = new RegistryList("a=1"); r.FillFromString((string[]?)null!);
            return r.Items.Count();
        }, 0);
        DemoConsole.Case("null string", () =>
        {
            var r = new RegistryList("a=1"); r.FillFromString((string?)null!);
            return r.Items.Count();
        }, 0);
    }

    private static void AddInsertEvents()
    {
        DemoConsole.Note("Add / Insert / событие AddedItem");
        DemoConsole.Case("2× Add → 2 события", () =>
        {
            var r = new RegistryList(); int cnt = 0;
            r.AddedItem += (_, _) => cnt++;
            r.Add(Item("a", "1")); r.Add(Item("b", "2"));
            return cnt;
        }, 2);

        DemoConsole.Case("Add(null) no-op", () =>
        {
            var r = new RegistryList(); int cnt = 0;
            r.AddedItem += (_, _) => cnt++;
            r.Add(null!);
            return (r.Items.Count(), cnt);
        }, (0, 0));

        DemoConsole.Case("Insert(0, item) fires event", () =>
        {
            var r = new RegistryList(); int cnt = 0;
            r.AddedItem += (_, _) => cnt++;
            r.Insert(0, Item("x", "1"));
            return cnt;
        }, 1);

        DemoConsole.Case("Insert(1) в конец", () =>
        {
            var r = new RegistryList(); r.Add(Item("a", "1"));
            r.Insert(1, Item("b", "2"));
            return string.Join(",", r.Items.Select(i => i.Key));
        }, "a,b");

        DemoConsole.Case("Insert(null) no-op", () =>
        {
            var r = new RegistryList(); int cnt = 0;
            r.AddedItem += (_, _) => cnt++;
            r.Insert(0, null!);
            return cnt;
        }, 0);
    }

    private static void AddLabelAndSystem()
    {
        DemoConsole.Note("AddLabel / AddNullItem / AddAllItems");
        DemoConsole.Case("AddLabel → IsLabel, Level=0, Value=текст", () =>
        {
            var r = new RegistryList(); r.AddLabel("Header");
            var i = r.Items.First();
            return (i.IsLabel, i.Level, i.Value);
        }, (true, 0, "Header"));

        DemoConsole.Case("AddNullItem вставляет в начало (key)", () =>
        {
            var r = new RegistryList("a=1"); r.AddNullItem("null-key");
            return r.Items.First().Key;
        }, "null-key");

        DemoConsole.Case("AddAllItems вставляет в начало (key)", () =>
        {
            var r = new RegistryList("a=1"); r.AddAllItems("all-key");
            return r.Items.First().Key;
        }, "all-key");

        DemoConsole.Note("Значения системных items берутся из Resources.Common.Text_EmptyItem/Text_AllItems");
        DemoConsole.Case("AddNullItem не падает", () =>
        {
            try { var r = new RegistryList(); r.AddNullItem(); return true; }
            catch { return false; }
        }, true);
    }

    private static void Lookups()
    {
        DemoConsole.Note("GetItem / GetKey / GetValue / GetLevel");
        var r = new RegistryList("a=1;b=2;c=3");

        DemoConsole.Case("GetItem(\"b\").Value", () => r.GetItem("b")?.Value, "2");
        DemoConsole.Case("GetItem(null) → null", () => r.GetItem((string?)null), (RegistryItem?)null);
        DemoConsole.Case("GetItem(missing) → null", () => r.GetItem("zzz"), (RegistryItem?)null);
        DemoConsole.Case("GetItem(int) → key.ToString", () => r.GetItem(99), (RegistryItem?)null);

        DemoConsole.Case("GetKey(\"2\") → \"b\"", () => r.GetKey("2"), "b");
        DemoConsole.Case("GetKey(null) → null", () => r.GetKey(null!), (string?)null);
        DemoConsole.Case("GetKey(missing) → null", () => r.GetKey("zzz"), (string?)null);

        DemoConsole.Case("GetValue(\"a\") → \"1\"", () => r.GetValue("a"), "1");
        DemoConsole.Case("GetValue(missing) → null", () => r.GetValue("zzz"), (string?)null);
        DemoConsole.Case("GetValue(int) missing → null", () => r.GetValue(99), (string?)null);
        DemoConsole.Case("GetValue(int?) null → null", () => r.GetValue((int?)null), (string?)null);

        DemoConsole.Case("GetLevel(\"a\") → 0", () => r.GetLevel("a"), 0);
        DemoConsole.Case("GetLevel(missing) → 0", () => r.GetLevel("zzz"), 0);
        DemoConsole.Case("GetLevel(int?) null → 0", () => r.GetLevel((int?)null), 0);
    }

    private static void GetValueOrKeyChecks()
    {
        DemoConsole.Note("GetValueOrKey — с декоратором «–key–» при промахе");
        var r = new RegistryList("a=1");
        DemoConsole.Case("existing → \"1\"", () => r.GetValueOrKey("a"), "1");
        DemoConsole.Case("missing → \"–zzz–\"", () => r.GetValueOrKey("zzz"), "–zzz–");
        DemoConsole.Case("int missing → \"–99–\"", () => r.GetValueOrKey(99), "–99–");
    }

    private static void GetKeyForValueChecks()
    {
        DemoConsole.Note("GetKeyForValue — авто-регистрация с ключом по Count");
        DemoConsole.Case("existing value → existing key", () =>
        {
            var r = new RegistryList("a=1;b=2");
            return r.GetKeyForValue("2");
        }, "b");

        DemoConsole.Case("new value без newKey → ключ = _items.Count", () =>
        {
            var r = new RegistryList("a=1;b=2");     // count=2 → новый ключ "2"
            return r.GetKeyForValue("newval");
        }, "2");

        DemoConsole.Case("new value с newKey", () =>
        {
            var r = new RegistryList("a=1");
            return r.GetKeyForValue("newval", "custom");
        }, "custom");

        DemoConsole.Note("TODO/особенность: если исходные ключи уже «1,2,3...» — возможна коллизия. " +
                         "Демка не падает, но знай: Add() не проверяет дубликаты ключей.");
    }

    private static void InclusionExpansion()
    {
        DemoConsole.Note("GetValueInclusion / GetValueExpansion");
        var r = new RegistryList("user_id=100;user_name=vasya;other=x");

        DemoConsole.Case("Inclusion(\"user_\") → первое совпадение", () =>
            r.GetValueInclusion("user_"), "100");
        DemoConsole.Case("Inclusion(\"\") → null", () => r.GetValueInclusion(""), (string?)null);
        DemoConsole.Case("Inclusion(missing) → null", () => r.GetValueInclusion("zzz"), (string?)null);

        DemoConsole.Case("Expansion(\"select * from user_name\") → vasya", () =>
            r.GetValueExpansion("select * from user_name"), "vasya");
        DemoConsole.Case("Expansion(\"\") → null", () => r.GetValueExpansion(""), (string?)null);
        DemoConsole.Case("Expansion(no match) → null", () => r.GetValueExpansion("zzz"), (string?)null);
    }

    private static void WidthAndMode()
    {
        DemoConsole.Note("GetMaxWidth / GetProposeWidth / GetProposeMode");
        DemoConsole.Case("empty → 9 (basePadding)", () => new RegistryList().GetMaxWidth(), 9);
        DemoConsole.Case("max len=5 → 5+9=14", () =>
        {
            var r = new RegistryList(); r.Add(Item("k", "abcde"));
            return r.GetMaxWidth();
        }, 14);

        DemoConsole.Note("GetProposeWidth — пороги 11, 41, 101");
        DemoConsole.Case("10 → Small", () => RegistryList.GetProposeWidth(10), WidthsEnum.Small);
        DemoConsole.Case("11 → Medium", () => RegistryList.GetProposeWidth(11), WidthsEnum.Medium);
        DemoConsole.Case("40 → Medium", () => RegistryList.GetProposeWidth(40), WidthsEnum.Medium);
        DemoConsole.Case("41 → Large", () => RegistryList.GetProposeWidth(41), WidthsEnum.Large);
        DemoConsole.Case("100 → Large", () => RegistryList.GetProposeWidth(100), WidthsEnum.Large);
        DemoConsole.Case("101 → Full", () => RegistryList.GetProposeWidth(101), WidthsEnum.Full);

        DemoConsole.Note("GetProposeMode — по ширине + количеству");
        DemoConsole.Case("малая ширина + 5 → Inputs", () =>
        {
            var r = new RegistryList();
            for (int i = 0; i < 5; i++) r.Add(Item($"k{i}", "x"));
            return r.GetProposeMode();
        }, RegistryModeEnum.Inputs);

        DemoConsole.Case("Full width + 50 → Select", () =>
        {
            var r = new RegistryList();
            for (int i = 0; i < 50; i++) r.Add(Item($"k{i}", new string('x', 200)));
            return r.GetProposeMode();
        }, RegistryModeEnum.Select);
    }

    private static void ToStringRoundTrip()
    {
        DemoConsole.Note("ToString / round-trip");
        DemoConsole.Case("empty → \"\"", () => new RegistryList().ToString(), "");
        DemoConsole.Case("одна запись", () => new RegistryList("a=1").ToString(), "a=1");
        DemoConsole.Case("две записи", () => new RegistryList("a=1;b=2").ToString(), "a=1;b=2");
        DemoConsole.Case("round-trip count=3", () =>
            new RegistryList(new RegistryList("a=1;b=2;c=#:3val").ToString()).Items.Count(), 3);
    }

    private static void LocalizationCheck()
    {
        DemoConsole.Note("Localization(string) — работает корректно только для одной пары без ';'");

        DemoConsole.Case("Локализация одной пары \"a=new\"", () =>
        {
            var r = new RegistryList("a=old"); r.Localization("a=new");
            return r.GetValue("a");
        }, "new");

        DemoConsole.Case("null → no-op", () =>
        {
            var r = new RegistryList("a=old"); r.Localization((string)null!);
            return r.GetValue("a");
        }, "old");

        DemoConsole.Case("\"\" → no-op", () =>
        {
            var r = new RegistryList("a=old"); r.Localization("");
            return r.GetValue("a");
        }, "old");

        DemoConsole.Note("BUG: Localization(string) использует new RegistryList(string.Empty, source) — " +
                         "это params-версия (2 аргумента → string[]), а не parse-with-semicolons. " +
                         "Поэтому ';' не парсится: 'a=new;c=added' уходит целиком в Value.");

        var r1 = new RegistryList("a=old;b=keep");
        r1.Localization("a=new;c=added");
        DemoConsole.Case("a → \"new;c=added\" (с ';' внутри)", () => r1.GetValue("a"), "new;c=added");
        DemoConsole.Case("b → \"keep\" (не перезаписан)", () => r1.GetValue("b"), "keep");
        DemoConsole.Case("c → null (ключ не появился)", () => r1.GetValue("c"), (string?)null);

        DemoConsole.CaseThrows<ArgumentException>("Localization(\"garbage\") — без '=' бросает", () =>
        {
            var r = new RegistryList("a=old"); r.Localization("garbage");
        });
    }

    private static void EventArgsAndEnum()
    {
        DemoConsole.Note("RegistryItemEventArgs / RegistryModeEnum");
        var it = Item("k", "v");
        var args = new RegistryItemEventArgs(it);
        DemoConsole.Case("EventArgs.Item == item по ссылке", () => ReferenceEquals(args.Item, it), true);

        DemoConsole.Case("Auto = 0", () => (int)RegistryModeEnum.Auto, 0);
        DemoConsole.Case("Inputs = 1", () => (int)RegistryModeEnum.Inputs, 1);
        DemoConsole.Case("Select = 2", () => (int)RegistryModeEnum.Select, 2);
    }
}