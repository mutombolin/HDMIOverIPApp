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
}
