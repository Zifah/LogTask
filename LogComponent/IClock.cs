using System;

namespace LogComponent
{
    public interface IClock
    {
        DateTime CurrentTime { get; }
    }
}