using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace SkyBot.Database
{
    public abstract class DBScanner<SET> where SET : class
    {
        public object SyncRoot { get; }

        private TimeSpan _delay;
        private Timer _timer;

        public DBScanner(TimeSpan scanDelay)
        {
            SyncRoot = new object();
            _delay = scanDelay;
        }

        public void Start()
        {
            lock(SyncRoot)
            {
                _timer = new Timer(_delay.TotalMilliseconds)
                {
                    AutoReset = true
                };

                _timer.Elapsed += OnScanTick;
                _timer.Start();
            }
        }

        private void OnScanTick(object sender, ElapsedEventArgs e)
        {
            using DBContext c = new DBContext();
            DbSet<SET> set = c.Set<SET>();

            OnScan(set, c);
        }

        protected virtual void OnScan(DbSet<SET> set, DbContext c)
        {

        }

        public void Stop()
        {
            lock (SyncRoot)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}
