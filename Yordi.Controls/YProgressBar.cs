using System.Drawing.Drawing2D;

namespace Yordi.Controls
{
    public enum ProgressPointType
    {
        PointOrCircle,
        Dash,
        Bar,
        Gradient
    }

    public class YProgressBar : ControlXYHL
    {
        private decimal maximum = 100m;
        private decimal progress = 0m;
        private Color barColor = Color.Blue;
        private Color backgroundColor = Color.LightGray;
        private ProgressPointType progressPointType = ProgressPointType.PointOrCircle;
        private Color colorProgressPoint = Color.Red;
        private bool infinite = false;
        protected int animationOffset = 0;
        protected int animationJump = 1;
        protected int animationInterval = 400;
        private bool animate = false;
        protected System.Windows.Forms.Timer animationTimer;
        private LineOrientation orientation;
        private Rectangle? barRect;


        #region Construtores
        /// <summary>
        /// Construtor que define a orientação da linha
        /// </summary>
        /// <param name="orientation">Orientação da linha</param>
        public YProgressBar(LineOrientation orientation)
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
        public YProgressBar() : this(LineOrientation.Horizontal) { }
        private void InitializeComponent()
        {
            //barColor = Color.Black;
            //lineThickness = 2;
            //lineBlink = lineThickness * 2;
        }

        #endregion
        /// <summary>
        /// Obtém ou define se a barra deve ser animada, semelhante a um gif
        /// </summary>
        public bool Infinite
        {
            get => infinite;
            set
            {
                infinite = value;
                if (infinite && !animationTimer.Enabled)
                    animationTimer.Start();
                else if (!infinite && animationTimer.Enabled)
                    animationTimer.Stop();
                Invalidate();
            }
        }

        /// <summary>
        /// Obtém ou define o valor máximo da barra
        /// </summary>
        public decimal Maximum
        {
            get => maximum;
            set
            {
                if (value <= 0) throw new ArgumentException("Maximum must be greater than 0.");
                maximum = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtém ou define o valor atual da barra
        /// </summary>
        public decimal Progress
        {
            get => progress;
            set
            {
                _progressRealValue = value;
                progress = Math.Clamp(value, 0, maximum);
                Invalidate();
            }
        }
        private decimal _progressRealValue;
        
        
        public new string Text
        {
            get => text;
            set { text = value; Invalidate(); }
        }
        private string text = string.Empty;

        public bool ShowText
        {
            get => showText;
            set 
            { 
                showText = value;
                if (showText)
                    showPercentage = false; // Se mostrar texto, não mostrar porcentagem
                Invalidate(); 
            }
        }
        private bool showText = true;

        public bool ShowPercentage
        {
            get => showPercentage;
            set 
            { 
                showPercentage = value;
                if (showPercentage)
                    showText = false; // Se mostrar porcentagem, não mostrar texto
                Invalidate(); 
            }
        }
        private bool showPercentage = true;

        /// <summary>
        /// Obtém ou define a cor da barra
        /// </summary>
        public Color BarColor
        {
            get => barColor;
            set
            {
                barColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtém ou define a cor de fundo da barra
        /// </summary>
        public Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtém ou define o tipo de ponto de progresso
        /// </summary>
        public ProgressPointType ProgressPointType
        {
            get => progressPointType;
            set
            {
                progressPointType = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Obtém ou define a cor do ponto de progresso
        /// </summary>
        public Color ColorProgressPoint
        {
            get => colorProgressPoint;
            set
            {
                colorProgressPoint = value;
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
        /// Obtém ou define o intervalo de animação. Usado com Infinite == true
        /// </summary>
        public int AnimationInterval 
        { 
            get => animationInterval;
            set
            {
                var status = animationTimer.Enabled;
                if (status)
                    animationTimer.Stop();
                animationTimer.Interval = value;
                animationInterval = value;
                if (status)
                    animationTimer.Start();
                Invalidate();
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Desenhar o fundo da barra
            using (Brush backgroundBrush = new SolidBrush(backgroundColor))
            {
                graphics.FillRectangle(backgroundBrush, ClientRectangle);
            }

            // Desenhar a barra cheia
            barRect = GetBarRectangle();
            using (Brush barBrush = new SolidBrush(barColor))
            {
                graphics.FillRectangle(barBrush, barRect.Value);
            }

            // Calcular a posição do progresso
            float progressRatio = infinite ? 0 : (float)(progress / maximum);
            int progressPosition;
            if (infinite)
                progressPosition = animationOffset;
            else
            {
                if (Orientation == LineOrientation.Horizontal)
                    progressPosition = (int)(Width * progressRatio);
                else
                    progressPosition = (int)(Height * progressRatio);
            }

            // Desenhar o ponto de progresso ou gradiente
            switch (progressPointType)
            {
                case ProgressPointType.PointOrCircle:
                    DrawProgressCircle(graphics, progressPosition);
                    break;
                case ProgressPointType.Dash:
                    DrawProgressDash(graphics, progressPosition);
                    break;
                case ProgressPointType.Bar:
                    DrawProgressBar(graphics, progressPosition);
                    break;
                case ProgressPointType.Gradient:
                    DrawMovingGradient(graphics, progressPosition);
                    break;
            }
            DrawText(graphics);
        }

        private void DrawProgressCircle(Graphics graphics, int progressPosition)
        {
            int diameter = Orientation == LineOrientation.Horizontal ? Height - Margin.Vertical : Width - Margin.Horizontal;
            int realPosition = progressPosition - diameter / 2;
            Rectangle circleRect = Orientation == LineOrientation.Horizontal
                ? new Rectangle(realPosition, Margin.Top, diameter, diameter)
                : new Rectangle(Margin.Left, (Height - diameter) - realPosition, diameter, diameter);

            using (Brush progressBrush = new SolidBrush(colorProgressPoint))
            {
                graphics.FillEllipse(progressBrush, circleRect);
            }
        }

        private void DrawProgressDash(Graphics graphics, int progressPosition)
        {
            int hMax = Height - Margin.Vertical;
            int wMax = Width - Margin.Horizontal;
            int h, w, x, y;
            if (orientation == LineOrientation.Horizontal)
            {
                x = progressPosition - (hMax / 2);
                y = Margin.Top;
                h = hMax;
                w = hMax / 2;
            }
            else
            {
                x = Margin.Left;
                y = Height - (progressPosition + (wMax/2));
                h = wMax / 2;
                w = wMax;
            }
            Rectangle dashRect = new Rectangle(x, y, w, h);

            using (Brush progressBrush = new SolidBrush(colorProgressPoint))
            {
                graphics.FillRectangle(progressBrush, dashRect);
            }
        }
        private void DrawText(Graphics graphics)
        {
            if (!showText && !showPercentage) return;
            string progressText = string.Empty;
            if (showPercentage) 
                progressText = _progressRealValue.ToString("0.##") + "%";
            else if (showText)
                progressText = text;
            TextRenderer.DrawText(graphics, progressText, Font, ClientRectangle, ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }
        private void DrawProgressBar(Graphics graphics, int progressPosition)
        {
            int h = Height - Margin.Vertical;
            int w = Width - Margin.Horizontal;
            Rectangle barProgressRect = Orientation == LineOrientation.Horizontal
                ? new Rectangle(Margin.Left, Margin.Top, progressPosition, h)
                : new Rectangle(Margin.Left, Height - progressPosition, w, progressPosition);

            using (Brush progressBrush = new SolidBrush(colorProgressPoint))
            {
                graphics.FillRectangle(progressBrush, barProgressRect);
            }
        }

        private void DrawMovingGradient(Graphics graphics, int progressPosition)
        {
            var linearOrientation = Orientation == LineOrientation.Horizontal
                ? LinearGradientMode.Horizontal
                : LinearGradientMode.Vertical;
            barRect = GetBarRectangle();
            //int position = infinite ? animationOffset : (int)(Width * (progress / maximum));
            using (LinearGradientBrush brush = new LinearGradientBrush(barRect.Value, Color.White, colorProgressPoint, linearOrientation))
            {
                brush.TranslateTransform(progressPosition, 0);
                graphics.FillRectangle(brush, barRect.Value);
            }
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (infinite)
            {
                animationOffset += 5; // Velocidade do movimento
                if ((orientation == LineOrientation.Horizontal && animationOffset > Width)
                    || (orientation == LineOrientation.Vertical && animationOffset > Height))
                    animationOffset = 0; // Reinicia o movimento
                Invalidate();
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
        private Rectangle GetBarRectangle()
        {
            if (barRect == null 
                || barRect.Value.Width != Width - Margin.Horizontal
                || barRect.Value.Height != Height - Margin.Vertical
                || barRect.Value.X != Margin.Left
                || barRect.Value.Y != Margin.Top)
                barRect = new Rectangle(Margin.Left, Margin.Top, Width - Margin.Horizontal, Height - Margin.Vertical);
            return barRect.Value;
        }
    }
}
