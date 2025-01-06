using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace windows_bitcoin_taskbar_ticker
{
    public partial class ConfigurationForm : Form
    {
        private ConfigManager configManager;

        public ConfigurationForm(ConfigManager config)
        {
            InitializeComponent();
            configManager = config;
            LoadCryptocurrencies();
        }

        /// <summary>
        /// Lädt die Liste der Kryptowährungen in die ListBox.
        /// </summary>
        private void LoadCryptocurrencies()
        {
            listBoxCryptos.Items.Clear();
            foreach (var crypto in configManager.Cryptocurrencies)
            {
                listBoxCryptos.Items.Add(crypto.ToUpper());
            }
            lblInfo.Text = "Prices will be displayed within the next minute.";
        }

        /// <summary>
        /// Behandelt das Klick-Ereignis des Add-Buttons.
        /// </summary>
        private async void btnAdd_Click(object sender, EventArgs e)
        {
            string newCrypto = txtNewCrypto.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(newCrypto))
            {
                return;
            }

            if (configManager.Cryptocurrencies.Contains(newCrypto))
            {
                MessageBox.Show("The currency is already in the list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validiere das Kryptowährungssymbol
            string id = configManager.GetIdBySymbol(newCrypto);
            if (id == null)
            {
                MessageBox.Show("Invalid cryptocurrency symbol. Please enter a valid symbol.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                await configManager.AddCryptocurrency(newCrypto);
                LoadCryptocurrencies();
                txtNewCrypto.Clear();
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to add currency.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Behandelt das Klick-Ereignis des Remove-Buttons.
        /// </summary>
        private async void btnRemove_Click(object sender, EventArgs e)
        {
            if (listBoxCryptos.SelectedItem == null)
            {
                return;
            }

            int selectedIndex = listBoxCryptos.SelectedIndex;
            string selectedCrypto = listBoxCryptos.SelectedItem.ToString().ToUpper();

            try
            {
                await configManager.RemoveCryptocurrency(selectedCrypto);
                LoadCryptocurrencies();

                // Automatisch die nächste Währung auswählen
                if (configManager.Cryptocurrencies.Count > 0)
                {
                    if (selectedIndex >= configManager.Cryptocurrencies.Count)
                    {
                        selectedIndex = configManager.Cryptocurrencies.Count - 1;
                    }
                    listBoxCryptos.SelectedIndex = selectedIndex;
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Behandelt das Klick-Ereignis des Close-Buttons.
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Behandelt das Drücken der Enter-Taste im txtNewCrypto TextBox.
        /// </summary>
        private void txtNewCrypto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnAdd_Click(this, new EventArgs());
                e.SuppressKeyPress = true; // Verhindert den "ding sound"
            }
        }
    }
}
