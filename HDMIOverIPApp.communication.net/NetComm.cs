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
    #region QueueHelper
    public class QueueHelper : Queue<NetPacket>
    {
        public ManualResetEvent WaitEvent = new ManualResetEvent(false);
        public object LockObject = new object();

        public NetPacket DequeuePacket()
        {
            NetPacket packet = null;

            lock (LockObject)
            {
                if (Count > 0)
                    packet = Dequeue();
            }
            WaitEvent.Reset();

            return packet;
        }

        public void EnqueuePacket(NetPacket packet)
        {
            lock (LockObject)
            {
                Enqueue(packet);
                WaitEvent.Set();
            }
        }
    }
    #endregion

    #region NetComm
    public class NetComm : IDisposable
    {
        private string _address = string.Empty;
        private string _port = string.Empty;
        private bool _isListening;
        private TcpListener _tcpListener;
        private EventWaitHandle _exitAcceptWaitHandle;
        private List<byte> _receiveData;
        private object _objectReceiveLock;

        private NetPacket _netPacket;

        #region Events
        public delegate void DelegateDataReceived(byte[] data);
        public event DelegateDataReceived DataReceived;

        private EventWaitHandle _timeoutHandle;
        private NetPacket _receivePacket;

        public delegate void SendCommand(NetPacket packet);
        public event SendCommand SendCommandHandler;
        #endregion

        private QueueHelper _queueHelperTransmit;
        private QueueHelper _queueHelperReceive;

//        public static readonly NetComm Instance = new NetComm();

        #region Construction
        public NetComm()
        {
            _queueHelperTransmit = new QueueHelper();
            _queueHelperReceive = new QueueHelper();

            _isListening = false;
            _exitAcceptWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            _timeoutHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            _receivePacket = new NetPacket();
            _receiveData = new List<byte>();
            _objectReceiveLock = new object();
        }
        #endregion

        public bool Open()
        {
            bool result = false;

            try
            {
                Thread commThread = new Thread(new ThreadStart(CommThread));
                commThread.Name = "NetComm.Server.CommThread";
                commThread.Start();
            }
            catch (Exception ex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    ex,
                    "NetComm.Server.Open: failed to open the tcplistener!");

                return false;
            }

            return result;
        }

        public void Close()
        {
            _isListening = false;
            _exitAcceptWaitHandle.Set();

            try
            {
                _tcpListener.Stop();
            }
            catch { }
        }

        #region Communication Thread
        private void CommThread()
        {
            if (_tcpListener != null)
                return;

            _isListening = true;
            WaitHandle waitHandle = _exitAcceptWaitHandle;

            try
            {
                string hostname = Dns.GetHostName();
                IPHostEntry ipHostEntry = Dns.GetHostEntry(hostname);
//                IPAddress serverIP = Dns.GetHostEntry(hostname).AddressList[4];
                int port = 8000;
                IPAddress serverIP = IPAddress.Parse("0.0.0.0");
                //                IPAddress ipAddr = IPAddress.Parse(_address);
                //                int port = Int32.Parse(_port);

                Console.WriteLine(string.Format("Server IP = {0}", serverIP.ToString()));

                _tcpListener = new TcpListener(serverIP, port);
                _tcpListener.Start();

                while (_isListening)
                {
                    _exitAcceptWaitHandle.Reset();

                    _tcpListener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), _tcpListener);

                    waitHandle.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    ex,
                    "NetComm.Server.CommThread: exception!");
            }
        }

        public void AcceptCallback(IAsyncResult asyncResult)
        {
            try
            {
                TcpListener tcpListener = (TcpListener)asyncResult.AsyncState;

                TcpClient tcpClient = tcpListener.EndAcceptTcpClient(asyncResult);

                NetworkStream ns = tcpClient.GetStream();

                Byte[] bytes = new Byte[256];

                int read = 0;

                do
                {
                    read = ns.Read(bytes, 0, bytes.Length);
                    System.Diagnostics.Debug.WriteLine(string.Format("read = {0} data = {1}", read, Encoding.ASCII.GetString(bytes)));

                    if (read != 0)
                    {
                        Array.Resize(ref bytes, read);
                        OnDataReceived(bytes);
                    }
                } while (read != 0);

                ns.Close();
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    ex,
                    "NetComm.Server.AcceptCallback: exception!");
            }
            finally
            {
                _exitAcceptWaitHandle.Set();
            }
        }
        #endregion

        #region Helpers
        internal void SendCommandBase(NetPacket packet)
        {
            if (SendCommandHandler != null)
                SendCommandHandler(packet);
        }

        private void PrepareSendSinglePacket(NetCommand command, byte[] data)
        {
            NetPacket packet = new NetPacket();
            NetCommandData commandData = new NetCommandData();

            commandData.Command = (byte)command;
            if (data != null)
            {
                commandData.Data = data;
            }

            packet.Add(commandData);

            SendCommandBase(packet);
        }

        public void SendReceive(NetCommand command, byte[] sendData)
        {
            PrepareSendSinglePacket(command, sendData);
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
        }
        #endregion

        #region Properties
        public bool IsOpen
        {
            get { return _isListening; }  
        }

        public NetPacket NetPacket { set { _netPacket = value; } }
        #endregion

        #region Event Handlers
        private void OnDataReceived(byte[] data)
        {
            if (DataReceived != null)
                DataReceived(data);
        }
        #endregion
    }
    #endregion

    #region Server
    public class Server
    {
        private string _address = string.Empty;
        private string _port = string.Empty;
        private bool _isListening;
        private TcpListener _tcpListener;
        private EventWaitHandle _exitAcceptWaitHandle;

        public delegate void DelegateMessage(string msg);
        public event DelegateMessage ReceivedMessage;

        public Server()
        {
            _isListening = false;
            _exitAcceptWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        }

        public bool Open()
        {
            bool result = false;

            try
            {
                Thread commThread = new Thread(new ThreadStart(CommThread));
                commThread.Name = "NetComm.Server.CommThread";
                commThread.Start();
            }
            catch (Exception ex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    ex,
                    "NetComm.Server.Open: failed to open the tcplistener!");

                return false;
            }

            return result;
        }

        public void Close()
        {
            _isListening = false;
            _exitAcceptWaitHandle.Set();

            _tcpListener.Stop();
        }

        #region Communication Thread
        private void CommThread()
        {
            if (_tcpListener != null)
                return;

            _isListening = true;
            WaitHandle waitHandle = _exitAcceptWaitHandle;

            try
            {
                string hostname = Dns.GetHostName();
                IPHostEntry ipHostEntry = Dns.GetHostEntry(hostname);
                IPAddress serverIP = Dns.GetHostEntry(hostname).AddressList[4];
                int port = 8000;
//                IPAddress ipAddr = IPAddress.Parse(_address);
//                int port = Int32.Parse(_port);

                Console.WriteLine(string.Format("Server IP = {0}", serverIP.ToString()));
                
                _tcpListener = new TcpListener(serverIP, port);
                _tcpListener.Start();

                while (_isListening)
                {
                    _exitAcceptWaitHandle.Reset();

                    _tcpListener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), _tcpListener);

                    waitHandle.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    ex,
                    "NetComm.Server.CommThread: exception!");
            }
        }

        public void AcceptCallback(IAsyncResult asyncResult)
        {
            try
            {
                TcpListener tcpListener = (TcpListener)asyncResult.AsyncState;

                TcpClient tcpClient = tcpListener.EndAcceptTcpClient(asyncResult);

                NetworkStream ns = tcpClient.GetStream();

                Byte[] bytes = new Byte[256];
                string receivedMsg = string.Empty;

                int read = 0;

                do
                {
                    read = ns.Read(bytes, 0, bytes.Length);

                    if (read != 0)
                    {
                        receivedMsg = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                        ReceivedMessage(receivedMsg);
                    }
                } while (read != 0);

                tcpClient.Close();
                _exitAcceptWaitHandle.Set();
            }
            catch (Exception ex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    ex,
                    "NetComm.Server.AcceptCallback: exception!");
            }
            finally
            {
                _exitAcceptWaitHandle.Set();
            }
        }
        #endregion
    }
    #endregion

    #region Client
    public class Client
    {
        public delegate void DelegateMessage(string msg);
        public event DelegateMessage SendMessage;

        public Client()
        { }

        public void Connect(string server, int port, byte[] data)
        {
            try
            {
                TcpClient tcpClient = new TcpClient(server, port);

                NetworkStream ns = tcpClient.GetStream();

                ns.Write(data, 0, data.Length);

                ns.Close();
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    ex,
                    "NetComm.Client.Connect: exception!");            
            }
        }

        public void Connect(string server, int port, string message)
        {
            try
            {
                TcpClient tcpClient = new TcpClient(server, port);

                Byte[] data = Encoding.ASCII.GetBytes(message);

                NetworkStream ns = tcpClient.GetStream();

                SendMessage(message);

                ns.Write(data, 0, data.Length);

                ns.Close();
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    ex,
                    "NetComm.Client.Connect: exception!");
            }
        }
    }
    #endregion
}
