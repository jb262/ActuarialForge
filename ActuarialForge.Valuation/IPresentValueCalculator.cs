using ActuarialForge.Primitives;

namespace ActuarialForge.Valuation
{
    /// <summary>
    /// Provides present value calculations for one or more cashflows using a discount curve.
    /// </summary>
    /// <remarks>
    /// Implementations are expected to apply discount factors derived from the supplied
    /// <see cref="DiscountCurve"/> to the cashflow items contained in the input <see cref="Cashflow"/> instances.
    /// </remarks>
    public interface IPresentValueCalculator
    {
        /// <summary>
        /// Computes the present value of a single cashflow using the specified discount curve.
        /// </summary>
        /// <param name="cashflow">The cashflow to be valued.</param>
        /// <param name="discountCurve">The discount curve used to derive discount factors.</param>
        /// <returns>The present value as a monetary amount.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="cashflow"/> or <paramref name="discountCurve"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if required discount factors are missing or currencies are inconsistent.
        /// </exception>
        Money PresentValue(Cashflow cashflow, DiscountCurve discountCurve);

        /// <summary>
        /// Computes the present value of a weighted combination of cashflows using the specified discount curve.
        /// </summary>
        /// <param name="cashflows">The cashflows to be valued.</param>
        /// <param name="weights">
        /// The weights applied to each cashflow. The sequence is expected to align with <paramref name="cashflows"/>
        /// (same order and same number of elements).
        /// </param>
        /// <param name="discountCurve">The discount curve used to derive discount factors.</param>
        /// <returns>The present value of the weighted combination as a monetary amount.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="cashflows"/>, <paramref name="weights"/>, or <paramref name="discountCurve"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the number of cashflows does not match the number of weights.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if required discount factors are missing or currencies are inconsistent.
        /// </exception>
        Money PresentValue(IEnumerable<Cashflow> cashflows, IEnumerable<decimal> weights, DiscountCurve discountCurve);
    }
}
