using ActuarialForge.Primitives;

namespace ActuarialForge.EconomicRegister
{
    /// <summary>
    /// Represents a single economic event recorded in an economic register.
    /// </summary>
    /// <remarks>
    /// An <see cref="EconomicRegisterEntry"/> models a financial transaction occurring at a specific 
    /// <see cref="ModelTime"/> within an actuarial model. 
    /// Typical examples include claim payments, recoveries, reinsurance transactions,
    /// premium cashflows, or expense allocations.
    /// The entry is immutable and uniquely identified by its <see cref="EventID"/>.
    /// </remarks>
    public sealed record EconomicRegisterEntry
    {
        /// <summary>
        /// Gets the unique identifier of the economic event.
        /// </summary>
        /// <remarks>
        /// This identifier is used to distinguish entries within an economic register.
        /// It should be unique across the entire register.
        /// </remarks>
        public Guid EventID { get; init; }

        /// <summary>
        /// Gets the model time at which the economic event occurs.
        /// </summary>
        /// <remarks>
        /// The model time determines the temporal allocation of the event
        /// within a discrete-time actuarial framework.
        /// </remarks>
        public ModelTime Time { get; init; }

        /// <summary>
        /// Gets the monetary amount associated with the event.
        /// </summary>
        /// <remarks>
        /// The sign convention (positive or negative) depends on the
        /// register's accounting interpretation and should be applied consistently.
        /// </remarks>
        public Money Amount { get; init; }

        /// <summary>
        /// Gets the logical category of the economic event.
        /// </summary>
        /// <remarks>
        /// Categories can be used to classify events such as claim payments,
        /// recoveries, premiums, or expenses.
        /// </remarks>
        public string Category { get; init; }

        /// <summary>
        /// Gets the source system or origin of the event.
        /// </summary>
        /// <remarks>
        /// This value can be used to track the origin of the transaction,
        /// for example a claims system, reinsurance system, or simulation engine.
        /// </remarks>
        public string Source { get; init; }

        /// <summary>
        /// Gets an optional identifier of the source object related to this event.
        /// </summary>
        /// <remarks>
        /// This can reference an external entity such as a claim number,
        /// policy identifier, or reinsurance contract ID.
        /// </remarks>
        public string? SourceId { get; init; }


        /// <summary>
        /// Initializes a new instance of the <see cref="EconomicRegisterEntry"/> record.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <param name="time">The model time at which the event occurs.</param>
        /// <param name="amount">The monetary amount of the event.</param>
        /// <param name="category">The logical classification of the event.</param>
        /// <param name="source">The original event triggering the event.</param>
        /// <param name="sourceId">
        /// An optional external reference identifier associated with the event.
        /// </param>
        public EconomicRegisterEntry(Guid eventId, ModelTime time, Money amount, string category, string source, string? sourceId = null)
        {
            EventID = eventId;
            Time = time;
            Amount = amount;
            Category = category;
            Source = source;
            SourceId = sourceId;
        }
    }
}
