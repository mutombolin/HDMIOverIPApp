using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDMIOverIPApp.communication.net
{
    public enum NetPacketParseResult
    { 
        Success,
        ErrorPacketLength,
        ErrorChecksum,
        ErrorStartFlag,
        ErrorStopFlag,
        ErrorUnknownCommand,
        ErrorEmptyFrame,
    }

    public enum NetCommand : byte
    { 
        Status = 0x1,
        SetSTBControlType,
        GetMCUVersion,
        SetIR = 0x10,
        SendIRRawPattern = 0x11,
        SetTVCommand = 0x20,
    }
}
