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
        private readonly Dictionary<Type, ServiceTypeFieldCollection> _fields = new Dictionary<Type, ServiceTypeFieldCollection>();

        public ServiceMessage Message { get; private set; }

        public Type Type { get; private set; }

        public ServiceTypeFieldCollection Fields { get; private set; }

        public ServiceType(Type type)
        {
            Require.NotNull(type, "type");

            Type = type;

            Fields = BuildFields(Type);

            var messageAttributes = type.GetCustomAttributes(typeof(ProtoMessageAttribute), true);

            if (messageAttributes.Length > 0)
            {
                Debug.Assert(messageAttributes.Length == 1);

                Message = ProtoChannel.ServiceRegistry.GetAssemblyRegistration(type.Assembly).MessagesById[
                    ((ProtoMessageAttribute)messageAttributes[0]).MessageId
                ];
            }
        }

        private ServiceTypeFieldCollection BuildFields(Type type)
        {
            ServiceTypeFieldCollection result;

            if (!_fields.TryGetValue(type, out result))
            {
                result = new ServiceTypeFieldCollection();

                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    AddMember(result, field, field.FieldType);
                }

                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    AddMember(result, property, property.PropertyType);
                }

                _fields[type] = result;
            }

            return result;
        }

        private static void AddMember(ServiceTypeFieldCollection result, MemberInfo member, Type memberType)
        {
            var attributes = member.GetCustomAttributes(typeof(ProtoMemberAttribute), true);

            if (attributes.Length == 0)
                return;

            Debug.Assert(attributes.Length == 1);

            var attribute = (ProtoMemberAttribute)attributes[0];

            result.Add(new ServiceTypeField(
                ReflectionOptimizer.BuildGetter(member),
                ReflectionOptimizer.BuildSetter(member, true),
                attribute.Tag,
                attribute.IsRequired,
                memberType
            ));
        }
    }
}
