using ActuarialForge.Primitives;
using ActuarialForge.Reserving.Model;
using ActuarialForge.Utils;

namespace ActuarialForge.Reserving.Methods
{
    /// <summary>
    /// Represents the Chain Ladder reserving method.
    /// </summary>
    /// <param name="triangle">The observed incremental triangle used as input.</param>
    /// <remarks>
    /// <see cref="ChainLadder"/> transforms the observed incremental triangle into a cumulative triangle,
    /// estimates multiplicative age-to-age development factors, and produces projected cumulative
    /// run-off structures, ultimate values, and variance-related parameters.
    /// </remarks>
    public sealed class ChainLadder(IncrementalTriangle triangle) : IPatternBasedReservingMethod
    {
        private readonly CumulativeTriangle _cumulativeTriangle = triangle is null ? throw new ArgumentNullException(nameof(triangle)) : triangle.ToCumulativeTriangle();

        private ChainLadderFactors? _factors;

        private VarianceParameters? _varianceParameters;

        private RunOffSquare? _projection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainLadder"/> class
        /// using the specified triangle and precomputed Chain Ladder factors.
        /// </summary>
        /// <param name="triangle">The observed incremental triangle.</param>
        /// <param name="factors">The precomputed Chain Ladder factors.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="factors"/> is <c>null</c>.
        /// </exception>
        public ChainLadder(IncrementalTriangle triangle, ChainLadderFactors factors) : this(triangle)
        {
            ArgumentNullException.ThrowIfNull(factors);
            _factors = factors;
        }

        /// <summary>
        /// Computes the Chain Ladder age-to-age factors implied by the observed triangle.
        /// </summary>
        /// <returns>The estimated Chain Ladder factors.</returns>
        public IRunOffFactors ComputeFactors()
        {
            _factors ??= new(ComputeFactors(_cumulativeTriangle));

            return _factors;
        }

        /// <summary>
        /// Computes the projected run-off square implied by the Chain Ladder method.
        /// </summary>
        /// <returns>The projected run-off square.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no valid Chain Ladder factors can be obtained.
        /// </exception>
        public RunOffSquare ComputeProjection()
        {
            if (_projection is null)
            {
                var factors = ComputeFactors() as ChainLadderFactors ?? throw new InvalidOperationException("No valid chain ladder factors computed.");

                _projection = new(ComputeColumns(_cumulativeTriangle, factors), _cumulativeTriangle.TimeGranularity, _cumulativeTriangle.ClaimDateBasis);
            }

            return _projection;
        }

        private static IEnumerable<decimal> ComputeFactors(CumulativeTriangle triangle)
        {
            for (int i = 1; i < triangle.DevelopmentPeriods; i++)
            {
                TriangleColumn column = triangle.GetTriangleColumn(i);
                var previousColumn = triangle.GetColumn(i - 1).Take(column.Count);

                decimal previousSum = previousColumn.Sum(x => x.Amount);

                yield return previousSum == decimal.Zero ?
                    throw new DivideByZeroException("Cannot compute chain ladder factors if the sum of a column equals zero.") :
                    column.Sum(x => x.Amount) / previousSum;
            }
        }

        private static IEnumerable<SquareColumn> ComputeColumns(CumulativeTriangle triangle, ChainLadderFactors factors)
        {
            SquareColumn previousColumn = new(triangle.GetTriangleColumn(0));

            yield return previousColumn;

            for (int i = 1; i < triangle.DevelopmentPeriods; i++)
            {
                SquareColumn column = new(triangle.GetTriangleColumn(i));

                if (column.Count < previousColumn.Count)
                {
                    int periodsToProject = previousColumn.Count - column.Count;
                    decimal factor = factors[new(i)];

                    IEnumerable<Money> projectedColumn = column
                        .Concat(previousColumn
                            .TakeLast(periodsToProject)
                            .Select(x => factor * x)
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

        /// <summary>
        /// Develops a full run-off square from the specified starting values.
        /// </summary>
        /// <param name="start">The starting values used as the first development column.</param>
        /// <returns>A projected run-off square.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no valid Chain Ladder factors can be obtained.
        /// </exception>
        public RunOffSquare DevelopSquare(TriangleColumn start)
        {
            SquareColumn previousColumn = new(start);

            List <SquareColumn> columns = new(start.Count)
            {
                previousColumn
            };

            ChainLadderFactors factors = ComputeFactors() as ChainLadderFactors ?? throw new InvalidOperationException("No valid chain ladder factors computed.");

            for (int i = 1; i < _cumulativeTriangle.DevelopmentPeriods; i++)
            {
                SquareColumn column = new(previousColumn.Select(a => a * factors[new(i)]));
                columns.Add(column);
                previousColumn = column;
            }

            return new(columns, _cumulativeTriangle.TimeGranularity, _cumulativeTriangle.ClaimDateBasis);
        }

        /// <summary>
        /// Develops a cumulative triangle from the specified starting values.
        /// </summary>
        /// <param name="start">The starting values used as the first development column.</param>
        /// <returns>A projected cumulative triangle.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no valid Chain Ladder factors can be obtained.
        /// </exception>
        public CumulativeTriangle DevelopTriangle(TriangleColumn start)
        {
            int accidentYears = start.Count;
            List<TriangleRow> rows = new(accidentYears);
            ChainLadderFactors factors = ComputeFactors() as ChainLadderFactors ?? throw new InvalidOperationException("No valid chain ladder factors computed.");

            for (int i = 0; i < accidentYears; i++)
            {
                List<Money> row = new(_cumulativeTriangle.DevelopmentPeriods);

                Money amount = start[i];
                row.Add(amount);

                for (int j = 1; j < _cumulativeTriangle.DevelopmentPeriods - i; j++)
                {
                    amount *= factors[new(j)];
                    row.Add(amount);
                }

                rows.Add(new(row));
            }

            return new(rows, _cumulativeTriangle.TimeGranularity, _cumulativeTriangle.ClaimDateBasis, _cumulativeTriangle.Currency);
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

                if (currentCurrency != ultimates[i].Currency)
                    throw new CurrencyMismatchException(ultimates[i].Currency, currentCurrency);

                if (premiumsArr[i].Amount == decimal.Zero)
                    throw new DivideByZeroException("Cannot compute loss ratio for a premium of amount zero.");

                yield return ultimates[i].Amount / premiumsArr[i].Amount;
            }
        }

        /// <summary>
        /// Computes the variance parameters implied by the Chain Ladder method.
        /// </summary>
        /// <returns>The estimated variance parameters.</returns>
        public VarianceParameters ComputeVarianceParameters()
        {
            if (_varianceParameters is null)
            {
                List<decimal> varianceParameters = [];

                // Variable names chosen according to usual actuarial conventions.
                int n = _cumulativeTriangle.DevelopmentPeriods;

                for (int k = 1; k < n - 1; k++)
                {
                    decimal varianceParameter = _cumulativeTriangle
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

            var individualFactors = _cumulativeTriangle
                .GetColumn(developmentPeriod)
                .Zip(_cumulativeTriangle.GetColumn(developmentPeriod - 1), (x, y) => x.Amount / y.Amount);

            foreach (var factor in individualFactors)
                yield return DecimalMath.DecimalPow(factor - estimatedFactor, 2);
        }

        /// <summary>
        /// Computes the ultimate cumulative values implied by the Chain Ladder projection.
        /// </summary>
        /// <returns>The projected ultimate values.</returns>
        public IEnumerable<Money> ComputeUltimates()
            => ComputeProjection().GetUltimates();
    }
}
