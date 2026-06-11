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

        /// <summary>
        /// Gets or sets a value indicating whether text should be colored based on contrast.
        /// If true, text color will be adjusted based on the background color of the progress bar.
        /// If false, text will use the ForeColor property.
        /// </summary>
        public bool ColorTextByContrast { get; set; } = false;

        private bool inProgress = false;
        /// <summary>
        /// Indica que a operação representada pela barra ainda está em andamento
        /// (o veredito sobre o valor ainda é pendente).
        /// <para>
        /// Quando <see langword="true"/>, a barra é pintada inteiramente com
        /// <see cref="InProgressColor"/> e os <see cref="ColorRanges"/> são ignorados.
        /// Quando <see langword="false"/>, a cor é resolvida avaliando o valor atual
        /// contra os <see cref="ColorRanges"/> (comportamento padrão/legado).
        /// </para>
        /// Default: <see langword="false"/>.
        /// </summary>
        public bool InProgress
        {
            get => inProgress;
            set { inProgress = value; Invalidate(); }
        }

        private Color inProgressColor = Color.Orange;
        /// <summary>
        /// Cor chapada usada para pintar toda a barra enquanto
        /// <see cref="InProgress"/> for <see langword="true"/>. Default: <see cref="Color.Orange"/>.
        /// </summary>
        public Color InProgressColor
        {
            get => inProgressColor;
            set { inProgressColor = value; if (inProgress) Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the default collection of color ranges used by cells in this column.
        /// </summary>
        public List<ProgressBarColorRange> DefaultColorRanges { get; set; } = new()
        {
            new ProgressBarColorRange { Min = 0, Max = 90, Color = Color.GreenYellow },
            new ProgressBarColorRange { Min = 90, Max = 100, Color = Color.Orange },
            new ProgressBarColorRange { Min = 100, Max = 110, Color = Color.Red },
            new ProgressBarColorRange { Min = 110, Max = int.MaxValue, Color = Color.Purple }
        };
        /// <summary>
        /// Gets or sets the collection of color ranges used to determine the visual representation of the progress bar
        /// based on its value.
        /// </summary>
        public List<ProgressBarColorRange>? ColorRanges { get; set; }

        /// <summary>
        /// Retorna os ranges efetivos: da instância, ou o DefaultColorRanges, ou null se nenhum.
        /// </summary>
        private List<ProgressBarColorRange>? GetEffectiveColorRanges()
        {
            if (ColorRanges != null && ColorRanges.Count > 0)
                return ColorRanges;

            if (DefaultColorRanges != null && DefaultColorRanges.Count > 0)
                return DefaultColorRanges;

            return null;
        }

        /// <summary>
        /// Retorna a cor efetiva do ponto de progresso baseada no estado atual.
        /// Se <see cref="InProgress"/> estiver ativo, retorna <see cref="InProgressColor"/>
        /// (barra chapada). Caso contrário, avalia o valor atual contra os ranges;
        /// se não houver ranges configurados, retorna ColorProgressPoint.
        /// </summary>
        private Color GetEffectiveColor()
        {
            if (inProgress) return inProgressColor;

            var ranges = GetEffectiveColorRanges();
            if (ranges == null) return colorProgressPoint;

            var range = ranges.FirstOrDefault(r => r.IsInRange((float)_progressRealValue));
            return range?.Color ?? colorProgressPoint;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Desenhar o fundo da barra
            using (Brush backgroundBrush = new SolidBrush(backgroundColor))
            {
                graphics.FillRoundedRectangle(backgroundBrush, ClientRectangle, BorderRadius);
            }

            // Desenhar a barra cheia
            barRect = GetBarRectangle();
            using (Brush barBrush = new SolidBrush(barColor))
            {
                graphics.FillRoundedRectangle(barBrush, barRect.Value, BorderRadius);
            }

            // Calcular a posição do progresso
            float progressRatio = infinite ? 0 : (float)(progress / maximum);
            int progressPosition;
            if (infinite)
                progressPosition = animationOffset;
            else
            {
                if (Orientation == LineOrientation.Horizontal)
                    progressPosition = (int)(barRect.Value.Width * progressRatio);
                else
                    progressPosition = (int)(barRect.Value.Height * progressRatio);
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
        }

        private void DrawProgressCircle(Graphics graphics, int progressPosition)
        {
            int diameter = Orientation == LineOrientation.Horizontal ? Height - Margin.Vertical : Width - Margin.Horizontal;
            int realPosition = progressPosition - diameter / 2;
            Rectangle circleRect = Orientation == LineOrientation.Horizontal
                ? new Rectangle(realPosition, Margin.Top, diameter, diameter)
                : new Rectangle(Margin.Left, (Height - diameter) - realPosition, diameter, diameter);

            var effectiveColor = GetEffectiveColor(); // <── aqui
            using (Brush progressBrush = new SolidBrush(effectiveColor))
                graphics.FillEllipse(progressBrush, circleRect);
            if (ColorTextByContrast)
                DrawTextForDashAndCircle(graphics, circleRect);
            else
                DrawText(graphics);
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
                y = Height - (progressPosition + (wMax / 2));
                h = wMax / 2;
                w = wMax;
            }
            Rectangle dashRect = new Rectangle(x, y, w, h);

            var effectiveColor = GetEffectiveColor(); // <── aqui
            using (Brush progressBrush = new SolidBrush(effectiveColor))
                graphics.FillRectangle(progressBrush, dashRect);

            if (ColorTextByContrast)
                DrawTextForDashAndCircle(graphics, dashRect);
            else
                DrawText(graphics);
        }

        // Substitua DrawText(Graphics) - troca TextRenderer (GDI) por DrawString (GDI+)
        private void DrawText(Graphics graphics)
        {
            if (!showText && !showPercentage) return;
            string progressText = string.Empty;
            if (showPercentage)
                progressText = _progressRealValue.ToString("0.##") + "%";
            else if (showText)
                progressText = text;

            using var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            using var brush = new SolidBrush(ForeColor);
            graphics.DrawString(progressText, Font, brush, (RectangleF)ClientRectangle, sf);
        }

        private void DrawTextForDashAndCircle(Graphics graphics, Rectangle dashRect)
        {
            if (!showText && !showPercentage) return;
            string progressText = showPercentage
                                ? _progressRealValue.ToString("0.##") + "%"
                                : text;
            using (StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                SizeF textSize = graphics.MeasureString(progressText, Font, ClientRectangle.Size, sf);
                Rectangle textRect = new Rectangle(
                    ClientRectangle.X + (ClientRectangle.Width - (int)textSize.Width) / 2,
                    ClientRectangle.Y + (ClientRectangle.Height - (int)textSize.Height) / 2,
                    (int)textSize.Width,
                    (int)textSize.Height);

                // 1. Desenha texto com contraste sobre o fundo geral
                Color normalColorText = GraphicsExtension.GetContrastingTextColor(backgroundColor);
                graphics.DrawString(progressText, Font, new SolidBrush(normalColorText), ClientRectangle, sf);

                // 2. Clip na interseção: contraste sobre a cor efetiva (range ou colorProgressPoint)
                Rectangle intersectRect = Rectangle.Intersect(textRect, dashRect);
                if (!intersectRect.IsEmpty)
                {
                    Region oldClip = graphics.Clip;
                    graphics.SetClip(intersectRect);
                    Color effectiveColor = GetEffectiveColor(); // <── corrigido
                    Color contrastColor = GraphicsExtension.GetContrastingTextColor(effectiveColor);
                    graphics.DrawString(progressText, Font, new SolidBrush(contrastColor), ClientRectangle, sf);
                    graphics.SetClip(oldClip, CombineMode.Replace);
                }
            }
        }

        private void DrawTextOverGradient(Graphics graphics, Rectangle barRect, LinearGradientBrush gradientBrush)
        {
            if (!showText && !showPercentage) return;
            string progressText = showPercentage ? _progressRealValue.ToString("0.##") + "%" : text;
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                // Medir o texto para centralizar
                SizeF textSize = graphics.MeasureString(progressText, Font, barRect.Size, sf);
                float startX = barRect.X + (barRect.Width - textSize.Width) / 2;
                float y = barRect.Y + (barRect.Height - textSize.Height) / 2;

                // Para cada caractere, calcula a posição e cor do gradiente
                float x = startX;
                foreach (char c in progressText)
                {
                    string s = c.ToString();
                    SizeF charSize = graphics.MeasureString(s, Font);
                    float charCenterX = x + charSize.Width / 2;

                    // Calcula a cor do gradiente na posição do caractere
                    float ratio = (charCenterX - barRect.X) / barRect.Width;
                    Color gradColor = InterpolateGradientColor(gradientBrush, ratio);

                    // Cor de contraste
                    Color textColor = GraphicsExtension.GetContrastingTextColor(gradColor);

                    using (var brush = new SolidBrush(textColor))
                        graphics.DrawString(s, Font, brush, x, y);

                    x += charSize.Width;
                }
            }
        }

        // Função para interpolar cor do gradiente (simplificada para LinearGradientBrush horizontal)
        private Color InterpolateGradientColor(LinearGradientBrush brush, float ratio)
        {
            Color c1 = brush.LinearColors[0];
            Color c2 = brush.LinearColors[1];
            int r = (int)(c1.R + (c2.R - c1.R) * ratio);
            int g = (int)(c1.G + (c2.G - c1.G) * ratio);
            int b = (int)(c1.B + (c2.B - c1.B) * ratio);
            return Color.FromArgb(r, g, b);
        }

        private void DrawText(Graphics g, float progressVal, Color barColor, Rectangle cellBounds, Rectangle barValue, Rectangle barDiff)
        {
            if (!showText && !showPercentage) return;
            string txt = string.Empty;
            if (showPercentage)
                txt = _progressRealValue.ToString("0.##") + "%";
            else if (showText)
                txt = text;

            // Desenha texto sobre a área da barra de progresso
            Region oldClip = g.Clip;

            // Determina a cor de fundo efetiva 
            Color backColor = barColor;
            if (backColor == Color.Transparent)
            {
                if (backgroundColor != Color.Transparent)
                    backColor = backgroundColor;
                else if (this.BackColor != Color.Transparent)
                    backColor = this.BackColor;
                else if (this.Parent?.BackColor != null && Parent.BackColor != Color.Transparent)
                    backColor = Parent.BackColor;
            }

            // Calcula a luminância para decidir a cor do texto
            Color textColorBackgound = GraphicsExtension.GetContrastingTextColor(backColor);
            Color textColorBar = GraphicsExtension.GetContrastingTextColor(barColor);

            using (StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                // Desenha texto sobre a área do fundo
                g.SetClip(barDiff);
                using (var brushBg = new SolidBrush(textColorBackgound))
                    g.DrawString(txt, Font, brushBg, cellBounds, sf);

                // Desenha texto sobre a área da barra
                g.SetClip(barValue);
                using (var brushBar = new SolidBrush(textColorBar))
                    g.DrawString(txt, Font, brushBar, cellBounds, sf);


                // Restaura o clip original
                g.SetClip(oldClip, CombineMode.Replace);
            }
        }

        private void DrawProgressBar(Graphics graphics, int progressPosition)
        {
            int h = Height - Margin.Vertical;
            int w = Width - Margin.Horizontal;
            Rectangle barValue = Orientation == LineOrientation.Horizontal
                ? new Rectangle(Margin.Left, Margin.Top, progressPosition, h)
                : new Rectangle(Margin.Left, Height - progressPosition, w, progressPosition);
            Rectangle barDiff = Orientation == LineOrientation.Horizontal
                ? new Rectangle(Margin.Left + progressPosition, barValue.Y, w - progressPosition, h)
                : new Rectangle(Margin.Left, Margin.Top + progressPosition, w, h - progressPosition);

            var effectiveColor = GetEffectiveColor(); // <── aqui
            using (Brush progressBrush = new SolidBrush(effectiveColor))
                graphics.FillRoundedRectangle(progressBrush, barValue, BorderRadius);

            if (ColorTextByContrast)
                DrawTextForDashAndCircle(graphics, barValue);
            else
                DrawText(graphics);
        }

        private void DrawMovingGradient(Graphics graphics, int progressPosition)
        {
            var linearOrientation = Orientation == LineOrientation.Horizontal
                ? LinearGradientMode.Horizontal
                : LinearGradientMode.Vertical;
            barRect = GetBarRectangle();

            var effectiveColor = GetEffectiveColor();

            // O gradiente parte de branco (ou a cor de fundo) até a cor efetiva
            Color gradientStart = backgroundColor == Color.Transparent ? Color.White : backgroundColor;

            using (LinearGradientBrush brush = new LinearGradientBrush(barRect.Value, gradientStart, effectiveColor, linearOrientation))
            {
                brush.TranslateTransform(progressPosition, 0);
                graphics.FillRoundedRectangle(brush, barRect.Value, BorderRadius);
            }

            if (ColorTextByContrast)
            {
                // Usa a cor intermediária do gradiente (ponto central) para calcular o contraste
                // pois o texto está centralizado sobre o gradiente
                float midRatio = 0.5f;
                int r = (int)(gradientStart.R + (effectiveColor.R - gradientStart.R) * midRatio);
                int g2 = (int)(gradientStart.G + (effectiveColor.G - gradientStart.G) * midRatio);
                int b = (int)(gradientStart.B + (effectiveColor.B - gradientStart.B) * midRatio);
                Color midColor = Color.FromArgb(r, g2, b);

                Color contrastColor = GraphicsExtension.GetContrastingTextColor(midColor);
                if (!showText && !showPercentage) return;
                string progressText = showPercentage ? _progressRealValue.ToString("0.##") + "%" : text;
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var brush = new SolidBrush(contrastColor);
                graphics.DrawString(progressText, Font, brush, (RectangleF)ClientRectangle, sf);
            }
            else
            {
                DrawText(graphics);
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
