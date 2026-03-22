using ActuarialForge.Primitives;
using ActuarialForge.Reserving.Model;
using ActuarialForge.Utils;

namespace ActuarialForge.Reserving.Methods
{
    /// <summary>
    /// Represents an additive reserving method based on volume measures.
    /// </summary>
    /// <remarks>
    /// <see cref="AdditiveMethod"/> estimates additive development factors by relating
    /// observed incremental losses to supplied volume measures. It supports projection
    /// of run-off structures, computation of ultimate values, ultimate loss ratios,
    /// and variance-related parameters.
    /// </remarks>
    public sealed class AdditiveMethod : IPatternBasedReservingMethod
    {
        private readonly IncrementalTriangle _triangle;

        private AdditiveFactors? _factors;

        private VarianceParameters? _varianceParameters;

        private RunOffSquare? _projection;

        /// <summary>
        /// Gets the volume measures used by the method.
        /// </summary>
        public IReadOnlyList<Money> VolumeMeasures { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditiveMethod"/> class.
        /// </summary>
        /// <param name="triangle">The observed incremental triangle.</param>
        /// <param name="volumeMeasures">
        /// The volume measures by accident period used to estimate additive factors.
        /// Missing accident periods are filled with zero values.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="triangle"/> or <paramref name="volumeMeasures"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="CurrencyMismatchException">
        /// Thrown if a supplied volume measure uses a different currency than the triangle.
        /// </exception>
        public AdditiveMethod(IncrementalTriangle triangle, IReadOnlyDictionary<AccidentPeriod, Money> volumeMeasures)
        {
            ArgumentNullException.ThrowIfNull(triangle);
            ArgumentNullException.ThrowIfNull(volumeMeasures);

            _triangle = triangle;

            List<Money> methodVolumeMeasures = [];

            for (int i = 0; i < _triangle.AccidentPeriods; i++)
            {
                if (!volumeMeasures.TryGetValue(new(i), out var volumeMeasure))
                    volumeMeasure = Money.Zero(triangle.Currency);
                else if (volumeMeasure.Currency != triangle.Currency)
                    throw new CurrencyMismatchException(triangle.Currency, volumeMeasure.Currency);

                methodVolumeMeasures.Add(volumeMeasure);
            }

            VolumeMeasures = methodVolumeMeasures.AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditiveMethod"/> class
        /// using precomputed additive factors.
        /// </summary>
        /// <param name="triangle">The observed incremental triangle.</param>
        /// <param name="volumeMeasures">The volume measures by accident period.</param>
        /// <param name="factors">The precomputed additive factors.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="factors"/> is <c>null</c>.
        /// </exception>
        public AdditiveMethod(IncrementalTriangle triangle, IReadOnlyDictionary<AccidentPeriod, Money> volumeMeasures, AdditiveFactors factors)
            : this(triangle, volumeMeasures)
        {
            ArgumentNullException.ThrowIfNull(factors);
            _factors = factors;
        }

        /// <summary>
        /// Computes the additive development factors implied by the triangle and volume measures.
        /// </summary>
        /// <returns>The estimated additive factors.</returns>
        public IRunOffFactors ComputeFactors()
        {
            _factors ??= new AdditiveFactors(ComputeFactors(_triangle, VolumeMeasures));

            return _factors;
        }

        private static IEnumerable<decimal> ComputeFactors(IncrementalTriangle triangle, IEnumerable<Money> volumeMeasures)
        {
            for (int i = 0; i < triangle.DevelopmentPeriods; i++)
            {
                TriangleColumn column = triangle.GetTriangleColumn(i);

                decimal totalClaims = column.Sum(c => c.Amount);
                decimal totalPremiums = volumeMeasures.Take(column.Count).Sum(c => c.Amount);

                yield return totalPremiums == decimal.Zero ? 
                    throw new InvalidOperationException("Volume measure must not be zero.") :
                    totalClaims / totalPremiums;
            }
        }

        /// <summary>
        /// Computes the projected run-off square implied by the additive method.
        /// </summary>
        /// <returns>The projected run-off square.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no valid additive factors can be obtained.
        /// </exception>
        public RunOffSquare ComputeProjection()
        {
            if (_projection is null)
            {
                var factors = ComputeFactors() as AdditiveFactors ?? throw new InvalidOperationException("No valid additive run off factors computed.");
                CumulativeTriangle cumulativeTriangle = _triangle.ToCumulativeTriangle();

                _projection = new(ComputeColumns(cumulativeTriangle, VolumeMeasures, factors), _triangle.TimeGranularity, _triangle.ClaimDateBasis);
            }

            return _projection;
        }

        private static IEnumerable<SquareColumn> ComputeColumns(CumulativeTriangle triangle, IReadOnlyList<Money> volumeMeasures, AdditiveFactors factors)
        {
            SquareColumn previousColumn = new(triangle.GetColumn(0));

            yield return previousColumn;

            for (int i = 1; i < triangle.DevelopmentPeriods; i++)
            {
                SquareColumn column = new(triangle.GetColumn(i));

                if (column.Count < previousColumn.Count)
                {
                    int periodsToProject = previousColumn.Count - column.Count;
                    decimal factor = factors[new(i)];

                    IEnumerable<Money> projectedColumn = column
                        .Concat(
                            CalculateIncrement(
                                volumeMeasures.TakeLast(periodsToProject),
                                factor)
                            .Zip(previousColumn.TakeLast(periodsToProject), (x, y) => x + y)
                            );

                    previousColumn = new(projectedColumn);
                }
                else
                {
                    previousColumn = column;
                }

                yield return previousColumn;
            }
        }

        private static IEnumerable<Money> CalculateIncrement(IEnumerable<Money> amounts, decimal factor)
            => amounts.Select(a => a * factor);

        /// <summary>
        /// Computes the variance parameters implied by the additive method.
        /// </summary>
        /// <returns>The estimated variance parameters.</returns>
        public VarianceParameters ComputeVarianceParameters()
        {
            if (_varianceParameters is null)
            {
                List<decimal> varianceParameters = [];

                // Variable names chosen according to usual actuarial conventions.
                int n = _triangle.DevelopmentPeriods;

                for (int k = 1; k < n - 1; k++)
                {
                    decimal varianceParameter = _triangle
                        .GetColumn(k - 1)
                        .Zip(ComputeSquaredFactorResiduals(k), (x, y) => x.Amount * y)
                        .Sum() / (n - k);

                    varianceParameters.Add(varianceParameter);
                }

                _varianceParameters = new(varianceParameters);
            }

            return _varianceParameters;
        }

        private IEnumerable<decimal> ComputeSquaredFactorResiduals(int developmentPeriod)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(developmentPeriod, 0);

            decimal estimatedFactor = ComputeFactors()[new(developmentPeriod)];

            var individualFactors = _triangle
                .GetColumn(developmentPeriod)
                .Zip(VolumeMeasures, (x, y) =>
                {
                    if (y.Amount == decimal.Zero)
                        throw new DivideByZeroException("Cannot compute additive residuals for a zero volume measure.");

                    return x.Amount / y.Amount;
                });

            foreach (var factor in individualFactors)
                yield return DecimalMath.DecimalPow(factor - estimatedFactor, 2);
        }

        /// <summary>
        /// Develops a full run-off square from the specified starting values.
        /// </summary>
        /// <param name="start">The starting values used as the first development column.</param>
        /// <returns>A projected run-off square.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no valid additive factors can be obtained.
        /// </exception>
        public RunOffSquare DevelopSquare(TriangleColumn start)
        {
            SquareColumn previous = new(start);

            List<SquareColumn> columns = new(_triangle.DevelopmentPeriods)
            {
                previous
            };

            var factors = ComputeFactors() as AdditiveFactors ?? throw new InvalidOperationException("No valid additive run off factors computed.");

            for (int i = 1; i < _triangle.DevelopmentPeriods; i++)
            {
                SquareColumn column = new(previous.Zip(CalculateIncrement(VolumeMeasures, factors[new(i)]), (x, y) => x + y));
                columns.Add(column);
                previous = column;
            }

            return new(columns, _triangle.TimeGranularity, _triangle.ClaimDateBasis);
        }

        /// <summary>
        /// Develops a cumulative triangle from the specified starting values.
        /// </summary>
        /// <param name="start">The starting values used as the first development column.</param>
        /// <returns>A projected cumulative triangle.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no valid additive factors can be obtained.
        /// </exception>
        public CumulativeTriangle DevelopTriangle(TriangleColumn start)
        {
            TriangleColumn previous = start;

            List<TriangleColumn> columns = new(_triangle.DevelopmentPeriods)
            {
                previous
            };

            var factors = ComputeFactors() as AdditiveFactors ?? throw new InvalidOperationException("No valid additive run off factors computed.");

            for (int i = 1; i < _triangle.DevelopmentPeriods;  i++)
            {
                TriangleColumn column = new(
                    previous.Take(previous.Count - 1)
                        .Zip(CalculateIncrement(VolumeMeasures.Take(previous.Count - 1), factors[new(i)]), (x, y) => x + y));

                columns.Add(column);
                previous = column;
            }

            return new(columns, _triangle.TimeGranularity, _triangle.ClaimDateBasis, _triangle.Currency);
        }

        /// <summary>
        /// Computes the ultimate loss ratios from the projected ultimates and the supplied premiums.
        /// </summary>
        /// <param name="premiums">The premiums used as denominators.</param>
        /// <returns>The ultimate loss ratios.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="premiums"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no premiums are provided or if not enough ultimates are available.
        /// </exception>
        /// <exception cref="CurrencyMismatchException">
        /// Thrown if the supplied premiums do not share a common currency or if premium and ultimate currencies differ.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown if one of the supplied premiums has amount zero.
        public IEnumerable<decimal> ComputeUltimateLossRatios(IEnumerable<Money> premiums)
        {
            ArgumentNullException.ThrowIfNull(premiums);
            var premiumsArr = premiums.ToArray();

            if (premiumsArr.Length == 0)
                throw new InvalidOperationException("No premiums provided.");

            var ultimates = ComputeUltimates().TakeLast(premiumsArr.Length).ToArray();

            if (ultimates.Length < premiumsArr.Length)
                throw new InvalidOperationException("Not enough ultimates available.");

            Currency currency = premiumsArr[0].Currency;

            for (int i = 0; i < premiumsArr.Length; i++)
            {
                Currency currentCurrency = premiumsArr[i].Currency;

                if (currentCurrency != currency)
                    throw new CurrencyMismatchException(currency, currentCurrency);

                if(currentCurrency != ultimates[i].Currency)
                    throw new CurrencyMismatchException(ultimates[i].Currency, currentCurrency);

                if (premiumsArr[i].Amount == decimal.Zero)
                    throw new DivideByZeroException("Cannot compute loss ratio for a premium of amount zero.");

                yield return ultimates[i].Amount / premiumsArr[i].Amount;
            }
        }

        /// <summary>
        /// Computes the ultimate loss ratios using the method's own volume measures.
        /// </summary>
        /// <returns>The ultimate loss ratios.</returns>
        public IEnumerable<decimal> ComputeUltimateLossRatios()
            => ComputeUltimateLossRatios(VolumeMeasures);

        /// <summary>
        /// Computes the ultimate cumulative values implied by the additive projection.
        /// </summary>
        /// <returns>The projected ultimate values.</returns>
        public IEnumerable<Money> ComputeUltimates()
            => ComputeProjection().GetUltimates();
    }
}
