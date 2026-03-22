using ActuarialForge.Reserving.Model;
using System.Collections;

namespace ActuarialForge.Reserving.Methods
{
    /// <summary>
    /// Represents the age-to-age development factors used in the Chain Ladder method.
    /// </summary>
    /// <remarks>
    /// <see cref="ChainLadderFactors"/> stores the multiplicative age-to-age factors
    /// estimated by the Chain Ladder method and exposes the corresponding cumulative
    /// development pattern.
    /// 
    /// The enumeration of this type yields the age-to-age factors, which are the
    /// primary factors used in the Chain Ladder method.
    /// </remarks>
    public sealed record ChainLadderFactors : IRunOffFactors
    {
        private readonly List<decimal> _factors = [];

        private readonly MultiplicativeDevelopmentPattern _pattern;

        /// <summary>
        /// Gets the number of development periods implied by the factor set.
        /// </summary>
        /// <remarks>
        /// The number of development periods is one greater than the number of
        /// age-to-age factors.
        /// </remarks>
        public int DevelopmentPeriods { get; }

        /// <summary>
        /// Gets the structural interpretation of the development pattern.
        /// </summary>
        public DevelopmentPatternStructure Pattern { get => DevelopmentPatternStructure.Multiplicative; }

        /// <summary>
        /// Gets the age-to-age Chain Ladder factors.
        /// </summary>
        public IReadOnlyList<decimal> AgeToAgeFactors { get => _pattern.AgeToAgeFactors; }

        /// <summary>
        /// Gets the cumulative multiplicative development factors implied by the
        /// Chain Ladder age-to-age factors.
        /// </summary>
        /// <remarks>
        /// Each cumulative factor represents the product of all age-to-age factors
        /// up to the corresponding development period.
        /// </remarks>
        public IReadOnlyList<decimal> CumulativeFactors { get => _pattern.CumulativeFactors; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainLadderFactors"/> class.
        /// </summary>
        /// <param name="factors">The estimated Chain Ladder age-to-age factors.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="factors"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if no factors are provided or if at least one factor is less than or equal to zero.
        /// </exception>
        /// <remarks>
        /// All factors must be strictly greater than zero in order to produce a valid
        /// multiplicative development pattern and meaningful ultimate values.
        /// </remarks>
        public ChainLadderFactors(IEnumerable<decimal> factors)
        {
            ArgumentNullException.ThrowIfNull(factors);

            decimal[] values = factors.ToArray();

            if (values.Length == 0)
                throw new ArgumentException("No run off factors provided.");

            _factors = [];

            int developmentPeriodIndex = 0;

            foreach (decimal factor in values)
            {
                if (factor <= decimal.Zero)
                    throw new ArgumentException("Chain Ladder factors must be greater than zero to produce a reasonable ultimate.");

                _factors.Add(factor);

                developmentPeriodIndex++;
            }

            _pattern = new(factors);

            DevelopmentPeriods = developmentPeriodIndex + 1;
        }

        /// <summary>
        /// Gets the Chain Ladder factor associated with the specified development period.
        /// </summary>
        /// <param name="developmentPeriod">The development period.</param>
        /// <returns>
        /// The age-to-age factor from the previous development period to the specified
        /// <paramref name="developmentPeriod"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="developmentPeriod"/> does not correspond to a valid factor index.
        /// </exception>
        /// <remarks>
        /// Development period zero does not have a corresponding age-to-age factor and is therefore invalid.
        /// </remarks>
        public decimal this[DevelopmentPeriod developmentPeriod]
        {
            get
            {
                if (developmentPeriod.Lag <= 0 || developmentPeriod.Lag >= DevelopmentPeriods)
                    throw new ArgumentOutOfRangeException(nameof(developmentPeriod));

                return _factors[developmentPeriod.Lag - 1];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the Chain Ladder age-to-age factors.
        /// </summary>
        /// <returns>An enumerator over the age-to-age factors.</returns>
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
        /// Determines whether this instance is equal to another <see cref="ChainLadderFactors"/>.
        /// </summary>
        /// <param name="other">The other factor set to compare with.</param>
        /// <returns>
        /// <c>true</c> if both instances contain the same sequence of factors;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ChainLadderFactors? other)
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
