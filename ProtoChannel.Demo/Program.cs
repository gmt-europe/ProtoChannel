using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Common.Logging;
using Common.Logging.Simple;

namespace ProtoChannel.Demo
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            MessageBox.Show(exception.Message);
        }
    }
}
