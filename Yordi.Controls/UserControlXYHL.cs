using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Yordi.Tools;

namespace Yordi.Controls
{
    /// <summary>
    /// Controle de usuário com capacidade de movimentação e redimensionamento.
    /// </summary>
    public partial class UserControlXYHL : UserControl, IUserControlXYHL, IControlName
    {
        private bool _enableDrag;
        private bool _enableMove;
        protected bool redesignBorderPath = false;
        private int borderWidth = 2;
        private int opacity = 100;
        private Point _initialLocation;
        private Size _initialSize;
        private Pen borderPen = new Pen(ControlPaint.Light(Color.Transparent, 0.0f), 0);
        private SolidBrush _backgroundBrush = new SolidBrush(Color.Transparent);
        private Rectangle areaPath;


        private bool isMovable;
        private bool isResizable; 
        protected bool IsMovable { get => isMovable; set { isMovable = value; Moving?.Invoke(value); } }
        protected bool IsResizable { get => isResizable; set { isResizable = value; Resising?.Invoke(value); } }

        protected EdgeEnum mEdge = EdgeEnum.None;
        protected Point mouseDownLocation;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsRuntime { get => (HabilitaDimensionar && IsResizable) || (HabilitaArrastar && IsMovable); }

        public event XYHLDelegate? XYHLChanged;
        public event MyMessage? MessageEvent;
        public event BoolChanged? Resising;
        public event BoolChanged? Moving;


        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public byte Alpha
        {
            get => alpha;
            set
            {
                if (alpha == value) return;
                alpha = value;
                ImageAlpha();
            }
        }
        private byte alpha = 255;

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Define um filtro para o raio das bordas do controle.")]
        public RectangleEdgeFilter BorderEdges
        {
            get { return borderEdges; }
            set
            {
                borderEdges = value;
                Invalidate(); // Redesenha o controle quando a propriedade é alterada
            }
        }
        private RectangleEdgeFilter borderEdges = RectangleEdgeFilter.All;

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Define o raio das bordas do controle.")]
        public virtual int BorderRadius
        {
            get => borderRadius;
            set { borderRadius = value; RectanglePath(); Invalidate(); }
        }
        private int borderRadius = 0;

        private Color _backgroundColor = Color.Transparent;
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundBrush = new SolidBrush(_backgroundColor = value);
                Invalidate();
            }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Appearence")]
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                redesignBorderPath = true;
                Invalidate();
            }
        }
        private Color borderColor = Color.Transparent;

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Appearence")]
        public virtual int BorderWidth
        {
            get => borderWidth;
            set
            {
                borderWidth = value;
                redesignBorderPath = true;
                RectanglePath();
                Invalidate();
            }
        }


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [Browsable(false)]
        public virtual bool EmUso
        {
            get => Visible;
            set
            {
                if (Visible == value) return;
                VisibleEx = value;
            }
        }

        [Category("Customs")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual byte AlphaForDisabled { get; set; } = 50;

        [Category("Customs")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool HabilitaDimensionar
        {
            get => _enableDrag;
            set
            {
                _enableDrag = value;
                ContextMenuVerify();
            }
        }

        [Category("Customs")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool HabilitaArrastar
        {
            get => _enableMove;
            set
            {
                _enableMove = value;
                ContextMenuVerify();
            }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Category("Appearence")]
        public int Opacity
        {
            get
            {
                if (opacity > 100) { opacity = 100; }
                else if (opacity < 1) { opacity = 1; }
                return this.opacity;
            }
            set
            {
                opacity = value;
                if (Parent != null) Parent.Invalidate(Bounds, true);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [Browsable(false)]
        public bool VisibleEx
        {
            get => Visible;
            set
            {
                if (InvokeRequired)
                    Invoke(() => SetVisibleCore(value));
                else
                    SetVisibleCore(value);
            }
        }

        /// <summary>
        ///  Name of this control. The designer will set this to the same
        ///  as the programatic Id "(name)" of the control.  The name can be
        ///  used as a key into the ControlCollection.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        [AllowNull]
        public new string Name
        {
            get => base.Name;
            set
            {
                base.Name = value;
                SetXYHL();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public virtual string ControlName => Name;

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Category("Appearence")]
        public new Padding Padding
        {
            get
            {
                PaddingMinimo(base.Padding);
                return base.Padding;
            }
            set
            {
                PaddingMinimo(value);
                Invalidate();
            }

        }

        public UserControlXYHL()
        {
            //SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint, true);
        }

        #region Métodos auxiliares
        private void PaddingMinimo(Padding padding)
        {
            var top = padding.Top;
            var bottom = padding.Bottom;
            var left = padding.Left;
            var right = padding.Right;
            if (top < 3) top = 3;
            if (bottom < 3) bottom = 3;
            if (left < 3) left = 3;
            if (right < 3) right = 3;
            if (top != base.Padding.Top || bottom != base.Padding.Bottom || left != base.Padding.Left || right != base.Padding.Right)
            {
                if (base.InvokeRequired)
                    base.Invoke(new Action(() => base.Padding = new Padding(left, top, right, bottom)));
                else
                    base.Padding = new Padding(left, top, right, bottom);
            }
        }
        protected internal void Messages(string msg, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0)
        {
            if (!Disposing)
                MessageEvent?.Invoke(msg, $"{Name}.{origem}", line);
        }
        protected virtual void DefinirCursor(MouseEventArgs e)
        {
            if (!IsRuntime)
            {
                Cursor = Cursors.Default;
                mEdge = EdgeEnum.None;
                return;
            }
            if (IsMovable)
            {
                Cursor = Cursors.SizeAll;
                mEdge = EdgeEnum.TopLeft;
                return;
            }
            if (IsResizable)
            {
                //top left corner
                if (e.X <= (Padding.Horizontal) & e.Y <= (Padding.Vertical))
                {
                    Cursor = Cursors.SizeAll;
                    mEdge = EdgeEnum.TopLeft;
                }
                //bottom right corner
                else if ((e.X >= (Width - (Padding.Horizontal + 1))) & (e.Y >= Height - (Padding.Vertical + 1)))
                {
                    Cursor = Cursors.SizeNWSE;
                    mEdge = EdgeEnum.BottomRight;
                }
                //left edge
                else if (e.X <= Padding.Horizontal)
                {
                    Cursor = Cursors.VSplit;
                    mEdge = EdgeEnum.Left;
                }
                //right edge
                else if (e.X > (Width - (Padding.Horizontal + 1)))
                {
                    Cursor = Cursors.VSplit;
                    mEdge = EdgeEnum.Right;
                }
                //top edge
                else if (e.Y <= Padding.Vertical)
                {
                    Cursor = Cursors.HSplit;
                    mEdge = EdgeEnum.Top;
                }
                //bottom edge
                else if (e.Y > Height - (Padding.Vertical + 1))
                {
                    Cursor = Cursors.HSplit;
                    mEdge = EdgeEnum.Bottom;
                }
                //no edge
                else
                {
                    Cursor = Cursors.Default;
                    mEdge = EdgeEnum.None;
                }
            }
        }
        protected virtual void RedimensionarControle(MouseEventArgs e)
        {
            SuspendLayout();
            if (IsMovable)
            {
                this.Left += e.X - mouseDownLocation.X;
                this.Top += e.Y - mouseDownLocation.Y;
            }
            else
            {
                switch (mEdge)
                {
                    case EdgeEnum.TopLeft:
                        SetBounds(Left + e.X, Top + e.Y, Width, Height);
                        break;
                    case EdgeEnum.Left:
                        SetBounds(Left + e.X, Top, Width - e.X, Height);
                        break;
                    case EdgeEnum.Right:
                        SetBounds(Left, Top, Width - (Width - e.X), Height);
                        break;
                    case EdgeEnum.Top:
                        SetBounds(Left, Top + e.Y, Width, Height - e.Y);
                        break;
                    case EdgeEnum.Bottom:
                        SetBounds(Left, Top, Width, Height - (Height - e.Y));
                        break;
                    case EdgeEnum.BottomRight:
                        SetBounds(Left, Top, Width - (Width - e.X), Height - (Height - e.Y));
                        break;
                }
            }
            ResumeLayout();
        }

        private void BasePaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle bounds = new Rectangle(0, 0, Width, Height);

            Color frmColor = Parent?.BackColor ?? Color.White;

            int alpha = (opacity * 255) / 100;

            if (IsRuntime)
            {
                Color dragBckColor;

                if (this.BackColor != Color.Transparent)
                {
                    int Rb = BackColor.R * alpha / 255 + frmColor.R * (255 - alpha) / 255;
                    int Gb = BackColor.G * alpha / 255 + frmColor.G * (255 - alpha) / 255;
                    int Bb = BackColor.B * alpha / 255 + frmColor.B * (255 - alpha) / 255;
                    dragBckColor = Color.FromArgb(Rb, Gb, Bb);
                }
                else
                    dragBckColor = frmColor;

                alpha = 255;

                using (var pen = new Pen(CurrentTheme.DraggingBorderColor, 2.0F))
                {
                    pen.DashStyle = DashStyle.Dash;
                    g.DrawRectangle(pen, bounds);
                }
            }
        }
        private void RectanglePath()
        {
            if (borderRadius > 0 || borderWidth > 0)
            {
                int x = borderWidth, y = borderWidth, l = Width - (borderWidth * 2), h = Height - (borderWidth * 2);
                if (areaPath.X != x || areaPath.Y != y || areaPath.Width != l || areaPath.Height != h)
                    areaPath = new Rectangle(x, y, l, h);
            }
            else
                areaPath = ClientRectangle;
        }

        protected virtual void ImageAlpha()
        {
            Invalidate();
        }

        protected void DrawPath(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            base.OnPaint(e);
            RectanglePath();
            e.Graphics.DrawRoundedRectangle(borderPen, areaPath, borderRadius, borderEdges);
        }
        protected void FillPath(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            RectanglePath();
            e.Graphics.FillRoundedRectangle(_backgroundBrush, areaPath, borderRadius, borderEdges);
        }

        protected override void OnResize(EventArgs e)
        {
            redesignBorderPath = true;
            base.OnResize(e);
        }

        #endregion

        #region Eventos sobreescritos
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }

        protected Pen BorderPen { get => borderPen; set { borderPen = value; redesignBorderPath = true; } }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            //BasePaint(e);
            DrawPath(e);
            FillPath(e);
            if (IsRuntime)
            {
                using (Pen dashedPen = new Pen(CurrentTheme.DraggingBorderColor, 2.0F))
                {
                    dashedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawRectangle(dashedPen, 0, 0, Width - 1, Height - 1);
                }
            }

        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!IsRuntime)
                base.OnMouseDown(e);
            else
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.Capture = true;
                    // Captura a posição do mouse em coordenadas de tela
                    Point mousePosition = Control.MousePosition;
                    // Converte a posição do mouse para coordenadas do controle
                    Point localPosition = this.PointToClient(mousePosition);
                    mouseDownLocation = localPosition;// e.Location;
                }
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            DefinirCursor(e);
            if (!IsRuntime)
                base.OnMouseMove(e);
            else if (this.Capture && mEdge != EdgeEnum.None)
                RedimensionarControle(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!IsRuntime)
                base.OnMouseUp(e);
            else if (e.Button == MouseButtons.Left)
                this.Capture = false;
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            if (!IsRuntime)
                base.OnMouseLeave(e);
            else
                mEdge = EdgeEnum.None;
        }
        protected override void OnMouseHover(EventArgs e)
        {
            if (!IsRuntime)
                base.OnMouseHover(e);
        }
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
                SetXYHL();
            ContextMenuVerify();
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ContextMenuVerify();
            _initialLocation = Location;
            _initialSize = Size;
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!IsRuntime)
            {
                base.OnKeyDown(e);
                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                SetOriginalXYHL();
                return;
            }
            int moveStep = 10; // Constante de movimentação padrão

            if (e.Shift)
                moveStep = 1; // Constante de movimentação com Shift
            else if (e.Control)
                moveStep = 3; // Constante de movimentação com Control

            if (e.KeyCode == Keys.Left)
            {
                if (IsMovable)
                    this.Left -= moveStep;
                else if (IsResizable)
                    this.Width -= moveStep;
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (IsMovable)
                    this.Left += moveStep;
                else if (IsResizable)
                    this.Width += moveStep;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (IsMovable)
                    this.Top -= moveStep;
                else if (IsResizable)
                    this.Height -= moveStep;
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (IsMovable)
                    this.Top += moveStep;
                else if (IsResizable)
                    this.Height += moveStep;
            }

            // Redesenha o controle após a movimentação ou redimensionamento
            if (IsMovable || IsResizable)
                Invalidate();
        }

        private void SetOriginalXYHL()
        {
            IsResizable = false;
            if (meDimensionar != null && !meDimensionar.IsDisposed)
                meDimensionar.Checked = IsResizable;
            IsMovable = false;
            if (meMove != null && !meMove.IsDisposed)
                meMove.Checked = IsMovable;
            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    Size = _initialSize;
                    Location = _initialLocation;
                });
            }
            else
            {
                Size = _initialSize;
                Location = _initialLocation;
            }
            //toRefresh = true;
            SetXYHL();
        }
        protected override bool IsInputKey(Keys keyData)
        {
            // Reconhece as teclas de seta e suas combinações com Shift e Control como teclas de entrada
            if ((keyData & Keys.KeyCode) == Keys.Left ||
                (keyData & Keys.KeyCode) == Keys.Right ||
                (keyData & Keys.KeyCode) == Keys.Up ||
                (keyData & Keys.KeyCode) == Keys.Down)
            {
                return true;
            }
            return base.IsInputKey(keyData);
        }
        protected virtual void Img_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) return;
            if (this is IEmUso emUso && !emUso.EmUso) return;
            if (sender is IControlXYHL my && !my.IsRuntime)
            {
                if (my.Alpha != AlphaForDisabled)
                    my.BorderStyle = BorderStyle.None;
            }
            else if (sender is PictureBox pb)
                pb.BorderStyle = BorderStyle.None;
            else if (sender is Panel pnl)
                pnl.BorderStyle = BorderStyle.None;
        }
        protected virtual void Img_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) return;
            if (this is IEmUso emUso && !emUso.EmUso) return;
            if (sender is IControlXYHL my && !my.IsRuntime)
            {
                if (my.Alpha != AlphaForDisabled) my.BorderStyle = BorderStyle.Fixed3D;
            }
            else if (sender is PictureBox pb)
                pb.BorderStyle = BorderStyle.Fixed3D;
            else if (sender is Panel pnl)
                pnl.BorderStyle = BorderStyle.Fixed3D;
        }

        #endregion

        #region Menu de contexto
        private ToolStripMenuItem? meDimensionar;
        private ToolStripMenuItem? meMove;

        protected void ContextMenuVerify()
        {
            if ((!HabilitaArrastar && !HabilitaDimensionar) || !Visible)
                ContextMenuStrip = null;
            else if (ContextMenuStrip == null)
            {
                if (components == null)
                    components = new Container();
                ContextMenuStrip = new ContextMenuStrip(components)
                {
                    BackColor = CurrentTheme.BackColor,
                    Font = CurrentTheme.Font,
                    ForeColor = CurrentTheme.ForeColor,
                    Name = "menu",
                    ShowCheckMargin = true,
                    ShowImageMargin = false,
                    Size = new Size(258, 52),
                    Text = "Alterações",
                };
            }
            VerifyDimensionarMenu();
            VerifyMoveMenu();
        }
        private void VerifyDimensionarMenu()
        {
            if (ContextMenuStrip == null || !HabilitaDimensionar)
            {
                if (meDimensionar != null)
                {
                    if (false.Equals(ContextMenuStrip?.Items.IsReadOnly))
                    {
                        if (ContextMenuStrip.Items.Contains(meDimensionar))
                            ContextMenuStrip.Items.Remove(meDimensionar);
                    }
                    meDimensionar.Click -= new EventHandler(MenuDimensionarClick);
                    meDimensionar.Dispose();
                    meDimensionar = null;
                }
            }
            else
            {
                if (meDimensionar == null)
                {
                    meDimensionar = new ToolStripMenuItem();
                    meDimensionar.CheckOnClick = true;
                    meDimensionar.Name = "meDimensionar";
                    meDimensionar.Size = new Size(208, 22);
                    meDimensionar.Text = "Dimensionar";
                    meDimensionar.Font = CurrentTheme.Font;
                    meDimensionar.ForeColor = CurrentTheme.ForeColor;
                }
                meDimensionar.Click -= new EventHandler(MenuDimensionarClick);
                meDimensionar.Click += new EventHandler(MenuDimensionarClick);
                if (false.Equals(ContextMenuStrip?.Items.IsReadOnly))
                {
                    if (!ContextMenuStrip.Items.Contains(meDimensionar))
                        ContextMenuStrip.Items.Add(meDimensionar);
                }
            }
        }
        private void VerifyMoveMenu()
        {
            if (ContextMenuStrip == null || !HabilitaArrastar)
            {
                if (meMove != null)
                {
                    if (false.Equals(ContextMenuStrip?.Items.IsReadOnly))
                    {
                        if (ContextMenuStrip.Items.Contains(meMove))
                            ContextMenuStrip.Items.Remove(meMove);
                    }
                    meMove.Click -= new EventHandler(MenuMoveClick);
                    meMove.Dispose();
                    meMove = null;
                }
            }
            else
            {
                if (meMove == null)
                {
                    meMove = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = "meMove",
                        Size = new Size(208, 22),
                        Text = "Mover",
                        Font = CurrentTheme.Font,
                        ForeColor = CurrentTheme.ForeColor,
                    };
                }
                meMove.Click -= new EventHandler(MenuMoveClick);
                meMove.Click += new EventHandler(MenuMoveClick);
                if (false.Equals(ContextMenuStrip?.Items.IsReadOnly))
                {
                    if (!ContextMenuStrip.Items.Contains(meMove))
                        ContextMenuStrip.Items.Add(meMove);
                }
            }
        }

        private void MenuMoveClick(object? sender, EventArgs e)
        {
            IsMovable = !IsMovable;
            if (meMove != null && !meMove.IsDisposed)
                meMove.Checked = IsMovable;
            if (!IsMovable) // se terminou de ajustar
                SaveXYHL();
            IsResizable = false;
            if (meDimensionar != null && !meDimensionar.IsDisposed)
                meDimensionar.Checked = IsResizable;
            //toRefresh = true;
            Invalidate();
        }
        private void MenuDimensionarClick(object? sender, EventArgs e)
        {
            IsResizable = !IsResizable;
            if (meDimensionar != null && !meDimensionar.IsDisposed)
                meDimensionar.Checked = IsResizable;
            if (!IsResizable) // se terminou de ajustar
                SaveXYHL();
            IsMovable = false;
            if (meMove != null && !meMove.IsDisposed)
                meMove.Checked = IsMovable;
            //toRefresh = true;
            Invalidate();
        }

        #endregion

        #region Localização
        public virtual void SetXYHL()
        {
            if (!Visible) return;
            var position = ControlXYHL.FindXYHL(this);
            SetLocationTask(position);
            Invalidate();
        }
        public void SaveXYHL()
        {
            var p = ControlXYHL.SaveXYHL(this);
            if (p != null)
            {
                _initialSize = p.Size;
                _initialLocation = p.Location;
                mEdge = EdgeEnum.None;
                XYHLChanged?.Invoke(p);
            }
        }
        protected void SetLocationTask(XYHL p)
        {
            if (InvokeRequired)
                Invoke(() => SetLocation(p));
            else
                SetLocation(p);
        }
        private void SetLocation(XYHL p)
        {
            if (p.X < 0) p.X = 0;
            if (p.Y < 0) p.Y = 0;
            if (Parent != null)
            {
                if (this.Parent is not Form frm || frm.WindowState == FormWindowState.Maximized)
                {
                    if (Parent.Height < p.Y + p.H + Padding.Vertical)
                        p.Y = (int)((Parent.Height) * 0.88m);
                    if (Parent.Width < p.X + p.L + Padding.Horizontal)
                        p.X = (int)(Parent.Width * 0.88m);
                }
            }
            bool invalidate = false;
            if (HabilitaDimensionar)
            {
                if (Size.Height != p.H || Size.Width != p.L)
                {
                    Size = p.Size;
                    invalidate = true;
                }
            }
            if (Location.X != p.X || Location.Y != p.Y)
            {
                Location = p.Location;
                invalidate = true;
            }
            if (invalidate) Invalidate();
        }

        #endregion
    }
}
