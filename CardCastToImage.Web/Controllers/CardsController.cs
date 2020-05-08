using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CardCastToImage.Models;
using CardCastToImage.Services;
using CardCastToImage.Web.Utility;
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

		#region Caches

		private static readonly AsyncCache<string, CardcastDeck> DeckCache          = new AsyncCache<string, CardcastDeck>( CardCastService.GetDeckAsync );
		private static readonly AsyncCache<string, List<Bitmap>> CallCardsCache     = new AsyncCache<string, List<Bitmap>>( CreateCallCards );
		private static readonly AsyncCache<string, List<Bitmap>> ResponseCardsCache = new AsyncCache<string, List<Bitmap>>( CreateResponseCards );

		private static async Task<List<Bitmap>> CreateCallCards( string deckCode )
		{
			var deck       = await DeckCache.GetItemAsync( deckCode );
			var cardSheets = RenderService.RenderCardSheets( deck.Calls, deckCode, Color.Black, Color.White );

			return cardSheets.ToList();
		}

		private static async Task<List<Bitmap>> CreateResponseCards( string deckCode )
		{
			var deck       = await DeckCache.GetItemAsync( deckCode );
			var cardSheets = RenderService.RenderCardSheets( deck.Responses, deckCode, Color.White, Color.Black );

			return cardSheets.ToList();
		}

		#endregion

		[ Route( "[controller]/[action]/{type}" ) ]
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

			cardBitmap.Save( cardStream, ImageFormat.Png );
			cardStream.Seek( 0, SeekOrigin.Begin );

			return File( cardStream, "image/png" );
		}

		[ Route( "[controller]/{deckCode}/[action]/{type}/{sheet?}" ) ]
		public async Task<IActionResult> Front( string deckCode, CardType? type, int? sheet )
		{
			if ( string.IsNullOrWhiteSpace( deckCode ) || deckCode.Length != 5 )
				return BadRequest( "Missing or invalid deck code" );
			if ( type == default )
				return BadRequest( "Unknown card type" );

			try
			{
				var sheets = type switch {
					CardType.Call     => await CallCardsCache.GetItemAsync( deckCode ),
					CardType.Response => await ResponseCardsCache.GetItemAsync( deckCode ),
					_                 => throw new InvalidOperationException(),
				};

				if ( sheet != default )
				{
					if ( sheet <= 0 )
						return BadRequest( "Sheet number must be greater than zero" );
					if ( sheet > sheets.Count )
						return BadRequest( $"Deck \"{deckCode}\" only has {sheets.Count} card sheets" );
				}

				var sheetBitmap = sheets[ ( sheet ?? 1 ) - 1 ];
				var sheetStream = new MemoryStream();

				sheetBitmap.Save( sheetStream, ImageFormat.Png );
				sheetStream.Seek( 0, SeekOrigin.Begin );

				return File( sheetStream, "image/png" );
			}
			catch ( HttpRequestException ex ) when ( ex.Message.Contains( "Not Found" ) )
			{
				return NotFound( $"Deck \"{deckCode}\" does not exist on Card Cast" );
			}
		}
	}
}