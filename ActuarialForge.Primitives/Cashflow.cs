using System.Collections;

namespace ActuarialForge.Primitives
{
    /// <summary>
    /// Represents a time-ordered cashflow in a single currency on a discrete model timeline.
    /// </summary>
    /// <remarks>
    /// A <see cref="Cashflow"/> is a collection of <see cref="CashflowItem"/> instances indexed by their
    /// <see cref="ModelTime"/>. The cashflow is single-currency: all items must use the same <see cref="Currency"/>.
    /// If multiple items with the same <see cref="ModelTime"/> are provided, they are aggregated into a single
    /// cashflow item per time point.
    /// </remarks>
    public sealed class Cashflow(Currency currency) : IEnumerable<CashflowItem>
    {
        private readonly SortedList<ModelTime, CashflowItem> _items = [];

        /// <summary>
        /// Gets the currency in which this cashflow is denominated.
        /// </summary>
        /// <remarks>
        /// A <see cref="Cashflow"/> is single-currency: all contained <see cref="CashflowItem"/> amounts
        /// must use the same <see cref="Currency"/>. Items with a different currency are rejected
        /// during construction and when merging cashflows.
        /// </remarks>
        public Currency Currency { get; } = currency;

        /// <summary>
        /// Gets the payment frequency metadata associated with this cashflow, if available.
        /// </summary>
        /// <remarks>
        /// This metadata is optional and is typically set when a cashflow is constructed from a sequence of amounts
        /// using period-based timing conventions.
        /// </remarks>
        public PaymentFrequency? PaymentFrequency { get; }

        /// <summary>
        /// Gets the payment timing metadata associated with this cashflow, if available.
        /// </summary>
        /// <remarks>
        /// This metadata is optional and is typically set when a cashflow is constructed from a sequence of amounts
        /// using period-based timing conventions.
        /// </remarks>
        public PaymentTiming? PaymentTiming { get; }

        /// <summary>
        /// Gets the number of (aggregated) cashflow items contained in this cashflow.
        /// </summary>
        public int Count { get => _items.Count; }

        /// <summary>
        /// Initializes a new <see cref="Cashflow"/> from a sequence of cashflow items.
        /// </summary>
        /// <param name="items">The cashflow items.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="items"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="items"/> is empty.</exception>
        /// <exception cref="CurrencyMismatchException">
        /// Thrown if <paramref name="items"/> contains amounts with different currencies.
        /// </exception>
        /// <remarks>
        /// The cashflow currency is determined from the first item.
        /// </remarks>
        public Cashflow(IEnumerable<CashflowItem> items) : this(GetCurrencyOrThrow(items))
        {

            foreach (var item in items)
            {
                if (item.Amount.Currency != Currency)
                    throw new CurrencyMismatchException(item.Amount.Currency, Currency);


                if (_items.TryGetValue(item.Time, out var cashflow))
                    cashflow += item;
                else
                    cashflow = new(item.Amount, item.Time);

                _items[item.Time] = cashflow;
            }
        }

        /// <summary>
        /// Initializes a new <see cref="Cashflow"/> from a sequence of amounts and period-based timing conventions.
        /// </summary>
        /// <param name="amounts">The cashflow amounts.</param>
        /// <param name="currency">The currency of all amounts.</param>
        /// <param name="paymentFrequency">The payment frequency (periods per year).</param>
        /// <param name="paymentTiming">Whether payments occur in advance or in arrears.</param>
        public Cashflow(IEnumerable<decimal> amounts, Currency currency, PaymentFrequency paymentFrequency, PaymentTiming paymentTiming)
            : this(CreateCashflowItems(amounts, currency, paymentFrequency, paymentTiming))
        {
            PaymentFrequency = paymentFrequency;
            PaymentTiming = paymentTiming;
        }

        private static IEnumerable<CashflowItem> CreateCashflowItems(IEnumerable<decimal> amounts, Currency currency, PaymentFrequency paymentFrequency, PaymentTiming paymentTiming)
        {
            using var enumerator = amounts.GetEnumerator();
            int i = 0;

            while (enumerator.MoveNext())
            {
                Money money = new(enumerator.Current, currency);
                CashflowItem cashflowItem = new(money, ++i, paymentFrequency, paymentTiming);
                yield return cashflowItem;
            }
        }

        private static Currency GetCurrencyOrThrow(IEnumerable<CashflowItem> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            using var e = items.GetEnumerator();
            if (!e.MoveNext())
                throw new ArgumentException("No cashflow items provided.", nameof(items));

            return e.Current.Amount.Currency;
        }

        /// <summary>
        /// Gets the total amount of the cashflow (sum of all aggregated items).
        /// </summary>
        public Money TotalAmount
        {
            get
            {
                Money total = new(Currency);
                foreach (var item in _items)
                {
                    total += item.Value.Amount;
                }

                return total;
            }
        }

        /// <summary>
        /// Returns a new cashflow representing the result of merging <paramref name="other"/> into this cashflow.
        /// </summary>
        /// <param name="other">The cashflow to merge into this cashflow.</param>
        /// <returns>
        /// A new cashflow where each item from <paramref name="other"/> is added to the matching time point if present,
        /// or inserted as a new time point otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="other"/> is <c>null</c>.</exception>
        /// <exception cref="CurrencyMismatchException">Thrown if the cashflows are denominated in different currencies.</exception>
        public Cashflow Add(Cashflow other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (other.Count == 0)
                return this;

            if (other.Currency != Currency)
                throw new CurrencyMismatchException(Currency, other.Currency);

            SortedList<ModelTime, CashflowItem> resultItems = new(_items);

            foreach (var item in other)
            {
                if (resultItems.TryGetValue(item.Time, out var existing))
                    resultItems[item.Time] = existing + item;
                else
                    resultItems[item.Time] = item;
            }

            return new Cashflow(resultItems.Values);
        }

        /// <summary>
        /// Returns a new cashflow where each item has been discounted using the provided discount curve.
        /// </summary>
        /// <param name="discountCurve">
        /// The discount curve used to obtain discount factors for each item time.
        /// </param>
        /// <returns>
        /// A new <see cref="Cashflow"/> containing the discounted cashflow items.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="discountCurve"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="MissingDiscountFactorException">
        /// Thrown if the discount curve does not provide a valid discount factor for one or more
        /// cashflow item times. The exception contains the list of affected cashflow items.
        /// </exception>
        /// <exception cref="DiscountDateMismatchException">
        /// Thrown if a discount factor is retrieved whose time does not match the time of the
        /// corresponding cashflow item.
        /// </exception>
        /// <remarks>
        /// For each <see cref="CashflowItem"/> contained in this cashflow, a corresponding
        /// <see cref="DiscountFactor"/> is requested from the provided <see cref="DiscountCurve"/>.
        /// If one or more discount factors are missing, all affected items are collected and
        /// reported via a single <see cref="MissingDiscountFactorException"/>.
        /// </remarks>
        public Cashflow Discount(DiscountCurve discountCurve)
        {
            ArgumentNullException.ThrowIfNull(discountCurve);

            List<CashflowItem> discountedCashflows = [];
            List<CashflowItem> missing = [];

            foreach (var item in this)
            {
                try
                {
                    DiscountFactor discountFactor = discountCurve.GetDiscountFactor(item.Time);
                    CashflowItem discountedCashflowItem = discountFactor.Apply(item);
                    discountedCashflows.Add(discountedCashflowItem);
                }
                catch (InvalidOperationException)
                {
                    missing.Add(item);
                }
            }

            if (missing.Count > 0)
                throw new MissingDiscountFactorException(missing);

            return new(discountedCashflows);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the cashflow items in ascending time order.
        /// </summary>
        public IEnumerator<CashflowItem> GetEnumerator()
            => _items.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
