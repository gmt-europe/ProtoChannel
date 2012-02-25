using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProtoChannel.CodeGenerator
{
    internal class ClientServiceGenerator : CodeGenerator
    {
        public ClientServiceGenerator()
            : base(GetFilename(), 4)
        {
        }

        private static string GetFilename()
        {
            if (Program.ResolvedArguments.ServerServiceType == null)
                throw new CommandLineArgumentException("Server service type is required when generating the client service");
            if (Program.Arguments.ClientServiceName == null)
                throw new CommandLineArgumentException("Client service class name is required when generating the client service");

            return Program.Arguments.ClientServiceTarget;
        }

        public override void Generate()
        {
            if (Program.Arguments.Namespace != null)
            {
                WriteLine("namespace {0}", Program.Arguments.Namespace);
                WriteLine("{");
                Indent();
            }

            WriteClass();

            if (Program.Arguments.Namespace != null)
            {
                Unindent();
                WriteLine("}");
            }
        }

        private void WriteClass()
        {
            WriteLine("[global::System.CodeDom.Compiler.GeneratedCode(\"{0}\", \"{1}\")]", GetType().Assembly.GetName().Name, GetType().Assembly.GetName().Version);
            WriteLine("{0} partial class {1} : global::ProtoChannel.ProtoClient", Modifier, Program.Arguments.ClientServiceName);
            WriteLine("{");
            Indent();

            WriteConstructors();

            WriteMethods();

            Unindent();
            WriteLine("}");
        }

        private void WriteConstructors()
        {
            WriteLine("[global::System.Diagnostics.DebuggerStepThrough]");
            WriteLine("public {0}(global::System.Net.IPEndPoint remoteEndPoint)", Program.Arguments.ClientServiceName);
            Indent();
            WriteLine(": base(remoteEndPoint)");
            Unindent();
            WriteLine("{");
            WriteLine("}");
            WriteLine();
            WriteLine("[global::System.Diagnostics.DebuggerStepThrough]");
            WriteLine("public {0}(global::System.Net.IPEndPoint remoteEndPoint, global::ProtoChannel.ProtoClientConfiguration configuration)", Program.Arguments.ClientServiceName);
            Indent();
            WriteLine(": base(remoteEndPoint, configuration)");
            Unindent();
            WriteLine("{");
            WriteLine("}");
            WriteLine();
            WriteLine("[global::System.Diagnostics.DebuggerStepThrough]");
            WriteLine("public {0}(global::System.Net.IPAddress address, int port)", Program.Arguments.ClientServiceName);
            Indent();
            WriteLine(": base(address, port)");
            Unindent();
            WriteLine("{");
            WriteLine("}");
            WriteLine();
            WriteLine("[global::System.Diagnostics.DebuggerStepThrough]");
            WriteLine("public {0}(global::System.Net.IPAddress address, int port, global::ProtoChannel.ProtoClientConfiguration configuration)", Program.Arguments.ClientServiceName);
            Indent();
            WriteLine(": base(address, port, configuration)");
            Unindent();
            WriteLine("{");
            WriteLine("}");
            WriteLine();
            WriteLine("[global::System.Diagnostics.DebuggerStepThrough]");
            WriteLine("public {0}(string hostname, int port)", Program.Arguments.ClientServiceName);
            Indent();
            WriteLine(": base(hostname, port)");
            Unindent();
            WriteLine("{");
            WriteLine("}");
            WriteLine();
            WriteLine("[global::System.Diagnostics.DebuggerStepThrough]");
            WriteLine("public {0}(string hostname, int port, global::ProtoChannel.ProtoClientConfiguration configuration)", Program.Arguments.ClientServiceName);
            Indent();
            WriteLine(": base(hostname, port, configuration)");
            Unindent();
            WriteLine("{");
            WriteLine("}");
        }

        private void WriteMethods()
        {
            foreach (var method in Program.ResolvedArguments.ServerServiceType.GetMethods())
            {
                var attribute = GetProtoMethodAttribute(method);

                if (attribute == null)
                    continue;

                WriteLine();

                if (((dynamic)attribute).IsOneWay)
                    WritePostMethod(method);
                else
                    WriteSendMethod(method);
            }
        }

        private void WritePostMethod(MethodInfo method)
        {
            WriteLine("[global::System.Diagnostics.DebuggerStepThrough]");
            WriteLine("public void {0}(global::{1} message)", method.Name, method.GetParameters()[0].ParameterType.FullName);
            WriteLine("{");
            Indent();
            WriteLine("PostMessage(message);");
            Unindent();
            WriteLine("}");
        }

        private void WriteSendMethod(MethodInfo method)
        {
            WriteLine("[global::System.Diagnostics.DebuggerStepThrough]");
            WriteLine("public global::{0} {1}(global::{2} message)", method.ReturnType.FullName, method.Name, method.GetParameters()[0].ParameterType.FullName);
            WriteLine("{");
            WriteLine("    return End{0}(Begin{0}(message, null, null));", method.Name);
            WriteLine("}");
            WriteLine();
            WriteLine("[global::System.Diagnostics.DebuggerStepThrough]");
            WriteLine("public global::System.IAsyncResult Begin{0}(global::{1} message, global::System.AsyncCallback callback, object asyncState)", method.Name, method.GetParameters()[0].ParameterType.FullName);
            WriteLine("{");
            WriteLine("    return BeginSendMessage(message, typeof(global::{0}), callback, asyncState);", method.ReturnType.FullName);
            WriteLine("}");
            WriteLine();
            WriteLine("[global::System.Diagnostics.DebuggerStepThrough]");
            WriteLine("public global::{0} End{1}(global::System.IAsyncResult asyncResult)", method.ReturnType.FullName, method.Name);
            WriteLine("{");
            WriteLine("    return (global::{0})EndSendMessage(asyncResult);", method.ReturnType.FullName);
            WriteLine("}");
        }

        private object GetProtoMethodAttribute(MethodInfo method)
        {
            foreach (var attribute in method.GetCustomAttributes(true))
            {
                if (attribute.GetType().FullName == "ProtoChannel.ProtoMethodAttribute")
                    return attribute;
            }

            return null;
        }
    }
}
