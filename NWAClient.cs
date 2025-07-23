using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NWA.NWAClient;

namespace NWA
{
    public class NWAClient
    {
        public enum ErrorKind
        {
            InvalidError,
            ProtocolError,
            CommandError,
            InvalidCommand,
            InvalidArgument,
            NotAllowed
        }
        public class NWAErrorException : Exception
        {
            public ErrorKind    ErrorKind;
            public string       reason;
            public NWAErrorException(ErrorKind errK, string reason)
                : base(NWAErrorStringToType.FirstOrDefault(x => x.Value == errK).Key + " : " + reason)
            {
                ErrorKind = errK;
                this.reason = reason;
            }
        }
        private static Dictionary<String, ErrorKind> NWAErrorStringToType = new Dictionary<String, ErrorKind>() {
                        { "protocol_error", ErrorKind.ProtocolError },
                        { "invalid_command", ErrorKind.InvalidCommand },
                        { "invalid_argument", ErrorKind.InvalidArgument },
                        { "command_error", ErrorKind.CommandError },
                        { "not_allowed", ErrorKind.NotAllowed }
                    };
        public enum NWAReplyType
        {
            ASCII,
            Binary,
            Error
        }
        public readonly struct NWAReply
        {
            public readonly NWAReplyType                        replyType;
            public readonly Dictionary<string, string>         map;
            public readonly List<Dictionary<string, string>>   listMap;

            public readonly ErrorKind?     errorKind;
            public readonly string        errorReason;
            public readonly byte[]        datas;
            
            public NWAReply(Dictionary<string, string> h)
            {
                errorKind = null;
                errorReason = null;
                datas = null;
                listMap = null;
                replyType = NWAReplyType.ASCII;
                map = h;
            }
            public NWAReply(byte[] data)
            {
                errorKind = null;
                errorReason = null;
                datas = null;
                listMap = null;
                map = null;
                replyType = NWAReplyType.Binary;
                datas = data;
            }
            public NWAReply(ErrorKind erk, string reason)
            {
                errorKind = null;
                errorReason = null;
                datas = null;
                listMap = null;
                map = null;
                replyType = NWAReplyType.Error;
                errorKind = erk;
                errorReason = reason;
            }
            public NWAReply(List<Dictionary<string, string>> lMap)
            {
                errorKind = null;
                errorReason = null;
                datas = null;
                listMap = null;
                map = null;
                errorKind = null;
                replyType = NWAReplyType.ASCII;
                listMap = lMap;
            }
        }
        private TcpClient tcpClient = new TcpClient();
        private NetworkStream ns;
        public NWAClient()
        {
            ns = null;
        }
        /// <summary>
        /// Connect to a NWA emulator, you can specify the host, port and a timeout for the connection
        /// </summary>
        /// <param name="host">The host to connect to</param>
        /// <param name="port">The port, default is 0xBEEF</param>
        /// <param name="timeout">timeout before abording the connection, default is 200 ms</param>
        /// <returns></returns>
        public async Task<bool> Connect(string host = "localhost", int port = 0xBEEF, int timeout = 200)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);

            //var coTask = tcpClient.ConnectAsync(host, port, cts.Token);
            var coTask = tcpClient.ConnectAsync(host, port);
            try
            {
                await coTask;
            } catch (SocketException ex)
            {
                return false;
            }
            catch (OperationCanceledException ex)
            { 
                return false;
            }
            if (coTask.IsCanceled)
                return false;
            if (tcpClient.Connected)
                ns = tcpClient.GetStream();
            return tcpClient.Connected;
        }
        public void Disconnect()
        {
            tcpClient.Close();
        }

        /// <summary>
        /// Execute a NWA command and returns the reply in a form of a NWAReply object
        /// </summary>
        /// <param name="command">the commande to execute</param>
        /// <param name="args">the arguments for the command</param>
        /// <param name="timeout">a optionnal timeout (in ms)</param>
        /// <returns>returns the NWA reply</returns>
        public async Task<NWAReply> DoCommand(string command, List<string> args = null, int timeout = 0)
        {
            if (ns == null)
                throw new Exception();
            string nwaString = command;
            if (args != null && args.Count > 0)
            {
                nwaString += " ";
            }
            if (args != null)
                nwaString += String.Join(";", args);
            nwaString += "\n";
            Console.WriteLine("Executing : " + nwaString);
            var data = System.Text.Encoding.ASCII.GetBytes(nwaString);
            await ns.WriteAsync(data, 0, data.Length);
            return await readReply(timeout);
        }
        public async Task<string> MyNameIs(string name)
        {
            var sName = new List<string>();
            sName.Add(name);
            var plop = await DoAsciiCommand("MY_NAME_IS", sName);
            return plop["name"];
        }
        public async Task<Dictionary<string, string>>   EmulatorInfo()
        {
            return await DoAsciiCommand("EMULATOR_INFO");
        }
        public async Task<Dictionary<string, string>>   EmulationStatus()
        {
            return await DoAsciiCommand("EMULATION_STATUS");
        }
        public async Task<List<Dictionary<string, string>>>   CoresList(string name = null)
        {
            if (name == null)
                return await DoListAsciiCommand("CORES_LIST");
            var sName = new List<string>();
            sName.Add(name);
            return await DoListAsciiCommand("CORES_LIST", sName);
        }
        public async Task<Dictionary<string, string>> CurrentCoreInfo()
        {
            return await DoAsciiCommand("CORE_CURRENT_INFO");
        }
        private async Task<List<Dictionary<string, string>>> DoListAsciiCommand(string command, List<string> args = null, int timeout = 0)
        {
            var commandTask = DoCommand(command, args, timeout);
            var reply = await commandTask;
            if (reply.replyType == NWAReplyType.ASCII)
            {
                return reply.listMap;
            }
            if (reply.replyType == NWAReplyType.Error)
            {
                throw new NWAErrorException((ErrorKind)reply.errorKind, (string)reply.errorReason);
            }
            else
            {
                throw new Exception();
            }
        }
        private async Task<Dictionary<string, string>> DoAsciiCommand(string command, List<string> args = null, int timeout = 0)
        {
            var reply = await DoCommand(command, args, timeout);
            if (reply.replyType == NWAReplyType.ASCII)
            {
                return reply.map;
            }
            if (reply.replyType == NWAReplyType.Error)
            {
                 throw new NWAErrorException((ErrorKind) reply.errorKind, (string) reply.errorReason);
            } else
            {
                throw new Exception();
            }
            
        }
        private async Task<NWAReply> readReply(int timeout = 0)
        {
            var cts = new CancellationTokenSource();
            if (timeout != 0)
                cts.CancelAfter(timeout);
            byte[] buffer = new byte[2048];
            var readTask = ns.ReadAsync(buffer, 0, 1, cts.Token);
            await readTask;
            if (readTask.IsCanceled || readTask.IsFaulted || readTask.Result == 0)
            {
                cts.Token.ThrowIfCancellationRequested();
            }
            // AScii reply
            Console.WriteLine("Readed : {1} First reply byte is {0}", buffer[0], readTask.Result);
            if (buffer[0] == '\n')
            {
                Console.WriteLine("Ascii reply");
                var asciiMap = new Dictionary<string, string>();
                var listAsciiMap = new List<Dictionary<string, string>>();
                uint loopCount = 0;
                string line = "";
                System.IO.StreamReader reader = new System.IO.StreamReader(ns, Encoding.UTF8, false, 1024, true);
                //cts.TryReset();
                while (true)
                {
                    var lineReadTask = reader.ReadLineAsync();
                    await lineReadTask;
                    if (lineReadTask.IsCanceled || lineReadTask.IsFaulted || line == null)
                    {
                        cts.Token.ThrowIfCancellationRequested();
                    }
                    line = lineReadTask.Result;
                    //Console.WriteLine(line + "$");
                    // Ok reply
                    if (line == "" && loopCount == 0)
                    {
                        return new NWAReply(asciiMap);
                    }
                    if (line == "")
                    {
                        if (listAsciiMap.Count > 0)
                        {
                            listAsciiMap.Add(new Dictionary<string, string>(asciiMap));
                        }
                        break;
                    }
                    if (line == null)
                        break;
                    // Extracting key/value
                    //var lineSplit = line.Split(:', 2);
                    var parts = line.Split(new[] { ':' }, StringSplitOptions.None);
                    var lineSplit = parts.Length > 2
                        ? new[] { parts[0], string.Join(":", parts.Skip(1)) }
                        : parts;
                    var key = lineSplit[0];
                    var value = lineSplit[1];
                    if (asciiMap.ContainsKey(key))
                    {
                        listAsciiMap.Add(new Dictionary<string, string>(asciiMap));
                        asciiMap.Clear();
                    }
                    asciiMap[key] = value;
                    loopCount++;
                }
                if (asciiMap.ContainsKey("error"))
                {
                    ErrorKind errorKind = ErrorKind.InvalidError;
                    if (asciiMap.ContainsKey("reason") && NWAErrorStringToType.ContainsKey(asciiMap["error"]))
                    {
                        errorKind = NWAErrorStringToType[asciiMap["error"]];
                        return new NWAReply(errorKind, asciiMap["reason"]);
                    }
                    else
                    {
                        return new NWAReply(errorKind, "");
                    }
                }
                if (listAsciiMap.Count == 0)
                {
                    return new NWAReply(asciiMap);
                }
                return new NWAReply(listAsciiMap);

            }
            // Binary reply
            if (buffer[0] == 0)
            {
                //cts.TryReset();
                Task<int> nreadTask = null;
                try
                {
                    int readed = 0;
                    while (readed != 4)
                    {
                        nreadTask = ns.ReadAsync(buffer, readed, 4 - readed);
                        var r = await nreadTask;
                        if (r == 0)
                        {
                            throw new EndOfStreamException();
                        }
                        readed += r;
                    }
                } catch (EndOfStreamException)
                {
                    throw new OperationCanceledException();
                }
                if (nreadTask.IsCanceled || nreadTask.IsFaulted)
                {
                    cts.Token.ThrowIfCancellationRequested();
                }
                UInt32 size = 0;
                if (BitConverter.IsLittleEndian)
                {
                    size += (UInt32)(buffer[0] << 24);
                    size += (UInt32)(buffer[1] << 16);
                    size += (UInt32)(buffer[2] << 8);
                    size += buffer[3];
                }
                else
                {
                    size += (UInt32)(buffer[3] << 24);
                    size += (UInt32)(buffer[2] << 16);
                    size += (UInt32)(buffer[1] << 8);
                    size += buffer[0];
                }
                //Console.WriteLine("{0} - Size : {1}", BitConverter.ToString(buffer, 0, 4), size);
                //cts.TryReset();
                buffer = new byte[size];
                try
                {
                    int readed = 0;
                    while (readed != size)
                    {
                        nreadTask = ns.ReadAsync(buffer, readed, (int)size - readed);
                        var r = await nreadTask;
                        if (r == 0)
                        {
                            throw new EndOfStreamException();
                        }
                        readed += r;
                    }
                }
                catch (EndOfStreamException)
                {
                    throw new OperationCanceledException();
                }
                if (nreadTask.IsCanceled || nreadTask.IsFaulted)
                {
                    cts.Token.ThrowIfCancellationRequested();
                }
                return new NWAReply(buffer);
            }
            return new NWAReply(ErrorKind.ProtocolError, "expected reply type");
        }
    }
}
