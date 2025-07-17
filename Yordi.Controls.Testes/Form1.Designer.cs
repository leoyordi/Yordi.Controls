namespace Yordi.Controls.Testes
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lineControl1 = new LineControl();
            pb = new YProgressBar();
            yRoundProgressBar1 = new YRoundProgressBar();
            dgv = new DataGridView();
            chkDGVDecrement = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)dgv).BeginInit();
            SuspendLayout();
            // 
            // lineControl1
            // 
            lineControl1.AlphaForDisabled = 50;
            lineControl1.Animate = true;
            lineControl1.AnimationInterval = 400;
            lineControl1.AnimationType = AnimationType.Gradient;
            lineControl1.BackColor = Color.Transparent;
            lineControl1.BorderEdges = RectangleEdgeFilter.All;
            lineControl1.BorderRadius = 0;
            lineControl1.BorderStyle = BorderStyle.None;
            lineControl1.BorderWidth = 0;
            lineControl1.HabilitaArrastar = false;
            lineControl1.HabilitaDimensionar = false;
            lineControl1.LineColor = Color.Black;
            lineControl1.Location = new Point(203, 82);
            lineControl1.Name = "lineControl1";
            lineControl1.Opacity = 100;
            lineControl1.Orientation = LineOrientation.Horizontal;
            lineControl1.Padding = new Padding(1);
            lineControl1.Size = new Size(362, 43);
            lineControl1.TabIndex = 0;
            lineControl1.Text = "lineControl1";
            // 
            // pb
            // 
            pb.AlphaForDisabled = 50;
            pb.AnimationInterval = 100;
            pb.BackColor = Color.Transparent;
            pb.BackgroundColor = Color.Transparent;
            pb.BarColor = Color.FromArgb(0, 41, 81);
            pb.BorderEdges = RectangleEdgeFilter.All;
            pb.BorderRadius = 5;
            pb.BorderStyle = BorderStyle.None;
            pb.BorderWidth = 0;
            pb.ColorProgressPoint = Color.Yellow;
            pb.HabilitaArrastar = false;
            pb.HabilitaDimensionar = false;
            pb.Infinite = true;
            pb.Location = new Point(89, 363);
            pb.Margin = new Padding(1);
            pb.Maximum = 100;
            pb.Name = "pb";
            pb.Opacity = 100;
            pb.Orientation = LineOrientation.Horizontal;
            pb.Padding = new Padding(1);
            pb.Progress = new decimal(new int[] { 10, 0, 0, 0 });
            pb.ProgressPointType = ProgressPointType.Bar;
            pb.ShowPercentage = true;
            pb.ShowText = false;
            pb.Size = new Size(255, 17);
            pb.TabIndex = 1;
            // 
            // yRoundProgressBar1
            // 
            yRoundProgressBar1.AlphaForDisabled = 50;
            yRoundProgressBar1.AnimationInterval = 100;
            yRoundProgressBar1.BackColor = Color.Transparent;
            yRoundProgressBar1.BackgroundColor = Color.Transparent;
            yRoundProgressBar1.BarColor = Color.Blue;
            yRoundProgressBar1.BorderEdges = RectangleEdgeFilter.None;
            yRoundProgressBar1.BorderRadius = 0;
            yRoundProgressBar1.BorderStyle = BorderStyle.None;
            yRoundProgressBar1.BorderWidth = 0;
            yRoundProgressBar1.HabilitaArrastar = false;
            yRoundProgressBar1.HabilitaDimensionar = false;
            yRoundProgressBar1.Infinite = true;
            yRoundProgressBar1.Location = new Point(361, 241);
            yRoundProgressBar1.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            yRoundProgressBar1.Name = "yRoundProgressBar1";
            yRoundProgressBar1.Opacity = 100;
            yRoundProgressBar1.Padding = new Padding(1);
            yRoundProgressBar1.Progress = new decimal(new int[] { 0, 0, 0, 0 });
            yRoundProgressBar1.Size = new Size(102, 103);
            yRoundProgressBar1.TabIndex = 2;
            yRoundProgressBar1.Text = "yRoundProgressBar1";
            // 
            // dgv
            // 
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.Location = new Point(597, 241);
            dgv.Name = "dgv";
            dgv.Size = new Size(645, 236);
            dgv.TabIndex = 3;
            // 
            // chkDGVDecrement
            // 
            chkDGVDecrement.AutoSize = true;
            chkDGVDecrement.Location = new Point(998, 188);
            chkDGVDecrement.Name = "chkDGVDecrement";
            chkDGVDecrement.Size = new Size(91, 19);
            chkDGVDecrement.TabIndex = 4;
            chkDGVDecrement.Text = "Decremento";
            chkDGVDecrement.UseVisualStyleBackColor = true;
            chkDGVDecrement.Click += chkDGVDecrement_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1272, 575);
            Controls.Add(chkDGVDecrement);
            Controls.Add(dgv);
            Controls.Add(yRoundProgressBar1);
            Controls.Add(pb);
            Controls.Add(lineControl1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)dgv).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private LineControl lineControl1;
        private YProgressBar pb;
        private YRoundProgressBar yRoundProgressBar1;
        private DataGridView dgv;
        private CheckBox chkDGVDecrement;
    }
}
