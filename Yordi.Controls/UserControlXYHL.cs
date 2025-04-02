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
        protected Point mouseDownLocation;

        private bool isMovable;
        private bool isResizable;
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Moving { get => isMovable; set { isMovable = value; MovingChanged?.Invoke(value); } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Resizing { get => isResizable; set { isResizable = value; ResizingChanged?.Invoke(value); } }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public EdgeEnum Edge { get; set; } = EdgeEnum.None;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IContainer? Components { get => components; set => components = value; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsRuntime { get => (HabilitaDimensionar && Resizing) || (HabilitaArrastar && Moving); }

        /// <summary>
        /// Evento disparado quando a posição ou tamanho do controle é alterado
        /// </summary>
        public event XYHLDelegate? XYHLChanged;

        /// <summary>
        /// Evento disparado quando o controle está sendo movido
        /// </summary>
        public event BoolChanged? MovingChanged;
        /// <summary>
        /// Evento disparado quando o controle está sendo redimensionado
        /// </summary>
        public event BoolChanged? ResizingChanged;

        /// <summary>
        /// Evento disparado para enviar mensagens
        /// </summary>
        public event MyMessage? MessageEvent;


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
                this.ContextMenuVerifyEx();
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
                this.ContextMenuVerifyEx();
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public bool HasClickSubscribers => _subsClick > 0 || _subsMouseClick > 0;

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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public Rectangle AreaPath { get => areaPath; set => areaPath = value; }

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
                Edge = EdgeEnum.None;
                return;
            }
            if (Moving)
            {
                Cursor = Cursors.SizeAll;
                Edge = EdgeEnum.TopLeft;
                return;
            }
            if (Resizing)
            {
                //top left corner
                if (e.X <= (Padding.Horizontal) & e.Y <= (Padding.Vertical))
                {
                    Cursor = Cursors.SizeAll;
                    Edge = EdgeEnum.TopLeft;
                }
                //bottom right corner
                else if ((e.X >= (Width - (Padding.Horizontal + 1))) & (e.Y >= Height - (Padding.Vertical + 1)))
                {
                    Cursor = Cursors.SizeNWSE;
                    Edge = EdgeEnum.BottomRight;
                }
                //left edge
                else if (e.X <= Padding.Horizontal)
                {
                    Cursor = Cursors.VSplit;
                    Edge = EdgeEnum.Left;
                }
                //right edge
                else if (e.X > (Width - (Padding.Horizontal + 1)))
                {
                    Cursor = Cursors.VSplit;
                    Edge = EdgeEnum.Right;
                }
                //top edge
                else if (e.Y <= Padding.Vertical)
                {
                    Cursor = Cursors.HSplit;
                    Edge = EdgeEnum.Top;
                }
                //bottom edge
                else if (e.Y > Height - (Padding.Vertical + 1))
                {
                    Cursor = Cursors.HSplit;
                    Edge = EdgeEnum.Bottom;
                }
                //no edge
                else
                {
                    Cursor = Cursors.Default;
                    Edge = EdgeEnum.None;
                }
            }
        }
        protected virtual void RedimensionarControle(MouseEventArgs e)
        {
            SuspendLayout();
            if (Moving)
            {
                this.Left += e.X - mouseDownLocation.X;
                this.Top += e.Y - mouseDownLocation.Y;
            }
            else
            {
                switch (Edge)
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
        private int _subsClick = 0;
        private int _subsMouseClick = 0;

        public new event EventHandler? Click
        {
            add
            {
                _subsClick++;
                base.Click += value;
            }
            remove
            {
                _subsClick--;
                base.Click -= value;
            }
        }

        public new event MouseEventHandler? MouseClick
        {
            add
            {
                _subsMouseClick++;
                base.MouseClick += value;
            }
            remove
            {
                _subsMouseClick--;
                base.MouseClick -= value;
            }
        }

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
            this.BordaTracejada(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!IsRuntime)
            {
                base.OnMouseDown(e);
                this.OnMouseDownEx(e);
            }
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
            else if (this.Capture && Edge != EdgeEnum.None)
                RedimensionarControle(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!IsRuntime)
            {
                base.OnMouseUp(e);
                this.OnMouseUpEx(e);
            }
            else if (e.Button == MouseButtons.Left)
            {
                this.Capture = false;
            }
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            if (!IsRuntime)
                base.OnMouseLeave(e);
            else
                Edge = EdgeEnum.None;
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
            this.ContextMenuVerifyEx();
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            this.ContextMenuVerifyEx();
            var xyhl = this.FindTrulyXYHL();
            if (xyhl == null)
            {
                _initialLocation = Location;
                _initialSize = Size;
            }
            else
            {
                this.SetLocation();
                _initialLocation = Location;
                _initialSize = Size;
            }
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
                if (Moving)
                    this.Left -= moveStep;
                else if (Resizing)
                    this.Width -= moveStep;
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (Moving)
                    this.Left += moveStep;
                else if (Resizing)
                    this.Width += moveStep;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (Moving)
                    this.Top -= moveStep;
                else if (Resizing)
                    this.Height -= moveStep;
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (Moving)
                    this.Top += moveStep;
                else if (Resizing)
                    this.Height += moveStep;
            }

            // Redesenha o controle após a movimentação ou redimensionamento
            if (Moving || Resizing)
                Invalidate();
        }

        private void SetOriginalXYHL()
        {
            Resizing = false;
            if (meDimensionar != null && !meDimensionar.IsDisposed)
                meDimensionar.Checked = Resizing;
            Moving = false;
            if (meMove != null && !meMove.IsDisposed)
                meMove.Checked = Moving;
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

        #endregion

        #region Menu de contexto
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public System.Windows.Forms.ToolStripMenuItem? meDimensionar { get; set; }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public System.Windows.Forms.ToolStripMenuItem? meMove { get; set; }


        public void MenuMoveClick(object? sender, EventArgs e)
        {
            this.MenuMoveClick();
        }
        public void MenuDimensionarClick(object? sender, EventArgs e)
        {
            this.MenuDimensionarClick();
        }

        #endregion

        #region Localização
        public virtual void SetXYHL()
        {
            if (!Visible) return;
            this.SetLocation();
        }
        public void SaveMeXYHL()
        {
            var p = this.SaveXYHL();
            if (p != null)
            {
                _initialSize = p.Size;
                _initialLocation = p.Location;
            }
        }
        public void XYHLChangedInvoke(XYHL p)
        {
            XYHLChanged?.Invoke(p);
        }

        #endregion
    }
}
