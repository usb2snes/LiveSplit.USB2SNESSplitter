using NWA;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using static NWA.NWAClient;

namespace LiveSplit.UI.Components
{
    internal class Connector
    {
        private interface IIConnector
        {
            Task SetName(string name);
            Task<bool> GetReady();
            ConnectorError Error();
            Task Reset();
            string DeviceName();
            string Name();
            void Disconnect();
            Task<List<byte[]>> GetAddress(List<Tuple<uint, uint>> addressSizePairs);
        }
        private class Usb2SnesConnector : IIConnector
        {
            private ConnectorError _error;
            private USB2SnesW.USB2SnesW client = null;
            private string _deviceName = null;
            private string _serverName = null;
            public Usb2SnesConnector(USB2SnesW.USB2SnesW cl, string name)
            {
                client = cl;
                _serverName = name;
            }
            public string DeviceName()
            { 
                return _deviceName; 
            }
            public async Task SetName(string name)
            {
                await client.SetName(name);
            }
            public void Disconnect()
            {
                client.Disconnect();
            }
            public async Task Reset()
            {
                client?.Reset();
            }
            public string Name()
            { 
                if (client.Legacy)
                    return "Unknow " + _serverName;
                return _serverName;
            }
            public async Task<bool> GetReady()
            {
                Debug.WriteLine("Usb2Snes getting device");
                var deviceList = await client.GetDevices();
                _deviceName = null;
                Debug.WriteLine("Usb2Snes getting device done");
                if (deviceList.Count == 0)
                {
                    _error = ConnectorError.USB2SNES_NO_DEVICE;
                    return false;                    
                }
                _deviceName = deviceList[0];
                client.Attach(deviceList[0]);
                await client.Info();
                return true;
            }
            public ConnectorError Error() { return _error; }
            public async Task<List<byte[]>> GetAddress(List<Tuple<uint, uint>> addressSizePairs)
            {
                return await client.GetAddress(addressSizePairs);
            }

        }
        private class NWAConnector : IIConnector
        {
            private ConnectorError _error;
            private NWAClient client = null;
            private string _deviceName = null;
            public NWAConnector(NWAClient client)
            {
                this.client = client;
            }
            public void Disconnect()
            {
                client.Disconnect();
            }
            public string Name()
            {
                return _deviceName;
            }
            public async Task Reset()
            {
                await client.DoCommand("EMULATION_RESET");
            }
            public string DeviceName() { return _deviceName; }
            public async Task SetName(string name)
            {
                await client.MyNameIs(name);
            }
            public async Task<bool> GetReady()
            {
                Dictionary<string, string> reply;
                try
                {
                    reply = await client.EmulatorInfo();
                    _deviceName = reply["name"];
                } catch (NWAErrorException e)
                {
                    return false;
                }
                try
                {
                    reply = await client.CurrentCoreInfo();
                } catch (NWAErrorException ex)
                {
                    return false;
                }
                if (reply["platform"] != "SNES")
                {
                    _error = ConnectorError.NWA_NO_VALID_CORE;
                    return false;
                }
                try
                {
                    reply = await client.EmulationStatus();
                } catch (NWAErrorException ex)
                {
                    return false;
                }
                if (reply["state"] == "running" || reply["state"] == "paused")
                    return true;
                _error = ConnectorError.NWA_NO_GAME_RUNNING;
                return false;
            }
            public async Task<List<byte[]>> GetAddress(List<Tuple<uint, uint>> addressSizePairs)
            {
                List<string> cmdArg = new List<string>();
                cmdArg.Add("WRAM");
                foreach (var pair  in addressSizePairs)
                {
                    cmdArg.Add(pair.Item1.ToString());
                    cmdArg.Add(pair.Item2.ToString());
                }
                var reply = await client.DoCommand("CORE_READ", cmdArg);
                if (reply.replyType != NWAReplyType.Binary)
                {
                    throw new Exception();
                }
                List<byte[]> result = new List<byte[]>();
                uint offset = 0;
                foreach (var pair in addressSizePairs)
                {
                    result.Add(reply.datas.SubArray(offset, pair.Item2));
                    offset += pair.Item2;
                }
                return result;
            }
            public ConnectorError Error() { return _error; }
        }

        public enum ConnectorState
        {
            NONE,
            CONNECTING,
            CONNECTED,
            GETTING_READY,
            READY
        }
        public enum ConnectorError
        {
            NONE,
            NO_SERVER,
            USB2SNES_NO_DEVICE,
            NWA_NO_VALID_CORE,
            NWA_NO_GAME_RUNNING
        }
        private USB2SnesW.USB2SnesW usb2snesClient = null;
        private NWAClient nwaClient = null;
        private string usb2snesHost = "localhost";
        private int usb2snesPort = 23074;
        private IIConnector connector = null;
        private ConnectorError _error = ConnectorError.NONE;
        private ConnectorState _state;
        public async Task Reset()
        {
            await connector.Reset();
        }
        public void Disconnect()
        {
            connector?.Disconnect();
            _state = ConnectorState.NONE;
            connector = null;
        }
        public string DeviceName()
        { 
            return connector?.DeviceName();
        }
        public string ConnectorName()
        {
            if (connector != null)
            {
                return connector.Name();
            }
            return "";
        }
        public ConnectorError Error()
        {
            if (connector != null &&_error == ConnectorError.NONE)
                return connector.Error();
            return _error;
        }

        public void setUsb2SnesHost(string host, int port)
        {
            usb2snesHost = host;
            usb2snesPort = port;
        }
        public Connector() {
            nwaClient = new NWAClient();
        }
        public async Task<bool> Connect()
        {
            _error = ConnectorError.NONE;
            _state = ConnectorState.CONNECTING;
            Debug.WriteLine("Trying to connect to NWA");
            nwaClient = new NWAClient();
            bool nwaHere = await nwaClient.Connect();
            if (nwaHere)
            {
                connector = new NWAConnector(nwaClient);
                _state = ConnectorState.CONNECTED;
                return true;
            }
            Debug.WriteLine("Trying to connect to Usb2snes");
            // default port
            usb2snesClient = new USB2SnesW.USB2SnesW(usb2snesHost, usb2snesPort);
            bool usbhere = await usb2snesClient.Connect();
            if (usbhere)
            {
                string name = await usb2snesClient.AppName();
                connector = new Usb2SnesConnector(usb2snesClient, name);
                _state = ConnectorState.CONNECTED;
                return true;
            }
            // legacy port
            Debug.WriteLine("Trying to connect to Usb2snes legacy");
            usb2snesClient = new USB2SnesW.USB2SnesW(usb2snesHost, 8080);
            usbhere = await usb2snesClient.Connect();
            if (usbhere)
            {
                string name = await usb2snesClient.AppName();
                connector = new Usb2SnesConnector(usb2snesClient, name);
                _state = ConnectorState.CONNECTED;
                return true;
            }
            _error = ConnectorError.NO_SERVER;
            _state = ConnectorState.NONE;
            return false;
        }
        public async Task<bool> GetReady()
        {
            _state = ConnectorState.GETTING_READY;
            Debug.WriteLine("GConnector getready");
            bool ready = await connector.GetReady();
            if (ready)
                _state = ConnectorState.READY;
            else
                _state = ConnectorState.CONNECTED;
            return ready;
        }
        public async Task SetName(string name)
        {
            await connector.SetName(name);
        }
        public async Task<List<byte[]>> GetAddress(List<Tuple<uint, uint>> addressSizePairs)
        {
            return await connector.GetAddress(addressSizePairs);
        }
        public ConnectorState  State() { return _state; }

    }
}
