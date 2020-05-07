using System;
using System.Linq;

namespace CardCastToImage.Models
{
	public class CardcastCard
	{
		public const string CardPrompt = "______";

		public string         Id        { get; set; }
		public string[]       Text      { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public bool           Nsfw      { get; set; }

		public string GetFullCardText()
		{
			if ( this.Text == null || this.Text.Length == 0 ) return string.Empty;
			if ( this.Text.Length == 1 ) return this.Text.First();

			return string.Join( CardPrompt, this.Text );
		}
	}
}