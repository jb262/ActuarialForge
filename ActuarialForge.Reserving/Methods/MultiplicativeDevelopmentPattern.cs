namespace ActuarialForge.Reserving.Methods
{
    /// <summary>
    /// Represents a multiplicative development pattern based on cumulative products.
    /// </summary>
    /// <remarks>
    /// This implementation interprets the supplied factors as multiplicative
    /// development factors. The cumulative factors are derived as cumulative
    /// products of the age-to-age factors.
    /// 
    /// Factors less than one are explicitly allowed and may represent decreasing
    /// development, such as reserve releases.
    /// </remarks>
    internal sealed record MultiplicativeDevelopmentPattern : IDevelopmentPattern
    {
        /// <summary>
        /// Gets the multiplicative age-to-age development factors.
        /// </summary>
        public IReadOnlyList<decimal> AgeToAgeFactors { get; }

        /// <summary>
        /// Gets the cumulative development factors.
        /// </summary>
        /// <remarks>
        /// Each cumulative factor represents the product of all age-to-age factors
        /// up to the corresponding development period.
        /// </remarks>
        public IReadOnlyList<decimal> CumulativeFactors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiplicativeDevelopmentPattern"/> class.
        /// </summary>
        /// <param name="factors">The multiplicative development factors.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="factors"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if no development factors are provided or if any factor is negative.
        /// </exception>
        public MultiplicativeDevelopmentPattern(IEnumerable<decimal> factors)
        {
            ArgumentNullException.ThrowIfNull(factors);

            decimal[] f = factors.ToArray();

            if (f.Length == 0)
                throw new ArgumentException("No development factors provided.");

            if (f.Any(x => x < 0))
                throw new ArgumentException("Multiplicative development factors must be non-negative.");

            AgeToAgeFactors = Array.AsReadOnly(f);
            CumulativeFactors = f.Scan(decimal.One, (x, y) => x * y).ToList().AsReadOnly();
        }
    }
}