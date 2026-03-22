namespace ActuarialForge.Primitives
{
    /// <summary>
    /// The exception that is thrown when a <see cref="DiscountFactor"/> is applied
    /// to a <see cref="CashflowItem"/> with a different model time.
    /// </summary>
    /// <remarks>
    /// In the discrete-time framework of this library, a discount factor must be
    /// dated at the same <see cref="ModelTime"/> as the cashflow item it is applied to.
    /// This exception indicates a violation of that requirement.
    /// </remarks>
    public sealed class DiscountDateMismatchException : InvalidOperationException
    {
        /// <summary>
        /// Gets the model time of the cashflow item.
        /// </summary>
        public ModelTime CashflowTime { get; }

        /// <summary>
        /// Gets the model time of the discount factor.
        /// </summary>
        public ModelTime DiscountTime { get; }

        
        private static string CreateMessage(ModelTime cashflowTime, ModelTime discountTime)
            => $"Cannot apply a discount factor dated {discountTime} to a cashflow item dated {cashflowTime}.";

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscountDateMismatchException"/> class.
        /// </summary>
        /// <param name="cashflowTime">The model time of the cashflow item.</param>
        /// <param name="discountTime">The model time of the discount factor.</param>
        public DiscountDateMismatchException(ModelTime cashflowTime, ModelTime discountTime) : base(CreateMessage(cashflowTime, discountTime))
        {
            CashflowTime = cashflowTime;
            DiscountTime = discountTime;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscountDateMismatchException"/> class
        /// with a specified inner exception.
        /// </summary>
        /// <param name="cashflowTime">The model time of the cashflow item.</param>
        /// <param name="discountTime">The model time of the discount factor.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public DiscountDateMismatchException(ModelTime cashflowTime, ModelTime discountTime, Exception? innerException)
            : base(CreateMessage(cashflowTime, discountTime), innerException)
        {
            CashflowTime = cashflowTime;
            DiscountTime = discountTime;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscountDateMismatchException"/> class
        /// using a cashflow item and a discount factor.
        /// </summary>
        /// <param name="cashflowItem">The cashflow item.</param>
        /// <param name="discountFactor">The discount factor.</param>
        public DiscountDateMismatchException(CashflowItem cashflowItem, DiscountFactor discountFactor)
            : this(cashflowItem.Time, discountFactor.Time) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscountDateMismatchException"/> class
        /// using a cashflow item and a discount factor, including an inner exception.
        /// </summary>
        /// <param name="cashflowItem">The cashflow item.</param>
        /// <param name="discountFactor">The discount factor.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public DiscountDateMismatchException(CashflowItem cashflowItem, DiscountFactor discountFactor, Exception? innerException)
            : this(cashflowItem.Time, discountFactor.Time, innerException) { }
    }
}
