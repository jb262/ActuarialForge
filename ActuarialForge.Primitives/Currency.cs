using System.Reflection;

namespace ActuarialForge.Primitives
{
    /// <summary>
    /// Represents a currency identified by an ISO 4217 three-letter currency code and its number of minor units.
    /// </summary>
    /// <remarks>
    /// This type is an immutable value object. It stores the ISO 4217 currency code (e.g. <c>EUR</c>, <c>USD</c>)
    /// and the number of minor units (digits after the decimal separator) commonly used for rounding.
    ///
    /// The static properties defined on this type provide predefined currency instances.
    /// </remarks>
    public sealed record Currency
    {
        /// <summary>
        /// Gets the ISO 4217 three-letter currency code.
        /// </summary>
        public string ISO { get; }

        /// <summary>
        /// Gets the number of minor units used for the currency (i.e. digits after the decimal point).
        /// </summary>
        /// <remarks>
        /// This information can be used for rounding monetary amounts. For example, EUR has 2 minor units,
        /// while JPY has 0.
        /// </remarks>
        public int MinorUnits { get; }

        
        private Currency(string iso, int minorUnits)
        {
            ISO = iso.ToUpperInvariant();
            MinorUnits = minorUnits;
        }

        /// <summary>
        /// Returns the ISO currency code.
        /// </summary>
        public override string ToString() => ISO;

        /// <summary>
        /// Represents the absence of a currency.
        /// </summary>
        /// <remarks>
        /// This is a sentinel value and does not represent a valid ISO 4217 currency code.
        /// </remarks>
        public static Currency None { get; } = new("XXX", default);

        private static readonly Lazy<IReadOnlyDictionary<string, Currency>> _byIso =
            new(() =>
            {
                var dict = new Dictionary<string, Currency>(StringComparer.OrdinalIgnoreCase);

                var props = typeof(Currency).GetProperties(BindingFlags.Public | BindingFlags.Static)
                    .Where(p => p.PropertyType == typeof(Currency));

                foreach (var p in props)
                {
                    var c = (Currency?)p.GetValue(null);
                    if (c is null)
                        continue;

                    // Skip sentinel
                    if (ReferenceEquals(c, None) || string.IsNullOrWhiteSpace(c.ISO))
                        continue;

                    // If duplicates exist, fail fast (should never happen)
                    if (!dict.TryAdd(c.ISO, c))
                        throw new InvalidOperationException($"Duplicate currency ISO code detected: {c.ISO}");
                }

                return dict;
            });

        /// <summary>
        /// Attempts to get a predefined <see cref="Currency"/> instance by ISO 4217 code.
        /// </summary>
        public static bool TryFromIso(string iso, out Currency currency)
        {
            if (string.IsNullOrWhiteSpace(iso))
            {
                currency = None;
                return false;
            }

            return _byIso.Value.TryGetValue(iso.Trim(), out currency!);
        }

        /// <summary>
        /// Gets a predefined <see cref="Currency"/> instance by ISO 4217 code.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="iso"/> is not recognized.</exception>
        public static Currency FromIso(string iso)
            => TryFromIso(iso, out var c)
                ? c
                : throw new ArgumentException($"Unknown ISO 4217 currency code: '{iso}'.", nameof(iso));

        // Predefined currencies follow:
        public static Currency AED { get; } = new("AED", 2);

        public static Currency AFN { get; } = new("AFN", 2);

        public static Currency ALL { get; } = new("ALL", 2);

        public static Currency AMD { get; } = new("AMD", 2);

        public static Currency AOA { get; } = new("AOA", 2);

        public static Currency ARS { get; } = new("ARS", 2);

        public static Currency AUD { get; } = new("AUD", 2);

        public static Currency AWG { get; } = new("AWG", 2);

        public static Currency AZN { get; } = new("AZN", 2);

        public static Currency BAM { get; } = new("BAM", 2);

        public static Currency BBD { get; } = new("BBD", 2);

        public static Currency BDT { get; } = new("BDT", 2);

        public static Currency BHD { get; } = new("BHD", 3);

        public static Currency BIF { get; } = new("BIF", 0);

        public static Currency BMD { get; } = new("BMD", 2);

        public static Currency BND { get; } = new("BND", 2);

        public static Currency BOB { get; } = new("BOB", 2);

        public static Currency BOV { get; } = new("BOV", 2);

        public static Currency BRL { get; } = new("BRL", 2);

        public static Currency BSD { get; } = new("BSD", 2);

        public static Currency BTN { get; } = new("BTN", 2);

        public static Currency BWP { get; } = new("BWP", 2);

        public static Currency BYN { get; } = new("BYN", 2);

        public static Currency BZD { get; } = new("BZD", 2);

        public static Currency CAD { get; } = new("CAD", 2);

        public static Currency CDF { get; } = new("CDF", 2);

        public static Currency CHE { get; } = new("CHE", 2);

        public static Currency CHF { get; } = new("CHF", 2);

        public static Currency CHW { get; } = new("CHW", 2);

        public static Currency CLF { get; } = new("CLF", 4);

        public static Currency CLP { get; } = new("CLP", 0);

        public static Currency CNY { get; } = new("CNY", 2);

        public static Currency COP { get; } = new("COP", 2);

        public static Currency COU { get; } = new("COU", 2);

        public static Currency CRC { get; } = new("CRC", 2);

        public static Currency CUP { get; } = new("CUP", 2);

        public static Currency CVE { get; } = new("CVE", 2);

        public static Currency CZK { get; } = new("CZK", 2);

        public static Currency DJF { get; } = new("DJF", 0);

        public static Currency DKK { get; } = new("DKK", 2);

        public static Currency DOP { get; } = new("DOP", 2);

        public static Currency DZD { get; } = new("DZD", 2);

        public static Currency EGP { get; } = new("EGP", 2);

        public static Currency ERN { get; } = new("ERN", 2);

        public static Currency ETB { get; } = new("ETB", 2);

        public static Currency EUR { get; } = new("EUR", 2);

        public static Currency FJD { get; } = new("FJD", 2);

        public static Currency FKP { get; } = new("FKP", 2);

        public static Currency GBP { get; } = new("GBP", 2);

        public static Currency GEL { get; } = new("GEL", 2);

        public static Currency GHS { get; } = new("GHS", 2);

        public static Currency GIP { get; } = new("GIP", 2);

        public static Currency GMD { get; } = new("GMD", 2);

        public static Currency GNF { get; } = new("GNF", 0);

        public static Currency GTQ { get; } = new("GTQ", 2);

        public static Currency GYD { get; } = new("GYD", 2);

        public static Currency HKD { get; } = new("HKD", 2);

        public static Currency HNL { get; } = new("HNL", 2);

        public static Currency HTG { get; } = new("HTG", 2);

        public static Currency HUF { get; } = new("HUF", 2);

        public static Currency IDR { get; } = new("IDR", 2);

        public static Currency ILS { get; } = new("ILS", 2);

        public static Currency INR { get; } = new("INR", 2);

        public static Currency IQD { get; } = new("IQD", 3);

        public static Currency IRR { get; } = new("IRR", 2);

        public static Currency ISK { get; } = new("ISK", 0);

        public static Currency JMD { get; } = new("JMD", 2);

        public static Currency JOD { get; } = new("JOD", 3);

        public static Currency JPY { get; } = new("JPY", 0);

        public static Currency KES { get; } = new("KES", 2);

        public static Currency KGS { get; } = new("KGS", 2);

        public static Currency KHR { get; } = new("KHR", 2);

        public static Currency KMF { get; } = new("KMF", 0);

        public static Currency KPW { get; } = new("KPW", 2);

        public static Currency KRW { get; } = new("KRW", 0);

        public static Currency KWD { get; } = new("KWD", 3);

        public static Currency KYD { get; } = new("KYD", 2);

        public static Currency KZT { get; } = new("KZT", 2);

        public static Currency LAK { get; } = new("LAK", 2);

        public static Currency LBP { get; } = new("LBP", 2);

        public static Currency LKR { get; } = new("LKR", 2);

        public static Currency LRD { get; } = new("LRD", 2);

        public static Currency LSL { get; } = new("LSL", 2);

        public static Currency LYD { get; } = new("LYD", 3);

        public static Currency MAD { get; } = new("MAD", 2);

        public static Currency MDL { get; } = new("MDL", 2);

        public static Currency MGA { get; } = new("MGA", 2);

        public static Currency MKD { get; } = new("MKD", 2);

        public static Currency MMK { get; } = new("MMK", 2);

        public static Currency MNT { get; } = new("MNT", 2);

        public static Currency MOP { get; } = new("MOP", 2);

        public static Currency MRU { get; } = new("MRU", 2);

        public static Currency MUR { get; } = new("MUR", 2);

        public static Currency MVR { get; } = new("MVR", 2);

        public static Currency MWK { get; } = new("MWK", 2);

        public static Currency MXN { get; } = new("MXN", 2);

        public static Currency MXV { get; } = new("MXV", 2);

        public static Currency MYR { get; } = new("MYR", 2);

        public static Currency MZN { get; } = new("MZN", 2);

        public static Currency NAD { get; } = new("NAD", 2);

        public static Currency NGN { get; } = new("NGN", 2);

        public static Currency NIO { get; } = new("NIO", 2);

        public static Currency NOK { get; } = new("NOK", 2);

        public static Currency NPR { get; } = new("NPR", 2);

        public static Currency NZD { get; } = new("NZD", 2);

        public static Currency OMR { get; } = new("OMR", 3);

        public static Currency PAB { get; } = new("PAB", 2);

        public static Currency PEN { get; } = new("PEN", 2);

        public static Currency PGK { get; } = new("PGK", 2);

        public static Currency PHP { get; } = new("PHP", 2);

        public static Currency PKR { get; } = new("PKR", 2);

        public static Currency PLN { get; } = new("PLN", 2);

        public static Currency PYG { get; } = new("PYG", 0);

        public static Currency QAR { get; } = new("QAR", 2);

        public static Currency RON { get; } = new("RON", 2);

        public static Currency RSD { get; } = new("RSD", 2);

        public static Currency RUB { get; } = new("RUB", 2);

        public static Currency RWF { get; } = new("RWF", 0);

        public static Currency SAR { get; } = new("SAR", 2);

        public static Currency SBD { get; } = new("SBD", 2);

        public static Currency SCR { get; } = new("SCR", 2);

        public static Currency SDG { get; } = new("SDG", 2);

        public static Currency SEK { get; } = new("SEK", 2);

        public static Currency SGD { get; } = new("SGD", 2);

        public static Currency SHP { get; } = new("SHP", 2);

        public static Currency SLE { get; } = new("SLE", 2);

        public static Currency SOS { get; } = new("SOS", 2);

        public static Currency SRD { get; } = new("SRD", 2);

        public static Currency SSP { get; } = new("SSP", 2);

        public static Currency STN { get; } = new("STN", 2);

        public static Currency SVC { get; } = new("SVC", 2);

        public static Currency SYP { get; } = new("SYP", 2);

        public static Currency SZL { get; } = new("SZL", 2);

        public static Currency THB { get; } = new("THB", 2);

        public static Currency TJS { get; } = new("TJS", 2);

        public static Currency TMT { get; } = new("TMT", 2);

        public static Currency TND { get; } = new("TND", 3);

        public static Currency TOP { get; } = new("TOP", 2);

        public static Currency TRY { get; } = new("TRY", 2);

        public static Currency TTD { get; } = new("TTD", 2);

        public static Currency TWD { get; } = new("TWD", 2);

        public static Currency TZS { get; } = new("TZS", 2);

        public static Currency UAH { get; } = new("UAH", 2);

        public static Currency UGX { get; } = new("UGX", 0);

        public static Currency USD { get; } = new("USD", 2);

        public static Currency USN { get; } = new("USN", 2);

        public static Currency UYI { get; } = new("UYI", 0);

        public static Currency UYU { get; } = new("UYU", 2);

        public static Currency UYW { get; } = new("UYW", 4);

        public static Currency UZS { get; } = new("UZS", 2);

        public static Currency VED { get; } = new("VED", 2);

        public static Currency VES { get; } = new("VES", 2);

        public static Currency VND { get; } = new("VND", 0);

        public static Currency VUV { get; } = new("VUV", 0);

        public static Currency WST { get; } = new("WST", 2);

        public static Currency XAD { get; } = new("XAD", 2);

        public static Currency XAF { get; } = new("XAF", 0);

        public static Currency XAG { get; } = new("XAG", 0);

        public static Currency XAU { get; } = new("XAU", 0);

        public static Currency XBA { get; } = new("XBA", 0);

        public static Currency XBB { get; } = new("XBB", 0);

        public static Currency XBC { get; } = new("XBC", 0);

        public static Currency XBD { get; } = new("XBD", 0);

        public static Currency XCD { get; } = new("XCD", 2);

        public static Currency XCG { get; } = new("XCG", 2);

        public static Currency XDR { get; } = new("XDR", 0);

        public static Currency XOF { get; } = new("XOF", 0);

        public static Currency XPD { get; } = new("XPD", 0);

        public static Currency XPF { get; } = new("XPF", 0);

        public static Currency XPT { get; } = new("XPT", 0);

        public static Currency XSU { get; } = new("XSU", 0);

        public static Currency XTS { get; } = new("XTS", 0);

        public static Currency XUA { get; } = new("XUA", 0);

        public static Currency XXX { get; } = new("XXX", 0);

        public static Currency YER { get; } = new("YER", 2);

        public static Currency ZAR { get; } = new("ZAR", 2);

        public static Currency ZMW { get; } = new("ZMW", 2);

        public static Currency ZWG { get; } = new("ZWG", 2);
    }
}
