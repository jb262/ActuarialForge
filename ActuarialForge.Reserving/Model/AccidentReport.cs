using ActuarialForge.EconomicRegister;
using ActuarialForge.Primitives;

namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Represents the reporting of an accident associated with a previously occurred claim event.
    /// </summary>
    /// <remarks>
    /// This event links a reporting date to an existing <see cref="AccidentOccurrence"/>.
    /// It is a non-monetary event and is therefore posted to the economic register with
    /// a zero amount using <see cref="Money.ZeroNone"/>.
    /// </remarks>
    public sealed record AccidentReport : IEconomicEvent
    {
        /// <summary>
        /// Gets the unique identifier of the event.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the model time at which the accident was reported.
        /// </summary>
        public ModelTime Time { get; }

        /// <summary>
        /// Gets the identifier of the claim associated with the reported accident.
        /// </summary>
        /// <remarks>
        /// This value is derived from the associated <see cref="OriginalOccurrence"/>.
        /// </remarks>
        public string ClaimId { get => OriginalOccurrence.ClaimId; }

        /// <summary>
        /// Gets the original accident occurrence associated with this report.
        /// </summary>
        public AccidentOccurrence OriginalOccurrence { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccidentReport"/> class
        /// with an explicit event identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <param name="time">The model time at which the accident was reported.</param>
        /// <param name="originalOccurrence">The original accident occurrence.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="originalOccurrence"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the reporting time precedes the occurrence time.
        /// </exception>
        public AccidentReport(Guid id, ModelTime time, AccidentOccurrence originalOccurrence)
        {
            Id = id;
            Time = time;

            ArgumentNullException.ThrowIfNull(originalOccurrence);

            if (originalOccurrence.Time > time)
                throw new ArgumentException("An accident cannot be reported before it occurred.");

            OriginalOccurrence = originalOccurrence;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccidentReport"/> class
        /// with a generated event identifier.
        /// </summary>
        /// <param name="time">The model time at which the accident was reported.</param>
        /// <param name="originalOccurrence">The original accident occurrence.</param>
        public AccidentReport(ModelTime time, AccidentOccurrence originalOccurrence)
            : this(Guid.NewGuid(), time, originalOccurrence) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccidentReport"/> class
        /// from a calendar reporting date and an explicit model base date.
        /// </summary>
        /// <param name="reportingDate">The calendar reporting date.</param>
        /// <param name="baseDate">The base date used for conversion to model time.</param>
        /// <param name="originalOccurrence">The original accident occurrence.</param>
        public AccidentReport(DateOnly reportingDate, DateOnly baseDate, AccidentOccurrence originalOccurrence)
            : this(ModelTime.ConvertToModelTime(reportingDate, baseDate), originalOccurrence) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccidentReport"/> class
        /// from a calendar reporting date using the default model base date.
        /// </summary>
        /// <param name="reportingDate">The calendar reporting date.</param>
        /// <param name="originalOccurrence">The original accident occurrence.</param>
        public AccidentReport(DateOnly reportingDate, AccidentOccurrence originalOccurrence)
            : this(ModelTime.ConvertToModelTime(reportingDate), originalOccurrence) { }

        /// Posts the accident report to the specified economic register.
        /// </summary>
        /// <param name="register">The economic register to which the event is posted.</param>
        /// <remarks>
        /// If the associated <see cref="OriginalOccurrence"/> has not yet been posted to the register,
        /// it is posted automatically before the accident report is recorded.
        ///
        /// The event is recorded as a non-monetary entry with zero amount and the category
        /// <c>AccidentReport</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="register"/> is <c>null</c>.
        /// </exception>
        public void Post(IEconomicRegister register)
        {
            ArgumentNullException.ThrowIfNull(register);

            if (!register.ContainsKey(OriginalOccurrence.Id))
                OriginalOccurrence.Post(register);

            register.Record(new(Id, Time, Money.ZeroNone, nameof(AccidentReport), ClaimId, OriginalOccurrence.Id.ToString()));
        }
    }
}
