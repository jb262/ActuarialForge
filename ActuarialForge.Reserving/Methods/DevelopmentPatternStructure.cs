namespace ActuarialForge.Reserving.Methods
{
    /// <summary>
    /// Specifies the structural interpretation of a development pattern.
    /// </summary>
    /// <remarks>
    /// The structure determines how development factors are interpreted and accumulated.
    /// </remarks>
    public enum DevelopmentPatternStructure
    {
        /// <summary>
        /// Represents a multiplicative development pattern, where factors are applied
        /// through cumulative products.
        /// </summary>
        Multiplicative,

        /// <summary>
        /// Represents an additive (incremental loss) development pattern, where factors
        /// are applied through cumulative sums.
        /// </summary>
        IncrementalLoss
    }
}
