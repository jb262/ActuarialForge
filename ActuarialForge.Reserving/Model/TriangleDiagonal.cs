using ActuarialForge.Primitives;
using System.Collections;

namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Represents a diagonal of monetary values within a reserving triangle.
    /// </summary>
    /// <remarks>
    /// A <see cref="TriangleDiagonal"/> is a single-currency ordered sequence of values,
    /// typically corresponding to a diagonal across accident and development periods.
    /// All contained monetary amounts must use the same <see cref="Currency"/>.
    /// </remarks>
    public sealed record TriangleDiagonal : IEnumerable<Money>
    {
        private readonly List<Money> _diagonal;

        /// <summary>
        /// Gets the number of values contained in the diagonal.
        /// </summary>
        public int Count { get =>  _diagonal.Count; }

        /// <summary>
        /// Gets the currency of all amounts contained in the diagonal.
        /// </summary>
        public Currency Currency { get; }

        /// Gets the monetary value at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index.</param>
        /// <returns>The monetary value at the specified index.</returns>
        public Money this[int index] { get => _diagonal[index]; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriangleDiagonal"/> class.
        /// </summary>
        /// <param name="diagonal">The monetary values of the diagonal.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="diagonal"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="diagonal"/> is empty.
        /// </exception>
        /// <exception cref="CurrencyMismatchException">
        /// Thrown if the supplied values do not all have the same currency.
        /// </exception>
        public TriangleDiagonal(IEnumerable<Money> diagonal)
        {
            ArgumentNullException.ThrowIfNull(diagonal);

            Money[] values = diagonal.ToArray();

            if (values.Length == 0)
                throw new ArgumentException("No diagonal values provided.", nameof(diagonal));

            Currency = values[0].Currency;

            _diagonal = [];

            foreach (Money amount in values)
            {
                if (amount.Currency != Currency)
                    throw new CurrencyMismatchException(Currency, amount.Currency);

                _diagonal.Add(amount);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the values in the diagonal.
        /// </summary>
        public IEnumerator<Money> GetEnumerator()
            => _diagonal.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Determines whether this instance is equal to another <see cref="TriangleDiagonal"/>.
        /// </summary>
        /// <param name="other">The other diagonal to compare with.</param>
        /// <returns>
        /// <c>true</c> if both diagonals have the same currency, length, and sequence of values;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(TriangleDiagonal? other)
        {
            if (other is null) return false;

            if (Currency != other.Currency) return false;
            if (Count != other.Count) return false;

            for (int i = 0; i < Count; i++)
                if (this[i] != other[i]) return false;

            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code based on the currency and contained values.</returns>
        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Currency);

            foreach (var amount in this)
                hash.Add(amount);

            return hash.ToHashCode();
        }
    }
}
