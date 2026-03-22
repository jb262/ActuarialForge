namespace ActuarialForge.Primitives
{
    /// <summary>
    /// The exception that is thrown when an operation is attempted on monetary values with different currencies.
    /// </summary>
    /// <remarks>
    /// This exception is typically thrown by arithmetic and comparison operations on <see cref="Money"/>
    /// when the currencies of the operands do not match.
    /// </remarks>
    public sealed class CurrencyMismatchException : InvalidOperationException
    {
        /// <summary>
        /// Gets the left-hand currency involved in the failed operation.
        /// </summary>
        public Currency LeftCurrency { get; }

        /// <summary>
        /// Gets the right-hand currency involved in the failed operation.
        /// </summary>
        public Currency RightCurrency { get; }

        private static string CreateMessage(Currency left, Currency right)
            => $"Currency mismatch: Cannot operate on amounts with different currencies ('{left}' and '{right}').";

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyMismatchException"/> class.
        /// </summary>
        /// <param name="leftCurrency">The left-hand currency involved in the operation.</param>
        /// <param name="rightCurrency">The right-hand currency involved in the operation.</param>
        public CurrencyMismatchException(Currency leftCurrency, Currency rightCurrency)
            : base(CreateMessage(leftCurrency, rightCurrency))
        {
            LeftCurrency = leftCurrency;
            RightCurrency = rightCurrency;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyMismatchException"/> class
        /// with a specified inner exception.
        /// </summary>
        /// <param name="leftCurrency">The left-hand currency involved in the operation.</param>
        /// <param name="rightCurrency">The right-hand currency involved in the operation.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        public CurrencyMismatchException(Currency leftCurrency, Currency rightCurrency, Exception innerException)
            : base(CreateMessage(leftCurrency, rightCurrency), innerException)
        {
            LeftCurrency = leftCurrency;
            RightCurrency = rightCurrency;
        }
    }
}
