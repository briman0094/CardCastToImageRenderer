using CardCastToImage.Web.HostedServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CardCastToImage.Web
{
	public class Program
	{
		public static void Main( string[] args )
		{
			CreateHostBuilder( args ).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder( string[] args ) =>
			Host.CreateDefaultBuilder( args )
				.ConfigureWebHostDefaults( webBuilder => {
					webBuilder.UseStartup<Startup>();
				} )
				.ConfigureServices( services => {
					services.AddHostedService<PeriodicService>();
				} );
	}
}