using System.Drawing.Drawing2D;

namespace Yordi.Controls
{
    /// <summary>
    /// Controle que representa uma linha. Deverá definir como horizontal ou vertical no momento da criação.
    /// </summary>
    public class LineControl : ControlXYHL
    {
        /// <summary>
        /// Enumeração para definir a orientação da linha
        /// </summary>
        public enum LineOrientation
        {
            Horizontal,
            Vertical
        }

        private LineOrientation orientation;
        private Color lineColor;
        private int lineThickness;
        private int lineBlink;
        private int animationOffset = 0;
        private int animationInterval = 400;
        private bool animate = false;
        private System.Windows.Forms.Timer animationTimer;

        #region Construtores
        /// <summary>
        /// Construtor que define a orientação da linha
        /// </summary>
        /// <param name="orientation">Orientação da linha</param>
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

        /// <summary>
        /// Construtor padrão que define a linha como horizontal
        /// </summary>
        public LineControl() : this(LineOrientation.Horizontal) { }
        private void InitializeComponent()
        {
            lineColor = Color.Black;
            lineThickness = 2;
            lineBlink = lineThickness * 2;
        }

        #endregion

        /// <summary>
        /// Obtém ou define a cor da linha
        /// </summary>
        public Color LineColor
        {
            get { return lineColor; }
            set { lineColor = value; Invalidate(); }
        }

        /// <summary>
        /// Obtém ou define a espessura da linha
        /// </summary>
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

        /// <summary>
        /// Obtém ou define a orientação da linha
        /// </summary>
        public LineOrientation Orientation
        {
            get { return orientation; }
            set { orientation = value; Invalidate(); }
        }

        /// <summary>
        /// Obtém ou define o intervalo de animação
        /// </summary>
        public int AnimationInterval { get => animationInterval; set => animationInterval = value; }

        /// <summary>
        /// Obtém ou define se a linha deve ser animada
        /// </summary>
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

        /// <summary>
        /// Método de pintura do controle
        /// </summary>
        /// <param name="e">Argumentos do evento de pintura</param>
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

        /// <summary>
        /// Desenha o fundo do controle
        /// </summary>
        /// <param name="graphics">Objeto Graphics para desenhar</param>
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

        /// <summary>
        /// Desenha um gradiente em movimento
        /// </summary>
        /// <param name="graphics">Objeto Graphics para desenhar</param>
        /// <param name="rect">Retângulo onde o gradiente será desenhado</param>
        private void DrawMovingGradient(Graphics graphics, Rectangle rect)
        {
            var linearOriention = orientation == LineOrientation.Horizontal ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical;
            using (LinearGradientBrush brush = new LinearGradientBrush(rect, Color.White, lineColor, linearOriention))
            {
                brush.TranslateTransform(animationOffset, 0);
                graphics.FillRectangle(brush, rect);
            }
        }

        /// <summary>
        /// Desenha um círculo em movimento
        /// </summary>
        /// <param name="graphics">Objeto Graphics para desenhar</param>
        /// <param name="rect">Retângulo onde o círculo será desenhado</param>
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

        /// <summary>
        /// Evento de pressionamento de tecla
        /// </summary>
        /// <param name="e">Argumentos do evento de tecla</param>
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
                moveStep = 1; // Constante de movimentação com Shift
            else if (e.Control)
                moveStep = 3; // Constante de movimentação com Control

            if (e.KeyCode == Keys.Left)
            {
                if (Moving)
                    this.Left -= moveStep;
                else if (Resizing && orientation == LineOrientation.Horizontal)
                    this.Width -= moveStep;
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (Moving)
                    this.Left += moveStep;
                else if (Resizing && orientation == LineOrientation.Horizontal)
                    this.Width += moveStep;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (Moving)
                    this.Top -= moveStep;
                else if (Resizing && orientation == LineOrientation.Vertical)
                    this.Height -= moveStep;
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (Moving)
                    this.Top += moveStep;
                else if (Resizing && orientation == LineOrientation.Vertical)
                    this.Height += moveStep;
            }

            // Redesenha o controle após a movimentação ou redimensionamento
            if (Moving || Resizing)
                Invalidate();
        }

        /// <summary>
        /// Define a posição e tamanho do controle
        /// </summary>
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

        /// <summary>
        /// Redimensiona o controle com base no evento do mouse
        /// </summary>
        /// <param name="e">Argumentos do evento do mouse</param>
        protected override void RedimensionarControle(MouseEventArgs e)
        {
            if (Resizing)
            {
                if (orientation == LineOrientation.Horizontal)
                    this.Width = e.X;
                else
                    this.Height = e.Y;
            }
            else if (Moving)
            {
                this.Left += e.X - mouseDownLocation.X;
                this.Top += e.Y - mouseDownLocation.Y;
            }
            if (Resizing || Moving)
                Invalidate();
        }

        /// <summary>
        /// Define o cursor do controle com base no evento do mouse
        /// </summary>
        /// <param name="e">Argumentos do evento do mouse</param>
        protected override void DefinirCursor(MouseEventArgs e)
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
            if (orientation == LineOrientation.Horizontal)
            {
                //left corner
                if (e.X <= Padding.Horizontal)
                {
                    Cursor = Cursors.SizeWE;
                    Edge = EdgeEnum.Left;
                }
                //right corner
                else if (e.X >= (Width - (Padding.Horizontal + 1)))
                {
                    Cursor = Cursors.SizeWE;
                    Edge = EdgeEnum.Right;
                }
            }
            else if (orientation == LineOrientation.Vertical)
            {
                //top corner
                if (e.Y <= Padding.Vertical)
                {
                    Cursor = Cursors.SizeNS;
                    Edge = EdgeEnum.Top;
                }
                //bottom corner
                else if (e.Y >= (Height - (Padding.Vertical + 1)))
                {
                    Cursor = Cursors.SizeNS;
                    Edge = EdgeEnum.Bottom;
                }
            }
            //no edge
            else
            {
                Cursor = Cursors.Default;
                Edge = EdgeEnum.None;
            }
        }

        bool blink = false;

        /// <summary>
        /// Evento de tick do temporizador de animação
        /// </summary>
        /// <param name="sender">Objeto que disparou o evento</param>
        /// <param name="e">Argumentos do evento</param>
        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            blink = !blink;
            Invalidate();
        }
    }
}
