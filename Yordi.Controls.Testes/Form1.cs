namespace Yordi.Controls.Testes
{
    public partial class Form1 : Form
    {
        private YTooltip tt;
        private System.Windows.Forms.Timer timer = new();
        public Form1()
        {
            InitializeComponent();
            CurrentTooltipTheme.BackColor = Color.DarkBlue;
            CurrentTooltipTheme.ForeColor = Color.White;
            LoadDataGridViewProgressColumn();
            tt = new YTooltip(CurrentTooltipTheme.Theme());
            Shown += (s, e) => SetToolTip(lineControl1, "Teste de mensagens com troca de linha");

            pb.Infinite = false;
            pb.ForeColor = Color.White;
            pb.ShowText = true;
            timer.Interval = 100;
            timer.Tick += TimerProgressBar_Tick;
            timer.Start();

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


        private void LoadDataGridViewProgressColumn()
        {
            var cores = new List<ProgressBarColorRange>()
                        {
                            new ProgressBarColorRange { Min = 0, Max = 90, Color = Color.Purple },
                            new ProgressBarColorRange { Min = 90, Max = 97, Color = Color.Yellow },
                            new ProgressBarColorRange { Min = 97.1f, Max = 103, Color = Color.FromArgb(100,221,23) },
                            new ProgressBarColorRange { Min = 103.1f, Max = 110, Color = Color.Yellow },
                            new ProgressBarColorRange { Min = 103.1f, Max = int.MaxValue, Color = Color.Red }
                        };
            DataGridViewProgressColumn column = new DataGridViewProgressColumn() { DefaultColorRanges = cores, DecrementViewerValue = false };

            dgv.ColumnCount = 2;
            dgv.Columns[0].Name = "Material";
            dgv.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv.Columns[1].Name = "Producao";
            dgv.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv.Columns.Add(column);
            dgv.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Desvio";


            object[] row1 = new object[] { "G1", "1234", 89 };
            object[] row2 = new object[] { "G2", "1234", 92 };
            object[] row3 = new object[] { "C", "1234", 102 };
            object[] row4 = new object[] { "A", "1234", 107 };
            object[] row5 = new object[] { "A", "1234", 112 };
            object[] rows = new object[] { row1, row2, row3, row4, row5 };

            foreach (object[] row in rows)
            {
                dgv.Rows.Add(row);
            }
        }

        private void chkDGVDecrement_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.Cells[row.Cells.Count - 1] is DataGridViewProgressCell cell)
                {
                    cell.DecrementViewerValue = chkDGVDecrement.Checked;
                    cell.ColorTextByContrast = chkDGVDecrement.Checked;
                }
            }
            dgv.Refresh();
        }



        int progress = 0;
        private void TimerProgressBar_Tick(object? sender, EventArgs e)
        {
            if (progress >= 120)
                progress = 0;
            else
                progress += 1;
            TesteProgressBar();
        }

        private void TesteProgressBar()
        {
            pb.Progress = progress;
            pb.Text = $"{progress}%";
            if (progress >= 97 && progress <= 103)
            {
                pb.ColorProgressPoint = Color.Green;
                pb.Text = "OK";
            }
            else if (progress >= 90 && progress <= 110)
            {
                pb.ColorProgressPoint = Color.Yellow;
                pb.Text = "Atenção";
            }
            else
            {
                pb.ColorProgressPoint = Color.Red;
                pb.Text = "Fora do padrão";
            }
            foreach (DataGridViewRow row in dgv.Rows)
            {
                row.Cells[2].Value = progress;
            }
        }

    }
}
