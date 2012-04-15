using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace KvjBoardGames.OnlineFunctions
{
    internal class ClientListener : NetworkInterface
    {
        public AsyncCallback dataReceivedCallback, clientConnectedCallback;
        public Socket listenSocket;
        public Socket workerSocket;

        private PacketProcessor handler;
        public override PacketProcessor Handler { get { return handler; } set { handler = value; } }

        internal ClientListener()
        {
            dataReceivedCallback = new AsyncCallback(OnDataReceived);
            clientConnectedCallback = new AsyncCallback(OnConnected);
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.NoDelay = true;
        }

        internal SocketException Listen(ushort port)
        {
            try
            {
                listenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                listenSocket.Listen(1);
                listenSocket.BeginAccept(clientConnectedCallback, null);
                return null;
            }
            catch (SocketException e)
            {
                return e;
            }
        }

        private void OnConnected(IAsyncResult asyn)
        {
            try
            {
                workerSocket = listenSocket.EndAccept(asyn);
                workerSocket.NoDelay = true;
                WaitForData(workerSocket);
                OnConnected(workerSocket.RemoteEndPoint);
                listenSocket.BeginAccept(clientConnectedCallback, null);
            }
            catch (ObjectDisposedException ode)
            {
                OnSocketClosedException(ode);
            }
            catch (SocketException se)
            {
                OnSocketExceptionCaught(se);
            }
        }

        private void WaitForData(Socket socket)
        {
            try
            {
                PartialPacket socketData = new PartialPacket();
                socketData.Socket = socket;
                socket.BeginReceive(socketData.Buffer, 0,
                                   socketData.Buffer.Length,
                                   SocketFlags.None,
                                   dataReceivedCallback,
                                   socketData);
            }
            catch (SocketException se)
            {
                OnSocketExceptionCaught(se);
            }
        }

        private void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                PartialPacket socketData = (PartialPacket)asyn.AsyncState;
                int iRx = socketData.Socket.EndReceive(asyn);
                if (iRx > 0)
                {
                    if (socketData.Buffer[0] == CommonOpHeaders.END_SESSION)
                    {
                        Disconnect();
                        OnDisconnected();
                    }
                    else
                    {
                        handler.Handle(socketData.Buffer);
                    }
                }
                WaitForData(socketData.Socket);
            }
            catch (ObjectDisposedException ode)
            {
                OnSocketClosedException(ode);
            }
            catch (SocketException se)
            {
                OnSocketExceptionCaught(se);
            }
        }

        public override void Disconnect()
        {
            if (workerSocket != null)
            {
                if (workerSocket.Connected)
                    workerSocket.Shutdown(SocketShutdown.Both);
                workerSocket.Close();
            }
            if (listenSocket != null)
            {
                if (listenSocket.Connected)
                    listenSocket.Shutdown(SocketShutdown.Both);
                listenSocket.Close();
            }
            OnDisconnected();
        }

        public override void SendMessage(byte[] packet)
        {
            workerSocket.Send(packet);
        }
    }
}
