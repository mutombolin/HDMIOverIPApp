using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HDMIOverIPApp.diagnostics.trace;

namespace HDMIOverIPApp.communication.net
{
    public class NetMaster
    {
        #region Fields
//        private NetComm _comm;
        private TCPComm _tcpComm;
        private UDPComm _udpComm;
        private object _objectReceiveLock;
        private object _objectSendLock;
        private List<byte> _receiveData;
        private System.Timers.Timer _timerReceiveTimeout;
        private string _server;
        private int _port;
        private List<DeviceData> _deviceList;
        #endregion

        #region Events
        public delegate void DelegateMessage(string msg);
        public event DelegateMessage ReceivedMessage;

        public event EventHandler Connected;
        public event EventHandler UpdateDevice;
        #endregion

        #region Construction
        public NetMaster()
        {
//            _comm = new NetComm();
//            _comm.DataReceived += new NetComm.DelegateDataReceived(_comm_DataReceived);
//            _comm.SendCommandHandler += new NetComm.SendCommand(_comm_SendCommandHandler);

            _tcpComm = new TCPComm();
            _tcpComm.SendCommandHandler += new TCPComm.SendCommand(_comm_SendCommandHandler);
            _tcpComm.DataReceived += new TCPComm.DelegateDataReceived(_comm_DataReceived);
            _tcpComm.Connected += new EventHandler(_tcpComm_Connected);

            _udpComm = new UDPComm();
            _udpComm.DataReceived += new UDPComm.DelegateDataReceived(_udpComm_DataReceived);
            _udpComm.UpdateStatus += new UDPComm.DelegateUpdateStatus(_udpComm_UpdateStatus);
            _udpComm.Message = "Hello\n";

            _deviceList = new List<DeviceData>();

            _objectReceiveLock = new object();
            _objectSendLock = new object();
            _receiveData = new List<byte>();

            _timerReceiveTimeout = new System.Timers.Timer();
            _timerReceiveTimeout.Elapsed += new System.Timers.ElapsedEventHandler(_timerReceiveTimeout_Elapsed);
        }
        #endregion

        #region Open/Close
        public bool Open(string server, int port)
        {
            try
            {
                if (_tcpComm != null)
                    _tcpComm.Open(server, port);
//                if (_comm != null)
//                    _comm.Open();
            }
            catch (Exception ex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    ex,
                    "Failed to open the httpListener");
                return false;
            }

            return true;
        }

        public void Close()
        {
//            if (_comm != null)
//                _comm.Close();

            if (_tcpComm != null)
                _tcpComm.Close();
        }
        #endregion

        #region Broadcast
        public void StartBroadcast(string server, int port)
        {
            _deviceList.Clear();

            if (_udpComm != null)
                _udpComm.StartBroadcast(server, port);
        }
        #endregion

        #region Event Handlers
        void _udpComm_UpdateStatus(UDPCommStatus status)
        {
            string message = string.Empty;

            switch (status)
            { 
                case UDPCommStatus.Broadcasting:
                    message = "Start broadcasting";
                    break;
                case UDPCommStatus.Broadcasted:
                    message = "Broadcast finished";
                    OnUpdateDevice();
                    break;
            }
            OnReceiveMessage(message);
        }

        private void _udpComm_DataReceived(byte[] data)
        {
            System.Diagnostics.Debug.WriteLine(Encoding.ASCII.GetString(data));

            DeviceData device = new DeviceData();
            device.Data = data;

            _deviceList.Add(device);
        }

        void _comm_DataReceived(byte[] data)
        {
            bool packetComplete = false;

            string message = string.Empty;

            lock (_objectReceiveLock)
            {
//                System.Diagnostics.Debug.Write("ReceiveData = ");
                message += "ReceiveData =";

                foreach (byte b in data)
                {
                    _receiveData.Add(b);

//                    System.Diagnostics.Debug.Write(string.Format(" 0x{0:X2}", b));
                    message += string.Format(" 0x{0:X2}", b); 

                    if (b == 0xF2)
                    {
                        NetPacket packet = new NetPacket();

                        packet.RawData = _receiveData.ToArray();

                        if (packet.NetPacketParseResult == NetPacketParseResult.Success)
                        {
//                            _comm.NetPacket = packet;
                        }
                        else
                        {
                            Trace.ShowMessage(
                                System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                                "Failed to parse Net packet! Error = " + packet.NetPacketParseResult.ToString(),
                                Trace.MessageLevel.Error);
                        }

                        _receiveData.Clear();
                        packetComplete = true;
                        _timerReceiveTimeout.Stop();

//                        System.Diagnostics.Debug.Write("\r\n");
                        OnReceiveMessage(message);
                    }
                }

                if (!packetComplete)
                    _timerReceiveTimeout.Start();
            }
        }

        void _comm_SendCommandHandler(NetPacket packet)
        {
//            if (!_comm.IsOpen)
            if (!_tcpComm.IsOpen)
                return;

            string message = string.Empty;

            message += "SendData =";

            foreach (byte b in packet.RawData)
                message += string.Format(" 0x{0:X2}", b);

            OnReceiveMessage(message);
/*
            lock (_objectSendLock)
            {
                byte[] packetData = packet.RawData;
                Client client = new Client();
                client.Connect(_server, _port, packetData);
            }
*/
        }

        private void _tcpComm_Connected(object sender, EventArgs e)
        {
            OnConnected();
        }
        #endregion

        #region Receive timeout handler
        void _timerReceiveTimeout_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_objectReceiveLock)
            {
                _receiveData.Clear();
            }

            _timerReceiveTimeout.Stop();
        }
        #endregion

        #region Properties
        public List<DeviceData> Devices
        {
            get { return _deviceList; }
        }

        public bool IsBroadcasting
        {
            get { return _udpComm.IsBroadcasting; }
        }
 
        public bool IsConnecting
        {
            get { return _tcpComm.IsConnecting; }
        }
        
        public bool IsOpen
        {
            get { return _tcpComm.IsOpen; }
        }

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
        #endregion

        #region Methods
        public int GetStatus()
        {
            int status = 0;

            _tcpComm.SendReceive(NetCommand.Status, null);
//            _comm.SendReceive(NetCommand.Status, null);

            return status;
        }

        public void SetSTBControlType(byte type)
        {
            List<byte> data = new List<byte>();

            data.Add(type);

            _tcpComm.SendReceive(NetCommand.SetSTBControlType, data.ToArray());
//            _comm.SendReceive(NetCommand.SetSTBControlType, data.ToArray());
        }

        public void GetMCUSWVersion()
        {
//            _comm.SendReceive(NetCommand.GetMCUVersion, null);
            _tcpComm.SendReceive(NetCommand.GetMCUVersion, null);
        }

        public void SetIRProtocol(byte[] data)
        {
//            _comm.SendReceive(NetCommand.SetIR, data);
            _tcpComm.SendReceive(NetCommand.SetIR, data);
        }

        public void SendIRRawPattern()
        {
//            _comm.SendReceive(NetCommand.SendIRRawPattern, null);
            _tcpComm.SendReceive(NetCommand.SendIRRawPattern, null);
        }

        public void SetTVCommand()
        {
//            _comm.SendReceive(NetCommand.SetTVCommand, null);
            _tcpComm.SendReceive(NetCommand.SetTVCommand, null);
        }
        #endregion

        #region Event Handlers
        private void OnUpdateDevice()
        {
            if (UpdateDevice != null)
                UpdateDevice(this, new EventArgs());
        }

        private void OnReceiveMessage(string msg)
        {
            if (ReceivedMessage != null)
                ReceivedMessage(msg);
        }

        private void OnConnected()
        {
            if (Connected != null)
                Connected(this, new EventArgs());
        }
        #endregion
    }
}
