using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DataHelper.FuncSet.ShortestPath
{
    public partial class GetSpeed : Form
    {
        private double speed = -1;
        private string cutOff = "";

        public double Speed() {
            return this.speed;
        }

        public string CutOff() {
            return this.cutOff;
        }

        public GetSpeed()
        {
            InitializeComponent();
        }

        private void init()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtBox_speed.Text))
                {
                    Log.Log.Warn("必须填写速度值!");
                    return;
                }
                else
                {
                    string val = txtBox_speed.Text.Trim();
                    this.speed = double.Parse(val);
                }
                if (string.IsNullOrWhiteSpace(txt_cutOff.Text))
                {
                    Log.Log.Warn("必须填写默认的CutOff值");
                    return;
                }
                else
                {
                    string val = txt_cutOff.Text.Trim();
                    this.cutOff = val;                   
                }
                this.DialogResult = DialogResult.OK;
            }
            catch (System.Exception ex)
            {
                Log.Log.Error("解析速度失败，可能是填写的格式不正确", ex);
            }
            finally
            {
                this.Close();
            }
        }

        private void btn_speed_Click(object sender, EventArgs e)
        {
            init();
        }  
    }
}
