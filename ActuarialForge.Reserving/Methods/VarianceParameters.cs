using ActuarialForge.Reserving.Model;
using ActuarialForge.Utils;
using System.Collections;

namespace ActuarialForge.Reserving.Methods
{
    /// <summary>
    /// Represents variance parameters associated with development periods.
    /// </summary>
    /// <remarks>
    /// <see cref="VarianceParameters"/> stores the observed variance parameters used in
    /// reserving methods such as Chain Ladder. The final variance parameter is typically
    /// not directly observed and may be estimated separately using <see cref="EstimateLast"/>.
    /// </remarks>
    public sealed record VarianceParameters : IEnumerable<decimal>
    {
        private readonly decimal[] _varianceParameters;

        /// <summary>
        /// Gets the number of development periods implied by the variance parameter set.
        /// </summary>
        /// <remarks>
        /// If <c>m</c> observed variance parameters are stored, the implied number of
        /// development periods is <c>m + 2</c>.
        /// </remarks>
        public int DevelopmentPeriods { get; }

        /// <summary>
        /// Gets the variance parameter associated with the specified development period.
        /// </summary>
        /// <param name="developmentPeriod">The development period.</param>
        /// <returns>The variance parameter for the specified development period.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="developmentPeriod"/> does not correspond to an observed variance parameter.
        /// </exception>
        public decimal this[DevelopmentPeriod developmentPeriod]
        {
            get
            {
                if (developmentPeriod.Lag <= 0 || developmentPeriod.Lag > _varianceParameters.Length)
                    throw new ArgumentOutOfRangeException(nameof(developmentPeriod));

                return _varianceParameters[developmentPeriod.Lag - 1];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VarianceParameters"/> class.
        /// </summary>
        /// <param name="varianceParameters">The observed variance parameters.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="varianceParameters"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if at least one variance parameter is negative.
        /// </exception>
        public VarianceParameters(IEnumerable<decimal> varianceParameters)
        {
            ArgumentNullException.ThrowIfNull(varianceParameters);

            decimal[] values = varianceParameters.ToArray();

            foreach (decimal value in values)
            {
                if (value < 0)
                    throw new ArgumentException("Variance must be non-negative.", nameof(varianceParameters));
            }

            _varianceParameters = values;

            DevelopmentPeriods = _varianceParameters.Length + 2;
        }

        /// <summary>
        /// Estimates the final variance parameter from the last two observed parameters.
        /// </summary>
        /// <returns>The estimated final variance parameter.</returns>
        /// <remarks>
        /// If fewer than two variance parameters are available, or if the penultimate
        /// parameter is zero, the estimate is zero.
        /// </remarks>
        public decimal EstimateLast()
        {
            decimal lastVariance;

            if (_varianceParameters.Length < 2 || _varianceParameters[^2] == decimal.Zero)
            {
                lastVariance = decimal.Zero;
            }
            else
            {
                decimal varianceEstimator = DecimalMath.DecimalPow(_varianceParameters[^1], 2) / _varianceParameters[^2];
                lastVariance = Math.Min(varianceEstimator, Math.Min(_varianceParameters[^1], _varianceParameters[^2]));
            }

            return lastVariance;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the observed variance parameters.
        /// </summary>
        public IEnumerator<decimal> GetEnumerator()
        {
            for (int i = 0; i < _varianceParameters.Length; i++)
                yield return _varianceParameters[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
