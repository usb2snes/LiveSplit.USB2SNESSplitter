using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using WebSocketSharp;


namespace USB2SnesW
{
    internal class USB2SnesW
    {
        private sealed class WebSocketConnectionListener : IDisposable
        {
            private readonly WebSocket m_webSocket;
            private readonly TaskCompletionSource<bool> m_taskCompletionSource;
            private bool m_disposed = false;

            internal WebSocketConnectionListener(WebSocket webSocket, TaskCompletionSource<bool> taskCompletionSource)
            {
                m_webSocket = webSocket;
                m_taskCompletionSource = taskCompletionSource;

                m_webSocket.OnClose += OnWebSocketClose;
                m_webSocket.OnError += OnWebSocketError;
                m_webSocket.OnOpen += OnWebSocketOpen;
            }

            public void Dispose()
            {
                if (!m_disposed)
                {
                    m_disposed = true;

                    m_webSocket.OnClose -= OnWebSocketClose;
                    m_webSocket.OnError -= OnWebSocketError;
                    m_webSocket.OnOpen -= OnWebSocketOpen;
                }
            }

            private void OnWebSocketClose(object sender, CloseEventArgs e)
            {
                m_taskCompletionSource.TrySetResult(false);
            }

            private void OnWebSocketError(object sender, ErrorEventArgs e)
            {
                m_taskCompletionSource.TrySetResult(false);
            }

            private void OnWebSocketOpen(object sender, EventArgs e)
            {
                m_taskCompletionSource.TrySetResult(true);
            }
        }

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
        public enum Commands
        {
            DeviceList,
            Name,
            GetAddress,
            Reset,
            Attach,
            Info
        }
        private WebSocket ws;

        public USB2SnesW()
        {
            Console.WriteLine("Creating USB2Snes");
            ws = new WebSocket("ws://localhost:8080");
            Console.WriteLine(ws);
        }

        public async Task<bool> Connect()
        {
            if (ws.ReadyState == WebSocketState.Open)
                return true;

            var tcs = new TaskCompletionSource<bool>();
            using (WebSocketConnectionListener listener = new WebSocketConnectionListener(ws, tcs))
            {
                try
                {
                    ws.ConnectAsync();
                }
                catch (Exception e)
                {
                    Log.Error($"Error when connecting to Usb2Snes: {e}");
                    tcs.TrySetResult(false);
                }

                return await tcs.Task;
            }
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
            //Console.WriteLine(cmd);
            string json = new JavaScriptSerializer().Serialize(req);
            ws.Send(json);
        }
        
        public void SetName(String name)
        {
            SendCommand(Commands.Name, name);
        }

        private async Task<USReply> waitForReply()
        {
            var tcs = new TaskCompletionSource<USReply>();
            EventHandler<MessageEventArgs> msgHandler = null;
            EventHandler<ErrorEventArgs> errorHandler = null;
            EventHandler<CloseEventArgs> closeHandler = null;
            msgHandler = (s, e) =>
            {
                ws.OnMessage -= msgHandler;
                ws.OnClose -= closeHandler;
                ws.OnError -= errorHandler;

                if (e.IsText)
                    tcs.TrySetResult(new JavaScriptSerializer().Deserialize<USReply>(e.Data));
                else
                    tcs.TrySetResult(new USReply());

            };
            errorHandler = (s, e) =>
            {
                ws.OnMessage -= msgHandler;
                ws.OnClose -= closeHandler;
                ws.OnError -= errorHandler;

                Console.WriteLine("Error in wait for reply : " + e.Message);
                // errorHandler and closeHandler can both be called for the same event, and SetCanceled is not re-entrant
                tcs.TrySetCanceled();
            };
            closeHandler = (s, e) =>
            {
                ws.OnMessage -= msgHandler;
                ws.OnClose -= closeHandler;
                ws.OnError -= errorHandler;

                Console.WriteLine("wsAttach: Connection closed");
                // errorHandler and closeHandler can both be called for the same event, and SetCanceled is not re-entrant
                tcs.TrySetCanceled();
            };
            ws.OnMessage += msgHandler;
            ws.OnError += errorHandler;
            ws.OnClose += closeHandler;
            return await tcs.Task;
        }
        public async Task<List<String>> GetDevices()
        {
            SendCommand(Commands.DeviceList, "");
            USReply rep = await waitForReply();
            return rep.Results;
        }
        public void Attach(String device)
        {
            SendCommand(Commands.Attach, device);
        }

        public async Task<List<byte[]>> GetAddress(List<Tuple<uint, uint>> addressSizePairs)
        {
            uint totalSize = 0;
            uint startingAddress = 0xFFFFFFFF;
            uint endingAddress = 0;
            List<String> args = new List<String>();
            foreach (var pair in addressSizePairs)
            {
                args.Add((0xF50000 + pair.Item1).ToString("X"));
                args.Add(pair.Item2.ToString("X"));
                totalSize += pair.Item2;
                if (startingAddress > pair.Item1)
                    startingAddress = pair.Item1;
                if (endingAddress < (pair.Item1 + pair.Item2))
                    endingAddress = pair.Item1 + pair.Item2;
            }
            bool consolidatedRequest = (addressSizePairs.Count > 1) && ((startingAddress + 255) > endingAddress);
            if (consolidatedRequest)
            {
                totalSize = endingAddress - startingAddress;
                args.Clear();
                args.Add((0xF50000 + startingAddress).ToString("X"));
                args.Add(totalSize.ToString("X"));
            }
            SendCommand(Commands.GetAddress, args);
            var tcs = new TaskCompletionSource<byte[]>();
            EventHandler<MessageEventArgs> msgHandler = null;
            EventHandler<ErrorEventArgs> errorHandler = null;
            EventHandler<CloseEventArgs> closeHandler = null;
            msgHandler = (s, e) =>
            {
                ws.OnMessage -= msgHandler;
                ws.OnClose -= closeHandler;
                ws.OnError -= errorHandler;

                if (e.IsBinary)
                    tcs.TrySetResult(e.RawData);
                else
                    tcs.TrySetResult(new byte[0]);
            };
            errorHandler = (s, e) =>
            {
                ws.OnMessage -= msgHandler;
                ws.OnClose -= closeHandler;
                ws.OnError -= errorHandler;

                Console.WriteLine("Error in get address : " + e.Message);
                tcs.SetCanceled();
            };
            closeHandler = (s, e) =>
            {
                ws.OnMessage -= msgHandler;
                ws.OnClose -= closeHandler;
                ws.OnError -= errorHandler;

                Console.WriteLine("Connection closed");
                tcs.SetCanceled();
            };
            ws.OnMessage += msgHandler;
            ws.OnError += errorHandler;
            ws.OnClose += closeHandler;
            var result = new List<byte[]>();
            if (await Task.WhenAny(tcs.Task, Task.Delay(100)) == tcs.Task)
            {
                if (tcs.Task.Result.Length == totalSize)
                {
                    if (consolidatedRequest)
                    {
                        foreach (var pair in addressSizePairs)
                        {
                            result.Add(tcs.Task.Result.SubArray<byte>(pair.Item1 - startingAddress, pair.Item2));
                        }
                    }
                    else if (addressSizePairs.Count > 1)
                    {
                        uint currentPosition = 0;
                        foreach (var pair in addressSizePairs)
                        {
                            result.Add(tcs.Task.Result.SubArray<byte>(currentPosition, pair.Item2));
                            currentPosition += pair.Item2;
                        }
                    }
                    else
                    {
                        result.Add(tcs.Task.Result);
                    }
                }
            }
            return result;
        }
        public void Reset()
        {
            SendCommand(Commands.Reset, "");
        }
        public bool Connected()
        {
            bool live = ws.IsAlive;
            return live;
        }
        public void Disconnect()
        {
            ws.Close();
        }
        public async Task<USInfo> Info()
        {
            SendCommand(Commands.Info, "");
            USInfo info = new USInfo();
            try
            {
                USReply result = await waitForReply();
                
                info.version = result.Results[0];
                info.romPlaying = result.Results[2];
                info.flags = result.Results.Skip(3).ToList();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return info;
        }
    }
}
