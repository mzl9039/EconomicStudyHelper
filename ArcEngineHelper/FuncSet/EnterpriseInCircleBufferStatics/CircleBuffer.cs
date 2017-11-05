using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DataHelper.FuncSet.EnterpriseInCircleBufferStatics
{
    public partial class CircleBuffer : Form
    {
        public double radius { get; set; }
        public CircleBuffer()
        {
            InitializeComponent();
        }

        private void Execute()
        {
            if (this.txtBox.Text.Trim() != null)
            {
                this.radius = double.Parse(this.txtBox.Text.Trim());
                this.Close();
            }            
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            Execute();
        }

        private void btn_OK_KeyDown(object sender, KeyEventArgs e)
        {
            Execute();
        }
    }
}
