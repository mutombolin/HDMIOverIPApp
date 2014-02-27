using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDMIOverIPApp.communication.net
{
    #region NetCommandData
    public class NetCommandData
    {
        #region Fields
        private byte _command;
        private List<byte> _data;
        #endregion

        #region Constructor
        public NetCommandData()
        {
            _command = 0;
            _data = new List<byte>();
        }
        #endregion

        #region Properties
        public byte Command
        {
            get { return _command; }
            set { _command = value; }
        }

        public byte[] Data
        {
            get
            {
                if (_data.Count == 0)
                    return new byte[0];

                return _data.ToArray();
            }
            set
            {
                _data.Clear();

                if ((value == null) || (value.Length == 0))
                    return;

                _data.AddRange(value);
            }
        }
        #endregion

        #region Clone
        public NetCommandData Clone()
        {
            NetCommandData result = new NetCommandData();

            result.Command = Command;
            result.Data = Data;

            return result;
        }
        #endregion
    }
    #endregion

    #region NetPacket
    public class NetPacket : List<NetCommandData>
    {
        #region Fields
        private const byte NetStartFlag = 0xF1;
        private const byte NetStopFlag = 0xF2;
        private byte _status;
        private NetPacketParseResult _parseResult;
        #endregion

        #region Enum
        private enum ParseState
        {
            command,
            length,
            data
        }
        #endregion

        #region Construction
        public NetPacket()
        {
            _status = 255;
            _parseResult = NetPacketParseResult.Success;
        }
        #endregion

        #region Properties
        public NetPacketParseResult NetPacketParseResult
        {
            get { return _parseResult; }
        }

        public byte Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public byte[] RawData
        {
            get
            {
                List<byte> packetData = new List<byte>();
                byte checksum = 0;

                // Start flag
                packetData.Add(NetStartFlag);

                foreach (NetCommandData ncd in this)
                {
                    if (ncd.Data.Length > 0)
                    {
                        // Add Command
                        packetData.Add(ncd.Command);
                        checksum ^= ncd.Command;

                        // Add length
                        byte[] length = BitConverter.GetBytes((Int16)ncd.Data.Length);

                        foreach (byte b in length.Reverse())
                        {
                            packetData.AddRange(ByteStuffing((byte)b));
                            checksum ^= b;
                        }

                        // Add data
                        foreach (byte b in ncd.Data)
                        {
                            packetData.AddRange(ByteStuffing(b));
                            checksum ^= b;
                        }
                    }
                    else
                    {
                        packetData.Add(ncd.Command);
                        checksum ^= ncd.Command;
                    }
                }

                // Add checksum
                packetData.AddRange(ByteStuffing(checksum));

                // Stop flag
                packetData.Add(NetStopFlag);

                return packetData.ToArray();
            }
            set
            {
                _parseResult = net.NetPacketParseResult.Success;

                // Clear the data
                Clear();

                // Error check
                if (value == null)
                    return;

                if (value.Length < 3)
                {
                    _parseResult = net.NetPacketParseResult.ErrorPacketLength;
                    return;
                }

                // Flag check
                if (value[0] != NetStartFlag)
                {
                    _parseResult = net.NetPacketParseResult.ErrorStartFlag;
                    return;
                }
                else if (value[value.Length - 1] != NetStopFlag)
                {
                    _parseResult = net.NetPacketParseResult.ErrorStopFlag;
                    return;
                }

                List<byte> packetData = new List<byte>();
                int index = 1;
                byte checksum = 0;

                // Extract the bytes by removing any byte stuffing
                for (int i = 1; (i < value.Length - 1) && (index < value.Length - 1); i++)
                    packetData.Add(ByteExtract(value, ref index));

                // Verify checksum
                for (int i = 0; i < packetData.Count - 1; i++)
                    checksum ^= packetData[i];

                if (checksum != packetData[packetData.Count - 1])
                {
                    _parseResult = net.NetPacketParseResult.ErrorChecksum;
                    return;
                }

                ParseState parseState = ParseState.command;
                byte command = 0;
                byte length = 0;
                List<byte> byteLength = new List<byte>();
                List<byte> data = new List<byte>();
                bool commandDataFound;

                for (int i = 0; i < packetData.Count - 1; i++)
                {
                    commandDataFound = false;

                    switch (parseState)
                    { 
                        case ParseState.command:
                            command = packetData[i];
                            parseState = ParseState.length;
                            break;
                        case ParseState.length:
                            byteLength.Add(packetData[i]);
                            if (byteLength.Count > 1)
                            {
                                length = (byte)BitConverter.ToInt16(byteLength.ToArray(), 0);
                                parseState = ParseState.data;
                            }
                            break;
                        case ParseState.data:
                            data.Add(packetData[i]);
                            if (data.Count == length)
                            {
                                parseState = ParseState.command;
                                commandDataFound = true;
                            }
                            break;
                    }

                    if (commandDataFound)
                    {
                        NetCommandData commandData = new NetCommandData();
                        commandData.Command = command;
                        if (data.Count > 0)
                            commandData.Data = data.ToArray();

                        length = 0;
                        command = 0;
                        data.Clear();

                        Add(commandData);
                    }
                }
            }
        }
        #endregion

        #region Helpers
        private byte[] ByteStuffing(byte data)
        {
            List<byte> result = new List<byte>();

            if ((data >= 0xF0) && (data < 0xF3))
            {
                result.Add((byte)0xF0);
                result.Add((byte)(data - 0xF0));
            }
            else
            {
                result.Add(data);
            }

            return result.ToArray();
        }

        private byte ByteExtract(byte[] packetData, ref int index)
        {
            byte result;

            if (packetData[index] == 0xF0)
            {
                index++;
                result = (byte)(packetData[index] + (byte)0xF0);
                index++;
            }
            else
            {
                result = packetData[index];
                index++;
            }

            return result;
        }
        #endregion

        #region Reset
        public void Reset()
        {
            Clear();

            _status = 255;
        }
        #endregion

        #region Clone
        public NetPacket Clone()
        {
            NetPacket result = new NetPacket();

            result.Status = Status;

            foreach (NetCommandData ncd in this)
                result.Add(ncd.Clone());

            return result;
        }
        #endregion
    }
    #endregion
}
