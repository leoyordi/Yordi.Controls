using System.Drawing.Drawing2D;

namespace Yordi.Controls
{
    /// <summary>
    /// Controle que representa uma linha. Deverá definir como horizontal ou vertical no momento da criação.
    /// </summary>
    public class LineControl : ControlXYHL
    {
        public enum LineOrientation
        {
            Horizontal,
            Vertical
        }

        private LineOrientation orientation;
        private Color lineColor;
        private int lineThickness;
        private int lineBlink;
        private int animationOffset;
        private int animationInterval = 400;
        private bool animate = false;
        private System.Windows.Forms.Timer animationTimer;

        #region Construtores
        public LineControl(LineOrientation orientation)
        {
            this.orientation = orientation;
            InitializeComponent();
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint, true);
            BackColor = Color.Transparent;

            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = animationInterval;
            animationTimer.Tick += AnimationTimer_Tick;
        }
        public LineControl() : this(LineOrientation.Horizontal) { }
        private void InitializeComponent()
        {
            lineColor = Color.Black;
            lineThickness = 2;
            lineBlink = lineThickness * 2;
        }

        #endregion

        public Color LineColor
        {
            get { return lineColor; }
            set { lineColor = value; Invalidate(); }
        }

        public int LineThickness
        {
            get { return lineThickness; }
            set
            {
                lineThickness = value;
                lineBlink = lineThickness * 2;
                Invalidate();
            }
        }

        public LineOrientation Orientation
        {
            get { return orientation; }
            set { orientation = value; Invalidate(); }
        }
        public int AnimationInterval { get => animationInterval; set => animationInterval = value; }
        public bool Animate
        {
            get => animate;
            set
            {
                animate = value;
                if (animate && !animationTimer.Enabled)
                    animationTimer.Start();
                else if (animationTimer.Enabled)
                    animationTimer.Stop();
                Invalidate();
            }
        }

        int line;
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            DrawBackground(graphics);
            base.OnPaint(e);
            int x, y, h, w;
            line = blink && animate ? lineBlink : lineThickness;
            if (orientation == LineOrientation.Horizontal)
            {
                x = 0;
                w = Width;
                h = Height / 2;
                y = h;
            }
            else
            {
                y = 0;
                w = Width / 2;
                h = Height;
                x = w;
            }
            using (Pen pen = new Pen(lineColor, line))
                e.Graphics.DrawLine(pen, x, y, w, h);
        }
        private void DrawBackground(Graphics graphics)
        {
            if (Parent != null)
            {
                // Desenha os controles que estão no mesmo nível e se intersectam com o controle atual
                foreach (Control control in Parent.Controls)
                {
                    if (control != this && control.Visible && control.TabIndex < this.TabIndex && control.Bounds.IntersectsWith(this.Bounds))
                    {
                        using (Bitmap controlBmp = new Bitmap(control.Width, control.Height))
                        {
                            control.DrawToBitmap(controlBmp, new Rectangle(0, 0, control.Width, control.Height));
                            graphics.DrawImage(controlBmp, control.Left - Left, control.Top - Top);
                        }
                    }
                }
            }
        }
        private void DrawMovingGradient(Graphics graphics, Rectangle rect)
        {
            var linearOriention = orientation == LineOrientation.Horizontal ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical;
            using (LinearGradientBrush brush = new LinearGradientBrush(rect, Color.White, lineColor, linearOriention))
            {
                brush.TranslateTransform(animationOffset, 0);
                graphics.FillRectangle(brush, rect);
            }
        }
        private void DrawMovingCircle(Graphics graphics, Rectangle rect)
        {
            int circleDiameter = lineThickness * 2; // Diâmetro do círculo
            if (orientation == LineOrientation.Horizontal)
            {
                graphics.FillEllipse(new SolidBrush(lineColor), animationOffset, rect.Y, circleDiameter, circleDiameter);
            }
            else
            {
                graphics.FillEllipse(new SolidBrush(lineColor), rect.X, animationOffset, circleDiameter, circleDiameter);
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
                isResizable = false;
                if (meDimensionar != null && !meDimensionar.IsDisposed)
                    meDimensionar.Checked = isResizable;
                isMovable = false;
                if (meMove != null && !meMove.IsDisposed)
                    meMove.Checked = isMovable;
                SetXYHL();
                return;
            }

            int moveStep = 10; // Constante de movimentação padrão
            if (e.Shift)
                moveStep = 1; // Constante de movimentação com Shift
            else if (e.Control)
                moveStep = 3; // Constante de movimentação com Control

            if (e.KeyCode == Keys.Left)
            {
                if (isMovable)
                    this.Left -= moveStep;
                else if (isResizable && orientation == LineOrientation.Horizontal)
                    this.Width -= moveStep;
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (isMovable)
                    this.Left += moveStep;
                else if (isResizable && orientation == LineOrientation.Horizontal)
                    this.Width += moveStep;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (isMovable)
                    this.Top -= moveStep;
                else if (isResizable && orientation == LineOrientation.Vertical)
                    this.Height -= moveStep;
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (isMovable)
                    this.Top += moveStep;
                else if (isResizable && orientation == LineOrientation.Vertical)
                    this.Height += moveStep;
            }

            // Redesenha o controle após a movimentação ou redimensionamento
            if (isMovable || isResizable)
                Invalidate();
        }

        public override void SetXYHL()
        {
            var position = FindXYHL();
            if (orientation == LineOrientation.Horizontal)
                position.H = lineThickness + Padding.Vertical;
            else
                position.L = lineThickness + Padding.Horizontal;
            SetLocationTask(position);
            Invalidate();
        }

        protected override void RedimensionarControle(MouseEventArgs e)
        {
            if (isResizable)
            {
                if (orientation == LineOrientation.Horizontal)
                    this.Width = e.X;
                else
                    this.Height = e.Y;
            }
            else if (isMovable)
            {
                this.Left += e.X - mouseDownLocation.X;
                this.Top += e.Y - mouseDownLocation.Y;
            }
            if (isResizable || isMovable)
                Invalidate();
        }

        protected override void DefinirCursor(MouseEventArgs e)
        {
            if (!IsRuntime)
            {
                Cursor = Cursors.Default;
                mEdge = EdgeEnum.None;
                return;
            }
            if (isMovable)
            {
                Cursor = Cursors.SizeAll;
                mEdge = EdgeEnum.TopLeft;
                return;
            }
            if (orientation == LineOrientation.Horizontal)
            {
                //left corner
                if (e.X <= Padding.Horizontal)
                {
                    Cursor = Cursors.SizeWE;
                    mEdge = EdgeEnum.Left;
                }
                //right corner
                else if (e.X >= (Width - (Padding.Horizontal + 1)))
                {
                    Cursor = Cursors.SizeWE;
                    mEdge = EdgeEnum.Right;
                }
            }
            else if (orientation == LineOrientation.Vertical)
            {
                //top corner
                if (e.Y <= Padding.Vertical)
                {
                    Cursor = Cursors.SizeNS;
                    mEdge = EdgeEnum.Top;
                }
                //bottom corner
                else if (e.Y >= (Height - (Padding.Vertical + 1)))
                {
                    Cursor = Cursors.SizeNS;
                    mEdge = EdgeEnum.Bottom;
                }
            }
            //no edge
            else
            {
                Cursor = Cursors.Default;
                mEdge = EdgeEnum.None;
            }
        }

        bool blink = false;
        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            blink = !blink;
            Invalidate();
        }
    }
}
