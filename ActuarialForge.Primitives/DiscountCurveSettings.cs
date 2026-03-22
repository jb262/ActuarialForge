namespace ActuarialForge.Primitives
{
    /// <summary>
    /// Defines configuration settings for constructing a <see cref="DiscountCurve"/>.
    /// </summary>
    /// <remarks>
    /// This settings object bundles the curve's time grid definition (<see cref="PaymentFrequency"/>),
    /// the discount timing convention (<see cref="DiscountConvention"/>), and the interpretation of the supplied
    /// interest rates (<see cref="RateType"/>).
    /// </remarks>
    public sealed record DiscountCurveSettings
    {
        /// <summary>
        /// Gets the payment frequency (periods per year) that defines the curve's discrete time grid.
        /// </summary>
        public PaymentFrequency PaymentFrequency { get; init; }

        /// <summary>
        /// Gets the discount convention describing where within a period discounting is applied.
        /// </summary>
        public DiscountConvention DiscountConvention { get; init; }

        /// <summary>
        /// Gets the type of interest rates supplied to the curve (e.g. spot rates or forward rates).
        /// </summary>
        public RateType RateType { get; init; }

        /// <summary>
        /// Initializes a new instance of <see cref="DiscountCurveSettings"/>.
        /// </summary>
        /// <param name="frequency">The payment frequency (periods per year) defining the curve grid.</param>
        /// <param name="discountConvention">The discount timing convention within each period.</param>
        /// <param name="rateType">The interpretation of the supplied rates (spot or forward).</param>
        public DiscountCurveSettings(PaymentFrequency frequency, DiscountConvention discountConvention, RateType rateType)
        {
            PaymentFrequency = frequency;
            DiscountConvention = discountConvention;
            RateType = rateType;
        }
    }
}
