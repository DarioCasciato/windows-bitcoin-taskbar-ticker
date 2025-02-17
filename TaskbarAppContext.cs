using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace windows_bitcoin_taskbar_ticker
{
    public class TaskbarAppContext : ApplicationContext
    {
        private NotifyIcon notifyIcon;
        private System.Windows.Forms.Timer timer;
        private HttpClient httpClient;
        private ConfigManager configManager;
        private bool isUpdating = false;

        // Dictionary zur Verfolgung jeder Kryptowährung
        private Dictionary<string, Cryptocurrency> cryptocurrencies = new Dictionary<string, Cryptocurrency>();

        public TaskbarAppContext()
        {
            try
            {
                // Initialize ConfigManager
                configManager = new ConfigManager();

                // Initialize HttpClient
                httpClient = new HttpClient();

                // Initialize NotifyIcon with embedded icon
                notifyIcon = new NotifyIcon()
                {
                    Icon = LoadEmbeddedIcon("windows_bitcoin_taskbar_ticker.Resources.bitcoin.ico"),
                    Visible = true,
                    Text = "Cryptocurrency Prices: Loading..."
                };

                // Context menu
                var contextMenu = new ContextMenuStrip();
                var configureMenuItem = new ToolStripMenuItem("Configure...", null, Configure);
                var exitMenuItem = new ToolStripMenuItem("Exit", null, Exit);
                contextMenu.Items.Add(configureMenuItem);
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add(exitMenuItem);
                notifyIcon.ContextMenuStrip = contextMenu;

                // Initialize Timer
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 60000; // 1 minute
                timer.Tick += async (sender, e) => await UpdatePricesAsync();
                timer.Start();

                // Initialisierung der Kryptowährungen
                InitializeCryptocurrencies();

                // Initial price fetch
                Task.Run(async () => await UpdatePricesAsync());
            }
            catch (Exception ex)
            {
                // Optional: Serilog verwenden
                // Log.Fatal(ex, "Unhandled exception in TaskbarAppContext constructor.");
                LogError($"Exception in TaskbarAppContext constructor: {ex.Message}");
                Exit(null, null);
            }
        }

        /// <summary>
        /// Lädt und initialisiert die Kryptowährungen aus der Konfiguration.
        /// </summary>
        private void InitializeCryptocurrencies()
        {
            foreach (var symbol in configManager.Cryptocurrencies)
            {
                string upperSymbol = symbol.ToUpper();
                string id = configManager.GetIdBySymbol(upperSymbol);
                if (id != null && !cryptocurrencies.ContainsKey(upperSymbol))
                {
                    cryptocurrencies.Add(upperSymbol, new Cryptocurrency
                    {
                        Symbol = upperSymbol,
                        Id = id,
                        Price = null,
                        IsLoading = true // Neu hinzugefügt, daher laden wir den Preis
                    });
                }
            }
        }

        /// <summary>
        /// Lädt das eingebettete Icon.
        /// </summary>
        /// <param name="resourceName">Vollqualifizierter Ressourcenname.</param>
        /// <returns>Icon, wenn gefunden; ansonsten ein Standard-Systemicon.</returns>
        private Icon LoadEmbeddedIcon(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    return new Icon(stream);
                }
                else
                {
                    // Handle missing resource
                    return SystemIcons.Application; // Use a default system icon
                }
            }
        }

        /// <summary>
        /// Aktualisiert die Preise der konfigurierten Kryptowährungen mithilfe der CoinGecko-API.
        /// </summary>
        /// <returns></returns>
        private async Task UpdatePricesAsync()
        {
            if (isUpdating)
                return; // Verhindert parallele Aktualisierungen

            try
            {
                isUpdating = true;
                notifyIcon.Text = "Prices loading...";

                if (cryptocurrencies.Count == 0)
                {
                    notifyIcon.Text = "No cryptocurrencies configured.";
                    return;
                }

                // Sammle alle IDs für die API-Anfrage
                var idsList = new List<string>();
                foreach (var crypto in cryptocurrencies.Values)
                {
                    idsList.Add(crypto.Id);
                }
                string ids = string.Join(",", idsList);
                string url = $"https://api.coingecko.com/api/v3/simple/price?ids={ids}&vs_currencies=usd";

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}).");
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseBody);

                List<string> priceTexts = new List<string>();

                foreach (var crypto in cryptocurrencies.Values)
                {
                    if (json[crypto.Id] != null && json[crypto.Id]["usd"] != null)
                    {
                        decimal price = 0;
                        bool parseSuccess = decimal.TryParse(json[crypto.Id]["usd"].ToString(), out price);
                        if (parseSuccess)
                        {
                            crypto.Price = price;
                            crypto.IsLoading = false; // Preis erfolgreich geladen

                            // Formatierung: 4 Dezimalstellen, wenn Preis < 1, sonst 2 Dezimalstellen
                            string formattedPrice = price < 1m ? price.ToString("N4") : price.ToString("N2");
                            priceTexts.Add($"{crypto.Symbol}: ${formattedPrice}");
                        }
                        else
                        {
                            priceTexts.Add($"{crypto.Symbol}: N/A");
                        }
                    }
                    else
                    {
                        priceTexts.Add($"{crypto.Symbol}: N/A");
                    }
                }


                // Aktualisiere den Tooltip mit den aktuellen Preisen
                notifyIcon.Text = string.Join("\n", priceTexts);
            }
            catch (Exception ex)
            {
                // Setze den Tooltip auf "Prices: N/A" und zeige eine Balloon-Tipp-Benachrichtigung
                notifyIcon.Text = "Prices: N/A";
                notifyIcon.ShowBalloonTip(3000, "Error", "Failed to fetch cryptocurrency prices.", ToolTipIcon.Error);
                LogError($"Error fetching cryptocurrency prices: {ex.Message}");
            }
            finally
            {
                isUpdating = false;
            }
        }

        /// <summary>
        /// Öffnet das Konfigurationsformular.
        /// </summary>
        private void Configure(object sender, EventArgs e)
        {
            using (var configForm = new ConfigurationForm(configManager))
            {
                configForm.ShowDialog();

                // Aktualisiere das Kryptowährungs-Dictionary
                foreach (var symbol in configManager.Cryptocurrencies)
                {
                    string upperSymbol = symbol.ToUpper();
                    if (!cryptocurrencies.ContainsKey(upperSymbol))
                    {
                        string id = configManager.GetIdBySymbol(upperSymbol);
                        if (id != null)
                        {
                            cryptocurrencies.Add(upperSymbol, new Cryptocurrency
                            {
                                Symbol = upperSymbol,
                                Id = id,
                                Price = null,
                                IsLoading = true // Neue Kryptowährung, daher laden wir den Preis
                            });
                        }
                    }
                }

                // Entferne Kryptowährungen, die aus der Konfiguration entfernt wurden
                var symbolsToRemove = new List<string>();
                foreach (var symbol in cryptocurrencies.Keys)
                {
                    if (!configManager.Cryptocurrencies.Select(s => s.ToUpper()).Contains(symbol))
                    {
                        symbolsToRemove.Add(symbol);
                    }
                }
                foreach (var symbol in symbolsToRemove)
                {
                    cryptocurrencies.Remove(symbol);
                }

                // Aktualisiere den Tooltip nur für die neuen Kryptowährungen
                List<string> priceTexts = new List<string>();
                foreach (var crypto in cryptocurrencies.Values)
                {
                    if (crypto.IsLoading)
                    {
                        priceTexts.Add($"{crypto.Symbol}: Price loading...");
                    }
                    else if (crypto.Price.HasValue)
                    {
                        decimal price = crypto.Price.Value;
                        string formattedPrice = price < 1m ? price.ToString("N4") : price.ToString("N2");
                        priceTexts.Add($"{crypto.Symbol}: ${formattedPrice}");
                    }
                    else
                    {
                        priceTexts.Add($"{crypto.Symbol}: N/A");
                    }
                }


                notifyIcon.Text = string.Join("\n", priceTexts);
            }
        }

        /// <summary>
        /// Beendet die Anwendung ordnungsgemäß.
        /// </summary>
        private void Exit(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        /// <summary>
        /// Protokolliert Fehler in eine versteckte Log-Datei.
        /// </summary>
        /// <param name="message">Die zu protokollierende Fehlermeldung.</param>
        private void LogError(string message)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                string logMessage = $"{DateTime.Now}: {message}\n";
                File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // Falls das Logging fehlschlägt, ignoriere es stillschweigend, um rekursive Fehler zu vermeiden
            }
        }
    }
}
