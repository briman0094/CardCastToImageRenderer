using System;

namespace CardCastToImage.Models
{
	public class CardcastDeck
	{
		public string            Name               { get; set; }
		public string            Code               { get; set; }
		public string            Description        { get; set; }
		public bool              Unlisted           { get; set; }
		public DateTimeOffset?   CreatedAt          { get; set; }
		public DateTimeOffset?   UpdatedAt          { get; set; }
		public bool              ExternalCopyright  { get; set; }
		public string            CopyrightHolderUrl { get; set; }
		public string            Category           { get; set; }
		public int               CallCount          { get; set; }
		public int               ResponseCount      { get; set; }
		public CardcastUser      Author             { get; set; }
		public decimal           Rating             { get; set; }
		public CardcastDeckCards Cards              { get; set; }
	}
}