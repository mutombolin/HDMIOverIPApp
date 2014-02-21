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
    #region NetComm
    public class NetComm : IDisposable
    {

        public void Dispose()
        { 
        }
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
