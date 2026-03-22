using ActuarialForge.EconomicRegister;
using ActuarialForge.Primitives;

namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Represents the occurrence of an accident associated with a claim.
    /// </summary>
    /// <remarks>
    /// This event marks the accident date of a claim within the economic event framework.
    /// It is a non-monetary event and is therefore posted to the economic register with
    /// a zero amount using <see cref="Money.ZeroNone"/>.
    /// </remarks>
    public sealed record AccidentOccurrence : IEconomicEvent
    {
        /// <summary>
        /// Gets the unique identifier of the event.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the model time at which the accident occurred.
        /// </summary>
        public ModelTime Time { get; }

        /// <summary>
        /// Gets the identifier of the claim associated with the accident.
        /// </summary>
        /// <remarks>
        /// The claim identifier links the accident occurrence to a specific claim.
        /// </remarks>
        public string ClaimId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccidentOccurrence"/> class
        /// with an explicit event identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <param name="time">The model time at which the accident occurred.</param>
        /// <param name="claimId">The identifier of the associated claim.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="claimId"/> is null, empty, or consists only of whitespace.
        /// </exception>
        public AccidentOccurrence(Guid id, ModelTime time, string claimId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(claimId);
            Id = id;
            Time = time;
            ClaimId = claimId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccidentOccurrence"/> class
        /// with a generated event identifier.
        /// </summary>
        /// <param name="time">The model time at which the accident occurred.</param>
        /// <param name="claimId">The identifier of the associated claim.</param>
        public AccidentOccurrence(ModelTime time, string claimId)
            : this(Guid.NewGuid(), time, claimId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccidentOccurrence"/> class
        /// from a calendar date and an explicit model base date.
        /// </summary>
        /// <param name="occurrenceDate">The accident occurrence date.</param>
        /// <param name="baseDate">The base date used for conversion to model time.</param>
        /// <param name="claimId">The identifier of the associated claim.</param>
        public AccidentOccurrence(DateOnly occurrenceDate, DateOnly baseDate, string claimId)
            : this(ModelTime.ConvertToModelTime(occurrenceDate, baseDate), claimId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccidentOccurrence"/> class
        /// from a calendar date using the default model base date.
        /// </summary>
        /// <param name="occurrenceDate">The accident occurrence date.</param>
        /// <param name="claimId">The identifier of the associated claim.</param>
        public AccidentOccurrence(DateOnly occurrenceDate, string claimId)
            : this(ModelTime.ConvertToModelTime(occurrenceDate), claimId) { }

        /// <summary>
        /// Posts the accident occurrence to the specified economic register.
        /// </summary>
        /// <param name="register">The economic register to which the event is posted.</param>
        /// <remarks>
        /// The event is recorded as a non-monetary entry with zero amount and the category
        /// <c>AccidentOccurrence</c>. The claim identifier is stored as the source identifier.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="register"/> is <c>null</c>.
        /// </exception>
        public void Post(IEconomicRegister register)
        {
            ArgumentNullException.ThrowIfNull(register);

            register.Record(new(Id, Time, Money.ZeroNone, nameof(AccidentOccurrence), ClaimId));
        }
    }
}
