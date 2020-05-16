using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using CardCastToImage.Services;
using CardCastToImage.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CardCastToImage.Web.Controllers
{
	[ SuppressMessage( "ReSharper", "ArrangeThisQualifier" ) ]
	public class DeckController : Controller
	{
		[ Route( "[controller]/{deckCode}/info" ) ]
		public async Task<IActionResult> Info( string deckCode )
		{
			if ( string.IsNullOrWhiteSpace( deckCode ) || deckCode.Length != 5 )
				return BadRequest( "Missing or invaild deck code" );

			try
			{
				var deck = await Cache.GetDeckAsync( deckCode );
				var deckInfo = new {
					name           = deck.Name,
					code           = deck.Code,
					description    = deck.Description,
					calls          = deck.CallCount,
					responses      = deck.ResponseCount,
					callSheets     = deck.CallCount / RenderService.CardsPerSheet + 1,
					responseSheets = deck.ResponseCount / RenderService.CardsPerSheet + 1,
				};

				return Json( deckInfo );
			}
			catch ( HttpRequestException ex ) when ( ex.Message.Contains( "Not Found" ) )
			{
				return NotFound( $"Deck \"{deckCode}\" does not exist on Card Cast" );
			}
		}
	}
}