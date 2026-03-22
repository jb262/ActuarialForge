namespace ActuarialForge.Utils;

/// <summary>
/// Static math class for exponential operations on decimal variables.
/// </summary>
public static class DecimalMath
{
    private static readonly decimal _epsilon = 1e-28m;
    private static readonly int _maxIterations = 100;

    public static decimal DecimalPow(decimal x, int power)
    {
        //Simplification: Zero to the power of zero is assumed to be one by definition.
        if (power == decimal.Zero) return decimal.One;

        if (power < 0) return decimal.One / DecimalPow(x, -power);

        return x * DecimalPow(x, power - 1);
    }

    public static decimal DecimalPow(decimal x, decimal power)
    {
        if (power < decimal.Zero)
            return decimal.One / DecimalPow(x, -power);

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(x, decimal.Zero);

        if (power % 1 == 0)
            return DecimalPow(x, (int)power);

        return Exp(power * Ln(x));
    }

    public static decimal Ln(decimal x)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(x, decimal.Zero);

        decimal ln = (decimal)Math.Log((double)x);

        for (int i = 0; i < _maxIterations; i++)
        {
            decimal eToGuess = Exp(ln);
            decimal next = ln - decimal.One + x / eToGuess;

            if (Math.Abs(next - ln) < _epsilon)
                return next;

            ln = next;
        }

        return ln;
    }

    public static decimal Exp(decimal x)
    {
        decimal sum = decimal.One;
        decimal term = decimal.One;

        for (int i = 1; i <= _maxIterations; i++)
        {
            term *= x / i;
            sum += term;

            if (Math.Abs(term) < _epsilon) break;
        }

        return sum;
    }

    public static decimal Sqrt(decimal x)
    {
        if (x < 0)
            throw new ArgumentOutOfRangeException(nameof(x), "Cannot calculate the square root of a negative number.");

        if (x == decimal.Zero)
            return decimal.Zero;

        decimal current = (decimal)Math.Sqrt((double)x); //Initial value, built in double square root is a very good first and fast guess.
        decimal previous;
        int iterations = 0;

        do
        {
            previous = current;
            current = (previous + x / previous) / 2m;
        } while (++iterations <= _maxIterations && Math.Abs(current - previous) > _epsilon);

        Console.WriteLine(iterations);

        return current;
    }
}
