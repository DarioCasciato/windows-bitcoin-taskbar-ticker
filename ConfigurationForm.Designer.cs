namespace windows_bitcoin_taskbar_ticker
{
    partial class ConfigurationForm
    {
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.listBoxCryptos = new System.Windows.Forms.ListBox();
            this.lblNewCrypto = new System.Windows.Forms.Label();
            this.txtNewCrypto = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // listBoxCryptos
            //
            this.listBoxCryptos.FormattingEnabled = true;
            this.listBoxCryptos.Location = new System.Drawing.Point(12, 12);
            this.listBoxCryptos.Name = "listBoxCryptos";
            this.listBoxCryptos.Size = new System.Drawing.Size(360, 160);
            this.listBoxCryptos.TabIndex = 0;
            //
            // lblNewCrypto
            //
            this.lblNewCrypto.AutoSize = true;
            this.lblNewCrypto.Location = new System.Drawing.Point(12, 175);
            this.lblNewCrypto.Name = "lblNewCrypto";
            this.lblNewCrypto.Size = new System.Drawing.Size(98, 13);
            this.lblNewCrypto.TabIndex = 1;
            this.lblNewCrypto.Text = "New Coin Symbol:";
            //
            // txtNewCrypto
            //
            this.txtNewCrypto.Location = new System.Drawing.Point(15, 205); // Mehr Abstand zum Label
            this.txtNewCrypto.Name = "txtNewCrypto";
            this.txtNewCrypto.Size = new System.Drawing.Size(357, 20);
            this.txtNewCrypto.TabIndex = 2;
            this.txtNewCrypto.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtNewCrypto_KeyDown);
            //
            // btnAdd
            //
            this.btnAdd.Location = new System.Drawing.Point(15, 245); // Nach unten verschoben
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(170, 35); // Erhöhte Höhe
            this.btnAdd.TabIndex = 3;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            //
            // btnRemove
            //
            this.btnRemove.Location = new System.Drawing.Point(205, 245); // Nach unten verschoben
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(170, 35); // Erhöhte Höhe
            this.btnRemove.TabIndex = 4;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            //
            // btnClose
            //
            this.btnClose.Location = new System.Drawing.Point(15, 295); // Nach unten verschoben
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(360, 35); // Erhöhte Höhe und volle Breite
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // lblInfo
            //
            this.lblInfo.AutoSize = true;
            this.lblInfo.ForeColor = System.Drawing.Color.Gray;
            this.lblInfo.Location = new System.Drawing.Point(5, 340); // Nach unten verschoben
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(277, 13);
            this.lblInfo.TabIndex = 6;
            this.lblInfo.Text = "Prices will be displayed within the next minute.";
            //
            // ConfigurationForm
            //
            this.ClientSize = new System.Drawing.Size(384, 371); // Vergrößerte Formgröße
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.txtNewCrypto);
            this.Controls.Add(this.lblNewCrypto);
            this.Controls.Add(this.listBoxCryptos);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; // Verhindert Größenänderung
            this.MinimizeBox = false; // Verhindert Minimierung
            this.Name = "ConfigurationForm";
            this.Text = "Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListBox listBoxCryptos;
        private System.Windows.Forms.Label lblNewCrypto;
        private System.Windows.Forms.TextBox txtNewCrypto;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblInfo;
    }
}
