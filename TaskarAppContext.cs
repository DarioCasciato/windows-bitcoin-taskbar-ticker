using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace windows_bitcoin_taskbar_ticker
{
    public class TaskbarAppContext : ApplicationContext
    {
        private NotifyIcon notifyIcon;
        private System.Windows.Forms.Timer timer; // Fully qualified name
        private HttpClient httpClient;

        public TaskbarAppContext()
        {
            httpClient = new HttpClient();

            // Initialize NotifyIcon
            notifyIcon = new NotifyIcon()
            {
                Icon = new Icon("bitcoin.ico"), // Ensure you have a bitcoin.ico in the output directory
                Visible = true,
                Text = "Bitcoin Price: Loading..."
            };

            // Context menu
            var contextMenu = new ContextMenuStrip();
            var exitMenuItem = new ToolStripMenuItem("Exit", null, Exit);
            contextMenu.Items.Add(exitMenuItem);
            notifyIcon.ContextMenuStrip = contextMenu;

            // Handle double-click (optional)
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            // Initialize Timer
            timer = new System.Windows.Forms.Timer(); // Fully qualified name
            timer.Interval = 60000; // 1 minute
            timer.Tick += async (sender, e) => await UpdateBitcoinPrice();
            timer.Start();

            // Initial price fetch
            Task.Run(async () => await UpdateBitcoinPrice());
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
            }
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            // Optionally, show a detailed form or perform an action
            MessageBox.Show("Current Bitcoin price is displayed in the tooltip.", "Bitcoin Taskbar Ticker", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Exit(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }
    }
}
