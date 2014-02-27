using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Sockets;
using System.Net;
using System.Threading;

using HDMIOverIPApp.diagnostics.trace;

namespace HDMIOverIPApp.communication.net
{
    public enum UDPCommStatus
    { 
        Broadcasting,
        Broadcasted
    }

    public class UDPComm
    {
        #region Fields
        private string _server;
        private int _port;
        private bool _isBroadcasting;
        private bool _hasFound;
        private string _message;
        private AsyncCallback _pfnWorkerCallback;
        #endregion

        #region Events
        public delegate void DelegateDataReceived(byte[] data);
        public event DelegateDataReceived DataReceived;

        public delegate void DelegateUpdateStatus(UDPCommStatus status);
        public event DelegateUpdateStatus UpdateStatus;

        private EventWaitHandle ReceiveDone;
        #endregion

        #region Construction
        public UDPComm()
        {
            _port = 8888;
            _hasFound = false;
            _isBroadcasting = false;
            _message = string.Empty;

            ReceiveDone = new EventWaitHandle(false, EventResetMode.ManualReset);
        }
        #endregion

        #region Open/Close
        public void StartBroadcast(string server, int port)
        {
            _server = server;
            _port = port;
            Thread commThread = new Thread(new ThreadStart(UDPCommThread));
            commThread.Name = "UDPComm.UDPCommThread";
            commThread.Start();
        }

        public void StopBroadcast()
        { }
        #endregion

        #region UDPCommThread
        private void UDPCommThread()
        {
            try
            {
                _isBroadcasting = true;
                OnUpdateStatus(UDPCommStatus.Broadcasting);

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.EnableBroadcast = true;
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

                var multicastAddress = IPAddress.Parse("239.0.0.222");
                var multicastEP = new IPEndPoint(multicastAddress, _port);

//                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multicastAddress, IPAddress.Any));

                socket.Connect(multicastEP);

                Socket socketReceive = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socketReceive.Bind(socket.LocalEndPoint);

                byte[] data = Encoding.ASCII.GetBytes(_message);

//                socket.SendTo(data, 0, data.Length, SocketFlags.None, multicastEP);
                socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), socket);

                while (_isBroadcasting)
                {
                    ReceiveDone.Reset();
                    WaitForUDPData(socketReceive);

                    if (ReceiveDone.WaitOne(2000))
                    {
                        WaitForUDPData(socket);
                    }
                    else
                    {
                        _isBroadcasting = false;
                    }
                }

//                socket.Shutdown(SocketShutdown.Both);
//                socket.Close();
                OnUpdateStatus(UDPCommStatus.Broadcasted);
            }
            catch (SocketException sex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    sex,
                    "UDPCommThread: Exception!");
            }
        }
        #endregion

        #region Send
        private void SendCallback(IAsyncResult iar)
        {
            try
            {
                Socket socket = (Socket)iar.AsyncState;

                int bytesSent = socket.EndSend(iar);
            }
            catch (SocketException sex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    sex,
                    "SendCallback: Exception!");
            }
        }
        #endregion

        #region Receive
        private void WaitForUDPData(Socket socket)
        {
            try
            {
                if (_pfnWorkerCallback == null)
                    _pfnWorkerCallback = new AsyncCallback(OnUDPDataReceived);

                CSocketPacket socketPacket = new CSocketPacket();
                socketPacket.workSocket = socket;

//                ReceiveDone.Reset();
                socket.BeginReceive(socketPacket.buffer, 0, CSocketPacket.BufferSize, SocketFlags.None, _pfnWorkerCallback, socketPacket);
/*
                if (ReceiveDone.WaitOne(2000))
                {
                    System.Diagnostics.Debug.WriteLine("Received");
                    WaitForUDPData(socket);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ReceiveDone Expired!");
                    socket.Close();
                }
*/
            }
            catch (SocketException sex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    sex,
                    "WaitForUDPData: Exception!");
            }
        }

        private void OnUDPDataReceived(IAsyncResult iar)
        {
            CSocketPacket socketPacket = (CSocketPacket)iar.AsyncState;
            Socket socket = socketPacket.workSocket;

            try
            {
                //                if (!socket.Connected)
                //                    return;

                int bytesRead = 0;

                try
                {
                    bytesRead = socket.EndReceive(iar);
                }
                catch (SocketException sex)
                {
                    Trace.ShowException(
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                        sex,
                        "OnUDPDataReceived: EndReceive exception!");

                    return;
                }
                finally
                {
//                    socket.Close();
                }

                if (bytesRead > 0)
                {
                    Array.Resize(ref socketPacket.buffer, bytesRead);
                    OnDataReceived(socketPacket.buffer);
                }
                else
                {
                    socket.Close();
                    return;
                }

                ReceiveDone.Set();
            }
            catch (SocketException sex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    sex,
                    "OnUDPDataReceived: Exception!");
            }
        }
        #endregion

        #region Broadcast
        public void BroadcastMessage(byte[] message, int port)
        {
            _port = port;

            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.EnableBroadcast = true;
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Broadcast, port);

                socket.SendTo(message, remoteEP);
            }
            catch (SocketException sex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    sex,
                    "BroadcastMessage: Exception!");
            }
        }
        #endregion

        #region Receive Broadcast
        private void ReceiveBroadcastMessage(Action<EndPoint, byte[]> receiveAction, int port)
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, port) as EndPoint;

                socket.Bind(remoteEP);

                byte[] buffer = new byte[512];
                var recv = socket.ReceiveFrom(buffer, ref remoteEP);
                var data = new byte[recv];

                Array.Copy(buffer, 0, data, 0, recv);
            }
            catch (SocketException sex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    sex,
                    "ReceiveBroadcastMessage: Exception!");
            }
        }
        #endregion

        #region Properties
        public int Port
        {
            set { _port = value; }
            get { return _port; }
        }

        public bool HasFound
        {
            get { return _hasFound; }
        }

        public bool IsBroadcasting
        {
            get { return _isBroadcasting; }
        }

        public string Message
        {
            set { _message = value; }
        }
        #endregion

        #region Event Handlers
        private void OnDataReceived(byte[] data)
        {
            if (DataReceived != null)
                DataReceived(data);
        }

        private void OnUpdateStatus(UDPCommStatus status)
        {
            if (UpdateStatus != null)
                UpdateStatus(status);
        }
        #endregion
    }
}
