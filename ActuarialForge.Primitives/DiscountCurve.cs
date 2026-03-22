using System.Collections;
using ActuarialForge.Utils;

namespace ActuarialForge.Primitives
{
    /// <summary>
    /// Represents a discrete-time discount curve defined by interest rates on a regular time grid.
    /// </summary>
    /// <remarks>
    /// The curve is parameterized by a <see cref="PaymentFrequency"/> (periods per year), a <see cref="DiscountConvention"/>
    /// (beginning/mid/end of period), and a <see cref="RateType"/> (spot or forward rates).
    ///
    /// Interest rates are stored on a base grid with times <c>t = i / m</c> (in years), where <c>m</c> is the number of
    /// periods per year and <c>i = 0..n-1</c>.
    ///
    /// This is a discrete model. Unless you add interpolation, discount factors can only be obtained for model times
    /// that correspond to stored grid points.
    /// </remarks>
    public sealed class DiscountCurve : IEnumerable<DiscountFactor>
    {
        private readonly SortedList<ModelTime, decimal> _interestRates;
        private readonly int _periodsPerYear;

        /// <summary>
        /// Gets the payment frequency (periods per year) used by this curve.
        /// </summary>
        public PaymentFrequency PaymentFrequency { get; init; }

        /// <summary>
        /// Gets the discount convention used to shift times within a period.
        /// </summary>
        /// <remarks>
        /// The convention is applied by converting a base grid time into an effective time within the period
        /// (e.g. beginning, mid, or end of period).
        /// </remarks>
        public DiscountConvention DiscountConvention { get; init; }

        /// <summary>
        /// Gets the type of interest rates stored in this curve (spot or forward rates).
        /// </summary>
        public RateType RateType { get; init; }

        /// <summary>
        /// Gets the number of rate points stored in this curve.
        /// </summary>
        public int Count { get => _interestRates.Count; }

        /// <summary>
        /// Initializes a new <see cref="DiscountCurve"/> from a sequence of interest rates and curve settings.
        /// </summary>
        /// <param name="interestRates">The sequence of interest rates on the curve grid, starting at time 0.</param>
        /// <param name="settings">Curve settings describing the timing convention and rate interpretation.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="interestRates"/> or <paramref name="settings"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="interestRates"/> is empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the payment frequency in <paramref name="settings"/> is invalid.</exception>
        public DiscountCurve(IEnumerable<decimal> interestRates, DiscountCurveSettings settings)
        {
            ArgumentNullException.ThrowIfNull(interestRates);
            ArgumentNullException.ThrowIfNull(settings);

            DiscountConvention = settings.DiscountConvention;
            RateType = settings.RateType;
            PaymentFrequency = settings.PaymentFrequency;

            _periodsPerYear = (int)settings.PaymentFrequency;

            if (_periodsPerYear <= 0)
                throw new InvalidOperationException();

            List<decimal> interestRatesList = interestRates.ToList();

            if (interestRatesList.Count == 0)
                throw new ArgumentException("Interest rates must not be empty.", nameof(interestRates));

            decimal step = decimal.One / _periodsPerYear;
            _interestRates = [];

            for (int i = 0; i < interestRatesList.Count; i++)
            {
                ModelTime time = new(i * step);
                _interestRates[time] = interestRatesList[i];
            }
        }

        /// <summary>
        /// Gets the discount factor applicable at the specified model time.
        /// </summary>
        /// <param name="time">The model time.</param>
        /// <returns>A <see cref="DiscountFactor"/> for the specified time.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the curve does not contain a rate for the specified <paramref name="time"/>.
        /// </exception>
        public DiscountFactor GetDiscountFactor(ModelTime time)
        { 
            decimal factor = ComputeDiscountFactor(time);

            return new(factor, time);
        }

        /// <summary>
        /// Computes the effective model time within a period according to <see cref="DiscountConvention"/>.
        /// </summary>
        /// <param name="baseTime">The base time on the period grid.</param>
        /// <returns>The shifted effective time.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the discount convention is not supported.</exception>
        private ModelTime ComputeEffectiveModelTime(ModelTime baseTime)
        {
            decimal shift = DiscountConvention switch
            {
                DiscountConvention.BeginningOfPeriod => decimal.Zero,
                DiscountConvention.MidPeriod => 0.5m,
                DiscountConvention.EndOfPeriod => decimal.One,
                _ => throw new InvalidOperationException()
            };

            decimal delta = shift / _periodsPerYear;

            return new(baseTime.Time + delta);
        }

        /// <summary>
        /// Computes the discount factor value for the specified time according to <see cref="RateType"/>.
        /// </summary>
        /// <param name="time">The model time.</param>
        /// <returns>The discount factor value.</returns>
        private decimal ComputeDiscountFactor(ModelTime time)
        {
            return RateType switch
            {
                RateType.Spot => ComputeSpotDiscountFactor(time),
                RateType.Forward => ComputeForwardDiscountFactor(time),
                _ => throw new NotSupportedException("Unsupported rate type provided."),
            };
        }

        /// <summary>
        /// Computes the discount factor from a spot rate at the specified grid time.
        /// </summary>
        /// <param name="time">A base grid time for which a spot rate exists.</param>
        /// <returns>The discount factor for the specified time.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the spot rate for <paramref name="time"/> is missing.</exception>
        /// <remarks>
        /// This implementation applies the <see cref="DiscountConvention"/> by using the convention-shifted effective time
        /// in the exponent.
        /// </remarks>
        private decimal ComputeSpotDiscountFactor(ModelTime time)
        { 
            if (!_interestRates.TryGetValue(time, out decimal spotRate))
                throw new InvalidOperationException("Spot rate missing.");

            return DecimalMath.DecimalPow(decimal.One + spotRate, -ComputeEffectiveModelTime(time).Time);
        }

        /// <summary>
        /// Computes the discount factor from a curve of forward rates on a regular grid.
        /// </summary>
        /// <param name="time">The base model time at which the discount factor is requested.</param>
        /// <returns>The discount factor for the specified time.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the requested time exceeds the curve horizon.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a required forward rate is missing on the grid.
        /// </exception>
        /// <remarks>
        /// The effective time (according to <see cref="DiscountConvention"/>) is converted into a number of periods.
        /// Full periods are discounted by multiplying per-period factors using the forward rates.
        /// If the time falls into a fractional last period, the last period factor is applied with a fractional exponent.
        /// </remarks>
        private decimal ComputeForwardDiscountFactor(ModelTime time)
        {
            decimal effectiveTime = ComputeEffectiveModelTime(time).Time;

            if (effectiveTime <= decimal.Zero)
                return decimal.One;

            var lastKey = _interestRates.Keys[^1];
            decimal maxEffectiveTime = ComputeEffectiveModelTime(lastKey).Time;

            if (effectiveTime > maxEffectiveTime)
                throw new ArgumentOutOfRangeException(nameof(time), $"Time {time} (effective {effectiveTime}) exceeds curve horizon {lastKey} (effective {maxEffectiveTime}).");

            decimal scaled = effectiveTime * _periodsPerYear;
            int fullPeriods = (int)decimal.Floor(scaled);
            decimal fraction = scaled - fullPeriods;

            decimal df = decimal.One;

            for (int k = 1; k <= fullPeriods; k++)
            {
                ModelTime t = new(k / (decimal)_periodsPerYear);

                if (!_interestRates.TryGetValue(t, out decimal forward))
                    throw new InvalidOperationException($"Forward rate missing for period end time {t}.");

                df /= (decimal.One + forward / _periodsPerYear);
            }

            if (fraction > decimal.Zero)
            {
                int k = fullPeriods + 1;
                ModelTime t = new(k / (decimal)_periodsPerYear);

                if (!_interestRates.TryGetValue(t, out decimal forward))
                    throw new InvalidOperationException($"Forward rate missing for period end time {t}.");

                df /= DecimalMath.DecimalPow(decimal.One + forward / _periodsPerYear, fraction);
            }

            return df;
        }

        /// <summary>
        /// Returns an enumerator that iterates through discount factors in ascending time order.
        /// </summary>
        /// <returns>An enumerator of <see cref="DiscountFactor"/> instances.</returns>
        /// <remarks>
        /// Enumerating the curve yields discount factors at each stored grid time.
        /// </remarks>
        public IEnumerator<DiscountFactor> GetEnumerator()
        {
            foreach (ModelTime time in _interestRates.Keys)
                yield return GetDiscountFactor(time);
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
