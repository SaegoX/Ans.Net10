using Ans.Net10.Common;

namespace _test_common.Framework
{
    /// <summary>
    /// Печатает вступительную информацию о приложении и подключённой библиотеке
    /// </summary>
    public class AppInfo
    {
        /// <summary>Печатает блок информации о сборке приложения, библиотеке и путях проекта</summary>
        public static void Print()
        {
            SuppConsole.WriteLineParam("App.Name", SuppApp.EntryAssembly.Name);
            SuppConsole.WriteLineParam("App.Version", SuppApp.EntryAssembly.Version);
            SuppConsole.WriteLineParam("App.FullVersion", SuppApp.EntryAssembly.FullVersion);
            SuppConsole.WriteLineParam("App.Description", SuppApp.EntryAssembly.Description);
            Console.WriteLine();

            SuppConsole.WriteLineParam("Ans.Net10.Common.Name", LibCommonInfo.Name);
            SuppConsole.WriteLineParam("Ans.Net10.Common.Version", LibCommonInfo.Version);
            SuppConsole.WriteLineParam("Ans.Net10.Common.FullVersion", LibCommonInfo.FullVersion);
            SuppConsole.WriteLineParam("Ans.Net10.Common.Description", LibCommonInfo.Description);
            Console.WriteLine();

            SuppConsole.WriteLineParam("SuppApp.CurrentDirectory", SuppApp.CurrentDirectory);
            SuppConsole.WriteLineParam("SuppApp.BaseDirectory", SuppApp.BaseDirectory);
            SuppConsole.WriteLineParam("SuppApp.VSProjectName", SuppApp.VSProjectName);
            SuppConsole.WriteLineParam("SuppApp.VSProjectPath", SuppApp.VSProjectPath);
            SuppConsole.WriteLineParam("SuppApp.VSSolutionName", SuppApp.VSSolutionName);
            SuppConsole.WriteLineParam("SuppApp.VSSolutionPath", SuppApp.VSSolutionPath);
            Console.WriteLine();
        }
    }
}
