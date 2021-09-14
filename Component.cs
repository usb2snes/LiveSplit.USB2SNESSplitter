using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model;
using LiveSplit.Options;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LiveSplit.USB2SNESSplitterTests")]

namespace LiveSplit.UI.Components
{
    public class USB2SNESComponent : IComponent
    {
        enum ConfigState
        {
            NONE,
            ERROR,
            READY,
        };

        enum ProtocolState
        {
            NONE,
            CONNECTING,
            FAILED_TO_CONNECT,
            DEVICE_NOT_FOUND,
            ATTACHING,
            FAILED_TO_ATTACH,
            ATTACHED,
        }

        private const float BorderThickness = 1.5f;
        private const float TextPadding = 2f;
        private readonly TimeSpan ReadyAutoHideTime = TimeSpan.FromSeconds(3);
        // This is a bit ugly, but it's to deal with the way LiveSplit handles sizing. When first starting up, or when the layout changes,
        // LiveSplit attempts to force the window's size to whatever's specified in the layout settings. Then, once everything has stabilized,
        // further changes to the size of components do not cause the whole window to be resized to match the layout settings. If we show the
        // status message right away, that will affect the "stable" size of this component, which means that once everything is ready and the
        // message is hidden, the window will be smaller than the layout settings specify. So, we delay showing the status message for a bit
        // to give LiveSplit a chance to lock in the correct size.
        private readonly TimeSpan ShowStatusMessageDelay = TimeSpan.FromSeconds(0.5);

        public string ComponentName => "USB2SNES Auto Splitter";

        public float HorizontalWidth { get; set; }

        public float MinimumHeight => 2 * BorderThickness;

        public float VerticalHeight { get; set; }

        public float MinimumWidth => 2 * BorderThickness;

        public float PaddingTop => 1;

        public float PaddingBottom => 1;

        public float PaddingLeft => 1;

        public float PaddingRight => 1;

        public IDictionary<string, Action> ContextMenuControls => null;

        private Timer _update_timer;
        private ComponentSettings _settings;
        private LiveSplitState _state;
        private TimerModel _model;
        private List<string> _splits;
        private ConfigState _config_state;
        private ProtocolState _proto_state;
        private DateTime _first_draw_time = DateTime.MinValue;
        private Stopwatch _ready_timer;
        private string _attached_device;
        private bool _inTimer;
        private USB2SnesW.USB2SnesW _usb2snes;
        private Color _ok_color = Color.FromArgb(0, 128, 0);
        private Color _error_color = Color.FromArgb(128, 0, 0);
        private Color _connecting_color = Color.FromArgb(128, 128, 0);
        private bool _stateChanged;

        private void init(LiveSplitState state, USB2SnesW.USB2SnesW usb2snesw)
        {
            _state = state;
            _config_state = ConfigState.NONE;
            _proto_state = ProtocolState.NONE;
            _ready_timer = new Stopwatch();
            _attached_device = string.Empty;
            _settings = new ComponentSettings(_state)
            {
                Dock = DockStyle.Fill,
            };
            _model = new TimerModel() { CurrentState = _state };
            _state.RegisterTimerModel(_model);
            _stateChanged = false;
            _splits = new List<string>();
            _inTimer = false;

            _update_timer = new Timer() { Interval = 1000 };
            _update_timer.Tick += (sender, args) => UpdateSplitsWrapper();
            _update_timer.Enabled = true;

            _state.OnReset += _state_OnReset;
            _state.OnStart += _state_OnStart;
            HorizontalWidth = 3;
            VerticalHeight = 3;
            _usb2snes = usb2snesw;
        }

        public USB2SNESComponent(LiveSplitState state)
        {
            init(state, new USB2SnesW.USB2SnesW());
        }

        internal USB2SNESComponent(LiveSplitState state, USB2SnesW.USB2SnesW usb2snesw)
        {
            init(state, usb2snesw);
        }

        private void SetConfigState(ConfigState state)
        {
            if (_config_state != state)
            {
                _stateChanged = true;
                _config_state = state;
            }
        }

        private void SetProtocolState(ProtocolState state)
        {
            if (_proto_state != state)
            {
                _stateChanged = true;
                _proto_state = state;
            }
        }

        private void CheckConfig()
        {
            _splits = _settings.GetSplits();
            if (_splits != null)
            {
                SetConfigState(ConfigState.READY);
            }
            else
            {
                SetConfigState(ConfigState.ERROR);
            }
        }

        private void CheckConnection()
        {
            if (!_usb2snes.Connected())
            {
                if (_proto_state != ProtocolState.CONNECTING)
                {
                    SetProtocolState(ProtocolState.CONNECTING);
                    _attached_device = string.Empty;

                    Task<bool> t = _usb2snes.Connect();
                    t.ContinueWith((t1) =>
                    {
                        if (t1.Result)
                        {
                            _usb2snes.SetName("LiveSplit AutoSplitter");
                            CheckAttachment();
                        }
                        else
                        {
                            SetProtocolState(ProtocolState.FAILED_TO_CONNECT);
                        }
                    });
                }
            }
            else
            {
                CheckAttachment();
            }
        }

        private async void CheckAttachment()
        {
            if (string.IsNullOrEmpty(_attached_device) || _attached_device != _settings.Device)
            {
                if (_proto_state != ProtocolState.ATTACHING)
                {
                    SetProtocolState(ProtocolState.ATTACHING);

                    List<String> devices;
                    try
                    {
                        devices = await _usb2snes.GetDevices();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception getting devices: " + e);
                        devices = new List<String>();
                    }

                    string device = _settings.Device;
                    if (!devices.Contains(device))
                    {
                        SetProtocolState(ProtocolState.DEVICE_NOT_FOUND);
                    }
                    else
                    {
                        _usb2snes.Attach(device);
                        var info = await _usb2snes.Info(); // Info is the only neutral way to know if we are attached to the device
                        if (string.IsNullOrEmpty(info.version))
                        {
                            SetProtocolState(ProtocolState.FAILED_TO_ATTACH);
                        }
                        else
                        {
                            _attached_device = device;
                            SetProtocolState(ProtocolState.ATTACHED);
                        }
                    }
                }
            }
            else
            {
                SetProtocolState(ProtocolState.ATTACHED);
            }
        }

        private void _state_OnStart(object sender, EventArgs e)
        {
            Console.WriteLine("On START?");
            return;
        }

        private void _state_OnReset(object sender, TimerPhase value)
        {
            if (_usb2snes.Connected())
            {
                if (_settings.ResetSNES)
                {
                    _usb2snes.Reset();
                }
            }
        }

        public void Dispose()
        {
            _update_timer?.Dispose();
            if (_usb2snes.Connected())
            {
                _usb2snes.Disconnect();
            }
            _state.OnStart -= _state_OnStart;
            _state.OnReset -= _state_OnReset;
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            return _settings;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            return _settings.GetSettings(document);
        }

        public void SetSettings(XmlNode settings)
        {
            _settings.SetSettings(settings);
        }

        public int GetSettingsHashCode()
        {
            return _settings.GetSettingsHashCode();
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (invalidator != null && (_stateChanged || _ready_timer.IsRunning))
            {
                _stateChanged = false;
                invalidator.Invalidate(0, 0, width, height);
            }
        }

        public async Task DoSplit()
        {
            if (_settings.Config.igt != null && _settings.Config.igt.active == "1" && _usb2snes.Connected())
            {
                var addressSizePairs = new List<Tuple<uint, uint>>();
                addressSizePairs.Add(new Tuple<uint, uint>(_settings.Config.igt.framesAddressInt, 2));
                addressSizePairs.Add(new Tuple<uint, uint>(_settings.Config.igt.secondsAddressInt, 2));
                addressSizePairs.Add(new Tuple<uint, uint>(_settings.Config.igt.minutesAddressInt, 2));
                addressSizePairs.Add(new Tuple<uint, uint>(_settings.Config.igt.hoursAddressInt, 2));
                List<byte[]> data = null;
                try
                {
                    data = await _usb2snes.GetAddress(addressSizePairs);
                }
                catch
                {
                    Debug.WriteLine("DoSplit: Exception getting address");
                    _model.Split();
                    return;
                }

                if ((null == data) || (data.Count != addressSizePairs.Count))
                {
                    Debug.WriteLine("DoSplit: Get address failed to return result");
                }
                else
                {
                    int ms = (int)data[0][0] + ((int)data[0][1] << 8);
                    int sec = (int)data[1][0] + ((int)data[1][1] << 8);
                    int min = (int)data[2][0] + ((int)data[2][1] << 8);
                    int hr = (int)data[3][0] + ((int)data[3][1] << 8);
                    var gt = new TimeSpan(0, hr, min, sec, ms);
                    _state.SetGameTime(gt);
                }
            }
            _model.Split();
        }

        private async void UpdateSplitsWrapper()
        {
            // "_inTimer" is a very questionable attempt at locking, but it's probably fine here.
            if (_inTimer)
            {
                Debug.WriteLine("In timer already! !!!");
                return;
            }
            _inTimer = true;
            try
            {
                await UpdateSplits();
            }
            catch (Exception e)
            {
                Log.Error($"{ComponentName} failed to update splits: {e}");
                CheckConnection();
            }
            _inTimer = false;
        }

        public async Task UpdateSplits()
        {
            if (_config_state != ConfigState.READY || _state.CurrentPhase == TimerPhase.NotRunning)
            {
                CheckConfig();
            }

            if (_state.CurrentPhase == TimerPhase.NotRunning || _proto_state != ProtocolState.ATTACHED)
            {
                CheckConnection();
            }

            if (_config_state != ConfigState.READY || _proto_state != ProtocolState.ATTACHED)
            {
                _update_timer.Interval = 1000;
            }
            else
            {
                _update_timer.Interval = 33;
                if (_state.CurrentPhase == TimerPhase.NotRunning)
                {
                    if (_settings.Config.autostart != null && _settings.Config.autostart.active == "1")
                    {
                        if (await doCheckSplitWithNext(_settings.Config.autostart.GetSplit()))
                        {
                            _model.Start();
                        }
                    }
                }
                else if (_state.CurrentPhase == TimerPhase.Running)
                {
                    var splitName = _splits[_state.CurrentSplitIndex];
                    var split = _settings.Config.definitions.Where(x => x.name == splitName).First();
                    if (await doCheckSplitWithNext(split))
                    {
                        await DoSplit();
                    }
                }
            }
        }

        async Task<bool> doCheckSplitWithNext(Split split)
        {
            if (split.next == null)
            {
                return await doCheckSplit(split);
            }

            bool ok = false;
            if (split.posToCheck > 0)
            {
                ok = await doCheckSplit(split.next[split.posToCheck - 1]);
            }
            else
            {
                ok = await doCheckSplit(split);
            }
            if (ok)
            {
                if (split.posToCheck < split.next.Count())
                {
                    split.posToCheck++;
                    ok = false;
                }
                else
                {
                    split.posToCheck = 0;
                }
            }
            return ok;
        }

        async Task<bool> doCheckSplit(Split split)
        {
            var addressSizePairs = new List<Tuple<uint, uint>>();
            addressSizePairs.Add(new Tuple<uint, uint>(split.addressInt, 2));// (split.type == "smboss") ? 16 : 2));
            if (split.more != null)
            {
                foreach (var moreSplit in split.more)
                {
                    addressSizePairs.Add(new Tuple<uint, uint>(moreSplit.addressInt, 2));
                }
            }
            List<byte[]> data = null;
            try
            {
                data = await _usb2snes.GetAddress(addressSizePairs);
            }
            catch
            {
                Debug.WriteLine("doCheckSplit: Exception getting address");
                CheckConnection();
                return false;
            }

            if ((null == data) || (data.Count != addressSizePairs.Count))
            {
                Debug.WriteLine("doCheckSplit: Get address failed to return result");
                CheckConnection();
                return false;
            }

            uint value = (uint)data[0][0];
            uint word = value + ((uint)data[0][1] << 8);
            bool result = split.check(value, word);
            if (result && (split.more != null))
            {
                int dataIndex = 1;
                foreach (var moreSplit in split.more)
                {
                    value = (uint)data[dataIndex][0];
                    word = value + ((uint)data[dataIndex][1] << 8);
                    if (!moreSplit.check(value, word))
                        return false;
                    dataIndex++;
                }
            }
            return result;
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            VerticalHeight = height;
            HorizontalWidth = 3;
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            if (_first_draw_time == DateTime.MinValue)
            {
                _first_draw_time = DateTime.Now;
            }

            Color borderColor = _error_color;
            string statusMessage = string.Empty;

            if (_config_state == ConfigState.READY && _proto_state == ProtocolState.ATTACHED)
            {
                borderColor = _ok_color;

                TimeSpan elapsedTime = _ready_timer.Elapsed;
                if (elapsedTime == TimeSpan.Zero)
                {
                    _ready_timer.Start();
                }

                if (elapsedTime < ReadyAutoHideTime)
                {
                    statusMessage = $"Ready (hiding in {Math.Ceiling((ReadyAutoHideTime - elapsedTime).TotalSeconds)})";
                }
                else if (_ready_timer.IsRunning)
                {
                    _ready_timer.Stop();
                }
            }
            else
            {
                if (_ready_timer.Elapsed != TimeSpan.Zero)
                {
                    _ready_timer.Reset();
                }

                if (_config_state == ConfigState.ERROR)
                {
                    borderColor = _error_color;
                    statusMessage = $"Configuration error - check Layout Settings for {ComponentName}";
                }
                else if (_config_state == ConfigState.READY)
                {
                    switch (_proto_state)
                    {
                        case ProtocolState.CONNECTING:
                            borderColor = _connecting_color;
                            statusMessage = "Connecting to Usb2Snes";
                            break;

                        case ProtocolState.FAILED_TO_CONNECT:
                            borderColor = _error_color;
                            statusMessage = "Failed to connect to Usb2Snes";
                            break;

                        case ProtocolState.DEVICE_NOT_FOUND:
                            borderColor = _error_color;
                            statusMessage = $"Device {_settings.Device} not found. Ensure device is running a game and connected to your PC";
                            break;

                        case ProtocolState.ATTACHING:
                            borderColor = _connecting_color;
                            statusMessage = $"Attaching to {_settings.Device}";
                            break;

                        case ProtocolState.FAILED_TO_ATTACH:
                            borderColor = _error_color;
                            statusMessage = $"Failed to attach to {_settings.Device}";
                            break;
                    }
                }
            }

            float borderWidth = width - BorderThickness;
            float borderHeight = BorderThickness;

            if (_settings.ShowStatusMessage && !string.IsNullOrEmpty(statusMessage) && (DateTime.Now - _first_draw_time) > ShowStatusMessageDelay)
            {
                float textWidth = borderWidth - BorderThickness - 2 * TextPadding;

                float textHeight = g.MeasureString(statusMessage, state.LayoutSettings.TextFont, (int)textWidth).Height;
                borderHeight += textHeight + TextPadding;

                Brush textBrush = new SolidBrush(state.LayoutSettings.TextColor);
                RectangleF textRectangle = new RectangleF(TextPadding + BorderThickness, TextPadding + BorderThickness, textWidth, textHeight);
                g.DrawString(statusMessage, state.LayoutSettings.TextFont, textBrush, textRectangle);
            }

            Pen borderPen = new Pen(borderColor, BorderThickness);
            g.DrawRectangle(borderPen, BorderThickness / 2, PaddingTop + BorderThickness / 2, borderWidth, borderHeight);

            // Changing the height of this component will change the height of the whole timer form, which breaks people's ability
            // to set a size for their layout. So, if we're changing the height, save the size of the timer form to restore it next update.
            VerticalHeight = borderHeight + BorderThickness + PaddingTop + PaddingBottom;
        }
    }
}
