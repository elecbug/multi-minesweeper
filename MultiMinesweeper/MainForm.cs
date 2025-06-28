using MmsProtocol;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace MultiMinesweeper
{
    public class MainForm : Form
    {
        private TcpClient _client { get; set; }
        private byte[]? _aesKey;

        public MainForm()
        {
            {
                Text = "Multi Minesweeper";
                Width = 800;
                Height = 600;
            }

            _client = new TcpClient();

            Task.Run(Loading);
        }

        private void Loading()
        {
            if (File.Exists("key.pub") == false)
            {
                MessageBox.Show("Public key file 'key.pub' not found. Please generate or load a key first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string pubKey = File.ReadAllText("key.pub");
            byte[] pubKeyBytes = Convert.FromBase64String(pubKey);

            _client.Connect("localhost", 443);

            RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(pubKeyBytes, out _);

            byte[] aesKey = new byte[Packet.AES_RAW_KEY_SIZE]; // AES key size is 256 bits (32 bytes)
            RandomNumberGenerator.Fill(aesKey); // Fill with random bytes

            byte[] encryptedKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.Pkcs1);

            NetworkStream stream = _client.GetStream();
            stream.Write(encryptedKey, 0, encryptedKey.Length);

            _aesKey = Packet.GenerateAESKey(aesKey);
        }
    }
}
