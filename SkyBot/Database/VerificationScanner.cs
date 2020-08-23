using Microsoft.EntityFrameworkCore;
using SkyBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyBot.Database
{
    public class VerificationScanner : DBScanner<Verification>
    {
        public VerificationScanner() : base(TimeSpan.FromHours(3))
        {

        }

        public override void Start()
        {
            base.Start();

            Task.Run(() => TryScan());
        }

        protected override void OnScan(DbSet<Verification> set, DbContext c)
        {
            lock(SyncRoot)
            {
                List<Verification> invalidVerifications = set.Where(v => v.InvalidatesAt <= DateTime.UtcNow).ToList();
                set.RemoveRange(invalidVerifications);
                c.SaveChanges();
            }
        }
    }
}
