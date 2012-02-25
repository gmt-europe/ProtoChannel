using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProtoChannel.CodeGenerator
{
    internal class ResolvedArguments
    {
        public Assembly SourceAssembly { get; private set; }
        public Type ServerServiceType { get; private set; }
        public Type ClientCallbackServiceType { get; private set; }

        public ResolvedArguments()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            SourceAssembly = Assembly.LoadFile(Path.GetFullPath(Program.Arguments.SourceAssembly));

            if (Program.Arguments.ServerServiceType != null)
                ServerServiceType = SourceAssembly.GetType(Program.Arguments.ServerServiceType);

            if (Program.Arguments.ClientCallbackServiceType != null)
                ClientCallbackServiceType = SourceAssembly.GetType(Program.Arguments.ClientCallbackServiceType);
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string name = args.Name.Split(',')[0].Trim();

            string path = Path.GetDirectoryName(Path.GetFullPath(Program.Arguments.SourceAssembly));

            if (File.Exists(Path.Combine(path, name + ".dll")))
                return Assembly.LoadFile(Path.Combine(path, name + ".dll"));
            else if (File.Exists(Path.Combine(path, name + ".exe")))
                return Assembly.LoadFile(Path.Combine(path, name + ".exe"));
            else
                return null;
        }
    }
}
