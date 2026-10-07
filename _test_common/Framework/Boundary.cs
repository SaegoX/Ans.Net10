namespace _test_common.Framework;

/// <summary>Шпаргалка пограничных значений — чтобы не выдумывать их каждый раз.</summary>
public static class Boundary
{
    public static object?[] Strings() => new object?[]
    {
        null, "", " ", "a", "  a  ", "\t\n",
        "привет", "🎉", new string('x', 1000)
    };

    public static int[] Ints() => new[]
    {
        0, 1, -1, int.MinValue, int.MaxValue, int.MinValue + 1, int.MaxValue - 1
    };

    public static double[] Doubles() => new[]
    {
        0.0, -0.0, double.NaN,
        double.PositiveInfinity, double.NegativeInfinity,
        double.Epsilon, double.MaxValue
    };

    public static DateTime[] DateTimes() => new[]
    {
        DateTime.MinValue, DateTime.MaxValue,
        DateTime.Now, DateTime.UtcNow, default
    };
}