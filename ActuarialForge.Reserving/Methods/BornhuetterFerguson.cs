using ActuarialForge.Primitives;
using ActuarialForge.Reserving.Model;

namespace ActuarialForge.Reserving.Methods
{
    /// <summary>
    /// Represents the Bornhuetter-Ferguson reserving method.
    /// </summary>
    /// <remarks>
    /// <see cref="BornhuetterFerguson"/> combines a-priori ultimate expectations with
    /// a development pattern to project run-off structures and ultimate values.
    ///
    /// Internally, the method is implemented as a specialised application of the
    /// additive method using Bornhuetter-Ferguson factors and a-priori ultimate values.
    /// </remarks>
    public sealed class BornhuetterFerguson : IPatternBasedReservingMethod
    {
        // The Bornhuetter-Ferguson is technically a variation of the additive method.
        // Therefore it provides the results of an instance of the additive method with a different set of factors and volume measures.
        private readonly AdditiveMethod _innerMethod;

        private readonly BornhuetterFergusonFactors _bfFactors;

        /// <summary>
        /// Initializes a new instance of the <see cref="BornhuetterFerguson"/> class
        /// from an observed triangle, Bornhuetter-Ferguson factors, and a-priori ultimate values.
        /// </summary>
        /// <param name="triangle">The observed incremental triangle.</param>
        /// <param name="factors">The Bornhuetter-Ferguson factors.</param>
        /// <param name="volumeMeasures">The a-priori ultimate values by accident period.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any required argument is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if no volume measures are provided or if fewer volume measures than accident periods are supplied.
        /// </exception>
        /// <remarks>
        /// If more volume measures than accident periods are supplied, only the values
        /// corresponding to the required accident periods are used; additional values are ignored.
        /// </remarks>
        public BornhuetterFerguson(IncrementalTriangle triangle, BornhuetterFergusonFactors factors, IEnumerable<Money> volumeMeasures)
        {
            ArgumentNullException.ThrowIfNull(triangle);
            ArgumentNullException.ThrowIfNull(factors);
            ArgumentNullException.ThrowIfNull(volumeMeasures);

            var volumeMeasureValues = volumeMeasures.ToArray();

            if (volumeMeasureValues.Length == 0)
                throw new ArgumentException("No volume measures provided.", nameof(volumeMeasures));

            if (volumeMeasureValues.Length < triangle.AccidentPeriods)
                throw new ArgumentException("Not enough volume measures provided.", nameof(volumeMeasures));

            var accidentPeriodIndices = Enumerable.Range(0, volumeMeasureValues.Length);

            Dictionary<AccidentPeriod, Money> aPrioriUltimates = volumeMeasureValues
                .Zip(accidentPeriodIndices, (x, y) => new { AccidentPeriod = new AccidentPeriod(y), VolumeMeasure = x} )
                .ToDictionary(x => x.AccidentPeriod, x => x.VolumeMeasure);

            _innerMethod = new(triangle, aPrioriUltimates, new(factors.AgeToAgeFactors));
            _bfFactors = factors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BornhuetterFerguson"/> class
        /// from premiums and ultimate loss ratios.
        /// </summary>
        /// <param name="triangle">The observed incremental triangle.</param>
        /// <param name="factors">The Bornhuetter-Ferguson factors.</param>
        /// <param name="premiums">The premium values.</param>
        /// <param name="ultimateLossRatios">The ultimate loss ratios.</param>
        public BornhuetterFerguson(IncrementalTriangle triangle, BornhuetterFergusonFactors factors, IEnumerable<Money> premiums, IEnumerable<decimal> ultimateLossRatios)
            : this(triangle, factors, premiums.Zip(ultimateLossRatios, (p, r) => p * r)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BornhuetterFerguson"/> class
        /// from an arbitrary run-off factor pattern and a-priori ultimate values.
        /// </summary>
        /// <param name="triangle">The observed incremental triangle.</param>
        /// <param name="factors">The source run-off factor pattern.</param>
        /// <param name="volumeMeasures">The a-priori ultimate values.</param>
        public BornhuetterFerguson(IncrementalTriangle triangle, IRunOffFactors factors, IEnumerable<Money> volumeMeasures)
            : this(triangle, BornhuetterFergusonFactors.FromRunOffFactors(factors), volumeMeasures) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BornhuetterFerguson"/> class
        /// from an arbitrary run-off factor pattern, premiums, and ultimate loss ratios.
        /// </summary>
        /// <param name="triangle">The observed incremental triangle.</param>
        /// <param name="factors">The source run-off factor pattern.</param>
        /// <param name="premiums">The premium values.</param>
        /// <param name="ultimateLossRatios">The ultimate loss ratios.</param>
        public BornhuetterFerguson(IncrementalTriangle triangle, IRunOffFactors factors, IEnumerable<Money> premiums, IEnumerable<decimal> ultimateLossRatios)
            : this(triangle, BornhuetterFergusonFactors.FromRunOffFactors(factors), premiums, ultimateLossRatios) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BornhuetterFerguson"/> class
        /// from another pattern-based reserving method and premiums.
        /// </summary>
        /// <param name="triangle">The observed incremental triangle.</param>
        /// <param name="reservingMethod">The source reserving method used to derive development factors and model-implied ultimate loss ratios.</param>
        /// <param name="premiums">The premium values.</param>
        public BornhuetterFerguson(IncrementalTriangle triangle, IPatternBasedReservingMethod reservingMethod, IEnumerable<Money> premiums)
            : this(triangle, BornhuetterFergusonFactors.FromRunOffFactors(reservingMethod.ComputeFactors()), premiums, reservingMethod.ComputeUltimateLossRatios(premiums)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BornhuetterFerguson"/> class
        /// from an additive method.
        /// </summary>
        /// <param name="triangle">The observed incremental triangle.</param>
        /// <param name="additiveMethod">The additive method providing development factors, volume measures, and model-implied ultimate loss ratios.</param>
        public BornhuetterFerguson(IncrementalTriangle triangle, AdditiveMethod additiveMethod)
            : this(triangle, BornhuetterFergusonFactors.FromRunOffFactors(additiveMethod.ComputeFactors()), additiveMethod.VolumeMeasures, additiveMethod.ComputeUltimateLossRatios()) { }

        /// <summary>
        /// Computes the Bornhuetter-Ferguson factors used by the method.
        /// </summary>
        /// <returns>The Bornhuetter-Ferguson factors.</returns>
        public IRunOffFactors ComputeFactors()
            => _bfFactors;

        /// <summary>
        /// Computes the projected run-off square implied by the Bornhuetter-Ferguson method.
        /// </summary>
        /// <returns>The projected run-off square.</returns>
        public RunOffSquare ComputeProjection()
            => _innerMethod.ComputeProjection();

        /// <summary>
        /// Computes the variance parameters implied by the method.
        /// </summary>
        /// <returns>The estimated variance parameters.</returns>
        public VarianceParameters ComputeVarianceParameters()
            => _innerMethod.ComputeVarianceParameters();

        /// <summary>
        /// Develops a full run-off square from the specified starting values.
        /// </summary>
        /// <param name="start">The starting values used as the first development column.</param>
        /// <returns>A projected run-off square.</returns>
        public RunOffSquare DevelopSquare(TriangleColumn start)
            => _innerMethod.DevelopSquare(start);

        /// <summary>
        /// Develops a cumulative triangle from the specified starting values.
        /// </summary>
        /// <param name="start">The starting values used as the first development column.</param>
        /// <returns>A projected cumulative triangle.</returns>
        public CumulativeTriangle DevelopTriangle(TriangleColumn start)
            => _innerMethod.DevelopTriangle(start);

        /// <summary>
        /// Computes the ultimate cumulative values implied by the Bornhuetter-Ferguson projection.
        /// </summary>
        /// <returns>The projected ultimate values.</returns>
        public IEnumerable<Money> ComputeUltimates()
            => ComputeProjection().GetUltimates();

        /// <summary>
        /// Computes the ultimate loss ratios implied by the model and the supplied premiums.
        /// </summary>
        /// <param name="premiums">The premium values.</param>
        /// <returns>The model-implied ultimate loss ratios.</returns>
        public IEnumerable<decimal> ComputeUltimateLossRatios(IEnumerable<Money> premiums)
            => _innerMethod.ComputeUltimateLossRatios(premiums);
    }
}
