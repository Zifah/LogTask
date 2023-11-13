using System;

namespace LogComponent.Helpers;

public static class Require
{
    internal static void NotNull<T>(T value, string paramName)
    {
        if (value == null)
        {
            throw new ArgumentNullException(paramName);
        }
    }
}