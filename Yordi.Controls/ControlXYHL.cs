using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using Yordi.Tools;

namespace Yordi.Controls
{
    /// <summary>
    /// Controle que implementa a interface IControlXYHL
    /// </summary>
    public class ControlXYHL : Control, IControlXYHL
    {
        public event XYHLDelegate? XYHLChanged;
        public event MyMessage? MessageEvent;
        public event EventHandler? Load;

        protected Point mouseDownLocation;
        public IContainer? components { get; set; } = null;
        private bool _enableDrag;
        private bool _enableMove;
        private int opacity  = 100;
        private Rectangle areaPath;
        private Pen borderPen = new Pen(ControlPaint.Light(Color.Transparent, 0.0f), 0);
        private SolidBrush _backgroundBrush = new SolidBrush(Color.Transparent);

        #region Construtores
        public ControlXYHL()
        {
            InitializeComponent();
            VisibleChanged += Control_VisibleChanged;
        }

        private void InitializeComponent()
        {
            Moving = false;
            Resizing = false;
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.ContextMenuVerifyEx();
            SetXYHL();
        }

        #endregion

        #region Propriedades

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IContainer? Components { get => components; set => components = value; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ToolStripMenuItem? meDimensionar { get; set; }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ToolStripMenuItem? meMove { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public EdgeEnum Edge { get; set; } = EdgeEnum.None;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Moving {get; protected set;}

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Resizing { get; protected set; }

        [Category("Customs")]
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
        public virtual bool HabilitaArrastar
        {
            get => _enableMove;
            set
            {
                _enableMove = value;
                this.ContextMenuVerifyEx();
            }
        }
        [Category("Customs")]
        public virtual byte AlphaForDisabled { get; set; } = 50;
        
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

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Define o estilo das bordas do controle. Usa-se o sistema do Windows para desenhar esse estilo.")]
        public BorderStyle BorderStyle { get; set; }


        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Appearence")]
        public virtual int BorderWidth
        {
            get => borderWidth;
            set
            {
                borderWidth = value;
                RectanglePath();
                Invalidate();
            }
        }
        private int borderWidth = 0;

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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsRuntime { get => (HabilitaDimensionar && Resizing) || (HabilitaArrastar && Moving); }


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
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

        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public bool HasClickSubscribers => _subsClick > 0 || _subsMouseClick > 0;
        #endregion


        protected internal void Message(string msg, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0)
        {
            if (!Disposing)
                MessageEvent?.Invoke(msg, $"{Name}.{origem}", line);
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

        protected virtual void ImageAlpha()
        {
            Invalidate();
        }

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

        #region Eventos
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


        private void Control_VisibleChanged(object? sender, EventArgs e)
        {
            SetXYHL();
        }

        public void MenuMoveClick(object? sender, EventArgs e)
        {
            Moving = !Moving;
            if (meMove != null && !meMove.IsDisposed)
                meMove.Checked = Moving;
            if (!Moving) // se terminou de ajustar
            {
                SaveXYHL();
                TabStop = false;
            }
            else
            {
                TabStop = true;
                Focus();
            }
            Resizing = false;
            if (meDimensionar != null && !meDimensionar.IsDisposed)
                meDimensionar.Checked = Resizing;
            Invalidate();
        }
        public void MenuDimensionarClick(object? sender, EventArgs e)
        {
            Resizing = !Resizing;
            if (meDimensionar != null && !meDimensionar.IsDisposed)
                meDimensionar.Checked = Resizing;
            if (!Resizing) // se terminou de ajustar
            {
                SaveXYHL();
                TabStop = false;
            }
            else
            {
                TabStop = true;
                Focus();
            }
            Moving = false;
            if (meMove != null && !meMove.IsDisposed)
                meMove.Checked = Moving;
            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Load?.Invoke(this, EventArgs.Empty);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
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
            {
                base.OnMouseDown(e);
                this.OnMouseDownEx(e);
            }
            else
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.Capture = true;
                    mouseDownLocation = e.Location;
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
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!IsRuntime)
            {
                base.OnKeyDown(e);
                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                Resizing = false;
                if (meDimensionar != null && !meDimensionar.IsDisposed)
                    meDimensionar.Checked = Resizing;
                Moving = false;
                if (meMove != null && !meMove.IsDisposed)
                    meMove.Checked = Moving;
                SetXYHL();
                return;
            }

            int moveStep = 10; // Constante de movimentação padrão

            if (e.Shift)
            {
                moveStep = 1; // Constante de movimentação com Shift
            }
            else if (e.Control)
            {
                moveStep = 3; // Constante de movimentação com Control
            }
            if (e.KeyCode == Keys.Left)
            {
                if (Moving)
                {
                    this.Left -= moveStep;
                }
                else if (Resizing)
                {
                    this.Width -= moveStep;
                }
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (Moving)
                {
                    this.Left += moveStep;
                }
                else if (Resizing)
                {
                    this.Width += moveStep;
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (Moving)
                {
                    this.Top -= moveStep;
                }
                else if (Resizing)
                {
                    this.Height -= moveStep;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (Moving)
                {
                    this.Top += moveStep;
                }
                else if (Resizing)
                {
                    this.Height += moveStep;
                }
            }

            // Redesenha o controle após a movimentação ou redimensionamento
            if (Moving || Resizing)
            {
                Invalidate();
            }
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

        protected virtual void RedimensionarControle(MouseEventArgs e)
        {
            Control c = (Control)this;
            c.SuspendLayout();
            switch (Edge)
            {
                case EdgeEnum.TopLeft:
                    c.SetBounds(c.Left + e.X, c.Top + e.Y, c.Width, c.Height);
                    break;
                case EdgeEnum.Left:
                    c.SetBounds(c.Left + e.X, c.Top, c.Width - e.X, c.Height);
                    break;
                case EdgeEnum.Right:
                    c.SetBounds(c.Left, c.Top, c.Width - (c.Width - e.X), c.Height);
                    break;
                case EdgeEnum.Top:
                    c.SetBounds(c.Left, c.Top + e.Y, c.Width, c.Height - e.Y);
                    break;
                case EdgeEnum.Bottom:
                    c.SetBounds(c.Left, c.Top, c.Width, c.Height - (c.Height - e.Y));
                    break;
                case EdgeEnum.BottomRight:
                    c.SetBounds(c.Left, c.Top, c.Width - (c.Width - e.X), c.Height - (c.Height - e.Y));
                    break;
            }
            c.ResumeLayout();
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



        #region Localização
        public virtual void SetXYHL()
        {
            var position = FindXYHL();
            SetLocationTask(position);
            Invalidate();
        }
        public void SaveXYHL()
        {
            var p = SaveXYHL(this);
            if (p != null)
            {
                Edge = EdgeEnum.None;
                XYHLChanged?.Invoke(p);
            }
        }
        protected XYHL FindXYHL()
        {
            return FindXYHL(this);
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


        #region Métodos estáticos
        public static XYHL FindXYHL(Control c)
        {
            var repo = XYHLRepository.Instancia();
            if (repo == null) return XYHLNew(c);

            Control? pai = c.Parent ?? c.FindForm();
            string? parentName = ParentName(c);
            bool alteraPosicao = pai is not Form frm || frm.WindowState == FormWindowState.Maximized;
            XYHL? p = repo.XYHL(c.Name, parentName);
            if (p != null)
            {
                if (pai != null && alteraPosicao)
                {
                    if (p.L > pai.Width)
                        p.L = pai.Width;
                    if (p.H > pai.Height)
                        p.H = pai.Height;
                    if (pai.Width < (p.X + p.L))
                        p.X = pai.Width - p.L;
                    if (pai.Height < (p.Y + p.H))
                        p.Y = pai.Height - p.H;
                }
            }
            else
            {
                p = XYHLNew(c);
            }
            return p;
        }
        private static XYHL XYHLNew(Control c)
        {
            return new XYHL()
            {
                H = c.Size.Height,
                X = c.Location.X,
                L = c.Size.Width,
                Y = c.Location.Y,
                ParentControl = ParentName(c),
                Nome = c.Name
            };
        }
        public static XYHL? SaveXYHL(Control c)
        {
            var repo = XYHLRepository.Instancia;
            if (repo == null) return null;
            if (c.Height <= c.Padding.Vertical)
                c.Height = c.Padding.Vertical;
            if (c.Width <= c.Padding.Horizontal)
                c.Width = c.Padding.Horizontal;
            int x = c.Location.X, y = c.Location.Y;
            if (x < 0) x = 1;
            if (y < 0) y = 1;
            if (c.Location.X != x || c.Location.Y != y)
                c.Location = new Point(x, y);
            c.Invalidate();
            string? name = ParentName(c);
            XYHL p = new XYHL
            {
                H = c.Height,
                L = c.Width,
                Nome = c.Name,
                X = c.Location.X,
                Y = c.Location.Y,
                Form = name
            };
            Task.Run(() => WritePositionSize(p));
            return p;
        }
        private static string? ParentName(Control c)
        {
            if (c.Parent != null)
                return c.Parent.Name;
            var parent = c.FindForm();
            if (parent != null)
                return parent.Name;
            return null;
        }
        private static bool salvando;

        public static Task WritePositionSize(XYHL positionSize)
        {
            var repo = XYHLRepository.Instancia();
            if (repo == null) return Task.CompletedTask;
            if (positionSize.X < 0) positionSize.X = 1;
            if (positionSize.Y < 0) positionSize.Y = 1;
            if (positionSize.L < 10) positionSize.L = 10;
            if (positionSize.H < 10) positionSize.H = 10;

            if (!salvando)
            {
                salvando = true;
                _ = repo.Salvar(positionSize);
                salvando = false;
            }
            return Task.CompletedTask;
        }

        #endregion

        #endregion


    }
}
