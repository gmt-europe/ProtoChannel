describe('Streams', function () {
    it('can send stream', function () {
        runs(function () {
            var me = this;

            this.channel = createChannel(
                0,
                function () {
                    var me = this;
                    var aid = this.getSendStreamAid();

                    var body =
                        '--BOUNDARY\r\n' +
                        'Content-Disposition: file; filename="Upload.txt"\r\n' +
                        'Content-Type: text/plain\r\n' +
                        '\r\n' +
                        'Payload\r\n' +
                        '\r\n' +
                        '--BOUNDARY--';

                    new Ajax.Request(this.getStreamUrl(aid), {
                        postBody: body,
                        requestHeaders: {
                            'Content-type': 'multipart/form-data; boundary=BOUNDARY', /* prototype has small 't' for Content-type; need to repeat here */
                            'Content-Length': body.length
                        },
                        onSuccess: function () {
                            me.postMessage(new StreamResponse({ streamId: aid }));
                        }
                    });
                },
                function (message) { me.response = message; }
            );
        });

        waitsFor(function () { return this.response !== undefined; });

        runs(function () {
            this.channel.close();

            expect(this.response instanceof OneWayPing).toBeTruthy();
            expect(this.response.payload).toEqual('Received stream: Upload.txt, 9, text/plain');
        });
    });

    it('can receive stream', function () {
        runs(function () {
            var me = this;

            this.channel = createChannel(
                0,
                function () {
                    this.sendMessage(
                        new StreamRequest(),
                        function (message) {
                            new Ajax.Request(this.getStreamUrl(message.streamId), {
                                method: 'get',
                                onSuccess: function (transport) { me.response = transport.responseText; }
                            });
                        }
                    );
                }
           );
        });

        waitsFor(function () { return this.response !== undefined; });

        runs(function () {
            this.channel.close();

            expect(this.response).toEqual('This is a stream payload');
        });
    });
});
