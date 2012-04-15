using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace KvjBoardGames.OnlineFunctions
{
    internal class Client : NetworkInterface
    {
        public AsyncCallback dataReceivedCallback;
        public Socket workerSocket;

        private PacketProcessor handler;
        public override PacketProcessor Handler { get { return handler; } set { handler = value; } }

        internal Client()
        {
            dataReceivedCallback = new AsyncCallback(OnDataReceived);
            workerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            workerSocket.NoDelay = true;
        }

        internal void Connect(string host, ushort port)
        {
            workerSocket.BeginConnect(host, port, new AsyncCallback(OnConnected), null);
        }

        private void OnConnected(IAsyncResult asyn)
        {
            try
            {
                workerSocket.EndConnect(asyn);
                WaitForData(workerSocket);
                OnConnected(workerSocket.RemoteEndPoint);
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

        public override void SendMessage(byte[] packet)
        {
            workerSocket.Send(packet);
        }

        public override void Disconnect()
        {
            if (workerSocket != null)
            {
                if (workerSocket.Connected)
                    workerSocket.Close();
                workerSocket = null;
            }
        }
    }
}
