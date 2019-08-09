using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;

using System.Windows.Forms;

namespace Utilities
{
    public sealed class SimpleLogger
    {
        private readonly DateTime _dt;
        private StreamWriter _log;
        private EventLog _eventLog;
        private string _eventSource;
        private string _fullLogFileName;
        private static readonly object LockObject = new object();
        private static readonly object LockColorObject = new object();
        private static readonly StringBuilder _sb = new StringBuilder();
        private static readonly Lazy<SimpleLogger> Lazy = new Lazy<SimpleLogger>(() => new SimpleLogger());
        public static SimpleLogger GetInstance => Lazy.Value;

        private SimpleLogger()
        {
            if (Environment.UserInteractive) SetConsole.StandardConfig();
            _dt = DateTime.Now;
        }
        public void InitOptions(string eventSource = null, string logFolder = @"", string logFile = null)
        {
            try
            {
                if (logFile != null)
                {
                    var logPath = Environment.ExpandEnvironmentVariables(logFolder);
                    if (logFolder != "") System.IO.Directory.CreateDirectory(logPath);
                    _fullLogFileName = Path.Combine(logPath, DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + logFile + ".log");
                    _log = new StreamWriter(_fullLogFileName);
                    _log.Flush();
                }
                _eventSource = eventSource;
                if (eventSource != null)
                {
                    _eventLog = new EventLog();
                    if (!EventLog.SourceExists(_eventSource)) EventLog.CreateEventSource(_eventSource, _eventSource); 
                }
            }
            catch (SecurityException e)
            {
                var exMsg = "First execution of program needs to be run elevated as \"Adminstrator\" so that the event-log source can be added. The exception message was --> " + e.Message;
                if (Environment.UserInteractive) MessageBox.Show(exMsg);
                Console.WriteLine(exMsg);
                Environment.Exit(999);
            }
            catch (Exception e)
            {
                WriteErrorLine("Severe error encountered during InitOptions.  Exception message --> " + e.Message);
                if (e.InnerException != null) WriteErrorLine("InnerException --> " + e.InnerException);
                Environment.Exit(999);
            }
            const string message = "SimpleLogger Initiated. ";
            WriteLine(message);
            WriteEventLog(message,EventLogEntryType.Information, eventId: 10);
        }
        public void WriteErrorLine(string message, EventLogEntryType eventLogLevel = EventLogEntryType.Error, int eventId = 1000)
        {
            WriteEventLog(message, eventLogLevel, eventId);

            if (_log != null) _log.Write(message);

            lock (LockColorObject)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                WriteLine(message);
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }
        private void WriteEventLog(string message, EventLogEntryType eventLogLevel = EventLogEntryType.Error, int eventId = 1000)
        {
            if (_eventLog == null) return;
            _eventLog.Source = _eventSource;
            _eventLog.WriteEntry("Application run log file: " + _fullLogFileName + Environment.NewLine + message, eventLogLevel, eventId);
        }
        public void WriteWarningLine(string message,EventLogEntryType eventLogLevel = EventLogEntryType.Warning,  int eventId = 500)
        {
            WriteEventLog(message, eventLogLevel, eventId);

            if (_log != null) _log.Write(message);

            lock (LockColorObject)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                WriteLine(message);
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }
        public void WriteLine(string message)
        {
            lock (LockObject)
            {
                var messageStr = GetExecutionInfo() + message;
                if (_log != null) _log.WriteLine(messageStr);
                Console.WriteLine(messageStr);
            }
        }
        public void Write(string message)
        {
            lock (LockObject)
            {
                Console.Write(message);
            }
        }
        public void WriteLine()
        {
            WriteLine("");
        }
        public void WriteException(System.Exception e, string message = "", bool stackTrace = false)
        {
            lock (_sb)
            {
                _sb.Clear();
                _sb.AppendLine(GetExecutionInfo() + " " + message + Environment.NewLine + "Exception was: " + e.Message);
                if (e.InnerException != null) { _sb.AppendLine("InnerException was: " + e.InnerException.Message); }
                _sb.AppendLine("GetBaseException().Message was: " + e.GetBaseException().Message);
                if (stackTrace) _sb.AppendLine("StackTrace: " + e.StackTrace); 
                WriteErrorLine(_sb.ToString());
            }
        }
        public void Close(bool waitBeforeConsoleClose = true, int waitSecondsToClose = 0)
        {
            var message = "SimpleLogger closed.  Execution time was " + (DateTime.Now - _dt);
            if (_eventLog != null) _eventLog.WriteEntry(GetExecutionInfo() + message, EventLogEntryType.Information, 15);
            WriteLine(message);
            if (_log != null) _log.Close();
            if (waitBeforeConsoleClose) Console.ReadLine();
            else System.Threading.Thread.Sleep(waitSecondsToClose * 1000);
        }
        private static string GetExecutionInfo()
        {
            return DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss-") + Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]) + "<P" + Process.GetCurrentProcess().Id + "><T" + Thread.CurrentThread.ManagedThreadId + "> ";
        }
    } 
    public static class SetConsole
    {
        public static void StandardConfig()
        {
            try
            {
            Console.BufferHeight = Int16.MaxValue - 1;
            Console.BufferWidth = 180;
            Console.WindowHeight = 50;
            Console.WindowWidth = 180;
            Console.ForegroundColor = ConsoleColor.Green;
            }
            catch (Exception)
            {
                ; 
            }
        }
    }
}
