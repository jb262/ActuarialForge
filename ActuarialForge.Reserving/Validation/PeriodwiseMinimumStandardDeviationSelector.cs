using ActuarialForge.Reserving.Methods;
using ActuarialForge.Reserving.Model;
using ActuarialForge.Utils;

namespace ActuarialForge.Reserving.Validation
{
    /// <summary>
    /// Selects reserving methods by counting how often they attain the minimum
    /// empirical standard deviation across development periods.
    /// </summary>
    /// <remarks>
    /// For each development period, the method with the smallest empirical standard
    /// deviation of residuals receives one point. The best method is the one with
    /// the highest number of period-wise minima.
    /// </remarks>
    public class PeriodwiseMinimumStandardDeviationSelector : IReservingMethodSelector<IPatternBasedReservingMethod, int>
    {
        private readonly CumulativeTriangle _cumulativeTriangle;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodwiseMinimumStandardDeviationSelector"/> class.
        /// </summary>
        /// <param name="cumulativeTriangle">The observed cumulative triangle used as the validation basis.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="cumulativeTriangle"/> is <c>null</c>.
        /// </exception>
        public PeriodwiseMinimumStandardDeviationSelector(CumulativeTriangle cumulativeTriangle)
        {
            ArgumentNullException.ThrowIfNull(cumulativeTriangle);
            _cumulativeTriangle = cumulativeTriangle;
        }

        /// <summary>
        /// Selects the best reserving method from the specified candidates.
        /// </summary>
        /// <param name="methods">The reserving methods to compare.</param>
        /// <returns>The method with the highest score.</returns>
        public IPatternBasedReservingMethod SelectBest(params IPatternBasedReservingMethod[] methods)
            => SelectBest(Scores(methods));

        /// <summary>
        /// Selects the best reserving method from a precomputed score dictionary.
        /// </summary>
        /// <param name="scores">The scores by reserving method.</param>
        /// <returns>The method with the highest score.</returns>
        public static IPatternBasedReservingMethod SelectBest(IReadOnlyDictionary<IPatternBasedReservingMethod, int> scores)
            => scores.MaxBy(x => x.Value).Key;

        /// <summary>
        /// Computes the scores of the specified reserving methods.
        /// </summary>
        /// <param name="methods">The reserving methods to evaluate.</param>
        /// <returns>
        /// A dictionary mapping each reserving method to the number of development periods
        /// in which it achieves the minimum empirical standard deviation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="methods"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if no methods are provided.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the methods produce incompatible numbers of comparable development periods.
        /// </exception>
        public IReadOnlyDictionary<IPatternBasedReservingMethod, int> Scores(params IPatternBasedReservingMethod[] methods)
        {
            ArgumentNullException.ThrowIfNull(methods);

            if (methods.Length == 0)
                throw new ArgumentException("At least one method must be provided.", nameof(methods));

            var sdPerMethod = methods
                .ToDictionary(
                    m => m,
                    m => EmpiricalStandardDeviations(m).ToArray());

            int periodCount = sdPerMethod.First().Value.Length;

            if (sdPerMethod.Any(x => x.Value.Length != periodCount))
                throw new InvalidOperationException("All methods must produce the same number of development periods.");

            var minimumCounter = methods.ToDictionary(m => m, _ => 0);

            for (int i = 0; i < periodCount; i++)
            {
                var minEntry = sdPerMethod
                    .Select(kvp => new
                    {
                        Method = kvp.Key,
                        Value = kvp.Value[i]
                    })
                    .MinBy(x => x.Value)
                    ?? throw new InvalidOperationException("No reserving methods available.");

                minimumCounter[minEntry.Method]++;
            }

            return minimumCounter.AsReadOnly();
        }

        /// <summary>
        /// Computes the empirical standard deviations of residuals by development period
        /// for the specified reserving method.
        /// </summary>
        /// <param name="method">The reserving method to evaluate.</param>
        /// <returns>The empirical standard deviations by development period.</returns>
        /// <remarks>
        /// Development period zero is skipped because its residual standard deviation is always zero.
        /// Development periods with fewer than two observations are not included.
        /// </remarks>
        public IEnumerable<decimal> EmpiricalStandardDeviations(IPatternBasedReservingMethod method)
        {
            Residuals residuals = new(_cumulativeTriangle, method);

            // The first column is skipped as its standard deviation is always zero.
            for (int i = 1; i < _cumulativeTriangle.DevelopmentPeriods; i++)
            {
                int observations = _cumulativeTriangle.AccidentPeriods - i;

                if (observations < 2)
                    continue;

                decimal sd = decimal.Zero;

                for (int j = 0; j < observations; j++)
                {
                    decimal squaredResidual = DecimalMath.DecimalPow(residuals[new(j), new(i)].Amount, 2);
                    sd += squaredResidual;
                }

                yield return DecimalMath.Sqrt(sd / (observations - 1));
            }
        }
    }
}