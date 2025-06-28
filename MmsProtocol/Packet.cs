using System.Runtime.Intrinsics.Arm;
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
        public const int AES_RAW_KEY_SIZE = 192;

        public PacketType Type { get; set; }
        public string[] Data { get; set; }

        public Packet(PacketType type, params string[] data)
        {
            Type = type;
            Data = data;
        }

        public byte[] Serialize()
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this));
        }

        public static Packet Deserialize(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);

            return JsonSerializer.Deserialize<Packet>(json) 
                ?? throw new InvalidOperationException("Deserialization failed");
        }

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
    }
}
