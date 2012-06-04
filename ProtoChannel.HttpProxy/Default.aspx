<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ProtoChannel.HttpProxy.Default" %>
<html>
<head>
    <title>ProtoChannel HTTP proxy</title>
    <script src="prototype.js" type="text/javascript"></script>
    <script src="protochannel.js" type="text/javascript"></script>
    <script src="service.js" type="text/javascript"></script>
    <script type="text/javascript">
        var channel;

        CallbackChannel = Class.create(ServiceCallbackChannel, {
            oneWayPing: function (message) {
                alert('Received one way ping callback response: ' + message.payload);
            },

            ping: function (message) {
                alert('Received ping callback response: ' + message.payload);
            }
        });

        window.onload = function () {
            var pos = document.URL.indexOf('://');
            pos = document.URL.indexOf('/', pos + 3);
            var host = document.URL.substr(0, pos);

            channel = new ServiceChannel(host, 1, { onReceived: new CallbackChannel() });
        };

        function submitPing() {
            channel.ping(
                { payload: $('payload').value },
                function (message) {
                    alert('Received response: ' + message.payload);
                }
            );

            return false;
        };

        function submitOneWayPing() {
            channel.oneWayPing(
                { payload: $('oneWay').value }
            );

            return false;
        };

        var uploadedAid = null;

        function submitUpload() {
            uploadedAid = channel.getSendStreamAid();

            $('upload').action = channel.getStreamUrl(uploadedAid);

            return true;
        };

        function processSinkNavigated() {
            if (uploadedAid != null) {
                var aid = uploadedAid;

                uploadedAid = null;

                channel.postMessage(new StreamResponse({ streamId: aid }));
            }
        };

        function submitDownload() {
            channel.sendMessage(
                new StreamRequest(),
                function (message) {
                    channel.downloadStream(message.streamId);
                }
            );

            return false;
        }
    </script>
</head>
<body>
    <form onsubmit="javascript: return (submitPing());" action="/" method="post">
        <table>
            <tr>
                <td>Payload:</td>
                <td><input type="text" id="payload" /></td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td><input type="submit" value="Submit Ping" /></td>
            </tr>
        </table>
    </form>

    <form onsubmit="javascript: return (submitOneWayPing());" action="/" method="post">
        <table>
            <tr>
                <td>Payload:</td>
                <td><input type="text" id="oneWay" /></td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td><input type="submit" value="Submit OneWayPing" /></td>
            </tr>
        </table>
    </form>
    
    <form id="upload" onsubmit="javascript: return (submitUpload());" action="/" method="post" enctype="multipart/form-data" target="sink">
        <table>
            <tr>
                <td>File:</td>
                <td><input type="file" name="stream" /></td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td><input type="submit" value="Submit Upload" /></td>
            </tr>
        </table>
    </form>
    
    <form onsubmit="javascript: return (submitDownload());" action="/" method="post" target="sink">
        <input type="submit" value="Submit Download" />
    </form>
    
    <iframe name="sink" src="about:blank" style="display: none;" onload="javascript: processSinkNavigated();"></iframe>
</body>
</html>
