using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace ProtoChannel.Web
{
    internal class StreamUploadRequest : Request
    {
        private readonly ProtoProxyClient _client;
        private readonly int? _associationId;
        private readonly List<StreamWrapper> _streams = new List<StreamWrapper>();
        private readonly bool _inConstructor = true;
        private readonly object _syncRoot = new object();

        public StreamUploadRequest(HttpContext context, AsyncCallback asyncCallback, object extraData, ProtoProxyClient client, int? associationId)
            : base(context, asyncCallback, extraData)
        {
            Require.NotNull(client, "client");

            _client = client;
            _associationId = associationId;

            _client.Touch();

            if (context.Request.Files.Count == 0)
                SetAsCompleted(null, true);
            else
                HandleRequest();

            _inConstructor = false;
        }

        private void HandleRequest()
        {
            if (Context.Request.Files.Count > 1 && _associationId.HasValue)
                throw new HttpException("AID must be provided in request with multiple file uploads");

            for (int i = 0; i < Context.Request.Files.Count; i++)
            {
                var file = Context.Request.Files[i];

                string associationIdString = Context.Request.Form["AID_" + i];

                if (
                    associationIdString == null &&
                    (!_associationId.HasValue || Context.Request.Form.Count > 1)
                )
                    throw new HttpException("AID wasn't provided in the request");

                int associationId;

                if (_associationId.HasValue)
                    associationId = _associationId.Value;
                else if (!int.TryParse(associationIdString, out associationId))
                    throw new HttpException("Invalid AID");

                var wrappedStream = new StreamWrapper(file.InputStream);

                wrappedStream.Closed += wrappedStream_Closed;

                int responseId = _client.Client.SendStream(wrappedStream, file.FileName, file.ContentType, associationId);

                Debug.Assert(associationId == responseId);
            }
        }

        void wrappedStream_Closed(object sender, EventArgs e)
        {
            lock (_syncRoot)
            {
                var wrappedStream = (StreamWrapper)sender;

                wrappedStream.Closed -= wrappedStream_Closed;

                _streams.Remove(wrappedStream);

                if (_streams.Count == 0)
                    SetAsCompleted(null, _inConstructor);
            }
        }

        private class StreamWrapper : Stream
        {
            private bool _disposed;
            private Stream _inner;

            public event EventHandler Closed;

            private void OnClosed(EventArgs e)
            {
                var ev = Closed;
                if (ev != null)
                    ev(this, e);
            }

            public StreamWrapper(Stream inner)
            {
                _inner = inner;
            }

            public override void Close()
            {
                _inner.Close();

                OnClosed(EventArgs.Empty);
            }

            public override void Flush()
            {
                _inner.Flush();
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return _inner.BeginRead(buffer, offset, count, callback, state);
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                return _inner.EndRead(asyncResult);
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                throw new NotSupportedException();
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                throw new NotSupportedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _inner.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _inner.Read(buffer, offset, count);
            }

            public override int ReadByte()
            {
                return _inner.ReadByte();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override void WriteByte(byte value)
            {
                throw new NotSupportedException();
            }

            public override bool CanRead
            {
                get { return _inner.CanRead; }
            }

            public override bool CanSeek
            {
                get { return _inner.CanSeek; }
            }

            public override bool CanTimeout
            {
                get { return _inner.CanTimeout; }
            }

            public override bool CanWrite
            {
                get { return _inner.CanWrite; }
            }

            public override long Length
            {
                get { return _inner.Length; }
            }

            public override long Position
            {
                get { return _inner.Position; }
                set { _inner.Position = value; }
            }

            public override int ReadTimeout
            {
                get { return _inner.ReadTimeout; }
                set { _inner.ReadTimeout = value; }
            }

            public override int WriteTimeout
            {
                get { return _inner.WriteTimeout; }
                set { _inner.WriteTimeout = value; }
            }

            protected override void Dispose(bool disposing)
            {
                if (!_disposed && disposing)
                {
                    if (_inner != null)
                    {
                        _inner.Dispose();
                        _inner = null;
                    }

                    OnClosed(EventArgs.Empty);

                    _disposed = true;
                }

                base.Dispose(disposing);
            }
        }
    }
}
