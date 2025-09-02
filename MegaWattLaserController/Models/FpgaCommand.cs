using System;

namespace LaserControllerApp.Models
{
    public class FpgaCommand
    {
        public ushort Command { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public int Length => Data?.Length ?? 0;

        public FpgaCommand(ushort command, byte[]? data = null)
        {
            Command = command;
            Data = data ?? Array.Empty<byte>();
        }

        public FpgaCommand()
        {
            // Default constructor
        }

        public byte[] ToByteArray()
        {
            var result = new byte[Data.Length + 4]; // Command (2 bytes) + Length (2 bytes) + Data
            result[0] = (byte)((Command >> 8) & 0xFF);
            result[1] = (byte)(Command & 0xFF);
            result[2] = (byte)(Data.Length >> 8);
            result[3] = (byte)(Data.Length & 0xFF);
            Array.Copy(Data, 0, result, 4, Data.Length);
            return result;
        }

        public static FpgaCommand FromByteArray(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4)
                return null;

            var command = (ushort)((bytes[0] << 8) | bytes[1]);
            var length = (bytes[2] << 8) | bytes[3];

            if (bytes.Length < 4 + length)
                return null;

            var data = new byte[length];
            Array.Copy(bytes, 4, data, 0, length);

            return new FpgaCommand(command, data);
        }

        public override string ToString()
        {
            return $"Command: 0x{Command:X4}, Data: {BitConverter.ToString(Data)}";
        }
    }
}