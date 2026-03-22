using ActuarialForge.Reserving.Model;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace ActuarialForge.Reserving.Validation
{
    /// <summary>
    /// Represents a triangle of correlation values indexed by run-off keys.
    /// </summary>
    /// <remarks>
    /// <see cref="CorrelationTriangle"/> stores correlation coefficients associated
    /// with run-off keys. All stored correlation values must lie between -1 and 1.
    /// </remarks>
    public sealed class CorrelationTriangle : IReadOnlyDictionary<RunOffKey, decimal>
    {
        private readonly Dictionary<RunOffKey, decimal> _correlations = [];

        /// <summary>
        /// Gets the correlation associated with the specified run-off key.
        /// </summary>
        /// <param name="key">The run-off key identifying the correlation entry.</param>
        /// <returns>The correlation value for the specified key.</returns>
        public decimal this[RunOffKey key] { get => _correlations[key]; }

        /// <summary>
        /// Gets the correlation associated with the specified accident and development period.
        /// </summary>
        /// <param name="accidentPeriod">The accident period.</param>
        /// <param name="developmentPeriod">The development period.</param>
        /// <returns>The correlation value for the specified key.</returns>
        public decimal this[AccidentPeriod accidentPeriod, DevelopmentPeriod developmentPeriod] { get => this[new(accidentPeriod, developmentPeriod)]; }

        /// <summary>
        /// Gets the run-off keys contained in the correlation triangle.
        /// </summary>
        public IEnumerable<RunOffKey> Keys { get => _correlations.Keys; }

        /// <summary>
        /// Gets the correlation values contained in the correlation triangle.
        /// </summary>
        public IEnumerable<decimal> Values { get => _correlations.Values; }

        /// <summary>
        /// Gets the number of correlation entries stored in this instance.
        /// </summary>
        public int Count { get => _correlations.Count; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationTriangle"/> class
        /// from the specified correlation values.
        /// </summary>
        /// <param name="correlations">The correlation values to store.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="correlations"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if at least one correlation value lies outside the interval [-1, 1].
        /// </exception>
        internal CorrelationTriangle(IReadOnlyDictionary<RunOffKey, decimal> correlations)
        {
            ArgumentNullException.ThrowIfNull(correlations);

            foreach (var correlation in correlations)
            {
                if (Math.Abs(correlation.Value) > decimal.One)
                    throw new ArgumentException("Correlations must lie between -1 and 1.");

                _correlations.Add(correlation.Key, correlation.Value);
            }
        }

        /// <summary>
        /// Determines whether the specified run-off key exists in the correlation triangle.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(RunOffKey key)
            => _correlations.ContainsKey(key);

        /// <summary>
        /// Returns an enumerator that iterates through the stored correlation entries.
        /// </summary>
        public IEnumerator<KeyValuePair<RunOffKey, decimal>> GetEnumerator()
            => _correlations.GetEnumerator();

        /// <summary>
        /// Attempts to get the correlation associated with the specified run-off key.
        /// </summary>
        /// <param name="key">The run-off key identifying the correlation entry.</param>
        /// <param name="value">
        /// When this method returns, contains the correlation associated with the specified key,
        /// if the key is found; otherwise, the default value for <see cref="decimal"/>.
        /// </param>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(RunOffKey key, [MaybeNullWhen(false)] out decimal value)
            => _correlations.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
