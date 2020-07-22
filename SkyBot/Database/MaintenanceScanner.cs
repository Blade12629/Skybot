using Microsoft.EntityFrameworkCore;
using SkyBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyBot.Database
{
    public sealed class MaintenanceScanner : DBScanner<ByteTable>
    {
        public bool IsMaintenance { get; private set; }
        public string MaintenanceMessage { get; private set; }
        public event EventHandler<(bool, string)> OnMaintenanceChanged;

        private const string _TABLE_KEY = "Maintenance";

        public MaintenanceScanner(TimeSpan scanDelay) : base(scanDelay)
        {

        }

        protected override void OnScan(DbSet<ByteTable> set, DbContext c)
        {
            lock(SyncRoot)
            {
                ByteTable bt = set.FirstOrDefault(bt => bt.Identifier.Equals(_TABLE_KEY));

                bool newStatus;
                string newMessage;
                if (bt == null)
                {
                    newStatus = false;
                    newMessage = "";
                }
                else
                {
                    newStatus = bt.Data[0] == 1;
                    newMessage = Encoding.UTF8.GetString(bt.Data, 1, bt.Data.Length - 1);
                }

                if (IsMaintenance == newStatus && MaintenanceMessage.Equals(newMessage))
                    return;

                IsMaintenance = newStatus;
                MaintenanceMessage = newMessage;

                OnMaintenanceChanged?.Invoke(this, (newStatus, newMessage));
            }
        }

        public void SetMaintenanceStatus(bool status, string message)
        {
            lock(SyncRoot)
            {
                using DBContext c = new DBContext();
                ByteTable bt = c.ByteTable.FirstOrDefault(bt => bt.Identifier.Equals(_TABLE_KEY));

                List<byte> data = new List<byte>()
                {
                    status ? (byte)1 : (byte)0
                };

                data.AddRange(Encoding.UTF8.GetBytes(message));


                if (bt == null)
                {
                    bt = new ByteTable(_TABLE_KEY, data.ToArray());

                    c.ByteTable.Add(bt);
                }
                else
                {
                    bt.Data = data.ToArray();
                    c.ByteTable.Update(bt);
                }

                c.SaveChanges();
            }
        }
    }
}
