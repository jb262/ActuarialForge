namespace ActuarialForge.Primitives
{
    /// <summary>
    /// Represents a discount factor applicable at a specific model time.
    /// </summary>
    /// <remarks>
    /// A <see cref="DiscountFactor"/> is used to convert a future monetary amount
    /// into its present value at the associated <see cref="Time"/>.
    /// The factor is typically derived from a <see cref="DiscountCurve"/>.
    /// </remarks>
    public sealed record DiscountFactor
    {
        /// <summary>
        /// Gets the discount factor value.
        /// </summary>
        /// <remarks>
        /// The factor is usually between 0 and 1 for positive interest rate environments.
        /// </remarks>
        public decimal Factor { get; init; }

        /// <summary>
        /// Gets the model time at which the discount factor applies.
        /// </summary>
        public ModelTime Time { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscountFactor"/> record.
        /// </summary>
        /// <param name="factor">The discount factor value.</param>
        /// <param name="time">The model time at which the factor applies.</param>
        public DiscountFactor(decimal factor, ModelTime time)
        {
            Factor = factor;
            Time = time;
        }

        /// <summary>
        /// Applies the discount factor to the specified cashflow item.
        /// </summary>
        /// <param name="item">The cashflow item to discount.</param>
        /// <returns>
        /// A new <see cref="CashflowItem"/> with the discounted monetary amount.
        /// </returns>
        /// <exception cref="DiscountDateMismatchException">
        /// Thrown if the cashflow item's time does not match the discount factor's time.
        /// </exception>
        /// <remarks>
        /// The discount operation multiplies the item's amount by the <see cref="Factor"/>.
        /// </remarks>
        public CashflowItem Apply(CashflowItem item)
        {
            if (Time != item.Time)
                throw new DiscountDateMismatchException(item, this);

            return item with { Amount = item.Amount * Factor };
        }
    }
}
