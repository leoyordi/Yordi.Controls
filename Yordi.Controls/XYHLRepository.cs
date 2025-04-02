using Yordi.Tools;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Yordi.Controls
{
    /// <summary>
    /// Repositório para controles que implementem XYHL (posição e dimensão)
    /// </summary>
    public class XYHLRepository : FileRepository<IEnumerable<XYHL>>
    {
        private static List<XYHL>? _lista;
        private static XYHLRepository? instancia = null;
        private XYHLRepository(string file) : base(file)
        {
            _lista = base.Ler()?.ToList();
        }

        public static XYHLRepository Instancia()
        {
            if (instancia == null)
            {
                string path = FileTools.Combina(FileTools.ConfigFolder, "ControlesXYHL.json");
                instancia = new XYHLRepository(path);
            }            
            return instancia;
        }

        /// <summary>
        /// Salva a posição do controle
        /// </summary>
        /// <param name="xyhl"></param>
        /// <returns></returns>
        public bool Salvar(XYHL xyhl)
        {
            if (xyhl == null) return false;
            _lista ??= base.Ler()?.ToList() ?? new();
            if (_lista.Count == 0)
                _lista.Add(xyhl);
            else
            {
                var index = _lista.FindIndex(m => m.Equals(xyhl));
                if (index == -1)
                    _lista.Add(xyhl);
                else
                    _lista[index] = xyhl;
            }
            return base.Salvar(_lista);
        }

        /// <summary>
        /// Retorna a posição do controle, conforme seu nome e o nome do seu controle ancestral
        /// </summary>
        /// <param name="nome"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public XYHL? XYHL(string nome, string? form)
        {
            var finds = _lista?.FindAll(m => string.Equals(m.Nome, nome));
            if (finds == null) return null;
            if (finds.Count == 1)
                return finds[0].Clone();
            var r = finds.Find(m => string.Equals(form, m.Form));
            return r?.Clone();
        }

        public XYHL? XYHL(Control? control)
        {
            if (control == null) return null;
            Control? pai = control.Parent ?? control.FindForm();
            string? parentName = pai?.Name;
            XYHL? p;
            var finds = _lista?.FindAll(m => string.Equals(m.Nome, control.Name));
            if (finds == null) return null;
            if (finds.Count == 1)
                p = finds[0].Clone();
            else
            {
                var r = finds.Find(m => string.Equals(parentName, m.Form));
                p = r?.Clone();
            }
            if (p == null) return null;
            try
            {
                var actualScreen = Screen.FromControl(control);
                if (actualScreen != null)
                {
                    p.CalculateHL(actualScreen);
                    p.CalculateXY(actualScreen);
                }
            }
            catch { }
            if (p != null)
            {
                bool alteraPosicao = pai is not Form frm || frm.WindowState == FormWindowState.Maximized;
                if (pai != null && alteraPosicao)
                {
                    if (p.L > pai.Width)
                        p.L = pai.Width;
                    if (p.H > pai.Height)
                        p.H = pai.Height;
                    if (pai.Width < (p.X + p.L))
                        p.X = pai.Width - p.L;
                    if (pai.Height < (p.Y + p.H))
                        p.Y = pai.Height - p.H;
                }
            }
            return p;
        }

    }
}
