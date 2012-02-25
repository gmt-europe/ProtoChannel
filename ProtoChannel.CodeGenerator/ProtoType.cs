using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProtoChannel.CodeGenerator
{
    internal class ProtoType
    {
        public Type Type { get; private set; }
        public int? MessageId { get; private set; }
        public IList<ProtoMember> Members { get; private set; }

        public ProtoType(Type type)
            : this(type, null)
        {
        }

        public ProtoType(Type type, int? messageId)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            Type = type;
            MessageId = messageId;

            var members = new List<ProtoMember>();

            foreach (var property in type.GetProperties())
            {
                var attribute = GetProtoMemberAttribute(property);

                if (attribute == null)
                    continue;

                members.Add(new ProtoMember(
                    property.Name,
                    ((dynamic)attribute).Tag,
                    ((dynamic)attribute).IsRequired
                ));
            }

            foreach (var field in type.GetFields())
            {
                var attribute = GetProtoMemberAttribute(field);

                if (attribute == null)
                    continue;

                members.Add(new ProtoMember(
                    field.Name,
                    ((dynamic)attribute).Tag,
                    ((dynamic)attribute).IsRequired
                ));
            }

            members.Sort((a, b) => a.Tag.CompareTo(b.Tag));

            Members = new ReadOnlyCollection<ProtoMember>(members);
        }

        private object GetProtoMemberAttribute(ICustomAttributeProvider member)
        {
            foreach (var attribute in member.GetCustomAttributes(true))
            {
                if (attribute.GetType().FullName == "ProtoBuf.ProtoMemberAttribute")
                    return attribute;
            }

            return null;
        }
    }
}
