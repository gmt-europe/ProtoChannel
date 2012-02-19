describe('Setup', function () {
    it('can setup', function () {
        runs(function () {
            var me = this;

            this.connected = false;
            this.channel = createChannel(
                0,
                function () { me.connected = true; }
            );
        });

        waitsFor(function () { return this.connected; });

        runs(function () {
            this.channel.close();

            expect(this.connected).toBeTruthy();
        });
    });
});
