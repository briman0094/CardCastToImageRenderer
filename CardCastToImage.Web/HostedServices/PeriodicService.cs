using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace CardCastToImage.Web.HostedServices
{
	public class PeriodicService : IHostedService, IAsyncDisposable
	{
		private const int TimerMinutes = 5;

		public static event Action Elapsed;

		private Timer timer;

		public Task StartAsync( CancellationToken cancellationToken )
		{
			if ( this.timer != default ) return Task.CompletedTask;

			var timerSpan = TimeSpan.FromMinutes( TimerMinutes );

			this.timer = new Timer( o => Elapsed?.Invoke(), null, timerSpan, timerSpan );

			return Task.CompletedTask;
		}

		public Task StopAsync( CancellationToken cancellationToken )
		{
			this.timer?.Change( Timeout.Infinite, Timeout.Infinite );

			return Task.CompletedTask;
		}

		public async ValueTask DisposeAsync()
		{
			await this.timer.DisposeAsync();
		}
	}
}