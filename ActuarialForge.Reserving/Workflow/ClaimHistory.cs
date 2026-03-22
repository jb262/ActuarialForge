using ActuarialForge.Primitives;
using ActuarialForge.Reserving.Model;

namespace ActuarialForge.Reserving.Workflow
{
    /// <summary>
    /// Represents the full history of a single claim.
    /// </summary>
    /// <remarks>
    /// <see cref="ClaimHistory"/> combines the occurrence date, reporting date,
    /// and all subsequent claim history events of a claim into a single workflow-level object.
    /// It can be converted to reserving model events using <see cref="ToClaimEvents"/>.
    /// </remarks>
    public sealed record ClaimHistory
    {
        /// <summary>
        /// Gets the claim identifier.
        /// </summary>
        public string ClaimID { get; }

        /// <summary>
        /// Gets the date on which the underlying loss occurred.
        /// </summary>
        public DateOnly OccurrenceDate { get; }

        /// <summary>
        /// Gets the date on which the claim was reported.
        /// </summary>
        public DateOnly ReportingDate { get; }

        /// <summary>
        /// Gets the currency of the claim history.
        /// </summary>
        /// <remarks>
        /// The currency is derived from the claim history events.
        /// All events must use the same currency.
        /// </remarks>
        public Currency Currency { get; }

        /// <summary>
        /// Gets the events belonging to the claim history.
        /// </summary>
        public IReadOnlyList<ClaimHistoryEvent> Events { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimHistory"/> class.
        /// </summary>
        /// <param name="claimID">The claim identifier.</param>
        /// <param name="occurrenceDate">The occurrence date of the claim.</param>
        /// <param name="reportingDate">The reporting date of the claim.</param>
        /// <param name="events">The claim history events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="claimID"/> or <paramref name="events"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="claimID"/> is empty or whitespace, or if no events are provided.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the resulting event collection is empty.
        /// </exception>
        /// <exception cref="CurrencyMismatchException">
        /// Thrown if the supplied events do not all use the same currency.
        /// </exception>
        public ClaimHistory(string claimID, DateOnly occurrenceDate, DateOnly reportingDate, IEnumerable<ClaimHistoryEvent> events)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(claimID);
            ArgumentNullException.ThrowIfNull(events);

            ClaimID = claimID;
            OccurrenceDate = occurrenceDate;
            ReportingDate = reportingDate;

            Events = events.ToList().AsReadOnly();
            if (Events.Count == 0)
                throw new ArgumentNullException(nameof(events));

            Currency = Events[0].Currency;

            for (int i = 1; i < Events.Count; i++)
                if (Events[i].Currency !=  Currency) throw new CurrencyMismatchException(Currency, Events[i].Currency);
        }

        /// <summary>
        /// Returns a new <see cref="ClaimHistory"/> with the specified events appended.
        /// </summary>
        /// <param name="events">The events to append.</param>
        /// <returns>A new claim history containing the existing and appended events.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="events"/> is <c>null</c>.
        /// </exception>
        public ClaimHistory AddEvents(IEnumerable<ClaimHistoryEvent> events)
        {
            ArgumentNullException.ThrowIfNull(events);

            return new(ClaimID, OccurrenceDate, ReportingDate, Events.Concat(events));
        }

        /// <summary>
        /// Converts this claim history to reserving model claim events.
        /// </summary>
        /// <returns>The corresponding reserving model claim events.</returns>
        public IEnumerable<ClaimEvent> ToClaimEvents()
        {
            AccidentOccurrence accidentOccurrence = new(OccurrenceDate, ClaimID);
            AccidentReport accidentReport = new(ReportingDate, accidentOccurrence);

            return Events.Select(e => new ClaimEvent(e.Date, e.Amount, e.ClaimEventType, accidentReport));
        }
    }
}
