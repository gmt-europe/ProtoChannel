using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            WriteLine("{0} partial class {1} : global::ProtoChannel.ProtoCallbackChannel", Modifier, Program.Arguments.ServerCallbackServiceName);
            WriteLine("{");
            Indent();

            WriteMethods();

            Unindent();
            WriteLine("}");
        }

        private void WriteMethods()
        {
            bool hadOne = false;

            foreach (var method in Program.ResolvedArguments.ServerServiceType.GetMethods())
            {
                var attribute = GetProtoMethodAttribute(method);

                if (attribute == null)
                    continue;

                if (hadOne)
                    WriteLine();
                else
                    hadOne = true;

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
