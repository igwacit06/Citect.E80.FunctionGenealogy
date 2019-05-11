using System;
using System.Diagnostics;
using System.IO;
using log4net;

// ReSharper disable StringLiteralTypo
// ReSharper disable All
#pragma warning disable 1591

namespace Citect.E80.FunctionGenealogy
{
	/// <summary>
	/// class to log output in csv format
	/// </summary>
	public class LogCsv
    {
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// default csv file path value. path can be changed by instantiating
		/// class with another file path
		/// </summary>
		public string Csvfilepath = "C:\\QR_PSCS\\Logs\\LinesOut.csv";
        private StreamWriter StreamWriter;
        private readonly bool Opened;
		private readonly bool Log;
       
        /// <summary>
        /// constructor
        /// provide filepath or use default filepath (enablelog is false by default)
        /// </summary>
        /// <param name="filepath"></param>
		/// <param name="enableLog"></param>
        public LogCsv(string filepath, bool enableLog = false)
        {
            if (!string.IsNullOrEmpty(filepath))
                Csvfilepath = filepath;

            if(File.Exists(Csvfilepath))
                File.Delete(Csvfilepath);   //remove previous

            try
            {
                StreamWriter = File.AppendText(Csvfilepath);
                StreamWriter.AutoFlush = true;				
                Opened = true;
				Log = enableLog;
            }
            catch (Exception ex)
            {                
                log.Error("error:", ex);
            }
        }

        /// <summary>
        /// logs message to csv via TraceListener
        /// </summary>
        /// <param name="logCsvfilepath"></param>
        /// <param name="message"></param>
        public LogCsv(string logCsvfilepath, string message)
        {
            var listener = new DelimitedListTraceListener(logCsvfilepath);
            Debug.Listeners.Add(listener);
            Debug.WriteLine(message);
            Debug.Flush();
        }

		/// <summary>
		/// write string value to output file
		/// </summary>
		/// <param name="value"></param>
		/// <param name="args"></param>
		public void WriteToFile(string value, params object[] args)
		{
			if (Opened && Log) StreamWriter.WriteLine(value, args);
		}

        /// <summary>
        /// close the file writer
        /// </summary>
        public void CloseFile() {
			if (Opened)
				StreamWriter.Close();
        }
    }
}
