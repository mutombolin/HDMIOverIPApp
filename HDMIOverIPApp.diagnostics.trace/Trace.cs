using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace HDMIOverIPApp.diagnostics.trace
{
    public class Trace
    {
        private static List<TraceSwitch> _switchList = new List<TraceSwitch>();
        private static object _lockObject = new object();

        public enum MessageLevel
        { 
            Error,
            Warning,
            Info,
            Verbose
        }

        public static void ShowMessage(string switchName, string message, MessageLevel level)
        {
            string newMessage = "[" + DateTime.Now.ToShortDateString() + " " +
                DateTime.Now.ToLongTimeString() + "]" + message;

            Publish(switchName, newMessage, level);
        }

        public static void ShowException(string switchName, Exception ex, string message)
        {
            StringBuilder exceptionInfo = new StringBuilder();

            exceptionInfo.Append("[" + DateTime.Now.ToShortDateString() + " " +
                DateTime.Now.ToLongTimeString() + "]" + message);

            exceptionInfo.Append(BuildExceptionString(ex));

            Publish(switchName, exceptionInfo.ToString(), MessageLevel.Error);
        }

        private static string BuildExceptionString(Exception ex)
        {
            StringBuilder exceptionInfo = new StringBuilder();
            int exceptionCount = 1;

            while (ex != null)
            {
                exceptionInfo.Append(
                    string.Format("\r\nException {0}:\r\nMessage: {1}\r\nSource: {2}\r\nStackTrace: {3}\r\n",
                        exceptionCount.ToString(),
                        ex.Message,
                        ex.Source,
                        ex.StackTrace)
                        );

                ex = ex.InnerException;
                exceptionCount++;
            }

            return exceptionInfo.ToString();
        }

        private static void Publish(string switchName, string message, MessageLevel level)
        {
            lock (_lockObject)
            {
                TraceSwitch traceSwitch = null;

                foreach (TraceSwitch ts in _switchList)
                {
                    if (ts.DisplayName.ToLower() == switchName.ToLower())
                        traceSwitch = ts;
                }

                if (traceSwitch == null)
                    traceSwitch = new TraceSwitch(switchName, "default description...");

                if (level == MessageLevel.Verbose && traceSwitch.TraceVerbose)
                    System.Diagnostics.Trace.WriteLine(message);
                else if (level == MessageLevel.Info && traceSwitch.TraceInfo)
                    System.Diagnostics.Trace.TraceInformation(message);
                else if (level == MessageLevel.Warning && traceSwitch.TraceWarning)
                    System.Diagnostics.Trace.TraceWarning(message);
                else if (level == MessageLevel.Error && traceSwitch.TraceError)
                    System.Diagnostics.Trace.TraceError(message);

                System.Diagnostics.Trace.Flush();
            }
        }
    }
}
