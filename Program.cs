using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Task8
{
	internal class Program
	{
		private static ServiceProvider CreateServices()
		{
			var sp = new ServiceCollection();
			sp.AddTransient<IHandler, Handler>();
			sp.AddLogging(builder => builder.AddConsole());

			return sp.BuildServiceProvider();
		}

		static async Task Main(string[] args)
		{
			var services = CreateServices();

			var h = services.GetService<IHandler>();
			
			await h.PerformOperation(CancellationToken.None);
		}
	}
}
