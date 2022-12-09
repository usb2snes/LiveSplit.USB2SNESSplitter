using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;

using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Diagnostics;
using USB2SnesW;
using System.Timers;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Newtonsoft.Json;
using System.Security.Policy;
using System.Web.UI.WebControls;

namespace LiveSplit.UI.Components
{
    public partial class MainUI: Form
    {
        enum Usb2SnesState
        {
            NoState,
            TryingToConnect,
            Connected,
            GettingDevices,
            NoDevice,
            TryingToAttach,
            Attached,
            Ready
        };
        private Dictionary<String, String> typeToText = new Dictionary<String, String>()
        {
            { "eq", "byte equal"},
            { "weq", "word equal" },
            { "bit", "byte bit set" },
            { "wbit", "word bit set"},
            { "lt", "byte less than"},
            { "wlt", "word less than" },
            { "lte", "byte less or equal" },
            { "wlte", "word less or equal"},
            { "gt", "byte greater than"},
            { "wgt", "word greater than"},
            { "gte", "byte greater or equal"},
            { "wgte", "word greater or equal" }
        };
        private Game game;
        private Split currentSplit;
        private int subSplitIndex = 0;
        private String fileName = null;
        private bool hasNext = false;
        private bool hasMore = false;
        private string attachedDevice;
        private USB2SnesW.USB2SnesW usb2snes;
        private Usb2SnesState connectionState;
        private DispatcherTimer usb2snesCoTimer;
        private SplitChecker splitChecker;

        public MainUI()
        {
            InitializeComponent();
            usb2snes = new USB2SnesW.USB2SnesW();
            connectionState = Usb2SnesState.NoState;
            usb2snesCoTimer = new DispatcherTimer();
            usb2snesCoTimer.Interval = new TimeSpan(0, 0, 3);
            usb2snesCoTimer.Start();
            usb2snesCoTimer.Tick += onTimerElapsed;
            splitChecker = new SplitChecker(this, usb2snes);
            splitChecker.onSplitOk = splitCheckIsGood;
            splitChecker.onSplitNok = splitCheckIsBad;
            splitChecker.onCheckedSplit = currentlyChecking;
            splitChecker.onUsb2snesError = usb2snesCommandFailed;
            var listValues = typeToText.Values.ToList<string>();

            listValues.Sort();
            foreach (string s in listValues)
            {
                typeComboBox.Items.Add(s);
                subSplitTypeComboBox.Items.Add(s);
            }
            subSplitUIEnable(false);
            SetUIEnable(false);
        }
        private void disableEvent()
        {
            listSplits.SelectedIndexChanged -= listSplits_SelectedIndexChanged;
            addressTextBox.TextChanged -= addressTextBox_TextChanged;
            valueDecTextBox.TextChanged -= valueDecTextBox_TextChanged;
            valueTextBox.TextChanged -= valueTextBox_TextChanged;
            typeComboBox.SelectedIndexChanged -= typeComboBox_SelectedIndexChanged;

            subSplitView.SelectionChanged -= subSplitView_SelectionChanged;
            subSplitAddress.TextChanged -= subSplitAddress_TextChanged;
            subSplitTypeComboBox.SelectedIndexChanged -= subSplitTypeComboBox_SelectedIndexChanged;
            subSplitValue.TextChanged -= subSplitValue_TextChanged;
            subSplitValueDecTextBox.TextChanged -= subSplitValueDecTextBox_TextChanged;
        }

        private void enableEvent()
        {
            listSplits.SelectedIndexChanged += listSplits_SelectedIndexChanged;
            addressTextBox.TextChanged += addressTextBox_TextChanged;
            valueDecTextBox.TextChanged += valueDecTextBox_TextChanged;
            valueTextBox.TextChanged += valueTextBox_TextChanged;
            typeComboBox.SelectedIndexChanged += typeComboBox_SelectedIndexChanged;

            subSplitView.SelectionChanged += subSplitView_SelectionChanged;
            subSplitAddress.TextChanged += subSplitAddress_TextChanged;
            subSplitTypeComboBox.SelectedIndexChanged += subSplitTypeComboBox_SelectedIndexChanged;
            subSplitValue.TextChanged += subSplitValue_TextChanged;
            subSplitValueDecTextBox.TextChanged += subSplitValueDecTextBox_TextChanged;
        }
        private void splitCheckIsGood()
        {
            splitOkButton.BackColor = Color.Green;
            checkButton.Text = "Check";
        }
        private void splitCheckIsBad()
        {
            splitOkButton.BackColor = Color.Orange;
        }
        private void currentlyChecking(Split split, uint word)
        {
            //Console.WriteLine(String.Format("Main Ui split checking value{0:X} : {1:X}", split.addressInt, word));
            checkStatusLabel.Text = String.Format("Value of address {0:X} : {1:X}", split.addressInt, word);
        }
        private void onTimerElapsed(Object source, EventArgs e)
        {
            //Console.WriteLine("Usb2Snes connection timer timout");
            if (connectionState != Usb2SnesState.Ready)
                tryToConnect();
        }
        private void usb2snesCommandFailed()
        {
            connectionState = Usb2SnesState.NoState;
        }
        async private void tryToConnect()
        {
            //Console.WriteLine("Trying to Connect");
            if (connectionState == Usb2SnesState.NoState)
            {
                usb2snesLabel.Text = "Trying to connect";
                connectionState = Usb2SnesState.TryingToConnect;
                bool co = await usb2snes.Connect();
                if (co)
                {
                    connectionState = Usb2SnesState.Connected;
                    usb2snes.SetName("LiveSplit Definition Helper");
                }
                else
                {
                    connectionState = Usb2SnesState.NoState;
                    usb2snesLabel.Text = "Failed to connect to Usb2Snes";
                    return;
                }
            }
            if (connectionState == Usb2SnesState.Connected)
            {
                List<String> devices;
                usb2snesLabel.Text = "Trying to get devices";
                try
                {
                    connectionState = Usb2SnesState.GettingDevices;
                    devices = await usb2snes.GetDevices();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception getting devices: " + e);
                    devices = new List<String>();
                    usb2snesCommandFailed();
                }
                if (devices.Count == 0)
                {
                    usb2snesLabel.Text = "No device found";
                    connectionState = Usb2SnesState.Connected;
                    return;
                }
                usb2snes.Attach(devices[0]);
                connectionState = Usb2SnesState.TryingToAttach;
                var info = await usb2snes.Info(); // Info is the only neutral way to know if we are attached to the device
                if (string.IsNullOrEmpty(info.version))
                {
                    connectionState = Usb2SnesState.Connected;
                }
                else
                {
                    connectionState = Usb2SnesState.Attached;
                    attachedDevice = devices[0];
                    //SetProtocolState(ProtocolState.ATTACHED);
                    connectionState = Usb2SnesState.Ready;
                    usb2snesLabel.Text = "Usb2Snes connection ready";
                    checkButton.Enabled = true;
                }
            }
        }

        public void changeSubSplitStatus(int index, Color col)
        {
            subSplitView.Rows[index].Cells["subSplitStatus"].Style.BackColor = col;
        }
        private void OpenGame_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Please Choose a json to open";
            openFileDialog.Filter = "Json File | *.json";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string configJson;
                try
                {
                    fileName = openFileDialog.FileName;
                    configJson = File.ReadAllText(openFileDialog.FileName);
                }
                catch (Exception exp)
                {
                    return;
                }
                game = Game.FromJSON(configJson);
                disableEvent();
                clearUi();
                TitleLabel.Text = game.name;
                listSplits.Items.Add("Autostart");
                if (game.definitions != null)
                {
                    foreach (var def in game.definitions)
                    {
                        listSplits.Items.Add(def.name);
                    }
                }
                enableEvent();
                listSplits.SetSelected(0, true);
                SetUIEnable(true);
                //currentSplit = game.autostart.GetSplit();
                //updateSplitInfos();
            }
        }
        private void clearUi()
        {
            listSplits.Items.Clear();
            addressTextBox.Clear();
            subSplitAddress.Clear();
            valueTextBox.Clear();
            valueDecTextBox.Clear();
            splitNameTextBox.Clear();
            subSplitValue.Clear();
            subSplitValueDecTextBox.Clear();
            subSplitTypeComboBox.Text = "";
            typeComboBox.Text = "";
        }
        private void SetUIEnable(bool enabled)
        {
            listSplits.Enabled = enabled;
            addressTextBox.Enabled = enabled;
            splitNameTextBox.Enabled = enabled;
            valueTextBox.Enabled = enabled;
            valueDecTextBox.Enabled = enabled;
            typeComboBox.Enabled = enabled;
            buttonOrderDown.Enabled = enabled;
            buttonOrderUp.Enabled = enabled;
            addSplitButton.Enabled = enabled;
            delSplitButton.Enabled = enabled;
            addNextButton.Enabled = enabled;
            addMoreButton.Enabled = enabled;
        }

        private void listSplits_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Split selection changed");
            if (listSplits.SelectedItems.Count == 0)
                return;
            int index = listSplits.SelectedIndices[0];
            var itemName = listSplits.SelectedItems[0];
            if (index == 0)
            {
                currentSplit = game.autostart;
                delSplitButton.Enabled = false;
                updateSplitInfos();
                return;
            }
            delSplitButton.Enabled = true;
            foreach (var def in game.definitions)
            {
                if (itemName.ToString() == def.name)
                {
                    currentSplit = def;
                    updateSplitInfos();
                    break;
                }
            }
        }
        private void updateSplitInfos()
        {
            Console.WriteLine("==Update split info");
            disableEvent();
            splitNameTextBox.Text = currentSplit.name;
            addressTextBox.Text = currentSplit.address;
            if (currentSplit.type != null)
                typeComboBox.Text = typeToText[currentSplit.type];
            valueTextBox.Text = currentSplit.value;
            valueDecTextBox.Text = String.Format("{0}", currentSplit.valueInt);
            subSplitView.Rows.Clear();
            hasMore = false;
            hasNext = false;
            if (currentSplit.next == null && currentSplit.more == null)
            {
                addMoreButton.Enabled = true;
                addNextButton.Enabled = true;
                subSplitUIEnable(false);
            }
            else
            {
                addMoreButton.Enabled = false;
                addNextButton.Enabled = false;
            }
            subSplitAddress.Text = "";
            subSplitTypeComboBox.Text = "";
            subSplitTypeComboBox.Text = "";
            if (currentSplit.next != null)
            {
                hasNext = true;
                addMoreButton.Enabled = false;
                addNextButton.Enabled = true;
                subSplitLabel.Text = "Has Next";
                int cpt = 1;
                foreach (var sp in currentSplit.next)
                {
                    if (sp.name != null)
                        subSplitView.Rows.Add(sp.name);
                    else
                        subSplitView.Rows.Add(String.Format("Next {0}", cpt));
                    cpt++;
                }
            }
            if (currentSplit.more != null)
            {
                hasMore = true;
                addNextButton.Enabled = false;
                addMoreButton.Enabled = true;
                subSplitLabel.Text = "Has More";
                int cpt = 1;
                foreach (var sp in currentSplit.more)
                {
                    if (sp.name != null)
                        subSplitView.Rows.Add(sp.name);
                    else
                        subSplitView.Rows.Add(String.Format("Next {0}", cpt));
                    cpt++;
                }
            }
            enableEvent();
            if (hasMore || hasNext)
            {
                subSplitView.Rows[0].Selected = true;
            }
            Console.WriteLine("==End Update split info");
        }

        private void splitNameTextBox_TextChanged(object sender, EventArgs e)
        {
            currentSplit.name = splitNameTextBox.Text;
            if (listSplits.SelectedItems.Count > 0)
            {
                listSplits.Items[listSplits.SelectedIndex] = currentSplit.name;
            }
        }
        private void addressTextBox_TextChanged(object sender, EventArgs e)
        {
            currentSplit.address = addressTextBox.Text;
        }
        private void typeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (typeComboBox.Text == "")
                return;
            string result = "";
            foreach (var kv in typeToText)
            {
                if (kv.Value == typeComboBox.Text)
                {
                    result = kv.Key;
                    break;
                }
            }
            currentSplit.type = result;
        }
        private void valueTextBox_TextChanged(object sender, EventArgs e)
        {
            currentSplit.value = valueTextBox.Text;
            valueDecTextBox.Text = String.Format("{0}", currentSplit.valueInt);
        }
        private void valueDecTextBox_TextChanged(object sender, EventArgs e)
        {
            currentSplit.value = String.Format("0x{0:X}", Convert.ToUInt32(valueDecTextBox.Text));
            valueTextBox.Text = currentSplit.value;
        }

        private void NewGame_Click(object sender, EventArgs e)
        {
            NewGame diag = new NewGame();
            var result = diag.ShowDialog();
            fileName = null;
            if (result == DialogResult.OK)
            {
                game = new Game();
                game.autostart = new Autostart();
                game.autostart.active = "0";
                game.autostart.name = "Autostart";
                clearUi();
                listSplits.Items.Add("Autostart");
                currentSplit = game.autostart;
                game.name = diag.gameName;
                TitleLabel.Text = game.name;
                game.definitions = new List<Split>();
                updateSplitInfos();
                SetUIEnable(true);
            }
            
        }

        private void addSplitButton_Click(object sender, EventArgs e)
        {
            Split newSplit = new Split();
            newSplit.name = "New Definition";
            newSplit.address = "0x0";
            newSplit.type = "eq";
            newSplit.value = "0x0";
            if (game.definitions == null)
                game.definitions = new List<Split>();
            game.definitions.Add(newSplit);
            listSplits.Items.Add(newSplit.name);
            listSplits.SelectedItems.Clear();
            listSplits.SetSelected(listSplits.Items.Count - 1, true);
            currentSplit = newSplit;
            updateSplitInfos();
            listSplits.Select();
        }

        private void SaveGame_Click(object sender, EventArgs e)
        {
            if (fileName == null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Json File | *.json";
                var result = saveFileDialog.ShowDialog();
                if (result == DialogResult.OK)
                    fileName = saveFileDialog.FileName;
                else
                    return;
            }
            String json = JsonConvert.SerializeObject(game, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(fileName, json);
        }
        private void initCheck()
        {
            if (hasNext || hasMore)
            {
                foreach (DataGridViewRow row in subSplitView.Rows)
                {
                    row.Cells["subSplitStatus"].Style.BackColor = Color.Gray;
                }
                currentSplit.posToCheck = 0;
            }
        }
        private void checkButton_Click(object sender, EventArgs e)
        {
            if (splitChecker.isActivated)
            {
                checkButton.Text = "Check Split";
                splitOkButton.BackColor = Color.Gray;
                splitChecker.StopCheck();
            }
            else
            {
                initCheck();
                checkButton.Text = "Stop Checking";
                splitOkButton.BackColor = Color.Orange;
                splitChecker.StartCheck(currentSplit);
            }
        }

        private void addNextButton_Click(object sender, EventArgs e)
        {
            Split split = new Split();
            hasNext = true;
            split.name = currentSplit.next == null ? "Next 1" : String.Format("Next {0}", currentSplit.next.Count() + 1);
            split.address = "";
            split.value = "";
            if (currentSplit.next == null)
            {
                currentSplit.next = new List<Split>();
            }
            currentSplit.next.Add(split);
            subSplitIndex = currentSplit.next.Count() - 1;
            subSplitView.Rows.Add(split.name);
            subSplitView.Rows[subSplitIndex].Selected = true;
        }
        private void addMoreButton_Click(object sender, EventArgs e)
        {
            Split split = new Split();
            hasMore = true;
            split.name = currentSplit.more == null ? "More 1" : String.Format("More {0}", currentSplit.more.Count() + 1);
            split.address = "";
            split.value = "";
            if (currentSplit.more == null)
            {
                currentSplit.more = new List<Split>();
            }
            currentSplit.more.Add(split);
            subSplitIndex = currentSplit.more.Count() - 1;
            subSplitView.Rows.Add(split.name);
            subSplitView.Rows[subSplitIndex].Selected = true;
        }
        private void subSplitUIEnable(bool enabled)
        {
            subSplitAddress.Enabled = enabled;
            subSplitTypeComboBox.Enabled = enabled;
            subSplitValue.Enabled = enabled;
            subSplitValueDecTextBox.Enabled = enabled;
        }
        private void subSplitAddress_TextChanged(object sender, EventArgs e)
        {
            if (hasNext == false && hasMore == false)
                return;
            Split subSplit = null;
            if (hasNext)
                subSplit = currentSplit.next[subSplitIndex];
            if (hasMore)
                subSplit = currentSplit.more[subSplitIndex];
            subSplit.address = subSplitAddress.Text;
        }

        private void subSplitTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((hasNext == false && hasMore == false) || subSplitTypeComboBox.Text == "")
                return;
            Split subSplit = null;
            if (hasNext)
                subSplit = currentSplit.next[subSplitIndex];
            if (hasMore)
                subSplit = currentSplit.more[subSplitIndex];
            string result = "";
            foreach (var kv in typeToText)
            {
                if (kv.Value == subSplitTypeComboBox.Text)
                {
                    result = kv.Key;
                    break;
                }
            }
            subSplit.type = result;
        }

        private void subSplitValue_TextChanged(object sender, EventArgs e)
        {
            if (hasNext == false && hasMore == false)
                return;
            Split subSplit = null;
            if (hasNext)
                subSplit = currentSplit.next[subSplitIndex];
            if (hasMore)
                subSplit = currentSplit.more[subSplitIndex];
            subSplit.value = subSplitValue.Text;
            subSplitValueDecTextBox.Text = String.Format("{0}", subSplit.valueInt);
        }

        private void subSplitValueDecTextBox_TextChanged(object sender, EventArgs e)
        {
            if (hasNext == false && hasMore == false)
                return;
            Split subSplit = null;
            if (hasNext)
                subSplit = currentSplit.next[subSplitIndex];
            if (hasMore)
                subSplit = currentSplit.more[subSplitIndex];
            subSplit.value = String.Format("0x{0:X}", Convert.ToUInt32(subSplitValueDecTextBox.Text));
            subSplitValue.Text = subSplit.value;
        }

        private void subSplitView_SelectionChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Selection changed");
            if (subSplitView.SelectedCells.Count == 0)
                return;
            if (subSplitValue.Enabled == false)
                subSplitUIEnable(true);
            Split subSplit = null;
            subSplitIndex = subSplitView.SelectedCells[0].RowIndex;
            Console.WriteLine("subsplit Row changed");
            if (hasNext)
                subSplit = currentSplit.next[subSplitIndex];
            if (hasMore)
                subSplit = currentSplit.more[subSplitIndex];
            subSplitAddress.Text = subSplit.address;
            if (subSplit.type != null)
                subSplitTypeComboBox.Text = typeToText[subSplit.type];
            subSplitValue.Text = subSplit.value;
        }

        private void delSplit_Click(object sender, EventArgs e)
        {
            int index = listSplits.SelectedIndex;
            if (index == 0)
                return;
            listSplits.Items.RemoveAt(index);
            game.definitions.RemoveAt(index - 1);
        }

        private void buttonOderUp_Click(object sender, EventArgs e)
        {
            int index = listSplits.SelectedIndices[0];
            if (index < 2)
                return;
            int indexInDef = index - 1;
            Split changedSplit = game.definitions[indexInDef];
            game.definitions[indexInDef] = game.definitions[indexInDef - 1];
            game.definitions[indexInDef - 1] = changedSplit;
            listSplits.Items[index - 1] = changedSplit.name;
            listSplits.Items[index] = game.definitions[indexInDef].name;
            listSplits.SetSelected(index - 1, true);
        }

        private void buttonOrderDown_Click(object sender, EventArgs e)
        {
            int index = listSplits.SelectedIndices[0];
            if (index == listSplits.Items.Count - 1)
                return;
            int indexInDef = index - 1;
            Split changedSplit = game.definitions[indexInDef];
            game.definitions[indexInDef] = game.definitions[indexInDef + 1];
            game.definitions[indexInDef + 1] = changedSplit;
            listSplits.Items[index + 1] = changedSplit.name;
            listSplits.Items[index] = game.definitions[indexInDef].name;
            listSplits.SetSelected(index + 1, true);
        }
    }
}
