using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProtoChannel.Web
{
    public class ProtoConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("host", DefaultValue = null, IsRequired = false)]
        public string Host
        {
            get { return (string)this["host"]; }
            set { this["host"] = value; }
        }

        [ConfigurationProperty("port", DefaultValue = -1, IsRequired = false)]
        public int Port
        {
            get { return (int)this["port"]; }
            set { this["port"] = value; }
        }

        [ConfigurationProperty("serviceAssembly", IsRequired = true)]
        public string ServiceAssembly
        {
            get { return (string)this["serviceAssembly"]; }
            set { this["serviceAssembly"] = value; }
        }

        [ConfigurationProperty("maxMessageSize", DefaultValue = -1, IsRequired = false)]
        public int MaxMessageSize
        {
            get { return (int)this["maxMessageSize"]; }
            set { this["maxMessageSize"] = value; }
        }

        [ConfigurationProperty("maxStreamSize", DefaultValue = -1, IsRequired = false)]
        public int MaxStreamSize
        {
            get { return (int)this["maxStreamSize"]; }
            set { this["maxStreamSize"] = value; }
        }

        [ConfigurationProperty("secure", DefaultValue = false, IsRequired = false)]
        public bool Secure
        {
            get { return (bool)this["secure"]; }
            set { this["secure"] = value; }
        }

        [ConfigurationProperty("skipCertificateValidations", DefaultValue = false, IsRequired = false)]
        public bool SkipCertificateValidations
        {
            get { return (bool)this["skipCertificateValidations"]; }
            set { this["skipCertificateValidations"] = value; }
        }

        [ConfigurationProperty("memoryStreamManager", IsRequired = false)]
        public MemoryStreamManagerSection MemoryStreamManager
        {
            get { return (MemoryStreamManagerSection)this["memoryStreamManager"]; }
            set { this["memoryStreamManager"] = value; }
        }

        [ConfigurationProperty("diskStreamManager", IsRequired = false)]
        public DiskStreamManagerSection DiskStreamManager
        {
            get { return (DiskStreamManagerSection)this["diskStreamManager"]; }
            set { this["diskStreamManager"] = value; }
        }

        [ConfigurationProperty("hybridStreamManager", IsRequired = false)]
        public HybridStreamManagerSection HybridStreamManager
        {
            get { return (HybridStreamManagerSection)this["hybridStreamManager"]; }
            set { this["hybridStreamManager"] = value; }
        }
    }

    public class MemoryStreamManagerSection : ConfigurationElement
    {
        [ConfigurationProperty("maxStreamSize", DefaultValue = -1, IsRequired = false)]
        public int MaxStreamSize
        {
            get { return (int)this["maxStreamSize"]; }
            set { this["maxStreamSize"] = value; }
        }
    }

    public class DiskStreamManagerSection : ConfigurationElement
    {
        [ConfigurationProperty("path", IsRequired = true)]
        public string Path
        {
            get { return (string)this["path"]; }
            set { this["path"] = value; }
        }

        [ConfigurationProperty("maxStreamSize", DefaultValue = -1, IsRequired = false)]
        public int MaxStreamSize
        {
            get { return (int)this["maxStreamSize"]; }
            set { this["maxStreamSize"] = value; }
        }
    }

    public class HybridStreamManagerSection : ConfigurationElement
    {
        [ConfigurationProperty("path", IsRequired = true)]
        public string Path
        {
            get { return (string)this["path"]; }
            set { this["path"] = value; }
        }

        [ConfigurationProperty("maxMemoryStreamSize", IsRequired = true)]
        public int MaxMemoryStreamSize
        {
            get { return (int)this["maxMemoryStreamSize"]; }
            set { this["maxMemoryStreamSize"] = value; }
        }

        [ConfigurationProperty("maxStreamSize", DefaultValue = -1, IsRequired = false)]
        public int MaxStreamSize
        {
            get { return (int)this["maxStreamSize"]; }
            set { this["maxStreamSize"] = value; }
        }
    }
}
