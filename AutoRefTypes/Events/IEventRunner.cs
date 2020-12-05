using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes.Events
{
    public interface IEventRunner
    {
        public void Register(EventObject @obj);
        public void Delete(EventObject @obj);
    }
}
