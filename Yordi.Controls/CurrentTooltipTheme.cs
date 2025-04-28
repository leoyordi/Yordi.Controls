namespace Yordi.Controls
{
    /// <summary>
    /// Classe para definir aparência de um tooltip personalizado
    /// </summary>
    public class CurrentTooltipTheme
    {
        private static Font _font = SystemFonts.DefaultFont;
        private static Color _foreColor = SystemColors.ControlText;
        private static Color _backColor = SystemColors.Control;
        private static Color _borderColor = SystemColors.ActiveBorder;
        private static Color _arrowColor = SystemColors.HotTrack;
        private static int _borderWidth = 0;

        public static event Action? TooltipThemeChanged;

        public static Font Font
        {
            get => _font;
            set
            {
                if (value == null) return;
                if (_font != value)
                {
                    _font = value;
                    OnTooltipThemeChanged();
                }
            }
        }

        public static Color ForeColor
        {
            get => _foreColor;
            set
            {
                if (_foreColor != value)
                {
                    _foreColor = value;
                    OnTooltipThemeChanged();
                }
            }
        }

        public static Color BackColor
        {
            get => _backColor;
            set
            {
                if (_backColor != value)
                {
                    _backColor = value;
                    OnTooltipThemeChanged();
                }
            }
        }

        public static Color BorderColor
        {
            get => _borderColor;
            set
            {
                if (_borderColor != value)
                {
                    _borderColor = value;
                    OnTooltipThemeChanged();
                }
            }
        }
        public static Color ArrowColor
        {
            get => _arrowColor;
            set
            {
                if (_arrowColor != value)
                {
                    _arrowColor = value;
                    OnTooltipThemeChanged();
                }
            }
        }
        public static int BorderWidth
        {
            get => _borderWidth;
            set
            {
                if (value < 0) return;
                if (_borderWidth != value)
                {
                    _borderWidth = value;
                    OnTooltipThemeChanged();
                }
            }
        }

        public static void SetTheme(TooltipTheme theme)
        {
            if (theme == null) return;
            if (theme.Font != null) _font = theme.Font;
            if (theme.ForeColor != null) _foreColor = theme.ForeColor.Value;
            if (theme.BackColor != null) _backColor = theme.BackColor.Value;
            if (theme.BorderColor != null) _borderColor = theme.BorderColor.Value;
            if (theme.BorderWidth >= 0) _borderWidth = theme.BorderWidth;
            if (theme.ArrowColor != null) _arrowColor = theme.ArrowColor.Value;
            OnTooltipThemeChanged();
        }

        public static TooltipTheme Theme()
        {
            return new TooltipTheme
            {
                Font = _font,
                ForeColor = _foreColor,
                BackColor = _backColor,
                BorderColor = _borderColor,
                BorderWidth = _borderWidth,
                ArrowColor = _arrowColor
            };
        }
        private static void OnTooltipThemeChanged()
        {
            TooltipThemeChanged?.Invoke();
        }

    }
}
