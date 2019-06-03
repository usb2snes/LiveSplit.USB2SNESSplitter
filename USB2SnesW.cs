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
            Console.WriteLine("Creating USB2Snes");
            ws = new ClientWebSocket();
            Console.WriteLine(ws);
        }

        private void SendCommand(Commands cmd, String arg)
        {
            List<String> args = new List<string>();
            args.Add(arg);
            SendCommand(cmd, args);
        }
        private async void SendCommand(Commands cmd, List<String> args)
        {
            USRequest req = new USRequest();
            req.Opcode = cmd.ToString();
            req.Space = "SNES";
            req.Operands = args;
            //Console.WriteLine(cmd);
            string json = new JavaScriptSerializer().Serialize(req);
            var sendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
            //Console.WriteLine(json);
            await ws.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        public async Task Connect()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(500);
            Console.WriteLine(ws.State);
            if (ws.State == WebSocketState.Aborted || ws.State == WebSocketState.CloseReceived)
            {
                ws.Dispose();
                ws = new ClientWebSocket();
            }
            await ws.ConnectAsync(new Uri("ws://localhost:8080"), cts.Token);
        }
        public void SetName(String name)
        {
            SendCommand(Commands.Name, name);
        }
        private async Task<USReply> waitForReply(int timeout)
        {
            byte[] buffer = new byte[1024];
            var segment = new ArraySegment<byte>(buffer, 0, buffer.Length);
            WebSocketReceiveResult recvResult;

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);
            var task = ws.ReceiveAsync(segment, cts.Token);
            await task;
            recvResult = task.Result;
            string rcvMsg = Encoding.UTF8.GetString(buffer.Take(recvResult.Count).ToArray());
            return new JavaScriptSerializer().Deserialize<USReply>(rcvMsg);
        }
        public async Task<List<String>> GetDevices()
        {
            List<String> toret = new List<string>();
            SendCommand(Commands.DeviceList, "");
            USReply rep = await waitForReply(1000);
            return rep.Results;
      }
        public void Attach(String device)
        {
            SendCommand(Commands.Attach, device);
        }
  
        public async Task<byte[]> GetAddress(uint address, uint size)
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
                var cts = new CancellationTokenSource();
                cts.CancelAfter(100);
                Console.WriteLine("Getting Address");
                cts.Token.Register(() => { });
                var task = ws.ReceiveAsync(new ArraySegment<byte>(ReadedData), cts.Token);
                await task;
                cts.Dispose();
                Console.WriteLine("Dispositing the token");
                if (task.Status != TaskStatus.RanToCompletion)
                    return new byte[0];
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
            Console.WriteLine("ws Checking connected" + ws.State);
            return ws.State == WebSocketState.Open;
        }
        public void Disconnect()
        {
            ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None).GetAwaiter().GetResult();
        }
        public async Task<USInfo> Info()
        {
            SendCommand(Commands.Info, "");
            USInfo info = new USInfo();
            try
            {
                USReply result = await waitForReply(50);
                
                info.version = result.Results[0];
                info.romPlaying = result.Results[2];
                info.flags = result.Results.Skip(3).ToList();
            } catch(Exception e) {
                Console.WriteLine(e.Message);
            }
            return info;
        }
    }
}
