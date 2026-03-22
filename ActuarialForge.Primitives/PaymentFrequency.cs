namespace ActuarialForge.Primitives
{
    /// <summary>
    /// Specifies the number of payment or compounding periods per year.
    /// </summary>
    /// <remarks>
    /// The numeric value of each enum member corresponds to the number of periods per year
    /// and is used directly in discounting and compounding calculations.
    /// </remarks>
    public enum PaymentFrequency
    {
        /// <summary>
        /// One period per year.
        /// </summary>
        Annually = 1,

        /// <summary>
        /// Two periods per year.
        /// </summary>
        SemiAnnually = 2,

        /// <summary>
        /// Four periods per year.
        /// </summary>
        Quarterly = 4,

        /// <summary>
        /// Twelve periods per year.
        /// </summary>
        Monthly = 12,

        /// <summary>
        /// Fifty-two periods per year.
        /// </summary>
        Weekly = 52
    }
}
