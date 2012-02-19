describe('Messages', function () {
    it('can receive result from ping', function () {
        runs(function () {
            var me = this;

            this.channel = createChannel(0, function () {
                this.sendMessage(
                    new Ping({ payload: 'Payload' }),
                    function (message) { me.response = message; }
                );
            });
        });

        waitsFor(function () { return this.response !== undefined; });

        runs(function () {
            this.channel.close();

            expect(this.response instanceof Pong).toBeTruthy();
            expect(this.response.payload).toEqual('Payload');
        });
    });

    it('can receive result from one way ping', function () {
        runs(function () {
            var me = this;

            this.channel = createChannel(
                0,
                function () {
                    this.postMessage(
                        new OneWayPing({ payload: 'Payload' })
                    );
                },
                function (message) { me.response = message; }
            );
        });

        waitsFor(function () { return this.response !== undefined; });

        runs(function () {
            this.channel.close();

            expect(this.response instanceof OneWayPing).toBeTruthy();
            expect(this.response.payload).toEqual('Payload');
        });
    });
});
