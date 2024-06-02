/*
 * By: Fox.Huang 黄文叶, Date:2018.12.18
 */
using UnityEngine;
using System.IO;
using System.Text;

namespace KittyHelpYouOut
{
    public class DebugLogOutput
    {
        static bool s_HasInit = false;
        static string s_LogPath = null;

        static int s_LastCount = 0; // 这个变量相关的逻辑,是用来做重新信息处理的.因为有时候错误会噼里啪啦的跳,减缓一点IO压力.
        static string s_LastLog = "";

        public static void InitLogFile(string logPath)
        {
            if (!string.IsNullOrEmpty(logPath))
            {
                s_HasInit = true;
                s_LogPath = logPath;
            }
        }

        public static void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (s_HasInit)
            {
                string logContent = "[" + type.ToString() + "]" + logString;

                if (s_LastCount < 10 || s_LastLog != logContent)
                {
                    if (s_LastLog == logContent)
                    {
                        ++s_LastCount;
                    }
                    else
                    {
                        s_LastCount = 1;
                        s_LastLog = logContent;
                    }
                    using (StreamWriter sw = new StreamWriter(s_LogPath, true, Encoding.GetEncoding("UTF-8")))
                    {
                        sw.WriteLine(logContent);
                        sw.Close();
                    }
                }
            }
        }
    }
}