using ActuarialForge.Primitives;

namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Represents a cumulative loss triangle.
    /// </summary>
    /// <remarks>
    /// A <see cref="CumulativeTriangle"/> stores cumulative monetary amounts by accident
    /// period and development period. The triangle may be created from incremental rows,
    /// cumulative rows, or triangle columns.
    /// </remarks>
    public sealed record CumulativeTriangle
    {
        private readonly Money[][] _cumulativeAmounts;

        /// <summary>
        /// Gets the reserving time granularity used by this triangle.
        /// </summary>
        public ReservingTimeGranularity TimeGranularity { get; }

        /// <summary>
        /// Gets the claim date basis associated with this triangle.
        /// </summary>
        public ClaimDateBasis ClaimDateBasis { get; }

        /// <summary>
        /// Gets the number of accident periods covered by the triangle.
        /// </summary>
        public int AccidentPeriods { get; }

        /// <summary>
        /// Gets the number of development periods covered by the triangle.
        /// </summary>
        public int DevelopmentPeriods { get; }

        /// <summary>
        /// Gets the currency of all monetary values contained in the triangle.
        /// </summary>
        public Currency Currency { get; }

        /// <summary>
        /// Gets the cumulative amount for the specified accident period index and development period index.
        /// </summary>
        /// <param name="accidentPeriod">The zero-based accident period index.</param>
        /// <param name="developmentPeriod">The zero-based development period index.</param>
        /// <returns>The cumulative amount stored in the specified triangle cell.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="accidentPeriod"/> or <paramref name="developmentPeriod"/> is outside the valid range.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the specified cell lies outside the effective bounds of the triangle.
        /// </exception>
        public Money this[int accidentPeriod, int developmentPeriod]
        {
            get
            {
                if (accidentPeriod < 0 || accidentPeriod >= AccidentPeriods)
                    throw new ArgumentOutOfRangeException(nameof(accidentPeriod));

                if (developmentPeriod < 0 || developmentPeriod >= DevelopmentPeriods)
                    throw new ArgumentOutOfRangeException(nameof(developmentPeriod));

                if (developmentPeriod >= _cumulativeAmounts[accidentPeriod].Length)
                    throw new InvalidOperationException("The given cell lies outside the triangle's bounds.");

                return _cumulativeAmounts[accidentPeriod][developmentPeriod];
            }
        }

        /// <summary>
        /// Gets the cumulative amount for the specified accident period and development period.
        /// </summary>
        /// <param name="accidentPeriod">The accident period.</param>
        /// <param name="developmentPeriod">The development period.</param>
        /// <returns>The cumulative amount stored in the specified triangle cell.</returns>
        public Money this[AccidentPeriod accidentPeriod, DevelopmentPeriod developmentPeriod]
        {
            get => this[accidentPeriod.Period, developmentPeriod.Lag];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CumulativeTriangle"/> class
        /// from incremental triangle rows.
        /// </summary>
        /// <param name="incrementalRows">The incremental rows to accumulate.</param>
        /// <param name="timeGranularity">The reserving time granularity.</param>
        /// <param name="shape">The geometric shape of the triangle.</param>
        /// <param name="claimDateBasis">The claim date basis.</param>
        /// <param name="currency">The currency of the triangle.</param>
        internal CumulativeTriangle(IEnumerable<IEnumerable<Money>> incrementalRows, ReservingTimeGranularity timeGranularity, TriangleShape shape, ClaimDateBasis claimDateBasis, Currency currency)
        {
            AccidentPeriods = shape.AccidentPeriods;
            DevelopmentPeriods = shape.DevelopmentPeriods;

            TimeGranularity = timeGranularity;
            ClaimDateBasis = claimDateBasis;
            Currency = currency;

            _cumulativeAmounts = new Money[AccidentPeriods][];

            int rowIndex = 0;

            foreach (var row in incrementalRows)
            {
                _cumulativeAmounts[rowIndex] = row.Scan(Money.Zero(Currency), (x, y) => x + y).ToArray();
                rowIndex++;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CumulativeTriangle"/> class
        /// from cumulative triangle rows.
        /// </summary>
        /// <param name="cumulativeRows">The cumulative rows.</param>
        /// <param name="timeGranularity">The reserving time granularity.</param>
        /// <param name="claimDateBasis">The claim date basis.</param>
        /// <param name="currency">The currency of the triangle.</param>
        internal CumulativeTriangle(IEnumerable<TriangleRow> cumulativeRows, ReservingTimeGranularity timeGranularity, ClaimDateBasis claimDateBasis, Currency currency)
        {
            var rows = cumulativeRows.ToArray();

            AccidentPeriods = rows.Length;
            DevelopmentPeriods = rows[0].Count;

            TimeGranularity = timeGranularity;
            ClaimDateBasis = claimDateBasis;
            Currency = currency;

            _cumulativeAmounts = new Money[AccidentPeriods][];

            int rowIndex = 0;

            foreach (var row in rows)
                _cumulativeAmounts[rowIndex++] = row.ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CumulativeTriangle"/> class
        /// from triangle columns.
        /// </summary>
        /// <param name="columns">The triangle columns.</param>
        /// <param name="timeGranularity">The reserving time granularity.</param>
        /// <param name="claimDateBasis">The claim date basis.</param>
        /// <param name="currency">The currency of the triangle.</param>
        internal CumulativeTriangle(IEnumerable<TriangleColumn> columns, ReservingTimeGranularity timeGranularity, ClaimDateBasis claimDateBasis, Currency currency)
        {
            TimeGranularity = timeGranularity;
            ClaimDateBasis = claimDateBasis;
            Currency = currency;

            TriangleColumn[] columnArr = columns.ToArray();

            int accidentPeriods = columnArr[0].Count;

            _cumulativeAmounts = new Money[accidentPeriods][];

            AccidentPeriods = accidentPeriods;
            DevelopmentPeriods = columnArr.Length;

            for (int i = 0; i < accidentPeriods; i++)
            {
                int rowLength = Math.Min(columnArr.Length, accidentPeriods - i);
                _cumulativeAmounts[i] = new Money[rowLength];

                for (int j = 0; j < rowLength; j++)
                    _cumulativeAmounts[i][j] = columnArr[j][i];
            }
        }

        internal IEnumerable<Money> GetColumn(DevelopmentPeriod developmentPeriod)
            => GetColumn(developmentPeriod.Lag);

        internal IEnumerable<Money> GetColumn(int developmentPeriod)
        {
            if (developmentPeriod < 0 || developmentPeriod >= DevelopmentPeriods)
                throw new ArgumentOutOfRangeException(nameof(developmentPeriod));

            int row = 0;

            while (row < AccidentPeriods && _cumulativeAmounts[row].Length > developmentPeriod)
            {
                yield return _cumulativeAmounts[row][developmentPeriod];
                row++;
            }
        }

        /// <summary>
        /// Gets the specified development-period column as a <see cref="TriangleColumn"/>.
        /// </summary>
        /// <param name="developmentPeriod">The development period.</param>
        /// <returns>The corresponding triangle column.</returns>
        public TriangleColumn GetTriangleColumn(DevelopmentPeriod developmentPeriod)
            => new(GetColumn(developmentPeriod));

        /// <summary>
        /// Gets the specified development-period column as a <see cref="TriangleColumn"/>.
        /// </summary>
        /// <param name="developmentPeriod">The zero-based development period index.</param>
        /// <returns>The corresponding triangle column.</returns>
        public TriangleColumn GetTriangleColumn(int developmentPeriod)
            => new(GetColumn(developmentPeriod));

        internal IEnumerable<Money> GetRow(AccidentPeriod accidentPeriod)
            => GetRow(accidentPeriod.Period);

        internal IEnumerable<Money> GetRow(int accidentPeriod)
        {
            if (accidentPeriod < 0 || accidentPeriod >= AccidentPeriods)
                throw new ArgumentOutOfRangeException(nameof(accidentPeriod));

            for (int i = 0; i < _cumulativeAmounts[accidentPeriod].Length; i++)
                yield return _cumulativeAmounts[accidentPeriod][i];
        }

        /// <summary>
        /// Gets the specified accident-period row as a <see cref="TriangleRow"/>.
        /// </summary>
        /// <param name="accidentPeriod">The accident period.</param>
        /// <returns>The corresponding triangle row.</returns>
        public TriangleRow GetTriangleRow(AccidentPeriod accidentPeriod)
            => new(GetRow(accidentPeriod));

        /// <summary>
        /// Gets the specified accident-period row as a <see cref="TriangleRow"/>.
        /// </summary>
        /// <param name="accidentPeriod">The zero-based accident period index.</param>
        /// <returns>The corresponding triangle row.</returns>
        public TriangleRow GetTriangleRow(int accidentPeriod)
            => new(GetRow(accidentPeriod));

        internal IEnumerable<Money> GetDiagonal(int offset)
        {
            if (offset > 0)
                throw new ArgumentException("Cannot offset the diagonal of a triangle beyond the triangle's right boundary.", nameof(offset));

            if (-offset >= DevelopmentPeriods)
                throw new ArgumentException("Cannot offset the diagonal of a triangle beyond the triangle's left boundary.", nameof(offset));

            for (int i = 0; i < AccidentPeriods; i++)
            {
                int developmentIndex = _cumulativeAmounts[i].Length - 1 + offset;

                if (developmentIndex < 0) yield break;

                yield return _cumulativeAmounts[i][developmentIndex];
            }
        }

        internal IEnumerable<Money> GetDiagonal()
            => GetDiagonal(0);

        /// <summary>
        /// Gets a diagonal of the triangle as a <see cref="TriangleDiagonal"/>.
        /// </summary>
        /// <param name="offset">
        /// The offset relative to the latest diagonal. Zero represents the latest diagonal,
        /// and negative values represent earlier diagonals.
        /// </param>
        /// <returns>The requested triangle diagonal.</returns>
        public TriangleDiagonal GetTriangleDiagonal(int offset)
            => new(GetDiagonal(offset));

        /// <summary>
        /// Gets the latest diagonal of the triangle.
        /// </summary>
        /// <returns>The latest triangle diagonal.</returns>
        public TriangleDiagonal GetTriangleDiagonal()
            => new(GetDiagonal());

        /// <summary>
        /// Returns a string representation of the triangle using the specified separator.
        /// </summary>
        /// <param name="separator">The separator character used between values.</param>
        /// <returns>A textual representation of the triangle.</returns>
        public string ToString(char separator)
        {
            List<string> lines = [];

            for (int i = 0; i < AccidentPeriods; i++)
            {
                AccidentPeriod accidentPeriod = new(i);

                lines.Add(string.Join(separator, GetRow(accidentPeriod)));
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Returns a tab-separated string representation of the triangle.
        /// </summary>
        /// <returns>A textual representation of the triangle.</returns>
        public override string ToString()
            => ToString('\t');

        /// <summary>
        /// Determines whether this instance is equal to another <see cref="CumulativeTriangle"/>.
        /// </summary>
        /// <param name="other">The other triangle to compare with.</param>
        /// <returns>
        /// <c>true</c> if both triangles have the same metadata and cell values;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(CumulativeTriangle? other)
        {
            if (other is null) return false;

            if (TimeGranularity != other.TimeGranularity) return false;
            if (ClaimDateBasis != other.ClaimDateBasis) return false;
            if (AccidentPeriods != other.AccidentPeriods) return false;
            if (DevelopmentPeriods != other.DevelopmentPeriods) return false;
            if (Currency != other.Currency) return false;

            int rows = _cumulativeAmounts.Length;
            if (rows != other._cumulativeAmounts.Length) return false;

            for (int i = 0; i < rows; i++)
            {
                int columns = _cumulativeAmounts[i].Length;
                if (columns != other._cumulativeAmounts[i].Length) return false;

                for (int j = 0; j < columns; j++)
                    if (_cumulativeAmounts[i][j] !=  other._cumulativeAmounts[i][j]) return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code based on triangle metadata and contained cumulative amounts.</returns>
        public override int GetHashCode()
        {
            HashCode hash = new();

            hash.Add(TimeGranularity);
            hash.Add(ClaimDateBasis);
            hash.Add(AccidentPeriods);
            hash.Add(DevelopmentPeriods);
            hash.Add(Currency);

            int rows = _cumulativeAmounts.Length;
            hash.Add(rows);

            for (int i = 0; i < rows; i++)
            {
                int columns = _cumulativeAmounts[i].Length;
                hash.Add(columns);

                for (int j = 0; j < columns; j++)
                    hash.Add(_cumulativeAmounts[i][j]);
            }

            return hash.ToHashCode();
        }
    }
}
