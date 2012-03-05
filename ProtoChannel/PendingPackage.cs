using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel
{
    internal struct PendingPackage
    {
        private readonly PackageType _type;
        private readonly uint _length;

        public PendingPackage(PackageType type, uint length)
        {
            _type = type;
            _length = length;
        }

        public PackageType Type
        {
            get { return _type; }
        }

        public uint Length
        {
            get { return _length; }
        }
    }
}
