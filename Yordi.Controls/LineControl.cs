using System.Drawing.Drawing2D;

namespace Yordi.Controls
{
    public enum AnimationType
    {
        None,
        Blink,
        Gradient,
        Circle
    }
    /// <summary>
    /// Controle que representa uma linha. Deverá definir como horizontal ou vertical no momento da criação.
    /// </summary>
    public class LineControl : ControlXYHL
    {
        /// <summary>
        /// Enumeração para definir a orientação da linha
        /// </summary>

        private LineOrientation orientation;
        private AnimationType animationType = AnimationType.None;
        private Color lineColor;
        //private int lineThickness;
        //private int lineBlink;
        protected int animationOffset = 0;
        protected int animationJump = 1;
        protected int animationInterval = 400;
        private bool animate = false;
        protected System.Windows.Forms.Timer animationTimer;

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
            Resize += (s, e) => Resized();
            MarginChanged += (s, e) => Resized();
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
            //lineThickness = 2;
            //lineBlink = lineThickness * 2;
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


        public AnimationType AnimationType
        {
            get { return animationType; }
            set { animationType = value; Invalidate(); }
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
            if (animationType == AnimationType.Gradient)
                DrawMovingGradient(graphics);
            else if (animationType == AnimationType.Circle)
                DrawMovingCircle(graphics);
            else
            {
                int x, y, h, w;
                int lineFull;
                if (orientation == LineOrientation.Horizontal)
                {
                    x = 0;
                    w = Width;
                    h = Height / 2;
                    y = h;
                    if (Margin.Vertical > 0)
                        lineFull = Height - Margin.Vertical;
                    else
                        lineFull = Height - 2; // sempre 2 pontos afastados do limite
                }
                else
                {
                    y = 0;
                    w = Width / 2;
                    h = Height;
                    x = w;
                    if (Margin.Horizontal > 0)
                        lineFull = Width - Margin.Horizontal;
                    else
                        lineFull = Width - 2; // sempre 2 pontos afastados do limite

                }
                if (lineFull <= 0)
                    lineFull = 1; // sempre 1 ponto
                if (blink && animate)
                    line = lineFull - 2;
                else
                    line = lineFull;
                //line = blink && animate ? lineBlink : lineThickness;
                if (line < 0)
                    line = 0; // sempre 0 ponto;
                using (Pen pen = new Pen(lineColor, line))
                    e.Graphics.DrawLine(pen, x, y, w, h);
            }
        }

        /// <summary>
        /// Desenha o fundo do controle
        /// </summary>
        /// <param name="graphics">Objeto Graphics para desenhar</param>
        private void DrawBackground(Graphics graphics)
        {
            if (Parent != null)
            {
                var lines = Parent.Controls.OfType<LineControl>().Where(l => l != this && l.Visible).ToList();
                if (lines == null || lines.Count == 0)
                    return;
                // Desenha os controles que estão no mesmo nível e se intersectam com o controle atual
                foreach (Control control in lines)
                {
                    if (control != this && control.Visible && control.TabIndex > this.TabIndex && control.Bounds.IntersectsWith(this.Bounds))
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
        private void DrawMovingGradient(Graphics graphics)
        {
            var linearOriention = orientation == LineOrientation.Horizontal ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical;
            Rectangle real = new Rectangle(Margin.Left, Margin.Top, Width - Margin.Horizontal, Height - Margin.Vertical);
            using (LinearGradientBrush brush = new LinearGradientBrush(ClientRectangle, Color.White, lineColor, linearOriention))
            {
                brush.TranslateTransform(animationOffset, 0);
                graphics.FillRectangle(brush, real);
            }
        }

        /// <summary>
        /// Desenha um círculo em movimento
        /// </summary>
        /// <param name="graphics">Objeto Graphics para desenhar</param>
        /// <param name="rect">Retângulo onde o círculo será desenhado</param>
        private void DrawMovingCircle(Graphics graphics)
        {
            int circleDiameter; // = lineThickness * 2; // Diâmetro do círculo
            if (orientation == LineOrientation.Horizontal)
            {
                circleDiameter = Height - Margin.Vertical;
                if (circleDiameter < 1)
                    circleDiameter = 1; // Garantir que o diâmetro seja pelo menos 1
                graphics.FillEllipse(new SolidBrush(lineColor), animationOffset, ClientRectangle.Y + Margin.Top, circleDiameter, circleDiameter);
            }
            else
            {
                circleDiameter = Width - Margin.Horizontal;
                if (circleDiameter < 1)
                    circleDiameter = 1; // Garantir que o diâmetro seja pelo menos 1
                graphics.FillEllipse(new SolidBrush(lineColor), ClientRectangle.X + Margin.Left, animationOffset, circleDiameter, circleDiameter);
            }
        }


        private void Resized()
        {
            if (orientation == LineOrientation.Horizontal)
                animationJump = (Width - Margin.Horizontal) / 10;
            else
                animationJump = (Height - Margin.Vertical) / 10;
            if (animationJump < 1)
                animationJump = 1; // Garantir que o salto seja pelo menos 1
            animationOffset = animationJump;
            Invalidate();
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
        public void SetXYHL()
        {
            var position = this.FindXYHL();
            if (orientation == LineOrientation.Horizontal)
                position.H = Height;// lineThickness + Padding.Vertical;
            else
                position.L = Width; // lineThickness + Padding.Horizontal;
            this.SetLocation(position);
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
            if (animationType == AnimationType.Blink)
                blink = !blink;
            else if (animationType == AnimationType.Gradient || animationType == AnimationType.Circle)
            {
                animationOffset += animationJump;
                if (orientation == LineOrientation.Horizontal)
                {
                    if (animationOffset > (Width - Margin.Horizontal))
                        animationOffset = 0;
                }
                else if (orientation == LineOrientation.Vertical)
                {
                    if (animationOffset > (Height - Margin.Vertical))
                        animationOffset = 0;
                }
            }
            Invalidate();
        }


        protected override void PaddingMinimo(Padding padding)
        {
            var top = padding.Top;
            var bottom = padding.Bottom;
            var left = padding.Left;
            var right = padding.Right;
            if (top < 0) top = 0;
            if (bottom < 0) bottom = 0;
            if (left < 0) left = 0;
            if (right < 0) right = 0;
            if (top != base.Padding.Top || bottom != base.Padding.Bottom || left != base.Padding.Left || right != base.Padding.Right)
                SetControlPadding(new Padding(left, top, right, bottom));
        }

    }
}
