namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Represents the index of an accident period within a reserving triangle.
    /// </summary>
    /// <remarks>
    /// <see cref="AccidentPeriod"/> is a lightweight value type used to identify
    /// accident periods in a type-safe manner. The period index is zero-based.
    /// </remarks>
    public readonly record struct AccidentPeriod : IComparable<AccidentPeriod>
    {
        /// <summary>
        /// Gets the zero-based index of the accident period.
        /// </summary>
        public int Period { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccidentPeriod"/> struct.
        /// </summary>
        /// <param name="periodIndex">The zero-based accident period index.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="periodIndex"/> is negative.
        /// </exception>
        public AccidentPeriod(int periodIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(periodIndex);

            Period = periodIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccidentPeriod"/> struct
        /// representing the first accident period.
        /// </summary>
        public AccidentPeriod() : this(0) { }

        /// <summary>
        /// Compares this instance to another <see cref="AccidentPeriod"/>.
        /// </summary>
        /// <param name="other">The other accident period to compare to.</param>
        /// <returns>
        /// A value less than zero if this instance is earlier than <paramref name="other"/>,
        /// zero if both are equal, or a value greater than zero if this instance is later.
        /// </returns>
        public int CompareTo(AccidentPeriod other)
            => Period.CompareTo(other.Period);
    }
}
