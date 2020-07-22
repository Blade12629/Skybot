using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Ratelimits
{
    public class QueueConfirmationToken<T>
    {
        public T Value { get; set; }

        public QueueConfirmationToken(T @default)
        {
            Value = @default;
        }
    }
}
