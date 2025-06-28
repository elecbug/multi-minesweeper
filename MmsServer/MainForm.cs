using System.Security.Cryptography;

namespace MmsServer
{
    public class MainForm : Form
    {
        public Color CONSOLE_BACK_COLOR = Color.FromArgb(red: 30, green: 30, blue: 30);

        private TableLayoutPanel _layoutPanel;
        private MenuStrip _menu;
        private RichTextBox _logBox;
        private RSA _rsa;

        public MainForm()
        {
            {
                Text = "Multi Minesweeper Server";
                Width = 400;
                Height = 600;
                StartPosition = FormStartPosition.CenterScreen;
                AutoSizeMode = AutoSizeMode.GrowAndShrink;
                Padding = new Padding(5);
            }

            _layoutPanel = new TableLayoutPanel()
            {
                Parent = this,
                Dock = DockStyle.Fill,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 2,
            };

            _menu = new MenuStrip()
            {
                Parent = _layoutPanel,
                Visible = true,
            };

            _menu.Items.Add(new ToolStripMenuItem("File", null,
            [
                new ToolStripMenuItem("Save log", null, (s, e) => SaveLog()),
                new ToolStripMenuItem("Exit", null, (s, e) => Application.Exit()),
            ]));

            _menu.Items.Add(new ToolStripMenuItem("Key", null,
            [
                new ToolStripMenuItem("Load key", null, (s, e) => LoadKey()),
                new ToolStripMenuItem("Save key", null, (s, e) => SaveKey()),
                new ToolStripMenuItem("Copy Public key", null, (s, e) => CopyPublicKey()),
            ]));

            _logBox = new RichTextBox()
            {
                Parent = _layoutPanel,
                Visible = true,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = CONSOLE_BACK_COLOR,
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                ScrollBars = RichTextBoxScrollBars.Horizontal | RichTextBoxScrollBars.Vertical,
            };

            AppendLog("Multi Minesweeper Server Log.",
                      "Welcome to the Multi Minesweeper server!",
                      "Use the menu to load or save keys.",
                      "The server is ready to accept connections.");

            _rsa = RSA.Create(2048);
        }

        private void AppendLog(params string[] messages)
        {
            foreach (string msg in messages)
            {
                _logBox.AppendText($"{DateTime.Now:HH:mm:ss}|{msg}\n");
            }
            _logBox.AppendText("\n");

            _logBox.SelectionStart = _logBox.TextLength;
            _logBox.ScrollToCaret();
        }

        private void SaveLog()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Log File|*.log";
                dialog.FileName = "server.log";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(dialog.FileName, _logBox.Text);
                    
                    AppendLog($"Log saved successfully to {dialog.FileName}.");
                }
            }
        }

        private void CopyPublicKey()
        {
            byte[] pubKey = _rsa.ExportRSAPublicKey();
            string pubKeyStr = Convert.ToBase64String(pubKey);
            
            Clipboard.SetText(pubKeyStr);

            AppendLog("Public key copied to clipboard.");
        }

        private void LoadKey()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Key File|*.key";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = dialog.FileName;
                
                    string keyContent = File.ReadAllText(fileName);
                    string[] keyParts = keyContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                    byte[] pubKey = Convert.FromBase64String(keyParts[0]);
                    byte[] privKey = Convert.FromBase64String(keyParts[1]);

                    _rsa.ImportRSAPublicKey(pubKey, out _);
                    _rsa.ImportRSAPrivateKey(privKey, out _);

                    AppendLog($"Key loaded successfully from {dialog.FileName}.");
                }
            }
        }

        private void SaveKey()
        {
            byte[] pubKey = _rsa.ExportRSAPublicKey();
            byte[] privKey = _rsa.ExportRSAPrivateKey();

            string pubKeyStr = Convert.ToBase64String(pubKey);
            string privKeyStr = Convert.ToBase64String(privKey);

            string keyStr = $"{pubKeyStr}\n{privKeyStr}";

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Key File|*.key";
                dialog.FileName = "new.key";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(dialog.FileName, keyStr);

                    AppendLog($"Key saved successfully to {dialog.FileName}.");
                }
            }
        }
    }
}
