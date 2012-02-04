using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ProtoChannel
{
    public class ProtoClientConfiguration
    {
        private bool _frozen;
        private bool _secure;
        private X509Certificate _certificate;
        private RemoteCertificateValidationCallback _validationCallback;
        private int _maxMessageSize;
        private int _maxStreamSize;
        private string _targetHost;

        public ProtoClientConfiguration()
        {
            _maxMessageSize = 65536;
            _maxStreamSize = 4194304;
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

        public string TargetHost
        {
            get { return _targetHost; }
            set
            {
                VerifyNotFrozen();

                _targetHost = value;
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
