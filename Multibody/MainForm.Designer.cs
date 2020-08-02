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
            this.Label_RotateZ = new System.Windows.Forms.Label();
            this.Label_RotateY = new System.Windows.Forms.Label();
            this.Label_RotateX = new System.Windows.Forms.Label();
            this.Label_OffsetZ = new System.Windows.Forms.Label();
            this.Label_OffsetY = new System.Windows.Forms.Label();
            this.Label_OffsetX = new System.Windows.Forms.Label();
            this.Panel_View = new System.Windows.Forms.Panel();
            this.Panel_Main.SuspendLayout();
            this.Panel_SideBar.SuspendLayout();
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
            this.Panel_SideBar.Controls.Add(this.Label_RotateZ);
            this.Panel_SideBar.Controls.Add(this.Label_RotateY);
            this.Panel_SideBar.Controls.Add(this.Label_RotateX);
            this.Panel_SideBar.Controls.Add(this.Label_OffsetZ);
            this.Panel_SideBar.Controls.Add(this.Label_OffsetY);
            this.Panel_SideBar.Controls.Add(this.Label_OffsetX);
            this.Panel_SideBar.Location = new System.Drawing.Point(900, 0);
            this.Panel_SideBar.Name = "Panel_SideBar";
            this.Panel_SideBar.Size = new System.Drawing.Size(300, 800);
            this.Panel_SideBar.TabIndex = 0;
            // 
            // Label_RotateZ
            // 
            this.Label_RotateZ.BackColor = System.Drawing.Color.Transparent;
            this.Label_RotateZ.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_RotateZ.ForeColor = System.Drawing.Color.White;
            this.Label_RotateZ.Location = new System.Drawing.Point(200, 80);
            this.Label_RotateZ.Name = "Label_RotateZ";
            this.Label_RotateZ.Size = new System.Drawing.Size(80, 30);
            this.Label_RotateZ.TabIndex = 0;
            this.Label_RotateZ.Text = "Rz";
            this.Label_RotateZ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Label_RotateY
            // 
            this.Label_RotateY.BackColor = System.Drawing.Color.Transparent;
            this.Label_RotateY.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_RotateY.ForeColor = System.Drawing.Color.White;
            this.Label_RotateY.Location = new System.Drawing.Point(110, 80);
            this.Label_RotateY.Name = "Label_RotateY";
            this.Label_RotateY.Size = new System.Drawing.Size(80, 30);
            this.Label_RotateY.TabIndex = 0;
            this.Label_RotateY.Text = "Ry";
            this.Label_RotateY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Label_RotateX
            // 
            this.Label_RotateX.BackColor = System.Drawing.Color.Transparent;
            this.Label_RotateX.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_RotateX.ForeColor = System.Drawing.Color.White;
            this.Label_RotateX.Location = new System.Drawing.Point(20, 80);
            this.Label_RotateX.Name = "Label_RotateX";
            this.Label_RotateX.Size = new System.Drawing.Size(80, 30);
            this.Label_RotateX.TabIndex = 0;
            this.Label_RotateX.Text = "Rx";
            this.Label_RotateX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Label_OffsetZ
            // 
            this.Label_OffsetZ.BackColor = System.Drawing.Color.Transparent;
            this.Label_OffsetZ.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_OffsetZ.ForeColor = System.Drawing.Color.White;
            this.Label_OffsetZ.Location = new System.Drawing.Point(200, 40);
            this.Label_OffsetZ.Name = "Label_OffsetZ";
            this.Label_OffsetZ.Size = new System.Drawing.Size(80, 30);
            this.Label_OffsetZ.TabIndex = 0;
            this.Label_OffsetZ.Text = "Δz";
            this.Label_OffsetZ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Label_OffsetY
            // 
            this.Label_OffsetY.BackColor = System.Drawing.Color.Transparent;
            this.Label_OffsetY.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_OffsetY.ForeColor = System.Drawing.Color.White;
            this.Label_OffsetY.Location = new System.Drawing.Point(110, 40);
            this.Label_OffsetY.Name = "Label_OffsetY";
            this.Label_OffsetY.Size = new System.Drawing.Size(80, 30);
            this.Label_OffsetY.TabIndex = 0;
            this.Label_OffsetY.Text = "Δy";
            this.Label_OffsetY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Label_OffsetX
            // 
            this.Label_OffsetX.BackColor = System.Drawing.Color.Transparent;
            this.Label_OffsetX.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_OffsetX.ForeColor = System.Drawing.Color.White;
            this.Label_OffsetX.Location = new System.Drawing.Point(20, 40);
            this.Label_OffsetX.Name = "Label_OffsetX";
            this.Label_OffsetX.Size = new System.Drawing.Size(80, 30);
            this.Label_OffsetX.TabIndex = 0;
            this.Label_OffsetX.Text = "Δx";
            this.Label_OffsetX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Panel_View
            // 
            this.Panel_View.BackColor = System.Drawing.Color.Transparent;
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
            this.Panel_SideBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel Panel_Main;
        private System.Windows.Forms.Panel Panel_View;
        private System.Windows.Forms.Panel Panel_SideBar;
        private System.Windows.Forms.Label Label_OffsetX;
        private System.Windows.Forms.Label Label_RotateZ;
        private System.Windows.Forms.Label Label_RotateY;
        private System.Windows.Forms.Label Label_RotateX;
        private System.Windows.Forms.Label Label_OffsetZ;
        private System.Windows.Forms.Label Label_OffsetY;
    }
}