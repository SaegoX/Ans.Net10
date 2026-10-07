using _test_common.Framework;
using Ans.Net10.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace _test_common
{

	internal class Program
	{

		static readonly IServiceCollection _Services = new ServiceCollection();
		//static readonly string _ApiUrl = "https://api.guap.ru/rasp-sem/v1/get-info";


		static void Main()
		{
			SuppCulture.AddCodePagesSupport();
			Console.InputEncoding = Encoding.UTF8;
			Console.OutputEncoding = Encoding.UTF8;

			SuppConsole.AppStart();

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

            var registry = DemoRegistry.Build(typeof(Program).Assembly);
            new DemoMenu(registry).Run();

            SuppConsole.AppEnd();

		}

	}

}
