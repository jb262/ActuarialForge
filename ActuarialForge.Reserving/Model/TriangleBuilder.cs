using ActuarialForge.Primitives;

namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Builds an <see cref="IncrementalTriangle"/> from a collection of claim events.
    /// </summary>
    /// <param name="currency">The currency of the triangle to be built.</param>
    /// <param name="timeGranularity">The reserving time granularity to use.</param>
    /// <param name="claimDateBasis">
    /// The claim date basis used to assign claim events to accident periods.
    /// </param>
    /// <remarks>
    /// <see cref="TriangleBuilder"/> collects <see cref="ClaimEvent"/> instances and
    /// creates an <see cref="IncrementalTriangle"/> using the configured
    /// <see cref="ReservingTimeGranularity"/> and <see cref="ClaimDateBasis"/>.
    /// All added claim events must use the same <see cref="Currency"/>.
    /// </remarks>
    public sealed class TriangleBuilder(Currency currency, ReservingTimeGranularity timeGranularity, ClaimDateBasis claimDateBasis)
    {
        private readonly List<ClaimEvent> _claimEvents = [];

        /// <summary>
        /// Gets the currency of the triangle being built.
        /// </summary>
        public Currency Currency { get; } = currency;

        /// <summary>
        /// Gets the reserving time granularity used when building the triangle.
        /// </summary>
        public ReservingTimeGranularity TimeGranularity { get; } = timeGranularity;

        /// <summary>
        /// Gets the claim date basis used when assigning claim events to accident periods.
        /// </summary>
        public ClaimDateBasis ClaimDateBasis { get; } = claimDateBasis;

        /// <summary>
        /// Adds a single claim event to the builder.
        /// </summary>
        /// <param name="claimEvent">The claim event to add.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="claimEvent"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="CurrencyMismatchException">
        /// Thrown if the claim event uses a different currency than the builder.
        /// </exception>
        public void AddClaimEvent(ClaimEvent claimEvent)
        {
            ArgumentNullException.ThrowIfNull(claimEvent);

            if (claimEvent.Amount.Currency != Currency)
                throw new CurrencyMismatchException(Currency, claimEvent.Amount.Currency);

            _claimEvents.Add(claimEvent);
        }

        /// <summary>
        /// Adds multiple claim events to the builder.
        /// </summary>
        /// <param name="claimEvents">The claim events to add.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="claimEvents"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="CurrencyMismatchException">
        /// Thrown if at least one claim event uses a different currency than the builder.
        /// </exception>
        public void AddClaimEvents(IEnumerable<ClaimEvent> claimEvents)
        {
            ArgumentNullException.ThrowIfNull(claimEvents);

            foreach (ClaimEvent claimEvent in claimEvents)
                AddClaimEvent(claimEvent);
        }

        /// <summary>
        /// Builds an <see cref="IncrementalTriangle"/> from the collected claim events.
        /// </summary>
        /// <returns>The constructed incremental triangle.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no claim events have been added.
        /// </exception>
        public IncrementalTriangle Build()
        {
            if (_claimEvents.Count == 0)
                throw new InvalidOperationException("Cannot create a triangle without any data.");

            return new(TimeGranularity, ClaimDateBasis, Currency, _claimEvents);
        }
    }
}
