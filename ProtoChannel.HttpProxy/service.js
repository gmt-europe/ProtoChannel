﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

DefaultValueTests = Class.create(ProtoMessage, {
    initialize: function ($super, values) {
        this.stringValue = 'Default value';
        this.intValue = 1;
        this.doubleValue = 1.0;

        $super(6, values);
    },

    serialize: function () {
        var message = {};

        if (this.stringValue !== 'Default value')
            message[1] = this.stringValue;
        if (this.intValue !== 1)
            message[2] = this.intValue;
        if (this.doubleValue !== 1.0)
            message[3] = this.doubleValue;

        return message;
    },

    deserialize: function (message) {
        if (message[1] !== undefined)
            this.stringValue = message[1];
        if (message[2] !== undefined)
            this.intValue = message[2];
        if (message[3] !== undefined)
            this.doubleValue = message[3];
    }
});

ProtoRegistry.registerType(DefaultValueTests, 6);

IntArrayTest = Class.create(ProtoMessage, {
    initialize: function ($super, values) {
        this.values = null;

        $super(8, values);
    },

    serialize: function () {
        var message = {};

        if (this.values !== null)
            message[1] = this.values;

        return message;
    },

    deserialize: function (message) {
        if (message[1] !== undefined)
            this.values = message[1];
    }
});

ProtoRegistry.registerType(IntArrayTest, 8);

NestedType = Class.create(ProtoType, {
    initialize: function ($super, values) {
        this.value = null;

        $super(values);
    },

    serialize: function () {
        var message = {};

        if (this.value !== null)
            message[1] = this.value;

        return message;
    },

    deserialize: function (message) {
        if (message[1] !== undefined)
            this.value = message[1];
    }
});

NestedTypeArrayTest = Class.create(ProtoMessage, {
    initialize: function ($super, values) {
        this.values = null;

        $super(10, values);
    },

    serialize: function () {
        var message = {};

        if (this.values !== null)
            message[1] = this.values;

        return message;
    },

    deserialize: function (message) {
        if (message[1] !== undefined)
            this.values = message[1];
    }
});

ProtoRegistry.registerType(NestedTypeArrayTest, 10);

NestedTypeTest = Class.create(ProtoMessage, {
    initialize: function ($super, values) {
        this.value = null;

        $super(9, values);
    },

    serialize: function () {
        var message = {};

        if (this.value !== null)
            message[1] = this.value;

        return message;
    },

    deserialize: function (message) {
        if (message[1] !== undefined)
            this.value = message[1];
    }
});

ProtoRegistry.registerType(NestedTypeTest, 9);

OneWayPing = Class.create(ProtoMessage, {
    initialize: function ($super, values) {
        this.payload = null;

        $super(5, values);
    },

    serialize: function () {
        var message = {};

        if (this.payload !== null)
            message[1] = this.payload;

        return message;
    },

    deserialize: function (message) {
        if (message[1] !== undefined)
            this.payload = message[1];
    }
});

ProtoRegistry.registerType(OneWayPing, 5);

Ping = Class.create(ProtoMessage, {
    initialize: function ($super, values) {
        this.payload = null;

        $super(1, values);
    },

    serialize: function () {
        var message = {};

        if (this.payload !== null)
            message[1] = this.payload;

        return message;
    },

    deserialize: function (message) {
        if (message[1] !== undefined)
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

        if (this.payload !== null)
            message[1] = this.payload;

        return message;
    },

    deserialize: function (message) {
        if (message[1] !== undefined)
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
        this.streamId = 0;

        $super(4, values);
    },

    serialize: function () {
        var message = {};

        if (this.streamId !== 0)
            message[1] = this.streamId;

        return message;
    },

    deserialize: function (message) {
        if (message[1] !== undefined)
            this.streamId = message[1];
    }
});

ProtoRegistry.registerType(StreamResponse, 4);

StringArrayTest = Class.create(ProtoMessage, {
    initialize: function ($super, values) {
        this.values = null;

        $super(7, values);
    },

    serialize: function () {
        var message = {};

        if (this.values !== null)
            message[1] = this.values;

        return message;
    },

    deserialize: function (message) {
        if (message[1] !== undefined)
            this.values = message[1];
    }
});

ProtoRegistry.registerType(StringArrayTest, 7);

ServiceChannel = Class.create(ProtoChannel, {
    ping: function (message, callback) {
        if (!(message instanceof Ping))
            message = new Ping(message);

        this.sendMessage(message, callback);
    },

    oneWayPing: function (message) {
        if (!(message instanceof OneWayPing))
            message = new OneWayPing(message);

        this.postMessage(message);
    }
});