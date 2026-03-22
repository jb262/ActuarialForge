namespace ActuarialForge.Reserving
{
    /// <summary>
    /// Provides extension methods for incremental and cumulative sequence transformations.
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Returns the cumulative scan of a sequence using the specified seed and accumulator.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="accumulator">
        /// A function that combines the current accumulated value with the next source element.
        /// </param>
        /// <returns>
        /// A sequence containing the intermediate accumulated values.
        /// </returns>
        public static IEnumerable<T> Scan<T>(this IEnumerable<T> source, T seed, Func<T, T, T> accumulator)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(accumulator);

            T current = seed;

            foreach (T item in source)
            {
                current = accumulator(current, item);
                yield return current;
            }
        }

        /// <summary>
        /// Returns the first-order differences of a sequence.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="substract">
        /// A function that subtracts the previous element from the current element.
        /// </param>
        /// <returns>
        /// A sequence whose first element equals the first source element, and whose
        /// subsequent elements are the differences between consecutive source elements.
        /// </returns>
        public static IEnumerable<T> Differences<T>(this IEnumerable<T> source, Func<T, T, T> subtract)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(subtract);

            using var enumerator = source.GetEnumerator();

            if (!enumerator.MoveNext()) yield break;

            T previous = enumerator.Current;
            yield return previous;

            while (enumerator.MoveNext())
            {
                T current = enumerator.Current;
                yield return subtract(current, previous);
                previous = current;
            }
        }
    }
}
