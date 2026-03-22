using ActuarialForge.Primitives;

namespace ActuarialForge.Valuation
{
    /// <summary>
    /// Builds a single cashflow as a weighted combination of multiple cashflows.
    /// </summary>
    /// <remarks>
    /// The builder aggregates cashflow items by a structural position key consisting of
    /// model time and optional payment metadata. Each added cashflow contributes its items
    /// scaled by the specified weight.
    /// 
    /// After all components have been added, <see cref="Build"/> returns an aggregated
    /// <see cref="Cashflow"/> representing the weighted combination.
    /// </remarks>
    public sealed class WeightedCashflowBuilder
    {
        private const decimal _tolerance = 1e-12m;

        private readonly Dictionary<CashflowPositionKey, CashflowItem> _items = [];

        private decimal _weightSum = decimal.Zero;

        /// <summary>
        /// Adds a cashflow component with the given weight to the builder.
        /// </summary>
        /// <param name="cashflow">The cashflow to add.</param>
        /// <param name="weight">
        /// The weight applied to all items of <paramref name="cashflow"/>. Typically between 0 and 1.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="cashflow"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="weight"/> is outside the allowed range.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the accumulated weight would exceed 1.
        /// </exception>
        public void Add(Cashflow cashflow, decimal weight)
        {
            ArgumentNullException.ThrowIfNull(cashflow);

            if (weight < decimal.Zero || weight > decimal.One)
                throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be between 0 and 1 (inclusive).");

            if (weight == decimal.Zero) return;

            decimal newSum = _weightSum + weight;

            if (newSum > decimal.One)
                throw new InvalidOperationException("Total weights cannot exceed 1.");

            _weightSum = newSum;

            foreach (CashflowItem cashflowItem in cashflow)
            {
                CashflowPositionKey key = new(cashflowItem, cashflow.PaymentFrequency, cashflow.PaymentTiming);

                if (_items.TryGetValue(key, out var existing))
                    _items[key] = existing + (cashflowItem * weight);
                else
                    _items[key] = cashflowItem * weight;
            }
        }

        /// <summary>
        /// Builds the aggregated cashflow.
        /// </summary>
        /// <returns>
        /// A <see cref="Cashflow"/> representing the weighted combination of all added cashflows.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the total weight does not meet the required target (typically 1),
        /// or if no items were added.
        /// </exception>
        public Cashflow Build()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("No cashflow items were added.");

            if (_weightSum < decimal.One - _tolerance || _weightSum > decimal.One + _tolerance)
                throw new InvalidOperationException("The sum of weights must equal 1.");

            return new(_items.Values);
        }
    }
}
