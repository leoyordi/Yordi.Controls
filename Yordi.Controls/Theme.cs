﻿namespace Yordi.Controls
{
    public class Theme : ITheme
    {
        public Font? Font { get; set; }

        public Color? ForeColor { get; set; }

        public Color? BackColor { get; set; }

        public Color? BorderColor { get; set; }

        public int BorderWidth { get; set; } = 0;
    }
}
