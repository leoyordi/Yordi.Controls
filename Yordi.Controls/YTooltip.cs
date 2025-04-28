
namespace Yordi.Controls
{
    /// <summary>
    /// Tooltip personalizado, desenhado com base em CurrentTooltipTheme
    /// </summary>
    public class YTooltip : ToolTip
    {
        private int radius = 5;
        private int arrowSize = 10;
        private Control? sender;
        private Size tooltipSize;
        private Size textSize;
        private int paddingH = 0, paddingV = 0;
        private Point? controlPosition;
        private Font font = SystemFonts.DefaultFont;

        public Font Font { get => font; set => font = value; }

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public YTooltip()
        {
            BackColor = Color.DarkBlue;// CurrentTooltipTheme.BackColor;
            ForeColor = Color.White; // CurrentTooltipTheme.ForeColor;
            CurrentTooltipTheme.TooltipThemeChanged += () => 
            { 
                BackColor = CurrentTooltipTheme.BackColor; 
                ForeColor = CurrentTooltipTheme.ForeColor;
                font = CurrentTooltipTheme.Font;
            };
            OwnerDraw = true;
            IsBalloon = true;
            Draw += Tooltip_Draw;
            Popup += Tooltip_Popup;
        }

        public YTooltip(TooltipTheme thema)
        {
            OwnerDraw = true;
            IsBalloon = true;
            if (thema.BackColor != null)
                BackColor = thema.BackColor.Value;
            if (thema.ForeColor != null)
                ForeColor = thema.ForeColor.Value;
            if (thema.Font != null)
                font = thema.Font;
            base.Draw += Tooltip_Draw;
            base.Popup += Tooltip_Popup;
        }
        /// <summary>
        /// Define o texto do tooltip para um controle
        /// </summary>
        /// <param name="control">Controle para o qual o tooltip será definido</param>
        /// <param name="texto">Texto do tooltip</param>
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
        /// <param name="sender">Controle que disparou o evento</param>
        /// <param name="msg">Mensagem do tooltip</param>
        /// <param name="isBallon">Indica se o tooltip deve ser em formato de balão</param>
        public void ShowTooltip(Control sender, string msg, bool isBallon = true, int duration = 5000)
        {
            IsBalloon = isBallon;
            this.sender = sender;
            controlPosition = new Point(0, -20);
            Show(msg, sender, controlPosition.Value, duration);
        }

        /// <summary>
        /// Mostra o tooltip para um controle em uma posição específica
        /// </summary>
        /// <param name="sender">Controle que disparou o evento</param>
        /// <param name="msg">Mensagem do tooltip</param>
        /// <param name="point">Posição onde o tooltip será exibido</param>
        /// <param name="isBallon">Indica se o tooltip deve ser em formato de balão</param>
        public void ShowTooltip(Control sender, string msg, Point point, bool isBallon = true, int duration = 5000)
        {
            IsBalloon = isBallon;
            this.sender = sender;
            controlPosition = point;
            Show(msg, sender, point, duration);
        }

        public void HideTooltip(Control control)
        {
            Hide(control);
            IsBalloon = true;
        }

        protected void Tooltip_Draw(object? sender, DrawToolTipEventArgs e)
        {
            if (!IsBalloon)
            {
                using (Pen pen = new Pen(BackColor))
                    e.Graphics.DrawRoundedRectangle(pen, e.Bounds, radius);
                using (SolidBrush back = new SolidBrush(BackColor))
                    e.Graphics.FillRoundedRectangle(back, e.Bounds, radius);
                DrawPointer(e.Graphics);
                TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.Top;
                Rectangle textBounds = new Rectangle(paddingH / 2, paddingV / 2, e.Bounds.Width - paddingH, e.Bounds.Height - paddingV);
                TextRenderer.DrawText(e.Graphics, e.ToolTipText, font, textBounds, ForeColor, flags);
            }
        }
        private void DrawPointer(Graphics g)
        {
            if (IsBalloon || controlPosition == null || sender == null) return;
            int y;
            var p = (sender.Size.Height - controlPosition.Value.Y) - tooltipSize.Height;
            if (p < 0)
                y = p * -1;
            else
                y = arrowSize;

            // seta
            Point[] arrowPoints = new Point[3]
            {
                new Point(0, y + (arrowSize / 2)),
                new Point(arrowSize, y),
                new Point(arrowSize, y + arrowSize)
            };
            using (SolidBrush brush = new SolidBrush(CurrentTooltipTheme.ArrowColor))
                g.FillPolygon(brush, arrowPoints);

            //// Linha
            //int x, y, h, w;
            //x = arrowSize / 2;
            //w = arrowSize;
            //h = arrowSize / 2;
            //using (Pen pen = new Pen(CurrentTooltipTheme.ArrowColor, arrowSize / 2))
            //    g.DrawLine(pen, x, y, w, h);

        }
        protected void Tooltip_Popup(object? sender, PopupEventArgs e)
        {
            if (sender is not ToolTip tooltip || e.AssociatedControl == null) return;
            paddingH = 0;
            paddingV = 0;
            if (e.AssociatedControl.Parent != null)
            {
                paddingH = e.AssociatedControl.Parent.Padding.Horizontal;
                paddingV = e.AssociatedControl.Parent.Padding.Vertical;
            }
            if (!IsBalloon)
            {
                int arrowSizeX2 = (arrowSize * 2) + 2;
                if (paddingH < arrowSizeX2)
                    paddingH = arrowSizeX2;
                if (paddingV < arrowSizeX2)
                    paddingV = arrowSizeX2;
                textSize = TextRenderer.MeasureText(tooltip.GetToolTip(e.AssociatedControl), font);
            }
            else
                textSize = TextRenderer.MeasureText(tooltip.GetToolTip(e.AssociatedControl), font);

            tooltipSize = new Size(textSize.Width + paddingH, textSize.Height + paddingV);
            e.ToolTipSize = tooltipSize;
            
        }

    }
}