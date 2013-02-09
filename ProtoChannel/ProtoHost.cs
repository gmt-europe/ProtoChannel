using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
#if !_NET_MD
using Common.Logging;
#endif

namespace ProtoChannel
{
    public abstract class ProtoHost : IDisposable
    {
#if !_NET_MD
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProtoHost<>));
#endif

        public const int ProtocolVersion = Constants.ProtocolVersion;

        private readonly object _syncRoot = new object();
        private TcpListener _listener;
        private bool _disposed;
        private readonly Dictionary<HostConnection, Client> _connections = new Dictionary<HostConnection, Client>();
        private readonly Dictionary<object, HostConnection> _clients = new Dictionary<object, HostConnection>();
        private readonly IStreamManager _streamManager;
        private AutoResetEvent _stateChangedEvent = new AutoResetEvent(false);
        private ProtoHostState _state;

        public ProtoHostState State
        {
            get { return _state; }
            private set
            {
                lock (_syncRoot)
                {
                    if (_state != value)
                    {
                        _state = value;

                        _stateChangedEvent.Set();
                    }
                }
            }
        }

        public IPEndPoint LocalEndPoint { get; set; }

        public ProtoHostConfiguration Configuration { get; private set; }

        public event UnhandledExceptionEventHandler UnhandledException;

        internal ServiceAssembly ServiceAssembly { get; private set; }

        internal Service Service { get; private set; }

        protected virtual void OnUnhandledException(UnhandledExceptionEventArgs e)
        {
            var ev = UnhandledException;

            if (ev != null)
                ev(this, e);
        }

        internal ProtoHost(IPEndPoint localEndPoint, Type serviceType, ProtoHostConfiguration configuration)
        {
            Require.NotNull(localEndPoint, "localEndPoint");
            Require.NotNull(serviceType, "serviceType");

            LocalEndPoint = localEndPoint;

            Configuration = configuration ?? new ProtoHostConfiguration();
            Configuration.Freeze();

            ServiceAssembly = ServiceRegistry.GetAssemblyRegistration(
                Configuration.ServiceAssembly ?? serviceType.Assembly
            );

            Service = ServiceAssembly.GetServiceRegistration(serviceType);

            _streamManager = Configuration.StreamManager ?? new MemoryStreamManager();

            Start();
        }

        private void Start()
        {
#if !_NET_MD
            Log.InfoFormat("Setting up host at {0}", LocalEndPoint);
#endif

            State = ProtoHostState.Listening;

            _listener = new TcpListener(LocalEndPoint);
            _listener.Start();

            LocalEndPoint = (IPEndPoint)_listener.LocalEndpoint;

            _listener.BeginAcceptTcpClient(AcceptTcpClientCallback, null);

#if !_NET_MD
            Log.InfoFormat("Listening for incoming connections at {0}", LocalEndPoint);
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void AcceptTcpClientCallback(IAsyncResult asyncResult)
        {
            lock (_syncRoot)
            {
                // Bail out if we're closing.

                if (State != ProtoHostState.Listening)
                    return;

                try
                {
                    var tcpClient = _listener.EndAcceptTcpClient(asyncResult);
                    HostConnection connection = null;

                    try
                    {
                        connection = new HostConnection(this, tcpClient, _streamManager);

                        _connections.Add(connection, null);

                        connection.Connect();
                    }
                    catch
                    {
                        if (connection != null)
                        {
                            _connections.Remove(connection);

                            connection.Dispose();
                        }

                        tcpClient.Close();

                        throw;
                    }
                }
                catch (Exception ex)
                {
#if !_NET_MD
                    Log.Info("Failed to accept TCP client", ex);
#endif
                }

                try
                {
                    _listener.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
                }
                catch (Exception ex)
                {
#if !_NET_MD
                    Log.Warn("BeginAcceptTcpClient failed", ex);
#endif

                    Close();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void RaiseUnhandledException(HostConnection connection, Exception exception)
        {
            Require.NotNull(connection, "connection");
            Require.NotNull(exception, "exception");

            try
            {
                OnUnhandledException(new UnhandledExceptionEventArgs(exception));
            }
            catch (Exception ex)
            {
#if !_NET_MD
                Log.Warn("Exception from UnhandledException event", ex);
#endif
            }

            try
            {
                connection.Dispose();
            }
            catch (Exception ex)
            {
#if !_NET_MD
                Log.Warn("Disposing connection failed", ex);
#endif
            }
        }

        internal void RemoveConnection(HostConnection connection)
        {
            Require.NotNull(connection, "connection");

            lock (_syncRoot)
            {
                Client hostClient;

                if (_connections.TryGetValue(connection, out hostClient))
                {
                    _connections.Remove(connection);

                    if (hostClient != null)
                    {
                        bool removed = _clients.Remove(hostClient.Instance);

                        Debug.Assert(removed);
                    }
                }

                // We progress to the closed state when all connections have
                // been closed.

                if (State == ProtoHostState.Closing && _connections.Count == 0)
                    State = ProtoHostState.Closed;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal Client RaiseClientConnected(HostConnection connection, int protocolNumber)
        {
            Require.NotNull(connection, "connection");

            lock (_syncRoot)
            {
                Debug.Assert(_connections.ContainsKey(connection) && _connections[connection] == null);

                object client = null;

                try
                {
                    client = CreateServiceCore(protocolNumber);
                }
                catch (Exception ex)
                {
#if !_NET_MD
                    Log.Error("Failed to create service", ex);
#endif
                }

                if (client != null)
                {
                    var hostClient = new Client(client, ServiceAssembly, Service);

                    _connections[connection] = hostClient;
                    _clients[client] = connection;

                    return hostClient;
                }
                else
                {
                    return null;
                }
            }
        }

        internal abstract object CreateServiceCore(int protocolNumber);

        public bool CloseClient(object client)
        {
            Require.NotNull(client, "client");

            HostConnection connection;

            lock (_syncRoot)
            {
                if (!_clients.TryGetValue(client, out connection))
                    return false;
            }

            connection.Dispose();

            return true;
        }

        public void Close()
        {
            Close(CloseMode.Gracefully, null);
        }

        public void Close(CloseMode mode)
        {
            Close(mode, null);
        }

        public void Close(CloseMode mode, TimeSpan timeout)
        {
            Close(mode, (TimeSpan?)timeout);
        }

        private void Close(CloseMode mode, TimeSpan? timeout)
        {
            var waitLimit =
                timeout.HasValue
                ? DateTime.Now + timeout.Value
                : (DateTime?)null;

            List<HostConnection> connectionsToDispose = null;

            lock (_syncRoot)
            {
                if (State == ProtoHostState.Closed)
                    return;

                if (State == ProtoHostState.Listening)
                {
                    _listener.Stop();

                    if (_connections.Count == 0)
                    {
                        State = ProtoHostState.Closed;
                    }
                    else
                    {
                        State = ProtoHostState.Closing;

                        if (mode == CloseMode.Abort)
                        {
                            // We need to dispose the connections outside of the
                            // lock because other processes may want to aquire
                            // the lock.

                            connectionsToDispose = new List<HostConnection>(_connections.Keys);
                        }
                    }
                }
            }

            if (connectionsToDispose != null)
            {
                foreach (var connection in connectionsToDispose)
                {
                    connection.Dispose();
                }
            }

            // Wait until we time out or we moved to a closed state.

            while (true)
            {
                lock (_syncRoot)
                {
                    if (State == ProtoHostState.Closed)
                        return;
                }

                if (waitLimit.HasValue)
                {
                    var wait = waitLimit.Value - DateTime.Now;

                    if (wait.Ticks <= 0)
                        return;

                    _stateChangedEvent.WaitOne(wait);
                }
                else
                {
                    _stateChangedEvent.WaitOne();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (State != ProtoHostState.Closed)
                    Close();

                if (_stateChangedEvent != null)
                {
                    _stateChangedEvent.Close();
                    _stateChangedEvent = null;
                }

                if (_listener != null)
                {
                    _listener.Stop();
                    _listener = null;
                }

                _disposed = true;
            }
        }
    }

    public class ProtoHost<T> : ProtoHost
        where T : class
    {
        public ProtoHost(IPEndPoint localEndPoint)
            : this(localEndPoint, null)
        {
        }

        public ProtoHost(IPEndPoint localEndPoint, ProtoHostConfiguration configuration)
            : base(localEndPoint, typeof(T), configuration)
        {
        }

        protected virtual T CreateService(int protocolNumber)
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

        internal override object CreateServiceCore(int protocolNumber)
        {
            return CreateService(protocolNumber);
        }
    }
}
