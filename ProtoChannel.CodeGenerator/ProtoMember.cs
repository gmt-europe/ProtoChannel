using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.CodeGenerator
{
    internal class ProtoMember
    {
        public string Name { get; private set; }
        public int Tag { get; private set; }
        public bool IsRequired { get; private set; }
        public object DefaultValue { get; private set; }
        public Type Type { get; private set; }
        public bool IsCollection { get; private set; }

        public ProtoMember(string name, object defaultValue, Type type, int tag, bool isRequired)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
            DefaultValue = defaultValue;
            Tag = tag;
            IsRequired = isRequired;

            var collectionType = TypeUtil.GetCollectionType(type);

            Type = collectionType ?? type;
            IsCollection = collectionType != null;
        }
    }
}
