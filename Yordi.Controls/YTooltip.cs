namespace Yordi.Controls
{
    /// <summary>
    /// Tooltip personalizado, desenhado com base em CurrentTooltipTheme
    /// </summary>
    public class YTooltip : ToolTip
    {
        public Font Font { get; set; } = CurrentTooltipTheme.Font;
        public YTooltip() // Corrigido o nome do construtor
        {
            BackColor = CurrentTooltipTheme.BackColor;
            ForeColor = CurrentTooltipTheme.ForeColor;
            OwnerDraw = true;
            Draw += Tooltip_Draw;
            Popup += Tooltip_Popup;
        }

        public new void SetToolTip(Control? control, string? texto)
        {
            if (control == null || !control.IsHandleCreated || control.IsDisposed) return;
            if (control.InvokeRequired)
                control.Invoke(() => base.SetToolTip(control, texto));
            else
                base.SetToolTip(control, texto);
        }
        public void ShowTooltip(Control sender, string msg, bool isBallon = true)
        {
            IsBalloon = isBallon;
            Show(msg, sender, 0, -20, 5000);
        }
        public void ShowTooltip(Control sender, string msg, Point point, bool isBallon = true)
        {
            IsBalloon = isBallon;
            Show(msg, sender, point, 5000);
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
                Point[] arrowPoints = GetArrowPoints(e.Bounds, ArrowDirection.Up); // Mudar a direção conforme necessário
                using (SolidBrush brush = new SolidBrush(CurrentTooltipTheme.BackColor))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                    e.Graphics.FillPolygon(brush, arrowPoints);
                }
                using (Pen pen = new Pen(CurrentTooltipTheme.BackColor))
                    e.Graphics.DrawRectangle(pen, e.Bounds);
                TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.Top;
                TextRenderer.DrawText(e.Graphics, e.ToolTipText, CurrentTooltipTheme.Font, e.Bounds, Color.White, flags);
            }
        }

        protected void Tooltip_Popup(object? sender, PopupEventArgs e)
        {
            if (sender is not ToolTip tooltip || e.AssociatedControl == null) return;
            var size = TextRenderer.MeasureText(tooltip.GetToolTip(e.AssociatedControl), this.Font);
            int paddingH = 0, paddingV = 0;
            if (e.AssociatedControl.Parent != null)
            {
                paddingH = e.AssociatedControl.Parent.Padding.Horizontal;
                paddingV = e.AssociatedControl.Parent.Padding.Vertical;
            }
            e.ToolTipSize = new Size(size.Width + paddingH, size.Height + paddingV);
        }
        private static Point[] GetArrowPoints(Rectangle bounds, ArrowDirection direction)
        {
            int arrowSize = 10;
            Point[] points = new Point[3];

            switch (direction)
            {
                case ArrowDirection.Up:
                    points[0] = new Point(bounds.Left + bounds.Width / 2 - arrowSize / 2, bounds.Top);
                    points[1] = new Point(bounds.Left + bounds.Width / 2 + arrowSize / 2, bounds.Top);
                    points[2] = new Point(bounds.Left + bounds.Width / 2, bounds.Top - arrowSize);
                    break;
                case ArrowDirection.Down:
                    points[0] = new Point(bounds.Left + bounds.Width / 2 - arrowSize / 2, bounds.Bottom);
                    points[1] = new Point(bounds.Left + bounds.Width / 2 + arrowSize / 2, bounds.Bottom);
                    points[2] = new Point(bounds.Left + bounds.Width / 2, bounds.Bottom + arrowSize);
                    break;
                case ArrowDirection.Left:
                    points[0] = new Point(bounds.Left, bounds.Top + bounds.Height / 2 - arrowSize / 2);
                    points[1] = new Point(bounds.Left, bounds.Top + bounds.Height / 2 + arrowSize / 2);
                    points[2] = new Point(bounds.Left - arrowSize, bounds.Top + bounds.Height / 2);
                    break;
                case ArrowDirection.Right:
                    points[0] = new Point(bounds.Right, bounds.Top + bounds.Height / 2 - arrowSize / 2);
                    points[1] = new Point(bounds.Right, bounds.Top + bounds.Height / 2 + arrowSize / 2);
                    points[2] = new Point(bounds.Right + arrowSize, bounds.Top + bounds.Height / 2);
                    break;
            }

            return points;
        }
        public enum ArrowDirection
        {
            Up,
            Down,
            Left,
            Right
        }

    }
}