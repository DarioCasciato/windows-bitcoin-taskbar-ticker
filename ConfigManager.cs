using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace windows_bitcoin_taskbar_ticker
{
    public class ConfigManager
    {
        public List<string> Cryptocurrencies { get; private set; }
        private readonly string configDirectory;
        private readonly string configFilePath;
        private Dictionary<string, string> SymbolIdMap { get; set; }
        private readonly HttpClient httpClient;

        public ConfigManager()
        {
            // Festlegen des Konfigurationsverzeichnisses und -pfads
            configDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WindowsBitcoinTaskbarTicker");
            configFilePath = Path.Combine(configDirectory, "config.json");

            // Initialisieren der Kryptowährungsliste und des Symbol-zu-ID-Mappings
            Cryptocurrencies = new List<string>();
            SymbolIdMap = new Dictionary<string, string>();
            httpClient = new HttpClient();

            // Hinzufügen des User-Agent-Headers
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("windows_bitcoin_taskbar_ticker/1.0 (https://yourwebsite.com)");

            // Laden der Konfiguration und Initialisieren des Mappings
            LoadConfig().Wait();
            InitializeSymbolIdMapAsync().Wait();
        }

        /// <summary>
        /// Lädt die Konfiguration aus der JSON-Datei.
        /// Wenn die Datei nicht existiert, wird eine Standardkonfiguration erstellt.
        /// </summary>
        private async Task LoadConfig()
        {
            try
            {
                if (!Directory.Exists(configDirectory))
                {
                    Directory.CreateDirectory(configDirectory);
                }

                if (File.Exists(configFilePath))
                {
                    string jsonString = await File.ReadAllTextAsync(configFilePath);
                    var configData = JsonSerializer.Deserialize<ConfigData>(jsonString);
                    if (configData != null && configData.Cryptocurrencies != null && configData.Cryptocurrencies.Count > 0)
                    {
                        Cryptocurrencies = configData.Cryptocurrencies;
                    }
                    else
                    {
                        // Standardkonfiguration, wenn die Datei leer oder ungültig ist
                        Cryptocurrencies = new List<string> { "BTC" };
                        await SaveConfig();
                    }
                }
                else
                {
                    // Erstelle eine Standardkonfiguration, wenn die Datei nicht existiert
                    Cryptocurrencies = new List<string> { "BTC" };
                    await SaveConfig();
                }
            }
            catch (Exception ex)
            {
                // Initialisiere mit einer Standardkonfiguration im Fehlerfall
                Cryptocurrencies = new List<string> { "BTC" };
                await SaveConfig();
            }
        }

        /// <summary>
        /// Speichert die aktuelle Konfiguration in der JSON-Datei.
        /// </summary>
        public async Task SaveConfig()
        {
            try
            {
                var configData = new ConfigData
                {
                    Cryptocurrencies = this.Cryptocurrencies
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(configData, options);
                await File.WriteAllTextAsync(configFilePath, jsonString);
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Fügt der Liste eine neue Kryptowährung hinzu.
        /// </summary>
        /// <param name="crypto">Das Kryptowährungssymbol, das hinzugefügt werden soll.</param>
        public async Task AddCryptocurrency(string crypto)
        {
            crypto = crypto.ToUpper();
            if (!Cryptocurrencies.Contains(crypto))
            {
                Cryptocurrencies.Add(crypto);
                await SaveConfig();
            }
        }

        /// <summary>
        /// Entfernt eine Kryptowährung aus der Liste.
        /// </summary>
        /// <param name="crypto">Das Kryptowährungssymbol, das entfernt werden soll.</param>
        public async Task RemoveCryptocurrency(string crypto)
        {
            crypto = crypto.ToUpper();
            if (Cryptocurrencies.Contains(crypto))
            {
                Cryptocurrencies.Remove(crypto);
                await SaveConfig();
            }
        }

        /// <summary>
        /// Initialisiert die Symbol-zu-ID-Zuordnung durch Abrufen der Top-Kryptowährungen von CoinGecko.
        /// </summary>
        private async Task InitializeSymbolIdMapAsync()
        {
            try
            {
                // Abrufen der Top 250 Kryptowährungen nach Marktkapitalisierung
                string url = "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=250&page=1&sparkline=false";
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}).");
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                var json = JArray.Parse(responseBody);

                foreach (var item in json)
                {
                    string symbol = item["symbol"].ToString().ToUpper();
                    string id = item["id"].ToString();
                    // Wenn das Symbol noch nicht im Mapping ist, füge es hinzu
                    if (!SymbolIdMap.ContainsKey(symbol))
                    {
                        SymbolIdMap.Add(symbol, id);
                    }
                }

                // Optional: Manuelles Mapping hinzufügen, falls erforderlich
                // Zum Beispiel, um sicherzustellen, dass "BTC" auf "bitcoin" abgebildet wird
                if (!SymbolIdMap.ContainsKey("BTC"))
                {
                    SymbolIdMap.Add("BTC", "bitcoin");
                }

                // Debugging: Entfernt, daher keine Aufrufe mehr
                // DebugPrintSymbolIdMap();
            }
            catch (Exception ex)
            {
                // Optional: Serilog verwenden
                // Log.Error(ex, "Error initializing symbol-to-ID mapping.");
                // MessageBox.Show ist weiterhin erforderlich für den Benutzer
            }
        }

        /// <summary>
        /// Holt die CoinGecko ID für ein gegebenes Symbol.
        /// </summary>
        /// <param name="symbol">Das Kryptowährungssymbol.</param>
        /// <returns>Die CoinGecko ID, wenn gefunden; ansonsten null.</returns>
        public string GetIdBySymbol(string symbol)
        {
            symbol = symbol.ToUpper();
            if (SymbolIdMap.ContainsKey(symbol))
            {
                return SymbolIdMap[symbol];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Debugging-Methode zum Anzeigen des Symbol-zu-ID-Mappings.
        /// </summary>
        private void DebugPrintSymbolIdMap()
        {
            string mapping = "Symbol-zu-ID Mapping:\n";
            foreach (var kvp in SymbolIdMap)
            {
                mapping += $"{kvp.Key} => {kvp.Value}\n";
            }

            // Anzeige des Mappings in einer MessageBox
            // MessageBox.Show(mapping, "Debug: Symbol-zu-ID Mapping", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    /// <summary>
    /// Klasse zur Darstellung der Konfigurationsdaten.
    /// </summary>
    public class ConfigData
    {
        public List<string> Cryptocurrencies { get; set; }
    }
}
