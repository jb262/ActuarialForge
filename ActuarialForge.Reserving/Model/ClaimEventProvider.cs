using ActuarialForge.EconomicRegister;

namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Provides <see cref="ClaimEvent"/> instances reconstructed from an economic register.
    /// </summary>
    /// <param name="register">
    /// The economic register containing the source entries from which reserving events are reconstructed.
    /// </param>
    /// <remarks>
    /// The provider interprets register entries as reserving domain events and reconstructs the
    /// corresponding event hierarchy consisting of:
    /// <list type="number">
    /// <item><description><see cref="AccidentOccurrence"/></description></item>
    /// <item><description><see cref="AccidentReport"/></description></item>
    /// <item><description><see cref="ClaimEvent"/></description></item>
    /// </list>
    /// Reconstructed objects are cached after the first access.
    /// </remarks>
    public sealed class ClaimEventProvider(IEconomicRegister register) : IEconomicEventProvider<ClaimEvent>
    {
        private readonly IReadOnlyList<EconomicRegisterEntry> _entries = register?.Entries ?? throw new ArgumentNullException(nameof(register));

        private Dictionary<Guid, AccidentOccurrence>? _occurrences;
        private Dictionary<Guid, AccidentReport>? _reports;
        private Dictionary<Guid, ClaimEvent>? _events;

        private ClaimEvent[]? _result;

        /// <summary>
        /// Gets the reconstructed claim events from the economic register.
        /// </summary>
        /// <returns>
        /// A read-only collection of reconstructed <see cref="ClaimEvent"/> instances.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the register contains inconsistent or incomplete event chains,
        /// for example when required accident occurrences or accident reports are missing.
        /// </exception>
        public IReadOnlyCollection<ClaimEvent> GetEvents()
        {
            _result ??= GetClaimEvents().Values.ToArray();
            return _result.AsReadOnly();
        }

        private Dictionary<Guid, AccidentOccurrence> GetAccidentOccurrences()
        {
            if (_occurrences is null)
            {
                _occurrences = [];

                foreach (var entry in _entries.Where(e => e.Category == nameof(AccidentOccurrence)))
                    _occurrences.Add(entry.EventID, new(entry.EventID, entry.Time, entry.Source));
            }

            return _occurrences;
        }

        private Dictionary<Guid, AccidentReport> GetAccidentReports()
        {
            if (_reports is null)
            {
                _reports = [];

                foreach (var entry in _entries.Where(e => e.Category == nameof(AccidentReport)))
                {
                    if (entry.SourceId is null)
                        throw new InvalidOperationException("A claim cannot be reported without an occurrence of an accident.");

                    if (!Guid.TryParse(entry.SourceId, out Guid sourceId))
                        throw new InvalidOperationException($"Invalid source id '{entry.SourceId}'.");

                    if (!GetAccidentOccurrences().TryGetValue(sourceId, out var occurrence))
                        throw new InvalidOperationException($"Accident occurrence {sourceId} not found.");

                    _reports.Add(entry.EventID, new(entry.EventID, entry.Time, occurrence));
                }
            }

            return _reports;
        }

        private Dictionary<Guid, ClaimEvent> GetClaimEvents()
        {
            if (_events is null)
            {
                _events = [];

                foreach (var entry in _entries)
                {
                    bool isInfrastructureEvent =
                        entry.Category == nameof(AccidentOccurrence) ||
                        entry.Category == nameof(AccidentReport);

                    if (isInfrastructureEvent)
                        continue;

                    if (!Enum.TryParse<ClaimEventType>(entry.Category, out var eventType))
                        continue;

                    if (entry.SourceId is null)
                        throw new InvalidOperationException("A claim cannot have any events without being reported.");

                    if (!Guid.TryParse(entry.SourceId, out Guid sourceId))
                        throw new InvalidOperationException($"Invalid source id '{entry.SourceId}'.");

                    if (!GetAccidentReports().TryGetValue(sourceId, out var report))
                        throw new InvalidOperationException($"Claim report {sourceId} not found.");

                    _events.Add(
                        entry.EventID,
                        new ClaimEvent(entry.EventID, entry.Time, entry.Amount, eventType, report));
                }
            }

            return _events;
        }
    }
}
