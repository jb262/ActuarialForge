namespace ActuarialForge.Primitives
{
    /// <summary>
    /// Specifies the time convention used to determine the effective discounting
    /// point within a period.
    /// </summary>
    /// <remarks>
    /// In a discrete-time framework, interest rates are defined on a period grid.
    /// The discount convention determines whether discounting is applied at the
    /// beginning, middle, or end of each period.
    /// </remarks>
    public enum DiscountConvention
    {
        /// <summary>
        /// Discounting is applied at the beginning of each period.
        /// </summary>
        BeginningOfPeriod,

        /// <summary>
        /// Discounting is applied at the midpoint of each period.
        /// </summary>
        MidPeriod,

        /// <summary>
        /// Discounting is applied at the end of each period.
        /// </summary>
        EndOfPeriod
    }
}
