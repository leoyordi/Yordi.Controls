namespace Yordi.Controls
{
    public interface IEnabled
    {
        bool Enabled { get; set; }
    }
    public interface IVisibleEx
    {
        bool VisibleEx { get; set; }
    }
    public interface IEmUso
    {
        bool EmUso { get; set; }
    }
    public interface IControlName
    {
        string ControlName { get; }
    }
    public interface IText : IEnabled, ITag
    {
        string Text { get; set; }
        bool ReadOnly { get; set; }
    }
    public interface ISelectedIndex : IEnabled
    {
        int SelectedIndex { get; set; }
    }
    public interface ICheckBox : IEnabled, ITag
    {
        bool AutoCheck { get; set; }
        bool Checked { get; set; }
        CheckState CheckState { get; set; }
    }
    public interface IImage : IEnabled
    {
        Image? Image { get; set; }
    }
    public interface ITag
    {
        object? Tag { get; set; }
    }
}
