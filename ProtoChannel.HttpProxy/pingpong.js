﻿Ping = Class.create(ProtoMessage, {
    initialize: function ($super, values) {
        this.payload = null;

        $super(1, values);
    },

    serialize: function () {
        var message = {};

        message[1] = this.payload;

        return message;
    },

    deserialize: function (message) {
        this.payload = message[1];
    }
});

ProtoRegistry.registerType(Ping, 1);

Pong = Class.create(ProtoMessage, {
    initialize: function ($super, values) {
        this.payload = null;

        $super(2, values);
    },

    serialize: function () {
        var message = {};

        message[1] = this.payload;

        return message;
    },

    deserialize: function (message) {
        this.payload = message[1];
    }
});

ProtoRegistry.registerType(Pong, 2);

StreamRequest = Class.create(ProtoMessage, {
    initialize: function ($super, values) {
        $super(3, values);
    },

    serialize: function () {
        var message = {};

        return message;
    },

    deserialize: function (message) {
        
    }
});

ProtoRegistry.registerType(StreamRequest, 3);

StreamResponse = Class.create(ProtoMessage, {
    initialize: function ($super, values) {
        this.streamId = null;

        $super(4, values);
    },

    serialize: function () {
        var message = {};

        message[1] = this.streamId;

        return message;
    },

    deserialize: function (message) {
        this.streamId = message[1];
    }
});

ProtoRegistry.registerType(StreamResponse, 4);

OneWayPing = Class.create(ProtoMessage, {
    initialize: function ($super, values) {
        this.payload = null;

        $super(5, values);
    },

    serialize: function () {
        var message = {};

        message[1] = this.payload;

        return message;
    },

    deserialize: function (message) {
        this.payload = message[1];
    }
});

ProtoRegistry.registerType(OneWayPing, 5);