namespace DataHelper.FuncSet.EnterpriseInCircleBufferStatics
{
    partial class CircleBuffer
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
            this.btn_intro = new System.Windows.Forms.Button();
            this.txtBox = new System.Windows.Forms.TextBox();
            this.btn_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_intro
            // 
            this.btn_intro.Location = new System.Drawing.Point(18, 28);
            this.btn_intro.Name = "btn_intro";
            this.btn_intro.Size = new System.Drawing.Size(161, 23);
            this.btn_intro.TabIndex = 0;
            this.btn_intro.Text = "请输入要搜索的圆的半径：";
            this.btn_intro.UseVisualStyleBackColor = true;
            // 
            // txtBox
            // 
            this.txtBox.Location = new System.Drawing.Point(196, 30);
            this.txtBox.Name = "txtBox";
            this.txtBox.Size = new System.Drawing.Size(126, 21);
            this.txtBox.TabIndex = 1;
            // 
            // btn_OK
            // 
            this.btn_OK.Location = new System.Drawing.Point(129, 78);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(109, 23);
            this.btn_OK.TabIndex = 2;
            this.btn_OK.Text = "确定";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            this.btn_OK.KeyDown += new System.Windows.Forms.KeyEventHandler(this.btn_OK_KeyDown);
            // 
            // CircleBuffer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(348, 134);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.txtBox);
            this.Controls.Add(this.btn_intro);
            this.Name = "CircleBuffer";
            this.Text = "读取半径";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_intro;
        private System.Windows.Forms.TextBox txtBox;
        private System.Windows.Forms.Button btn_OK;
    }
}