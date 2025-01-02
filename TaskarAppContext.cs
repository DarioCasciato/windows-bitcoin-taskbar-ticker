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

        public TaskbarAppContext()
        {
            try
            {
                // Initialize HttpClient
                httpClient = new HttpClient();

                // Initialize NotifyIcon with embedded icon
                notifyIcon = new NotifyIcon()
                {
                    Icon = LoadEmbeddedIcon("windows_bitcoin_taskbar_ticker.Resources.bitcoin.ico"),
                    Visible = true,
                    Text = "Bitcoin Price: Loading..."
                };

                // Context menu
                var contextMenu = new ContextMenuStrip();
                var exitMenuItem = new ToolStripMenuItem("Exit", null, Exit);
                contextMenu.Items.Add(exitMenuItem);
                notifyIcon.ContextMenuStrip = contextMenu;

                // Initialize Timer
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 60000; // 1 minute
                timer.Tick += async (sender, e) => await UpdateBitcoinPrice();
                timer.Start();

                // Initial price fetch
                Task.Run(async () => await UpdateBitcoinPrice());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception in TaskbarAppContext constructor: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Exit(null, null);
            }
        }

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
                    MessageBox.Show($"Embedded resource '{resourceName}' not found. Using default icon.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return SystemIcons.Application; // Use a default system icon
                }
            }
        }

        private async Task UpdateBitcoinPrice()
        {
            try
            {
                string url = "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=usd";
                var response = await httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);
                decimal price = json["bitcoin"]["usd"].Value<decimal>();

                // Update tooltip
                notifyIcon.Text = $"Bitcoin: ${price:N2}";
            }
            catch (Exception ex)
            {
                // Log the error or notify the user
                notifyIcon.Text = "Bitcoin: N/A";
                notifyIcon.ShowBalloonTip(3000, "Error", "Failed to fetch Bitcoin price.", ToolTipIcon.Error);
                MessageBox.Show($"Error fetching Bitcoin price: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }
    }
}