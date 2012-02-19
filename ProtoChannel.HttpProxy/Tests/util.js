ProtoChannel.prototype._processFailure = function () { throw 'Protocol failure'; };

function createChannel(protocolVersion, connectCallback, receiveCallback) {
    var pos = document.URL.indexOf('://');
    pos = document.URL.indexOf('/', pos + 3);
    var host = document.URL.substr(0, pos);

    if (protocolVersion === undefined)
        protocolVersion = 0;

    return new ProtoChannel(host, protocolVersion, connectCallback, receiveCallback);
};
