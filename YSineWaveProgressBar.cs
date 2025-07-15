using System.Drawing.Drawing2D;

namespace Yordi.Controls
{
    public class YRoundProgressBar : ControlXYHL
    {
        private decimal maximum = 100m;
        private decimal progress = 0m;
        private Color barColor = Color.Blue;
        private Color backgroundColor = Color.LightGray;
        private bool infinite = false;
        private int animationOffset = 0;
        private int animationInterval = 400;
        private System.Windows.Forms.Timer animationTimer;

        // Propriedades para a senoide
        private int amplitude = 20; // Amplitude da senoide
        private int frequency = 5;  // Frequência da senoide

        public YRoundProgressBar()
        {
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

        public decimal Progress
        {
            get => progress;
            set
            {
                progress = Math.Clamp(value, 0, maximum);
                Invalidate();
            }
        }

        public Color BarColor
        {
            get => barColor;
            set
            {
                barColor = value;
                Invalidate();
            }
        }

        public Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
                Invalidate();
            }
        }

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

        public int Amplitude
        {
            get => amplitude;
            set
            {
                amplitude = value;
                Invalidate();
            }
        }

        public int Frequency
        {
            get => frequency;
            set
            {
                frequency = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Desenhar o fundo
            using (Brush backgroundBrush = new SolidBrush(backgroundColor))
            {
                graphics.FillRectangle(backgroundBrush, ClientRectangle);
            }

            // Desenhar a senoide
            using (Pen progressPen = new Pen(barColor, 2))
            {
                PointF[] points = GenerateSineWavePoints();
                graphics.DrawLines(progressPen, points);
            }
        }

        private PointF[] GenerateSineWavePoints()
        {
            int width = ClientRectangle.Width;
            int height = ClientRectangle.Height / 2; // Linha base da senoide
            int centerY = ClientRectangle.Y + height;

            List<PointF> points = new List<PointF>();
            for (int x = 0; x < width; x++)
            {
                float y = (float)(centerY + amplitude * Math.Sin((2 * Math.PI * frequency * (x + animationOffset)) / width));
                points.Add(new PointF(x, y));
            }

            return points.ToArray();
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (infinite)
            {
                animationOffset = (animationOffset + 5) % ClientRectangle.Width; // Incrementa o deslocamento
                Invalidate();
            }
        }
    }
}
