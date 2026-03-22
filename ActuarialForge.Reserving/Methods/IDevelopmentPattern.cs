namespace ActuarialForge.Reserving.Methods
{
    /// <summary>
    /// Represents a development pattern used for reserving projections.
    /// </summary>
    /// <remarks>
    /// A development pattern consists of age-to-age development factors and the
    /// corresponding cumulative development factors derived from them.
    /// 
    /// Implementations may represent either multiplicative or additive development,
    /// depending on the interpretation of the factors.
    /// </remarks>
    public interface IDevelopmentPattern
    {
        /// <summary>
        /// Gets the age-to-age development factors of the pattern.
        /// </summary>
        /// <remarks>
        /// Each factor represents the development from one development period to the next.
        /// The interpretation (multiplicative or additive) depends on the implementation.
        /// </remarks>
        IReadOnlyList<decimal> AgeToAgeFactors { get; }

        /// <summary>
        /// Gets the cumulative development factors of the pattern.
        /// </summary>
        /// <remarks>
        /// Each cumulative factor represents the cumulative effect of the age-to-age factors
        /// up to the corresponding development period.
        /// 
        /// For multiplicative patterns, this corresponds to cumulative products.
        /// For additive patterns, this corresponds to cumulative sums.
        /// </remarks>
        IReadOnlyList<decimal> CumulativeFactors { get; }
    }
}
