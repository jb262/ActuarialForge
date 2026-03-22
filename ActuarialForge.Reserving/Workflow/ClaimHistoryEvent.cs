using ActuarialForge.Primitives;
using ActuarialForge.Reserving.Model;

namespace ActuarialForge.Reserving.Workflow
{
    /// <summary>
    /// Represents a single event in the history of a claim.
    /// </summary>
    /// <remarks>
    /// A claim history event captures a monetary change at a specific point in time,
    /// such as a payment, reserve adjustment, or recovery.
    public sealed record ClaimHistoryEvent
    {
        /// <summary>
        /// Gets the monetary amount associated with the event.
        /// </summary>
        public Money Amount { get; }

        /// <summary>
        /// Gets the date on which the event occurred.
        /// </summary>
        public DateOnly Date { get; }

        /// <summary>
        /// Gets the type of the claim event.
        /// </summary>
        public ClaimEventType ClaimEventType { get; }

        /// <summary>
        /// Gets the currency of the event.
        /// </summary>
        /// <remarks>
        /// This value is derived from the <see cref="Amount"/>.
        /// </remarks>
        public Currency Currency { get => Amount.Currency; }

        /// Initializes a new instance of the <see cref="ClaimHistoryEvent"/> class.
        /// </summary>
        /// <param name="date">The date of the event.</param>
        /// <param name="amount">The monetary amount of the event.</param>
        /// <param name="claimEventType">The type of the event.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="amount"/> is <c>null</c>.
        /// </exception>
        public ClaimHistoryEvent(DateOnly date, Money amount, ClaimEventType claimEventType)
        {
            ArgumentNullException.ThrowIfNull(amount);
            Date = date;
            Amount = amount;
            ClaimEventType = claimEventType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimHistoryEvent"/> class.
        /// </summary>
        /// <param name="date">The date of the event.</param>
        /// <param name="amount">The monetary amount of the event.</param>
        /// <param name="currency">The currency of the amount.</param>
        /// <param name="claimEventType">The type of the event.</param>
        public ClaimHistoryEvent(DateOnly date, decimal amount, Currency currency, ClaimEventType claimEventType)
            : this(date, new(amount, currency), claimEventType) { }
    }
}
