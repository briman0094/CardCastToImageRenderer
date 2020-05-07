using System.Collections.Generic;

namespace CardCastToImage.Extensions
{
	public static class EnumerableExtensions
	{
		public static IEnumerable<(int index, T item)> Pairs<T>( this IEnumerable<T> source )
		{
			var i = 0;

			foreach ( var item in source )
				yield return ( i++, item );
		}
	}
}