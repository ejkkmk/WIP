using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;

namespace GeometricLayouts.Controllers
{
    /* This class uses the Singleton pattern to faciliate event logging to a file */
    public sealed class FileLogger
    {
        private const string _LogFileSetting = "LogFile";              // Key name for the log file path and name setting in web.config

        private static FileLogger instance = null;
        private static readonly object threadlock = new object();
        private static StreamWriter _LogFile;

        FileLogger()
        {
            try
            {
                // Retieve the log file path and name from the web.config file
                string LogFilePathName = ConfigurationManager.AppSettings[_LogFileSetting];

                // Confirm the log file path exists 
                if (Directory.Exists(Path.GetDirectoryName(LogFilePathName)))
                  _LogFile = new StreamWriter(LogFilePathName, true);
            }

            catch { }
        }

        public static FileLogger Instance
        {
            get
            {
                if (instance == null)
                {
                    // set up a lock to prevent multithreaded issues
                    lock (threadlock)
                    {
                        if (instance == null)
                        {
                            instance = new FileLogger();
                        }

                    }
                }

                return instance;
            }
        }

        /* Write the given text to the log file.  The text is prepended with the current date-time. */
        public void WriteLog(string text)
        {
            if (_LogFile != null)
            {
                _LogFile.WriteLine(String.Format("{0}:  {1}", System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), text));
                _LogFile.Flush();
            }
        }

    }
}