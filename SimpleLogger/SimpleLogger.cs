﻿using System;
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
        private readonly DateTime dt;
        private StreamWriter log;
        private EventLog eventLog;
        private string eventSource;
        private string fullLogFileName;
        private static readonly object LockObject = new object();
        private static readonly object LockColorObject = new object();
        private static readonly StringBuilder Sb = new StringBuilder();
        private static readonly Lazy<SimpleLogger> Lazy = new Lazy<SimpleLogger>(() => new SimpleLogger());
        public static SimpleLogger GetInstance => Lazy.Value;

        private SimpleLogger()
        {
            if (Environment.UserInteractive) SetConsole.StandardConfig();
            dt = DateTime.Now;
        }
        public void InitOptions(string eventSource = null, string logFolder = @"", string logFile = null)
        {
            try
            {
                if (logFile != null)
                {
                    var logPath = Environment.ExpandEnvironmentVariables(logFolder);
                    if (logFolder != "") Directory.CreateDirectory(logPath);
                    fullLogFileName = Path.Combine(logPath, DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + logFile + ".log");
                    log = new StreamWriter(fullLogFileName);
                    log.Flush();
                }
                this.eventSource = eventSource;
                if (eventSource != null)
                {
                    eventLog = new EventLog();
                    if (!EventLog.SourceExists(this.eventSource)) EventLog.CreateEventSource(this.eventSource, this.eventSource); 
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

            if (log != null) log.Write(message);

            lock (LockColorObject)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                WriteLine(message);
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }
        private void WriteEventLog(string message, EventLogEntryType eventLogLevel = EventLogEntryType.Error, int eventId = 1000)
        {
            if (eventLog == null) return;
            eventLog.Source = eventSource;
            eventLog.WriteEntry("Application run log file: " + fullLogFileName + Environment.NewLine + message, eventLogLevel, eventId);
        }
        public void WriteWarningLine(string message,EventLogEntryType eventLogLevel = EventLogEntryType.Warning,  int eventId = 500)
        {
            WriteEventLog(message, eventLogLevel, eventId);

            log?.Write(message);

            lock (LockColorObject)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                WriteLine(message);
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }
        public void WriteLine(string message, bool writeToEventLog = false, EventLogEntryType eventLogLevel = EventLogEntryType.Information, int eventId = 100)
        {
            if (writeToEventLog) WriteEventLog(message, eventLogLevel, eventId);

            lock (LockObject)
            {
                var messageStr = GetExecutionInfo() + message;
                log?.WriteLine(messageStr);
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
        public void WriteLine(bool writeToEventLog = false, EventLogEntryType eventLogLevel = EventLogEntryType.Information, int eventId = 100)
        {
            WriteLine("");
        }
        public void WriteException(Exception e, string message = "", bool stackTrace = false)
        {
            lock (Sb)
            {
                Sb.Clear();
                Sb.AppendLine(GetExecutionInfo() + " " + message + Environment.NewLine + "Exception was: " + e.Message);
                if (e.InnerException != null) { Sb.AppendLine("InnerException was: " + e.InnerException.Message); }
                Sb.AppendLine("GetBaseException().Message was: " + e.GetBaseException().Message);
                if (stackTrace) Sb.AppendLine("StackTrace: " + e.StackTrace); 
                WriteErrorLine(Sb.ToString());
            }
        }
        public void Close(bool waitBeforeConsoleClose = true, int waitSecondsToClose = 0)
        {
            var message = "SimpleLogger closed.  Execution time was " + (DateTime.Now - dt);
            eventLog?.WriteEntry(GetExecutionInfo() + message, EventLogEntryType.Information, 15);
            WriteLine(message);
            log?.Close();
            if (waitBeforeConsoleClose) Console.ReadLine();
            else Thread.Sleep(waitSecondsToClose * 1000);
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
                Console.BufferHeight = short.MaxValue - 1;
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
