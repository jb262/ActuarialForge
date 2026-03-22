namespace ActuarialForge.Utils
{
    /// <summary>
    /// Provides regression-related utility methods.
    /// </summary>
    public static class Regression
    {
        /// <summary>
        /// Computes the parameters of a simple linear regression.
        /// </summary>
        /// <param name="x">The explanatory variable values.</param>
        /// <param name="y">The dependent variable values.</param>
        /// <param name="includeIntercept">
        /// <c>true</c> to fit a regression with intercept;
        /// <c>false</c> to fit a regression through the origin.
        /// </param>
        /// <returns>The estimated regression parameters.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="x"/> or <paramref name="y"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="x"/> and <paramref name="y"/> do not have the same length,
        /// or if fewer than two observations are provided.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown if the regression parameters are not identifiable because the required denominator is zero.
        /// </exception>
        public static RegressionParameters SimpleLinearRegression(IReadOnlyList<decimal> x, IReadOnlyList<decimal> y, bool includeIntercept)
        {
            ArgumentNullException.ThrowIfNull(x);
            ArgumentNullException.ThrowIfNull(y);

            if (x.Count != y.Count)
                throw new ArgumentException("x and y must have the same length.");

            if (x.Count < 2)
                throw new ArgumentException("At least two observations required.");

            decimal sumX = decimal.Zero;
            decimal sumY = decimal.Zero;
            decimal sumXX = decimal.Zero;
            decimal sumXY = decimal.Zero;

            for (int i = 0; i < x.Count; i++)
            {
                sumX += x[i];
                sumY += y[i];
                sumXX += x[i] * x[i];
                sumXY += x[i] * y[i];
            }

            if (includeIntercept)
            {
                decimal denominator = x.Count * sumXX - sumX * sumX;

                if (denominator == decimal.Zero)
                    throw new DivideByZeroException("Cannot compute a linear regression with intercept if all x values are identical.");

                decimal slope = (x.Count * sumXY - sumX * sumY) / denominator;
                decimal intercept = (sumY - slope * sumX) / x.Count;

                return RegressionParameters.SimpleLinearRegressionCoefficients(intercept, slope);
            }
            else
            {
                if (sumXX == decimal.Zero)
                    throw new DivideByZeroException("Cannot compute a linear regression through the origin if all x values are zero.");

                decimal slope = sumXY / sumXX;
                return RegressionParameters.SimpleLinearRegressionCoefficients(slope);
            }
        }
    }
}