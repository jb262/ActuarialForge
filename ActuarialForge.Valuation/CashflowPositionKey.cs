using ActuarialForge.Primitives;

namespace ActuarialForge.Valuation
{
    /// <summary>
    /// Represents an internal key identifying a cashflow position
    /// by its model time and optional payment metadata.
    /// </summary>
    /// <remarks>
    /// This type is used for grouping and aggregating cashflow positions
    /// in valuation logic.
    /// 
    /// Two keys are considered equal if their <see cref="Time"/>,
    /// <see cref="PaymentFrequency"/>, and <see cref="PaymentTiming"/>
    /// are equal.
    /// </remarks>
    internal readonly struct CashflowPositionKey(CashflowItem item, PaymentFrequency? paymentFrequency, PaymentTiming? paymentTiming) : IEquatable<CashflowPositionKey>
    {
        public ModelTime Time { get; } = item.Time;

        public PaymentFrequency? PaymentFrequency { get; } = paymentFrequency;

        public PaymentTiming? PaymentTiming { get; } = paymentTiming;

        public bool Equals(CashflowPositionKey other)
        {
            return Time.Equals(other.Time) &&
                PaymentFrequency == other.PaymentFrequency &&
                PaymentTiming == other.PaymentTiming;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;
            else
                return obj is CashflowPositionKey other && Equals(other);
        }

        public override int GetHashCode()
            => HashCode.Combine(Time, PaymentFrequency, PaymentTiming);

        public static bool operator ==(CashflowPositionKey left, CashflowPositionKey right)
            => left.Equals(right);

        public static bool operator !=(CashflowPositionKey left, CashflowPositionKey right)
            => !left.Equals(right);
    }
}