namespace Multibody
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.Panel_Main = new System.Windows.Forms.Panel();
            this.PictureBox_View = new System.Windows.Forms.PictureBox();
            this.Panel_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox_View)).BeginInit();
            this.SuspendLayout();
            // 
            // Panel_Main
            // 
            this.Panel_Main.BackColor = System.Drawing.Color.Transparent;
            this.Panel_Main.Controls.Add(this.PictureBox_View);
            this.Panel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel_Main.Location = new System.Drawing.Point(0, 0);
            this.Panel_Main.Name = "Panel_Main";
            this.Panel_Main.Size = new System.Drawing.Size(1200, 800);
            this.Panel_Main.TabIndex = 0;
            // 
            // PictureBox_View
            // 
            this.PictureBox_View.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PictureBox_View.Location = new System.Drawing.Point(0, 0);
            this.PictureBox_View.Name = "PictureBox_View";
            this.PictureBox_View.Size = new System.Drawing.Size(1200, 800);
            this.PictureBox_View.TabIndex = 0;
            this.PictureBox_View.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.Panel_Main);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Panel_Main.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox_View)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel Panel_Main;
        private System.Windows.Forms.PictureBox PictureBox_View;
    }
}