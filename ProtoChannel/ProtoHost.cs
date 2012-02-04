using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

        public IPEndPoint LocalEndPoint { get; set; }

        public ProtoHostConfiguration Configuration { get; private set; }

        public UnhandledExceptionEventHandler UnhandledException;

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
                var connection = new ProtoHostConnection<T>(this, tcpClient);

                lock (_syncRoot)
                {
                    _connections.Add(connection, null);
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

        private void RemoveConnection(ProtoHostConnection<T> connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            lock (_syncRoot)
            {
                _connections.Remove(connection);
            }
        }

        internal void RaiseClientConnected(ProtoHostConnection<T> connection, int protocolNumber)
        {
            lock (_syncRoot)
            {
                Debug.Assert(_connections.ContainsKey(connection) && _connections[connection] == null);

                _connections[connection] = CreateService(protocolNumber);
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
                _disposed = true;
            }
        }
    }
}
