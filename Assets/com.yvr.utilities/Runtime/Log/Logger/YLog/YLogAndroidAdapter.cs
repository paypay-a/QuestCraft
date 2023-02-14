using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.XR;

namespace YVR.Utilities
{
    [ExcludeFromDocs]
    public class YLogAndroidAdapter : IYLogAdapter
    {

        [DllImport("yvrutilities")]
        private static extern void initializeRamLog(string tag, int ramLogSize);

        [DllImport("yvrutilities")]
        private static extern LogPriority getLogcatPriorityTHold();

        [DllImport("yvrutilities")]
        private static extern void setLogcatPriorityTHold(LogPriority priority);

        [DllImport("yvrutilities")]
        private static extern LogPriority getRamLogPriorityTHold();

        [DllImport("yvrutilities")]
        private static extern void setRamLogPriorityTHold(LogPriority priority);

        [DllImport("yvrutilities")]
        private static extern void saveLog();

        [DllImport("yvrutilities")]
        private static extern void debug(string logMsg, string prefix = null);

        [DllImport("yvrutilities")]
        private static extern void info(string logMsg, string prefix = null);

        [DllImport("yvrutilities")]
        private static extern void warn(string logMsg, string prefix = null);

        [DllImport("yvrutilities")]
        private static extern void error(string logMsg, string prefix = null);

        [DllImport("yvrutilities")]
        private static extern void setDebugOutputHandler(Action<string, string> outputViaUnityHandler);

        [DllImport("yvrutilities")]
        private static extern void setInfoOutputHandler(Action<string, string> outputViaUnityHandler);

        [DllImport("yvrutilities")]
        private static extern void setWarnOutputHandler(Action<string, string> outputViaUnityHandler);

        [DllImport("yvrutilities")]
        private static extern void setErrorOutputHandler(Action<string, string> outputViaUnityHandler);

        private static LogPriority m_LogcatPriority = LogPriority.LOWEST;
        private static LoggerControllerBase m_logcatOutputController = LogPrefixDummyController.defaultController;
        public LoggerControllerBase logcatOutputController { get => m_logcatOutputController; set => m_logcatOutputController = value ?? LogPrefixDummyController.defaultController; }

        public void ConfigureYLog(string tag, int ramLogSize = 3)
        {
            initializeRamLog(tag, ramLogSize);
        }

        public void SaveLog()
        {
            saveLog();
        }

        public void DebugHandle(in object context, in string message, in string prefix = null)
        {
            debug(message, prefix);
        }

        public void InfoHandle(in object context, in string message, in string prefix = null)
        {
            info(message, prefix);
        }

        public void WarnHandle(in object context, in string message, in string prefix = null)
        {
            warn(message, prefix);
        }

        public void ErrorHandle(in object context, in string message, in string prefix = null)
        {
            error(message, prefix);
        }

        public LogPriority GetLogcatPriorityTHold()
        {
            return getLogcatPriorityTHold();
        }

        public LogPriority GetRamLogPriorityTHold()
        {
            return getRamLogPriorityTHold();
        }

        public void SetLogcatPriority(LogPriority priority)
        {
            m_LogcatPriority = priority;
            setLogcatPriorityTHold(priority);
        }

        public void SetRamLogPriority(LogPriority priority)
        {
            setRamLogPriorityTHold(priority);
        }

        /// <summary>
        /// Basically used for debugging native log
        /// </summary>
        /// <param name="viaUnity"> if true, native log will be output via Unity Debug, otherwise directly via android logcat </param>
        public void SetLogcatOutputHandler(bool viaUnity)
        {
            setDebugOutputHandler(viaUnity ? (Action<string, string>)DebugViaUnity : null);
            setInfoOutputHandler(viaUnity ? (Action<string, string>)InfoViaUnity : null);
            setWarnOutputHandler(viaUnity ? (Action<string, string>)WarnViaUnity : null);
            setErrorOutputHandler(viaUnity ? (Action<string, string>)ErrorViaUnity : null);
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void DebugViaUnity(string msg, string prefix)
        {
            if (m_LogcatPriority > LogPriority.DEBUG && !m_logcatOutputController.IsLogValid(null, msg, LogPriority.DEBUG, prefix)) return;
            Debug.Log(string.IsNullOrEmpty(prefix) ? msg : $"[{prefix}]  {msg}");
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void InfoViaUnity(string msg, string prefix)
        {
            if (m_LogcatPriority > LogPriority.INFO && !m_logcatOutputController.IsLogValid(null, msg, LogPriority.INFO, prefix)) return;
            Debug.Log(string.IsNullOrEmpty(prefix) ? msg : $"[{prefix}]  {msg}");
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void WarnViaUnity(string msg, string prefix)
        {
            if (m_LogcatPriority > LogPriority.WARN && !m_logcatOutputController.IsLogValid(null, msg, LogPriority.WARN, prefix)) return;
            Debug.LogWarning(string.IsNullOrEmpty(prefix) ? msg : $"[{prefix}]  {msg}");
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void ErrorViaUnity(string msg, string prefix)
        {
            if (m_LogcatPriority > LogPriority.ERROR && !m_logcatOutputController.IsLogValid(null, msg, LogPriority.ERROR, prefix)) return;
            Debug.LogError(string.IsNullOrEmpty(prefix) ? msg : $"[{prefix}]  {msg}");
        }
    }
}