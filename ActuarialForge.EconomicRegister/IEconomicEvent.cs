using ActuarialForge.Primitives;

namespace ActuarialForge.EconomicRegister
{
    /// <summary>
    /// Represents an economic event that can be posted to an economic register.
    /// </summary>
    /// <remarks>
    /// An <see cref="IEconomicEvent"/> models a domain-level financial event
    /// occurring at a specific <see cref="ModelTime"/>.
    ///
    /// Implementations define how the event translates into one or more
    /// <see cref="EconomicRegisterEntry"/> instances by posting themselves
    /// to a given <see cref="IEconomicRegister"/>.
    /// </remarks>
    public interface IEconomicEvent
    {
        /// <summary>
        /// Gets the unique identifier of the economic event.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the model time at which the event occurs.
        /// </summary>
        ModelTime Time { get; }

        /// <summary>
        /// Posts the event to the specified economic register.
        /// </summary>
        /// <param name="register">
        /// The economic register to which the event should be posted.
        /// </param>
        /// <remarks>
        /// Implementations are responsible for translating the event into
        /// one or more register entries using the provided register.
        /// </remarks>
        void Post(IEconomicRegister register);
    }
}
