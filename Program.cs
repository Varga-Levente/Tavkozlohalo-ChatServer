using ChatServer.Net.IO;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    class Program
    {
        static List<Client> _users;
        static TcpListener _listener;

        // Main function starts the server and listens for incoming connections on port 7890
        // (Currently the port is not editable)
        static void Main(string[] args)
        {
            _users = new List<Client>();
            _listener = new TcpListener(IPAddress.Parse("0.0.0.0"), 7890);
            _listener.Start();

            while (true)
            {
                var client = new Client(_listener.AcceptTcpClient());
                _users.Add(client);

                /*Broadcat to everyone on the server*/
                BroadcastConnection();
            }
        }

        // Broadcasts the connection to all users on the server
        static void BroadcastConnection()
        {
            foreach (var user in _users)
            {
                foreach (var usr in _users)
                {
                    var broadcastPacket = new PacketBuilder();
                    broadcastPacket.WriteOpCode(1);
                    broadcastPacket.WriteMessage(usr.Username);
                    broadcastPacket.WriteMessage(usr.UID.ToString());
                    user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                }
            }
        }

        // Broadcasts the message to all users on the server
        public static void BroadcastMessage(string message)
        {
            foreach(var user in _users)
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode(5);
                msgPacket.WriteMessage(message);
                user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
            }
        }


        // Sends a private message to a specific user
        public static void SendPrivateMessage(string message, string targetusername, string sender)
        {
            var targetuser = _users.Where(x => x.Username.ToString() == targetusername).FirstOrDefault();
            var senderuser = _users.Where(x => x.Username.ToString() == sender).FirstOrDefault();
            if (targetuser == null)
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode(55);
                msgPacket.WriteMessage("[ERROR] - User not found!");
                senderuser.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
                return;
            }
            else
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode(55);
                msgPacket.WriteMessage(message);
                targetuser.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
                senderuser.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
            }
        }

        // Sends a file to a specific user
        public static void SendFile(string fromUser, string filename, string targetusername, string fileurl)
        {
            Console.WriteLine($"File saved on API server. From: {fromUser} | File: {filename} | To: {targetusername}");

            var targetuser = _users.Where(x => x.Username.ToString() == targetusername).FirstOrDefault();
            var senderuser = _users.Where(x => x.Username.ToString() == fromUser).FirstOrDefault();
            
            if (targetuser != null)
            {
                var filePacket = new PacketBuilder();
                filePacket.WriteOpCode(21);
                filePacket.WriteMessage($"{fromUser}|{filename}|{fileurl}");
                targetuser.ClientSocket.Client.Send(filePacket.GetPacketBytes());

                var senderinfo = new PacketBuilder();
                senderinfo.WriteOpCode(55);
                senderinfo.WriteMessage("[FILE] - The file sending request has been sent successfully.");
                senderuser.ClientSocket.Client.Send(senderinfo.GetPacketBytes());

            }
        }

        // Broadcasts the disconnection to all users on the server
        public static void BroadcastDisconnect(string uid)
        {
            var disconnectedUser = _users.Where(x => x.UID.ToString() == uid).FirstOrDefault();
            _users.Remove(disconnectedUser);

            foreach(var user in _users)
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(10);
                broadcastPacket.WriteMessage(uid);
                user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
            }
            BroadcastMessage($"({disconnectedUser.Username} Disconnected!)");
        }
    }
}