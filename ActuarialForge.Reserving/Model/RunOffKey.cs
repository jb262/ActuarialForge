namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Represents the key of a cell in a run-off triangle.
    /// </summary>
    /// <param name="accidentPeriod">The accident period of the triangle cell.</param>
    /// <param name="developmentPeriod">The development period of the triangle cell.</param>
    /// <remarks>
    /// <see cref="RunOffKey"/> uniquely identifies a cell in a run-off triangle
    /// by combining an <see cref="AccidentPeriod"/> and a <see cref="DevelopmentPeriod"/>.
    /// </remarks>
    public readonly struct RunOffKey(AccidentPeriod accidentPeriod, DevelopmentPeriod developmentPeriod) : IComparable<RunOffKey>
    {
        /// <summary>
        /// Gets the accident period of the triangle cell.
        /// </summary>
        public AccidentPeriod AccidentPeriod { get; } = accidentPeriod;

        /// <summary>
        /// Gets the development period of the triangle cell.
        /// </summary>
        public DevelopmentPeriod DevelopmentPeriod { get; } = developmentPeriod;

        /// <summary>
        /// Compares this instance to another <see cref="RunOffKey"/>.
        /// </summary>
        /// <param name="other">The other run-off key to compare to.</param>
        /// <returns>
        /// A value less than zero if this instance precedes <paramref name="other"/>,
        /// zero if both are equal, or a value greater than zero if this instance follows it.
        /// </returns>
        /// <remarks>
        /// Keys are compared lexicographically: first by <see cref="AccidentPeriod"/>,
        /// then by <see cref="DevelopmentPeriod"/>.
        /// </remarks>
        public int CompareTo(RunOffKey other)
        {
            int comparison = AccidentPeriod.CompareTo(other.AccidentPeriod);
            if (comparison != 0) return comparison;

            return DevelopmentPeriod.CompareTo(other.DevelopmentPeriod);
        }
    }
}
