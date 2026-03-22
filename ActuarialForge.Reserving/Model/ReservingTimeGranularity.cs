namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Specifies the time granularity of a period used in actuarial modelling, such as accident or development periods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The numeric value of each enum member represents the number of periods per year. This allows direkct use in actuarial
    /// calculations (e.g. triangle indexing, development factor alignment or periodic arithmetic.
    /// </para>
    /// <para>
    /// Weekly granularity is defined as exactly 52 periods per year.
    /// It represents a technical discretization (1/52 of a year) and does not follow ISO calendar week conventions.
    /// </para>
    /// <para>
    /// This enumeration is intended for discrete reserving models (e.g. Chain-Ladder) where time is modelled in evenly
    /// spaced development steps.
    /// </para>
    /// </remarks>
    public enum ReservingTimeGranularity
    {
        /// <summary>
        /// One period per year.
        /// </summary>
        Annual = 1,

        /// <summary>
        /// Two periods per year.
        /// </summary>
        SemiAnnual = 2,

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
