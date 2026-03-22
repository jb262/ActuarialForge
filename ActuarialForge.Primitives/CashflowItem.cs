namespace ActuarialForge.Primitives
{
    /// <summary>
    /// Represents a single cashflow amount occurring at a specific model time.
    /// </summary>
    /// <remarks>
    /// A <see cref="CashflowItem"/> combines a <see cref="Money"/> amount and an occurrence <see cref="ModelTime"/>.
    /// It is an immutable value object intended to be used as a building block for cashflow collections and valuation.
    ///
    /// The natural ordering of <see cref="CashflowItem"/> is by <see cref="Time"/> only. This means that two items
    /// with the same time may compare as equal (CompareTo returns 0) even if their amounts differ.
    /// </remarks>
    public sealed record CashflowItem : IComparable, IComparable<CashflowItem>
    {
        /// <summary>
        /// Gets the monetary amount of the cashflow item.
        /// </summary>
        public Money Amount { get; init; }

        /// <summary>
        /// Gets the occurrence time of the cashflow item on the model timeline.
        /// </summary>
        public ModelTime Time { get; init; }

        /// Initializes a new <see cref="CashflowItem"/> instance.
        /// </summary>
        /// <param name="amount">The monetary amount.</param>
        /// <param name="time">The occurrence time on the model timeline.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="amount"/> is <c>null</c>.</exception>
        public CashflowItem(Money amount, ModelTime time)
        {
            Amount = amount ?? throw new ArgumentNullException(nameof(amount));
            Time = time;
        }

        /// <summary>
        /// Initializes a new <see cref="CashflowItem"/> instance using period-based timing conventions.
        /// </summary>
        /// <param name="amount">The monetary amount.</param>
        /// <param name="period">The period index.</param>
        /// <param name="paymentFrequency">The payment frequency (periods per year).</param>
        /// <param name="paymentTiming">Whether the payment occurs in advance or in arrears.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="amount"/> is <c>null</c>.</exception>
        public CashflowItem(Money amount, int period, PaymentFrequency paymentFrequency, PaymentTiming paymentTiming)
            : this(amount, ModelTime.ConvertToModelTime(period, paymentFrequency, paymentTiming)) { }

        /// <summary>
        /// Compares this instance to another <see cref="CashflowItem"/> by occurrence time.
        /// </summary>
        /// <param name="other">The other cashflow item.</param>
        /// <returns>
        /// A value less than zero if this instance occurs earlier than <paramref name="other"/>,
        /// zero if both occur at the same time, and greater than zero if this instance occurs later.
        /// </returns>
        public int CompareTo(CashflowItem? other)
        {
            if (other is null) return 1;
            return Time.CompareTo(other.Time);
        }

        /// <summary>
        /// Compares this instance to another object.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>
        /// A value less than zero if this instance is less than <paramref name="obj"/>,
        /// zero if equal, and greater than zero if greater.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="obj"/> is not a <see cref="CashflowItem"/>.</exception>
        public int CompareTo(object? obj)
        {
            if (obj is null) return 1;
            if (obj is not CashflowItem other)
                throw new ArgumentException("Object must be a CashflowItem.", nameof(obj));

            return CompareTo(other);
        }

        private void EnsureSameOccurrenceDate(CashflowItem other)
        {
            if (Time != other.Time)
                throw new InvalidOperationException($"Cannot combine two cashflow items with different occurrence times ({Time} vs. {other.Time}).");
        }

        /// <summary>
        /// Adds two cashflow items occurring at the same time.
        /// </summary>
        /// <param name="a">The first cashflow item.</param>
        /// <param name="b">The second cashflow item.</param>
        /// <returns>A new cashflow item whose amount is the sum of both amounts.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the items occur at different times.</exception>
        /// <exception cref="CurrencyMismatchException">Thrown if the amounts use different currencies.</exception>
        public static CashflowItem operator +(CashflowItem a, CashflowItem b)
        {
            a.EnsureSameOccurrenceDate(b);

            return a with { Amount = a.Amount + b.Amount };
        }

        /// <summary>
        /// Subtracts two cashflow items occurring at the same time.
        /// </summary>
        /// <param name="a">The left cashflow item.</param>
        /// <param name="b">The right cashflow item.</param>
        /// <returns>A new cashflow item whose amount is the difference of both amounts.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the items occur at different times.</exception>
        /// <exception cref="CurrencyMismatchException">Thrown if the amounts use different currencies.</exception>
        public static CashflowItem operator -(CashflowItem a, CashflowItem b)
        {
            a.EnsureSameOccurrenceDate(b);

            return a with { Amount = a.Amount - b.Amount };
        }

        /// <summary>
        /// Scales the cashflow item's amount by the specified factor.
        /// </summary>
        /// <param name="item">The cashflow item.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>A new cashflow item with a scaled amount.</returns>
        public static CashflowItem operator *(CashflowItem item, decimal factor)
            => item with { Amount = item.Amount * factor };

        /// <summary>
        /// Scales the cashflow item's amount by the specified factor.
        /// </summary>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="item">The cashflow item.</param>
        /// <returns>A new cashflow item with a scaled amount.</returns>
        public static CashflowItem operator *(decimal factor, CashflowItem item)
            => item * factor;

        /// <summary>
        /// Divides the cashflow item's amount by the specified denominator.
        /// </summary>
        /// <param name="item">The cashflow item.</param>
        /// <param name="denominator">The denominator.</param>
        /// <returns>A new cashflow item with a scaled amount.</returns>
        /// <exception cref="DivideByZeroException">Thrown if <paramref name="denominator"/> is zero.</exception>
        public static CashflowItem operator /(CashflowItem item, decimal denominator)
        {
            if (denominator == decimal.Zero)
                throw new DivideByZeroException("Cannot divide the monetary amount of a cashflow item by zero.");

            return item with { Amount = item.Amount / denominator };
        }
    }
}
