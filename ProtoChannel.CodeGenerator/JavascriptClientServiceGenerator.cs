using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.CodeGenerator
{
    internal class JavascriptClientServiceGenerator : CodeGenerator
    {
        public JavascriptClientServiceGenerator()
            : base(GetFilename(), 2)
        {
        }

        private static string GetFilename()
        {
            if (Program.ResolvedArguments.ServerServiceType == null)
                throw new CommandLineArgumentException("Server service type is required when generating the Javascript client service");
            if (Program.Arguments.JavascriptClientServiceName == null)
                throw new CommandLineArgumentException("Javascript service class name is required when generating the Javascript client service");

            return Program.Arguments.JavascriptClientServiceTarget;
        }

        public override void Generate()
        {
            throw new NotImplementedException();
        }
    }
}
