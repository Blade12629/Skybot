using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace SkyBot.Ratelimits
{
    /// <summary>
    /// Threadsafe rate limiter
    /// </summary>
    public class BaseRateLimiter : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public object SyncRoot { get; }
        public int Current
        {
            get
            {
                return _current;
            }
            set
            {
                lock(SyncRoot)
                {
                    _current = value;
                }
            }
        }

        public int Max
        {
            get
            {
                return _max;
            }
            set
            {
                lock(SyncRoot)
                {
                    _max = value;
                }
            }
        }

        protected int _current;
        protected int _max;
        private Timer _rateTimer;

        /// <summary>
        /// Creates and starts a rate limiter
        /// </summary>
        /// <param name="start">Initial counter value</param>
        /// <param name="max">Max counter value</param>
        /// <param name="resetTime">Time to reset counter value</param>
        public BaseRateLimiter(int start, int max, TimeSpan resetTime)
        {
            SyncRoot = new object();

            lock (SyncRoot)
            {
                start = Math.Max(start, 0);
                _max = max;

                if (resetTime == TimeSpan.Zero)
                    throw new ArgumentException("Time cannot be zero", nameof(resetTime));

                _rateTimer = new Timer()
                {
                    Interval = resetTime.TotalMilliseconds,
                    AutoReset = true
                };
                _rateTimer.Elapsed += OnTick;

                _rateTimer.Start();
            }
        }

        ~BaseRateLimiter()
        {
            Dispose();
        }

        protected virtual void OnTick(object sender, ElapsedEventArgs e)
        {
            Reset();
        }

        /// <summary>
        /// Resets the counter
        /// </summary>
        public void Reset()
        {
            Current = 0;
        }

        /// <summary>
        /// Increments the counter
        /// </summary>
        /// <returns>True - Available, False - Unavailable</returns>
        public virtual bool Increment()
        {
            if (Current >= Max)
                return false;

            Current++;
            return true;
        }

        public void Dispose()
        {
            lock(SyncRoot)
            {
                if (IsDisposed)
                    return;

                try
                {
                    _rateTimer?.Stop();
                    _rateTimer?.Dispose();
                }
                catch (Exception)
                {

                }

                _max = 0;
                _current = 0;

                GC.Collect();
                GC.SuppressFinalize(this);

                IsDisposed = true;
            }
        }
    }
}
