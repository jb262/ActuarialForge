using ActuarialForge.Primitives;
using System.Collections;

namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Represents a row of monetary values within a square structure.
    /// </summary>
    /// <remarks>
    /// A <see cref="SquareRow"/> is a single-currency ordered sequence of values
    /// with no triangular truncation. All contained monetary amounts must use the same
    /// <see cref="Currency"/>.
    /// </remarks>
    public sealed record SquareRow : IEnumerable<Money>
    {
        private readonly List<Money> _row;

        /// <summary>
        /// Gets the currency of all amounts contained in the row.
        /// </summary>
        public Currency Currency { get; }

        /// <summary>
        /// Gets the number of values contained in the row.
        /// </summary>
        public int Count { get => _row.Count; }

        /// <summary>
        /// Gets the monetary value at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index.</param>
        /// <returns>The monetary value at the specified index.</returns>
        public Money this[int index] { get => _row[index]; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SquareRow"/> class.
        /// </summary>
        /// <param name="row">The monetary values of the row.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="row"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="row"/> is empty.
        /// </exception>
        /// <exception cref="CurrencyMismatchException">
        /// Thrown if the supplied values do not all have the same currency.
        /// </exception>
        public SquareRow(IEnumerable<Money> row)
        {
            ArgumentNullException.ThrowIfNull(row);

            Money[] values = row.ToArray();

            if (values.Length == 0)
                throw new ArgumentException("No row values provided.", nameof(row));

            _row = [];

            Currency = values[0].Currency;

            foreach (Money amount in values)
            {
                if (amount.Currency != Currency)
                    throw new CurrencyMismatchException(Currency, amount.Currency);

                _row.Add(amount);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the values in the row.
        /// </summary>
        public IEnumerator<Money> GetEnumerator()
            => _row.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Determines whether this instance is equal to another <see cref="SquareRow"/>.
        /// </summary>
        /// <param name="other">The other row to compare with.</param>
        /// <returns>
        /// <c>true</c> if both rows have the same currency, length, and sequence of values;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(SquareRow? other)
        {
            if (other == null) return false;
            if (Currency != other.Currency) return false;
            if (_row.Count != other._row.Count) return false;

            for (int i = 0; i < _row.Count; i++)
                if (_row[i] != other._row[i]) return false;

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

            foreach (Money amount in this)
                hash.Add(amount);

            return hash.ToHashCode();
        }
    }
}
