using System.Text;
using System.Collections.Generic;
using System;
using System.Threading;

namespace ProtoChannel.Util
{
    internal class AsyncResultImpl : IAsyncResult
    {
        // Taken from http://msdn.microsoft.com/en-us/magazine/cc163467.aspx.

        // Fields set at construction which never change while 
        // operation is pending
        private readonly AsyncCallback _callback;
        private readonly object _asyncState;

        // Fields set at construction which do change after 
        // operation completes
        private const int StatePending = 0;
        private const int StateCompletedSynchronously = 1;
        private const int StateCompletedAsynchronously = 2;

        private int _completedState = StatePending;

        // Field that may or may not get set depending on usage
        private ManualResetEvent _waitHandle;

        // Fields set when operation completes
        private Exception _exception;

        public AsyncResultImpl(AsyncCallback callback, object state)
        {
            _callback = callback;
            _asyncState = state;
        }

        public void SetAsCompleted(Exception exception, bool completedSynchronously)
        {
            // Passing null for exception means no error occurred. 
            // This is the common case
            _exception = exception;

            // The _completedState field MUST be set prior calling the callback
            int prevState = Interlocked.Exchange(
                ref _completedState,
                completedSynchronously ? StateCompletedSynchronously : StateCompletedAsynchronously
            );

            if (prevState != StatePending)
                throw new InvalidOperationException("You can set a result only once");

            // If the event exists, set it
            if (_waitHandle != null)
                _waitHandle.Set();

            // If a callback method was set, call it
            if (_callback != null)
                _callback(this);
        }

        public void EndInvoke()
        {
            // This method assumes that only 1 thread calls EndInvoke 
            // for this object
            if (!IsCompleted)
            {
                // If the operation isn't done, wait for it
                AsyncWaitHandle.WaitOne();
                AsyncWaitHandle.Close();

                _waitHandle = null;  // Allow early GC
            }

            // Operation is done: if an exception occured, throw it
            if (_exception != null)
                throw _exception;
        }

        public object AsyncState
        {
            get { return _asyncState; }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return Thread.VolatileRead(ref _completedState) == StateCompletedSynchronously;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (_waitHandle == null)
                {
                    bool done = IsCompleted;

                    var mre = new ManualResetEvent(done);

                    if (Interlocked.CompareExchange(ref _waitHandle, mre, null) != null)
                    {
                        // Another thread created this object's event; dispose 
                        // the event we just created
                        mre.Close();
                    }
                    else
                    {
                        if (!done && IsCompleted)
                        {
                            // If the operation wasn't done when we created 
                            // the event but now it is done, set the event
                            _waitHandle.Set();
                        }
                    }
                }

                return _waitHandle;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return Thread.VolatileRead(ref _completedState) !=
                    StatePending;
            }
        }
    }

    internal class AsyncResultImpl<TResult> : AsyncResultImpl
    {
        // Taken from http://msdn.microsoft.com/en-us/magazine/cc163467.aspx.

        // Field set when operation completes
        private TResult _result;

        public AsyncResultImpl(AsyncCallback callback, object asyncState)
            : base(callback, asyncState)
        {
        }

        public void SetAsCompleted(TResult result, bool completedSynchronously)
        {
            // Save the asynchronous operation's result
            _result = result;

            // Tell the base class that the operation completed 
            // successfully (no exception)

            SetAsCompleted(null, completedSynchronously);
        }

        new public TResult EndInvoke()
        {
            base.EndInvoke(); // Wait until operation has completed 

            return _result;  // Return the result (if above didn't throw)
        }
    }
}
