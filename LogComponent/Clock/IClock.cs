using System;

namespace LogComponent.Clock;

public interface IClock
{
    DateTime CurrentTime { get; }
}