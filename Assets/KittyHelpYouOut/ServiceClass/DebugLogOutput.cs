using UnityEngine;
using System.IO;
using System.Text;

namespace KittyHelpYouOut
{
    public class DebugLogOutput
    {
        static bool hasInit = false;
        static string logPath = null;

        static int lastCount = 0;
        static string lastLog = "";

        public static void Init(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                hasInit = true;
                logPath = path;
            }
        }

        public static void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (hasInit)
            {
                string logContent = "[" + type.ToString() + "]" + logString;

                if (lastCount < 10 || lastLog != logContent)
                {
                    if (lastLog == logContent)
                    {
                        ++lastCount;
                    }
                    else
                    {
                        lastCount = 1;
                        lastLog = logContent;
                    }
                    using (StreamWriter sw = new StreamWriter(logPath, true, Encoding.GetEncoding("UTF-8")))
                    {
                        sw.WriteLine(logContent);
                        sw.Close();
                    }
                }
            }
        }
    }
}