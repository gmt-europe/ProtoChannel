using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.CodeGenerator
{
    internal class ServerCallbackServiceGenerator : CodeGenerator
    {
        public ServerCallbackServiceGenerator()
            : base(GetFilename(), 4)
        {
        }

        private static string GetFilename()
        {
            if (Program.ResolvedArguments.ClientCallbackServiceType == null)
                throw new CommandLineArgumentException("Client callback service type is required when generating the server callback service");
            if (Program.Arguments.ServerCallbackServiceName == null)
                throw new CommandLineArgumentException("Server callback service class name is required when generating the server callback service");

            return Program.Arguments.ServerCallbackServiceTarget;
        }

        public override void Generate()
        {
            throw new NotImplementedException();
        }
    }
}
