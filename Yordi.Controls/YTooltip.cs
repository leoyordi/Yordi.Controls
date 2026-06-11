using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Yordi.Controls
{
    /// <summary>
    /// Tooltip personalizado, desenhado com base em CurrentTooltipTheme
    /// </summary>
    public class YTooltip : ToolTip
    {
        private int radius = 10;
        private int arrowSize = 10;
        private Control? sender;
        private SizeF tooltipSize;
        private SizeF textSize;
        private int paddingH = 0, paddingV = 0;
        private Point? controlPosition;
        private Font font = SystemFonts.DefaultFont;

        public Font Font { get => font; set => font = value; }

        // P/Invoke para recortar a janela nativa do tooltip com cantos arredondados
        [DllImport("user32.dll")] private static extern IntPtr WindowFromDC(IntPtr hDC);
        [DllImport("user32.dll")] private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        // IsBalloon=true bloqueia o evento Draw internamente no .NET 8 (mesmo com OwnerDraw=true).
        // A propriedade é ocultada para neutralizar qualquer chamada externa.
        public new bool IsBalloon
        {
            get => false;
            set { /* Ignorado: incompatível com OwnerDraw no .NET 8 */ }
        }

        private static GraphicsPath CreateRoundedPath(Rectangle rect, int r)
        {
            var path = new GraphicsPath();
            int d = r * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public YTooltip()
        {
            BackColor = CurrentTooltipTheme.BackColor;
            ForeColor = CurrentTooltipTheme.ForeColor;
            font = CurrentTooltipTheme.Font;
            CurrentTooltipTheme.TooltipThemeChanged += () =>
            {
                BackColor = CurrentTooltipTheme.BackColor;
                ForeColor = CurrentTooltipTheme.ForeColor;
                font = CurrentTooltipTheme.Font;
            };
            OwnerDraw = true;
            Draw += Tooltip_Draw;
            Popup += Tooltip_Popup;
        }

        public YTooltip(TooltipTheme thema)
        {
            OwnerDraw = true;
            if (thema.BackColor != null)
                BackColor = thema.BackColor.Value;
            if (thema.ForeColor != null)
                ForeColor = thema.ForeColor.Value;
            if (thema.Font != null)
                font = thema.Font;
            Draw += Tooltip_Draw;
            Popup += Tooltip_Popup;
        }

        /// <summary>
        /// Define o texto do tooltip para um controle
        /// </summary>
        public new void SetToolTip(Control? control, string? texto)
        {
            if (control == null || !control.IsHandleCreated || control.IsDisposed) return;
            if (control.InvokeRequired)
                control.Invoke(() =>
                {
                    sender = control;
                    base.SetToolTip(control, texto);
                });
            else
            {
                sender = control;
                base.SetToolTip(control, texto);
            }
        }

        /// <summary>
        /// Mostra o tooltip para um controle
        /// </summary>
        public void ShowTooltip(Control sender, string msg, bool isBallon = true, int duration = 5000)
        {
            this.sender = sender;
            controlPosition = new Point(0, -20);
            Show(msg, sender, controlPosition.Value, duration);
        }

        /// <summary>
        /// Mostra o tooltip para um controle em uma posição específica
        /// </summary>
        public void ShowTooltip(Control sender, string msg, Point point, bool isBallon = true, int duration = 5000)
        {
            this.sender = sender;
            controlPosition = point;
            Show(msg, sender, point, duration);
        }

        public void HideTooltip(Control control)
        {
            Hide(control);
            sender = null;           // Limpa estado para evitar ponteiro stale
            controlPosition = null;
        }

        protected void Tooltip_Draw(object? sender, DrawToolTipEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var path = CreateRoundedPath(e.Bounds, radius);

            // Aplica a Region arredondada na janela nativa → recorta os cantos de verdade
            IntPtr hdc = g.GetHdc();
            IntPtr hwnd = WindowFromDC(hdc);
            g.ReleaseHdc(hdc);
            if (hwnd != IntPtr.Zero)
            {
                using var rgn = new Region(path);
                IntPtr hRgn = rgn.GetHrgn(g);
                SetWindowRgn(hwnd, hRgn, true); // Windows toma posse de hRgn após esta chamada
            }

            // 1. Preenche o fundo
            using var bgBrush = new SolidBrush(CurrentTooltipTheme.BackColor);
            g.FillPath(bgBrush, path);

            // 2. Borda
            using var borderPen = new Pen(CurrentTooltipTheme.ForeColor, 1.5f);
            g.DrawPath(borderPen, path);

            // 3. Seta/ponteiro direcional
            DrawPointer(g);

            // 4. Texto
            Rectangle textBounds = new Rectangle(paddingH / 2, paddingV / 2, e.Bounds.Width - paddingH, e.Bounds.Height - paddingV);
            TextRenderer.DrawText(g, e.ToolTipText, font, textBounds, CurrentTooltipTheme.ForeColor, flags);
        }

        /// <summary>
        /// Desenha o tooltip no formato balão com seta dinâmica centralizada no controle associado
        /// </summary>
        protected void DrawBalloon(object? sender, DrawToolTipEventArgs e)
        {
            int balloonRadius = 12;
            int arrowWidth = 16;
            int arrowHeight = 10;
            int margin = 2;

            int arrowCenterX = e.Bounds.Width / 2;

            if (this.sender?.Parent != null)
            {
                var control = this.sender;
                var tooltipScreen = control.PointToScreen(Point.Empty);
                var parentScreen = control.Parent.PointToScreen(Point.Empty);
                int controlCenterX = tooltipScreen.X - parentScreen.X + control.Width / 2;
                arrowCenterX = Math.Max(balloonRadius + arrowWidth / 2 + margin,
                    Math.Min(e.Bounds.Width - balloonRadius - arrowWidth / 2 - margin, controlCenterX));
            }

            Rectangle balloonRect = new Rectangle(
                e.Bounds.X + margin,
                e.Bounds.Y + margin,
                e.Bounds.Width - margin * 2,
                e.Bounds.Height - arrowHeight - margin * 2
            );

            using (GraphicsPath path = new GraphicsPath(FillMode.Winding))
            {
                path.AddArc(balloonRect.Left, balloonRect.Top, balloonRadius, balloonRadius, 180, 90);
                path.AddArc(balloonRect.Right - balloonRadius, balloonRect.Top, balloonRadius, balloonRadius, 270, 90);
                path.AddArc(balloonRect.Right - balloonRadius, balloonRect.Bottom - balloonRadius, balloonRadius, balloonRadius, 0, 90);
                path.AddLine(balloonRect.Right, balloonRect.Bottom, arrowCenterX + arrowWidth / 2, balloonRect.Bottom);
                path.AddLine(arrowCenterX + arrowWidth / 2, balloonRect.Bottom, arrowCenterX, balloonRect.Bottom + arrowHeight);
                path.AddLine(arrowCenterX, balloonRect.Bottom + arrowHeight, arrowCenterX - arrowWidth / 2, balloonRect.Bottom);
                path.AddLine(arrowCenterX - arrowWidth / 2, balloonRect.Bottom, balloonRect.Left + balloonRadius, balloonRect.Bottom);
                path.AddArc(balloonRect.Left, balloonRect.Bottom - balloonRadius, balloonRadius, balloonRadius, 90, 90);
                path.CloseFigure();

                using (SolidBrush brush = new SolidBrush(CurrentTooltipTheme.BackColor))
                    e.Graphics.FillPath(brush, path);
                using (Pen pen = new Pen(CurrentTooltipTheme.ForeColor))
                    e.Graphics.DrawPath(pen, path);
            }

            Rectangle textRect = new Rectangle(balloonRect.Left + 6, balloonRect.Top + 4, balloonRect.Width - 12, balloonRect.Height - 8);
            TextRenderer.DrawText(e.Graphics, e.ToolTipText, font, textRect, CurrentTooltipTheme.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        private void DrawPointer(Graphics g)
        {
            if (controlPosition == null || sender == null) return;
            int y;
            var p = (sender.Size.Height - controlPosition.Value.Y) - tooltipSize.Height;
            y = p < 0 ? (int)p * -1 : arrowSize;

            Point[] arrowPoints =
            [
                new Point(0, y + (arrowSize / 2)),
                new Point(arrowSize, y),
                new Point(arrowSize, y + arrowSize)
            ];
            using (SolidBrush brush = new SolidBrush(CurrentTooltipTheme.ArrowColor))
                g.FillPolygon(brush, arrowPoints);
        }

        TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak;

        protected void Tooltip_Popup(object? sender, PopupEventArgs e)
        {
            if (sender is not ToolTip || e.AssociatedControl == null) return;
            PaddingText(e);
            CalculateTextSize(e, false);
            e.ToolTipSize = Size.Round(tooltipSize);
        }

        private void CalculateTextSize(PopupEventArgs e, bool textRenderer = true)
        {
            if (e.AssociatedControl == null) return;
            string? txt = GetToolTip(e.AssociatedControl);
            if (string.IsNullOrEmpty(txt)) return;
            if (textRenderer)
                textSize = TextRenderer.MeasureText(txt, font, new Size(200, 0), flags);
            else
            {
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                    textSize = g.MeasureString(txt, font);
            }
            tooltipSize = new SizeF(textSize.Width + paddingH, textSize.Height + paddingV);
        }

        private void PaddingText(PopupEventArgs e)
        {
            paddingH = 0;
            paddingV = 0;
            if (e?.AssociatedControl?.Parent != null)
            {
                paddingH = e.AssociatedControl.Parent.Padding.Horizontal;
                paddingV = e.AssociatedControl.Parent.Padding.Vertical;
            }
            int arrowSizeX2 = (arrowSize * 2) + 2;
            if (paddingH < arrowSizeX2) paddingH = arrowSizeX2;
            if (paddingV < arrowSizeX2) paddingV = arrowSizeX2;
        }
    }
}