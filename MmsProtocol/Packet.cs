using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MmsProtocol
{
    public enum PacketType
    {
        LoginRequest = 0x0001,
        LoginResponse = 0x0002,
        SignUpRequest = 0x0003,
        SignUpResponse = 0x0004,
        ConnectRequest = 0x0005,
        ConnectResponse = 0x0006,
        PingRequest = 0x0007,
        PingResponse = 0x0008,
    }
 
    public class Packet
    {
        public const string PING_MSG = "Hello, world! Ping Test zz";

        public PacketType Type { get; set; }
        public string[] Data { get; set; }

        public Packet(PacketType type, params string[] data)
        {
            Type = type;
            Data = data;
        }

        public byte[] Serialize()
        {
            string json = JsonSerializer.Serialize(this);
            return Encoding.UTF8.GetBytes(json);
        }

        public static Packet Deserialize(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<Packet>(json) 
                ?? throw new InvalidOperationException("Deserialization failed");
        }
    }

    public class AESWrapper
    {
        public const int AES_RAW_KEY_SIZE = 192;

        public static byte[] GenerateAESKey(byte[] packet)
        {
            if (packet.Length != AES_RAW_KEY_SIZE)
            {
                throw new ArgumentException("Packet must be 1024 bytes long.");
            }

            byte[] result = new byte[32];

            for (int i = 0; i < packet.Length; i += 32)
            {
                for (int j = 0; j < 32; j++)
                {
                    result[j] ^= packet[i + j];
                }
            }

            return result;
        }

        public static byte[] EncryptPacket(byte[] packetData, byte[] aesKey)
        {
            if (aesKey.Length != 32)
            {
                throw new ArgumentException("AES key must be 256 bits (32 bytes) long.");
            }

            using var aes = Aes.Create();

            aes.Key = aesKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] iv = new byte[aes.BlockSize / 8];
            RandomNumberGenerator.Fill(iv); // Fill IV with random bytes

            byte[] result = aes.EncryptCbc(packetData, iv, PaddingMode.PKCS7);

            return iv.Concat(result).ToArray();
        }

        public static byte[] DecryptPacket(byte[] encryptedData, byte[] aesKey)
        {
            if (aesKey.Length != 32)
            {
                throw new ArgumentException("AES key must be 256 bits (32 bytes) long.");
            }
            using var aes = Aes.Create();

            aes.Key = aesKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] iv = new byte[aes.BlockSize / 8];
            Array.Copy(encryptedData, 0, iv, 0, iv.Length); // Extract IV from the beginning of the encrypted data

            byte[] encryptedContent = new byte[encryptedData.Length - iv.Length];
            Array.Copy(encryptedData, iv.Length, encryptedContent, 0, encryptedContent.Length);

            byte[] result = aes.DecryptCbc(encryptedContent, iv, PaddingMode.PKCS7);

            return result;
        }

    }
}
