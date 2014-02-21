using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

using HDMIOverIPApp.diagnostics.trace;

namespace HDMIOverIPApp.communication.net
{
    [XmlRoot("settings")]
    public class NetSettings
    {
        [XmlIgnore()]
        private const string FileName = "NetSettings.xml";

        [XmlElementAttribute("ipaddress")]
        public string ipAddress = "192.168.100.10";

        [XmlElementAttribute("port")]
        public string port = "8000";

        [XmlElementAttribute("transmit_retries")]
        public int TransmitRetries = 3;

        [XmlElementAttribute("transmit_timeout_ms")]
        public int TransmitTimeoutMs = 500;

        [XmlElementAttribute("receive_timeout_ms")]
        public int ReceiveTimeoutMs = 1000;

        public static NetSettings Load()
        {
            NetSettings result = new NetSettings();

            FileStream fs = null;

            try
            {
                string filePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), FileName);
                fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                XmlSerializer ser = new XmlSerializer(typeof(NetSettings));
                result = (NetSettings)ser.Deserialize(fs);
                fs.Close();
            }
            catch (Exception ex)
            {
                Trace.ShowException(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                    ex,
                    "Failed to load net settings file!");
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }

            return result;
        }
    }
}
