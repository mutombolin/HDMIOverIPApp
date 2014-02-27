using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDMIOverIPApp.communication.net
{
    public class DeviceData
    {
        private string _name;
        private string _address;
        private int _port;
        private string _version;
        private string _tcpVersion;

        #region Enum
        private enum ParseState
        {
            Name,
            Address,
            Port,
            Version,
            TCPVersion
        }
        #endregion

        public DeviceData()
        {
            _name = string.Empty;
            _address = string.Empty;
            _port = 0;
            _version = string.Empty;
            _tcpVersion = string.Empty;
        }

        public byte[] Data
        {
            set
            {
                if (value == null)
                    return;

                ParseState parseState = ParseState.Name;

                List<byte> data = new List<byte>();
                bool finished = false;

                for (int i = 0; i < value.Length; i++)
                {
                    if (finished)
                        break;

                    if (value[i] != ',')
                        data.Add(value[i]);

                    switch (parseState)
                    { 
                        case ParseState.Name:
                            if (value[i] == ',')
                            {
                                _name = Encoding.ASCII.GetString(data.ToArray());
                                parseState = ParseState.Address;
                                data.Clear();
                            }
                            break;
                        case ParseState.Address:
                            if (value[i] == ',')
                            {
                                _address = Encoding.ASCII.GetString(data.ToArray());
                                parseState = ParseState.Port;
                                data.Clear();
                            }
                            break;
                        case ParseState.Port:
                            if (value[i] == ',')
                            {
                                _port = Int32.Parse(Encoding.ASCII.GetString(data.ToArray()));
                                parseState = ParseState.Version;
                                data.Clear();
                            }
                            break;
                        case ParseState.Version:
                            if (value[i] == ',')
                            {
                                _version = Encoding.ASCII.GetString(data.ToArray());
                                parseState = ParseState.TCPVersion;
                                data.Clear();
                            }
                            break;
                        case ParseState.TCPVersion:
                            if (value[i] == ',')
                            {
                                _tcpVersion = Encoding.ASCII.GetString(data.ToArray());
                                finished = true;
                                data.Clear();
                            }
                            break;
                    }
                }
            }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Address
        {
            get { return _address; }
        }

        public int Port
        {
            get { return _port; }
        }

        public string Version
        {
            get { return _version; }
        }

        public string TCPVersion
        {
            get { return _tcpVersion; }
        }
    }
}
