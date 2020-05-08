using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CardCastToImage.Web
{
	public class Startup
	{
		public Startup( IConfiguration configuration )
		{
			this.Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices( IServiceCollection services )
		{
			services.AddControllersWithViews()
					.AddRazorRuntimeCompilation();
		}

		public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
		{
			// Set culture
			var defaultCulture = new CultureInfo( "en-US" );

			CultureInfo.CurrentCulture                = defaultCulture;
			CultureInfo.CurrentUICulture              = defaultCulture;
			CultureInfo.DefaultThreadCurrentCulture   = defaultCulture;
			CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

			if ( env.IsDevelopment() )
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler( "/Home/Error" );
				app.UseHsts();
			}

			// app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints( endpoints => {
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}" );
			} );
		}
	}
}