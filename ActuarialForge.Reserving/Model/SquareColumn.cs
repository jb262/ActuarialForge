using ActuarialForge.Primitives;
using System.Collections;

namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Represents a column of monetary values within a square matrix structure.
    /// </summary>
    /// <remarks>
    /// A <see cref="SquareColumn"/> is a single-currency ordered sequence of values
    /// with no triangular truncation. All contained monetary amounts must use the same
    /// <see cref="Currency"/>.
    /// </remarks>
    public sealed record SquareColumn : IEnumerable<Money>
    {
        private readonly List<Money> _column;

        /// <summary>
        /// Gets the currency of all amounts contained in the column.
        /// </summary>
        public Currency Currency { get; }

        /// <summary>
        /// Gets the number of values contained in the column.
        /// </summary>
        public int Count { get => _column.Count; }

        /// <summary>
        /// Gets the monetary value at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index.</param>
        /// <returns>The monetary value at the specified index.</returns>
        public Money this[int index] { get => _column[index]; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SquareColumn"/> class.
        /// </summary>
        /// <param name="column">The monetary values of the column.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="column"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="column"/> is empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the supplied values do not all have the same currency.
        /// </exception>
        public SquareColumn(IEnumerable<Money> column)
        {
            ArgumentNullException.ThrowIfNull(column);

            Money[] values = column.ToArray();

            if (values.Length == 0)
                throw new ArgumentException("No column values provided.", nameof(column));

            _column = [];

            Currency = values[0].Currency;

            foreach (Money amount in values)
            {
                if (amount.Currency != Currency)
                    throw new CurrencyMismatchException(Currency, amount.Currency);

                _column.Add(amount);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the values in the column.
        /// </summary>
        public IEnumerator<Money> GetEnumerator()
            => _column.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Determines whether this instance is equal to another <see cref="SquareColumn"/>.
        /// </summary>
        /// <param name="other">The other column to compare with.</param>
        /// <returns>
        /// <c>true</c> if both columns have the same currency, length, and sequence of values;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(SquareColumn? other)
        {
            if (other == null) return false;
            if (Currency  != other.Currency) return false;
            if (_column.Count != other._column.Count) return false;

            for (int i = 0; i < _column.Count; i++)
                if (_column[i] != other._column[i]) return false;

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
