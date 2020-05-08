using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CardCastToImage.Web.Utility
{
	public class AsyncCache<TKey, TItem>
	{
		private readonly Dictionary<TKey, TItem>                       items                 = new Dictionary<TKey, TItem>();
		private readonly Dictionary<TKey, DateTime>                    itemExpiries          = new Dictionary<TKey, DateTime>();
		private readonly Dictionary<TKey, TaskCompletionSource<TItem>> itemCompletionSources = new Dictionary<TKey, TaskCompletionSource<TItem>>();

		public AsyncCache( Func<TKey, Task<TItem>> itemFactory, TimeSpan? expiry = default )
		{
			this.ItemFactory = itemFactory ?? throw new ArgumentNullException( nameof(itemFactory) );
			this.Expiry      = expiry ?? TimeSpan.Zero;
		}

		public    TimeSpan                Expiry      { get; set; }
		protected Func<TKey, Task<TItem>> ItemFactory { get; }

		public async Task<TItem> GetItemAsync( TKey key )
		{
			// If we have the item, just return it
			lock ( this.items )
			{
				if ( this.items.ContainsKey( key ) )
				{
					if ( this.itemExpiries.ContainsKey( key ) )
					{
						// Check if expired

						var expiry = this.itemExpiries[ key ];

						if ( expiry > DateTime.UtcNow )
							return this.items[ key ];

						// If we got here, it was expired. Intentionally fall through below to re-create it.
					}
					else
					{
						// No expiry set; just return the item

						return this.items[ key ];
					}
				}
			}

			// If we don't have the item, see if we have a pending completion source for it
			Task<TItem> itemTask = default;

			lock ( this.itemCompletionSources )
			{
				if ( this.itemCompletionSources.ContainsKey( key ) )
					itemTask = this.itemCompletionSources[ key ].Task;
			}

			// If we did, await and return that completion source's task
			if ( itemTask != default )
				return await itemTask;

			// If not, this is the first request for this item (at least since it last expired) so let's 
			var completionSource = new TaskCompletionSource<TItem>();

			lock ( this.itemCompletionSources )
			{
				this.itemCompletionSources.Add( key, completionSource );
			}

			try
			{
				var result = await this.ItemFactory( key );

				lock ( this.items )
				{
					if ( this.items.ContainsKey( key ) )
						this.items.Remove( key );
					if ( this.itemExpiries.ContainsKey( key ) )
						this.itemExpiries.Remove( key );

					this.items.Add( key, result );

					if ( this.Expiry != TimeSpan.Zero )
						this.itemExpiries.Add( key, DateTime.UtcNow + this.Expiry );
				}

				completionSource.TrySetResult( result );

				return result;
			}
			catch ( Exception ex )
			{
				completionSource.TrySetException( ex );
				throw;
			}
			finally
			{
				lock ( this.itemCompletionSources )
				{
					this.itemCompletionSources.Remove( key );
				}
			}
		}
	}
}