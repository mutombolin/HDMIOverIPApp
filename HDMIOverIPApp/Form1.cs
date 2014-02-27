using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using HDMIOverIPApp.diagnostics.trace;
using HDMIOverIPApp.communication.net;

namespace HDMIOverIPApp
{
    public partial class Form1 : Form
    {
        private HDMIOverIPApp.communication.net.NetMaster _netMaster;
        private bool _isConnected = false;

        public Form1()
        {
            InitializeComponent();

            _netMaster = new NetMaster();
            _netMaster.ReceivedMessage += new NetMaster.DelegateMessage(_netMaster_ReceivedMessage);
            _netMaster.Connected += new EventHandler(_netMaster_Connected);
            _netMaster.UpdateDevice += new EventHandler(_netMaster_UpdateDevice);

//            NetComm.Instance.ReceivedMessage += new NetComm.DelegateMessage(Instance_ReceivedMessage);
//            NetComm.Instance.SendCommandHandler += new NetComm.SendCommand(Instance_SendCommandHandler);

            Trace.ShowMessage(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                "Form1 Constructed",
                Trace.MessageLevel.Verbose);

//            _netMaster.Open();

            Load += new EventHandler(Form1_Load);
            FormClosed += new FormClosedEventHandler(Form1_FormClosed);
        }

        void Form1_Load(object sender, EventArgs e)
        {
            NetSettings settings = NetSettings.Load();

//            textBox1.Text = settings.ipAddress;
//            textBox2.Text = settings.port;

            textBox3.Text = settings.multicastAddress;
            textBox4.Text = settings.multicastPort;

            this.MaximizeBox = false;

            SetToolTips();

            listBox2.MouseDown += new MouseEventHandler(listBox2_MouseDown);
        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
//            NetComm.Instance.Close();
            _netMaster.Close();
        }

        private void SetToolTips()
        {
            ToolTip toolTipStatus = new ToolTip();
            toolTipStatus.SetToolTip(btnStatus, "Inquiry status command");

            ToolTip toolTipSTB = new ToolTip();
            toolTipSTB.SetToolTip(btnSTB, "Set STB control type");

            ToolTip toolTipMCU = new ToolTip();
            toolTipMCU.SetToolTip(btnMCU, "Get MCU SW version");

            ToolTip toolTipIR = new ToolTip();
            toolTipIR.SetToolTip(btnIRProtocol, "Set IR protocol setting");

            ToolTip toolTipIRRaw = new ToolTip();
            toolTipIRRaw.SetToolTip(btnIRPattern, "Send IR raw pattern");

            ToolTip toolTipTV = new ToolTip();
            toolTipTV.SetToolTip(btnTV, "Set TV command");
        }

        private void Instance_ReceivedMessage(string msg)
        {
            BeginInvoke(new Server.DelegateMessage(SAFE_Instance_ReceivedMessage), msg);
        }

        private void SAFE_Instance_ReceivedMessage(string msg)
        {
            listBox1.Items.Add(msg);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        void Instance_SendCommandHandler(NetPacket packet)
        {
            BeginInvoke(new NetComm.SendCommand(SAFE_Instance_SendCommandHandler), packet);
        }

        private void SAFE_Instance_SendCommandHandler(NetPacket packet)
        { 
        
        }

        void _netMaster_ReceivedMessage(string msg)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new NetMaster.DelegateMessage(_netMaster_ReceivedMessage), msg);
                return;
            }

            listBox1.Items.Add(msg);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        private void SAFE_netMaster_ReceivedMessage(string msg)
        {
            listBox1.Items.Add(msg);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        private void _netMaster_UpdateDevice(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler(_netMaster_UpdateDevice), e);
                return;
            }

            listBox2.Items.Clear();

            if (_netMaster.Devices.Count > 0)
            { 
                foreach (DeviceData device in _netMaster.Devices)
                {
                    listBox2.Items.Add(device.Name);
                }
            }
        }

        private void _netMaster_Connected(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler(_netMaster_Connected), e);
                return;
            }

            _isConnected = _netMaster.IsOpen;

            string btnName = string.Empty;
            if (_isConnected)
            {
                btnName = "Disconnect";
            }
            else
            {
                btnName = "Connect";
            }
            btnConnect.Text = btnName;
        }

        private void btnStatus_Click(object sender, EventArgs e)
        {
            if (_isConnected)
                _netMaster.GetStatus();
        }
        
        private void btnSTB_Click(object sender, EventArgs e)
        {
            if (_isConnected)
                _netMaster.SetSTBControlType(0x1);
        }

        private void btnMCU_Click(object sender, EventArgs e)
        {
            if (_isConnected)
                _netMaster.GetMCUSWVersion();
        }

        private void btnIRProtocol_Click(object sender, EventArgs e)
        {
            if (_isConnected)
                _netMaster.SetIRProtocol(new byte[4] { 0xF1, 0xF2, 0x6E, 0x00 });
        }

        private void btnIRPattern_Click(object sender, EventArgs e)
        {
            if (_isConnected)
                _netMaster.SendIRRawPattern();
        }

        private void btnTV_Click(object sender, EventArgs e)
        {
            if (_isConnected)
                _netMaster.SetTVCommand();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (IsConnecting)
                return;

            string server = textBox1.Text;
            int port = Int32.Parse(textBox2.Text);

            if (_netMaster != null)
            {
                if (_isConnected)
                    _netMaster.Close();
                else
                    _netMaster.Open(server, port);
            }
        }

        private void btnBroadcast_Click(object sender, EventArgs e)
        {
            if (IsBroadcasting)
                return;

            string server = textBox3.Text;
            int port = Int32.Parse(textBox4.Text);

            if (_netMaster != null)
                _netMaster.StartBroadcast(server, port);
        }

        private void listBox2_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (DeviceData device in _netMaster.Devices)
            {
                if (string.Compare(listBox2.Items[listBox2.SelectedIndex].ToString(), device.Name, true) == 0)
                {
                    textBox1.Text = device.Address;
                    textBox2.Text = device.Port.ToString();
                    break;
                }
            }
        }

        private bool IsConnecting
        {
            get
            {
                return _netMaster.IsConnecting;
            }
        }

        private bool IsBroadcasting
        {
            get 
            {
                return _netMaster.IsBroadcasting; 
            }
        }
    }
}
