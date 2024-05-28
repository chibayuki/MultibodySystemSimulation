/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2024 chibayuki@foxmail.com

多体系统模拟 (MultibodySystemSimulation)
Version 1.0.117.1000.M2.201101-1440

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
using PointD = Com.PointD;
using PointD3D = Com.PointD3D;
using Statistics = Com.Statistics;
using FormManager = Com.WinForm.FormManager;
using FormState = Com.WinForm.FormState;
using Theme = Com.WinForm.Theme;

namespace Multibody
{
    public partial class MainForm : Form
    {
        #region 窗口定义

        public FormManager FormManager { get; private set; }

        public MainForm() : this(null)
        {
        }

        public MainForm(FormManager owner)
        {
            InitializeComponent();

            //

            FormManager = new FormManager(this, owner)
            {
                Caption = Application.ProductName,
                ShowCaptionBarColor = false,
                EnableCaptionBarTransparent = false,
                Theme = Theme.Black,
                ThemeColor = ColorManipulation.GetRandomColorX()
            };

            FormManager.Loading += Form_Loading;
            FormManager.Loaded += Form_Loaded;
            FormManager.Closed += Form_Closed;
            FormManager.Resize += Form_Resize;
            FormManager.SizeChanged += Form_SizeChanged;
            FormManager.ThemeChanged += Form_ThemeChanged;
            FormManager.ThemeColorChanged += Form_ThemeChanged;
        }

        #endregion

        #region 窗口事件回调

        private void Form_Loading(object sender, EventArgs e)
        {
            int h = Statistics.RandomInteger(360);
            const int s = 100;
            const int v = 70;
            const int d = 37;

            _InteractiveManager = new InteractiveManager(Panel_View, _RedrawMethod, _ViewSize);

            int id = 0;
            _Particles = new List<Particle>()
            {
                new Particle(id++, 1E8, 5, ColorX.FromHSL((h + d * id) % 360, s, v).ToColor(), new PointD3D(0, 0, 1000), new PointD3D(0, 0, 0)),
                new Particle(id++, 1E3, 2, ColorX.FromHSL((h + d * id) % 360, s, v).ToColor(), new PointD3D(0, -200, 1400), new PointD3D(0.001, 0.001, 0)),
                new Particle(id++, 1E1, 2, ColorX.FromHSL((h + d * id) % 360, s, v).ToColor(), new PointD3D(-200, 0, 2000), new PointD3D(0.0007, -0.0007, 0))
            };
        }

        private void Form_Loaded(object sender, EventArgs e)
        {
            FormManager.OnThemeChanged();
            FormManager.OnSizeChanged();

            //

            Panel_View.MouseEnter += Panel_View_MouseEnter;
            Panel_View.LostFocus += Panel_View_LostFocus;
            Panel_View.KeyDown += Panel_View_KeyDown;
            Panel_View.KeyUp += Panel_View_KeyUp;
            Panel_View.MouseDown += Panel_View_MouseDown;
            Panel_View.MouseUp += Panel_View_MouseUp;
            Panel_View.MouseMove += Panel_View_MouseMove;
            Panel_View.MouseWheel += Panel_View_MouseWheel;

            //

            foreach (Particle particle in _Particles)
            {
                _InteractiveManager.AddParticle(particle);
            }

            _InteractiveManager.SimulationStart();
        }

        private void Form_Closed(object sender, EventArgs e)
        {
            _InteractiveManager.SimulationStop();
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            _InteractiveManager.UpdateViewSize(_ViewSize);
        }

        private void Form_SizeChanged(object sender, EventArgs e)
        {
            FormManager.OnResize();
        }

        private void Form_ThemeChanged(object sender, EventArgs e)
        {
            this.BackColor = FormManager.RecommendColors.FormBackground.ToColor();

            Panel_View.BackColor = FormManager.RecommendColors.FormBackground.ToColor();

            Panel_SideBar.BackColor = FormManager.RecommendColors.Background_DEC.ToColor();
        }

        #endregion

        #region 粒子与多体系统

        private InteractiveManager _InteractiveManager; // 交互管理器。

        private List<Particle> _Particles; // 粒子列表。

        #endregion

        #region 视图控制

        // 视图大小。
        private Size _ViewSize => FormManager.Size;

        //

        private const double _ShiftPerPixel = 1; // 每像素的偏移量（像素）。
        private const double _RadPerPixel = Math.PI / 180; // 每像素的旋转角度（弧度）。

        private Point _MouseDownLocation; // 鼠标按下时的指针位置。
        private bool _AdjustingViewNow = false; // 是否正在调整视图。

        private HashSet<Keys> _PressedKeys = new HashSet<Keys>(); // 键盘正在按下的按键。

        private void Panel_View_MouseEnter(object sender, EventArgs e)
        {
            if (FormManager.IsActive)
            {
                Panel_View.Focus();
            }
        }

        private void Panel_View_LostFocus(object sender, EventArgs e)
        {
            _PressedKeys.Clear();
            _InteractiveManager.PressedKeysChanged(_PressedKeys);
        }

        private void Panel_View_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
            {
                _PressedKeys.Add(e.KeyCode);
                _InteractiveManager.PressedKeysChanged(_PressedKeys);
            }
        }

        private void Panel_View_KeyUp(object sender, KeyEventArgs e)
        {
            _PressedKeys.Remove(e.KeyCode);
            _InteractiveManager.PressedKeysChanged(_PressedKeys);
        }

        private void Panel_View_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _InteractiveManager.ViewOperationStart();
                _MouseDownLocation = e.Location;
                _AdjustingViewNow = true;
            }
        }

        private void Panel_View_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _AdjustingViewNow = false;
                _InteractiveManager.ViewOperationStop();
            }
        }

        private void Panel_View_MouseMove(object sender, MouseEventArgs e)
        {
            if (_AdjustingViewNow)
            {
                if (_PressedKeys.Count == 0)
                {
                    PointD off = (new PointD(e.X, e.Y) - _MouseDownLocation) * _InteractiveManager.SpaceMag * _ShiftPerPixel;
                    _InteractiveManager.ViewOperationOffsetXY(off);
                }
                else if (_PressedKeys.Count == 1)
                {
                    if (_PressedKeys.Contains(Keys.X))
                    {
                        double off = (e.X - _MouseDownLocation.X) * _InteractiveManager.SpaceMag * _ShiftPerPixel;
                        _InteractiveManager.ViewOperationOffsetX(off);
                    }
                    else if (_PressedKeys.Contains(Keys.Y))
                    {
                        double off = (e.Y - _MouseDownLocation.Y) * _InteractiveManager.SpaceMag * _ShiftPerPixel;
                        _InteractiveManager.ViewOperationOffsetY(off);
                    }
                    else if (_PressedKeys.Contains(Keys.Z))
                    {
                        double off = -(e.Y - _MouseDownLocation.Y) * _InteractiveManager.SpaceMag * _ShiftPerPixel;
                        _InteractiveManager.ViewOperationOffsetZ(off);
                    }
                }
                else if (_PressedKeys.Count == 2)
                {
                    if (_PressedKeys.Contains(Keys.X) && _PressedKeys.Contains(Keys.Y))
                    {
                        PointD off = (new PointD(e.X, e.Y) - _MouseDownLocation) * _InteractiveManager.SpaceMag * _ShiftPerPixel;
                        _InteractiveManager.ViewOperationOffsetXY(off);
                    }
                    else if (_PressedKeys.Contains(Keys.R))
                    {
                        if (_PressedKeys.Contains(Keys.X))
                        {
                            double rot = -(e.Y - _MouseDownLocation.Y) * _RadPerPixel;
                            _InteractiveManager.ViewOperationRotateX(rot);
                        }
                        else if (_PressedKeys.Contains(Keys.Y))
                        {
                            double rot = (e.X - _MouseDownLocation.X) * _RadPerPixel;
                            _InteractiveManager.ViewOperationRotateY(rot);
                        }
                        else if (_PressedKeys.Contains(Keys.Z))
                        {
                            PointD viewCenter = new PointD(_ViewSize) / 2;
                            double rot = ((e.X, e.Y) - viewCenter).Azimuth - (_MouseDownLocation - viewCenter).Azimuth;
                            _InteractiveManager.ViewOperationRotateZ(rot);
                        }
                    }
                }
            }
        }

        private void Panel_View_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!_AdjustingViewNow)
            {
                if (_PressedKeys.Count == 0)
                {
                    double off = _InteractiveManager.SpaceMag * _ShiftPerPixel;
                    if (e.Delta > 0)
                    {
                        off = -off;
                    }

                    _InteractiveManager.ViewOperationStart();
                    _InteractiveManager.ViewOperationOffsetZ(off);
                    _InteractiveManager.ViewOperationStop();
                }
                else if (_PressedKeys.Count == 1)
                {
                    double off = _InteractiveManager.SpaceMag * _ShiftPerPixel;

                    if (_PressedKeys.Contains(Keys.X))
                    {
                        if (e.Delta < 0)
                        {
                            off = -off;
                        }

                        _InteractiveManager.ViewOperationStart();
                        _InteractiveManager.ViewOperationOffsetX(off);
                        _InteractiveManager.ViewOperationStop();
                    }
                    else if (_PressedKeys.Contains(Keys.Y))
                    {
                        if (e.Delta < 0)
                        {
                            off = -off;
                        }

                        _InteractiveManager.ViewOperationStart();
                        _InteractiveManager.ViewOperationOffsetY(off);
                        _InteractiveManager.ViewOperationStop();
                    }
                    else if (_PressedKeys.Contains(Keys.Z))
                    {
                        if (e.Delta > 0)
                        {
                            off = -off;
                        }

                        _InteractiveManager.ViewOperationStart();
                        _InteractiveManager.ViewOperationOffsetZ(off);
                        _InteractiveManager.ViewOperationStop();
                    }
                }
                else if (_PressedKeys.Count == 2)
                {
                    if (_PressedKeys.Contains(Keys.R))
                    {
                        double rot = _RadPerPixel;

                        if (_PressedKeys.Contains(Keys.X))
                        {
                            if (e.Delta > 0)
                            {
                                rot = -rot;
                            }

                            _InteractiveManager.ViewOperationStart();
                            _InteractiveManager.ViewOperationRotateX(rot);
                            _InteractiveManager.ViewOperationStop();
                        }
                        else if (_PressedKeys.Contains(Keys.Y))
                        {
                            if (e.Delta < 0)
                            {
                                rot = -rot;
                            }

                            _InteractiveManager.ViewOperationStart();
                            _InteractiveManager.ViewOperationRotateY(rot);
                            _InteractiveManager.ViewOperationStop();
                        }
                        else if (_PressedKeys.Contains(Keys.Z))
                        {
                            if (e.Delta > 0)
                            {
                                rot = -rot;
                            }

                            _InteractiveManager.ViewOperationStart();
                            _InteractiveManager.ViewOperationRotateZ(rot);
                            _InteractiveManager.ViewOperationStop();
                        }
                    }
                }
            }
        }

        #endregion

        #region 重绘

        private Bitmap _MultibodyBitmap = null; // 多体系统的位图。

        // 重绘方法。
        private void _RedrawMethod(Bitmap bitmap)
        {
            _MultibodyBitmap?.Dispose();
            _MultibodyBitmap = bitmap;

            if (_MultibodyBitmap != null)
            {
                if (FormManager.FormState == FormState.FullScreen)
                {
                    FormManager.CaptionBarBackgroundImage = null;
                    Panel_View.CreateGraphics().DrawImage(_MultibodyBitmap, new Point(0, 0));
                }
                else
                {
                    FormManager.CaptionBarBackgroundImage = _MultibodyBitmap;
                    Panel_View.CreateGraphics().DrawImage(_MultibodyBitmap, new Point(0, -FormManager.CaptionBarHeight));
                }
            }
        }

        private void Panel_View_Paint(object sender, PaintEventArgs e)
        {
            if (_MultibodyBitmap != null)
            {
                if (FormManager.FormState == FormState.FullScreen)
                {
                    e.Graphics.DrawImage(_MultibodyBitmap, new Point(0, 0));
                }
                else
                {
                    e.Graphics.DrawImage(_MultibodyBitmap, new Point(0, -FormManager.CaptionBarHeight));
                }
            }
        }

        #endregion
    }
}