﻿namespace Yordi.Controls
{
    /// <summary>
    /// Classe para definir aparência dos menus de contexto personalizados
    /// </summary>
    public class CurrentContextMenuTheme
    {
        private static Font _font = SystemFonts.DefaultFont;
        private static Color _foreColor = SystemColors.ControlText;
        private static Color _backColor = SystemColors.Control;
        private static Color _borderColor = SystemColors.ActiveBorder;
        private static int _borderWidth = 0;

        public static event Action? ContextMenuThemeChanged;

        public static Font Font
        {
            get => _font;
            set
            {
                if (value == null) return;
                if (_font != value)
                {
                    _font = value;
                    OnThemeChanged();
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
                    OnThemeChanged();
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
                    OnThemeChanged();
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
                    OnThemeChanged();
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
                    OnThemeChanged();
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
            OnThemeChanged();
        }

        private static void OnThemeChanged()
        {
            ContextMenuThemeChanged?.Invoke();
        }
    }
}
