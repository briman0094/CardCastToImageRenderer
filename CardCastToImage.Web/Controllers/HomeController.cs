using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using CardCastToImage.Web.Models;

namespace CardCastToImage.Web.Controllers
{
	[ SuppressMessage( "ReSharper", "ArrangeThisQualifier" ) ]
	public class HomeController : Controller
	{
		public IActionResult Index()
			=> View();

		[ ResponseCache( Duration = 0, Location = ResponseCacheLocation.None, NoStore = true ) ]
		public IActionResult Error()
			=> View( new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier } );
	}
}