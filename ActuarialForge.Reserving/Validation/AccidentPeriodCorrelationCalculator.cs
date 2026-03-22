using ActuarialForge.Reserving.Methods;
using ActuarialForge.Reserving.Model;
using ActuarialForge.Utils;

namespace ActuarialForge.Reserving.Validation
{
    /// <summary>
    /// Provides methods to compute accident-period correlations from reserving residuals.
    /// </summary>
    /// <remarks>
    /// The calculator derives correlations between consecutive accident periods
    /// based on the overlapping residual development paths.
    /// </remarks>
    public static class AccidentPeriodCorrelationCalculator
    {
        /// <summary>
        /// Computes the accident-period correlation triangle from the residuals implied by the specified reserving method.
        /// </summary>
        /// <param name="method">The reserving method used to derive residuals.</param>
        /// <returns>A correlation triangle based on accident-period residual correlations.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="method"/> is <c>null</c>.
        /// </exception>
        public static CorrelationTriangle Compute(IPatternBasedReservingMethod method)
            => Compute(new Residuals(method));

        /// <summary>
        /// Computes the accident-period correlation triangle from an observed cumulative triangle
        /// and the specified reserving method.
        /// </summary>
        /// <param name="triangle">The observed cumulative triangle.</param>
        /// <param name="method">The reserving method used to derive the projected triangle.</param>
        /// <returns>A correlation triangle based on accident-period residual correlations.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="triangle"/> or <paramref name="method"/> is <c>null</c>.
        /// </exception>
        public static CorrelationTriangle Compute(CumulativeTriangle triangle, IPatternBasedReservingMethod method)
            => Compute(new Residuals(triangle, method));

        /// <summary>
        /// Computes the accident-period correlation triangle from a residual collection.
        /// </summary>
        /// <param name="residuals">The residual collection.</param>
        /// <returns>A correlation triangle based on accident-period residual correlations.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="residuals"/> is <c>null</c>.
        /// </exception>
        public static CorrelationTriangle Compute(Residuals residuals)
            => Compute(residuals.ToTriangle());

        /// <summary>
        /// Computes the accident-period correlation triangle from a residual triangle.
        /// </summary>
        /// <param name="residualTriangle">The residual triangle.</param>
        /// <returns>A correlation triangle based on correlations between consecutive accident periods.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="residualTriangle"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown if a correlation cannot be computed because one of the residual rows has zero variance.
        /// </exception>
        public static CorrelationTriangle Compute(ResidualTriangle residualTriangle)
        {
            Dictionary<RunOffKey, decimal> correlations = [];

            for (int i = 0; i < residualTriangle.AccidentPeriods - 1; i++)
            {
                TriangleRow row = residualTriangle.GetTriangleRow(i);
                TriangleRow nextRow = residualTriangle.GetTriangleRow(i + 1);

                int commonDevelopmentPeriods = Math.Min(row.Count, nextRow.Count);

                if (commonDevelopmentPeriods < 2) continue;

                decimal meanRow = row.Take(commonDevelopmentPeriods).Average(m => m.Amount);
                decimal meanNextRow = nextRow.Take(commonDevelopmentPeriods).Average(m => m.Amount);

                decimal covariance = decimal.Zero;
                decimal rowVariance = decimal.Zero;
                decimal nextRowVariance = decimal.Zero;

                for (int j = 0; j < commonDevelopmentPeriods; j++)
                {
                    decimal dx = row[j].Amount - meanRow;
                    decimal dy = nextRow[j].Amount - meanNextRow;

                    covariance += dx * dy;
                    rowVariance += dx * dx;
                    nextRowVariance += dy * dy;
                }

                decimal denominator = DecimalMath.Sqrt(rowVariance * nextRowVariance);

                if (denominator == decimal.Zero)
                    throw new DivideByZeroException("Cannot compute accident period correlation for residual rows with zero variance.");

                decimal correlation = covariance / denominator;

                correlations[new(new(i), new(i + 1))] = correlation;
            }

            return new(correlations);
        }
    }
}
