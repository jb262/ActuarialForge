using ActuarialForge.Primitives;
using ActuarialForge.Reserving.Model;

namespace ActuarialForge.Reserving.Methods
{
    /// <summary>
    /// Defines a reserving method based on a development pattern or run-off factor pattern.
    /// </summary>
    /// <remarks>
    /// Implementations of <see cref="IPatternBasedReservingMethod"/> derive development factors,
    /// project cumulative development structures, and compute ultimate quantities and variance-related
    /// parameters based on an observed triangle.
    /// </remarks>
    public interface IPatternBasedReservingMethod
    {
        /// <summary>
        /// Computes the run-off factors implied by the reserving method.
        /// </summary>
        /// <returns>The run-off factors used by the method.</returns>
        IRunOffFactors ComputeFactors();

        /// <summary>
        /// Computes the projected run-off square implied by the reserving method.
        /// </summary>
        /// <returns>The projected run-off square.</returns>
        RunOffSquare ComputeProjection();

        /// <summary>
        /// Develops a run-off square starting from the specified observed values.
        /// </summary>
        /// <param name="start">
        /// The starting values used as the basis for development.
        /// </param>
        /// <returns>A projected run-off square.</returns>
        RunOffSquare DevelopSquare(TriangleColumn start);

        /// <summary>
        /// Develops a cumulative triangle starting from the specified observed values.
        /// </summary>
        /// <param name="start">
        /// The starting values used as the basis for development.
        /// </param>
        /// <returns>A projected cumulative triangle.</returns>
        CumulativeTriangle DevelopTriangle(TriangleColumn start);

        /// <summary>
        /// Computes the ultimate values implied by the reserving method.
        /// </summary>
        /// <returns>The ultimate values by accident period.</returns>
        IEnumerable<Money> ComputeUltimates();

        /// <summary>
        /// Computes ultimate loss ratios based on the supplied premium values.
        /// </summary>
        /// <param name="premiums">The premium values used as the denominator.</param>
        /// <returns>The ultimate loss ratios.</returns>
        IEnumerable<decimal> ComputeUltimateLossRatios(IEnumerable<Money> premiums);

        /// <summary>
        /// Computes the variance-related parameters implied by the reserving method.
        /// </summary>
        /// <returns>The variance parameters associated with the method.</returns>
        VarianceParameters ComputeVarianceParameters();
    }
}
