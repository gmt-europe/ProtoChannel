using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common.Logging;

namespace ProtoChannel
{
    public abstract class ProtoHost : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProtoHost<>));

        public const int ProtocolVersion = Constants.ProtocolVersion;

        private readonly object _syncRoot = new object();
        private TcpListener _listener;
        private bool _disposed;
        private readonly Dictionary<HostConnection, object> _connections = new Dictionary<HostConnection, object>();
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

        public UnhandledExceptionEventHandler UnhandledException;

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
            Log.InfoFormat("Setting up host at {0}", LocalEndPoint);

            State = ProtoHostState.Listening;

            _listener = new TcpListener(LocalEndPoint);
            _listener.Start();

            LocalEndPoint = (IPEndPoint)_listener.LocalEndpoint;

            _listener.BeginAcceptTcpClient(AcceptTcpClientCallback, null);

            Log.InfoFormat("Listening for incoming connections at {0}", LocalEndPoint);
        }

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

                    try
                    {
                        var connection = new HostConnection(this, tcpClient, _streamManager);

                        _connections.Add(connection, null);
                    }
                    catch
                    {
                        tcpClient.Close();

                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Log.Info("Failed to accept TCP client", ex);
                }

                try
                {
                    _listener.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
                }
                catch (Exception ex)
                {
                    Log.Warn("BeginAcceptTcpClient failed", ex);

                    Close();
                }
            }
        }

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
                Log.Warn("Exception from UnhandledException event", ex);
            }

            try
            {
                connection.Dispose();
            }
            catch (Exception ex)
            {
                Log.Warn("Disposing connection failed", ex);
            }
        }

        internal void RemoveConnection(HostConnection connection)
        {
            Require.NotNull(connection, "connection");

            lock (_syncRoot)
            {
                _connections.Remove(connection);

                // We progress to the closed state when all connections have
                // been closed.

                if (State == ProtoHostState.Closing && _connections.Count == 0)
                    State = ProtoHostState.Closed;
            }
        }

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
                    Log.Error("Failed to create service", ex);
                }

                if (client != null)
                {
                    var hostClient = new Client(client, ServiceAssembly, Service);

                    _connections[connection] = hostClient;

                    return hostClient;
                }
                else
                {
                    return null;
                }
            }
        }

        internal abstract object CreateServiceCore(int protocolNumber);

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
                            // Create a copy of the collection because disposing
                            // the connection will remove it from the
                            // _connections list.

                            var connections = new List<HostConnection>(_connections.Keys);

                            foreach (var connection in connections)
                            {
                                connection.Dispose();
                            }
                        }
                    }
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
            if (!_disposed)
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
