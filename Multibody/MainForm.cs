/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2020 chibayuki@foxmail.com

多体系统模拟 (MultibodySystemSimulation)
Version 1.0.0.0.DEV.200702-0000

This file is part of "多体系统模拟" (MultibodySystemSimulation)

"多体系统模拟" (MultibodySystemSimulation) is released under the GPLv3 license
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ColorManipulation = Com.ColorManipulation;
using ColorX = Com.ColorX;
using Geometry = Com.Geometry;
using PointD = Com.PointD;
using PointD3D = Com.PointD3D;
using Statistics = Com.Statistics;
using FormManager = Com.WinForm.FormManager;
using Theme = Com.WinForm.Theme;
using UIMessage = Com.WinForm.UIMessage;
using UIMessageProcessorState = Com.WinForm.UIMessageProcessorState;

namespace Multibody
{
    public partial class MainForm : Form
    {
        #region 窗口定义

        private FormManager Me;

        public FormManager FormManager
        {
            get
            {
                return Me;
            }
        }

        private void _Ctor(FormManager owner)
        {
            InitializeComponent();

            //

            if (owner != null)
            {
                Me = new FormManager(this, owner);
            }
            else
            {
                Me = new FormManager(this);
            }

            //

            FormDefine();
        }

        public MainForm()
        {
            _Ctor(null);
        }

        public MainForm(FormManager owner)
        {
            _Ctor(owner);
        }

        private void FormDefine()
        {
            Me.Caption = Application.ProductName;
            Me.ShowCaptionBarColor = false;
            Me.EnableCaptionBarTransparent = false;
            Me.Theme = Theme.Black;
            Me.ThemeColor = ColorManipulation.GetRandomColorX();

            Me.Loading += Me_Loading;
            Me.Loaded += Me_Loaded;
            Me.Closed += Me_Closed;
            Me.Resize += Me_Resize;
            Me.SizeChanged += Me_SizeChanged;
            Me.ThemeChanged += Me_ThemeChanged;
            Me.ThemeColorChanged += Me_ThemeChanged;
        }

        #endregion

        #region 窗口事件回调

        private void Me_Loading(object sender, EventArgs e)
        {
            int h = Statistics.RandomInteger(360);
            const int s = 100;
            const int v = 70;
            const int d = 37;
            int i = 0;

            _Simulation = new Simulation(Panel_View, _RedrawMethod, _ViewCenter(), _ViewSize());

            _Particles = new List<Particle>();
            _Particles.Add(new Particle(1E8, 5, new PointD3D(0, 0, 1000), new PointD3D(0, 0, 0), ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(1E3, 2, new PointD3D(0, -200, 1400), new PointD3D(0.001, 0.001, 0), ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(1E1, 2, new PointD3D(-200, 0, 2000), new PointD3D(0.0007, -0.0007, 0), ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
        }

        private void Me_Loaded(object sender, EventArgs e)
        {
            Me.OnThemeChanged();
            Me.OnSizeChanged();

            //

            Label_OffsetX.MouseEnter += Label_ViewOperation_MouseEnter;
            Label_OffsetY.MouseEnter += Label_ViewOperation_MouseEnter;
            Label_OffsetZ.MouseEnter += Label_ViewOperation_MouseEnter;
            Label_RotateX.MouseEnter += Label_ViewOperation_MouseEnter;
            Label_RotateY.MouseEnter += Label_ViewOperation_MouseEnter;
            Label_RotateZ.MouseEnter += Label_ViewOperation_MouseEnter;

            Label_OffsetX.MouseLeave += Label_ViewOperation_MouseLeave;
            Label_OffsetY.MouseLeave += Label_ViewOperation_MouseLeave;
            Label_OffsetZ.MouseLeave += Label_ViewOperation_MouseLeave;
            Label_RotateX.MouseLeave += Label_ViewOperation_MouseLeave;
            Label_RotateY.MouseLeave += Label_ViewOperation_MouseLeave;
            Label_RotateZ.MouseLeave += Label_ViewOperation_MouseLeave;

            Label_OffsetX.MouseDown += Label_ViewOperation_MouseDown;
            Label_OffsetY.MouseDown += Label_ViewOperation_MouseDown;
            Label_OffsetZ.MouseDown += Label_ViewOperation_MouseDown;
            Label_RotateX.MouseDown += Label_ViewOperation_MouseDown;
            Label_RotateY.MouseDown += Label_ViewOperation_MouseDown;
            Label_RotateZ.MouseDown += Label_ViewOperation_MouseDown;

            Label_OffsetX.MouseUp += Label_ViewOperation_MouseUp;
            Label_OffsetY.MouseUp += Label_ViewOperation_MouseUp;
            Label_OffsetZ.MouseUp += Label_ViewOperation_MouseUp;
            Label_RotateX.MouseUp += Label_ViewOperation_MouseUp;
            Label_RotateY.MouseUp += Label_ViewOperation_MouseUp;
            Label_RotateZ.MouseUp += Label_ViewOperation_MouseUp;

            Label_OffsetX.MouseMove += Label_OffsetX_MouseMove;
            Label_OffsetY.MouseMove += Label_OffsetY_MouseMove;
            Label_OffsetZ.MouseMove += Label_OffsetZ_MouseMove;
            Label_RotateX.MouseMove += Label_RotateX_MouseMove;
            Label_RotateY.MouseMove += Label_RotateY_MouseMove;
            Label_RotateZ.MouseMove += Label_RotateZ_MouseMove;

            Panel_View.MouseDown += Panel_View_MouseDown;
            Panel_View.MouseUp += Panel_View_MouseUp;
            Panel_View.MouseMove += Panel_View_MouseMove;
            Panel_View.MouseWheel += Panel_View_MouseWheel;

            //

            _Simulation.Start();

            foreach (Particle particle in _Particles)
            {
                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.AddParticle) { RequestData = particle });
            }

            _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.SimulationStart));
        }

        private void Me_Closed(object sender, EventArgs e)
        {
            _Simulation.Stop();
        }

        private void Me_Resize(object sender, EventArgs e)
        {
            Panel_SideBar.Height = Panel_Main.Height;
            Panel_SideBar.Left = Panel_Main.Width - Panel_SideBar.Width;

            Panel_View.Size = Panel_Main.Size;

            //

            if (_Simulation.State == UIMessageProcessorState.Running)
            {
                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.UpdateCoordinateOffset) { RequestData = _ViewCenter() });
                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.UpdateBitmapSize) { RequestData = _ViewSize() });
            }
        }

        private void Me_SizeChanged(object sender, EventArgs e)
        {
            Me.OnResize();
        }

        private void Me_ThemeChanged(object sender, EventArgs e)
        {
            this.BackColor = Me.RecommendColors.FormBackground.ToColor();

            Panel_View.BackColor = Me.RecommendColors.FormBackground.ToColor();

            Panel_SideBar.BackColor = Me.RecommendColors.Background_DEC.ToColor();

            Label_OffsetX.ForeColor = Label_OffsetY.ForeColor = Label_OffsetZ.ForeColor = Label_RotateX.ForeColor = Label_RotateY.ForeColor = Label_RotateZ.ForeColor = Me.RecommendColors.Text_INC.ToColor();
            Label_OffsetX.BackColor = Label_OffsetY.BackColor = Label_OffsetZ.BackColor = Label_RotateX.BackColor = Label_RotateY.BackColor = Label_RotateZ.BackColor = Me.RecommendColors.Background_INC.ToColor();
        }

        #endregion

        #region 粒子与多体系统

        private Simulation _Simulation; // 仿真对象。
        private List<Particle> _Particles; // 粒子列表。

        #endregion

        #region 视图控制

        // 视图中心。
        private Point _ViewCenter()
        {
            return new Point(Panel_View.Width / 2, Panel_View.Height / 2);
        }

        // 视图中心。
        private Size _ViewSize()
        {
            return new Size(Panel_View.Width, Me.CaptionBarHeight + Panel_View.Height);
        }

        //

        private const double _ShiftPerPixel = 1; // 每像素的偏移量（像素）。
        private const double _RadPerPixel = Math.PI / 180; // 每像素的旋转角度（弧度）。

        private Point _CursorLocation; // 鼠标指针位置。
        private bool _AdjustNow = false; // 是否正在调整。

        private void Label_ViewOperation_MouseEnter(object sender, EventArgs e)
        {
            ((Label)sender).BackColor = Me.RecommendColors.Button_DEC.ToColor();
        }

        private void Label_ViewOperation_MouseLeave(object sender, EventArgs e)
        {
            ((Label)sender).BackColor = Me.RecommendColors.Background_INC.ToColor();
        }

        private void Label_ViewOperation_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ((Label)sender).BackColor = Me.RecommendColors.Button_INC.ToColor();
                ((Label)sender).Cursor = Cursors.SizeWE;

                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.ViewOperationStart));

                _CursorLocation = e.Location;
                _AdjustNow = true;
            }
        }

        private void Label_ViewOperation_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _AdjustNow = false;

                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.ViewOperationStop));

                ((Label)sender).BackColor = (Geometry.CursorIsInControl((Label)sender) ? Me.RecommendColors.Button_DEC.ToColor() : Me.RecommendColors.Background_INC.ToColor());
                ((Label)sender).Cursor = Cursors.Default;
            }
        }

        private void Label_OffsetX_MouseMove(object sender, MouseEventArgs e)
        {
            if (_AdjustNow)
            {
                double off = (e.X - _CursorLocation.X) * _ShiftPerPixel * _Simulation.SpaceMag;

                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.ViewOperationUpdateParam) { RequestData = new (Simulation.ViewOperationType, double)[] { (Simulation.ViewOperationType.OffsetX, off) } });
            }
        }

        private void Label_OffsetY_MouseMove(object sender, MouseEventArgs e)
        {
            if (_AdjustNow)
            {
                double off = (e.X - _CursorLocation.X) * _ShiftPerPixel * _Simulation.SpaceMag;

                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.ViewOperationUpdateParam) { RequestData = new (Simulation.ViewOperationType, double)[] { (Simulation.ViewOperationType.OffsetY, off) } });
            }
        }

        private void Label_OffsetZ_MouseMove(object sender, MouseEventArgs e)
        {
            if (_AdjustNow)
            {
                double off = (e.X - _CursorLocation.X) * _ShiftPerPixel * _Simulation.SpaceMag;

                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.ViewOperationUpdateParam) { RequestData = new (Simulation.ViewOperationType, double)[] { (Simulation.ViewOperationType.OffsetZ, off) } });
            }
        }

        private void Label_RotateX_MouseMove(object sender, MouseEventArgs e)
        {
            if (_AdjustNow)
            {
                double rot = (e.X - _CursorLocation.X) * _RadPerPixel;

                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.ViewOperationUpdateParam) { RequestData = new (Simulation.ViewOperationType, double)[] { (Simulation.ViewOperationType.RotateX, rot) } });
            }
        }

        private void Label_RotateY_MouseMove(object sender, MouseEventArgs e)
        {
            if (_AdjustNow)
            {
                double rot = (e.X - _CursorLocation.X) * _RadPerPixel;

                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.ViewOperationUpdateParam) { RequestData = new (Simulation.ViewOperationType, double)[] { (Simulation.ViewOperationType.RotateY, rot) } });
            }
        }

        private void Label_RotateZ_MouseMove(object sender, MouseEventArgs e)
        {
            if (_AdjustNow)
            {
                double rot = (e.X - _CursorLocation.X) * _RadPerPixel;

                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.ViewOperationUpdateParam) { RequestData = new (Simulation.ViewOperationType, double)[] { (Simulation.ViewOperationType.RotateZ, rot) } });
            }
        }

        private void Panel_View_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.ViewOperationStart));

                _CursorLocation = e.Location;
                _AdjustNow = true;
            }
        }

        private void Panel_View_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _AdjustNow = false;

                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.ViewOperationStop));
            }
        }

        private void Panel_View_MouseMove(object sender, MouseEventArgs e)
        {
            if (_AdjustNow)
            {
                PointD off = new PointD((e.X - _CursorLocation.X) * _Simulation.SpaceMag, (e.Y - _CursorLocation.Y) * _Simulation.SpaceMag);

                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.ViewOperationUpdateParam) { RequestData = new (Simulation.ViewOperationType, double)[] { (Simulation.ViewOperationType.OffsetX, off.X), (Simulation.ViewOperationType.OffsetY, off.Y) } });
            }
        }

        private void Panel_View_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!_AdjustNow)
            {
                double off = _Simulation.SpaceMag;

                if (e.Delta > 0)
                {
                    off = -off;
                }

                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.ViewOperationStart));
                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.ViewOperationUpdateParam) { RequestData = new (Simulation.ViewOperationType, double)[] { (Simulation.ViewOperationType.OffsetZ, off) } });
                _Simulation.PushMessage(new UIMessage((int)Simulation.MessageCode.ViewOperationStop));
            }
        }

        #endregion

        #region 重绘

        private Bitmap _MultibodyBitmap = null; // 多体系统的位图。

        // 重绘方法。
        private void _RedrawMethod(Bitmap bitmap)
        {
            if (_MultibodyBitmap != null)
            {
                _MultibodyBitmap.Dispose();
            }

            _MultibodyBitmap = bitmap;

            if (_MultibodyBitmap != null)
            {
                Me.CaptionBarBackgroundImage = _MultibodyBitmap;

                Panel_View.CreateGraphics().DrawImage(_MultibodyBitmap, new Point(0, -Me.CaptionBarHeight));
            }
        }

        private void Panel_View_Paint(object sender, PaintEventArgs e)
        {
            if (_MultibodyBitmap != null)
            {
                e.Graphics.DrawImage(_MultibodyBitmap, new Point(0, -Me.CaptionBarHeight));
            }
        }

        #endregion
    }
}