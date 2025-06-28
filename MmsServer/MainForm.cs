using System.Net.Sockets;
using System.Security.Cryptography;

namespace MmsServer
{
    public partial class MainForm : Form
    {
        public Color CONSOLE_BACK_COLOR = Color.FromArgb(red: 30, green: 30, blue: 30);

        private TableLayoutPanel _layoutPanel;
        private MenuStrip _menu;
        private RichTextBox _logBox;
        private RSA _rsa;
        private TcpListener? _server;

        public MainForm()
        {
            {
                Text = "Multi Minesweeper Server";
                Width = 800;
                Height = 600;
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

            _menu.Items.Add(new ToolStripMenuItem("Network", null,
            [
                new ToolStripMenuItem("Open server", null, (s, e) => OpenServer()),
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
                Text = "=====Multi Minesweeper Server Log=====\n\n",
            };

            AppendLog("Welcome to the Multi Minesweeper server!",
                      "Use the menu to load or save keys.", "The server is ready to accept connections.");

            _rsa = RSA.Create(2048);
        }
    }
}