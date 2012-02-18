using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class PendingReceiveStream : PendingStream
    {
        private bool _isCompleted;
        private Exception _completedException;
        private AsyncResultImpl<PendingReceiveStream> _asyncResult;

        public Stream Stream { get; private set; }

        public PendingReceiveStream(long length, string streamName, string contentType, uint associationId, Stream stream)
            : base(length, streamName, contentType, associationId)
        {
            Require.NotNull(stream, "stream");

            // Stream is not disposed. The receiving party is supposed to dispose
            // of the stream.

            Stream = stream;
        }

        public IAsyncResult GetAsyncResult(AsyncCallback callback, object asyncState)
        {
            if (_asyncResult != null)
                throw new ProtoChannelException("BeginGetStream cannot be called multiple times");

            _asyncResult = new AsyncResultImpl<PendingReceiveStream>(callback, asyncState);

            if (_isCompleted)
                CompleteAsyncResult(true);

            return _asyncResult;
        }

        public void SetAsCompleted()
        {
            Debug.Assert(!_isCompleted && _completedException == null);

            _isCompleted = true;

            if (_asyncResult != null)
                CompleteAsyncResult(false);
        }

        public void SetAsFailed(Exception exception)
        {
            Require.NotNull(exception, "exception");

            Debug.Assert(!_isCompleted && _completedException == null);

            _isCompleted = true;
            _completedException = exception;

            if (_asyncResult != null)
                CompleteAsyncResult(false);
        }

        private void CompleteAsyncResult(bool completedSynchronously)
        {
            if (_completedException != null)
                _asyncResult.SetAsCompleted(_completedException, completedSynchronously);
            else
                _asyncResult.SetAsCompleted(this, completedSynchronously);
        }

        public static ProtoStream EndGetStream(IAsyncResult asyncResult)
        {
            Require.NotNull(asyncResult, "asyncResult");

            var result = ((AsyncResultImpl<PendingReceiveStream>)asyncResult).EndInvoke();

            result.Stream.Position = 0;

            return new ProtoStream(
                result.Length, result.StreamName, result.ContentType, result.Stream
            );
        }
    }
}
