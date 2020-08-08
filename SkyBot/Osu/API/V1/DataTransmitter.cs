using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.API.V1
{
    public class DataTransmitter<T>
    {
        public T Value { get; set; }

        public DataTransmitter(T value)
        {
            Value = value;
        }

        public DataTransmitter()
        {

        }
    }
}
