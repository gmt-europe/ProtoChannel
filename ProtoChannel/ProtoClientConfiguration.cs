using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Security;
using System.Reflection;
using System.Text;

namespace ProtoChannel
{
    public class ProtoClientConfiguration
    {
        private bool _frozen;
        private bool _secure;
        private RemoteCertificateValidationCallback _validationCallback;
        private int _maxMessageSize;
        private int _maxStreamSize;
        private Assembly _serviceAssembly;
        private IStreamManager _streamManager;
        private object _callbackObject;
        private TimeSpan? _keepAlive = TimeSpan.FromSeconds(30);

        public ProtoClientConfiguration()
        {
            _maxMessageSize = 0x10000;
            _maxStreamSize = 0x400000;
        }

        public bool Secure
        {
            get { return _secure; }
            set
            {
                VerifyNotFrozen();

                _secure = value;
            }
        }

        public RemoteCertificateValidationCallback ValidationCallback
        {
            get { return _validationCallback; }
            set
            {
                VerifyNotFrozen();

                _validationCallback = value;
            }
        }

        [DefaultValue(65536)]
        public int MaxMessageSize
        {
            get { return _maxMessageSize; }
            set
            {
                VerifyNotFrozen();

                _maxMessageSize = value;
            }
        }

        [DefaultValue(4194304)]
        public int MaxStreamSize
        {
            get { return _maxStreamSize; }
            set
            {
                VerifyNotFrozen();

                _maxStreamSize = value;
            }
        }

        public Assembly ServiceAssembly
        {
            get { return _serviceAssembly; }
            set
            {
                VerifyNotFrozen();

                _serviceAssembly = value;
            }
        }

        public IStreamManager StreamManager
        {
            get { return _streamManager; }
            set
            {
                VerifyNotFrozen();

                _streamManager = value;
            }
        }

        public object CallbackObject
        {
            get { return _callbackObject; }
            set
            {
                VerifyNotFrozen();

                _callbackObject = value;
            }
        }

        public TimeSpan? KeepAlive
        {
            get { return _keepAlive; }
            set
            {
                VerifyNotFrozen();

                _keepAlive = value;
            }
        }

        internal void Freeze()
        {
            _frozen = true;
        }

        private void VerifyNotFrozen()
        {
            if (_frozen)
                throw new InvalidOperationException("Configuration cannot be modified after the host has been started");
        }
    }
}
