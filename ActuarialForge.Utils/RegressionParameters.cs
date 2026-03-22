using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace ActuarialForge.Utils
{
    /// <summary>
    /// Represents a set of regression coefficients indexed by variable number.
    /// </summary>
    /// <remarks>
    /// Index <c>0</c> is reserved for the intercept term.
    /// Positive indices represent explanatory variables.
    /// </remarks>
    public sealed class RegressionParameters : IReadOnlyDictionary<int, decimal>
    {
        private readonly Dictionary<int, decimal> _coefficients = [];

        /// <summary>
        /// Gets the intercept term, if present.
        /// </summary>
        public decimal? Intercept
        {
            get
            {
                if (_coefficients.TryGetValue(0, out decimal intercept))
                    return intercept;
                return null;
            }
        }

        /// <summary>
        /// Gets the regression coefficients as a read-only dictionary.
        /// </summary>
        public IReadOnlyDictionary<int, decimal> Coefficients { get => _coefficients.AsReadOnly(); }

        /// <summary>
        /// Gets the variable indices contained in this parameter set.
        /// </summary>
        public IEnumerable<int> Keys { get => _coefficients.Keys; }

        /// <summary>
        /// Gets the coefficient values contained in this parameter set.
        /// </summary>
        public IEnumerable<decimal> Values { get => _coefficients.Values; }

        /// <summary>
        /// Gets the number of stored coefficients, including the intercept if present.
        /// </summary>
        public int Count { get => _coefficients.Count; }

        /// <summary>
        /// Gets the coefficient associated with the specified variable index.
        /// </summary>
        /// <param name="key">The variable index.</param>
        /// <returns>The coefficient associated with the specified variable index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="key"/> is negative.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if no coefficient is stored for the specified variable index.
        /// </exception>
        public decimal this[int key]
        {
            get
            {
                if (key < 0)
                    throw new ArgumentOutOfRangeException(nameof(key), "Cannot access a coefficient for a variable with a negative index.");

                if (!_coefficients.TryGetValue(key, out decimal value))
                    throw new KeyNotFoundException($"No coefficient computed for the variable with index {key}.");

                return value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegressionParameters"/> class
        /// without an intercept term.
        /// </summary>
        /// <param name="coefficients">The regression coefficients indexed by variable number.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="coefficients"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if a coefficient key is less than or equal to zero.
        /// </exception>
        public RegressionParameters(IReadOnlyDictionary<int, decimal> coefficients)
        {
            ArgumentNullException.ThrowIfNull(coefficients);
            foreach (var kvp in coefficients)
            {
                if (kvp.Key <= 0)
                    throw new ArgumentException("Variable indices must be positive. Index 0 is reserved for the intercept..", nameof(coefficients));

                _coefficients.Add(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegressionParameters"/> class
        /// with an intercept term.
        /// </summary>
        /// <param name="intercept">The intercept term.</param>
        /// <param name="coefficients">The regression coefficients indexed by variable number.</param>
        public RegressionParameters(decimal intercept, IReadOnlyDictionary<int, decimal> coefficients) : this(coefficients)
        {
            _coefficients.Add(0, intercept);
        }

        /// <summary>
        /// Creates regression parameters for a simple linear regression with intercept.
        /// </summary>
        /// <param name="intercept">The intercept term.</param>
        /// <param name="coefficient">The coefficient of the single explanatory variable.</param>
        /// <returns>A regression parameter set representing the simple linear regression.</returns>
        public static RegressionParameters SimpleLinearRegressionCoefficients(decimal intercept, decimal coefficient)
            => new(intercept, new Dictionary<int, decimal>() { { 1, coefficient} });

        /// <summary>
        /// Creates regression parameters for a simple linear regression without intercept.
        /// </summary>
        /// <param name="coefficient">The coefficient of the single explanatory variable.</param>
        /// <returns>A regression parameter set representing the simple linear regression.</returns>
        public static RegressionParameters SimpleLinearRegressionCoefficients(decimal coefficient)
            => new(new Dictionary<int, decimal>() { { 1, coefficient } });

        /// <summary>
        /// Determines whether the specified variable index exists in the parameter set.
        /// </summary>
        /// <param name="key">The variable index.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(int key)
            => _coefficients.ContainsKey(key);

        /// <summary>
        /// Attempts to get the coefficient associated with the specified variable index.
        /// </summary>
        /// <param name="key">The variable index.</param>
        /// <param name="value">
        /// When this method returns, contains the coefficient associated with the specified key,
        /// if the key is found; otherwise, the default value for <see cref="decimal"/>.
        /// </param>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(int key, [MaybeNullWhen(false)] out decimal value)
            => _coefficients.TryGetValue(key, out value);

        // <summary>
        /// Returns an enumerator that iterates through the stored coefficients.
        /// </summary>
        public IEnumerator<KeyValuePair<int, decimal>> GetEnumerator()
            => _coefficients.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}