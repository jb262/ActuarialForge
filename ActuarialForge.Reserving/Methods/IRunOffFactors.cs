using ActuarialForge.Reserving.Model;

namespace ActuarialForge.Reserving.Methods
{
    /// <summary>
    /// Represents a collection of run-off development factors used in a reserving method.
    /// </summary>
    /// <remarks>
    /// A <see cref="IRunOffFactors"/> instance provides indexed and enumerable access
    /// to the primary development factors used by a reserving method.
    /// 
    /// The meaning of the enumerated factors depends on the specific method, e.g.:
    /// - For multiplicative methods (e.g. Chain Ladder), the enumeration typically
    ///   represents age-to-age factors.
    /// - For methods such as Bornhuetter-Ferguson, the enumeration may represent
    ///   cumulative development factors.
    /// And so on.
    /// 
    /// For explicit access to both representations, see
    /// <see cref="IDevelopmentPattern.AgeToAgeFactors"/> and
    /// <see cref="IDevelopmentPattern.CumulativeFactors"/>.
    /// </remarks>
    public interface IRunOffFactors : IEnumerable<decimal>, IDevelopmentPattern
    {
        /// <summary>
        /// Gets the number of development periods covered by the factor set.
        /// </summary>
        int DevelopmentPeriods { get; }

        /// <summary>
        /// Gets the structural interpretation of the development pattern.
        /// </summary>
        DevelopmentPatternStructure Pattern { get; }

        /// <summary>
        /// Gets the factor associated with the specified development period.
        /// </summary>
        /// <param name="developmentPeriod">The development period.</param>
        /// <returns>The factor for the specified development period.</returns>
        decimal this[DevelopmentPeriod developmentPeriod] { get; }
    }
}
