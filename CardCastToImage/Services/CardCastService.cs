using System.Net.Http;
using System.Threading.Tasks;
using CardCastToImage.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CardCastToImage.Services
{
	public static class CardCastService
	{
		private const string BaseUrl = "https://api.cardcastgame.com/v1";

		private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings {
			ContractResolver = new DefaultContractResolver {
				NamingStrategy = new SnakeCaseNamingStrategy(),
			},
		};

		public static async Task<CardcastDeck> GetDeckAsync( string deckCode, bool includeCards = true )
		{
			using var httpClient = new HttpClient();
			using var response   = await httpClient.GetAsync( $"{BaseUrl}/decks/{deckCode}" );

			response.EnsureSuccessStatusCode();

			var deckJson = await response.Content.ReadAsStringAsync();
			var deck     = JsonConvert.DeserializeObject<CardcastDeck>( deckJson, JsonSettings );

			if ( deck == default )
				return default;

			if ( includeCards )
				deck.Cards = await GetDeckCardsAsync( deckCode );

			return deck;
		}

		public static async Task<CardcastDeckCards> GetDeckCardsAsync( string deckCode )
		{
			using var httpClient = new HttpClient();
			using var response   = await httpClient.GetAsync( $"{BaseUrl}/decks/{deckCode}/cards" );

			response.EnsureSuccessStatusCode();

			var deckJson = await response.Content.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<CardcastDeckCards>( deckJson, JsonSettings );
		}
	}
}