using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Web.Script.Serialization;
using System.Threading;

namespace USB2SnesW
{
    class USB2SnesW
    {
        class USRequest
        {
            public String Opcode;
            public String Space;
            public List<String> Operands;
        }
        public class USInfo
        {
            public String version;
            public String romPlaying;
            public List<String> flags;
        }
        class USReply
        {
            public List<string> Results { get; set; }
        }
        public enum Commands {
            DeviceList,
            Name,
            GetAddress,
            Reset,
            Attach,
            Info
        }
        private ClientWebSocket ws;

        public USB2SnesW()
        {
            ws = new ClientWebSocket();
        }

        private void SendCommand(Commands cmd, String arg)
        {
            List<String> args = new List<string>();
            args.Add(arg);
            SendCommand(cmd, args);
        }
        private void SendCommand(Commands cmd, List<String> args)
        {
            USRequest req = new USRequest();
            req.Opcode = cmd.ToString();
            req.Space = "SNES";
            req.Operands = args;
            string json = new JavaScriptSerializer().Serialize(req);
            var sendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
            ws.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None).GetAwaiter().GetResult();
        }
        public bool Connect()
        {
            try
            {
                var cts = new CancellationTokenSource();
                var task = ws.ConnectAsync(new Uri("ws://localhost:8080"), cts.Token);
                int timeoutcpt = 0;
                while (!task.IsCompleted && timeoutcpt < 10)
                {
                    Thread.Sleep(10);
                    timeoutcpt++;
                }
                if (!task.IsCompleted && !task.IsCanceled)
                {
                    cts.Cancel();
                    return false;
                }
                return ws.State == WebSocketState.Open;
            } catch {
                return false;
            }

        }
        public void SetName(String name)
        {
            SendCommand(Commands.Name, name);
        }
        private USReply waitForReply(int timeout)
        {
            byte[] buffer = new byte[1024];
            var segment = new ArraySegment<byte>(buffer, 0, buffer.Length);
            WebSocketReceiveResult recvResult;

            var cts = new CancellationTokenSource();
            var task = ws.ReceiveAsync(segment, cts.Token);
            // The CancelationToken stuff is buggy, so fuck it
            int timeoutcpt = 0;
            while (!task.IsCompleted && timeoutcpt < timeout / 10)
            {
                Thread.Sleep(10);
                timeoutcpt++;
            }
            if (!task.IsCompleted)
            {
                cts.Cancel();
                throw new Exception();
            }
            recvResult = task.Result;
            string rcvMsg = Encoding.UTF8.GetString(buffer.Take(recvResult.Count).ToArray());
            return new JavaScriptSerializer().Deserialize<USReply>(rcvMsg);
        }
        public List<String> GetDevices()
        {
            List<String> toret = new List<string>();
            SendCommand(Commands.DeviceList, "");
            try
            {
                USReply rep = waitForReply(1000);
                return rep.Results;
            } catch
            {
                return toret;
            }
        }
        public void Attach(String device)
        {
            SendCommand(Commands.Attach, device);
        }
  
        public byte[] GetAddress(uint address, uint size)
        {
            List<String> args = new List<String>();
            args.Add(address.ToString("X"));
            args.Add(size.ToString("X"));
            SendCommand(Commands.GetAddress, args);
            byte[] result = new byte[size];
            byte[] ReadedData = new byte[1024];
            int count = 0;
            while (count < size)
            {
                var task = ws.ReceiveAsync(new ArraySegment<byte>(ReadedData), CancellationToken.None);
                // The CancelationToken stuff is buggy, so fuck it
                int timeoutcpt = 0;
                while (!task.IsCompleted && timeoutcpt != 10)
                {
                    Thread.Sleep(10);
                    timeoutcpt++;
                }
                var wsResult = task.Result;
                if (wsResult.CloseStatus.HasValue)
                    return new byte[0];
                for (int i = 0; i < wsResult.Count; i++)
                {
                    result[i + count] = ReadedData[i];
                }
                count += wsResult.Count;
            }
            return result;
        }
        public void Reset()
        {
            SendCommand(Commands.Reset, "");
        }
        public bool Connected()
        {
            return ws.State == WebSocketState.Open;
        }
        public void Disconnect()
        {
            ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None).GetAwaiter().GetResult();
        }
        public USInfo Info()
        {
            SendCommand(Commands.Info, "");
            USInfo info = new USInfo();
            try
            {
                USReply result = waitForReply(100);
                
                info.version = result.Results[0];
                info.romPlaying = result.Results[2];
                info.flags = result.Results.Skip(3).ToList();
            } catch {

            }
            return info;
        }
    }
}
