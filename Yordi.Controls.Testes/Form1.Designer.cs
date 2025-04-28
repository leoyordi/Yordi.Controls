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
            lineControl1.components = null;
            lineControl1.HabilitaArrastar = false;
            lineControl1.HabilitaDimensionar = false;
            lineControl1.LineColor = Color.Black;
            lineControl1.Location = new Point(197, 143);
            lineControl1.Name = "lineControl1";
            lineControl1.Opacity = 100;
            lineControl1.Orientation = LineOrientation.Horizontal;
            lineControl1.Padding = new Padding(1);
            lineControl1.Size = new Size(362, 43);
            lineControl1.TabIndex = 0;
            lineControl1.Text = "lineControl1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lineControl1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private LineControl lineControl1;
    }
}
