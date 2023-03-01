using ChatServer.Net.IO;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace ChatServer
{
    class Program
    {
        static List<Client> _users;
        static TcpListener _listener;
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