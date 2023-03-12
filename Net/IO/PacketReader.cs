using System.Net.Sockets;
using System.Text;

namespace ChatServer.Net.IO
{
    class PacketReader : BinaryReader
    {
        private NetworkStream _ns;

        // Constructor for the PacketReader
        public PacketReader(NetworkStream ns) : base(ns)
        {
            _ns = ns;
        }

        // Reads the incoming packet and returns entire packet as a string
        public string ReadMessage()
        {
            byte[] msgBuffer;
            var length = ReadInt32();
            msgBuffer = new byte[length];
            _ns.Read(msgBuffer, 0, length);

            var msg = Encoding.ASCII.GetString(msgBuffer);

            return msg;
        }
    }
}
