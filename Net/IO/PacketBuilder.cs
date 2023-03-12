using System.Text;

namespace ChatServer.Net.IO
{
    class PacketBuilder
    {
        MemoryStream _ms;

        // Constructor for the PacketBuilder
        public PacketBuilder()
        {
            _ms = new MemoryStream();
        }

        // Write the opcode to the packet (Opcode identifies the action)
        public void WriteOpCode(byte opcode)
        {
            _ms.WriteByte(opcode);
        }

        // Write the message to the packet (Message is the data)
        public void WriteMessage(string msg)
        {
            var msgLength = msg.Length;
            _ms.Write(BitConverter.GetBytes(msgLength));
            _ms.Write(Encoding.ASCII.GetBytes(msg));
        }

        // This function creates byte array from the packet
        public byte[] GetPacketBytes()
        {
            return _ms.ToArray();
        }
    }
}
