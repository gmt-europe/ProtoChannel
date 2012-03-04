using System;
using System.Collections.Generic;
using System.Linq;
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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
