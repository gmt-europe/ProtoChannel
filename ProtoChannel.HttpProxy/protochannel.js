ProtoChannel = Class.create({
    initialize: function (host, protocol, connectedCallback) {
        if (host.substr(-1) != '/')
            host = host + '/';

        this._host = host;
        this._nextAid = 1;
        this._messages = {};
        this._connectedCallback = connectedCallback;

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

        if (this._connectedCallback !== undefined)
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
        var callback = this._messages[message.a];

        delete this._messages[message.a];

        callback(message.p);
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

    sendMessage: function (message, type, callback) {
        var aid = this._nextAid++;

        this._messages[aid] = callback;

        new Ajax.Request(
            this._host + 'pchx/channel?VER=1&CID=' + encodeURIComponent(this._cid),
            {
                parameters: {
                    count: 1,
                    req0_key: Object.toJSON({
                        r: 1 /* request */,
                        a: aid,
                        t: type,
                        p: message
                    })
                },
                method: 'post',
                onFailure: this._processFailure.bind(this)
            }
        );
    },

    postMessage: function (message) {
    },

    sendStream: function (stream) {
    },

    getStream: function (associationId, callback) {
    }
});
