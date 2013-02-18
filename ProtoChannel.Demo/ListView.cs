using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ProtoChannel.Demo
{
    internal class ListView : System.Windows.Forms.ListView
    {
        public ListView()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }
    }
}
