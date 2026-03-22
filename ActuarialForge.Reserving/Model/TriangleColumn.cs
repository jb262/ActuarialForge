using ActuarialForge.Primitives;
using System.Collections;

namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Represents a column of monetary values within a reserving triangle.
    /// </summary>
    /// <remarks>
    /// A <see cref="TriangleColumn"/> is a single-currency ordered sequence of values,
    /// typically corresponding to one development period across multiple accident periods.
    /// All contained monetary amounts must use the same <see cref="Currency"/>.
    /// </remarks>
    public sealed record TriangleColumn : IEnumerable<Money>
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
        /// Initializes a new instance of the <see cref="TriangleColumn"/> class.
        /// </summary>
        /// <param name="column">The monetary values of the column.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="column"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="column"/> is empty.
        /// </exception>
        /// <exception cref="CurrencyMismatchException">
        /// Thrown if the supplied values do not all have the same currency.
        /// </exception>
        public TriangleColumn(IEnumerable<Money> column)
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
        /// Returns a new column with the specified values appended to the end.
        /// </summary>
        /// <param name="values">The values to append.</param>
        /// <returns>
        /// A new <see cref="TriangleColumn"/> containing the existing values followed by the appended values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="values"/> is <c>null</c>.
        /// </exception>
        public TriangleColumn Append(IEnumerable<Money> values)
        {
            ArgumentNullException.ThrowIfNull(values);

            return new(_column.Concat(values));
        }

        /// <summary>
        /// Returns the last <paramref name="accidentPeriods"/> values of the column.
        /// </summary>
        /// <param name="accidentPeriods">The number of values to take from the end of the column.</param>
        /// <returns>An enumerable containing the selected values.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="accidentPeriods"/> exceeds the number of values in the column.
        /// </exception>
        public IEnumerable<Money> TakeLast(int accidentPeriods)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(accidentPeriods, Count);

            return _column.Skip(Count - accidentPeriods).Take(accidentPeriods);
        }

        /// <summary>
        /// Returns the last <paramref name="accidentPeriods"/> values of the column,
        /// each scaled by the specified factor.
        /// </summary>
        /// <param name="accidentPeriods">The number of values to take from the end of the column.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>An enumerable containing the scaled values.</returns>
        public IEnumerable<Money> TakeLastScaled(int accidentPeriods, decimal factor)
            => TakeLast(accidentPeriods).Select(c => c * factor);

        /// <summary>
        /// Returns the last <paramref name="accidentPeriods"/> values of the column,
        /// each incremented by the specified monetary amount.
        /// </summary>
        /// <param name="accidentPeriods">The number of values to take from the end of the column.</param>
        /// <param name="increment">The monetary increment to add to each selected value.</param>
        /// <returns>An enumerable containing the incremented values.</returns>
        public IEnumerable<Money> TakeLastAdded(int accidentPeriods, Money increment)
            => TakeLast(accidentPeriods).Select(c => c + increment);

        /// <summary>
        /// Determines whether this instance is equal to another <see cref="TriangleColumn"/>.
        /// </summary>
        /// <param name="other">The other column to compare with.</param>
        /// <returns>
        /// <c>true</c> if both columns have the same currency, length, and sequence of values;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(TriangleColumn? other)
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
