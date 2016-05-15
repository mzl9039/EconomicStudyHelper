/*
 * Created by SharpDevelop.
 * User: mzl
 * Date: 2015-10-16
 * Time: 18:54
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace EconomicStudyHelper
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            this.btn_Start = new System.Windows.Forms.Button();
            this.cbxFuncType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lbl_kd = new System.Windows.Forms.Label();
            this.cbxKdFuncType = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btn_Start
            // 
            this.btn_Start.Location = new System.Drawing.Point(108, 100);
            this.btn_Start.Name = "btn_Start";
            this.btn_Start.Size = new System.Drawing.Size(75, 23);
            this.btn_Start.TabIndex = 0;
            this.btn_Start.Text = "开始运行";
            this.btn_Start.UseVisualStyleBackColor = true;
            this.btn_Start.Click += new System.EventHandler(this.Btn_StartClick);
            // 
            // cbxFuncType
            // 
            this.cbxFuncType.FormattingEnabled = true;
            this.cbxFuncType.Location = new System.Drawing.Point(108, 29);
            this.cbxFuncType.Name = "cbxFuncType";
            this.cbxFuncType.Size = new System.Drawing.Size(121, 20);
            this.cbxFuncType.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(59, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "方法：";
            // 
            // lbl_kd
            // 
            this.lbl_kd.AutoSize = true;
            this.lbl_kd.Location = new System.Drawing.Point(35, 71);
            this.lbl_kd.Name = "lbl_kd";
            this.lbl_kd.Size = new System.Drawing.Size(65, 12);
            this.lbl_kd.TabIndex = 3;
            this.lbl_kd.Text = "K(d)类型：";
            // 
            // cbxKdFuncType
            // 
            this.cbxKdFuncType.FormattingEnabled = true;
            this.cbxKdFuncType.Location = new System.Drawing.Point(108, 64);
            this.cbxKdFuncType.Name = "cbxKdFuncType";
            this.cbxKdFuncType.Size = new System.Drawing.Size(121, 20);
            this.cbxKdFuncType.TabIndex = 4;
            this.cbxKdFuncType.SelectedIndexChanged += new System.EventHandler(this.cbxKdFuncType_SelectedIndexChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 149);
            this.Controls.Add(this.cbxKdFuncType);
            this.Controls.Add(this.lbl_kd);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbxFuncType);
            this.Controls.Add(this.btn_Start);
            this.Name = "MainForm";
            this.Text = "EconomicStudyHelper";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private System.Windows.Forms.Button btn_Start;
        private System.Windows.Forms.ComboBox cbxFuncType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbl_kd;
        private System.Windows.Forms.ComboBox cbxKdFuncType;
    }
}
