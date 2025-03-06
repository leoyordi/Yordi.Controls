using System.ComponentModel;

namespace Yordi.Controls
{
    /// <summary>
    /// Extensões para controles
    /// </summary>
    public static class ControlExtensions
    {
        /// <summary>
        /// Trunca um número e define o texto do controle. <br/>
        /// <b>Esse método não é thread safe. A propriedade Text do controle deve ser acessada na thread de UI.</b>
        /// </summary>
        /// <param name="control">Controle que implementa a interface IText</param>
        /// <param name="valor">Valor a ser truncado</param>
        /// <param name="limit">Limite de caracteres</param>
        public static void TruncaNumero(this IText control, decimal valor, int limit = 5)
        {
            if (valor == 0)
                control.Text = "0";
            else
            {
                string s = valor.ToString();
                if (limit > 0 && s.Length > limit)
                    control.Text = s[..(limit + 1)];
                else
                    control.Text = s;
            }
        }

        /// <summary>
        /// Define o cursor do controle quando estiver arrastando ou dimensionando
        /// </summary>
        /// <param name="xyhl">Controle que implementa a interface IControlXYHL</param>
        /// <param name="e">Argumentos do evento do mouse</param>
        public static void DefinirCursor(this IControlXYHL xyhl, MouseEventArgs e)
        {
            if (xyhl is not Control c) return;
            if (!xyhl.IsRuntime)
            {
                c.Cursor = Cursors.Default;
                xyhl.Edge = EdgeEnum.None;
                return;
            }
            if (xyhl.Moving)
            {
                c.Cursor = Cursors.SizeAll;
                xyhl.Edge = EdgeEnum.TopLeft;
                return;
            }
            if (xyhl.Resizing)
            {
                //top left corner
                if (e.X <= (c.Padding.Horizontal) & e.Y <= (c.Padding.Vertical))
                {
                    c.Cursor = Cursors.SizeAll;
                    xyhl.Edge = EdgeEnum.TopLeft;
                }
                //bottom right corner
                else if ((e.X >= (c.Width - (c.Padding.Horizontal + 1))) & (e.Y >= c.Height - (c.Padding.Vertical + 1)))
                {
                    c.Cursor = Cursors.SizeNWSE;
                    xyhl.Edge = EdgeEnum.BottomRight;
                }
                //left edge
                else if (e.X <= c.Padding.Horizontal)
                {
                    c.Cursor = Cursors.VSplit;
                    xyhl.Edge = EdgeEnum.Left;
                }
                //right edge
                else if (e.X > (c.Width - (c.Padding.Horizontal + 1)))
                {
                    c.Cursor = Cursors.VSplit;
                    xyhl.Edge = EdgeEnum.Right;
                }
                //top edge
                else if (e.Y <= c.Padding.Vertical)
                {
                    c.Cursor = Cursors.HSplit;
                    xyhl.Edge = EdgeEnum.Top;
                }
                //bottom edge
                else if (e.Y > c.Height - (c.Padding.Vertical + 1))
                {
                    c.Cursor = Cursors.HSplit;
                    xyhl.Edge = EdgeEnum.Bottom;
                }
                //no edge
                else
                {
                    c.Cursor = Cursors.Default;
                    xyhl.Edge = EdgeEnum.None;
                }
            }
        }

        /// <summary>
        /// Verifica e atualiza o menu de contexto do controle para arrasto e dimensionamento, <br/>
        /// de acordo com as propriedades HabilitaArrastar e HabilitaDimensionar
        /// </summary>
        /// <param name="control">Controle que implementa a interface IControlXYHL</param>
        public static void ContextMenuVerifyEx(this IControlXYHL control)
        {
            if (control is not Control c) return;
            if ((!control.HabilitaArrastar && !control.HabilitaDimensionar) || !c.Visible)
                c.ContextMenuStrip = null;
            else if (c.ContextMenuStrip == null)
            {
                if (control.Components == null)
                    control.Components = new Container();
                c.ContextMenuStrip = new ContextMenuStrip(control.Components)
                {
                    BackColor = CurrentContextMenuTheme.BackColor,
                    Font = CurrentContextMenuTheme.Font,
                    ForeColor = CurrentContextMenuTheme.ForeColor,
                    Name = "menu",
                    ShowCheckMargin = true,
                    ShowImageMargin = false,
                    Size = new Size(258, 52),
                    Text = "Alterações",
                };
            }
            VerifyDimensionarMenu(control);
            VerifyMoveMenu(control);
        }

        /// <summary>
        /// Verifica e atualiza a opção de dimensionar no menu de contexto
        /// </summary>
        /// <param name="control">Controle que implementa a interface IControlXYHL</param>
        private static void VerifyDimensionarMenu(this IControlXYHL control)
        {
            if (control is not Control c) return;
            if (c.ContextMenuStrip == null || !control.HabilitaDimensionar)
            {
                if (control.meDimensionar != null)
                {
                    if (false.Equals(c.ContextMenuStrip?.Items.IsReadOnly))
                    {
                        if (c.ContextMenuStrip.Items.Contains(control.meDimensionar))
                            c.ContextMenuStrip.Items.Remove(control.meDimensionar);
                    }
                    control.meDimensionar.Click -= new EventHandler(control.MenuDimensionarClick);
                    control.meDimensionar.Dispose();
                }
            }
            else
            {
                if (control.meDimensionar == null)
                {
                    control.meDimensionar = new ToolStripMenuItem();
                    control.meDimensionar.CheckOnClick = true;
                    control.meDimensionar.Name = "meDimensionar";
                    control.meDimensionar.Size = new Size(208, 22);
                    control.meDimensionar.Text = "Dimensionar";
                    control.meDimensionar.Font = CurrentContextMenuTheme.Font;
                    control.meDimensionar.ForeColor = CurrentContextMenuTheme.ForeColor;
                }
                control.meDimensionar.Click -= new EventHandler(control.MenuDimensionarClick);
                control.meDimensionar.Click += new EventHandler(control.MenuDimensionarClick);
                if (false.Equals(c.ContextMenuStrip?.Items.IsReadOnly))
                {
                    if (!c.ContextMenuStrip.Items.Contains(control.meDimensionar))
                        c.ContextMenuStrip.Items.Add(control.meDimensionar);
                }
            }
        }

        /// <summary>
        /// Verifica e atualiza a opção de mover no menu de contexto
        /// </summary>
        /// <param name="control">Controle que implementa a interface IControlXYHL</param>
        private static void VerifyMoveMenu(this IControlXYHL control)
        {
            if (control is not Control c) return;
            if (c.ContextMenuStrip == null || !control.HabilitaArrastar)
            {
                if (control.meMove != null)
                {
                    if (false.Equals(c.ContextMenuStrip?.Items.IsReadOnly))
                    {
                        if (c.ContextMenuStrip.Items.Contains(control.meMove))
                            c.ContextMenuStrip.Items.Remove(control.meMove);
                    }
                    control.meMove.Click -= new EventHandler(control.MenuMoveClick);
                    control.meMove.Dispose();
                    control.meMove = null;
                }
            }
            else
            {
                if (control.meMove == null)
                {
                    control.meMove = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = "meMove",
                        Size = new Size(208, 22),
                        Text = "Mover",
                        Font = CurrentContextMenuTheme.Font,
                        ForeColor = CurrentContextMenuTheme.ForeColor
                    };
                }
                control.meMove.Click -= new EventHandler(control.MenuMoveClick);
                control.meMove.Click += new EventHandler(control.MenuMoveClick);
                if (false.Equals(c.ContextMenuStrip?.Items.IsReadOnly))
                {
                    if (!c.ContextMenuStrip.Items.Contains(control.meMove))
                        c.ContextMenuStrip.Items.Add(control.meMove);
                }
            }
        }

        /// <summary>
        /// Evento de mouse up para controles que implementam a interface IControlXYHL.<br/>
        /// Imita o soltar de um botão se esse controle possuir assinantes de algum evento click
        /// </summary>
        /// <param name="control">Controle que implementa a interface IControlXYHL</param>
        /// <param name="e">Argumentos do evento do mouse</param>
        public static void OnMouseUpEx(this IControlXYHL control, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right || control.IsRuntime) return;
            if (!control.HasClickSubscribers) return;
            if (control is IEmUso emUso && !emUso.EmUso) return;
            if (!control.IsRuntime)
            {
                if (control.Alpha != 255)
                    control.BorderStyle = BorderStyle.None;
            }
            else if (control is PictureBox pb)
                pb.BorderStyle = BorderStyle.None;
            else if (control is Panel pnl)
                pnl.BorderStyle = BorderStyle.None;
        }

        /// <summary>
        /// Evento de mouse down para controles que implementam a interface IControlXYHL.<br/>
        /// Imita o pressionar de um botão se esse controle possuir assinantes de algum evento click
        /// </summary>
        /// <param name="control">Controle que implementa a interface IControlXYHL</param>
        /// <param name="e">Argumentos do evento do mouse</param>
        public static void OnMouseDownEx(this IControlXYHL control, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right || control.IsRuntime) return;
            if (!control.HasClickSubscribers) return;
            if (control is IEmUso emUso && !emUso.EmUso) return;
            if (!control.IsRuntime)
            {
                if (control.Alpha != 255) control.BorderStyle = BorderStyle.Fixed3D;
            }
            else if (control is PictureBox pb)
                pb.BorderStyle = BorderStyle.Fixed3D;
            else if (control is Panel pnl)
                pnl.BorderStyle = BorderStyle.Fixed3D;
        }
    }
}
