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
        /// Define o cursor do controle quando estiver arrastando ou dimensionando.
        /// Para o controle LineControl, usar método específico para tal.
        /// </summary>
        /// <param name="control">Controle que implementa a interface IControlXYHL</param>
        /// <param name="e">Argumentos do evento do mouse</param>
        public static void DefinirCursorEx<T>(this T control, MouseEventArgs e) where T : Control, IControlXYHL
        {
            if (!control.IsRuntime)
            {
                control.Cursor = Cursors.Default;
                control.Edge = EdgeEnum.None;
                return;
            }
            if (control.Moving)
            {
                control.Cursor = Cursors.SizeAll;
                control.Edge = EdgeEnum.TopLeft;
                return;
            }
            if (control.Resizing)
            {
                //top left corner
                if (e.X <= (control.Padding.Horizontal) & e.Y <= (control.Padding.Vertical))
                {
                    control.Cursor = Cursors.SizeAll;
                    control.Edge = EdgeEnum.TopLeft;
                }
                //bottom right corner
                else if ((e.X >= (control.Width - (control.Padding.Horizontal + 1))) & (e.Y >= control.Height - (control.Padding.Vertical + 1)))
                {
                    control.Cursor = Cursors.SizeNWSE;
                    control.Edge = EdgeEnum.BottomRight;
                }
                //left edge
                else if (e.X <= control.Padding.Horizontal)
                {
                    control.Cursor = Cursors.VSplit;
                    control.Edge = EdgeEnum.Left;
                }
                //right edge
                else if (e.X > (control.Width - (control.Padding.Horizontal + 1)))
                {
                    control.Cursor = Cursors.VSplit;
                    control.Edge = EdgeEnum.Right;
                }
                //top edge
                else if (e.Y <= control.Padding.Vertical)
                {
                    control.Cursor = Cursors.HSplit;
                    control.Edge = EdgeEnum.Top;
                }
                //bottom edge
                else if (e.Y > control.Height - (control.Padding.Vertical + 1))
                {
                    control.Cursor = Cursors.HSplit;
                    control.Edge = EdgeEnum.Bottom;
                }
                //no edge
                else
                {
                    control.Cursor = Cursors.Default;
                    control.Edge = EdgeEnum.None;
                }
            }
        }


        public static void RedimensionarControleEx<T>(this T control, MouseEventArgs e) where T : Control, IControlXYHL
        {
            switch (control.Edge)
            {
                case EdgeEnum.TopLeft:
                    control.SetBounds(control.Left + e.X, control.Top + e.Y, control.Width, control.Height);
                    break;
                case EdgeEnum.Left:
                    control.SetBounds(control.Left + e.X, control.Top, control.Width - e.X, control.Height);
                    break;
                case EdgeEnum.Right:
                    control.SetBounds(control.Left, control.Top, control.Width - (control.Width - e.X), control.Height);
                    break;
                case EdgeEnum.Top:
                    control.SetBounds(control.Left, control.Top + e.Y, control.Width, control.Height - e.Y);
                    break;
                case EdgeEnum.Bottom:
                    control.SetBounds(control.Left, control.Top, control.Width, control.Height - (control.Height - e.Y));
                    break;
                case EdgeEnum.BottomRight:
                    control.SetBounds(control.Left, control.Top, control.Width - (control.Width - e.X), control.Height - (control.Height - e.Y));
                    break;
            }
            control.ResumeLayout();
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

        public static void BordaTracejada<T>(this T control, PaintEventArgs e) where T : Control, IControlXYHL
        {
            if (control.IsRuntime)
            {
                using (Pen dashedPen = new Pen(CurrentTheme.DraggingBorderColor, 2.0F))
                {
                    dashedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawRectangle(dashedPen, 0, 0, control.Width - 1, control.Height - 1);
                }
            }
        }

        public static void CalculateAreaPath<T>(this T control) where T : Control, IControlXYHL
        {
            if (control.BorderRadius > 0 || control.BorderWidth > 0)
            {
                int x = control.BorderWidth, y = control.BorderWidth, l = control.Width - (control.BorderWidth * 2), h = control.Height - (control.BorderWidth * 2);
                if (control.AreaPath.X != x || control.AreaPath.Y != y || control.AreaPath.Width != l || control.AreaPath.Height != h)
                    control.AreaPath = new Rectangle(x, y, l, h);
            }
            else
                control.AreaPath = control.ClientRectangle;
        }

        #region Localização XYHL
        public static void SetLocation<T>(this T control) where T : Control, IControlXYHL
        {
            if (control == null || control.IsDisposed || !control.IsHandleCreated) return;
            var position = FindXYHL(control);
            if (control.InvokeRequired)
                control.Invoke(() => SetLocation_NTS(control, position));
            else
                SetLocation_NTS(control, position);
        }
        public static void SetLocation<T>(this T control, XYHL position) where T : Control, IControlXYHL
        {
            if (control == null || control.IsDisposed || !control.IsHandleCreated) return;
            if (control.InvokeRequired)
                control.Invoke(() => SetLocation_NTS(control, position));
            else
                SetLocation_NTS(control, position);
        }
        private static void SetLocation_NTS<T>(T control, XYHL p) where T : Control, IControlXYHL
        {
            if (p.X < 0) p.X = 0;
            if (p.Y < 0) p.Y = 0;
            if (control.Parent != null)
            {
                if (control.Parent is not Form frm || frm.WindowState == FormWindowState.Maximized)
                {
                    if (control.Parent.Height < p.Y + p.H + control.Padding.Vertical)
                        p.Y = (int)((control.Parent.Height) * 0.88m);
                    if (control.Parent.Width < p.X + p.L + control.Padding.Horizontal)
                        p.X = (int)(control.Parent.Width * 0.88m);
                }
            }
            bool invalidate = false;
            if (control.HabilitaDimensionar)
            {
                if (control.Size.Height != p.H || control.Size.Width != p.L)
                {
                    control.Size = p.Size;
                    invalidate = true;
                }
            }
            if (control.Location.X != p.X || control.Location.Y != p.Y)
            {
                control.Location = p.Location;
                invalidate = true;
            }
            if (invalidate) control.Invalidate();
        }

        public static XYHL FindXYHL<T>(this T control) where T : Control, IControlXYHL
        {
            var repo = XYHLRepository.Instancia();
            XYHL? p = repo?.XYHL(control) ?? XYHLNew(control);
            return p;
        }

        public static XYHL? FindTrulyXYHL<T>(this T control) where T : Control, IControlXYHL
        {
            var repo = XYHLRepository.Instancia();
            return repo?.XYHL(control);
        }

        public static XYHL XYHLNew(Control c)
        {
            Screen screen = Screen.FromControl(c);
            var p = new XYHL()
            {
                H = c.Size.Height,
                X = c.Location.X,
                L = c.Size.Width,
                Y = c.Location.Y,
                ParentControl = ParentName(c),
                Nome = c.Name,
            };
            p.CalculateHL(screen);
            p.CalculateXY(screen);
            return p;
        }
        public static XYHL? SaveXYHL<T>(this T control) where T : Control, IControlXYHL
        {
            var repo = XYHLRepository.Instancia;
            if (repo == null) return null;
            if (control.Height <= control.Padding.Vertical)
                control.Height = control.Padding.Vertical;
            if (control.Width <= control.Padding.Horizontal)
                control.Width = control.Padding.Horizontal;
            int x = control.Location.X, y = control.Location.Y;
            if (x < 0) x = 1;
            if (y < 0) y = 1;
            if (control.Location.X != x || control.Location.Y != y)
                control.Location = new Point(x, y);
            control.Invalidate();
            string? name = ParentName(control);
            Screen reference = Screen.FromControl(control);
            XYHL p = new XYHL
            {
                H = control.Height,
                L = control.Width,
                Nome = control.Name,
                X = control.Location.X,
                Y = control.Location.Y,
                Form = name,
                ReferenceHeight = reference.Bounds.Height,
                ReferenceWidth = reference.Bounds.Width
            };
            Task.Run(() =>
            {
                WritePositionSize(p);
                if (p != null)
                {
                    control.Edge = EdgeEnum.None;
                    control.XYHLChangedInvoke(p);
                }
            });
            return p;
        }
        private static string? ParentName(Control c)
        {
            if (c.Parent != null)
                return c.Parent.Name;
            var parent = c.FindForm();
            if (parent != null)
                return parent.Name;
            return null;
        }
        private static bool salvando;

        private static Task WritePositionSize(XYHL positionSize)
        {
            var repo = XYHLRepository.Instancia();
            if (repo == null) return Task.CompletedTask;
            if (positionSize.X < 0) positionSize.X = 1;
            if (positionSize.Y < 0) positionSize.Y = 1;
            if (positionSize.L < 2) positionSize.L = 2;
            if (positionSize.H < 2) positionSize.H = 2;

            if (!salvando)
            {
                salvando = true;
                _ = repo.Salvar(positionSize);
                salvando = false;
            }
            return Task.CompletedTask;
        }

        #endregion

        #region Clicks do menu
        public static void MenuMoveClick<T>(this T control) where T : Control, IControlXYHL
        {
            control.Moving = !control.Moving;
            if (control.meMove != null && !control.meMove.IsDisposed)
                control.meMove.Checked = control.Moving;
            if (!control.Moving) // se terminou de ajustar
            {
                control.SaveXYHL();
                control.TabStop = false;
            }
            else
            {
                control.TabStop = true;
                control.Focus();
            }
            control.Resizing = false;
            if (control.meDimensionar != null && !control.meDimensionar.IsDisposed)
                control.meDimensionar.Checked = control.Resizing;
            control.Invalidate();
        }
        public static void MenuDimensionarClick<T>(this T control) where T : Control, IControlXYHL
        {
            control.Resizing = !control.Resizing;
            if (control.meDimensionar != null && !control.meDimensionar.IsDisposed)
                control.meDimensionar.Checked = control.Resizing;
            if (!control.Resizing) // se terminou de ajustar
            {
                control.SaveXYHL();
                control.TabStop = false;
            }
            else
            {
                control.TabStop = true;
                control.Focus();
            }
            control.Moving = false;
            if (control.meMove != null && !control.meMove.IsDisposed)
                control.meMove.Checked = control.Moving;
            control.Invalidate();
        }

        #endregion
    }
}
