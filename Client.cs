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

        // Client constructor creates new PacketReader instance
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

        // Process the incoming packets from the client and handle them accordingly (opcode)
        // Opcode 5 is a global message
        // Opcode 20 is a file
        // Opcode 55 is a private message
        // On catch (Exception) the client has disconnected
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
                            Console.WriteLine($"[{DateTime.Now}]: Global message recieved!");
                            Program.BroadcastMessage( $"[{Username}]: {msg}" );
                            break;
                        case 20:
                            var file = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: File recieved! {file}");
                            var fileSplit = file.Split('|');
                            var toUser = fileSplit[0];
                            var fileName = fileSplit[1];
                            var filedownloadurl = fileSplit[2];

                            Console.WriteLine($"{fileName}");

                            Program.SendFile( Username, toUser, fileName, filedownloadurl );
                            break;
                        case 55:
                            var pmp = _packetReader.ReadMessage();
                            var pmsplit = pmp.Split(' ');
                            var TARGETUSER = pmsplit[1].Replace("@", "");
                            var pm = string.Join(" ", pmsplit.Skip(2));
                            Console.WriteLine($"[Private Message] - From: {Username} | To: {TARGETUSER}");
                            Program.SendPrivateMessage($"[{Username}]->[{TARGETUSER}]: {pm}", TARGETUSER, Username);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"[{Username}][{UID.ToString()}]: Disconnected!");
                    Program.BroadcastDisconnect(UID.ToString());
                    ClientSocket.Close();
                    break;
                }
            }
        }

    }
}
