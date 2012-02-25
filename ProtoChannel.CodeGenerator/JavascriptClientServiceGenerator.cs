using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProtoChannel.CodeGenerator
{
    internal class JavascriptClientServiceGenerator : CodeGenerator
    {
        public JavascriptClientServiceGenerator()
            : base(GetFilename(), 4)
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
            var types = new List<ProtoType>();

            foreach (var type in Program.ResolvedArguments.SourceAssembly.GetTypes())
            {
                if (GetProtoContractType(type) == null)
                    continue;

                var attribute = GetProtoMessageAttribute(type);

                types.Add(new ProtoType(type, attribute == null ? null : ((dynamic)attribute).MessageId));
            }

            foreach (var type in types.OrderBy(p => p.Type.FullName))
            {
                WriteType(type);

                WriteLine();
            }

            WriteChannel();
        }

        private void WriteType(ProtoType type)
        {
            if (type.MessageId.HasValue)
                WriteLine("{0} = Class.create(ProtoMessage, {{", type.Type.Name);
            else
                WriteLine("{0} = Class.create(ProtoType, {{", type.Type.Name);

            Indent();

            if (type.MessageId.HasValue)
                WriteMessageConstructor(type);
            else
                WriteTypeConstructor(type);

            WriteLine();

            WriteMessageSerialize(type);

            WriteLine();

            WriteMessageDeserializer(type);

            Unindent();
            WriteLine("});");

            if (type.MessageId.HasValue)
            {
                WriteLine();
                WriteLine("ProtoRegistry.registerType({0}, {1});", type.Type.Name, type.MessageId);
            }
        }

        private void WriteMessageConstructor(ProtoType type)
        {
            WriteLine("initialize: function ($super, values) {");
            Indent();

            if (type.Members.Count > 0)
            {
                foreach (var member in type.Members)
                {
                    if (member.IsCollection)
                        WriteLine("this.{0} = [];", EncodeName(member.Name));
                    else
                        WriteLine("this.{0} = {1};", EncodeName(member.Name), Encode(member.DefaultValue));
                }

                WriteLine();
            }

            WriteLine("$super({0}, values);", type.MessageId);

            Unindent();
            WriteLine("},");
        }

        private void WriteTypeConstructor(ProtoType type)
        {
            WriteLine("initialize: function ($super, values) {");
            Indent();

            if (type.Members.Count > 0)
            {
                foreach (var member in type.Members)
                {
                    WriteLine("this.{0} = {1};", EncodeName(member.Name), Encode(member.DefaultValue));
                }

                WriteLine();
            }

            WriteLine("$super(values);");

            Unindent();
            WriteLine("},");
        }

        private void WriteMessageSerialize(ProtoType type)
        {
            WriteLine("serialize: function () {");
            Indent();

            WriteLine("var message = {};");
            WriteLine();

            if (type.Members.Count > 0)
            {
                foreach (var member in type.Members)
                {
                    if (member.IsCollection)
                    {
                        WriteLine("if (this.{0} !== null && this.{0}.length > 0) {{", EncodeName(member.Name));
                        Indent();

                        if (GetProtoContractType(member.Type) == null)
                        {
                            WriteLine("message[{0}] = this.{1};", member.Tag, EncodeName(member.Name));
                        }
                        else
                        {
                            WriteLine("var items = [];");
                            WriteLine("for (var i = 0; i < this.{0}.length; i++) {{", EncodeName(member.Name));
                            Indent();

                            WriteLine("items.push(this.{0}[i].serialize());", EncodeName(member.Name));

                            Unindent();
                            WriteLine("}");

                            WriteLine("message[{0}] = items;", member.Tag);
                        }

                        Unindent();
                        WriteLine("}");
                    }
                    else
                    {
                        WriteLine("if (this.{0} !== {1})", EncodeName(member.Name), Encode(member.DefaultValue));

                        Indent();

                        if (GetProtoContractType(member.Type) == null)
                        {
                            WriteLine("message[{0}] = this.{1};", member.Tag, EncodeName(member.Name));
                        }
                        else
                        {
                            WriteLine("message[{0}] = this.{1}.serialize();", member.Tag, EncodeName(member.Name));
                        }

                        Unindent();
                    }
                }

                WriteLine();
            }

            WriteLine("return message;");

            Unindent();
            WriteLine("},");
        }

        private void WriteMessageDeserializer(ProtoType type)
        {
            WriteLine("deserialize: function (message) {");
            Indent();

            foreach (var member in type.Members)
            {
                WriteLine("if (message[{0}] !== undefined) {{", member.Tag);
                Indent();

                    if (GetProtoContractType(member.Type) == null)
                    {
                        WriteLine("this.{0} = message[{1}];", EncodeName(member.Name), member.Tag);
                    }
                    else if (member.IsCollection)
                    {
                        WriteLine("this.{0} = [];", EncodeName(member.Name));

                        WriteLine("for (var i = 0; i < message[{0}].length; i++) {{", member.Tag);
                        Indent();

                        WriteLine("var item = new {0}();", member.Type.Name);
                        WriteLine("item.deserialize(message[{0}][i]);", member.Tag);
                        WriteLine("this.{0}.push(item);", EncodeName(member.Name));

                        Unindent();
                        WriteLine("}");
                    }
                    else
                    {
                        WriteLine("var item = new {0}();", member.Type.Name);
                        WriteLine("item.deserialize(message[{0}]);", member.Tag);
                        WriteLine("this.{0} = item;", EncodeName(member.Name));
                    }

                Unindent();
                WriteLine("}");
            }

            Unindent();
            WriteLine("}");
        }

        private void WriteChannel()
        {
            WriteLine("{0} = Class.create(ProtoChannel, {{", Program.Arguments.JavascriptClientServiceName);
            Indent();

            var methods = Program.ResolvedArguments.ClientCallbackServiceType.GetMethods().Where(p => GetProtoMethodAttribute(p) != null).ToArray();

            for (int i = 0; i < methods.Length; i++)
            {
                var attribute = GetProtoMethodAttribute(methods[i]);

                if (((dynamic)attribute).IsOneWay)
                    WriteLine("{0}: function (message) {{", EncodeName(methods[i].Name));
                else
                    WriteLine("{0}: function (message, callback) {{", EncodeName(methods[i].Name));
                Indent();

                WriteLine("if (!(message instanceof {0}))", methods[i].GetParameters()[0].ParameterType.Name);
                Indent();
                WriteLine("message = new {0}(message);", methods[i].GetParameters()[0].ParameterType.Name);
                Unindent();

                WriteLine();

                if (((dynamic)attribute).IsOneWay)
                    WriteLine("this.postMessage(message);");
                else
                    WriteLine("this.sendMessage(message, callback);");

                Unindent();
                WriteLine("}" + (i < methods.Length - 1 ? "," : ""));

                if (i < methods.Length - 1)
                    WriteLine();
            }

            Unindent();
            WriteLine("});");
        }

        private string EncodeName(string name)
        {
            if (name.Length > 0)
                return Char.ToLower(name[0]) + name.Substring(1);
            else
                return "";
        }

        private object GetProtoContractType(Type type)
        {
            foreach (var attribute in type.GetCustomAttributes(true))
            {
                if (attribute.GetType().FullName == "ProtoBuf.ProtoContractAttribute")
                    return attribute;
            }

            return null;
        }

        private object GetProtoMessageAttribute(Type type)
        {
            foreach (var attribute in type.GetCustomAttributes(true))
            {
                if (attribute.GetType().FullName == "ProtoChannel.ProtoMessageAttribute")
                    return attribute;
            }

            return null;
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

        private object Encode(object value)
        {
            if (value == null)
            {
                return "null";
            }
            else if (value is string)
            {
                return EncodeString((string)value);
            }
            else if (value is bool)
            {
                return (bool)value ? "true" : "false";
            }
            else
            {
                string stringValue = value.ToString();

                if (
                    (value is float || value is decimal || value is double) &&
                    !stringValue.Contains(".")
                )
                    stringValue += ".0";

                return stringValue;
            }
        }

        private object EncodeString(string value)
        {
            var sb = new StringBuilder();

            sb.Append("'");

            foreach (char c in value)
            {
                switch (c)
                {
                    case '\'': sb.Append("\\'"); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    default: sb.Append(c); break;
                }
            }

            sb.Append("'");

            return sb.ToString();
        }
    }
}
