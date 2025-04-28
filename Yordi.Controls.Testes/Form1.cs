namespace Yordi.Controls.Testes
{
    public partial class Form1 : Form
    {
        private YTooltip tt;
        public Form1()
        {
            InitializeComponent();
            CurrentTooltipTheme.BackColor = Color.DarkBlue;
            CurrentTooltipTheme.ForeColor = Color.White;
            tt = new YTooltip(CurrentTooltipTheme.Theme());
            Shown += (s, e) => SetToolTip(lineControl1, "Teste de mensagens com troca de linha");
        }

        public void SetToolTip(Control? control, string? texto)
        {
            if (control == null || !control.IsHandleCreated || control.IsDisposed) return;
            tt ??= new YTooltip(CurrentTooltipTheme.Theme());
            if (control.InvokeRequired)
                control.Invoke(() => tt.SetToolTip(control, texto));
            else
                tt.SetToolTip(control, texto);
        }

    }
}
