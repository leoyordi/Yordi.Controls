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
        private bool odd; //ciclo ímpar

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

        protected  void OnPaintOriginal(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw background circle
            using (Brush backgroundBrush = new SolidBrush(backgroundColor))
            {
                graphics.FillEllipse(backgroundBrush, ClientRectangle);
            }

            // Draw progress arc
            float progressAngle = infinite ? animationOffset : (float)(360 * (progress / maximum));
            using (Pen progressPen = new Pen(barColor, Math.Min(Width, Height) / 10f))
            {
                RectangleF arcRect = new RectangleF(
                    ClientRectangle.X + progressPen.Width / 2,
                    ClientRectangle.Y + progressPen.Width / 2,
                    ClientRectangle.Width - (int)progressPen.Width,
                    ClientRectangle.Height - (int)progressPen.Width);
                graphics.DrawArc(progressPen, arcRect, -90, progressAngle);
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Desenhar o círculo de fundo
            using (Brush backgroundBrush = new SolidBrush(backgroundColor))
            {
                graphics.FillEllipse(backgroundBrush, ClientRectangle);
            }

            // Calcular o ângulo do progresso
            float progressAngle = infinite ? animationOffset : (float)(360 * (progress / maximum));

            // Desenhar o arco
            using (Pen progressPen = new Pen(barColor, Math.Min(Width, Height) / 10f))
            {
                RectangleF arcRect = new RectangleF(
                    ClientRectangle.X + progressPen.Width / 2,
                    ClientRectangle.Y + progressPen.Width / 2,
                    ClientRectangle.Width - (int)progressPen.Width,
                    ClientRectangle.Height - (int)progressPen.Width);
                if (!odd)
                    graphics.DrawArc(progressPen, arcRect, -90, progressAngle);
                else
                    graphics.DrawArc(progressPen, arcRect, -90 + progressAngle, 360 - progressAngle);
            }
        }


        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (infinite)
            {
                animationOffset = (animationOffset + 5) % 360; // Incrementa e reinicia o ciclo
                if (animationOffset == 0) // Quando o ciclo completa 360 graus
                {
                    odd = !odd; // Alterna a cor
                }
                Invalidate();
            }
        }
    }
}
