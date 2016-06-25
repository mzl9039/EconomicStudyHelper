using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace LogHelper
{
    public class Log
    {
        private static object obj = new object();

        /// <summary>
        /// 操作日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public static void WriteLog(string content)
        {
            WriteLogs(GetClassAndMethodName(), content, "操作日志");
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public static void WriteError(string content)
        {
            WriteLogs(GetClassAndMethodName(), content, "错误日志");
        }

        public static void WriteLogs(string title, string content, string type)
        {
            lock (obj)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                if (!string.IsNullOrEmpty(path))
                {
                    path = AppDomain.CurrentDomain.BaseDirectory + "log";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    path = path + "\\" + DateTime.Now.ToString("yyMM");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    path += "\\" + DateTime.Now.ToString("dd") + ".txt";
                    if (!File.Exists(path))
                    {
                        FileStream fs = File.Create(path);
                        fs.Close();
                    }
                    if (File.Exists(path))
                    {
                        StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default);
                        sw.WriteLine(DateTime.Now + " " + title);
                        sw.WriteLine("日志类型：" + type);
                        sw.WriteLine("详情：" + content);
                        sw.WriteLine("------------------------------------------------------------");
                        sw.Close();
                    }
                }
            }
        }

        public static string GetClassAndMethodName()
        {
            StackTrace trace = new StackTrace();
            MethodBase methodName = trace.GetFrame(2).GetMethod();
            string className = methodName.ReflectedType.ToString();
            return className + "." + methodName + " ";
        }
    }
}
