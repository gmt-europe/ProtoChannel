<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Tests.aspx.cs" Inherits="ProtoChannel.HttpProxy.Tests" %>
<html>
<head>
    <title>Tests</title>
    <link rel="stylesheet" href="Jasmine/jasmine.css" type="text/css" />
    <script src="Jasmine/jasmine.js" type="text/javascript"></script>
    <script src="Jasmine/jasmine-html.js" type="text/javascript"></script>
    <script src="prototype.js" type="text/javascript"></script>
    <script src="protochannel.js" type="text/javascript"></script>
    <script src="service.js" type="text/javascript"></script>
    <script src="Tests/util.js" type="text/javascript"></script>
    <script src="Tests/Setup.js" type="text/javascript"></script>
    <script src="Tests/Messages.js" type="text/javascript"></script>
    <script src="Tests/Streams.js" type="text/javascript"></script>
    <script type="text/javascript">
        (function () {
            var jasmineEnv = jasmine.getEnv();
            jasmineEnv.updateInterval = 1000;

            var trivialReporter = new jasmine.TrivialReporter();

            jasmineEnv.addReporter(trivialReporter);

            jasmineEnv.specFilter = function (spec) {
                return trivialReporter.specFilter(spec);
            };

            var currentWindowOnload = window.onload;

            window.onload = function () {
                if (currentWindowOnload) {
                    currentWindowOnload();
                }
                execJasmine();
            };

            function execJasmine() {
                jasmineEnv.execute();
            }
        })();
    </script>
</head>
<body>
</body>
</html>
