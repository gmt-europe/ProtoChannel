using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.CodeGenerator
{
    internal class Argument
    {
        public bool IsMandatory { get; private set; }
        public string Flag { get; private set; }
        public string Description { get; private set; }
        public bool IsProvided { get; private set; }

        public Argument(string flag, string description)
            : this(false, flag, description)
        {
        }

        protected Argument(bool isMandatory, string flag, string description)
        {
            IsMandatory = isMandatory;
            Flag = flag;
            Description = description;
        }

        public virtual void Parse(string[] args, ref int offset)
        {
            IsProvided = true;
        }
    }

    internal class Argument<T> : Argument
    {
        public T Value { get; private set; }

        public Argument(bool isMandatory, string flag, string description)
            : base(isMandatory, flag, description)
        {
        }

        public override void Parse(string[] args, ref int offset)
        {
            string value = args[offset++];

            try
            {
                Value = (T)Convert.ChangeType(value, typeof(T));

                base.Parse(args, ref offset);
            }
            catch
            {
                throw new CommandLineArgumentException(String.Format("Cannot parse value '{0}' for argument '{1}'", value, Flag));
            }
        }
    }
}
