<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ProtoChannel.HttpProxy.Default" %>
<html>
<head>
    <title>ProtoChannel HTTP proxy</title>
    <script src="prototype.js" type="text/javascript"></script>
    <script src="protochannel.js" type="text/javascript"></script>
    <script src="pingpong.js" type="text/javascript"></script>
    <script type="text/javascript">
        var channel;

        window.onload = function () {
            var pos = document.URL.indexOf('://');
            pos = document.URL.indexOf('/', pos + 3);
            var host = document.URL.substr(0, pos);

            channel = new ProtoChannel(host, 0, function () {
                this.sendMessage(
                    {
                        1: 'Payload'
                    },
                    1,
                    function (message) {
                        alert('Accepted response ' + message['1']);
                    }
                );
            });
        };

        function submitPing() {
            channel.sendMessage(
                {
                    1: $('payload').value
                },
                1,
                function (message) {
                    alert('Received response: ' + message['1']);
                }
            );

            return false;
        }
    </script>
</head>
<body>
    <form onsubmit="javascript: return (submitPing());">
        <table>
            <tr>
                <td>Payload:</td>
                <td><input type="text" id="payload" /></td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td><input type="submit" /></td>
            </tr>
        </table>
    </form>
</body>
</html>
