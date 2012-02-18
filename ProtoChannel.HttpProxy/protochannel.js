ProtoChannel = Class.create({
    initialize: function (host, protocol, connectedCallback, receiveCallback) {
        if (host.substr(-1) != '/')
            host = host + '/';

        this._host = host;
        this._nextAid = 0;
        this._nextStreamAid = 0;
        this._messages = {};
        this._connectedCallback = connectedCallback;
        this._receiveCallback = receiveCallback;

        new Ajax.Request(
            this._host + 'pchx/channel?VER=1&PVER=' + encodeURIComponent(protocol),
            {
                method: 'get',
                onSuccess: this._processConnect.bind(this),
                onFailure: this._processFailure.bind(this)
            }
        );
    },

    _processFailure: function () {
        alert('Protocol failure');
    },

    _processConnect: function (transport) {
        var json = transport.responseJSON;

        if (json.c === undefined)
            this._processFailure(transport);

        this._cid = json.c;

        this._startDownstream();

        if (Object.isFunction(this._connectedCallback))
            this._connectedCallback.apply(this);
    },

    _processDownstreamProgress: function (transport) {
        while (true) {
            var pos = transport.responseText.indexOf('\n', this._downstreamOffset);

            if (pos == -1)
                return;

            var length = parseInt(transport.responseText.substr(this._downstreamOffset, pos - this._downstreamOffset), 10);

            if (transport.responseText.length < pos + 1 + length)
                return;

            this._downstreamOffset = pos + 1 + length;

            var message = transport.responseText.substr(pos + 1, length).evalJSON();

            this._processReceivedMessage(message);
        }
    },

    _processReceivedMessage: function (message) {
        var type = ProtoRegistry.getMessageType(message.t);

        if (type === undefined)
            throw 'Invalid message type; no message registered';

        var deserialized = new type;

        deserialized.deserialize(message.p);

        switch (message.r) {
            case 0: this._processOneWayMessage(message, deserialized); break;
            case 1: this._processRequestMessage(message, deserialized); break;
            case 2: this._processResponseMessage(message, deserialized); break;
        }
    },

    _processOneWayMessage: function (message, deserialized) {
        if (!Object.isFunction(this._receiveCallback))
            throw 'Message received but callback not provided';

        this._receiveCallback(deserialized, false);
    },

    _processRequestMessage: function (message, deserialized) {
        if (!Object.isFunction(this._receiveCallback))
            throw 'Message received but callback not provided';

        var response = this._receiveCallback(deserialized, true);

        this._sendMessage(2 /* response */, response, message.a);
    },

    _processResponseMessage: function (message, deserialized) {
        var callback = this._messages[message.a];

        delete this._messages[message.a];

        callback(deserialized);
    },

    _processDownstreamCompleted: function () {
        this._startDownstream();
    },

    _startDownstream: function () {
        this._downstreamOffset = 0;

        new Ajax.Request(
            this._host + 'pchx/channel?VER=1&CID=' + encodeURIComponent(this._cid),
            {
                method: 'get',
                onInteractive: this._processDownstreamProgress.bind(this),
                onSuccess: this._processDownstreamCompleted.bind(this),
                onFailure: this._processFailure.bind(this)
            }
        );
    },

    sendMessage: function (message, callback) {
        var aid = this._nextAid++;

        this._messages[aid] = callback;

        this._sendMessage(1 /* request */, message, aid);
    },

    postMessage: function (message) {
        this._sendMessage(0 /* one way */, message, undefined);
    },

    _sendMessage: function (kind, message, aid) {
        if (!(message instanceof ProtoMessage))
            throw 'Message does not inherit from ProtoMessage';

        new Ajax.Request(
            this._host + 'pchx/channel?VER=1&CID=' + encodeURIComponent(this._cid),
            {
                parameters: {
                    count: 1,
                    req0_key: Object.toJSON({
                        r: kind,
                        a: aid,
                        t: message.getId(),
                        p: message.serialize()
                    })
                },
                method: 'post',
                onFailure: this._processFailure.bind(this)
            }
        );
    },

    getSendStreamAid: function () {
        var aid = this._nextStreamAid++;

        if (this._nextStreamAid > 0x1fffff /* max supported by the protocol */)
            this._nextStreamAid = 0;

        return aid;
    },

    getStreamUrl: function (aid) {
        return this._host + 'pchx/stream?VER=1&CID=' + encodeURIComponent(this._cid) + '&AID=' + encodeURIComponent(aid);
    }
});

ProtoRegistry = {
    _registeredMessages: {},

    registerType: function (type, id) {
        if (type.superclass != ProtoMessage)
            throw 'Expected type to inherit from ProtoMessage';

        ProtoRegistry._registeredMessages[id] = type;
    },

    getMessageType: function (id) {
        return ProtoRegistry._registeredMessages[id];
    }
};

ProtoMessage = Class.create({
    initialize: function (id, values) {
        this._id = id;

        if (values !== undefined)
            Object.extend(this, values);
    },

    getId: function () {
        return this._id;
    },

    serialize: function () {
        throw 'Serialize not implemented';
    },

    deserialize: function () {
        throw 'Deserialize not implemented';
    }
});
