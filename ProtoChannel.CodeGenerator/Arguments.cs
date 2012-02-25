using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.CodeGenerator
{
    internal class Arguments
    {
        private readonly Argument<string> _sourceAssembly = new Argument<string>(true, "-s", "Source assembly");
        private readonly Argument<string> _namespace = new Argument<string>(false, "-ns", "Generated code namespace");
        private readonly Argument _public = new Argument("-p", "Generate public classes");
        private readonly Argument<string> _serverServiceType = new Argument<string>(false, "-ss", "Server service type");
        private readonly Argument<string> _clientServiceTarget = new Argument<string>(false, "-cs", "Client service target file");
        private readonly Argument<string> _clientServiceName = new Argument<string>(false, "-csn", "Client service class name");
        private readonly Argument<string> _clientCallbackServiceType = new Argument<string>(false, "-cc", "Client callback service type");
        private readonly Argument<string> _serverCallbackServiceTarget = new Argument<string>(false, "-sc", "Server callback service target file");
        private readonly Argument<string> _serverCallbackServiceName = new Argument<string>(false, "-scn", "Server callback service class name");
        private readonly Argument<string> _javascriptClientServiceTarget = new Argument<string>(false, "-jcs", "Javascript client service target file");
        private readonly Argument<string> _javascriptClientServiceName = new Argument<string>(false, "-jcsn", "Javascript client service class name");

        private readonly Argument[] _arguments;

        public string SourceAssembly { get { return _sourceAssembly.Value; } }
        public string Namespace { get { return _namespace.Value; } }
        public bool Public { get { return _public.IsProvided; } }
        public string ServerServiceType { get { return _serverServiceType.Value; } }
        public string ClientServiceTarget { get { return _clientServiceTarget.Value; } }
        public string ClientServiceName { get { return _clientServiceName.Value; } }
        public string ClientCallbackServiceType { get { return _clientCallbackServiceType.Value; } }
        public string ServerCallbackServiceTarget { get { return _serverCallbackServiceTarget.Value; } }
        public string ServerCallbackServiceName { get { return _serverCallbackServiceName.Value; } }
        public string JavascriptClientServiceTarget { get { return _javascriptClientServiceTarget.Value; } }
        public string JavascriptClientServiceName { get { return _javascriptClientServiceName.Value; } }

        public Arguments()
        {
            _arguments = new Argument[]
            {
                _sourceAssembly,
                _namespace,
                _public,
                _serverServiceType,
                _clientServiceTarget,
                _clientServiceName,
                _clientCallbackServiceType,
                _serverCallbackServiceTarget,
                _serverCallbackServiceName,
                _javascriptClientServiceTarget,
                _javascriptClientServiceName
            };
        }

        public void Parse(string[] args)
        {
            int offset = 0;

            while (offset < args.Length)
            {
                string flag = args[offset];
                bool matched = false;

                for (int i = 0; i < _arguments.Length; i++)
                {
                    if (_arguments[i].Flag == flag)
                    {
                        offset++;

                        _arguments[i].Parse(args, ref offset);

                        matched = true;
                        break;
                    }
                }

                if (!matched)
                    throw new CommandLineArgumentException(String.Format("Can't understand argument '{0}'", flag));
            }

            foreach (var argument in _arguments)
            {
                if (argument.IsMandatory && !argument.IsProvided)
                    throw new CommandLineArgumentException(String.Format("Argument '{0}' is mandatory", argument.Flag));
            }
        }

        public void WriteUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine();

            foreach (var argument in _arguments)
            {
                Console.Write("\t" + argument.Flag + ":\t" + argument.Description);

                if (argument.IsMandatory)
                    Console.Write(" (mandatory)");

                Console.WriteLine();
            }
        }
    }
}
