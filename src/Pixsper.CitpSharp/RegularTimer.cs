using System;
using System.Diagnostics;
using System.Threading;

namespace Pixsper.CitpSharp
{
	internal sealed class RegularTimer : IDisposable
	{
		private bool _isDisposed;

		private readonly int _intervalMs;
		private readonly Stopwatch _stopwatch = new Stopwatch();

		private readonly object _lock = new object();
		private Timer? _timer;

		public RegularTimer(TimeSpan interval)
		{
			_intervalMs = (int)Math.Round(interval.TotalMilliseconds, MidpointRounding.AwayFromZero);
		}

		public RegularTimer(int intervalMs)
		{
			_intervalMs = intervalMs;
		}

		public void Dispose()
		{
			if (_isDisposed)
				return;

			lock (_lock)
			{
				if (_timer != null)
				{
					_timer.Dispose();
					_timer = null;
				}
			}

			_isDisposed = true;
		}

		public void Start()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(RegularTimer));

			lock (_lock)
			{
				if (_timer != null)
					throw new InvalidOperationException("Timer is already running");

				lock (_lock)
					_timer = new Timer(onTimerElapsed, null, _intervalMs, Timeout.Infinite);
			}
		}

		public void Stop()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(RegularTimer));

			lock (_lock)
			{
				if (_timer == null)
					throw new InvalidOperationException("Timer is already stopped");

				_timer.Dispose();
				_timer = null;
			}
		}

		public event EventHandler? Elapsed;

		private void onTimerElapsed(object state)
		{
			if (_isDisposed)
				return;

			_stopwatch.Restart();
			Elapsed?.Invoke(this, EventArgs.Empty);

			int processTime = (int)_stopwatch.ElapsedMilliseconds;

			lock(_lock)
				_timer?.Change(Math.Max(_intervalMs - processTime, 0), Timeout.Infinite);
		}
	}
}
