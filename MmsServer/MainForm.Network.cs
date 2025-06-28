using MmsProtocol;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace MmsServer
{
    public partial class MainForm : Form
    {
        private async void OpenServer()
        {
            _server = new TcpListener(IPAddress.Any, 443);
            _server.Start();

            AppendLog("Server opened successfully. Waiting for connections...");
            while (true)
            {
                try
                {
                    TcpClient client = await _server.AcceptTcpClientAsync();
                    AppendLog($"New connection from {client.Client.RemoteEndPoint}.");

                    byte[] buffer = new byte[256];
                    await client.GetStream().ReadAsync(buffer);
                    AppendLog($"Received data from {client.Client.RemoteEndPoint}.");

                    byte[] result = _rsa.Decrypt(buffer, RSAEncryptionPadding.Pkcs1);
                    AppendLog($"Decrypted data from {client.Client.RemoteEndPoint}.");

                    byte[] aesKey = Packet.GenerateAESKey(result);
                    AppendLog($"Generated AES key for {client.Client.RemoteEndPoint}.");

                    //HandleClient(client);
                }
                catch (Exception ex)
                {
                    AppendLog($"Error accepting client: {ex.Message}");
                }
            }
        }
    }
}
