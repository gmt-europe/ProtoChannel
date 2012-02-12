using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    internal struct StreamSendRequest
    {
        private readonly PendingSendStream _stream;
        private readonly long _length;

        public StreamSendRequest(PendingSendStream stream, long length)
        {
            Require.NotNull(stream, "stream");

            _stream = stream;
            _length = length;
        }

        public PendingSendStream Stream
        {
            get { return _stream; }
        }

        public long Length
        {
            get { return _length; }
        }

        public bool IsCompleted
        {
            get { return _length == 0; }
        }
    }
}
