using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CardCastToImage.Services;
using CardCastToImage.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CardCastToImage.Web.Controllers
{
	public enum CardType
	{
		Call,
		Response,
	}

	[ SuppressMessage( "ReSharper", "ArrangeThisQualifier" ) ]
	public class CardsController : Controller
	{
		private static Bitmap CallCardBack;
		private static Bitmap ResponseCardBack;

		[ Route( "[controller]/{type}/[action]" ) ]
		[ ResponseCache( Duration = 3600, Location = ResponseCacheLocation.Any ) ]
		public IActionResult Back( CardType? type )
		{
			if ( type == default )
				return BadRequest( "Unknown card type" );

			CallCardBack     ??= RenderService.RenderCardBack( Color.Black, Color.White );
			ResponseCardBack ??= RenderService.RenderCardBack( Color.White, Color.Black );

			var cardBitmap = type switch {
				CardType.Call     => CallCardBack,
				CardType.Response => ResponseCardBack,
				_                 => throw new InvalidOperationException(),
			};
			var cardStream = new MemoryStream();

			lock ( cardBitmap )
			{
				cardBitmap.Save( cardStream, ImageFormat.Png );
				cardStream.Seek( 0, SeekOrigin.Begin );
			}

			return File( cardStream, "image/png" );
		}

		[ Route( "[controller]/{deckCode}/{type}/[action]/{sheet?}" ) ]
		[ ResponseCache( Duration = 3600, Location = ResponseCacheLocation.Any ) ]
		public async Task<IActionResult> Front( string deckCode, CardType? type, int? sheet )
		{
			if ( string.IsNullOrWhiteSpace( deckCode ) || deckCode.Length != 5 )
				return BadRequest( "Missing or invalid deck code" );
			if ( type == default )
				return BadRequest( "Unknown card type" );

			try
			{
				var sheets = type switch {
					CardType.Call     => await Cache.GetCallCardsAsync( deckCode ),
					CardType.Response => await Cache.GetResponseCardsAsync( deckCode ),
					_                 => throw new InvalidOperationException(),
				};

				if ( sheet != default )
				{
					if ( sheet <= 0 )
						return BadRequest( "Sheet number must be greater than zero" );
					if ( sheet > sheets.Count )
						return BadRequest( $"Deck \"{deckCode}\" only has {sheets.Count} card sheets" );
				}

				var sheetBuffer = sheets[ ( sheet ?? 1 ) - 1 ];

				return File( sheetBuffer, "image/png" );
			}
			catch ( HttpRequestException ex ) when ( ex.Message.Contains( "Not Found" ) )
			{
				return NotFound( $"Deck \"{deckCode}\" does not exist on Card Cast" );
			}
		}
	}
}