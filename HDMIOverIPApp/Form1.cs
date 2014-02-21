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
        private Server _server;

        public Form1()
        {
            InitializeComponent();

            _server = new Server();
            _server.Open();
            _server.ReceivedMessage += new Server.DelegateMessage(_server_ReceivedMessage);

            Trace.ShowMessage(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                "Form1 Constructed",
                Trace.MessageLevel.Verbose);

            Load += new EventHandler(Form1_Load);
            FormClosed += new FormClosedEventHandler(Form1_FormClosed);
        }

        void Form1_Load(object sender, EventArgs e)
        {
            NetSettings settings = NetSettings.Load();

            textBox1.Text = settings.ipAddress;
            textBox2.Text = settings.port;

            SetToolTips();
        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _server.Close();
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

        private void btnStatus_Click(object sender, EventArgs e)
        {
            Client client = new Client();
            client.SendMessage += new Client.DelegateMessage(_server_ReceivedMessage);
            client.Connect("192.168.100.10", 8000, "Status");
        }

        private void _server_ReceivedMessage(string msg)
        {
            BeginInvoke(new Server.DelegateMessage(SAFE_server_ReceivedMessage), msg);
        }

        private void SAFE_server_ReceivedMessage(string msg)
        {
            listBox1.Items.Add(msg);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }
    }
}
