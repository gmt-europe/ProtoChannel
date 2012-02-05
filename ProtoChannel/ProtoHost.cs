using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ProtoChannel
{
    public class ProtoHost<T> : IDisposable
        where T : class, new()
    {
        private TcpListener _listener;
        private bool _closing;
        private bool _disposed;
        private readonly Dictionary<ProtoHostConnection<T>, T> _connections = new Dictionary<ProtoHostConnection<T>, T>();
        private readonly object _syncRoot = new object();
        private readonly IStreamManager _streamManager;
        private AutoResetEvent _connectionsChangedEvent = new AutoResetEvent(false);

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

        protected virtual T CreateService(int protocolNumber)
        {
            return new T();
        }

        public ProtoHost(IPEndPoint localEndPoint)
            : this(localEndPoint, null)
        {
        }

        public ProtoHost(IPEndPoint localEndPoint, ProtoHostConfiguration configuration)
        {
            LocalEndPoint = localEndPoint;

            Configuration = configuration ?? new ProtoHostConfiguration();
            Configuration.Freeze();

            ServiceAssembly = ServiceRegistry.GetAssemblyRegistration(
                Configuration.ServiceAssembly ?? typeof(T).Assembly
            );

            Service = ServiceAssembly.GetServiceRegistration(typeof(T));

            _streamManager = Configuration.StreamManager ?? new MemoryStreamManager();

            Start();
        }

        private void Start()
        {
            _listener = new TcpListener(LocalEndPoint);
            _listener.Start();

            LocalEndPoint = (IPEndPoint)_listener.LocalEndpoint;

            _listener.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
        }

        private void AcceptTcpClientCallback(IAsyncResult asyncResult)
        {
            if (_closing)
                return;

            TcpClient tcpClient;

            try
            {
                tcpClient = _listener.EndAcceptTcpClient(asyncResult);
            }
            catch
            {
                Dispose();
                return;
            }

            try
            {
                var connection = new ProtoHostConnection<T>(this, tcpClient, _streamManager);

                lock (_syncRoot)
                {
                    _connections.Add(connection, null);

                    _connectionsChangedEvent.Set();
                }
            }
            catch (Exception ex)
            {
                tcpClient.Close();

                OnUnhandledException(new UnhandledExceptionEventArgs(ex));
            }

            _listener.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
        }

        internal void RaiseUnhandledException(ProtoHostConnection<T> connection, Exception exception)
        {
            OnUnhandledException(new UnhandledExceptionEventArgs(exception));

            try
            {
                connection.Dispose();
            }
            catch (Exception ex)
            {
                OnUnhandledException(new UnhandledExceptionEventArgs(ex));
            }

            RemoveConnection(connection);
        }

        internal void RemoveConnection(ProtoHostConnection<T> connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            lock (_syncRoot)
            {
                _connections.Remove(connection);

                _connectionsChangedEvent.Set();
            }
        }

        internal T RaiseClientConnected(ProtoHostConnection<T> connection, int protocolNumber)
        {
            lock (_syncRoot)
            {
                Debug.Assert(_connections.ContainsKey(connection) && _connections[connection] == null);

                var client = CreateService(protocolNumber);

                _connections[connection] = client;

                return client;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (!_closing)
                {
                    _closing = true;

                    _listener.Stop();
                }

                while (true)
                {
                    lock (_syncRoot)
                    {
                        if (_connections.Count == 0)
                            break;
                    }

                    _connectionsChangedEvent.WaitOne();
                }

                _disposed = true;
            }
        }
    }
}
