using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardCastToImage.Web.HostedServices;

namespace CardCastToImage.Web.Utility
{
	public class AsyncCache<TKey, TItem> : IDisposable
	{
		private readonly Dictionary<TKey, TItem>                       items                 = new Dictionary<TKey, TItem>();
		private readonly Dictionary<TKey, DateTime>                    itemExpiries          = new Dictionary<TKey, DateTime>();
		private readonly Dictionary<TKey, TaskCompletionSource<TItem>> itemCompletionSources = new Dictionary<TKey, TaskCompletionSource<TItem>>();

		public AsyncCache( Func<TKey, Task<TItem>> itemFactory, TimeSpan? expiry = default )
		{
			this.ItemFactory = itemFactory ?? throw new ArgumentNullException( nameof(itemFactory) );
			this.Expiry      = expiry ?? TimeSpan.Zero;

			PeriodicService.Elapsed += this.RemoveExpiredCacheEntries;
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
			bool                        foundExistingCompletionSource = false;
			TaskCompletionSource<TItem> completionSource              = default;

			lock ( this.itemCompletionSources )
			{
				if ( this.itemCompletionSources.ContainsKey( key ) )
				{
					// We found an existing completion source
					foundExistingCompletionSource = true;
					completionSource              = this.itemCompletionSources[ key ];
				}
				else
				{
					// If not, this is the first request for this item (at least since it last expired)
					completionSource = new TaskCompletionSource<TItem>();

					this.itemCompletionSources.Add( key, completionSource );
				}
			}

			// If we can, just await the existing completion source's task. Otherwise fall through and create
			// the item and eventually resolve the completion source
			if ( foundExistingCompletionSource )
				return await completionSource.Task;

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

		private void RemoveExpiredCacheEntries()
		{
			List<TKey> expiredKeys;

			lock ( this.items )
			{
				expiredKeys = this.items.Keys.Where( key => this.itemExpiries.ContainsKey( key ) && this.itemExpiries[ key ] < DateTime.UtcNow ).ToList();
			}

			foreach ( var key in expiredKeys )
			{
				lock ( this.items )
				{
					this.items.Remove( key );
					this.itemExpiries.Remove( key );
				}

				lock ( this.itemCompletionSources )
				{
					if ( this.itemCompletionSources.ContainsKey( key ) )
					{
						this.itemCompletionSources.Remove( key, out var completionSource );

						completionSource?.TrySetCanceled();
					}
				}
			}
		}

		public void Dispose()
		{
			PeriodicService.Elapsed -= this.RemoveExpiredCacheEntries;
		}
	}
}