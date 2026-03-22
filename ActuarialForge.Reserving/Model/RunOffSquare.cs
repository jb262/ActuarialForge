using ActuarialForge.Primitives;

namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Represents a full run-off square of cumulative monetary amounts.
    /// </summary>
    /// <remarks>
    /// A <see cref="RunOffSquare"/> stores a square development structure by development-period columns.
    /// It supports extraction of rows, columns, diagonals, conversion to a cumulative triangle,
    /// and calculation of outstanding reserve cashflows.
    /// </remarks>
    public sealed record RunOffSquare
    {

        private readonly List<SquareColumn> _claimRunOff;

        private readonly TimeAxis _timeAxis;

        /// <summary>
        /// Gets the reserving time granularity used by this run-off square.
        /// </summary>
        public ReservingTimeGranularity TimeGranularity { get; }

        /// <summary>
        /// Gets the claim date basis associated with this run-off square.
        /// </summary>
        public ClaimDateBasis ClaimDateBasis { get; }

        /// <summary>
        /// Gets the number of accident periods represented in the square.
        /// </summary>
        public int AccidentPeriods { get; }

        /// <summary>
        /// Gets the number of development periods represented in the square.
        /// </summary>
        public int DevelopmentPeriods { get => _claimRunOff.Count; }

        /// <summary>
        /// Gets the currency of all values contained in the square.
        /// </summary>
        public Currency Currency { get; }

        /// <summary>
        /// Gets the monetary value at the specified accident and development period.
        /// </summary>
        /// <param name="accidentPeriod">The accident period.</param>
        /// <param name="developmentPeriod">The development period.</param>
        /// <returns>The monetary value at the specified cell.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified accident or development period lies outside the square bounds.
        /// </exception>
        public Money this[AccidentPeriod accidentPeriod, DevelopmentPeriod developmentPeriod]
        {
            get
            {
                if (accidentPeriod.Period >= AccidentPeriods)
                    throw new ArgumentOutOfRangeException(nameof(accidentPeriod));

                if (developmentPeriod.Lag >= DevelopmentPeriods)
                    throw new ArgumentOutOfRangeException(nameof(developmentPeriod));

                return _claimRunOff[developmentPeriod.Lag][accidentPeriod.Period];
            }
        }

        /// <summary>
        /// Gets the monetary value at the specified accident and development period indices.
        /// </summary>
        /// <param name="accidentPeriod">The zero-based accident period index.</param>
        /// <param name="developmentPeriod">The zero-based development period index.</param>
        /// <returns>The monetary value at the specified cell.</returns>
        public Money this[int accidentPeriod, int developmentPeriod]
        {
            get => this[new AccidentPeriod(accidentPeriod), new DevelopmentPeriod(developmentPeriod)];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunOffSquare"/> class.
        /// </summary>
        /// <param name="columns">The development-period columns of the square.</param>
        /// <param name="timeGranularity">The reserving time granularity.</param>
        /// <param name="claimDateBasis">The claim date basis associated with the square.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="columns"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if no run-off data is provided.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the supplied columns do not all have the same length.
        /// </exception>
        /// <exception cref="CurrencyMismatchException">
        /// Thrown if the supplied columns do not all have the same currency.
        /// </exception>
        internal RunOffSquare(IEnumerable<SquareColumn> columns, ReservingTimeGranularity timeGranularity, ClaimDateBasis claimDateBasis)
        {
            ArgumentNullException.ThrowIfNull(columns);

            SquareColumn[] squareColumns = columns.ToArray();

            TimeGranularity = timeGranularity;
            ClaimDateBasis = claimDateBasis;

            _timeAxis = new(timeGranularity);

            if (squareColumns.Length == 0)
                throw new ArgumentException("No run off data provided.");

            SquareColumn firstColumn = squareColumns[0];

            Currency = firstColumn.Currency;
            AccidentPeriods = firstColumn.Count;

            _claimRunOff = [];

            foreach (SquareColumn column in squareColumns)
            {
                if (column.Count != AccidentPeriods)
                    throw new InvalidOperationException("Cannot add columns with different lengths to a run off square.");

                if (column.Currency != Currency)
                    throw new CurrencyMismatchException(Currency, column.Currency);

                _claimRunOff.Add(column);
            }
        }

        /// <summary>
        /// Extracts the corresponding cumulative triangle from the run-off square.
        /// </summary>
        /// <returns>A cumulative triangle containing the upper-left triangle of the square.</returns>
        public CumulativeTriangle ExtractTriangle()
        {
            TriangleRow[] rows = new TriangleRow[AccidentPeriods];

            for (int i = 0; i < rows.Length; i++)
                rows[i] = new(GetRow(i));

            return new(rows, TimeGranularity, ClaimDateBasis, Currency);
        }


        internal IEnumerable<Money> GetColumn(int developmentPeriod)
            => _claimRunOff[developmentPeriod];

        internal IEnumerable<Money> GetColumn(DevelopmentPeriod developmentPeriod)
            => GetColumn(developmentPeriod.Lag);

        /// <summary>
        /// Gets the specified development-period column as a <see cref="SquareColumn"/>.
        /// </summary>
        /// <param name="developmentPeriod">The zero-based development period index.</param>
        /// <returns>The corresponding square column.</returns>
        public SquareColumn GetSquareColumn(int developmentPeriod)
            => _claimRunOff[developmentPeriod];

        /// <summary>
        /// Gets the specified development-period column as a <see cref="SquareColumn"/>.
        /// </summary>
        /// <param name="developmentPeriod">The development period.</param>
        /// <returns>The corresponding square column.</returns>
        public SquareColumn GetSquareColumn(DevelopmentPeriod developmentPeriod)
            => GetSquareColumn(developmentPeriod.Lag);

        internal IEnumerable<Money> GetDiagonal(int offset)
        {
            int discardRows = Math.Max(0, AccidentPeriods - DevelopmentPeriods);

            if (offset >= AccidentPeriods - discardRows || offset <= -DevelopmentPeriods)
                throw new ArgumentOutOfRangeException(nameof(offset));

            int columnIndex;
            int rowStart;

            if (offset < 0)
            {
                columnIndex = DevelopmentPeriods + offset;
                rowStart = discardRows;

            }
            else
            {
                columnIndex = DevelopmentPeriods;
                rowStart = discardRows + offset;
            }

            for (int rowIndex = rowStart; rowIndex < AccidentPeriods; rowIndex++)
                yield return _claimRunOff[--columnIndex][rowIndex];
        }

        internal IEnumerable<Money> GetDiagonal()
            => GetDiagonal(0);

        /// <summary>
        /// Gets a diagonal of the run-off square as a <see cref="SquareDiagonal"/>.
        /// </summary>
        /// <param name="offset">
        /// The offset relative to the latest diagonal. Zero represents the latest diagonal,
        /// negative values represent earlier diagonals.
        /// </param>
        /// <returns>The requested square diagonal.</returns>
        public SquareDiagonal GetSquareDiagonal(int offset)
            => new(GetDiagonal(offset));

        /// <summary>
        /// Gets the latest diagonal of the run-off square.
        /// </summary>
        /// <returns>The latest square diagonal.</returns>
        public SquareDiagonal GetSquareDiagonal()
            => new(GetDiagonal());

        internal IEnumerable<Money> GetRow(int accidentPeriod)
        {
            for (int i = 0; i < DevelopmentPeriods; i++)
                yield return _claimRunOff[i][accidentPeriod];
        }

        internal IEnumerable<Money> GetRow(AccidentPeriod accidentPeriod)
            => GetRow(accidentPeriod.Period);

        /// <summary>
        /// Gets the specified accident-period row as a <see cref="SquareRow"/>.
        /// </summary>
        /// <param name="accidentPeriod">The accident period.</param>
        /// <returns>The corresponding square row.</returns>
        public SquareRow GetSquareRow(AccidentPeriod accidentPeriod)
            => new(GetRow(accidentPeriod));

        /// <summary>
        /// Gets the specified accident-period row as a <see cref="SquareRow"/>.
        /// </summary>
        /// <param name="accidentPeriod">The zero-based accident period index.</param>
        /// <returns>The corresponding square row.</returns>
        public SquareRow GetSquareRow(int accidentPeriod)
            => new(GetRow(accidentPeriod));

        /// <summary>
        /// Gets the ultimate values represented by the last development-period column.
        /// </summary>
        /// <returns>The ultimate values of the run-off square.</returns>
        public IEnumerable<Money> GetUltimates()
            => _claimRunOff[DevelopmentPeriods - 1];

        private ModelTime ComputeModelTime(PaymentTiming paymentTiming, int accidentPeriod, int developmentPeriod)
        {
            if (paymentTiming == PaymentTiming.Advance)
                return _timeAxis.GetStartTime(new(accidentPeriod + developmentPeriod));
            else
                return _timeAxis.GetEndTime(new(accidentPeriod + developmentPeriod));
        }

        /// <summary>
        /// Calculates the outstanding reserve cashflows implied by the run-off square.
        /// </summary>
        /// <param name="paymentTiming">
        /// The timing assumption used to assign the reserve cashflow to model time.
        /// </param>
        /// <returns>A cashflow representing the outstanding reserves.</returns>
        public Cashflow CalculateOutstandingReserves(PaymentTiming paymentTiming = PaymentTiming.Arrears)
        {
            int discardRows = Math.Max(0, AccidentPeriods - DevelopmentPeriods);

            var outstandingReserves = GetUltimates()
                .Skip(discardRows)
                .Zip(GetDiagonal(), (u, d) => u - d);

            int accidentPeriod = discardRows;
            List<CashflowItem> items = [];

            foreach (Money outstandingReserve in outstandingReserves)
            {
                ModelTime time = ComputeModelTime(paymentTiming, accidentPeriod, DevelopmentPeriods - 1);

                items.Add(new(outstandingReserve, time));
                accidentPeriod++;
            }

            return new(items);
        }

        /// <summary>
        /// Returns a string representation of the run-off square using the specified separator.
        /// </summary>
        /// <param name="separator">The separator character used between values.</param>
        /// <returns>A textual representation of the run-off square.</returns>
        public string ToString(char separator)
        {
            List<string> lines = [];

            for (int i = 0; i < AccidentPeriods; i++)
                lines.Add(string.Join(separator, GetRow(i)));

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Returns a tab-separated string representation of the run-off square.
        /// </summary>
        /// <returns>A textual representation of the run-off square.</returns>
        public override string ToString()
            => ToString('\t');

        /// <summary>
        /// Determines whether this instance is equal to another <see cref="RunOffSquare"/>.
        /// </summary>
        /// <param name="other">The other run-off square to compare with.</param>
        /// <returns>
        /// <c>true</c> if both run-off squares have the same metadata and columns;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(RunOffSquare? other)
        {
            if (other is null) return false;
            if (TimeGranularity != other.TimeGranularity) return false;
            if (ClaimDateBasis != other.ClaimDateBasis) return false;
            if (Currency != other.Currency) return false;
            if (DevelopmentPeriods != other.DevelopmentPeriods) return false;

            for (int i = 0; i < DevelopmentPeriods; i++)
                if (_claimRunOff[i] != other._claimRunOff[i]) return false;

            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code based on metadata and contained columns.</returns>
        public override int GetHashCode()
        {
            HashCode hash = new();

            hash.Add(TimeGranularity);
            hash.Add(ClaimDateBasis);
            hash.Add(Currency);

            foreach (var column in _claimRunOff)
                hash.Add(column);

            return hash.ToHashCode();
        }
    }
}
