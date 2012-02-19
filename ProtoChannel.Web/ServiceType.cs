using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System;
using ProtoBuf;
using ProtoChannel.Util;
using ProtoChannel.Web.Util;

namespace ProtoChannel.Web
{
    internal class ServiceType
    {
        private readonly Dictionary<Type, IKeyedCollection<int, ServiceTypeField>> _fields = new Dictionary<Type, IKeyedCollection<int, ServiceTypeField>>();

        public ServiceMessage Message { get; private set; }

        public Type Type { get; private set; }

        public IKeyedCollection<int, ServiceTypeField> Fields { get; private set; }

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

        private IKeyedCollection<int, ServiceTypeField> BuildFields(Type type)
        {
            IKeyedCollection<int, ServiceTypeField> result;

            if (!_fields.TryGetValue(type, out result))
            {
                var fields = new ServiceTypeFieldCollection();

                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    AddMember(fields, field, field.FieldType);
                }

                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    AddMember(fields, property, property.PropertyType);
                }

                result = new ReadOnlyKeyedCollection<int, ServiceTypeField>(fields);

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

        private class ServiceTypeFieldCollection : KeyedCollection<int, ServiceTypeField>
        {
            protected override int GetKeyForItem(ServiceTypeField item)
            {
                return item.Tag;
            }
        }
    }
}
