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
        private DateTime _first_draw_time = DateTime.MinValue;
        private Stopwatch _ready_timer;
        private bool _inTimer;
        private Connector _connector;
        private int _ready_attempt = 0;
        private Color _ok_color = Color.FromArgb(0, 128, 0);
        private Color _error_color = Color.FromArgb(128, 0, 0);
        private Color _connecting_color = Color.FromArgb(128, 128, 0);
        private bool _stateChanged;

        private void init(LiveSplitState state, Connector connector)
        {
            _state = state;
            _config_state = ConfigState.NONE;
            _ready_timer = new Stopwatch();
            _settings = new ComponentSettings(_state)
            {
                Dock = DockStyle.Fill,
            };
            _model = new TimerModel() { CurrentState = _state };
            _state.RegisterTimerModel(_model);
            //_stateChanged = false;
            _splits = new List<string>();
            _inTimer = false;

            _update_timer = new Timer() { Interval = 1000 };
            _update_timer.Tick += (sender, args) => UpdateSplitsWrapper();
            _update_timer.Enabled = true;

            _state.OnReset += _state_OnReset;
            _state.OnStart += _state_OnStart;
            HorizontalWidth = 3;
            VerticalHeight = 3;

            _connector = connector;
        }

        public USB2SNESComponent(LiveSplitState state)
        {
            init(state, new Connector());
        }

        private void SetConfigState(ConfigState state)
        {
            if (_config_state != state)
            {
                _stateChanged = true;
                _config_state = state;
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

        private async Task CheckConnection()
        {
            if (_connector.State() == Connector.ConnectorState.READY)
                return;
            if (_connector.State() == Connector.ConnectorState.NONE)
            {
                
                bool connected = await _connector.Connect();
                if (connected)
                {
                    await _connector.SetName("LiveSplit AutoSplitter");
                    _ready_attempt = 0;
                    //await _connector.GetReady();
                }
                _stateChanged = true;
            }
            else
            {
                if (_connector.State() == Connector.ConnectorState.CONNECTED)
                {
                    _stateChanged = true;
                    Debug.WriteLine("Ready attempt : ", _ready_attempt);
                    bool ready = await _connector.GetReady();
                    if (ready == false)
                        _ready_attempt++;
                    if (_ready_attempt == 5)
                    {
                        _connector.Disconnect();
                        _ready_attempt = 0;
                    }
                }
            }
        }

        private void _state_OnStart(object sender, EventArgs e)
        {
            Console.WriteLine("On START?");
            return;
        }

        private void _state_OnReset(object sender, TimerPhase value)
        {
            if (_connector.State() != Connector.ConnectorState.NONE)
            {
                if (_settings.ResetSNES)
                {
                    _connector.Reset();
                }
            }
        }

        public void Dispose()
        {
            _update_timer?.Dispose();
            if (_connector.State() != Connector.ConnectorState.NONE)
            {
                _connector.Disconnect();
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
                //_stateChanged = false;
                invalidator.Invalidate(0, 0, width, height);
            }
        }

        public async Task DoSplit()
        {
            if (_settings.Config.igt != null && _settings.Config.igt.active == "1" && _connector.State() == Connector.ConnectorState.READY)
            {
                var addressSizePairs = new List<Tuple<uint, uint>>();
                addressSizePairs.Add(new Tuple<uint, uint>(_settings.Config.igt.framesAddressInt, 2));
                addressSizePairs.Add(new Tuple<uint, uint>(_settings.Config.igt.secondsAddressInt, 2));
                addressSizePairs.Add(new Tuple<uint, uint>(_settings.Config.igt.minutesAddressInt, 2));
                addressSizePairs.Add(new Tuple<uint, uint>(_settings.Config.igt.hoursAddressInt, 2));
                List<byte[]> data = null;
                try
                {
                    data = await _connector.GetAddress(addressSizePairs);
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
                    int frames = (int)data[0][0] + ((int)data[0][1] << 8);
                    int ms = (frames * 1000) / 60;
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
                await CheckConnection();
            }
            _inTimer = false;
        }

        public async Task UpdateSplits()
        {
            if (_config_state != ConfigState.READY || _state.CurrentPhase == TimerPhase.NotRunning)
            {
                CheckConfig();
            }

            if (_state.CurrentPhase == TimerPhase.NotRunning || _connector.State() != Connector.ConnectorState.READY)
            {
                await CheckConnection();
            }

            if (_config_state != ConfigState.READY || _connector.State() != Connector.ConnectorState.READY)
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
            addressSizePairs.Add(new Tuple<uint, uint>(split.addressInt, 2));
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
                data = await _connector.GetAddress(addressSizePairs);
            }
            catch
            {
                Debug.WriteLine("doCheckSplit: Exception getting address");
                await CheckConnection();
                return false;
            }

            if ((null == data) || (data.Count != addressSizePairs.Count))
            {
                Debug.WriteLine("doCheckSplit: Get address failed to return result");
                await CheckConnection();
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

            if (_config_state == ConfigState.READY && _connector.State() == Connector.ConnectorState.READY)
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
                    //Debug.WriteLine("Update ui Connector state : " + _connector.State());
                    switch (_connector.State())
                    {
                        case Connector.ConnectorState.CONNECTING:
                            borderColor = _connecting_color;
                            statusMessage = "Connecting to Usb2SnesW or NWA emulator";
                            break;

                        case Connector.ConnectorState.CONNECTED:
                            borderColor = _connecting_color;
                            statusMessage = $"Connected to {_connector.ConnectorName()}";
                            break;

                        case Connector.ConnectorState.GETTING_READY:
                            borderColor = _connecting_color;
                            statusMessage = $"Connected to {_connector.ConnectorName()}, getting ready";
                            break;
                        case Connector.ConnectorState.NONE:
                            borderColor = _error_color;
                            statusMessage = "Connected to nothing";
                            break;
                    }
                    if (_connector.Error() != Connector.ConnectorError.NONE)
                    {
                        switch (_connector.Error())
                        {
                            case Connector.ConnectorError.NO_SERVER:
                                borderColor = _error_color;
                                statusMessage = "Nothing to connect to. No Usb2snesW server or Emulator with NWA support";
                                break;

                            case Connector.ConnectorError.USB2SNES_NO_DEVICE:
                                borderColor = _error_color;
                                statusMessage = "No device to attach on the Usb2SnesW server";
                                break;
                            case Connector.ConnectorError.NWA_NO_VALID_CORE:
                                borderColor = _error_color;
                                statusMessage = $"Emulator {_connector.DeviceName()} does not have a SNES core loaded";
                                break;
                            case Connector.ConnectorError.NWA_NO_GAME_RUNNING:
                                borderColor = _error_color;
                                statusMessage = $"Emulator {_connector.DeviceName()} is not running a game";
                                break;
                            case Connector.ConnectorError.NWA_ERROR:
                                borderColor = _error_color;
                                statusMessage = $"Emulator {_connector.DeviceName()} error";
                                break;
                        }
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
