using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using HDMIOverIPApp.diagnostics.trace;

namespace HDMIOverIPApp.communication.net
{
    public class TCPComm
    {
        #region Fields
        private string _server;
        private int _port;
        private string _response = string.Empty;
        private Socket _client;
        private AsyncCallback _pfnWorkerCallback;
        private bool _isOpen;
        private bool _isConnecting;
        #endregion

        #region Events
        public delegate void DelegateDataReceived(byte[] data);
        public event DelegateDataReceived DataReceived;

        public delegate void SendCommand(NetPacket packet);
        public event SendCommand SendCommandHandler;

        public event EventHandler Connected;

        private EventWaitHandle connectDone;
        private EventWaitHandle receiveDone;
        #endregion

        #region Construction
        public TCPComm()
        {
            _server = "192.168.100.10";
            _port = 8000;
            _isOpen = false;
            _isConnecting = false;

            connectDone = new EventWaitHandle(false, EventResetMode.ManualReset);
            receiveDone = new EventWaitHandle(false, EventResetMode.ManualReset);
        }
        #endregion

        #region Open/Close
        public void Open(string server, int port)
        {
            _isConnecting = true;
            _server = server;
            _port = port;
            try
            {
                Thread commThread = new Thread(new ThreadStart(CommThread));
                commThread.Name = "TCPComm.CommThread";
                commThread.Start();
            }
            catch { }
        }

        public void Close()
        {
            if (_client != null)
            {
                if (_isOpen)
                    _client.Shutdown(SocketShutdown.Both);
                _client.Close();
                _client = null;
            }

            _isOpen = false;
            OnConnected();
        }

        private void ConnectCallback(IAsyncResult iar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)iar.AsyncState;

                
                // Complete the connection.
                if (client.Connected)
                {
                    client.EndConnect(iar);
                    connectDone.Set();
                }
            }
            catch (SocketException sex) 
            {
                System.Diagnostics.Debug.WriteLine(sex.Message);
            }
        }

        private void WaitForData(Socket client)
        {
            try
            {
                if (_pfnWorkerCallback == null)
                    _pfnWorkerCallback = new AsyncCallback(OnDataReceived);

                CSocketPacket state = new CSocketPacket();
                state.workSocket = client;

                client.BeginReceive(state.buffer, 0, CSocketPacket.BufferSize, SocketFlags.None,
                    _pfnWorkerCallback, state);
            }
            catch (SocketException sex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    sex,
                    "WaitForData: Socket failed!");
            }
        }

        public void OnDataReceived(IAsyncResult iar)
        {
            try
            {
                CSocketPacket state = (CSocketPacket)iar.AsyncState;
                Socket client = state.workSocket;

                if (!client.Connected)
                    return;

                int bytesRead = 0;

                try
                {
                    bytesRead = client.EndReceive(iar);
                }
                catch (SocketException sex)
                {
                    Trace.ShowException(
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                        sex,
                        "Apparently server has been closed and cannot answer.");

                    OnConnectionDropped(_client);
                    return;
                }

                if (bytesRead > 0)
                {
                    Array.Resize(ref state.buffer, bytesRead);
                    OnDataReceived(state.buffer);

                    WaitForData(_client);
                }
                else
                {
                    return;
                }
            }
            catch (SocketException sex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    sex,
                    "OnDataReceived: Exception!");
            }
        }

        public void SendReceive(NetCommand command, byte[] data)
        {
            if (_client == null)
                return;

            NetPacket packet = new NetPacket();
            NetCommandData commandData = new NetCommandData();

            commandData.Command = (byte)command;
            if (data != null)
            {
                commandData.Data = data;
            }

            packet.Add(commandData);

            OnDataSent(packet);

            _client.BeginSend(packet.RawData, 0, packet.RawData.Length, 0, new AsyncCallback(SendCallback), _client);
        }

        private void SendCallback(IAsyncResult iar)
        {
            try
            {
                Socket client = (Socket)iar.AsyncState;

                int bytesSent = client.EndSend(iar);

                System.Diagnostics.Debug.WriteLine(string.Format("Sent {0} bytes to Server", bytesSent));
            }
            catch (SocketException sex)
            {
                System.Diagnostics.Debug.Fail(sex.ToString(), "SendCallback: failed!");
            }
        }
        #endregion

        #region Helpers
        private void OnDataSent(NetPacket packet)
        {
            if (SendCommandHandler != null)
                SendCommandHandler(packet);
        }

        private void OnDataReceived(byte[] data)
        {
            if (DataReceived != null)
                DataReceived(data);
        }
        #endregion

        #region Properties
        public string Server
        {
            set { _server = value; }
            get { return _server; }
        }

        public int Port
        {
            set { _port = value; }
            get { return _port; }
        }

        public bool IsOpen
        {
            get { return _isOpen; }
        }

        public bool IsConnecting
        {
            get { return _isConnecting; }
        }
        #endregion

        #region Communication Thread
        private void CommThread()
        {
            try
            {
                //                string hostname = Dns.GetHostName();
                //                IPHostEntry ipHostEntry = Dns.GetHostEntry(hostname);
                //                IPAddress ipAddress = Dns.GetHostEntry(hostname).AddressList[4];
                IPAddress ipAddress = IPAddress.Parse(_server);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);

                int retry = 0;

                while (retry < 3)
                {
                    // Create a TCP/IP socket.
                    _client = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);

                    _client.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, 1000, 1000), null);

                    // Connect to the remote endpoint
                    _client.BeginConnect(remoteEP,
                        new AsyncCallback(ConnectCallback), _client);
                    if (connectDone.WaitOne(2000))
                    {
                        System.Diagnostics.Debug.WriteLine("Connect done");
                        _isOpen = true;
                        OnConnected();
                    }
                    else
                    {
                        _isOpen = false;
                        System.Diagnostics.Debug.WriteLine("Can't connect to server.");
                        _client.Close();
                    }

                    //                    connectDone.Reset();

                    if (_isOpen)
                        break;

                    retry++;

                    System.Diagnostics.Debug.WriteLine(string.Format("retry {0} times", retry));

                    Thread.Sleep(1000);
                }

                if (_isOpen)
                    WaitForData(_client);
                else
                    Close();
            }
            catch (SocketException sex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    sex,
                    "CommThread: Exception!");

                Close();
            }
            finally
            {
                _isConnecting = false;
            }
        }

        private byte[] KeepAlive(int onOff, int keepAliveTime, int keepAliveInterval)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);

            return buffer;
        }
        #endregion

        #region Event Handlers
        private void OnConnected()
        {
            if (Connected != null)
                Connected(this, new EventArgs());
        }

        private void OnDisconnection(Socket socket)
        { 
            
        }

        private void OnConnectionDropped(Socket socket)
        {
            Close();
        }
        #endregion
    }
}
