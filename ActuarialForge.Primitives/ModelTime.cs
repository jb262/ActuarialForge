namespace ActuarialForge.Primitives
{
    /// <summary>
    /// Represents a point in time on a discrete-time actuarial model timeline.
    /// </summary>
    /// <remarks>
    /// <see cref="ModelTime"/> stores time as a non-negative decimal value, typically interpreted as
    /// "years since model start" on a discrete time grid.
    /// The type provides helper methods to map between:
    /// <list type="bullet">
    /// <item><description>continuous model time and discrete period indices (given periods per year), and</description></item>
    /// <item><description>calendar dates and model time using a month-bucket mapping.</description></item>
    /// </list>
    /// The date conversion methods implement a discrete month logic: a date that falls on the first day of a month
    /// is treated as belonging to the previous month (e.g. 2026-03-31 and 2026-04-01 map to the same model period).
    /// If <c>baseDate</c> is not provided when converting from dates, the base date defaults to January 1st of the
    /// current calendar year for convenience in ad-hoc analyses.
    /// </remarks>
    public readonly record struct ModelTime : IComparable<ModelTime>
    {
        private static readonly decimal _epsilon = 1e-28m;

        /// <summary>
        /// Gets the underlying time value.
        /// </summary>
        /// <remarks>
        /// The value is non-negative and is typically interpreted as years since model start.
        /// </remarks>
        public decimal Time { get; init; }

        /// <summary>
        /// Initializes a new <see cref="ModelTime"/> instance.
        /// </summary>
        /// <param name="time">The non-negative time value.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="time"/> is negative.</exception>
        public ModelTime(decimal time)
        {
            if (decimal.IsNegative(time))
                throw new ArgumentException("Invalid model time.");

            Time = time;
        }

        /// <summary>
        /// Returns a string representation of the underlying time value.
        /// </summary>
        public override string ToString()
            => Time.ToString();

        /// <summary>
        /// Determines whether the underlying time value is (approximately) an integer.
        /// </summary>
        /// <returns><c>true</c> if the time value is close to an integer; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// The comparison uses a small tolerance value and is intended for grid-aligned model times.
        /// </remarks>
        public bool IsInteger()
            => Math.Abs(Time - Math.Round(Time)) < _epsilon;

        /// <summary>
        /// Attempts to convert this model time to an integer period index given a number of periods per year.
        /// </summary>
        /// <param name="periodsPerYear">The number of periods per year (e.g. 12 for monthly).</param>
        /// <param name="periodIndex">The resulting period index if the conversion succeeds.</param>
        /// <returns><c>true</c> if the model time corresponds to an integer period index; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="periodsPerYear"/> is less than or equal to zero.
        /// </exception>
        /// <remarks>
        /// The conversion succeeds only if <c>Time * periodsPerYear</c> is (approximately) an integer.
        /// </remarks>
        public bool TryGetPeriodIndex(int periodsPerYear, out int periodIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(periodsPerYear);

            ModelTime scaled = this with { Time = Time * periodsPerYear };

            if (scaled.IsInteger())
            {
                periodIndex = (int)Math.Round(scaled.Time);
                return true;
            }

            periodIndex = default;
            return false;
        }

        /// <summary>
        /// Converts a period index to a model time based on the specified payment frequency and timing convention.
        /// </summary>
        /// <param name="periodIndex">The period index.</param>
        /// <param name="paymentFrequency">The number of periods per year.</param>
        /// <param name="paymentTiming">Whether the timing is in advance or in arrears.</param>
        /// <returns>The corresponding <see cref="ModelTime"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="periodIndex"/> is negative.</exception>
        /// <remarks>
        /// For payments in advance, the first period is typically represented by <paramref name="periodIndex"/> = 1.
        /// For payments in arrears, <paramref name="periodIndex"/> = 0 corresponds to time 0.
        /// </remarks>
        public static ModelTime ConvertToModelTime(int periodIndex, PaymentFrequency paymentFrequency, PaymentTiming paymentTiming)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(periodIndex);

            decimal step = decimal.One / (decimal)paymentFrequency;
            decimal time = paymentTiming == PaymentTiming.Advance ?
                (periodIndex - 1) * step :
                periodIndex * step;

            return new(time);
        }

        /// /// <summary>
        /// Converts a calendar date to a model time using a discrete month-bucket mapping.
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <param name="baseDate">
        /// The model start date. If not provided, January 1st of the current calendar year is used.
        /// </param>
        /// <returns>The corresponding <see cref="ModelTime"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="date"/> is earlier than <paramref name="baseDate"/>.</exception>
        /// <remarks>
        /// The conversion computes the number of whole months between <paramref name="baseDate"/> and <paramref name="date"/>
        /// and returns <c>months / 12</c>.
        /// If <paramref name="date"/> falls on the first day of a month, it is treated as belonging to the previous month
        /// by subtracting one day before the month difference is computed. This implements a discrete month-boundary logic.
        /// </remarks>
        public static ModelTime ConvertToModelTime(DateOnly date, DateOnly? baseDate = null)
        {
            baseDate ??= new DateOnly(DateTime.Now.Year, 1, 1);

            if (date < baseDate)
                throw new ArgumentException("Date before model start.");

            if (date == baseDate)
                return new();

            if (date.Day == 1)
                date = date.AddDays(-1);

            int months = (date.Year - baseDate.Value.Year) * 12 + (date.Month - baseDate.Value.Month);

            return new(months / 12m);
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> to a model time using <see cref="ConvertToModelTime(DateOnly, DateOnly?)"/>.
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <param name="baseDate">The model start date.</param>
        /// <returns>The corresponding <see cref="ModelTime"/>.</returns>
        public static ModelTime ConvertToModelTime(DateTime date, DateTime? baseDate = null)
        {
            DateOnly dateOnly = DateOnly.FromDateTime(date);
            DateOnly? baseDateOnly = baseDate is null ? null : DateOnly.FromDateTime(baseDate.Value);

            return ConvertToModelTime(dateOnly, baseDateOnly);
        }

        /// <summary>
        /// Compares this instance to another <see cref="ModelTime"/> value.
        /// </summary>
        /// <param name="other">The other model time.</param>
        /// <returns>
        /// A value less than zero if this instance is less than <paramref name="other"/>,
        /// zero if equal, and greater than zero if greater.
        /// </returns>
        public int CompareTo(ModelTime other)
            => Time.CompareTo(other.Time);

        public static bool operator >(ModelTime left, ModelTime right)
            => left.Time > right.Time;

        public static bool operator <(ModelTime left, ModelTime right)
            => left.Time < right.Time;

        public static bool operator <=(ModelTime left, ModelTime right)
            => left.Time <= right.Time;

        public static bool operator >=(ModelTime left, ModelTime right)
            => left.Time >= right.Time;

        public static ModelTime operator +(ModelTime left, ModelTime right)
            => left with { Time = left.Time + right.Time };

        public static ModelTime operator -(ModelTime left, ModelTime right)
        {
            if (right > left)
                throw new ArgumentException("Subtraction would result in negative model time.");

            return left with { Time = left.Time - right.Time };
        }

        public static ModelTime operator +(ModelTime time, decimal value)
            => time with { Time = time.Time + value };

        public static ModelTime operator -(ModelTime time, decimal value)
        {
            if (value >  time.Time)
                throw new ArgumentException("Subtraction would result in negative model time.");

            return time with { Time = time.Time - value };
        }
    }
}
