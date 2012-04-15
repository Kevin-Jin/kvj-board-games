using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace KvjBoardGames.OnlineFunctions
{
    public delegate void OpponentConnected(NetworkInterface comm, EndPoint remoteAddress);
    public delegate void OpponentDisconnected();
    public delegate void SocketExceptionCaught(SocketException e);
    public delegate void ClosedSocketException(ObjectDisposedException e);

    public abstract class NetworkInterface
    {
        protected class PartialPacket
        {
            private const int MAX_BUFFER_SIZE = 1024;

            public Socket Socket;
            public byte[] Buffer = new byte[MAX_BUFFER_SIZE];
        }

        public abstract PacketProcessor Handler { get; set; }

        public event OpponentConnected Connected;
        public event OpponentDisconnected Disconnected;
        public event SocketExceptionCaught CaughtSocketException;
        public event ClosedSocketException CaughtClosedSocket;

        protected void OnConnected(EndPoint remoteAddress)
        {
            if (Connected != null)
                Connected(this, remoteAddress);
        }

        protected void OnDisconnected()
        {
            if (Disconnected != null)
                Disconnected();
        }

        protected void OnSocketExceptionCaught(SocketException e)
        {
            if (CaughtSocketException != null)
                CaughtSocketException(e);
        }

        protected void OnSocketClosedException(ObjectDisposedException e)
        {
            if (CaughtClosedSocket != null)
                CaughtClosedSocket(e);
        }

        public abstract void SendMessage(byte[] packet);
        public abstract void Disconnect();
    }

    public interface PacketProcessor
    {
        void Handle(byte[] packet);
    }
}
