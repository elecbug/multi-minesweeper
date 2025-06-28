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

                    byte[] aesKey = AESWrapper.GenerateAESKey(result);
                    AppendLog($"Generated AES key for {client.Client.RemoteEndPoint}.");

                    HandleClient(client, aesKey);
                }
                catch (Exception ex)
                {
                    AppendLog($"Error accepting client: {ex.Message}");
                }
            }
        }

        private async void HandleClient(TcpClient client, byte[] aesKey)
        {
            AppendLog($"Handling client {client.Client.RemoteEndPoint}.");

            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    AppendLog($"Closing connection to {client.Client.RemoteEndPoint}.");
                    byte[] buffer = new byte[1024];
                
                    while (true)
                    {
                        int count = await stream.ReadAsync(buffer);

                        byte[] data = AESWrapper.DecryptPacket(buffer[0..count], aesKey);
                        Packet packet =  Packet.Deserialize(data);
                        
                        switch (packet.Type)
                        {
                            case PacketType.PingRequest:
                                AppendLog($"Ping request from {client.Client.RemoteEndPoint}.");

                                if (packet.Data.Length == 0 || packet.Data[0] != Packet.PING_MSG)
                                {
                                    AppendLog($"Invalid ping message from {client.Client.RemoteEndPoint}.");
                                    break;
                                }

                                Packet response = new Packet(PacketType.PingResponse, Packet.PING_MSG);
                                byte[] responseData = AESWrapper.EncryptPacket(response.Serialize(), aesKey);

                                await stream.WriteAsync(responseData, 0, responseData.Length);
                                AppendLog($"Ping response sent to {client.Client.RemoteEndPoint}.");
                                
                                break;
                            default:
                                AppendLog($"Unknown packet type {packet.Type} from {client.Client.RemoteEndPoint}.");
                                break;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Error handling client {client.Client.RemoteEndPoint}: {ex.Message}");
            }
            finally
            {
                client.Close();
                AppendLog($"Connection to {client.Client.RemoteEndPoint} closed.");
            }
        }
    }
}
