namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Specifies which time axis is used for indexing a claims triangle.
    /// Determines whether the triangle is organized by accident date or reporting date.
    /// </summary>
    public enum ClaimDateBasis
    {
        /// <summary>
        /// Triangle is indexed by the accident occurrence date.
        /// </summary>
        AccidentDate,

        /// <summary>
        /// Triangle is indexed by the claim reporting date.
        /// </summary>
        ReportDate
    }
}
