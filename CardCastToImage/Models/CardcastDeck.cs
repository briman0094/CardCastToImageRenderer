using System.Collections.Generic;

namespace CardCastToImage.Models
{
	public class CardcastDeck
	{
		public List<CardcastCard> Calls     { get; set; }
		public List<CardcastCard> Responses { get; set; }
	}
}