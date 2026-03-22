using ActuarialForge.Primitives;
using System.Collections;

namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Represents a row of monetary values within a reserving triangle.
    /// </summary>
    /// <remarks>
    /// A <see cref="TriangleRow"/> is a single-currency ordered sequence of values,
    /// typically corresponding to one accident period across multiple development periods.
    /// All contained monetary amounts must use the same <see cref="Currency"/>.
    /// </remarks>
    public sealed record TriangleRow : IEnumerable<Money>
    {
        private readonly List<Money> _row;

        /// <summary>
        /// Gets the number of values contained in the row.
        /// </summary>
        public int Count { get => _row.Count; }


        /// <summary>
        /// Gets the currency of all amounts contained in the row.
        /// </summary>
        public Currency Currency { get; }

        /// <summary>
        /// Gets the monetary value at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index.</param>
        /// <returns>The monetary value at the specified index.</returns>
        public Money this[int index] { get => _row[index]; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriangleRow"/> class.
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
        public TriangleRow(IEnumerable<Money> row)
        {
            ArgumentNullException.ThrowIfNull(row);

            Money[] values = row.ToArray();

            if (values.Length == 0)
                throw new ArgumentException("No row values provided.", nameof(row));

            Currency = values[0].Currency;

            _row = [];

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
        /// Determines whether this instance is equal to another <see cref="TriangleRow"/>.
        /// </summary>
        /// <param name="other">The other row to compare with.</param>
        /// <returns>
        /// <c>true</c> if both rows have the same currency, length, and sequence of values;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(TriangleRow? other)
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
