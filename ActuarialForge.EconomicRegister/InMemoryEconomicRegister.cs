using ActuarialForge.Primitives;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace ActuarialForge.EconomicRegister
{
    /// <summary>
    /// Provides an in-memory implementation of <see cref="IEconomicRegister"/> backed by a dictionary.
    /// </summary>
    /// <remarks>
    /// This register stores <see cref="EconomicRegisterEntry"/> instances keyed by their unique event identifier.
    /// It is intended for in-process usage such as actuarial analytics, deterministic valuation workflows,
    /// and simulation scenarios where persistence is not required.
    ///
    /// The register supports multiple currencies. Aggregation methods such as <see cref="Balance(Currency)"/> and
    /// <see cref="ToCashflow(Currency)"/> operate on a single specified currency and do not perform currency conversion.
    /// This class is not thread-safe.
    /// </remarks>
    public sealed class InMemoryEconomicRegister : IEconomicRegister
    {
        private readonly Dictionary<Guid, EconomicRegisterEntry> _entries = [];

        public IEnumerable<Guid> Keys { get => _entries.Keys; }

        public IEnumerable<EconomicRegisterEntry> Values { get => _entries.Values; }

        public int Count {  get => _entries.Count; }

        public IReadOnlyList<EconomicRegisterEntry> Entries { get => _entries.Values.ToList().AsReadOnly(); }

        public EconomicRegisterEntry this[Guid key] => _entries[key];
 
        public void Record(EconomicRegisterEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            _entries.Add(entry.EventID, entry);
        }

        public Money Balance(Currency currency)
        {
            decimal balance = _entries.Values
                .Where(e => e.Amount.Currency == currency)
                .Sum(e => e.Amount.Amount);
            return new(balance, currency);
        }

        public Cashflow ToCashflow(Currency currency)
        {
            var items = _entries.Values
                .Where(e => e.Amount.Currency == currency)
                .GroupBy(e => e.Time)
                .Select(g => new CashflowItem(
                    new Money(g.Sum(x => x.Amount.Amount), currency),
                    g.Key));

            return new Cashflow(items);
        }

        public bool ContainsKey(Guid key)
            => _entries.ContainsKey(key);

        public bool TryGetValue(Guid key, [MaybeNullWhen(false)] out EconomicRegisterEntry value)
            => _entries.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<Guid, EconomicRegisterEntry>> GetEnumerator()
            => _entries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
