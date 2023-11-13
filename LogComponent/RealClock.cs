using System;

namespace LogComponent
{
    public class RealClock : IClock
    {
        public DateTime CurrentTime => DateTime.Now;
    }
}