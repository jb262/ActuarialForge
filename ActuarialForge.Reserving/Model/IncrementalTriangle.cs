using ActuarialForge.Primitives;

namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Represents an incremental loss triangle derived from claim events.
    /// </summary>
    /// <remarks>
    /// An <see cref="IncrementalTriangle"/> aggregates monetary claim events into accident
    /// periods and development periods according to a specified
    /// <see cref="ReservingTimeGranularity"/> and <see cref="ClaimDateBasis"/>.
    /// Each triangle cell contains the incremental amount assigned to the corresponding
    /// accident/development period combination.
    /// </remarks>
    public sealed record IncrementalTriangle
    {
        private readonly Dictionary<RunOffKey, Money> _claimsAmounts;

        private readonly TriangleShape _shape;

        private readonly TimeAxis _timeAxis;

        /// <summary>
        /// Gets the reserving time granularity used by this triangle.
        /// </summary>
        public ReservingTimeGranularity TimeGranularity { get; }

        /// <summary>
        /// Gets the claim date basis used to assign claim events to accident periods.
        /// </summary>
        public ClaimDateBasis ClaimDateBasis { get; }

        /// <summary>
        /// Gets the number of accident periods covered by the triangle.
        /// </summary>
        public int AccidentPeriods { get => _shape.AccidentPeriods; }

        /// <summary>
        /// Gets the number of development periods covered by the triangle.
        /// </summary>
        public int DevelopmentPeriods { get => _shape.DevelopmentPeriods; }

        /// <summary>
        /// Gets the currency of all monetary values contained in the triangle.
        /// </summary>
        public Currency Currency { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementalTriangle"/> class
        /// from a sequence of claim events.
        /// </summary>
        /// <param name="timeGranularity">The reserving time granularity.</param>
        /// <param name="claimDateBasis">
        /// The date basis used to determine the accident period of a claim event.
        /// </param>
        /// <param name="currency">The currency of the triangle.</param>
        /// <param name="claims">The claim events to aggregate into the triangle.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no claim data is provided.
        /// </exception>
        internal IncrementalTriangle(ReservingTimeGranularity timeGranularity, ClaimDateBasis claimDateBasis, Currency currency, IEnumerable<ClaimEvent> claims)
        {
            TimeGranularity = timeGranularity;
            ClaimDateBasis = claimDateBasis;
            Currency = currency;

            _timeAxis = new(TimeGranularity);
            _claimsAmounts = AddClaims(claims, claimDateBasis, _timeAxis);

            if (_claimsAmounts.Count == 0)
                throw new InvalidOperationException("No loss data provided.");

            int maxAccidentPeriod = _claimsAmounts.Keys.Max(k => k.AccidentPeriod.Period);
            int minAccidentPeriod = _claimsAmounts.Keys.Min(k => k.AccidentPeriod.Period);

            int triangleOffset = _claimsAmounts.Keys
                .Where(k => k.AccidentPeriod.Period == maxAccidentPeriod)
                .Max(k => k.DevelopmentPeriod.Lag);

            int maxDevelopmentPeriod = _claimsAmounts.Keys.Max(k => k.DevelopmentPeriod.Lag);

            _shape = new(minAccidentPeriod, maxAccidentPeriod, 0, maxDevelopmentPeriod, triangleOffset);
        }

        /// <summary>
        /// Gets the incremental amount for the specified accident and development period.
        /// </summary>
        /// <param name="accidentPeriod">The accident period.</param>
        /// <param name="developmentPeriod">The development period.</param>
        /// <returns>
        /// The incremental amount for the specified cell, or zero if the cell lies within
        /// the triangle shape but no claim amount was recorded for it.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the specified cell lies outside the bounds of the triangle.
        /// </exception>
        public Money this[AccidentPeriod accidentPeriod, DevelopmentPeriod developmentPeriod]
        {
            get
            {
                if (!_shape.Contains(accidentPeriod, developmentPeriod))
                    throw new ArgumentException("The combination of periods lies outside the bounds of the run off triangle.");

                RunOffKey key = new(accidentPeriod, developmentPeriod);

                if (_claimsAmounts.TryGetValue(key, out var amount))
                    return amount;
                else
                    return Money.Zero(Currency);
            }
        }

        /// <summary>
        /// Gets the incremental amount for the specified accident period index and development lag.
        /// </summary>
        /// <param name="accidentPeriodIndex">The zero-based accident period index.</param>
        /// <param name="developmentLag">The zero-based development lag.</param>
        /// <returns>The incremental amount for the specified cell.</returns>
        public Money this[int accidentPeriodIndex, int developmentLag]
        {
            get => this[new AccidentPeriod(accidentPeriodIndex), new DevelopmentPeriod(developmentLag)];
        }

        private static Dictionary<RunOffKey, Money> AddClaims(IEnumerable<ClaimEvent> claims, ClaimDateBasis claimDateBasis, TimeAxis timeAxis)
        {
            Dictionary<RunOffKey, Money> amounts = [];

            foreach (ClaimEvent claim in claims)
            {
                ModelTime claimReferenceTime =
                    claimDateBasis == ClaimDateBasis.AccidentDate ?
                    claim.OriginalReport.OriginalOccurrence.Time :
                    claim.OriginalReport.Time;

                AccidentPeriod accidentPeriod = timeAxis.GetAccidentPeriod(claimReferenceTime);
                DevelopmentPeriod developmentPeriod = timeAxis.GetDevelopmentPeriod(claimReferenceTime, claim.Time);

                RunOffKey key = new(accidentPeriod, developmentPeriod);

                if (!amounts.TryGetValue(key, out var amount))
                    amount = Money.Zero(claim.Amount.Currency);

                amounts[key] = amount + claim.Amount;
            }

            return amounts;
        }

        /// <summary>
        /// Converts this incremental triangle into a cumulative triangle.
        /// </summary>
        /// <returns>A cumulative triangle representing cumulative development by accident period.</returns>
        public CumulativeTriangle ToCumulativeTriangle()
        {
            List<IEnumerable<Money>> rows = [];

            for (int i = _shape.MinAccidentPeriod; i <= _shape.MaxAccidentPeriod; i++)
                rows.Add(GetRow(i));

            return new(rows, TimeGranularity, _shape, ClaimDateBasis, Currency);
        }

        internal IEnumerable<Money> GetColumn(DevelopmentPeriod developmentPeriod)
        {
            if (_claimsAmounts.Count == 0) yield break;

            for (int i = _shape.MinAccidentPeriod;  i <= _shape.MaxAccidentPeriod; i++)
            {
                AccidentPeriod accidentPeriod = new(i);

                if (developmentPeriod.Lag <= _shape.MaxDevelopmentPeriodForAccidentPeriod(accidentPeriod))
                    yield return this[accidentPeriod, developmentPeriod];
                else
                    yield break;
            }
        }

        internal IEnumerable<Money> GetColumn(int developmentPeriod)
            => GetColumn(new DevelopmentPeriod(developmentPeriod));

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
        {
            if (_claimsAmounts.Count == 0) yield break;

            for (int i = _shape.MinDevelopmentPeriod; i <= _shape.MaxDevelopmentPeriodForAccidentPeriod(accidentPeriod); i++)
                yield return this[accidentPeriod, new(i)];
        }

        internal IEnumerable<Money> GetRow(int accidentPeriod)
            => GetRow(new AccidentPeriod(accidentPeriod));

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

            for (int i = _shape.MinAccidentPeriod; i <= _shape.MaxAccidentPeriod; i++)
            {
                AccidentPeriod accidentPeriod = new(i);
                int lag = _shape.MaxDevelopmentPeriodForAccidentPeriod(accidentPeriod) + offset;

                if (lag >= 0)
                    yield return this[accidentPeriod, new(lag)];
                else
                    yield break;
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

            for (int i = _shape.MinAccidentPeriod; i <= _shape.MaxAccidentPeriod; i++)
            {
                AccidentPeriod accidentPeriod = new(i);

                string line = string.Join(separator, GetRow(accidentPeriod));
                lines.Add(line);
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
        /// Determines whether this instance is equal to another <see cref="IncrementalTriangle"/>.
        /// </summary>
        /// <param name="other">The other triangle to compare with.</param>
        /// <returns>
        /// <c>true</c> if both triangles have the same shape, metadata, and cell values;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(IncrementalTriangle? other)
        {
            if (other is null) return false;

            if (_shape != other._shape) return false;
            if (TimeGranularity != other.TimeGranularity) return false;
            if (ClaimDateBasis != other.ClaimDateBasis) return false;
            if (Currency != other.Currency) return false;

            if (_claimsAmounts.Count != other._claimsAmounts.Count) return false;

            foreach (var claim in _claimsAmounts)
            {
                if (!other._claimsAmounts.TryGetValue(claim.Key, out var claimAmount)) return false;

                if (_claimsAmounts[claim.Key] != claimAmount) return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code based on triangle metadata and contained claim amounts.</returns>
        public override int GetHashCode()
        {
            HashCode hash = new();

            hash.Add(TimeGranularity);
            hash.Add(ClaimDateBasis);
            hash.Add(Currency);
            hash.Add(_shape);

            foreach (var claim in _claimsAmounts)
            {
                hash.Add(claim.Key);
                hash.Add(claim.Value);
            }

            return hash.ToHashCode();
        }
    }
}
