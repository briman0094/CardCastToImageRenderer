using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using CardCastToImage.Extensions;
using CardCastToImage.Services;

namespace CardCastToImage
{
	class Program
	{
		private static async Task Main( string[] args )
		{
			if ( args.Length < 1 )
			{
				Console.WriteLine( "Usage: CardCastToImage.exe <deck codes>" );
				return;
			}

			var deckCodes = args[ 0 ].Split( ',' );

			// Render the card backs (calls then responses)
			using ( var callCardBack = RenderService.RenderCardBack( Color.Black, Color.White ) )
				callCardBack.Save( "CallCardBack.png", ImageFormat.Png );

			using ( var responseCardBack = RenderService.RenderCardBack( Color.White, Color.Black ) )
				responseCardBack.Save( "ResponseCardBack.png", ImageFormat.Png );

			// Fetch and render all the individual decks
			foreach ( var deckCode in deckCodes )
			{
				try
				{
					Console.Write( $"Attempting to fetch card deck \"{deckCode}\"..." );

					var deck = await CardCastService.GetDeckAsync( deckCode );

					Console.WriteLine( "Done!" );
					Console.Write( "Rendering..." );

					// Render the card fronts (calls then responses)
					var callCardSheets     = RenderService.RenderCardSheet( deck.Calls, deckCode, Color.Black, Color.White );
					var responseCardSheets = RenderService.RenderCardSheet( deck.Responses, deckCode, Color.White, Color.Black );

					foreach ( var (sheetIndex, sheetBitmap) in callCardSheets.Pairs() )
					{
						using ( sheetBitmap )
							sheetBitmap.Save( $"DeckCalls-{deckCode}-{sheetIndex}.png", ImageFormat.Png );
					}

					foreach ( var (sheetIndex, sheetBitmap) in responseCardSheets.Pairs() )
					{
						using ( sheetBitmap )
							sheetBitmap.Save( $"DeckResponses-{deckCode}-{sheetIndex}.png", ImageFormat.Png );
					}

					Console.WriteLine( "Done!" );

					// Don't want to piss off the server timeouts
					Thread.Sleep( 1000 );
				}
				catch ( Exception ex )
				{
					Console.WriteLine( "Error!" );
					Console.WriteLine( ex.ToString() );
				}
			}

			Console.WriteLine( "Done with all decks! Press any key to exit." );

			while ( Console.KeyAvailable ) Console.ReadKey( true );

			Console.ReadKey( true );
		}
	}
}