namespace windows_bitcoin_taskbar_ticker
{
    /// <summary>
    /// Repräsentiert eine Kryptowährung mit ihren relevanten Eigenschaften.
    /// </summary>
    public class Cryptocurrency
    {
        /// <summary>
        /// Das Symbol der Kryptowährung (z.B., BTC, ETH).
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Die CoinGecko-ID der Kryptowährung (z.B., bitcoin, ethereum).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Der aktuelle Preis der Kryptowährung in USD.
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Gibt an, ob der Preis gerade geladen wird.
        /// </summary>
        public bool IsLoading { get; set; }
    }
}
