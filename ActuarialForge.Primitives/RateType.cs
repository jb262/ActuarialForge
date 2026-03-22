namespace ActuarialForge.Primitives
{
    /// <summary>
    /// Specifies the interpretation of interest rates provided to a <see cref="DiscountCurve"/>.
    /// </summary>
    /// <remarks>
    /// The rate type determines how discount factors are computed from the stored interest rates.
    /// </remarks>
    public enum RateType
    {
        /// <summary>
        /// Spot rates: a single rate applies to the entire period from time 0 to the specified model time.
        /// </summary>
        Spot,

        /// <summary>
        /// Forward rates: rates apply to individual periods and discount factors are computed
        /// as the product of per-period discount factors.
        /// </summary>
        Forward
    }
}
