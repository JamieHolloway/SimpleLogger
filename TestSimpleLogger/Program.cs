using System;
using System.Text;
using Utilities;

namespace TestSimpleLogger
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var log = SimpleLogger.GetInstance;

            //log.InitOptions(eventSource: "TestSimpleLogger", logFile: "TestLogFile");
            //log.WriteLine("test");
            //log.Close();


            //log = SimpleLogger.GetInstance;
            //log.InitOptions(logFile: "TestLogFile");
            //log.WriteLine("test2");
            //log.Close();

            //log = SimpleLogger.GetInstance;
            //log.InitOptions(eventSource: "TestSimpleLogger");
            //log.WriteLine("test3");
            //log.Close();

            log.InitOptions(eventSource: "TestSimpleLogger", logFolder: @"%systemdrive%\PutLogsHere", logFile: "TestLogFile");
            try
            {
                string temp = null;
                //var temp2 = temp.ToCharArray();

                Test2();
            }
            catch (System.Exception e)
            {
                log.WriteException(e, "Exception caught here");
            }
            finally
            {
                log.Close();
            }

            Console.ReadLine();
        }

        private static void Test2()
        {
            try
            {
                Test3();
            }
            catch (Exception ex)
            {
                throw new Exception("test2 -- Source was " + ex.Source, ex);
            }
        }
        private static void Test3()
        {
            try
            {
                Test4();
            }
            catch (Exception ex)
            {
                throw new Exception("test3 -- Source was " + ex.Source, ex);
            }
        }
        private static void Test4()
        {
            try
            {
                /*
                var datatable = new DataTable();
                using (var connection = new SqlConnection("Data Source=garb;Database=network;Integrated Security=True"))
                {
                    var adapter = new SqlDataAdapter("select * from xcon", connection);
                    adapter.Fill(datatable);
                    Console.WriteLine("the number of rows is: {0}", datatable.Rows.Count);
                }
                */
                var z = 0;
                var x = 1;
                var zeroDivide = x / z;
            }
            catch (Exception ex)
            {
                throw new Exception("test4 -- Source was " + ex.Source, ex);
            }
        }


        private static StringBuilder GetInnerExceptionMessage(StringBuilder unwrapMessages, Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
            {
                unwrapMessages.AppendLine("unwrapping...." + ex.Message);
            }

            if (ex.InnerException != null)
            {
                unwrapMessages = GetInnerExceptionMessage(unwrapMessages, ex.InnerException);
            }
            return unwrapMessages;
        }

    }
}
