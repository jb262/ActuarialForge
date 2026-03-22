using ActuarialForge.Primitives;

namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Provides conversions between model time and reserving-specific accident and development periods.
    /// </summary>
    /// <param name="timeGranularity">
    /// The reserving time granularity defining the number of periods per year.
    /// </param>
    /// <remarks>
    /// <see cref="TimeAxis"/> translates <see cref="ModelTime"/> values into
    /// <see cref="AccidentPeriod"/> and <see cref="DevelopmentPeriod"/> instances
    /// based on the configured <see cref="ReservingTimeGranularity"/>.
    /// </remarks>
    internal sealed class TimeAxis(ReservingTimeGranularity timeGranularity)
    {
        private readonly int _periodsPerYear = (int)timeGranularity;

        /// <summary>
        /// Gets the reserving time granularity used by this time axis.
        /// </summary>
        public ReservingTimeGranularity TimeGranularity { get; } = timeGranularity;

        /// <summary>
        /// Gets the origin of the time axis.
        /// </summary>
        /// <remarks>
        /// The base time is currently fixed at zero.
        /// </remarks>
        public ModelTime BaseTime { get; } = new();

        /// <summary>
        /// Gets the accident period containing the specified model time.
        /// </summary>
        /// <param name="time">The model time.</param>
        /// <returns>The corresponding accident period.</returns>
        public AccidentPeriod GetAccidentPeriod(ModelTime time)
        {
            int fullPeriods = (int)Math.Floor(time.Time);
            decimal fraction = time.Time - fullPeriods;

            int periodIndex = fullPeriods * _periodsPerYear + (int)Math.Floor(fraction * _periodsPerYear);

            return new(periodIndex);
        }

        /// <summary>
        /// Gets the development period between an accident time and a development time.
        /// </summary>
        /// <param name="accidentTime">The accident time.</param>
        /// <param name="developmentTime">The development time.</param>
        /// <returns>
        /// The development period representing the lag between the accident time and the development time.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="developmentTime"/> is earlier than <paramref name="accidentTime"/>.
        /// </exception>
        public DevelopmentPeriod GetDevelopmentPeriod(ModelTime accidentTime, ModelTime developmentTime)
        {
            int accidentPeriodIndex = GetAbsolutePeriodIndex(accidentTime);
            int developmentPeriodIndex = GetAbsolutePeriodIndex(developmentTime);

            if (developmentPeriodIndex < accidentPeriodIndex)
                throw new ArgumentException(
                    "The development time cannot be earlier than the accident time.",
                    nameof(developmentTime));

            return new DevelopmentPeriod(developmentPeriodIndex - accidentPeriodIndex);
        }

        private int GetAbsolutePeriodIndex(ModelTime time)
        {
            int fullPeriods = (int)Math.Floor(time.Time);
            decimal fraction = time.Time - fullPeriods;

            int periodIndex = (int)Math.Floor(fraction * _periodsPerYear);

            return fullPeriods * _periodsPerYear + periodIndex;
        }

        /// <summary>
        /// Gets the start time of the specified accident period.
        /// </summary>
        /// <param name="period">The accident period.</param>
        /// <returns>The start time of the accident period.</returns>
        public ModelTime GetStartTime(AccidentPeriod period)
            => new(period.Period / (decimal)_periodsPerYear);

        /// <summary>
        /// Gets the end time of the specified accident period.
        /// </summary>
        /// <param name="period">The accident period.</param>
        /// <returns>The end time of the accident period.</returns>
        public ModelTime GetEndTime(AccidentPeriod period)
            => new((period.Period + 1) / (decimal)_periodsPerYear);
    }
}
