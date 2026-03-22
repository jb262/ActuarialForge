using ActuarialForge.Primitives;

namespace ActuarialForge.Valuation
{
    /// <summary>
    /// Provides deterministic present value calculations for cashflows
    /// using a single discount curve.
    /// </summary>
    /// <remarks>
    /// This implementation discounts each cashflow item using the provided
    /// <see cref="DiscountCurve"/> and aggregates the discounted monetary amounts.
    /// 
    /// For multiple cashflows, a weighted aggregate cashflow is constructed
    /// before discounting.
    /// </remarks>
    public class DeterministicPresentValueCalculator : IPresentValueCalculator
    {
        /// <summary>
        /// Computes the present value of a weighted combination of cashflows
        /// using the specified discount curve.
        /// </summary>
        /// <param name="cashflows">The cashflows to be valued.</param>
        /// <param name="weights">
        /// The weights applied to each cashflow. The number of weights must match
        /// the number of cashflows.
        /// </param>
        /// <param name="discountCurve">The discount curve used for discounting.</param>
        /// <returns>
        /// The present value of the weighted aggregate cashflow.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any argument is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if no cashflows or weights are provided.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the number of cashflows does not match the number of weights.
        /// </exception>
        public Money PresentValue(IEnumerable<Cashflow> cashflows, IEnumerable<decimal> weights, DiscountCurve discountCurve)
        {
            ArgumentNullException.ThrowIfNull(cashflows);
            ArgumentNullException.ThrowIfNull(weights);
            ArgumentNullException.ThrowIfNull(discountCurve);

            List<Cashflow> cashflowList = cashflows.ToList();
            List<decimal> weightsList = weights.ToList();

            if (cashflowList.Count == 0)
                throw new ArgumentException("No cashflows provided.");

            if (weightsList.Count == 0)
                throw new ArgumentException("No weights provided.");

            if (cashflowList.Count != weightsList.Count)
                throw new InvalidOperationException("The number of cashflows must match the number of weights.");

            var cashflowWeightPairs = cashflowList
                .Zip(weightsList, (x, y) => new { Cashflow = x, Weight = y });

            WeightedCashflowBuilder weightedCashflowBuilder = new();

            foreach (var pair in cashflowWeightPairs)
                weightedCashflowBuilder.Add(pair.Cashflow, pair.Weight);

            return PresentValue(weightedCashflowBuilder.Build(), discountCurve);
        }

        /// <summary>
        /// Computes the present value of a single cashflow using the specified discount curve.
        /// </summary>
        /// <param name="cashflow">The cashflow to be valued.</param>
        /// <param name="discountCurve">The discount curve used for discounting.</param>
        /// <returns>
        /// The present value as a <see cref="Money"/> instance.
        /// Returns zero if the cashflow contains no items.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="cashflow"/> or <paramref name="discountCurve"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="MissingDiscountFactorException">
        /// Thrown if one or more required discount factors are missing.
        /// </exception>
        public Money PresentValue(Cashflow cashflow, DiscountCurve discountCurve)
        {
            ArgumentNullException.ThrowIfNull(cashflow);
            ArgumentNullException.ThrowIfNull(discountCurve);

            if (cashflow.Count == 0) return Money.Zero(cashflow.Currency);


            decimal amount = cashflow.Discount(discountCurve).Select(c => c.Amount).Sum(m => m.Amount);

            return new(amount, cashflow.Currency);
        }
    }
}
