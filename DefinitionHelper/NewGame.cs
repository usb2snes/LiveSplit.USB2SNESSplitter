using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.UI.Components
{
    public partial class NewGame : Form
    {
        public String gameName;
        public NewGame()
        {
            InitializeComponent();
        }

        private void newNameTextBox_TextChanged(object sender, EventArgs e)
        {
            gameName = newNameTextBox.Text;
        }
    }
}
