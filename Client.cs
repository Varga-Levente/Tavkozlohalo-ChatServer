using ChatServer.Net.IO;
using System.Net.Sockets;

namespace ChatServer
{
    internal class Client
    {
        public string Username { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }

        PacketReader _packetReader;

        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();

            _packetReader = new PacketReader(client.GetStream()); // create new PacketReader instance
            var opcode = _packetReader.ReadByte();
            Username = _packetReader.ReadMessage();

            Console.WriteLine($"[{DateTime.Now}]: Client has connected with the username: {Username}");

            Task.Run(() => Process());

        }

        void Process()
        {
            while ( true )
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    switch ( opcode )
                    {
                        case 5:
                            var msg = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: Message recieved! {msg}");
                            Program.BroadcastMessage( $"[{Username}]: {msg}" );
                            break;
                        case 55:
                            var pmp = _packetReader.ReadMessage();
                            var pmsplit = pmp.Split(' ');
                            var TARGETUSER = pmsplit[1].Replace("@", "");
                            var pm = string.Join(" ", pmsplit.Skip(2));

                            Console.WriteLine($"From: {Username} | To: {TARGETUSER} | Message: {pm}");

                            //Console.WriteLine($"[{DateTime.Now}]: Pivate message recieved! From: {Username} | To: {TARGETUSER}");
                            Program.SendPrivateMessage($"[Private][{Username}]: {pm}", TARGETUSER, Username);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"[{UID.ToString()}]: Disconnected!");
                    Program.BroadcastDisconnect(UID.ToString());
                    ClientSocket.Close();
                    break;
                }
            }
        }

    }
}
