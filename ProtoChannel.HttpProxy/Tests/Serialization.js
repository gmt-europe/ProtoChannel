describe('Serialization', function () {
    it('default values aren\'t serialized', function () {
        var message = new DefaultValueTests();

        expect(Object.keys(message.serialize()).length).toEqual(0);
    });

    it('default values from parameters aren\'t serialized', function () {
        var message = new DefaultValueTests({ stringValue: 'Default value' });

        expect(Object.keys(message.serialize()).length).toEqual(0);
    });

    it('non default values are serialized', function () {
        var message = new DefaultValueTests({ stringValue: null });
        var serialized = message.serialize();

        expect(Object.keys(serialized).length).toEqual(1);
        expect(serialized[1]).toEqual(null);
    });

    it('string arrays are serialized', function () {
        var message = new StringArrayTest({ values: ['a', 'b'] });
        var serialized = message.serialize();

        expect(serialized[1].length).toEqual(2);
        expect(serialized[1][0]).toEqual('a');
        expect(serialized[1][1]).toEqual('b');
    });

    it('nested types are serialized', function () {
        var message = new NestedTypeTest();

        message.value = new NestedType({ value: 'value' });

        var serialized = message.serialize();

        expect(Object.keys(serialized[1]).length).toEqual(1);
        expect(serialized[1][1]).toEqual('value');
    });

    it('nested types are wrapped', function () {
        var message = new NestedTypeTest();

        message.value = { value: 'value' };

        var serialized = message.serialize();

        expect(Object.keys(serialized[1]).length).toEqual(1);
        expect(serialized[1][1]).toEqual('value');
    });

    it('nested type arrays are serialized', function () {
        var message = new NestedTypeArrayTest();

        message.values.push(new NestedType({ value: 'a' }));
        message.values.push(new NestedType({ value: 'b' }));

        var serialized = message.serialize();

        expect(serialized[1].length).toEqual(2);
        expect(serialized[1][0][1]).toEqual('a');
        expect(serialized[1][1][1]).toEqual('b');
    });

    it('string array roundtrip', function () {
        runs(function () {
            sendRequest.apply(this, [new StringArrayTest({ values: ['a', 'b'] })]);
        });

        waitsFor(waitForResponse);

        runs(function () {
            var response = getResponse.apply(this);

            expect(response instanceof StringArrayTest).toBeTruthy();
            expect(response.values.length).toEqual(2);
            expect(response.values[0]).toEqual('a');
            expect(response.values[1]).toEqual('b');
        });
    });

    it('int array roundtrip', function () {
        runs(function () {
            sendRequest.apply(this, [new IntArrayTest({ values: [1, 2] })]);
        });

        waitsFor(waitForResponse);

        runs(function () {
            var response = getResponse.apply(this);

            expect(response instanceof IntArrayTest).toBeTruthy();
            expect(response.values.length).toEqual(2);
            expect(response.values[0]).toEqual(1);
            expect(response.values[1]).toEqual(2);
        });
    });

    it('default values roundtrip', function () {
        runs(function () {
            sendRequest.apply(this, [new DefaultValueTests()]);
        });

        waitsFor(waitForResponse);

        runs(function () {
            var response = getResponse.apply(this);

            expect(response.stringValue).toEqual('Default value');
            expect(response.intValue).toEqual(1);
            expect(response.doubleValue).toEqual(1.0);
        });
    });

    it('nested type roundtrip', function () {
        runs(function () {
            var request = new NestedTypeTest({
                value: { value: 'value' }
            });

            sendRequest.apply(this, [request]);
        });

        waitsFor(waitForResponse);

        runs(function () {
            var response = getResponse.apply(this);

            expect(response instanceof NestedTypeTest).toBeTruthy();
            expect(response.value instanceof NestedType).toBeTruthy();
            expect(response.value.value).toEqual('value');
        });
    });

    it('nested type array roundtrip', function () {
        runs(function () {
            var request = new NestedTypeArrayTest({
                values: [{ value: 'a' }, { value: 'b' }]
            });

            sendRequest.apply(this, [request]);
        });

        waitsFor(waitForResponse);

        runs(function () {
            var response = getResponse.apply(this);

            expect(response instanceof NestedTypeArrayTest).toBeTruthy();
            expect(response.values.length).toEqual(2);
            expect(response.values[0].value).toEqual('a');
            expect(response.values[1].value).toEqual('b');
        });
    });
});
