namespace ActuarialForge.Reserving.Methods
{
    /// <summary>
    /// Represents an additive development pattern based on incremental loss development.
    /// </summary>
    /// <remarks>
    /// This implementation interprets the supplied factors as additive incremental
    /// development components. The cumulative factors are derived as cumulative sums
    /// of the incremental factors.
    /// 
    /// This type of pattern is typically used in additive reserving methods.
    /// </remarks>
    internal sealed record IncrementalLossDevelopmentPattern : IDevelopmentPattern
    {
        /// <summary>
        /// Gets the additive incremental development factors.
        /// </summary>
        public IReadOnlyList<decimal> AgeToAgeFactors { get; }

        /// <summary>
        /// Gets the cumulative additive development factors.
        /// </summary>
        /// <remarks>
        /// Each cumulative factor represents the sum of all incremental factors up to
        /// the corresponding development period.
        /// </remarks>
        public IReadOnlyList<decimal> CumulativeFactors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementalLossDevelopmentPattern"/> class.
        /// </summary>
        /// <param name="factors">The additive incremental development factors.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="factors"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if no development factors are provided.
        /// </exception>
        public IncrementalLossDevelopmentPattern(IEnumerable<decimal> factors)
        {
            ArgumentNullException.ThrowIfNull(factors);

            List<decimal> f = factors.ToList();

            if (f.Count == 0)
                throw new ArgumentException("No development factors provided.");

            AgeToAgeFactors = f.AsReadOnly();
            CumulativeFactors = f.Scan(decimal.Zero, (x, y) => x + y).ToList().AsReadOnly();
        }
    }
}
