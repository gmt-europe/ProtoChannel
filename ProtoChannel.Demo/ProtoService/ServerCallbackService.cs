﻿﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProtoChannel.Demo.ProtoService
{
    [global::System.CodeDom.Compiler.GeneratedCode("codegen", "1.0.1.0")]
    internal partial class ServerCallbackService : global::ProtoChannel.ProtoCallbackChannel
    {
        [global::System.Diagnostics.DebuggerStepThrough]
        public void StreamReceivedMessage(global::ProtoChannel.Demo.Shared.StreamReceivedMessage message)
        {
            PostMessage(message);
        }
    }
}
