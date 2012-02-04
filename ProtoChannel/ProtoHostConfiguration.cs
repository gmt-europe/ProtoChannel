using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ProtoChannel
{
    public class ProtoHostConfiguration
    {
        private bool _frozen;
        private bool _secure;
        private X509Certificate _certificate;
        private RemoteCertificateValidationCallback _validationCallback;
        private int _maxMessageSize;
        private int _maxStreamSize;
        private int _minimumProtocolNumber;
        private int _maximumProtocolNumber;
        private Assembly _serviceAssembly;

        public ProtoHostConfiguration()
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

        public X509Certificate Certificate
        {
            get { return _certificate; }
            set
            {
                VerifyNotFrozen();

                _certificate = value;
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

        public int MinimumProtocolNumber
        {
            get { return _minimumProtocolNumber; }
            set
            {
                VerifyNotFrozen();

                _minimumProtocolNumber = value;
            }
        }

        public int MaximumProtocolNumber
        {
            get { return _maximumProtocolNumber; }
            set
            {
                VerifyNotFrozen();

                _maximumProtocolNumber = value;
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

        internal void Freeze()
        {
            if (_minimumProtocolNumber < 0 || _maximumProtocolNumber < 0)
                throw new ProtoChannelException("Minimum and maximum protocol numbers must be greater than zero");
            if (_minimumProtocolNumber > _maximumProtocolNumber)
                throw new ProtoChannelException("Minimum protocol number must be less than or equal to the maximum protocol number");

            _frozen = true;
        }

        private void VerifyNotFrozen()
        {
            if (_frozen)
                throw new InvalidOperationException("Configuration cannot be modified after the host has been started");
        }
    }
}
