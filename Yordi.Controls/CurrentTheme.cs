namespace Yordi.Controls
{
    /// <summary>
    /// Classe para definir a aparência atual do tema
    /// </summary>
    public class CurrentTheme
    {
        private static Font _font = SystemFonts.DefaultFont;
        private static Color _foreColor = SystemColors.ControlText;
        private static Color _backColor = SystemColors.Control;
        private static Color _borderColor = SystemColors.ActiveBorder;
        private static Color _draggingBorderColor = SystemColors.HotTrack;
        private static Color _padrao1 = SystemColors.Desktop;
        private static Color _padrao2 = SystemColors.AppWorkspace;
        private static int _borderWidth = 0;

        /// <summary>
        /// Evento disparado quando o tema é alterado
        /// </summary>
        public static event Action? ThemeChanged;

        /// <summary>
        /// Obtém ou define a fonte do tema
        /// </summary>
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

        /// <summary>
        /// Obtém ou define a cor do texto do tema
        /// </summary>
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

        /// <summary>
        /// Obtém ou define a cor de fundo do tema
        /// </summary>
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

        /// <summary>
        /// Obtém ou define a cor da borda do tema
        /// </summary>
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

        /// <summary>
        /// Obtém ou define a largura da borda do tema
        /// </summary>
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

        /// <summary>
        /// Obtém ou define a cor da borda de arraste do tema
        /// </summary>
        public static Color DraggingBorderColor
        {
            get => _draggingBorderColor;
            set
            {
                if (_draggingBorderColor != value)
                {
                    _draggingBorderColor = value;
                    OnThemeChanged();
                }
            }
        }

        /// <summary>
        /// Obtém ou define a cor padrão 1 do tema
        /// </summary>
        public static Color CorPadrao1
        {
            get => _padrao1;
            set
            {
                if (_padrao1 != value)
                {
                    _padrao1 = value;
                    OnThemeChanged();
                }
            }
        }

        /// <summary>
        /// Obtém ou define a cor padrão 2 do tema
        /// </summary>
        public static Color CorPadrao2
        {
            get => _padrao2;
            set
            {
                if (_padrao2 != value)
                {
                    _padrao2 = value;
                    OnThemeChanged();
                }
            }
        }

        /// <summary>
        /// Define o tema com base em um objeto Theme
        /// </summary>
        /// <param name="theme">Objeto Theme contendo as definições do tema</param>
        public static void SetTheme(Theme theme)
        {
            if (theme == null) return;
            if (theme.Font != null) _font = theme.Font;
            if (theme.ForeColor != null) _foreColor = theme.ForeColor.Value;
            if (theme.BackColor != null) _backColor = theme.BackColor.Value;
            if (theme.BorderColor != null) _borderColor = theme.BorderColor.Value;
            if (theme.BorderWidth >= 0) _borderWidth = theme.BorderWidth;
            if (theme.DraggingBorderColor != null) _draggingBorderColor = theme.DraggingBorderColor.Value;
            if (theme.CorPadrao1 != null) _padrao1 = theme.CorPadrao1.Value;
            if (theme.CorPadrao2 != null) _padrao2 = theme.CorPadrao2.Value;

            OnThemeChanged();
        }

        /// <summary>
        /// Método chamado quando o tema é alterado
        /// </summary>
        private static void OnThemeChanged()
        {
            ThemeChanged?.Invoke();
        }
    }
}
