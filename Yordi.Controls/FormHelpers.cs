using Yordi.Tools;

namespace Yordi.Controls
{
    public class LimpaCampos
    {
        public static void Limpa(Control f, params Control[]? except)
        {
            if (f == null || !f.IsHandleCreated) return;
            if (f.Controls == null || f.Controls.Count == 0)
            {
                if (f is Panel || f is GroupBox || f is ContainerControl || f is SplitterPanel)
                { }
                else
                    LimpaControleIndividual(f, except);
            }
            else
            {
                foreach (Control ctrl in f.Controls)
                {
                    LimpaControleIndividual(ctrl, except);
                }
            }
        }

        private static void LimpaControleIndividual(Control ctrl, params Control[]? except)
        {
            if (except != null && except.Length > 0)
            {
                foreach (var item in except)
                {
                    if (item.Name.Equals(ctrl.Name))
                        return;
                }
            }

            if (ctrl is Panel)
                Limpa(ctrl);

            else if (ctrl is GroupBox)
                Limpa(ctrl);

            else if (ctrl is SplitContainer)
                Limpa(ctrl);

            else if (ctrl is ComboBox cmb)
            {
                int i = 0;
                while (cmb.SelectedIndex != -1 && i < 100)
                {
                    cmb.SelectedIndex = -1;
                    i++;
                }
            }
            else if (ctrl is ISelectedIndex selectedIndex)
            {
                int i = 0;
                while (selectedIndex.SelectedIndex != -1 && i < 100)
                {
                    selectedIndex.SelectedIndex = -1;
                    i++;
                }
            }
            else if (ctrl is TextBox txt)
                txt.Text = string.Empty;

            else if (ctrl is IText myTxt)
                myTxt.Text = string.Empty;

            else if (ctrl is MaskedTextBox msk)
                msk.Text = String.Empty;


            else if (ctrl is RadioButton rd)
                rd.Checked = false;

            else if (ctrl is CheckBox chk)
            {
                var b = chk.AutoCheck;
                chk.AutoCheck = true;
                chk.Checked = false;
                chk.AutoCheck = b;
            }
            else if (ctrl is ICheckBox myChk)
            {
                var b = myChk.AutoCheck;
                myChk.AutoCheck = true;
                myChk.Checked = false;
                myChk.AutoCheck = b;
            }

            else if (ctrl is DataGridView c)
            {
                c.Rows.Clear();
                c.DataSource = null;
                c.Invalidate();
            }

            else if (ctrl is DateTimePicker dt)
                dt.Value = DataPadrao.Brasilia;

            else if (ctrl is PictureBox pb)
                pb.Image = null;
            else if (ctrl is IImage img)
                img.Image = null;

            else if (ctrl is NumericUpDown ud)
                ud.Value = 0;

            else if (ctrl is UserControl)
                Limpa(ctrl);

        }
    }

    public class HabilitaCampos
    {
        public static void Habilita(Control f, bool status, string? tag, params Control[]? except)
        {
            string[]? tags = tag?.Split(';');
            if (f.Controls == null || f.Controls.Count == 0)
            {
                if (f is Panel || f is GroupBox || f is SplitContainer || f is TableLayoutPanel)
                { }
                else
                    HabilitaControleIndividual(status, f, tag, except);
            }
            else
            {
                foreach (Control ctrl in f.Controls)
                {
                    if (tags == null || tags.Length == 0)
                        HabilitaControleIndividual(status, ctrl, tag, except);
                    else
                    {
                        foreach (var t in tags)
                        {
                            HabilitaControleIndividual(status, ctrl, t, except);
                        }
                    }
                }
            }
        }

        private static void HabilitaControleIndividual(bool status, Control ctrl, string? t, params Control[]? except)
        {
            if (except != null && except.Length > 0)
            {
                foreach (var item in except)
                {
                    if (item.Name.Equals(ctrl.Name))
                        return;
                }
            }
            if (ctrl is Panel)
                Habilita(ctrl, status, t);

            else if (ctrl is GroupBox)
                Habilita(ctrl, status, t);

            else if (ctrl is SplitContainer)
                Habilita(ctrl, status, t);

            bool tagOk = false;
            if (string.IsNullOrEmpty(t))
                tagOk = true;
            else if ((ctrl.Tag != null) && true.Equals(ctrl.Tag.ToString()?.Contains(t)))
                tagOk = true;
            if (!tagOk)
                return;

            //if (string.IsNullOrEmpty(t) || (ctrl.Tag != null && ctrl.Tag.ToString().Contains(t)))
            //    return;

            if (ctrl is DateTimePicker dt)
                dt.Enabled = status;

            if (ctrl is TextBox txt)
                txt.ReadOnly = !status;
            else if (ctrl is MaskedTextBox box)
                box.Enabled = status;
            else if (ctrl is IText mytxt)
                mytxt.ReadOnly = !status;

            else if (ctrl is IEnabled myCmb)
                myCmb.Enabled = status;
            else if (ctrl is ComboBox cmb)
                cmb.Enabled = status;

            else if (ctrl is ListBox lst)
                lst.Enabled = status;

            else if (ctrl is Button button)
                button.Enabled = status;

            else if (ctrl is RadioButton rb)
                rb.Enabled = status;

            else if (ctrl is CheckBox chk)
                chk.AutoCheck = status;
            else if (ctrl is ICheckBox myChk)
                myChk.AutoCheck = status;

            else if (ctrl is PictureBox pb)
                pb.Enabled = status;

            else if (ctrl is UserControl userControl)
                userControl.Enabled = status;

            else if (ctrl is NumericUpDown nud)
                nud.Enabled = status;

        }

    }
    public static class AtualizaControlesIndexados
    {
        public static void AtualizaControles(this IObjectStringIndexer obj, Control.ControlCollection controles)
        {
            foreach (Control control in controles)
            {
                if (control.Tag == null) continue;
                var valor = DefineValor(obj, control);
                AtualizaControle(control, valor);
            }
        }
        public static void AtualizaControles(this IObjectStringIndexer obj, Control[] controles)
        {
            foreach (Control control in controles)
            {
                if (control.Tag == null) continue;
                var valor = DefineValor(obj, control);
                AtualizaControle(control, valor);
            }
        }

        private static void AtualizaControle(Control control, object? valor)
        {
            if (control is Label lbl)
            { }
            else if (control is TextBox || control is IText)
                control.Text = valor?.ToString();
            else if (control is ComboBox cmb)
            {
                if (valor == null)
                    cmb.SelectedIndex = -1;
                else
                    cmb.SelectedValue = valor;
            }
            else if (control is ISelectedIndex ind)
            {
                if (valor == null)
                    ind.SelectedIndex = -1;
                else
                    ind.SelectedValue = valor;
            }

            else if (control is ICheckBox chk)
            {
                if (valor == null)
                    chk.Checked = false;
                else
                    chk.Checked = Conversores.ToBool(valor);
            }
            else if (control is CheckBox my)
            {
                if (valor == null)
                    my.Checked = false;
                else
                    my.Checked = Conversores.ToBool(valor);
            }
            else if (control is DateTimePicker d)
            {
                if (valor != null && valor is DateTime dt)
                    d.Value = dt;
                else
                    d.Value = DateTime.Now;
            }
        }
        private static object? DefineValor(IObjectStringIndexer obj, Control control)
        {
            var tag = control.Tag == null ? null : control.Tag?.ToString();
            if (string.IsNullOrEmpty(tag)) return null;
            object? s = null;
            var split = tag.ToString().Split('.');
            if (split.Length == 1) // Nome da propriedade, sem extensão
            {
                dynamic? prop = obj[tag];
                //if (prop is CLPPosicao pos)
                //    s = pos.Variavel; // nesses casos, mostrar a variável (MW??? ou M???)
                //else
                if (prop is Enum enumOk)
                    s = Convert.ToInt32(enumOk);
                else
                    s = prop;
            }
            //Nome da propriedade + extensão. Normalmente é um objeto CLPPosicao, para mostrar a subpropriedade Valor. 
            // Nesses casos, usar o valor
            else if (split.Length == 2 && split[1] == "valor")
            {
                //if (obj[split[0]] is CLPPosicao pos)
                //    s = pos.Valor;
                //else
                    s = obj[tag];
            }
            return s;
        }
    }
}
