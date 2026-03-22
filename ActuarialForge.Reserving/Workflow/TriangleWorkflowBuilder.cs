using ActuarialForge.Primitives;
using ActuarialForge.Reserving.Model;

namespace ActuarialForge.Reserving.Workflow
{
    /// <summary>
    /// Represents a workflow builder for creating incremental reserving triangles
    /// from claim histories.
    /// </summary>
    /// <remarks>
    /// <see cref="TriangleWorkflowBuilder"/> collects the workflow configuration
    /// required to transform claim histories into an <see cref="IncrementalTriangle"/>.
    /// Instances are immutable and return modified copies when workflow options are changed.
    /// </remarks>
    public sealed record TriangleWorkflowBuilder
    {
        /// <summary>
        /// Gets the claim histories used as the input of the workflow.
        /// </summary>
        public IReadOnlyList<ClaimHistory>? Claims { get; private set; }

        /// <summary>
        /// Gets the claim date basis used to build the triangle.
        /// </summary>
        public ClaimDateBasis? ClaimDateBasis { get; private set; }

        /// <summary>
        /// Gets the reserving time granularity used to build the triangle.
        /// </summary>
        public ReservingTimeGranularity? TimeGranularity { get; private set; }

        /// <summary>
        /// Gets the currency of the workflow input.
        /// </summary>
        public Currency? Currency { get; private set; }

        /// <summary>
        /// Returns a new builder configured with the specified claim histories.
        /// </summary>
        /// <param name="claims">The claim histories.</param>
        /// <returns>A new builder containing the specified claim histories.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="claims"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if no claim histories are provided.
        /// </exception>
        /// <exception cref="CurrencyMismatchException">
        /// Thrown if the supplied claim histories do not all use the same currency.
        /// </exception>
        public TriangleWorkflowBuilder FromClaimHistories(IEnumerable<ClaimHistory> claims)
        {
            ArgumentNullException.ThrowIfNull(claims);

            var claimsArr = claims.ToArray();
            if (claimsArr.Length == 0)
                throw new ArgumentException("No claims provided.");
            var currency = claimsArr[0].Currency;

            for (int i = 1; i < claimsArr.Length; i++)
                if (claimsArr[i].Currency != currency) throw new CurrencyMismatchException(currency, claimsArr[i].Currency);

            return this with { Claims = Array.AsReadOnly(claimsArr), Currency = currency };
        }

        /// <summary>
        /// Returns a new builder configured with the specified reserving time granularity.
        /// </summary>
        /// <param name="timeGranularity">The reserving time granularity.</param>
        /// <returns>A new builder containing the specified time granularity.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="timeGranularity"/> is not a valid enumeration value.
        /// </exception>
        public TriangleWorkflowBuilder WithTimeGranularity(ReservingTimeGranularity timeGranularity)
        {
            if (!Enum.IsDefined(timeGranularity))
                throw new ArgumentOutOfRangeException(nameof(timeGranularity));

            return this with { TimeGranularity = timeGranularity };
        }

        /// <summary>
        /// Returns a new builder configured with the specified claim date basis.
        /// </summary>
        /// <param name="claimDateBasis">The claim date basis.</param>
        /// <returns>A new builder containing the specified claim date basis.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="claimDateBasis"/> is not a valid enumeration value.
        /// </exception>
        public TriangleWorkflowBuilder UsingClaimDateBasis(ClaimDateBasis claimDateBasis)
        {
            if (!Enum.IsDefined(claimDateBasis))
                throw new ArgumentOutOfRangeException(nameof(claimDateBasis));

            return this with { ClaimDateBasis = claimDateBasis };
        }

        /// <summary>
        /// Builds an incremental triangle from the current workflow configuration.
        /// </summary>
        /// <returns>The resulting incremental triangle.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if required workflow settings or claim histories are missing.
        /// </exception>
        public IncrementalTriangle Build()
        {
            if (Currency is null)
                throw new InvalidOperationException("No currency provided.");

            if (Claims is null || Claims.Count == 0)
                throw new InvalidOperationException("No claims provided.");

            if (ClaimDateBasis is null)
                throw new InvalidOperationException("No claim date basis provided.");

            if (TimeGranularity is null)
                throw new InvalidOperationException("No reserving time granularity provided.");

            return new(TimeGranularity.Value, ClaimDateBasis.Value, Currency, Claims.SelectMany(c => c.ToClaimEvents()));
        }

    }
}
