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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.Panel_Main = new System.Windows.Forms.Panel();
            this.Panel_SideBar = new System.Windows.Forms.Panel();
            this.Panel_View = new System.Windows.Forms.Panel();
            this.Label_HelpMessage = new System.Windows.Forms.Label();
            this.Label_PressedKey = new System.Windows.Forms.Label();
            this.Panel_Main.SuspendLayout();
            this.Panel_View.SuspendLayout();
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
            this.Panel_View.Controls.Add(this.Label_HelpMessage);
            this.Panel_View.Controls.Add(this.Label_PressedKey);
            this.Panel_View.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel_View.Location = new System.Drawing.Point(0, 0);
            this.Panel_View.Name = "Panel_View";
            this.Panel_View.Size = new System.Drawing.Size(1200, 800);
            this.Panel_View.TabIndex = 0;
            this.Panel_View.Paint += new System.Windows.Forms.PaintEventHandler(this.Panel_View_Paint);
            // 
            // Label_HelpMessage
            // 
            this.Label_HelpMessage.AutoSize = true;
            this.Label_HelpMessage.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_HelpMessage.ForeColor = System.Drawing.Color.White;
            this.Label_HelpMessage.Location = new System.Drawing.Point(0, 30);
            this.Label_HelpMessage.Name = "Label_HelpMessage";
            this.Label_HelpMessage.Size = new System.Drawing.Size(256, 136);
            this.Label_HelpMessage.TabIndex = 0;
            this.Label_HelpMessage.Text = resources.GetString("Label_HelpMessage.Text");
            // 
            // Label_PressedKey
            // 
            this.Label_PressedKey.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_PressedKey.ForeColor = System.Drawing.Color.White;
            this.Label_PressedKey.Location = new System.Drawing.Point(0, 0);
            this.Label_PressedKey.Name = "Label_PressedKey";
            this.Label_PressedKey.Size = new System.Drawing.Size(100, 30);
            this.Label_PressedKey.TabIndex = 0;
            this.Label_PressedKey.Text = "Δx, Δy, Δz";
            this.Label_PressedKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.Panel_View.ResumeLayout(false);
            this.Panel_View.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel Panel_Main;
        private System.Windows.Forms.Panel Panel_View;
        private System.Windows.Forms.Panel Panel_SideBar;
        private System.Windows.Forms.Label Label_PressedKey;
        private System.Windows.Forms.Label Label_HelpMessage;
    }
}