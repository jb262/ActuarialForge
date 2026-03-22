namespace ActuarialForge.EconomicRegister
{
    /// <summary>
    /// Defines a provider that supplies economic events.
    /// </summary>
    /// <typeparam name="TEvent">
    /// The type of economic event returned by the provider.
    /// </typeparam>
    /// <remarks>
    /// An <see cref="IEconomicEventProvider{TEvent}"/> exposes a read-only
    /// collection of economic events originating from a register, projection,
    /// simulation engine, or other domain-specific source.
    /// The interface does not prescribe ordering, filtering, or persistence semantics.
    /// Implementations should document whether the returned collection represents
    /// a materialized snapshot or is computed dynamically.
    /// </remarks>
    public interface IEconomicEventProvider<out TEvent> where TEvent : IEconomicEvent
    {
        /// <summary>
        /// Gets the economic events provided by this instance.
        /// </summary>
        /// <returns>
        /// A read-only collection of economic events.
        /// </returns>
        IReadOnlyCollection<TEvent> GetEvents();
    }
}
