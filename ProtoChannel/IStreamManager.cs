﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    public interface IStreamManager
    {
        Stream GetStream(ProtoStream stream);
    }
}
