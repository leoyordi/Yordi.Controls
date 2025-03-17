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

        protected Point mouseDownLocation;
        public IContainer? components { get; set; } = null;
        private bool _enableDrag;
        private bool _enableMove;
        private Pen borderPen = new Pen(ControlPaint.Light(Color.Transparent, 0.0f), 0);
        private SolidBrush _backgroundBrush = new SolidBrush(Color.Transparent);

        #region Construtores
        public ControlXYHL()
        {
            InitializeComponent();
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
            this.SetLocation();
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
        public bool Moving {get; set;}

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Resizing { get; set; }

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
            set { borderRadius = value; this.CalculateAreaPath(); Invalidate(); }
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
                this.CalculateAreaPath();
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
        private int opacity = 100;


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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public Rectangle AreaPath { get => areaPath; set => areaPath = value; }
        private Rectangle areaPath;
        #endregion


        protected internal void Message(string msg, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0)
        {
            if (!Disposing)
                MessageEvent?.Invoke(msg, $"{Name}.{origem}", line);
        }

        protected void DrawPath(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            base.OnPaint(e);
            this.CalculateAreaPath();
            e.Graphics.DrawRoundedRectangle(borderPen, areaPath, borderRadius, borderEdges);
        }
        protected void FillPath(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            this.CalculateAreaPath();
            e.Graphics.FillRoundedRectangle(_backgroundBrush, areaPath, borderRadius, borderEdges);
        }

        protected virtual void ImageAlpha()
        {
            Invalidate();
        }

        protected virtual void PaddingMinimo(Padding padding)
        {
            var top = padding.Top;
            var bottom = padding.Bottom;
            var left = padding.Left;
            var right = padding.Right;
            if (top < 1) top = 1;
            if (bottom < 1) bottom = 1;
            if (left < 1) left = 1;
            if (right < 1) right = 1;
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

        public event XYHLDelegate? XYHLChanged;
        public event MyMessage? MessageEvent;


        public void MenuMoveClick(object? sender, EventArgs e)
        {
            this.MenuMoveClick();
        }
        public void MenuDimensionarClick(object? sender, EventArgs e)
        {
            this.MenuDimensionarClick();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            this.SetLocation();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            BordaTracejada(e);
        }

        private void BordaTracejada(PaintEventArgs e)
        {
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
            if (!IsRuntime && e.Button == MouseButtons.Left)
            {
                base.OnMouseDown(e);
                this.OnMouseDownEx(e);
            }
            else if (e.Button == MouseButtons.Left)
            {
                this.Capture = true;
                mouseDownLocation = e.Location;
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
            if (!IsRuntime && e.Button == MouseButtons.Left)
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
                this.SetLocation();
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
            SuspendLayout();
            this.RedimensionarControleEx(e);
            ResumeLayout();
        }
        protected virtual void DefinirCursor(MouseEventArgs e)
        {
            this.DefinirCursorEx(e);
        }




        #region Localização
        public void XYHLChangedInvoke(XYHL p)
        {
            XYHLChanged?.Invoke(p);
        }

        #region Métodos estáticos

        #endregion

        #endregion


    }
}
