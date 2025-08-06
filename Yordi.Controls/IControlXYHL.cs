using System.ComponentModel;
using Yordi.Tools;

namespace Yordi.Controls
{
    public interface IControlXYHL : IVisibleEx
    {
        event MyMessage? MessageEvent;
        event XYHLDelegate? XYHLChanged;
        bool HabilitaArrastar { get; set; }
        bool Moving { get; set; }
        bool HabilitaDimensionar { get; set; }
        bool Resizing { get; set; }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsRuntime => (HabilitaDimensionar && Resizing) || (HabilitaArrastar && Moving);

        byte Alpha { get; set; }

        BorderStyle BorderStyle { get; set; }
        int BorderRadius { get; set; }
        EdgeEnum Edge { get; set; }

        ContextMenuStrip? ContextMenuStrip { get; set; }
        ToolStripMenuItem? meDimensionar { get; set; }
        ToolStripMenuItem? meMove { get; set; }
        IContainer? Components { get; set; }
        bool HasClickSubscribers { get; }
        int BorderWidth { get; set; }
        Rectangle AreaPath { get; set; }
        RectangleEdgeFilter BorderEdges { get; set; }

        void MenuDimensionarClick(object? sender, EventArgs e);
        void MenuMoveClick(object? sender, EventArgs e);
        void XYHLChangedInvoke(XYHL p);
    }
    public interface IUserControlXYHL : IControlXYHL, IEmUso { }

}