ProtoChannel.prototype._processFailure = function () { throw 'Protocol failure'; };

function createChannel(protocolVersion, connectCallback, receiveCallback) {
    var pos = document.URL.indexOf('://');
    pos = document.URL.indexOf('/', pos + 3);
    var host = document.URL.substr(0, pos);

    if (protocolVersion === undefined)
        protocolVersion = 0;

    return new ServiceChannel(host, protocolVersion, connectCallback, receiveCallback);
};

function sendRequest(request) {
    var me = this;

    this.channel = createChannel(0, function () {
        this.sendMessage(
                request,
                function (message) { me.response = message; }
            );
    });
}

function waitForResponse() {
    return this.response !== undefined;
}

function getResponse() {
    this.channel.close();

    return this.response;
}
