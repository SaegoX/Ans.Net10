using _test_common.Framework;

namespace _test_common.Demos;

/// <summary>
/// Точка входа в окружение демок. Здесь перечислены все демо-классы в порядке отображения.
/// </summary>
public static class AllDemos
{
    public static async Task Run()
    {
        DemoConsole.Section("~classes");

        // Словари / парсеры
        await _Dict_ProtoDemo.Run();
        await DictIntDemo.Run();
        await DictStringDemo.Run();
        await DictTagStylersDemo.Run();
        await KeysComparerDemo.Run();
        await StringParserDemo.Run();
        await RegistryItemDemo.Run();
        await RegistryListDemo.Run();

        // Пагинация
        await _ListPaginatedModelDemo.Run();
        await PaginatedQueryableHelperDemo.Run();
        await PaginationHelperDemo.Run();    
        await PaginationModelDemo.Run();
        await PaginatedDataModelDemo.Run();   

        // Дерево
        await TreeItemDemo.Run();
        await TreeEntityHelperDemo.Run();

        // Сеть / почта / кэш
        await _WebResult_BaseDemo.Run();
        await WebApiResultDemo.Run();  
        await WebApiHelperDemo.Run();
        await IPSubnetDemo.Run();
        await HybridCacheHelperDemo.Run();
        await MailMessageModelDemo.Run();

        // URL-параметры
        await ParamsCollectionDemo.Run();      
        await ParamsBuilderDemo.Run();        

        // Коллекции
        await CollectionsComparerDemo.Run();

        // HTML-стили
        await TagClassesBuilderDemo.Run();
        await TagStylesBuilderDemo.Run();
        await TagStylerDemo.Run();
        await WidthWrapperDemo.Run();

        // HTML-атрибуты
        await TagAttributesBuilderDemo.Run();
        await TinyLogWriterHelperDemo.Run();  
        await TinyTimerHelperDemo.Run();       
        await TypografHelperDemo.Run();         

        // Метаданные / сортировка
        await ObjInfoBuilderDemo.Run();
        await OrderBuilderDemo.Run();

        // Прочее
        await ClassIdWrapperDemo.Run();
        await ContentInfoDemo.Run();
        await CrudFaceDemo.Run();
        await GridRowParserDemo.Run();
        await ImageResizeHelperDemo.Run();
        await DateTimeHelperDemo.Run();


        // Утилиты
        await TinyBreakerHelperDemo.Run(); 
        await ResourcesHelperDemo.Run();     

        // Консоль
        await ConsoleMenuDemo.Run();

        DemoConsole.Section("~exts");

        await Exts__toDemo.Run();
        await Exts__collectionsDemo.Run();    
        await Exts__datetimeDemo.Run();    
        await Exts__dictionaryDemo.Run();     
        await Exts__diDemo.Run();
        await Exts__ioDemo.Run();   
        await Exts__linqDemo.Run();
        await Exts__makeDemo.Run();   
        await Exts__reflectionDemo.Run();  

        DemoConsole.Section("~supps");

        await SuppDateTimeDemo.Run();   
    }
}
