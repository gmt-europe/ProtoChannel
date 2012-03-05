using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class ServiceMethod
    {
        private readonly ProtoMethodAttribute _attribute;
        private readonly ReflectionOptimizer.MessageInvoker _invoker;
        private readonly ReflectionOptimizer.VoidMessageInvoker _voidInvoker;

        public bool IsOneWay
        {
            get { return _attribute.IsOneWay; }
        }

        public object Invoke(object service, object message)
        {
            if (_invoker != null)
            {
                return _invoker(service, message);
            }
            else
            {
                _voidInvoker(service, message);

                return null;
            }
        }

        public ServiceMessage Request { get; private set; }

        public ServiceMessage Response { get; private set; }

        public ServiceMethod(MethodInfo method, ProtoMethodAttribute attribute, ServiceAssembly assembly)
        {
            Require.NotNull(method, "method");
            Require.NotNull(attribute, "attribute");

            _attribute = attribute;

            var parameters = method.GetParameters();

            if (parameters.Length != 1)
                throw new ProtoChannelException(String.Format("Invalid service method signature for method '{0}'; service methods must accept a ProtoMessage parameter and must return a ProtoMessage or void", method));

            ServiceMessage request;

            if (!assembly.MessagesByType.TryGetValue(parameters[0].ParameterType, out request))
                throw new ProtoChannelException(String.Format("Assembly '{0}' does not contain a message type '{1}'", assembly.Assembly, parameters[0].ParameterType));

            Request = request;

            if (method.ReturnType != typeof(void))
            {
                if (attribute.IsOneWay)
                    throw new ProtoChannelException(String.Format("Invalid service method signature for method '{0}'; IsOneWay methods must return void", method));

                ServiceMessage response;

                if (!assembly.MessagesByType.TryGetValue(method.ReturnType, out response))
                    throw new ProtoChannelException(String.Format("Assembly '{0}' does not contain a message type '{1}'", assembly.Assembly, method.ReturnType));

                Response = response;

                _invoker = ReflectionOptimizer.BuildMessageInvoker(method);
            }
            else
            {
                _voidInvoker = ReflectionOptimizer.BuildVoidMessageInvoker(method);
            }
        }
    }
}
