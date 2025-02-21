using Yordi.Tools;

namespace Yordi.Controls
{
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

        public XYHL? XYHL(string nome, string? form)
        {
            var finds = _lista?.FindAll(m => string.Equals(m.Nome, nome));
            if (finds == null) return null;
            if (finds.Count == 1)
                return finds[0].Clone();
            var r = finds.Find(m => string.Equals(form, m.Form));
            return r?.Clone();
        }
    }
}
