using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DataHelper.FuncSet.SimulateTimes
{
    public partial class SimulateTimes : Form
    {
        public int  SimulateTime { get; set; }
        public SimulateTimes()
        {
            InitializeComponent();
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            Excute();
        }

        private void SimulateTimes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Excute();
            }
        }

        private void Excute()
        {
            if (this.txtBox_SetSimuTimes.Text.Trim() != null)
                SimulateTime = int.Parse(this.txtBox_SetSimuTimes.Text.Trim());
            this.Close();
        }
    }
}
