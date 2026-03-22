namespace ActuarialForge.Primitives
{
    /// <summary>
    /// The exception that is thrown when one or more cashflow items
    /// cannot be matched to a corresponding discount factor.
    /// </summary>
    /// <remarks>
    /// In the discrete-time framework of this library, each <see cref="CashflowItem"/>
    /// must have a corresponding discount factor at the same <see cref="ModelTime"/>.
    /// This exception indicates that one or more required discount factors are missing.
    /// </remarks>
    public sealed class MissingDiscountFactorException : InvalidOperationException
    {
        /// <summary>
        /// Gets the cashflow items for which no matching discount factor was found.
        /// </summary>
        public IReadOnlyList<CashflowItem> MissingItems { get; }

        private static string CreateMessage(IEnumerable<CashflowItem> missingItems)
        {
            var missingDates = missingItems.Select(i => i.Time.ToString());

            return $"The discount factors do not contain matching discount factors for the following cashflow dates {string.Join(", ", missingDates)}";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingDiscountFactorException"/> class.
        /// </summary>
        /// <param name="missingItems">
        /// The cashflow items for which no corresponding discount factor exists.
        /// </param>
        public MissingDiscountFactorException(IEnumerable<CashflowItem> missingItems)
            : base(CreateMessage(missingItems))
        {
            MissingItems = missingItems.ToList().AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingDiscountFactorException"/> class
        /// with a specified inner exception.
        /// </summary>
        /// <param name="missingItems">
        /// The cashflow items for which no corresponding discount factor exists.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// </param>
        public MissingDiscountFactorException(IEnumerable<CashflowItem> missingItems, Exception? innerException)
            : base(CreateMessage(missingItems), innerException)
        {
            MissingItems = missingItems.ToList().AsReadOnly();
        }
    }
}
