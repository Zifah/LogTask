namespace Shared.Helpers;

public static class Require
{
    public static void NotNull<T>(T value, string paramName)
    {
        if (value == null)
        {
            throw new ArgumentNullException(paramName);
        }
    }
}