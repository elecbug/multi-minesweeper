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

            Loading();
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

            if (File.Exists("server_ip") == false)
            {
                MessageBox.Show("Server IP file 'server_ip' not found. Please create a file with the server IP.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string serverIP = File.ReadAllText("server_ip");
            _client.Connect(serverIP, 443);

            RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(pubKeyBytes, out _);

            byte[] aesKey = new byte[AESWrapper.AES_RAW_KEY_SIZE]; // AES key size is 256 bits (32 bytes)
            RandomNumberGenerator.Fill(aesKey); // Fill with random bytes

            byte[] encryptedKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.Pkcs1);

            NetworkStream stream = _client.GetStream();
            stream.Write(encryptedKey, 0, encryptedKey.Length);

            _aesKey = AESWrapper.GenerateAESKey(aesKey);

            Packet packet = new Packet(PacketType.PingRequest, Packet.PING_MSG);

            byte[] packetData = packet.Serialize();
            byte[] encryptedPacket = AESWrapper.EncryptPacket(packetData, _aesKey);
        
            stream.Write(encryptedPacket, 0, encryptedPacket.Length);
            stream.Read(encryptedPacket, 0, encryptedPacket.Length);

            packetData = AESWrapper.DecryptPacket(encryptedPacket, _aesKey);
            packet = Packet.Deserialize(packetData);

            if (packet.Type == PacketType.PingResponse && packet.Data.Length > 0 && packet.Data[0] == Packet.PING_MSG)
            {
                MessageBox.Show("Ping response received successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Failed to receive valid ping response.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
