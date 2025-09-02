using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaserControllerApp.Models
{
    public class FpgaCommand
    {
        /// <summary>
        /// Command ID, usually from FpgaCommandIds.
        /// </summary>
        public ushort Command { get; set; }

        /// <summary>
        /// Payload data bytes.
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FpgaCommand() { }

        /// <summary>
        /// Constructor with command ID and optional data.
        /// </summary>
        /// <param name="command">Command ID</param>
        /// <param name="data">Optional payload</param>
        public FpgaCommand(ushort command, byte[]? data = null)
        {
            Command = command;
            Data = data ?? Array.Empty<byte>();
        }

        /// <summary>
        /// Returns a readable string representation.
        /// </summary>
        public override string ToString()
        {
            string dataStr = Data != null && Data.Length > 0
                ? BitConverter.ToString(Data)
                : "No Data";

            return $"Command: 0x{Command:X4}, Data: {dataStr}";
        }
    }
}
