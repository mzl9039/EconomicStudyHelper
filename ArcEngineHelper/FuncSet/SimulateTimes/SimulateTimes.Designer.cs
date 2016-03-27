namespace DataHelper.FuncSet.SimulateTimes
{
    partial class SimulateTimes
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbl_SetSimuTimes = new System.Windows.Forms.Label();
            this.txtBox_SetSimuTimes = new System.Windows.Forms.TextBox();
            this.btn_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbl_SetSimuTimes
            // 
            this.lbl_SetSimuTimes.AutoSize = true;
            this.lbl_SetSimuTimes.Location = new System.Drawing.Point(52, 36);
            this.lbl_SetSimuTimes.Name = "lbl_SetSimuTimes";
            this.lbl_SetSimuTimes.Size = new System.Drawing.Size(53, 12);
            this.lbl_SetSimuTimes.TabIndex = 0;
            this.lbl_SetSimuTimes.Text = "模拟值：";
            // 
            // txtBox_SetSimuTimes
            // 
            this.txtBox_SetSimuTimes.Location = new System.Drawing.Point(132, 33);
            this.txtBox_SetSimuTimes.Name = "txtBox_SetSimuTimes";
            this.txtBox_SetSimuTimes.Size = new System.Drawing.Size(100, 21);
            this.txtBox_SetSimuTimes.TabIndex = 1;
            // 
            // btn_OK
            // 
            this.btn_OK.Location = new System.Drawing.Point(156, 89);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(75, 23);
            this.btn_OK.TabIndex = 2;
            this.btn_OK.Text = "确定";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // SimulateTimes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 143);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.txtBox_SetSimuTimes);
            this.Controls.Add(this.lbl_SetSimuTimes);
            this.Name = "SimulateTimes";
            this.Text = "设置模拟次数";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SimulateTimes_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_SetSimuTimes;
        private System.Windows.Forms.TextBox txtBox_SetSimuTimes;
        private System.Windows.Forms.Button btn_OK;
    }
}