using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardCastToImage.Models;
using CardCastToImage.Services;
using CardCastToImage.Web.Utility;

namespace CardCastToImage.Web.Services
{
	public static class Cache
	{
		private static readonly TimeSpan                         CacheValiditySpan  = TimeSpan.FromMinutes( 5 );
		private static readonly AsyncCache<string, CardcastDeck> DeckCache          = new AsyncCache<string, CardcastDeck>( deckCode => CardCastService.GetDeckAsync( deckCode ), CacheValiditySpan );
		private static readonly AsyncCache<string, List<byte[]>> CallCardsCache     = new AsyncCache<string, List<byte[]>>( CreateCallCards, CacheValiditySpan );
		private static readonly AsyncCache<string, List<byte[]>> ResponseCardsCache = new AsyncCache<string, List<byte[]>>( CreateResponseCards, CacheValiditySpan );

		private static async Task<List<byte[]>> CreateCallCards( string deckCode )
		{
			var deck         = await DeckCache.GetItemAsync( deckCode );
			var cardSheets   = RenderService.RenderCardSheets( deck.Cards.Calls, deckCode, Color.Black, Color.White );
			var sheetBuffers = new List<byte[]>();

			Debug.WriteLine( $"Creating call cards for deck {deckCode}" );

			foreach ( var sheet in cardSheets )
			{
				await using var sheetStream = new MemoryStream();

				sheet.Save( sheetStream, ImageFormat.Png );
				sheetBuffers.Add( sheetStream.ToArray() );
			}

			return sheetBuffers;
		}

		private static async Task<List<byte[]>> CreateResponseCards( string deckCode )
		{
			var deck         = await DeckCache.GetItemAsync( deckCode );
			var cardSheets   = RenderService.RenderCardSheets( deck.Cards.Responses, deckCode, Color.White, Color.Black );
			var sheetBuffers = new List<byte[]>();

			Debug.WriteLine( $"Creating response cards for deck {deckCode}" );

			foreach ( var sheet in cardSheets )
			{
				await using var sheetStream = new MemoryStream();

				sheet.Save( sheetStream, ImageFormat.Png );
				sheetBuffers.Add( sheetStream.ToArray() );
			}

			return sheetBuffers;
		}

		public static Task<CardcastDeck> GetDeckAsync( string deckCode ) => DeckCache.GetItemAsync( deckCode );
		public static Task<List<byte[]>> GetCallCardsAsync( string deckCode ) => CallCardsCache.GetItemAsync( deckCode );
		public static Task<List<byte[]>> GetResponseCardsAsync( string deckCode ) => ResponseCardsCache.GetItemAsync( deckCode );
	}
}