using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace HDMIOverIPApp.diagnostics.trace
{
    class LimitedFileTraceListener : TraceListener
    {
        private FileStream _file;
        private object _objectLock;
        private const long MaxFileSize = 1000000;
        private string _fileName;

        public LimitedFileTraceListener()
            : this(string.Empty)
        { 
        
        }

        public LimitedFileTraceListener(string initializeData)
        {
            _file = null;
            _objectLock = new object();
            _fileName = initializeData;
        }

        private FileStream File
        {
            get
            {
                if (_file == null)
                    Open(true);

                return _file;
            }
            set
            {
                _file = value;
            }
        }

        private void Open(bool append)
        {
            lock (_objectLock)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_fileName));
                }
                catch { }

                try
                {
                    File = new FileStream(_fileName, append ? FileMode.OpenOrCreate : FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

                    File.Seek(_file.Length, SeekOrigin.Begin);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to open file for writing diagnostics!" + ex.Message);
                }
            }
        }

        private void MonitorSize()
        {
            if (File == null)
                return;

            if (File.Length > MaxFileSize)
            {
                Close();
                Open(false);
            }
        }

        public override void Write(string message)
        {
            if (File == null)
                return;

            lock (_objectLock)
            {
                try
                {
                    byte[] data = ASCIIEncoding.ASCII.GetBytes(message);
                    File.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to write diagnostics file!" + ex.Message);
                }

                MonitorSize();
            }
        }

        public override void WriteLine(string message)
        {
            Write(message + "\r\n");
        }

        public override void Flush()
        {
            if (File == null)
                return;

            lock (_objectLock)
            {
                try
                {
                    File.Flush();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to flush diagnostics file!" + ex.Message);
                }
            }
        }

        public override void Close()
        {
            if (File == null)
                return;

            lock (_objectLock)
            {
                try
                {
                    File.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to close diagnostics file!" + ex.Message);
                }

                File = null;
            }
        }

        public override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }
    }
}
