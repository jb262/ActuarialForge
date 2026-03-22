using ActuarialForge.Reserving.Model;
using ActuarialForge.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Collections;

namespace ActuarialForge.Reserving.Methods
{
    /// <summary>
    /// Represents the cell-wise residuals between an observed cumulative triangle
    /// and the triangle implied by a reserving method.
    /// </summary>
    /// <remarks>
    /// <see cref="Residuals"/> stores, for each observed triangle cell, the difference
    /// between the observed cumulative amount and the corresponding projected cumulative amount.
    /// Residuals are indexed by <see cref="RunOffKey"/>.
    /// </remarks>
    public sealed record Residuals : IReadOnlyDictionary<RunOffKey, Money>
    {
        private readonly Dictionary<RunOffKey, Money> _residuals = [];

        /// <summary>
        /// Gets the residual associated with the specified run-off key.
        /// </summary>
        /// <param name="key">The run-off key identifying the cell.</param>
        /// <returns>The residual for the specified cell.</returns>
        public Money this[RunOffKey key] { get => _residuals[key]; }

        /// <summary>
        /// Gets the residual associated with the specified accident and development period.
        /// </summary>
        /// <param name="accidentPeriod">The accident period.</param>
        /// <param name="developmentPeriod">The development period.</param>
        /// <returns>The residual for the specified cell.</returns>
        public Money this[AccidentPeriod accidentPeriod, DevelopmentPeriod developmentPeriod] { get => this[new(accidentPeriod, developmentPeriod)]; }

        /// <summary>
        /// Gets the run-off keys contained in the residual set.
        /// </summary>
        public IEnumerable<RunOffKey> Keys { get =>  _residuals.Keys; }

        /// <summary>
        /// Gets the residual values contained in the residual set.
        /// </summary>
        public IEnumerable<Money> Values {  get => _residuals.Values; }

        /// <summary>
        /// Gets the number of residual cells stored in this instance.
        /// </summary>
        public int Count { get => _residuals.Count; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Residuals"/> class
        /// using the observed cumulative triangle implied by the specified reserving method.
        /// </summary>
        /// <param name="method">The reserving method.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="method"/> is <c>null</c>.
        /// </exception>
        public Residuals(IPatternBasedReservingMethod method) : this(method.ComputeProjection().ExtractTriangle(), method) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Residuals"/> class
        /// from an observed cumulative triangle and a reserving method.
        /// </summary>
        /// <param name="triangle">The observed cumulative triangle.</param>
        /// <param name="method">The reserving method used to generate the projected triangle.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="triangle"/> or <paramref name="method"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the projected triangle does not match the dimensions of the observed triangle.
        /// </exception>
        public Residuals(CumulativeTriangle triangle, IPatternBasedReservingMethod method)
        {
            ArgumentNullException.ThrowIfNull(triangle);
            ArgumentNullException.ThrowIfNull(method);

            TriangleColumn firstColumn = triangle.GetTriangleColumn(0);

            CumulativeTriangle projectedTriangle = method.DevelopTriangle(firstColumn);

            if (projectedTriangle.DevelopmentPeriods != triangle.DevelopmentPeriods || projectedTriangle.AccidentPeriods != triangle.AccidentPeriods)
                throw new InvalidOperationException("Dimension mismatch between the projected and the original triangle.");

            for (int developmentPeriod = 0; developmentPeriod < projectedTriangle.DevelopmentPeriods; developmentPeriod++)
            {
                var columnResiduals = triangle
                    .GetColumn(developmentPeriod)
                    .Zip(projectedTriangle.GetColumn(developmentPeriod), (x, y) => x - y);

                int accidentPeriod = 0;

                foreach (var cellResidual in columnResiduals)
                    _residuals[new(new(accidentPeriod++), new(developmentPeriod))] = cellResidual;
            }
        }

        /// <summary>
        /// Converts the residual set to a residual triangle.
        /// </summary>
        /// <returns>A <see cref="ResidualTriangle"/> representing the residuals.
        public ResidualTriangle ToTriangle()
            => new(this);


        /// <summary>
        /// Determines whether the specified run-off key exists in the residual set.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(RunOffKey key)
            => _residuals.ContainsKey(key);

        /// <summary>
        /// Returns an enumerator that iterates through the stored residual cells.
        /// </summary>
        public IEnumerator<KeyValuePair<RunOffKey, Money>> GetEnumerator()
            => _residuals.GetEnumerator();

        /// <summary>
        /// Attempts to get the residual associated with the specified run-off key.
        /// </summary>
        /// <param name="key">The run-off key identifying the cell.</param>
        /// <param name="value">
        /// When this method returns, contains the residual associated with the specified key,
        /// if the key is found; otherwise, the default value for <see cref="Money"/>.
        /// </param>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(RunOffKey key, [MaybeNullWhen(false)] out Money value)
            => _residuals.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Determines whether this instance is equal to another <see cref="Residuals"/>.
        /// </summary>
        /// <param name="other">The other residual set to compare with.</param>
        /// <returns>
        /// <c>true</c> if both instances contain the same set of run-off keys and residual values;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Residuals? other)
        {
            if (other is null) return false;

            if (_residuals.Count != other._residuals.Count) return false;

            foreach (var kvp in  _residuals)
            {
                if (!other._residuals.TryGetValue(kvp.Key, out var value)) return false;

                if (kvp.Value != value) return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code based on the contained residual cells.</returns>
        public override int GetHashCode()
        {
            HashCode hash = new();

            foreach (var kvp in _residuals.OrderBy(x => x.Key))
                hash.Add(kvp);

            return hash.ToHashCode();
        }
    }
}
