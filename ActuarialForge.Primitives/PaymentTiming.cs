namespace ActuarialForge.Primitives
{
    /// <summary>
    /// Specifies whether payments occur at the beginning or at the end of a period.
    /// </summary>
    /// <remarks>
    /// In a discrete-time framework, this value determines how period indices
    /// are converted into model times.
    /// </remarks>
    public enum PaymentTiming
    {
        /// <summary>
        /// Payment occurs at the beginning of the period (in advance).
        /// </summary>
        Advance,

        /// <summary>
        /// Payment occurs at the end of the period (in arrears).
        /// </summary>
        Arrears
    }
}
