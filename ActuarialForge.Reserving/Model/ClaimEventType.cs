namespace ActuarialForge.Reserving.Model
{
    /// <summary>
    /// Classifies the type of a claim related event within the reserving system.
    /// </summary>
    public enum ClaimEventType
    {
        /// <summary>
        /// Payment of a claim.
        /// </summary>
        ClaimPayment,

        /// <summary>
        /// Change in reserve.
        /// </summary>
        ReserveChange,

        /// <summary>
        /// Recovery / Regress payment.
        /// </summary>
        RecoveryPayment,

        /// <summary>
        /// Change in recovery reserve.
        /// </summary>
        RecoveryReserveChange,

        /// <summary>
        /// Expense / Cost related to the claim (e.g. handling cost).
        /// </summary>
        Expense
    }
}