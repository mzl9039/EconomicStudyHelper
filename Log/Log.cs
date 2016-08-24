/************************************************************************/
/* Description: Log class 扩展log4net，记录日志的类，取自标农
 *              里的工程LogHelper
 * Author:      mzl  
 * Date:        2015.9.15
 * Version:     1.0.0
/************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Log
{
    public class Log
    {
        // 日志记录
        private static readonly ILog log = LogManager.GetLogger("logerror");
        private static readonly ILog info = LogManager.GetLogger("loginfo");

        static Log()
        {
            // 初始化日志记录
            // 此日志配置文件嵌入项目中不可修改
            const string log4NetConfig = "log4net.xml";
            try
            {
                // alter by mzl 2015.9.21 在当前目录下搜索文件log4net_DataSubmit.xml，若找到则
                string[] files = Directory.GetFiles(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, log4NetConfig, SearchOption.AllDirectories);
                if (files.Length != 0)
                    log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(files[0]));
            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }

        /// <summary>
        /// 动态获取调用日志方法的类的方法名，
        /// 即若classA的方法methodA调用了日志，
        /// 则返回 classA.methodA: 
        /// </summary>
        /// <returns></returns>
        public static string GetClassAndMethodName(Exception ex)
        {
            StackTrace trace = new StackTrace();
            StackFrame sf = trace.GetFrame(2);
            string className = sf.GetMethod().ReflectedType.Name;
            string methodName = sf.GetMethod().Name;
            string funcName = ex.TargetSite.Name;
            string stack = ex.StackTrace;
            string result = "异常所在函数：" + className + "." + methodName + "\r\n" + "引发当前异常的方法：" + funcName +
            "\r\n" + "异常消息：" + ex.Message + "\r\n" + "异常堆栈内容：\r\n" + stack + "\r\n";
            return result;
        }

        public static string GetClassAndMethodName()
        {
            StackTrace trace = new StackTrace();
            string methodName = trace.GetFrame(2).GetMethod().Name;
            string className = trace.GetFrame(2).GetMethod().ReflectedType.Name;
            return className + "." + methodName + ":";
        }

        #region ILog Members

        public static void Debug(Exception ex)
        {
            log.Debug(GetClassAndMethodName(ex) + ex.Message);
        }

        public static void Debug(object message)
        {
            log.Debug(GetClassAndMethodName() + message);
        }

        public static void Debug(object message, Exception exception)
        {
            log.Debug(GetClassAndMethodName() + message, exception);
        }

        public static void DebugFormat(string format, params object[] args)
        {
            log.DebugFormat(format, args);
        }

        public static void DebugFormat(string format, object arg0)
        {
            log.DebugFormat(format, arg0);
        }

        public static void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            log.DebugFormat(provider, format, args);
        }

        public static void DebugFormat(string format, object arg0, object arg1)
        {
            log.DebugFormat(format, arg0, arg1);
        }

        public static void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            log.DebugFormat(format, arg0, arg1, arg2);
        }

        public static void Error(Exception ex)
        {
            log.Error(GetClassAndMethodName(ex));
        }

        public static void Error(object message)
        {
            log.Error(GetClassAndMethodName() + message);
        }

        public static void Error(object message, Exception exception)
        {
            log.Error(GetClassAndMethodName() + message, exception);
        }

        public static void ErrorFormat(string format, object arg0)
        {
            log.ErrorFormat(format, arg0);
        }

        public static void ErrorFormat(string format, params object[] args) 
        {
            log.ErrorFormat(format, args);
        }

        public static void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            log.ErrorFormat(provider, format, args);
        }

        public static void ErrorFormat(string format, object arg0, object arg1)
        {
            log.ErrorFormat(format, arg0, arg1);
        }

        public static void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            log.ErrorFormat(format, arg0, arg1, arg2);
        }

        public static void Fatal(Exception ex)
        {
            log.Fatal(GetClassAndMethodName(ex));
        }

        public static void Fatal(object message)
        {
            log.Fatal(GetClassAndMethodName() + message);
        }

        public static void Fatal(object message, Exception exception)
        {
            log.Fatal(GetClassAndMethodName() + message, exception);
        }

        public static void FatalFormat(string format, object arg0)
        {
            log.FatalFormat(format, arg0);
        }

        public static void FatalFormat(string format, params object[] args)
        {
            log.FatalFormat(format, args);
        }

        public static void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            log.FatalFormat(provider, format, args);
        }

        public static void FatalFormat(string format, object arg0, object arg1)
        {
            log.FatalFormat(format, arg0, arg1);
        }

        public static void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            log.FatalFormat(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// 记录操作信息
        /// </summary>
        /// <param name="message"></param>
        public static void Info(object message)
        {
            info.Info(message);
        }

        /// <summary>
        /// 记录Info级别的错误信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Info(object message, Exception exception)
        {
            log.Info(GetClassAndMethodName() + message, exception);
        }

        public static void InfoFormat(string format, object arg0)
        {
            info.InfoFormat(format, arg0);
        }

        public static void InfoFormat(string format, params object[] args)
        {
            try 
            {
                info.InfoFormat(format, args);
            }
            catch(Exception ex)
            {
                throw ex; 
            }            
        }

        public static void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            info.InfoFormat(provider, format, args);
        }

        public static void InfoFormat(string format, object arg0, object arg1)
        {
            info.InfoFormat(format, arg0, arg1);
        }

        public static void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            info.InfoFormat(format, arg0, arg1, arg2);
        }

        public static void Warn(Exception ex)
        {
            log.Warn(GetClassAndMethodName(ex));
        }

        public static void Warn(object message)
        {
            log.Warn(GetClassAndMethodName() + message);
        }

        public static void Warn(object message, Exception exception)
        {
            log.Warn(GetClassAndMethodName() + message, exception);
        }

        public static void WarnFormat(string format, params object[] args)
        {
            log.WarnFormat(format, args);
        }

        public static void WarnFormat(string format, object arg0)
        {
            log.WarnFormat(format, arg0);
        }

        public static void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            log.WarnFormat(provider, format, args);
        }

        public static void WarnFormat(string format, object arg0, object arg1)
        {
            log.WarnFormat(format, arg0, arg1);
        }

        public static void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            log.WarnFormat(format, arg0, arg1, arg2);
        }

        public static bool IsDebugEnabled { get { return log.IsDebugEnabled; } }
        public static bool IsErrorEnabled { get { return log.IsErrorEnabled; } }
        public static bool IsFatalEnabled { get { return log.IsFatalEnabled; } }
        public static bool IsInfoEnabled { get { return log.IsInfoEnabled; } }
        public static bool IsWarnEnabled { get { return log.IsWarnEnabled; } }

        #endregion
    }
}
