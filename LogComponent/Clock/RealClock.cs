using System;

namespace LogComponent.Clock;

public class RealClock : IClock
{
    public DateTime CurrentTime => DateTime.Now;
}