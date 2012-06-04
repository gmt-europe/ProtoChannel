using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System;
using ProtoBuf;
using ProtoChannel.Util;

namespace ProtoChannel.Web
{
    internal class ServiceType
    {
        public ServiceMessage Message { get; private set; }

        public Type Type { get; private set; }

        public ServiceTypeFieldCollection Fields { get; private set; }

        public ServiceType(Type type)
        {
            Require.NotNull(type, "type");

            Type = type;
        }

        public void Build(ServiceAssembly assembly)
        {
            Fields = BuildFields(assembly, Type);

            var messageAttributes = Type.GetCustomAttributes(typeof(ProtoMessageAttribute), true);

            if (messageAttributes.Length > 0)
            {
                Debug.Assert(messageAttributes.Length == 1);

                Message = ProtoChannel.ServiceRegistry.GetAssemblyRegistration(Type.Assembly).MessagesById[
                    ((ProtoMessageAttribute)messageAttributes[0]).MessageId
                ];
            }
        }

        private ServiceTypeFieldCollection BuildFields(ServiceAssembly assembly, Type type)
        {
            var result = new ServiceTypeFieldCollection();

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                AddMember(assembly, result, field, field.FieldType);
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                AddMember(assembly, result, property, property.PropertyType);
            }

            return result;
        }

        private static void AddMember(ServiceAssembly assembly, ServiceTypeFieldCollection result, MemberInfo member, Type memberType)
        {
            var attributes = member.GetCustomAttributes(typeof(ProtoMemberAttribute), true);

            if (attributes.Length == 0)
                return;

            Debug.Assert(attributes.Length == 1);

            var attribute = (ProtoMemberAttribute)attributes[0];

            result.Add(new ServiceTypeField(
                assembly,
                ReflectionOptimizer.BuildGetter(member),
                ReflectionOptimizer.BuildSetter(member, true),
                attribute.Tag,
                attribute.IsRequired,
                memberType
            ));
        }
    }
}
