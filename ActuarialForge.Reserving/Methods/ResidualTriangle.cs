using ActuarialForge.Primitives;
using ActuarialForge.Reserving.Model;

namespace ActuarialForge.Reserving.Methods
{
    /// <summary>
    /// Represents a triangle of cumulative residuals.
    /// </summary>
    /// <remarks>
    /// <see cref="ResidualTriangle"/> stores the residuals of a cumulative triangle
    /// after excluding development period zero. Residuals are arranged in triangular form
    /// and can be accessed by accident period and development period.
    /// </remarks>
    public sealed record ResidualTriangle
    {
        private readonly Money[][] _residuals;

        /// <summary>
        /// Gets the number of accident periods represented in the residual triangle.
        /// </summary>
        public int AccidentPeriods { get; }

        /// <summary>
        /// Gets the number of development periods represented in the underlying triangle.
        /// </summary>
        /// <remarks>
        /// Development period zero is not part of the residual triangle.
        /// </remarks>
        public int DevelopmentPeriods { get; }

        /// <summary>
        /// Gets the residual value at the specified accident period and development period.
        /// </summary>
        /// <param name="accidentPeriod">The zero-based accident period index.</param>
        /// <param name="developmentPeriod">
        /// The development period index. Development period zero is not part of the residual triangle.
        /// </param>
        /// <returns>The residual value at the specified cell.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="accidentPeriod"/> or <paramref name="developmentPeriod"/> lies outside the valid range.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the specified cell lies outside the triangle's bounds.
        /// </exception>
        public Money this[int accidentPeriod, int developmentPeriod]
        {
            get
            {
                if (accidentPeriod < 0 || accidentPeriod >= AccidentPeriods)
                    throw new ArgumentOutOfRangeException(nameof(accidentPeriod));

                if (developmentPeriod < 1 || developmentPeriod >= DevelopmentPeriods)
                    throw new ArgumentOutOfRangeException(nameof(developmentPeriod));

                if (accidentPeriod + developmentPeriod >= DevelopmentPeriods)
                    throw new InvalidOperationException("The given cell lies outside the triangle's bounds.");

                return _residuals[accidentPeriod][developmentPeriod - 1];
            }
        }

        /// <summary>
        /// Gets the residual value at the specified accident period and development period.
        /// </summary>
        /// <param name="accidentPeriod">The accident period.</param>
        /// <param name="developmentPeriod">The development period.</param>
        /// <returns>The residual value at the specified cell.</returns>
        public Money this[AccidentPeriod accidentPeriod, DevelopmentPeriod developmentPeriod] { get => this[accidentPeriod.Period, developmentPeriod.Lag]; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResidualTriangle"/> class
        /// from a collection of residuals.
        /// </summary>
        /// <param name="residuals">The residual collection.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="residuals"/> is <c>null</c>.
        /// </exception>
        internal ResidualTriangle(Residuals residuals)
        {
            ArgumentNullException.ThrowIfNull(residuals);

            int maxAccident = residuals.Keys.Max(k => k.AccidentPeriod.Period);
            int maxDevelopment = residuals.Keys.Max(k => k.DevelopmentPeriod.Lag);

            AccidentPeriods = maxAccident + 1;
            DevelopmentPeriods = maxDevelopment + 1;

            _residuals = new Money[AccidentPeriods][];

            for (int i = 0; i < AccidentPeriods; i++)
            {
                int rowLength = DevelopmentPeriods - 1 - i;

                if (rowLength > 0)
                    _residuals[i] = new Money[rowLength];
                else
                    _residuals[i] = [];
            }

            foreach (var kvp in residuals)
            {
                if (kvp.Key.DevelopmentPeriod.Lag != 0)
                    _residuals[kvp.Key.AccidentPeriod.Period][kvp.Key.DevelopmentPeriod.Lag - 1] = kvp.Value;
            }
        }

        /// <summary>
        /// Gets the specified residual row as a <see cref="TriangleRow"/>.
        /// </summary>
        /// <param name="accidentPeriod">The zero-based accident period index.</param>
        /// <returns>The corresponding residual row.</returns>
        public TriangleRow GetTriangleRow(int accidentPeriod)
            => new(_residuals[accidentPeriod]);

        /// Gets the specified residual row as a <see cref="TriangleRow"/>.
        /// </summary>
        /// <param name="accidentPeriod">The accident period.</param>
        /// <returns>The corresponding residual row.</returns>
        public TriangleRow GetTriangleRow(AccidentPeriod accidentPeriod)
            => GetTriangleRow(accidentPeriod.Period);

        private IEnumerable<Money> GetColumn(int developmentPeriod)
        {
            if (developmentPeriod <= 0 || developmentPeriod >= DevelopmentPeriods)
                throw new ArgumentOutOfRangeException(nameof(developmentPeriod));

            int row = 0;
            int colIndex = developmentPeriod - 1;

            while (row < AccidentPeriods && _residuals[row].Length > colIndex)
            {
                yield return _residuals[row][colIndex];
                row++;
            }
        }

        /// <summary>
        /// Gets the specified residual column as a <see cref="TriangleColumn"/>.
        /// </summary>
        /// <param name="developmentPeriod">
        /// The development period index. Must be greater than zero.
        /// </param>
        /// <returns>The corresponding residual column.</returns>
        public TriangleColumn GetTriangleColumn(int developmentPeriod)
            => new(GetColumn(developmentPeriod));

        /// <summary>
        /// Gets the specified residual column as a <see cref="TriangleColumn"/>.
        /// </summary>
        /// <param name="developmentPeriod">The development period.</param>
        /// <returns>The corresponding residual column.</returns>
        public TriangleColumn GetTriangleColumn(DevelopmentPeriod developmentPeriod)
            => GetTriangleColumn(developmentPeriod.Lag);

        private IEnumerable<Money> GetDiagonal(int offset)
        {
            if (offset > 0)
                throw new ArgumentException("Cannot offset the diagonal of a triangle beyond the triangle's right boundary.", nameof(offset));

            if (-offset >= DevelopmentPeriods)
                throw new ArgumentException("Cannot offset the diagonal of a triangle beyond the triangle's left boundary.", nameof(offset));

            for (int i = 0; i < AccidentPeriods; i++)
            {
                int developmentIndex = _residuals[i].Length - 1 + offset;

                if (developmentIndex < 0) yield break;

                yield return _residuals[i][developmentIndex];
            }
        }

        /// <summary>
        /// Gets a residual diagonal as a <see cref="TriangleDiagonal"/>.
        /// </summary>
        /// <param name="offset">
        /// The offset relative to the latest residual diagonal. Negative values refer to earlier diagonals.
        /// </param>
        /// <returns>The requested residual diagonal.</returns>
        public TriangleDiagonal GetTriangleDiagonal(int offset)
            => new(GetDiagonal(offset));

        /// <summary>
        /// Determines whether this instance is equal to another <see cref="ResidualTriangle"/>.
        /// </summary>
        /// <param name="other">The other residual triangle to compare with.</param>
        /// <returns>
        /// <c>true</c> if both residual triangles have the same shape and values;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ResidualTriangle? other)
        {
            if (other is null) return false;

            if (_residuals.Length != other._residuals.Length) return false;

            for (int i = 0; i < _residuals.Length; i++)
            {
                if (_residuals[i].Length !=  other._residuals[i].Length) return false;

                for (int j = 0; j < _residuals[i].Length; j++)
                    if (_residuals[i][j] != other._residuals[i][j]) return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code based on the contained residual values.</returns>
        public override int GetHashCode()
        {
            HashCode hash = new();

            for (int i = 0; i < _residuals.Length; i++)
                for (int j = 0; j < _residuals[i].Length; j++)
                    hash.Add(_residuals[i][j]);

            return hash.ToHashCode();
        }
    }
}
