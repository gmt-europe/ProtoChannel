using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace ProtoChannel.CodeGenerator
{
    internal class Program
    {
        public static Arguments Arguments { get; private set; }
        public static ResolvedArguments ResolvedArguments { get; private set; }

        static void Main(string[] args)
        {
            // Reset the current culture for ToString.

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            try
            {
                Arguments = new Arguments();

                Arguments.Parse(args);

                ResolvedArguments = new ResolvedArguments();

                if (Arguments.ClientServiceTarget != null)
                {
                    using (var generator = new ClientServiceGenerator())
                    {
                        generator.Generate();
                    }
                }

                if (Arguments.ServerCallbackServiceTarget != null)
                {
                    using (var generator = new ServerCallbackServiceGenerator())
                    {
                        generator.Generate();
                    }
                }

                if (Arguments.JavascriptClientServiceTarget != null)
                {
                    using (var generator = new JavascriptClientServiceGenerator())
                    {
                        generator.Generate();
                    }
                }

            }
            catch (CommandLineArgumentException ex)
            {
                Console.WriteLine("Invalid command line arguments provided:");
                Console.WriteLine();
                Console.WriteLine("\t" + ex.Message);
                Console.WriteLine();

                Arguments.WriteUsage();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected situation has occurred:");
                Console.WriteLine();

                PrintStackTrace(ex);
            }
        }

        private static void PrintStackTrace(Exception ex)
        {
            if (ex.InnerException != null)
            {
                PrintStackTrace(ex.InnerException);

                Console.WriteLine();
                Console.WriteLine("=== Caused ===");
                Console.WriteLine();
            }

            Console.WriteLine(ex.Message);

            if (!String.IsNullOrEmpty(ex.StackTrace))
            {
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace.TrimEnd());
            }
        }
    }
}
