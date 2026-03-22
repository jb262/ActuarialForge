namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Represents the geometric bounds of a reserving triangle.
    /// </summary>
    /// <remarks>
    /// <see cref="TriangleShape"/> defines the valid accident and development period ranges
    /// for a triangle and provides helper methods for checking whether a given cell lies
    /// within those bounds.
    /// <para>
    /// The <c>triangleOffset</c> parameter allows the shape to represent triangles that are
    /// not perfectly square, for example when the triangle contains more development periods
    /// than accident periods and some older accident periods are already fully developed.
    /// </para>
    /// </remarks>
    internal readonly record struct TriangleShape
    {
        /// <summary>
        /// Gets the minimum accident period index included in the triangle.
        /// </summary>
        public int MinAccidentPeriod { get; }

        /// <summary>
        /// Gets the maximum accident period index included in the triangle.
        /// </summary>
        public int MaxAccidentPeriod { get; }

        /// <summary>
        /// Gets the minimum development period index included in the triangle.
        /// </summary>
        public int MinDevelopmentPeriod { get; }

        /// <summary>
        /// Gets the maximum development period index included in the triangle.
        /// </summary>
        public int MaxDevelopmentPeriod { get; }

        /// <summary>
        /// Gets the number of accident periods covered by the triangle.
        /// </summary>
        public int AccidentPeriods { get => MaxAccidentPeriod - MinAccidentPeriod + 1; }

        /// <summary>
        /// Gets the number of development periods covered by the triangle.
        /// </summary>
        public int DevelopmentPeriods { get => MaxDevelopmentPeriod - MinDevelopmentPeriod + 1; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriangleShape"/> struct.
        /// </summary>
        /// <param name="minAccidentPeriod">The minimum accident period index.</param>
        /// <param name="maxAccidentPeriod">The maximum accident period index.</param>
        /// <param name="minDevelopmentPeriod">The minimum development period index.</param>
        /// <param name="maxDevelopmentPeriod">The maximum development period index.</param>
        /// <param name="triangleOffset">
        /// An offset used to extend the maximum development period beyond the accident period range,
        /// allowing representation of non-square or skewed triangles.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified bounds are inconsistent or if any supplied value is negative.
        /// </exception>
        public TriangleShape(int minAccidentPeriod, int maxAccidentPeriod, int minDevelopmentPeriod, int maxDevelopmentPeriod, int triangleOffset)
        {
            if (minAccidentPeriod > maxAccidentPeriod)
                throw new ArgumentOutOfRangeException(nameof(minAccidentPeriod), "The minimum accident period cannot be larger than the maximum accident period.");

            if (minDevelopmentPeriod > maxDevelopmentPeriod)
                throw new ArgumentOutOfRangeException(nameof(minDevelopmentPeriod),"The minimum development period cannot be larger than the maximum development period.");

            ArgumentOutOfRangeException.ThrowIfNegative(minAccidentPeriod);
            ArgumentOutOfRangeException.ThrowIfNegative(maxAccidentPeriod);
            ArgumentOutOfRangeException.ThrowIfNegative(minDevelopmentPeriod);
            ArgumentOutOfRangeException.ThrowIfNegative(maxDevelopmentPeriod);
            ArgumentOutOfRangeException.ThrowIfNegative(triangleOffset);

            MinAccidentPeriod = minAccidentPeriod;
            MinDevelopmentPeriod = minDevelopmentPeriod;

            MaxAccidentPeriod = maxAccidentPeriod;

            MaxDevelopmentPeriod = Math.Max(maxDevelopmentPeriod, maxAccidentPeriod + triangleOffset);
        }

        /// <summary>
        /// Gets the maximum valid development period for the specified accident period.
        /// </summary>
        /// <param name="accidentPeriod">The accident period.</param>
        /// <returns>
        /// The maximum development period index that is valid for the specified accident period.
        /// </returns>
        public int MaxDevelopmentPeriodForAccidentPeriod(AccidentPeriod accidentPeriod)
            => MaxDevelopmentPeriod - (accidentPeriod.Period - MinAccidentPeriod);

        /// <summary>
        /// Determines whether the specified triangle cell lies within the shape.
        /// </summary>
        /// <param name="accidentPeriod">The accident period of the cell.</param>
        /// <param name="developmentPeriod">The development period of the cell.</param>
        /// <returns>
        /// <c>true</c> if the specified cell is contained within the triangle shape;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(AccidentPeriod accidentPeriod, DevelopmentPeriod developmentPeriod)
        {
            if (accidentPeriod.Period < MinAccidentPeriod || accidentPeriod.Period > MaxAccidentPeriod)
                return false;

            return developmentPeriod.Lag >= MinDevelopmentPeriod && developmentPeriod.Lag <= MaxDevelopmentPeriodForAccidentPeriod(accidentPeriod);
        }
    }
}
