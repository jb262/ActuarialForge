using ActuarialForge.Reserving.Model;
using System.Collections;

namespace ActuarialForge.Reserving.Methods
{
    /// <summary>
    /// Represents the additive development factors used in additive reserving methods.
    /// </summary>
    /// <remarks>
    /// <see cref="AdditiveFactors"/> stores the additive development factors
    /// estimated by an additive reserving method and exposes the corresponding
    /// incremental-loss development pattern.
    /// 
    /// The enumeration of this type yields the additive factors, which are the
    /// primary factors used in additive development.
    /// </remarks>
    public sealed record AdditiveFactors : IRunOffFactors
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
        /// Gets the additive development factors.
        /// </summary>
        public IReadOnlyList<decimal> AgeToAgeFactors { get => _pattern.AgeToAgeFactors; }

        /// <summary>
        /// Gets the cumulative additive development factors implied by the
        /// incremental-loss pattern.
        /// </summary>
        /// <remarks>
        /// Each cumulative factor represents the cumulative sum of additive
        /// development factors up to the corresponding development period.
        /// </remarks>
        public IReadOnlyList<decimal> CumulativeFactors { get => _pattern.CumulativeFactors; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditiveFactors"/> class.
        /// </summary>
        /// <param name="factors">The additive development factors.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="factors"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if no factors are provided.
        /// </exception>
        public AdditiveFactors(IEnumerable<decimal> factors)
        {
            ArgumentNullException.ThrowIfNull(factors);

            List<decimal> values = factors.ToList();

            if (values.Count == 0)
                throw new ArgumentException("No run off factors provided.");

            _factors = values;
            DevelopmentPeriods = values.Count;

            _pattern = new(factors);
        }

        /// <summary>
        /// Gets the additive factor associated with the specified development period.
        /// </summary>
        /// <param name="developmentPeriod">The development period.</param>
        /// <returns>The additive factor for the specified development period.</returns>
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
        /// Returns an enumerator that iterates through the additive development factors.
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
        /// Determines whether this instance is equal to another <see cref="AdditiveFactors"/>.
        /// </summary>
        /// <param name="other">The other factor set to compare with.</param>
        /// <returns>
        /// <c>true</c> if both instances contain the same sequence of factors;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(AdditiveFactors? other)
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
