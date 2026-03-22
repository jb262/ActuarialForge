using ActuarialForge.Primitives;
using ActuarialForge.EconomicRegister;

namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Represents a monetary claim-related event associated with a previously reported accident.
    /// </summary>
    /// <remarks>
    /// A <see cref="ClaimEvent"/> represents a financial movement in the lifecycle of a claim,
    /// such as a payment, reserve change, recovery, or expense.
    ///
    /// Each claim event is linked to an <see cref="AccidentReport"/> and, indirectly,
    /// to the originating <see cref="AccidentOccurrence"/>.
    /// </remarks>
    public sealed record ClaimEvent : IEconomicEvent
    {
        /// <summary>
        /// Gets the unique identifier of the event.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the model time at which the claim event occurred.
        /// </summary>
        public ModelTime Time { get; }

        /// <summary>
        /// Gets the monetary amount associated with the claim event.
        /// </summary>
        public Money Amount { get; }

        /// <summary>
        /// Gets the type of the claim event.
        /// </summary>
        public ClaimEventType ClaimEventType { get; }

        /// <summary>
        /// Gets the original accident report associated with this claim event.
        /// </summary>
        public AccidentReport OriginalReport { get; }

        /// <summary>
        /// Gets the identifier of the claim associated with this event.
        /// </summary>
        /// <remarks>
        /// This value is derived from the associated <see cref="OriginalReport"/>.
        /// </remarks>
        public string ClaimId { get => OriginalReport.ClaimId; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimEvent"/> class
        /// with an explicit event identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <param name="time">The model time at which the claim event occurred.</param>
        /// <param name="amount">The monetary amount associated with the event.</param>
        /// <param name="claimEventType">The type of the claim event.</param>
        /// <param name="originalReport">The original accident report associated with the event.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="amount"/> or <paramref name="originalReport"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the claim event time precedes the reporting time.
        /// </exception>
        public ClaimEvent(Guid id, ModelTime time, Money amount, ClaimEventType claimEventType, AccidentReport originalReport)
        {
            ArgumentNullException.ThrowIfNull(amount);

            Id = id;
            Time = time;
            Amount = amount;
            ClaimEventType = claimEventType;

            ArgumentNullException.ThrowIfNull(originalReport);

            if (originalReport.Time > time)
                throw new ArgumentException("A claim event cannot be triggered before a claim was reported.");

            OriginalReport = originalReport;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimEvent"/> class
        /// with a generated event identifier.
        /// </summary>
        /// <param name="time">The model time at which the claim event occurred.</param>
        /// <param name="amount">The monetary amount associated with the event.</param>
        /// <param name="claimEventType">The type of the claim event.</param>
        /// <param name="report">The original accident report associated with the event.</param>
        public ClaimEvent(ModelTime time, Money amount, ClaimEventType claimEventType, AccidentReport report)
            : this(Guid.NewGuid(), time, amount, claimEventType, report) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimEvent"/> class
        /// from a numeric amount and currency.
        /// </summary>
        /// <param name="time">The model time at which the claim event occurred.</param>
        /// <param name="amount">The numeric monetary amount.</param>
        /// <param name="currency">The currency of the amount.</param>
        /// <param name="claimEventType">The type of the claim event.</param>
        /// <param name="report">The original accident report associated with the event.</param>
        public ClaimEvent(ModelTime time, decimal amount, Currency currency, ClaimEventType claimEventType, AccidentReport report)
            : this(time, new(amount, currency), claimEventType, report) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimEvent"/> class
        /// from a calendar date and an explicit model base date.
        /// </summary>
        /// <param name="date">The calendar date of the claim event.</param>
        /// <param name="baseDate">The base date used for conversion to model time.</param>
        /// <param name="amount">The monetary amount associated with the event.</param>
        /// <param name="claimEventType">The type of the claim event.</param>
        /// <param name="report">The original accident report associated with the event.</param>
        public ClaimEvent(DateOnly date, DateOnly baseDate, Money amount, ClaimEventType claimEventType, AccidentReport report)
            : this(ModelTime.ConvertToModelTime(date, baseDate), amount, claimEventType, report) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimEvent"/> class
        /// from a calendar date, explicit base date, numeric amount, and currency.
        /// </summary>
        /// <param name="date">The calendar date of the claim event.</param>
        /// <param name="baseDate">The base date used for conversion to model time.</param>
        /// <param name="amount">The numeric monetary amount.</param>
        /// <param name="currency">The currency of the amount.</param>
        /// <param name="claimEventType">The type of the claim event.</param>
        /// <param name="report">The original accident report associated with the event.</param>
        public ClaimEvent(DateOnly date, DateOnly baseDate, decimal amount, Currency currency, ClaimEventType claimEventType, AccidentReport report)
            : this(date, baseDate, new(amount, currency), claimEventType, report) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimEvent"/> class
        /// from a calendar date using the default model base date.
        /// </summary>
        /// <param name="date">The calendar date of the claim event.</param>
        /// <param name="amount">The monetary amount associated with the event.</param>
        /// <param name="claimEventType">The type of the claim event.</param>
        /// <param name="report">The original accident report associated with the event.</param>
        public ClaimEvent(DateOnly date, Money amount, ClaimEventType claimEventType, AccidentReport report)
            : this(ModelTime.ConvertToModelTime(date), amount, claimEventType, report) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimEvent"/> class
        /// from a calendar date, numeric amount, and currency using the default model base date.
        /// </summary>
        /// <param name="date">The calendar date of the claim event.</param>
        /// <param name="amount">The numeric monetary amount.</param>
        /// <param name="currency">The currency of the amount.</param>
        /// <param name="claimEventType">The type of the claim event.</param>
        /// <param name="report">The original accident report associated with the event.</param>
        public ClaimEvent(DateOnly date, decimal amount, Currency currency, ClaimEventType claimEventType, AccidentReport report)
            : this(date, new(amount, currency), claimEventType, report) { }

        /// <summary>
        /// Posts the claim event to the specified economic register.
        /// </summary>
        /// <param name="register">The economic register to which the event is posted.</param>
        /// <remarks>
        /// If the associated <see cref="OriginalReport"/> has not yet been posted to the register,
        /// it is posted automatically before the claim event is recorded.
        ///
        /// The event is recorded with its monetary amount, its claim event type as category,
        /// the claim identifier as source, and the original report identifier as source identifier.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="register"/> is <c>null</c>.
        /// </exception>
        public void Post(IEconomicRegister register)
        {
            ArgumentNullException.ThrowIfNull(register);

            if (!register.ContainsKey(OriginalReport.Id))
                OriginalReport.Post(register);

            register.Record(new(Id, Time, Amount, ClaimEventType.ToString(), ClaimId, OriginalReport.Id.ToString()));
        }
    }
}
