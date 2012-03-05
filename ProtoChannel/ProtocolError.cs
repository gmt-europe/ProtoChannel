using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel
{
    public enum ProtocolError
    {
        UnknownError = 0,
        InvalidPackageType = 1,
        ProtocolUnsupported = 2,
        InvalidPackageLength = 3,
        UnexpectedStreamPackageType = 4,
        InvalidStreamData = 5,
        InvalidStreamPackageType = 6,
        InvalidProtocolHeader = 7,
        InvalidProtocol = 8,
        InvalidMessageKind,
        InvalidMessageType,
        ExpectedIsOneWay,
        ExpectedRequest,
        UnexpectedMessageType,
        InvalidStreamAssociationId,
        CannotProcessRequestMessage,
        ErrorProcessingRequest
    }
}
