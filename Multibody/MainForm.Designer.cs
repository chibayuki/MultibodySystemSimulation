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
            this.Panel_SideBar = new System.Windows.Forms.Panel();
            this.Panel_View = new System.Windows.Forms.Panel();
            this.Panel_Main.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel_Main
            // 
            this.Panel_Main.BackColor = System.Drawing.Color.Transparent;
            this.Panel_Main.Controls.Add(this.Panel_SideBar);
            this.Panel_Main.Controls.Add(this.Panel_View);
            this.Panel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel_Main.Location = new System.Drawing.Point(0, 0);
            this.Panel_Main.Name = "Panel_Main";
            this.Panel_Main.Size = new System.Drawing.Size(1200, 800);
            this.Panel_Main.TabIndex = 0;
            // 
            // Panel_SideBar
            // 
            this.Panel_SideBar.BackColor = System.Drawing.Color.Transparent;
            this.Panel_SideBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.Panel_SideBar.Location = new System.Drawing.Point(900, 0);
            this.Panel_SideBar.Name = "Panel_SideBar";
            this.Panel_SideBar.Size = new System.Drawing.Size(300, 800);
            this.Panel_SideBar.TabIndex = 0;
            this.Panel_SideBar.Visible = false;
            // 
            // Panel_View
            // 
            this.Panel_View.BackColor = System.Drawing.Color.Transparent;
            this.Panel_View.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel_View.Location = new System.Drawing.Point(0, 0);
            this.Panel_View.Name = "Panel_View";
            this.Panel_View.Size = new System.Drawing.Size(1200, 800);
            this.Panel_View.TabIndex = 0;
            this.Panel_View.Paint += new System.Windows.Forms.PaintEventHandler(this.Panel_View_Paint);
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
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel Panel_Main;
        private System.Windows.Forms.Panel Panel_View;
        private System.Windows.Forms.Panel Panel_SideBar;
    }
}