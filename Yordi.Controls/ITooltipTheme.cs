namespace Yordi.Controls
{
    public interface ITooltipTheme
    {
        Font? Font { get; set; }
        Color? ForeColor { get; set; }
        Color? BackColor { get; set; }
        Color? BorderColor { get; set; }
        int BorderWidth { get; set; }
    }
}
