using System.Text;
using System.Text.Json;

namespace MmsProtocol
{
    public enum PacketType
    {
        LoginRequest = 0x0000-0001,
        LoginResponse = 0x0000-0002,
        SignUpRequest = 0x0000-0003,
        SignUpResponse = 0x0000-0004,
    }
 
    public class Packet
    {
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
    }
}
