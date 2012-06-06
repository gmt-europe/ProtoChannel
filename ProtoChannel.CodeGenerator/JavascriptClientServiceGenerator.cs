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
            if (Program.ResolvedArguments.ClientCallbackServiceType == null)
                throw new CommandLineArgumentException("Client callback service type is required when generating the Javascript client service");
            if (Program.Arguments.JavascriptClientServiceName == null)
                throw new CommandLineArgumentException("Javascript service class name is required when generating the Javascript client service");
            if (Program.Arguments.JavascriptCallbackServiceName == null)
                throw new CommandLineArgumentException("Javascript callback class name is required when generating the Javascript client service");

            return Program.Arguments.JavascriptClientServiceTarget;
        }

        public override void Generate()
        {
            var types = new List<ProtoType>();

            if (Program.Arguments.JavascriptNamespace != null)
            {
                var sb = new StringBuilder();

                foreach (string part in Program.Arguments.JavascriptNamespace.Split('.'))
                {
                    if (sb.Length > 0)
                        sb.Append('.');
                    sb.Append(part);

                    WriteLine("if ({0} === undefined) {0} = {{}};", sb);
                }

                WriteLine();
            }

            foreach (var type in Program.ResolvedArguments.SourceAssembly.GetTypes())
            {
                if (GetProtoContractType(type) == null)
                    continue;

                var attribute = GetProtoMessageAttribute(type);

                types.Add(new ProtoType(type, attribute == null ? null : ((dynamic)attribute).MessageId));
            }

            foreach (var type in types.OrderBy(p => p.Type.FullName))
            {
                if (type.Type.IsEnum)
                    WriteEnumType(type);
                else
                    WriteType(type);

                WriteLine();
            }

            WriteChannel();

            WriteLine();

            WriteCallbackChannel();
        }

        private void WriteEnumType(ProtoType type)
        {
            WriteLine("{0} = {{", TypeName(type.Type));

            var values = Enum.GetValues(type.Type);

            for (int i = 0; i < values.Length; i++)
            {
                WriteLine("    {0}: {1}{2}", values.GetValue(i), (int)values.GetValue(i), i < values.Length - 1 ? "," : "");
            }

            WriteLine("};");
        }

        private void WriteType(ProtoType type)
        {
            if (type.MessageId.HasValue)
                WriteLine("{0} = Class.create(ProtoMessage, {{", TypeName(type.Type));
            else
                WriteLine("{0} = Class.create(ProtoType, {{", TypeName(type.Type));

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
                WriteLine("ProtoRegistry.registerType({0}, {1});", TypeName(type.Type), type.MessageId);
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
                    if (member.IsCollection)
                        WriteLine("this.{0} = [];", EncodeName(member.Name));
                    else
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

                            if (member.Type.IsEnum)
                            {
                                WriteLine("items.push(this.{0}[i].serialize());", EncodeName(member.Name));
                            }
                            else
                            {
                                WriteLine("var item = this.{0}[i];", EncodeName(member.Name));

                                WriteLine("if (!(item instanceof {0})) {{", TypeName(member.Type));
                                Indent();
                                WriteLine("item = new {0}(item);", TypeName(member.Type));
                                Unindent();
                                WriteLine("}");

                                WriteLine("items.push(item.serialize());");
                            }

                            Unindent();
                            WriteLine("}");

                            WriteLine("message[{0}] = items;", member.Tag);
                        }

                        Unindent();
                        WriteLine("}");
                    }
                    else
                    {
                        WriteLine("if (this.{0} !== {1}) {{", EncodeName(member.Name), Encode(member.DefaultValue));

                        Indent();

                        if (GetProtoContractType(member.Type) == null || member.Type.IsEnum)
                        {
                            WriteLine("message[{0}] = this.{1};", member.Tag, EncodeName(member.Name));
                        }
                        else
                        {
                            WriteLine("var item = this.{0};", EncodeName(member.Name));
                            WriteLine("if (!(item instanceof {0})) {{", TypeName(member.Type));
                            Indent();
                            WriteLine("item = new {0}(item);", TypeName(member.Type));
                            Unindent();
                            WriteLine("}");
                            
                            WriteLine("message[{0}] = item.serialize();", member.Tag);
                        }

                        Unindent();
                        WriteLine("}");
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

                    if (GetProtoContractType(member.Type) == null || member.Type.IsEnum)
                    {
                        WriteLine("this.{0} = message[{1}];", EncodeName(member.Name), member.Tag);
                    }
                    else if (member.IsCollection)
                    {
                        WriteLine("this.{0} = [];", EncodeName(member.Name));

                        WriteLine("for (var i = 0; i < message[{0}].length; i++) {{", member.Tag);
                        Indent();

                        WriteLine("var value = message[{0}][i];", member.Tag);

                        WriteLine("if (value === null) {");
                        Indent();

                        WriteLine("this.{0}.push(null);", EncodeName(member.Name));

                        Unindent();
                        WriteLine("} else {");
                        Indent();

                        WriteLine("var item = new {0}();", TypeName(member.Type));
                        WriteLine("item.deserialize(value);");
                        WriteLine("this.{0}.push(item);", EncodeName(member.Name));

                        Unindent();
                        WriteLine("}");

                        Unindent();
                        WriteLine("}");
                    }
                    else
                    {
                        WriteLine("if (message[{0}] === null) {{", member.Tag);
                        Indent();

                        WriteLine("this.{0} = null;", EncodeName(member.Name));

                        Unindent();
                        WriteLine("} else {");
                        Indent();

                        WriteLine("var item = new {0}();", TypeName(member.Type));
                        WriteLine("item.deserialize(message[{0}]);", member.Tag);
                        WriteLine("this.{0} = item;", EncodeName(member.Name));

                        Unindent();
                        WriteLine("}");
                    }

                Unindent();
                WriteLine("}");
            }

            Unindent();
            WriteLine("}");
        }

        private void WriteChannel()
        {
            WriteLine("{0} = Class.create(ProtoChannel, {{", TypeName(Program.Arguments.JavascriptClientServiceName));
            Indent();

            var methods = Program.ResolvedArguments.ServerServiceType.GetMethods().Where(p => GetProtoMethodAttribute(p) != null).ToArray();

            for (int i = 0; i < methods.Length; i++)
            {
                var attribute = GetProtoMethodAttribute(methods[i]);

                if (((dynamic)attribute).IsOneWay)
                    WriteLine("{0}: function (message) {{", EncodeName(methods[i].Name));
                else
                    WriteLine("{0}: function (message, callback) {{", EncodeName(methods[i].Name));
                Indent();

                WriteLine("if (!(message instanceof {0}))", TypeName(methods[i].GetParameters()[0].ParameterType));
                Indent();
                WriteLine("message = new {0}(message);", TypeName(methods[i].GetParameters()[0].ParameterType));
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

        private void WriteCallbackChannel()
        {
            WriteLine("{0} = Class.create(ProtoCallbackChannel, {{", TypeName(Program.Arguments.JavascriptCallbackServiceName));
            Indent();

            var methods = Program.ResolvedArguments.ClientCallbackServiceType.GetMethods().Where(p => GetProtoMethodAttribute(p) != null).ToArray();

            WriteLine("initialize: function ($super) {");
            Indent();
            WriteLine("$super({");

            Indent();

            for (int i = 0; i < methods.Length; i++)
            {
                WriteLine("{0}: {1}{2}", EncodeName(methods[i].Name), TypeName(methods[i].GetParameters()[0].ParameterType), i == methods.Length - 1 ? "" : ",");
            }

            Unindent();
            WriteLine("});");
            Unindent();
            WriteLine("},");

            for (int i = 0; i < methods.Length; i++)
            {
                WriteLine();
                WriteLine("{0}: function (message, expectResponse) {{", EncodeName(methods[i].Name));
                Indent();
                WriteLine("throw 'Not implemented';");
                Unindent();
                WriteLine("}" + (i == methods.Length - 1 ? "" : ","));
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
            else if (value.GetType().IsEnum)
            {
                if (Enum.IsDefined(value.GetType(), value))
                    return TypeName(value.GetType()) + "." + value;
                else
                    return Encode(0);
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

        private string TypeName(string name)
        {
            if (Program.Arguments.JavascriptNamespace != null)
                return Program.Arguments.JavascriptNamespace + "." + name;
            else
                return name;
        }

        private string TypeName(Type type)
        {
            string name = type.FullName;

            int pos = name.LastIndexOf('.');

            if (pos != -1)
                name = name.Substring(pos + 1);

            return TypeName(name.Replace('+', '.'));
        }
    }
}
