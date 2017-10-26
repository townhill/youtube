using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace youtube.Models
{
    using System.IO;
    using System.Reflection;


    public class LogWriter
    {
        private string m_exePath = string.Empty;
        public LogWriter(string logMessage)
        {
            if (!string.IsNullOrEmpty(logMessage))
                LogWrite(logMessage + "  returns");
            else
                LogWrite("Arrived");
        }
        public void LogWrite(string logMessage)
        {
            m_exePath = @"c:\logs";
            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "log.txt"))
                {
                    Log(logMessage, w);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
              //  txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
            //    txtWriter.WriteLine("  :");
            //    txtWriter.WriteLine("  :{0}", logMessage);
            ///    txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception ex)
            {
            }
        }
    }
}