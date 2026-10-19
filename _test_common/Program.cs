using _test_common.Demos;
using _test_common.Framework;
using Ans.Net10.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Globalization;

namespace _test_common
{

	internal class Program
	{

		static readonly IServiceCollection _Services = new ServiceCollection();
		//static readonly string _ApiUrl = "https://api.guap.ru/rasp-sem/v1/get-info";


		static async Task Main()
		{
			SuppCulture.AddCodePagesSupport();
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
            CultureInfo.CurrentUICulture = new CultureInfo("ru-RU");
            Console.InputEncoding = Encoding.UTF8;
			Console.OutputEncoding = Encoding.UTF8;

			SuppConsole.AppStart();

            AppInfo.Print();
            await AllDemos.Run();

            SuppConsole.AppEnd();

		}

	}

}
