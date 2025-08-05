using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Yordi.Controls
{
    public delegate Task XYHLDelegate(XYHL positionSize);

    /// <summary>
    /// Define a posição e tamanho de um controle em um formulário ou área
    /// </summary>
    public class XYHL : IEquatable<XYHL?>
    {
        private DateTime created;
        public XYHL()
        {
            created = DateTime.Now;
        }
        [Key]
        /// <summary>
        /// Nome do formulário que esse controle faz parte
        /// </summary>
        public string? Form { get; set; }

        [Key]
        /// <summary>
        /// Nome do controle
        /// </summary>
        public string? Nome { get; set; }

        /// <summary>
        /// Controle Pai, caso necessário
        /// </summary>
        public string? ParentControl { get; set; }

        /// <summary>
        /// Posição X do controle
        /// </summary>
        public decimal X { get => x; set => x = value; }
        private decimal x;

        /// <summary>
        /// Posição Y do controle
        /// </summary>
        public decimal Y { get => y; set => y = value; }
        private decimal y;

        /// <summary>
        /// Altura do controle (Height)
        /// </summary>
        public decimal H { get => h; set => h = value; }
        private decimal h;

        /// <summary>
        /// Largura do controle (Width)
        /// </summary>
        public decimal L { get => l; set => l = value; }
        private decimal l;

        public int ReferenceWidth { get; set; } = 1920; // Ex: 1920
        public int ReferenceHeight { get; set; } = 1080; // Ex: 1080

        [BDIgnorar, JsonIgnore]
        public Size Size => new Size((int)L, (int)H);
        [BDIgnorar, JsonIgnore]
        public Point Location => new Point((int)X, (int)Y);

        public void CalculateHL(Screen actualScreen)
        {
            decimal currentWidth = actualScreen.Bounds.Width;
            decimal widthScale = currentWidth / ReferenceWidth;
            l *= widthScale;
            h *= widthScale;
        }
        public void CalculateXY(Screen actualScreen)
        {
            decimal currentWidth = actualScreen.Bounds.Width;
            decimal currentHeight = actualScreen.Bounds.Height;
            decimal widthScale = currentWidth / ReferenceWidth;
            decimal heightScale = currentHeight / ReferenceHeight;
            x *= widthScale;
            y *= heightScale;
        }
        public override string ToString()
        {
            return $"{Form}.{Nome}: {X}, {Y}, {H}, {L}";
        }
        public bool Equals(XYHL? other)
        {
            if (other == null) return false;
            return Equals(Nome, other.Nome) && Equals(other.Form, Form);
        }

        public XYHL Clone()
        {
            return new XYHL()
            {
                Form = Form,
                H = H,
                Y = Y,
                L = L,
                Nome = Nome,
                ParentControl = ParentControl,
                X = X,
            };
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as XYHL);
        }

        public override int GetHashCode()
        {
            return created.GetHashCode();
        }
    }
}
