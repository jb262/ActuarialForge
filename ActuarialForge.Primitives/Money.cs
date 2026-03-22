namespace ActuarialForge.Primitives
{
    /// <summary>
    /// Represents a monetary amount in a specific currency.
    /// </summary>
    /// <remarks>
    /// <see cref="Money"/> is an immutable value object combining a decimal amount and a <see cref="Currency"/>.
    /// Arithmetic operations and comparisons require both operands to be in the same currency; otherwise a
    /// <see cref="CurrencyMismatchException"/> is thrown.
    ///
    /// Amounts are stored without implicit rounding. Use <see cref="Round"/> to round according to
    /// <see cref="Currency.MinorUnits"/> when needed for display or accounting purposes.
    /// </remarks>
    public sealed record Money : IComparable, IComparable<Money>
    {
        /// <summary>
        /// Gets the numeric amount.
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Gets the currency of the amount.
        /// </summary>
        public Currency Currency { get; init; }

        /// <summary>
        /// Gets a zero monetary value using <see cref="Currency.None"/> as a sentinel currency.
        /// </summary>
        /// <remarks>
        /// This value does not represent a real-world currency. It can be useful as a sentinel,
        /// but should be used with care because currency mismatch checks treat <see cref="Currency.None"/>
        /// as a distinct currency.
        /// </remarks>
        public static Money ZeroNone { get; } = new(Currency.None);

        /// <summary>
        /// Creates a zero monetary value in the specified currency.
        /// </summary>
        /// <param name="currency">The currency of the returned zero amount.</param>
        /// <returns>A <see cref="Money"/> instance representing zero in the specified currency.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="currency"/> is <c>null</c>.</exception>
        public static Money Zero(Currency currency)
            => new(currency);

        /// <summary>
        /// Initializes a new <see cref="Money"/> instance with the specified amount and currency.
        /// </summary>
        /// <param name="amount">The numeric amount.</param>
        /// <param name="currency">The currency of the amount.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="currency"/> is <c>null</c>.</exception>
        public Money(decimal amount, Currency currency)
        {
            Amount = amount;
            Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        }

        /// <summary>
        /// Initializes a new <see cref="Money"/> instance representing zero in the specified currency.
        /// </summary>
        /// <param name="currency">The currency of the amount.</param>
        public Money(Currency currency) : this(decimal.Zero, currency) { }

        /// <summary>
        /// Rounds the amount to the number of minor units specified by <see cref="Currency.MinorUnits"/>.
        /// </summary>
        /// <returns>A new <see cref="Money"/> instance with a rounded amount.</returns>
        /// <remarks>
        /// Rounding uses <see cref="Math.Round(decimal,int)"/> and therefore the default midpoint rounding mode.
        /// </remarks>
        public Money Round()
            => this with { Amount = Math.Round(Amount, Currency.MinorUnits) };


        private void EnsureSameCurrency(Money other)
        {
            if (Currency != other.Currency)
                throw new CurrencyMismatchException(Currency, other.Currency);
        }

        /// <summary>
        /// Compares this instance with another object.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>
        /// A value less than zero if this instance is less than <paramref name="obj"/>,
        /// zero if equal, and greater than zero if greater.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="obj"/> is not a <see cref="Money"/>.</exception>
        /// <exception cref="CurrencyMismatchException">Thrown if currencies do not match.</exception>
        public int CompareTo(object? obj)
        {
            if (obj is null) return 1;
            if (obj is not Money other)
                throw new ArgumentException("Object must be a Money.", nameof(obj));

            return CompareTo(other);
        }

        /// <summary>
        /// Compares this instance with another <see cref="Money"/> instance.
        /// </summary>
        /// <param name="other">The other monetary value.</param>
        /// <returns>
        /// A value less than zero if this instance is less than <paramref name="other"/>,
        /// zero if equal, and greater than zero if greater.
        /// </returns>
        /// <exception cref="CurrencyMismatchException">Thrown if currencies do not match.</exception>
        public int CompareTo(Money? other)
        {
            if (other is null) return 1;
            EnsureSameCurrency(other);
            return Amount.CompareTo(other.Amount);
        }

        /// <summary>
        /// Formats the monetary amount as a string.
        /// </summary>
        /// <param name="displayCurrency">
        /// If <c>true</c>, appends the currency code to the formatted amount.
        /// </param>
        /// <returns>The formatted string.</returns>
        /// <remarks>
        /// The formatted amount is rounded according to <see cref="Currency.MinorUnits"/>.
        /// </remarks>
        public string ToString(bool displayCurrency)
        {
            Money display = Round();

            return $"{display.Amount.ToString($"N{Currency.MinorUnits}")}" + (displayCurrency ? $" {Currency}" : string.Empty);
        }

        /// <summary>
        /// Returns the formatted amount without appending the currency code.
        /// </summary>
        public override string ToString()
            => ToString(false);

        public static Money operator +(Money a, Money b)
        {
            a.EnsureSameCurrency(b);

            return a with { Amount = a.Amount + b.Amount };
        }

        public static Money operator -(Money a, Money b)
        {
            a.EnsureSameCurrency(b);

            return a with { Amount = a.Amount - b.Amount };
        }

        public static Money operator *(Money a, decimal factor)
            => a with { Amount = a.Amount * factor };

        public static Money operator *(decimal factor, Money a)
            => a * factor;

        public static Money operator /(Money a, decimal denominator)
        {
            if (denominator == decimal.Zero)
                throw new DivideByZeroException("Monetary amounts cannot be divided by zero.");

            return a with { Amount = a.Amount / denominator };
        }

        public static bool operator <(Money a, Money b)
        {
            a.EnsureSameCurrency(b);

            return a.Amount < b.Amount;
        }

        public static bool operator >(Money a, Money b)
            => b < a;

        public static bool operator <=(Money a, Money b)
        {
            a.EnsureSameCurrency(b);

            return a.Amount <= b.Amount;
        }

        public static bool operator >=(Money a, Money b)
            => b <= a;
    }
}
