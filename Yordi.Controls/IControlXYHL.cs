using Yordi.Tools;

namespace Yordi.Controls
{
    public interface IControlXYHL : IVisibleEx
    {
        bool HabilitaArrastar { get; set; }
        bool HabilitaDimensionar { get; set; }
        bool IsRuntime { get; }

        byte Alpha { get; set; }

        BorderStyle BorderStyle { get; set; }
        int BorderRadius { get; set; }

        event MyMessage? MessageEvent;
        event XYHLDelegate? XYHLChanged;

        void SaveXYHL();
        void SetXYHL();
    }
    public interface IUserControlXYHL : IControlXYHL, IEmUso { }

}