namespace ActuarialForge.Reserving.Validation
{
    /// <summary>
    /// Defines a strategy for comparing reserving methods and selecting the best one.
    /// </summary>
    /// <typeparam name="TReservingMethod">The type of reserving method to evaluate.</typeparam>
    /// <typeparam name="TScore">The type of score used to evaluate methods.</typeparam>
    /// <remarks>
    /// Implementations of this interface assign scores to candidate reserving methods
    /// and determine which method performs best according to the scoring logic.
    /// </remarks>
    public interface IReservingMethodSelector<TReservingMethod, TScore>
    {
        /// <summary>
        /// Selects the best reserving method from the specified candidates.
        /// </summary>
        /// <param name="methods">The reserving methods to compare.</param>
        /// <returns>The best-performing reserving method.</returns>
        TReservingMethod SelectBest(params TReservingMethod[] methods);

        /// Computes the scores of the specified reserving methods.
        /// </summary>
        /// <param name="methods">The reserving methods to evaluate.</param>
        /// <returns>
        /// A dictionary mapping each reserving method to its corresponding score.
        /// </returns>
        IReadOnlyDictionary<TReservingMethod, TScore> Scores(params TReservingMethod[] methods);
    }
}
