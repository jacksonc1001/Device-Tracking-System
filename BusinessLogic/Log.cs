using System;
using System.IO;
using System.Configuration;
using System.Xml.Serialization;
using System.Xml;

/*
* Author: Jackson
* Date: 21/05/2021
* Version: 1.0.0.0
* Objective: Maintian all the write log method
*/
namespace Device_Tracking_System.BusinessLogic
{
    class Log
    {
        public static void WriteToMESLogFile(object obj, bool logFileType, string actionName)
        {
            try
            {
                XmlSerializer x = new XmlSerializer(obj.GetType());
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                var tw = new StringWriter();
                var xmlWriter = XmlWriter.Create(tw, new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = true });
                x.Serialize(xmlWriter, obj, ns);

                string LocationPath = ConfigurationManager.AppSettings["LogFilePath"] + @"\MESLog";
                //--> Write to log file-----------------------------------------------------*
                string FileName = LocationPath + @"\MESLog_" + DateTime.Now.ToString("ddMMyyyy") + ".txt";

                string type;

                if (logFileType == true)
                {
                    type = "->";
                }
                else
                {
                    type = "<-";
                }
                //--> Check and create directory--------------------------------------------*
                if (!Directory.Exists(LocationPath))
                {
                    Directory.CreateDirectory(LocationPath);
                }

                var time = DateTime.Now;
                string Time = time.ToString("MM/dd/yyyy HH:mm:ss.fff");
                string MessageText = "\r\n" + Time + " " + actionName + " " + type + "\r\n" + tw.ToString();

                //--> Save to logFile-------------------------------------------------------*
                using (FileStream fs = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(MessageText);
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
 
        }
        public static void WriteToDTSLogFile(object obj, bool logFileType, string methodName)
        {
            try
            {
                XmlSerializer x = new XmlSerializer(obj.GetType());
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                var tw = new StringWriter();
                var xmlWriter = XmlWriter.Create(tw, new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = true });
                x.Serialize(xmlWriter, obj, ns);

                string LocationPath = ConfigurationManager.AppSettings["LogFilePath"] + @"\DISLog";
                //--> Write to log file-----------------------------------------------------*
                string FileName = LocationPath + @"\DISLog_" + DateTime.Now.ToString("ddMMyyyy") + ".txt";

                string type;

                if (logFileType == true)
                {
                    type = "->";
                }
                else
                {
                    type = "<-";
                }
                //--> Check and create directory--------------------------------------------*
                if (!Directory.Exists(LocationPath))
                {
                    Directory.CreateDirectory(LocationPath);
                }

                var time = DateTime.Now;
                string Time = time.ToString("MM/dd/yyyy HH:mm:ss.fff");
                string MessageText = "\r\n" + Time + " " + methodName + " " + type + "\r\n" + tw.ToString();

                //--> Save to logFile-------------------------------------------------------*
                using (FileStream fs = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(MessageText);
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public static void WriteToErrorLogFile(string text)
        {
            try
            {
                string LocationPath = ConfigurationManager.AppSettings["LogFilePath"] + @"\ErrorLog";
                //--> Write to log file-----------------------------------------------------*
                string FileName = LocationPath + @"\ErrorLog_" + DateTime.Now.ToString("ddMMyyyy") + ".txt";

                //--> Check and create directory--------------------------------------------*
                if (!Directory.Exists(LocationPath))
                {
                    Directory.CreateDirectory(LocationPath);
                }

                var time = DateTime.Now;
                string Time = time.ToString("MM/dd/yyyy HH:mm:ss.fff");
                string MessageText = "\r\n" + Time + "\r\n" + text;

                //--> Save to logFile-------------------------------------------------------*
                using (FileStream fs = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(MessageText);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
