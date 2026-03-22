using ActuarialForge.Reserving.Model;
using System.Collections;

namespace ActuarialForge.Reserving.Methods
{
    /// <summary>
    /// Represents the Bornhuetter-Ferguson development factors.
    /// </summary>
    /// <remarks>
    /// <see cref="BornhuetterFergusonFactors"/> stores the cumulative proportions used
    /// in the Bornhuetter-Ferguson method and exposes the corresponding incremental-loss
    /// development pattern.
    /// 
    /// The enumeration of this type yields the primary Bornhuetter-Ferguson factors.
    /// </remarks>
    public sealed record BornhuetterFergusonFactors : IRunOffFactors
    {
        private readonly List<decimal> _factors = [];

        private readonly IncrementalLossDevelopmentPattern _pattern;

        /// <summary>
        /// Gets the number of development periods covered by the factor set.
        /// </summary>
        public int DevelopmentPeriods { get; }

        /// <summary>
        /// Gets the structural interpretation of the development pattern.
        /// </summary>
        public DevelopmentPatternStructure Pattern { get => DevelopmentPatternStructure.IncrementalLoss; }

        /// <summary>
        /// Gets the incremental-loss representation of the Bornhuetter-Ferguson factors.
        /// </summary>
        public IReadOnlyList<decimal> AgeToAgeFactors { get => _pattern.AgeToAgeFactors; }

        /// <summary>
        /// Gets the cumulative Bornhuetter-Ferguson factors.
        /// </summary>
        public IReadOnlyList<decimal> CumulativeFactors { get => _pattern.CumulativeFactors; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BornhuetterFergusonFactors"/> class.
        /// </summary>
        /// <param name="factors">The Bornhuetter-Ferguson factors.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="factors"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if no factors are provided or if at least one factor is negative.
        /// </exception>
        public BornhuetterFergusonFactors(IEnumerable<decimal> factors)
        {
            ArgumentNullException.ThrowIfNull(factors);

            decimal[] values = factors.ToArray();

            if (values.Length == 0)
                throw new InvalidOperationException("No run-off factors provided.");

            _factors = [];

            foreach (decimal factor in values)
            {
                if (factor < decimal.Zero)
                    throw new ArgumentException("A Bornhuetter Ferguson factor must be non-negative.");

                _factors.Add(factor);
            }

            var increments = values
                .Zip(values.Skip(1), (prev, next) => next - prev);

            _pattern = new(increments.Prepend(values[0]));

            DevelopmentPeriods = values.Length;
        }

        /// <summary>
        /// Derives Bornhuetter-Ferguson factors from an existing run-off factor pattern.
        /// </summary>
        /// <param name="runOffFactors">The source run-off factors.</param>
        /// <returns>The derived Bornhuetter-Ferguson factors.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="runOffFactors"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if multiplicative factors imply zero cumulative development.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown if an additive pattern implies zero total cumulative development.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown if the pattern structure is not supported.
        /// </exception>
        public static BornhuetterFergusonFactors FromRunOffFactors(IRunOffFactors runOffFactors)
        {
            ArgumentNullException.ThrowIfNull(runOffFactors);

            switch (runOffFactors.Pattern)
            {
                case DevelopmentPatternStructure.Multiplicative:
                    var bfFactors = runOffFactors.AgeToAgeFactors
                        .Reverse()
                        .Scan(decimal.One, (x, y) =>
                        {
                            if (x == decimal.Zero || y == decimal.Zero)
                                throw new InvalidOperationException("Multiplicative factors must not be zero.");

                            return x * y;
                        })
                        .Reverse()
                        .Select(f => decimal.One / f)
                        .Append(decimal.One);
                    return new(bfFactors);

                case DevelopmentPatternStructure.IncrementalLoss:
                    decimal denominator = runOffFactors.CumulativeFactors[runOffFactors.CumulativeFactors.Count - 1];
                    if (denominator == decimal.Zero)
                        throw new DivideByZeroException(nameof(denominator));
                    return new(runOffFactors.CumulativeFactors.Select(f => f / denominator));

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets the Bornhuetter-Ferguson factor associated with the specified development period.
        /// </summary>
        /// <param name="developmentPeriod">The development period.</param>
        /// <returns>The factor for the specified development period.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="developmentPeriod"/> does not correspond to a valid factor index.
        /// </exception>
        public decimal this[DevelopmentPeriod developmentPeriod]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(developmentPeriod.Lag, DevelopmentPeriods, nameof(developmentPeriod));

                return _factors[developmentPeriod.Lag];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the Bornhuetter-Ferguson factors.
        /// </summary>
        public IEnumerator<decimal> GetEnumerator()
         => _factors.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Returns a string representation of the factor set using the specified separator.
        /// </summary>
        /// <param name="separator">The separator character used between factors.</param>
        /// <returns>A textual representation of the factor set.</returns>
        public string ToString(char separator)
            => string.Join(separator, _factors);

        /// <summary>
        /// Returns a tab-separated string representation of the factor set.
        /// </summary>
        /// <returns>A textual representation of the factor set.</returns>
        public override string ToString()
            => ToString('\t');

        /// <summary>
        /// Determines whether this instance is equal to another <see cref="BornhuetterFergusonFactors"/>.
        /// </summary>
        /// <param name="other">The other factor set to compare with.</param>
        /// <returns>
        /// <c>true</c> if both instances contain the same sequence of factors;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(BornhuetterFergusonFactors? other)
        {
            if (other is null) return false;

            if (_factors.Count != other._factors.Count) return false;

            for (int i = 0; i < _factors.Count; i++)
                if (_factors[i] != other._factors[i]) return false;

            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code based on the contained factors.</returns>
        public override int GetHashCode()
        {
            HashCode hash = new();

            foreach (var factor in _factors)
                hash.Add(factor);

            return hash.ToHashCode();
        }
    }
}
