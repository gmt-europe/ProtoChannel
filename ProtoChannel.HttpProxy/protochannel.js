// Entry point for "forever frame streaming".

function __mr(cid, did, message) {
    var channel = ProtoChannel._channels[cid];

    if (channel !== undefined)
        channel._processReceivedMessage(did, message);
};

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
        this._downstreamId = 0;
        this._lastDownstreamTime = null;
        this._closed = false;

        new Ajax.Request(
            this._getUrl('channel', { PVER: protocol }),
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

        ProtoChannel._channels[this._cid] = this;

        this._startDownstream();

        if (Object.isFunction(this._connectedCallback))
            this._connectedCallback.apply(this);
    },

    _processDownstreamProgress: function (transport, downstreamId) {
        if (this._downstreamId != downstreamId)
            return;

        while (true) {
            var pos;
            var length;

            try {
                pos = transport.responseText.indexOf('\n', this._downstreamOffset);

                if (pos == -1)
                    return;

                length = parseInt(transport.responseText.substr(this._downstreamOffset, pos - this._downstreamOffset), 10);

                if (length == 0) {
                    this._startDownstream();
                    return;
                }
            }
            catch (e) {
                return;
            }

            if (transport.responseText.length < pos + 1 + length)
                return;

            this._downstreamOffset = pos + 1 + length;

            var message = transport.responseText.substr(pos + 1, length).evalJSON(); ;

            this._processReceivedMessage(downstreamId, message);
        }
    },

    _processReceivedMessage: function (downstreamId, message) {
        if (this._downstreamId != downstreamId)
            return;

        if (message == null) {
            this._startDownstream();
            return;
        }

        // No-ops.

        if (Object.isArray(message))
            return;

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

        callback.apply(this, [deserialized]);
    },

    _processDownstreamCompleted: function (transport, downstreamId) {
        if (downstreamId == this._downstreamId)
            this._startDownstream();
    },

    _startDownstream: function () {
        if (this._closed)
            return;

        var lastDownstreamTime = this._lastDownstreamTime;
        this._lastDownstreamTime = new Date().getTime();

        if (lastDownstreamTime != null) {
            var diff = new Date(this._lastDownstreamTime - lastDownstreamTime);

            var seconds = diff.getHours() * 3600 + diff.getMinutes() * 60 + diff.getSeconds();

            if (seconds < 5) {
                this._processFailure();
                return;
            }
        }

        this._downstreamOffset = 0;
        var downstreamId = ++this._downstreamId;

        if (Prototype.Browser.IE)
            this._startIeDownstream(downstreamId);
        else
            this._startSaneDownstream(downstreamId);
    },

    _startIeDownstream: function (downstreamId) {
        if (this._downstreamFrame === undefined) {
            this._downstreamFrame = document.createElement('iframe');
            this._downstreamFrame.setAttribute('style', 'display:none');
            document.appendChild(this._downstreamFrame);
        }

        // DID query string parameter triggers the IFrame behavior.

        this._downstreamFrame.setAttribute('src', this._getUrl('channel', { CID: this._cid, DID: downstreamId }));
    },

    _startSaneDownstream: function (downstreamId) {
        var me = this;

        this._downstream = new Ajax.Request(
            this._getUrl('channel', { CID: this._cid }),
            {
                method: 'get',
                onInteractive: function (transport) { me._processDownstreamProgress(transport, downstreamId); },
                onSuccess: function (transport) { me._processDownstreamCompleted(transport, downstreamId); },
                onFailure: this._processFailure.bind(this)
            }
        );
    },

    sendMessage: function (message, callback) {
        this._verifyNotClosed();

        var aid = this._nextAid++;

        this._messages[aid] = callback;

        this._sendMessage(1 /* request */, message, aid);
    },

    postMessage: function (message) {
        this._verifyNotClosed();

        this._sendMessage(0 /* one way */, message, undefined);
    },

    _sendMessage: function (kind, message, aid) {
        if (!(message instanceof ProtoMessage))
            throw 'Message does not inherit from ProtoMessage';

        new Ajax.Request(
            this._getUrl('channel', { CID: this._cid }),
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
        this._verifyNotClosed();

        var aid = this._nextStreamAid++;

        if (this._nextStreamAid >= 0x1fffff /* max supported by the protocol */)
            this._nextStreamAid = 0;

        return aid;
    },

    getStreamUrl: function (aid, disposition) {
        this._verifyNotClosed();

        if (disposition === undefined)
            disposition = 'inline';

        return this._getUrl('stream', { CID: this._cid, AID: aid, disposition: disposition });
    },

    downloadStream: function (aid) {
        this._verifyNotClosed();

        if (this._downloadIframe === undefined) {
            this._downloadIframe = document.createElement('iframe');
            this._downloadIframe.setAttribute('style', 'display:none');

            document.body.appendChild(this._downloadIframe);
        }

        this._downloadIframe.setAttribute('src', this.getStreamUrl(aid, 'attachment'));
    },

    _getUrl: function (action, params) {
        var result =
            this._host + 'pchx/' + action +
            '?VER=' + encodeURIComponent(ProtoChannel._protocolVersion) +
            '&zx=' + encodeURIComponent(this._getZx());

        for (var key in params) {
            result += '&' + encodeURIComponent(key) + '=' + encodeURIComponent(params[key]);
        }

        return result;
    },

    _getZx: function () {
        var result = '';

        for (var i = 0; i < 8; i++) {
            var index = Math.floor(Math.random() * ProtoChannel._zxSeed.length);
            result += ProtoChannel._zxSeed.substr(index, 1);
        }

        return result;
    },

    close: function () {
        this._verifyNotClosed();

        this._closed = true;

        delete ProtoChannel._channels[this._cid];

        new Ajax.Request(
            this._getUrl('channel', { CID: this._cid }),
            {
                parameters: {
                    count: 1,
                    req0_key: Object.toJSON(['close'])
                },
                method: 'post',
                onFailure: this._processFailure.bind(this)
            }
        );
    },

    _verifyNotClosed: function () {
        if (this._closed)
            throw 'Channel has been closed';
    }
});

Object.extend(ProtoChannel, {
    _zxSeed: 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890',
    _protocolVersion: 1,
    _channels: {}
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

ProtoType = Class.create({
    initialize: function (values) {
        if (values !== undefined)
            Object.extend(this, values);
    },

    serialize: function () {
        throw 'Serialize not implemented';
    },

    deserialize: function () {
        throw 'Deserialize not implemented';
    }
});

ProtoMessage = Class.create(ProtoType, {
    initialize: function ($super, id, values) {
        this._id = id;

        $super(values);
    },

    getId: function () {
        return this._id;
    }
});
