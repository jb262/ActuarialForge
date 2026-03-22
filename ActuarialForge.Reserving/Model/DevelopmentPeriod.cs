namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Represents a development period (lag) within a reserving triangle.
    /// </summary>
    /// <remarks>
    /// <see cref="DevelopmentPeriod"/> identifies the development lag relative to an
    /// accident period. The lag is zero-based, where <c>0</c> represents the first
    /// development period.
    /// </remarks>
    public readonly record struct DevelopmentPeriod : IComparable<DevelopmentPeriod>
    {
        /// <summary>
        /// Gets the zero-based development lag.
        /// </summary>
        public int Lag { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DevelopmentPeriod"/> struct.
        /// </summary>
        /// <param name="lag">The zero-based development lag.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="lag"/> is negative.
        /// </exception>
        public DevelopmentPeriod(int lag)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(lag);

            Lag = lag;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DevelopmentPeriod"/> struct
        /// representing the first development period.
        /// </summary>
        public DevelopmentPeriod() : this(0) { }

        /// <summary>
        /// Compares this instance to another <see cref="DevelopmentPeriod"/>.
        /// </summary>
        /// <param name="other">The other development period.</param>
        /// <returns>
        /// A value less than zero if this instance is earlier than <paramref name="other"/>,
        /// zero if both are equal, or a value greater than zero if this instance is later.
        /// </returns>
        public int CompareTo(DevelopmentPeriod other)
            => Lag.CompareTo(other.Lag);
    }
}
