namespace DataHelper.FuncSet.ShortestPath
{
    partial class GetSpeed
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
            this.btn_speed = new System.Windows.Forms.Button();
            this.txtBox_speed = new System.Windows.Forms.TextBox();
            this.lbl_speed = new System.Windows.Forms.Label();
            this.lbl_cutOff = new System.Windows.Forms.Label();
            this.txt_cutOff = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btn_speed
            // 
            this.btn_speed.Font = new System.Drawing.Font("宋体", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_speed.Location = new System.Drawing.Point(41, 88);
            this.btn_speed.Name = "btn_speed";
            this.btn_speed.Size = new System.Drawing.Size(211, 35);
            this.btn_speed.TabIndex = 0;
            this.btn_speed.Text = "确定";
            this.btn_speed.UseVisualStyleBackColor = true;
            this.btn_speed.Click += new System.EventHandler(this.btn_speed_Click);
            // 
            // txtBox_speed
            // 
            this.txtBox_speed.Location = new System.Drawing.Point(59, 37);
            this.txtBox_speed.Name = "txtBox_speed";
            this.txtBox_speed.Size = new System.Drawing.Size(62, 21);
            this.txtBox_speed.TabIndex = 1;
            this.txtBox_speed.Text = "50";
            // 
            // lbl_speed
            // 
            this.lbl_speed.AutoSize = true;
            this.lbl_speed.Font = new System.Drawing.Font("宋体", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_speed.Location = new System.Drawing.Point(9, 40);
            this.lbl_speed.Name = "lbl_speed";
            this.lbl_speed.Size = new System.Drawing.Size(45, 15);
            this.lbl_speed.TabIndex = 2;
            this.lbl_speed.Text = "速度:";
            // 
            // lbl_cutOff
            // 
            this.lbl_cutOff.AutoSize = true;
            this.lbl_cutOff.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_cutOff.Location = new System.Drawing.Point(127, 39);
            this.lbl_cutOff.Name = "lbl_cutOff";
            this.lbl_cutOff.Size = new System.Drawing.Size(64, 16);
            this.lbl_cutOff.TabIndex = 3;
            this.lbl_cutOff.Text = "CutOff:";
            // 
            // txt_cutOff
            // 
            this.txt_cutOff.Location = new System.Drawing.Point(194, 36);
            this.txt_cutOff.Name = "txt_cutOff";
            this.txt_cutOff.Size = new System.Drawing.Size(71, 21);
            this.txt_cutOff.TabIndex = 4;
            this.txt_cutOff.Text = "180";
            // 
            // GetSpeed
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 135);
            this.Controls.Add(this.txt_cutOff);
            this.Controls.Add(this.lbl_cutOff);
            this.Controls.Add(this.lbl_speed);
            this.Controls.Add(this.txtBox_speed);
            this.Controls.Add(this.btn_speed);
            this.Name = "GetSpeed";
            this.Text = "输入点到线的速度";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_speed;
        private System.Windows.Forms.TextBox txtBox_speed;
        private System.Windows.Forms.Label lbl_speed;
        private System.Windows.Forms.Label lbl_cutOff;
        private System.Windows.Forms.TextBox txt_cutOff;
    }
}