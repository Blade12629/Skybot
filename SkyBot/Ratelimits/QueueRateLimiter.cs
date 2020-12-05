using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace SkyBot.Ratelimits
{
    public class QueueRateLimiter : BaseRateLimiter
    {
        private Queue<(Action, Action<object>, object)> _queue;

        public QueueRateLimiter(int start, int max, TimeSpan resetTime) : base(start, max, resetTime)
        {
            _queue = new Queue<(Action, Action<object>, object)>();
        }

        protected override void OnTick(object sender, ElapsedEventArgs e)
        {
            base.OnTick(sender, e);

            lock (SyncRoot)
            {
                int count = _queue.Count;

                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        if (_current > _max)
                        {
                            _queue.Enqueue(_queue.Dequeue());
                            continue;
                        }

                        _current++;

                        (Action, Action<object>, object) nextTick = _queue.Dequeue();

                        try
                        {
                            nextTick.Item1.Invoke();
                        }
                        catch (Exception ex)
                        {
                            Logger.Log("Failed to invoke next tick of QueueRateLimit: " + ex, LogLevel.Error);
                        }
                        finally
                        {
                            nextTick.Item2?.Invoke(nextTick.Item3);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex, LogLevel.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Increment ratelimit and enqueue action if we are at the limit or invoke action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueAction">Action to invoke</param>
        /// <param name="confirmAction">Action to confirm token once invoked(Required for async callback)</param>
        /// <param name="token">Token with confirm status (Required for async callback)</param>
        /// <returns>False - Enqueued token, True - Invoked action</returns>
        public bool Increment<T>(Action queueAction, Action<object> confirmAction = null, T token = default)
        {
            if (Increment())
            {
                queueAction?.Invoke();
                return true;
            }

            lock (SyncRoot)
            {
                _queue.Enqueue((queueAction, confirmAction, token));
            }

            return false;
        }
    }
}
