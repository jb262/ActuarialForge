using ActuarialForge.Primitives;

namespace ActuarialForge.EconomicRegister
{
    /// <summary>
    /// Represents a read-only economic register that stores and manages economic events identified by a unique identifier.
    /// </summary>
    /// <remarks>
    /// An <see cref="IEconomicRegister"/> acts as a central ledger for economic transactions such as claim payments,
    /// recoveries, reinsurance transactions, or other financial events relevant in actuarial modeling.
    /// Entries are uniquely identified by a <see cref="Guid"/> and can be accessed in a dictionary-like manner.
    /// Implementations are responsible for enforcing uniqueness and ensuring data consistency.
    /// </remarks>
    public interface IEconomicRegister : IReadOnlyDictionary<Guid, EconomicRegisterEntry>
    {
        /// <summary>
        /// Gets all recorded economic register entries.
        /// </summary>
        /// <remarks>
        /// This collection represents the current state of the register.
        /// The ordering of entries is implementation-specific unless explicitly documented otherwise.
        /// </remarks>
        IReadOnlyList<EconomicRegisterEntry> Entries { get; }

        /// <summary>
        /// Records a new economic register entry in the register.
        /// </summary>
        /// <param name="entry">
        /// The <see cref="EconomicRegisterEntry"/> to record.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if an entry with the same identifier already exists in the register.
        /// </exception>
        void Record(EconomicRegisterEntry entry);

        /// <summary>
        /// Calculates the current balance of the register in the specified currency.
        /// </summary>
        /// <param name="currency">
        /// The currency in which the balance should be calculated.
        /// </param>
        /// <returns>
        /// A <see cref="Money"/> instance representing the aggregated balance
        /// of all entries denominated in the specified currency.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if entries with incompatible currencies are encountered.
        /// </exception>
        Money Balance(Currency currency);

        /// <summary>
        /// Converts the economic register entries into a cashflow representation.
        /// </summary>
        /// <param name="currency">
        /// The currency in which the resulting cashflow should be expressed.
        /// </param>
        /// <returns>
        /// A <see cref="Cashflow"/> representing all recorded economic entries
        /// aggregated and ordered by their model time.
        /// </returns>
        /// <remarks>
        /// The resulting cashflow typically aggregates entries occurring at the same model time.
        /// The ordering of cashflow items follows the chronological order defined by the underlying model time.
        /// </remarks>
        Cashflow ToCashflow(Currency currency);
    }
}
