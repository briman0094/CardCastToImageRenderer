using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using CardCastToImage.Extensions;
using CardCastToImage.Models;

namespace CardCastToImage.Services
{
	public static class RenderService
	{
		private const string CardBackText    = "TOTALLY NOT CARDS AGAINST HUMANITY";
		private const string CardBackSubText = "ARCANOX";

		private const int CardColumns = 10;
		private const int CardRows    = 7;

		private const int CardWidth  = 600;
		private const int CardHeight = 840; //2.5" x 3.5" = 1.4 aspect ratio

		private const int LeftMargin   = (int) ( CardWidth * 0.1 );
		private const int RightMargin  = (int) ( CardWidth * 0.1 );
		private const int TopMargin    = (int) ( CardHeight * 0.1 );
		private const int BottomMargin = (int) ( CardHeight * 0.1 );

		private const int MainTextFontSize = 46;
		private const int SubTextFontSize  = 20;

		private static readonly Size CardSize = new Size( CardWidth, CardHeight );

		private static Bitmap CreateSheetBitmap() => new Bitmap( CardWidth * CardColumns, CardHeight * CardRows );

		public static IEnumerable<Bitmap> RenderCardSheets( IEnumerable<CardcastCard> cards, string deckCode, Color background, Color foreground )
		{
			var bitmap       = CreateSheetBitmap();
			var currentSheet = 0;
			var graphics     = Graphics.FromImage( bitmap );

			try
			{
				using var bgBrush = new SolidBrush( background );

				foreach ( var (i, card) in cards.Pairs() )
				{
					var row = ( i / CardColumns ) - ( currentSheet * CardRows );
					var col = i % CardColumns;

					if ( row >= CardRows )
					{
						// Yield the current sheet and start a new one

						yield return bitmap;

						graphics.Dispose();
						bitmap   = CreateSheetBitmap();
						graphics = Graphics.FromImage( bitmap );
						currentSheet++;
						row -= CardRows;
					}

					var cardOrigin = new Point( col * CardWidth, row * CardHeight );
					var cardBounds = new Rectangle( cardOrigin, CardSize );

					graphics.SetClip( cardBounds );
					graphics.Clear( background );
					graphics.TranslateTransform( cardOrigin.X, cardOrigin.Y );
					RenderCardText( graphics, foreground, card.GetFullCardText(), deckCode );
					graphics.ResetTransform();
				}

				// Yield the final sheet
				yield return bitmap;
			}
			finally
			{
				graphics.Dispose();
			}
		}

		public static Bitmap RenderCardBack( Color background, Color foreground )
		{
			// Make main text have line breaks
			var mainText = CardBackText.Replace( " ", Environment.NewLine );
			var bitmap   = new Bitmap( CardWidth, CardHeight );

			using var graphics = Graphics.FromImage( bitmap );

			graphics.Clear( background );
			RenderCardText( graphics, foreground, mainText, CardBackSubText );

			return bitmap;
		}

		private static void RenderCardText( Graphics graphics, Color foreground, string mainText, string subText )
		{
			using var mainFormat = new StringFormat( StringFormatFlags.NoWrap, CultureInfo.CurrentCulture.LCID ) { Trimming  = StringTrimming.EllipsisWord };
			using var subFormat  = new StringFormat( StringFormatFlags.NoWrap, CultureInfo.CurrentCulture.LCID ) { Alignment = StringAlignment.Far };
			using var mainFont   = new Font( "Arial", MainTextFontSize, FontStyle.Bold );
			using var subFont    = new Font( "Arial", SubTextFontSize, FontStyle.Bold );
			using var textBrush  = new SolidBrush( foreground );
			using var textPen    = new Pen( textBrush ) { Width = 1 };

			graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
			graphics.SmoothingMode     = SmoothingMode.AntiAlias;
			graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

			// Measure subText first, then fit main text to card above it
			var subTextSize = graphics.MeasureString( subText, subFont );
			var subTextOrigin = new PointF(
				CardWidth - RightMargin,
				CardHeight - BottomMargin - subTextSize.Height );
			var linePos = TopMargin;

			while ( mainText.Length > 0 )
			{
				var lineOrigin = new Point( LeftMargin, linePos );
				var mainTextBounds = new SizeF(
					CardWidth - ( LeftMargin + RightMargin ),
					CardHeight - ( TopMargin + BottomMargin + subTextSize.Height ) );

				graphics.MeasureString( mainText, mainFont, mainTextBounds, mainFormat, out var charsFitted, out _ );

				var lineText = mainText.Substring( 0, charsFitted );

				if ( charsFitted != mainText.Length && !Regex.IsMatch( lineText, @"[\s-\.]$" ) )
					lineText += "-";

				graphics.DrawString( lineText, mainFont, textBrush, lineOrigin, mainFormat );
				mainText =  mainText.Substring( charsFitted, mainText.Length - charsFitted );
				linePos  += (int) mainFont.GetHeight( graphics );
			}

			graphics.DrawString( subText, subFont, textBrush, subTextOrigin, subFormat );
		}
	}
}