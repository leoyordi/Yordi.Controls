using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Yordi.Controls
{
    /// <summary>
    /// Represents a DataGridView column that displays progress bars in its cells.
    /// </summary>
    /// <remarks>
    /// This column is designed to work with <see cref="DataGridViewProgressCell"/>, which renders a
    /// progress bar in each cell. The progress value is typically represented as a percentage (0 to 100).
    /// </remarks>
    /// https://stackoverflow.com/questions/4646920/populating-a-datagridview-with-text-and-progressbars
    public class DataGridViewProgressColumn : DataGridViewImageColumn
    {
        /// <summary>
        /// Gets or sets the default collection of color ranges used by cells in this column.
        /// </summary>
        public List<ProgressBarColorRange> DefaultColorRanges { get; set; } = new()
        {
            new ProgressBarColorRange { Min = 0, Max = 90, Color = Color.GreenYellow },
            new ProgressBarColorRange { Min = 90, Max = 100, Color = Color.Orange },
            new ProgressBarColorRange { Min = 100, Max = 110, Color = Color.Red },
            new ProgressBarColorRange { Min = 110, Max = int.MaxValue, Color = Color.Purple }
        };
        public bool DecrementViewerValue { get; set; } = false;
        public bool ColorTextByContrast { get; set; } = false;

        public DataGridViewProgressColumn()
        {
            CellTemplate = new DataGridViewProgressCell();
        }
    }

    public class DataGridViewProgressCell : DataGridViewImageCell
    {
        /// <summary>
        /// Gets or sets the collection of color ranges used to determine the visual representation of the progress bar
        /// based on its value. If null, uses the column's DefaultColorRanges.
        /// </summary>
        public List<ProgressBarColorRange>? ColorRanges { get; set; }

        public bool DecrementViewerValue
        {
            get => _decrementViewerValue;
            set
            {
                _decrementViewerValue = value;
                if (this.OwningColumn is DataGridViewProgressColumn col)
                    col.DecrementViewerValue = value;
            }
        }
        private bool _decrementViewerValue = false;

        /// <summary>
        /// Gets or sets a value indicating whether text should be colored based on contrast.
        /// If true, text color will be adjusted based on the background color of the progress bar.
        /// If false, text will use the cell's foreground color.
        /// </summary>
        public bool ColorTextByContrast 
        { 
            get => _colorTextByContrast;
            set
            {
                _colorTextByContrast = value;
                if (this.OwningColumn is DataGridViewProgressColumn col)
                    col.ColorTextByContrast = value;
            }
        }
        private bool _colorTextByContrast = false;

        // Fallback padrão caso nem célula nem coluna tenham ranges definidos
        private static readonly List<ProgressBarColorRange> fallbackColorRanges = new()
        {
            new ProgressBarColorRange { Min = 0, Max = 90, Color = Color.GreenYellow },
            new ProgressBarColorRange { Min = 90, Max = 100, Color = Color.Orange },
            new ProgressBarColorRange { Min = 100, Max = 110, Color = Color.Red },
            new ProgressBarColorRange { Min = 110, Max = int.MaxValue, Color = Color.Purple }
        };

        // Used to make custom cell consistent with a DataGridViewImageCell
        static readonly Image emptyImage;
        static DataGridViewProgressCell()
        {
            emptyImage = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }
        public DataGridViewProgressCell()
        {
            this.ValueType = typeof(float);
        }

        /// <summary>
        /// Returns the effective color ranges for this cell, using the cell's own, the column's, or a fallback.
        /// </summary>
        private List<ProgressBarColorRange> GetEffectiveColorRanges()
        {
            if (ColorRanges != null)
                return ColorRanges;

            if (this.OwningColumn is DataGridViewProgressColumn col && col.DefaultColorRanges != null)
                return col.DefaultColorRanges;

            return fallbackColorRanges;
        }

        private Color GetColorForValue(float value)
        {
            var range = GetEffectiveColorRanges().FirstOrDefault(r => r.IsInRange(value));
            return range?.Color ?? Color.GreenYellow;
        }

        // Method required to make the Progress Cell consistent with the default Image Cell. 
        // The default Image Cell assumes an Image as a value, although the value of the Progress Cell is an int.
        protected override object GetFormattedValue(object value,
                                int rowIndex, ref DataGridViewCellStyle cellStyle,
                                TypeConverter valueTypeConverter,
                                TypeConverter formattedValueTypeConverter,
                                DataGridViewDataErrorContexts context)
        {
            return emptyImage;
        }
        private bool GetValueViewPattern()
        {
            if (OwningColumn is DataGridViewProgressColumn col)
            {
                return col.DecrementViewerValue;
            }
            return DecrementViewerValue;
        }

        protected override void Paint(Graphics g, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState,
            object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            if (value == null) return;
            try
            {
                bool isSelected = (cellState & DataGridViewElementStates.Selected) != 0;
                float progressVal = Convert.ToSingle(value);
                float percentage = (progressVal / 100.0f);

                using var backColorBrush = new SolidBrush(cellStyle.BackColor);

                base.Paint(g, clipBounds, cellBounds,
                 rowIndex, cellState, value, formattedValue, errorText,
                 cellStyle, advancedBorderStyle, (paintParts & ~DataGridViewPaintParts.ContentForeground));

                if (percentage > 0.0)
                {
                    var limit = Math.Min(percentage, 1.0f);
                    var widthValue = Convert.ToInt32((limit * cellBounds.Width - 4));
                    var barValue = new Rectangle(cellBounds.X + 2, cellBounds.Y + 2, widthValue, cellBounds.Height - 4);
                    var barColor = GetColorForValue(progressVal);
                    using var barBrush = new SolidBrush(barColor);
                    g.FillRectangle(barBrush, barValue);

                    string txt;
                    if (!GetValueViewPattern())
                        txt = $"{progressVal:0.##}%";
                    else
                        txt = $"{(100 - progressVal).ToString("0.##")}%";
                    
                    if (ColorTextByContrast)
                    {
                        var barDiff = new Rectangle(cellBounds.X + 2 + widthValue, cellBounds.Y + 2, cellBounds.Width - widthValue, cellBounds.Height - 4);
                        DrawText(g, cellStyle, cellBounds, barValue, barDiff, txt, rowIndex, barColor, isSelected);
                    }
                    else
                    {
                        TextFormatFlags align = TextFormatFlags.WordBreak | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                        TextRenderer.DrawText(g, txt, cellStyle.Font, cellBounds, cellStyle.ForeColor, align);
                    }
                }
            }
            catch (Exception) { }
        }

        private void DrawText(Graphics g, DataGridViewCellStyle cellStyle,
                Rectangle cellBounds, Rectangle barValue, Rectangle barDiff,
                string txt, int rowIndex, Color barColor, bool isSelected)
        {
            // Desenha texto sobre a área da barra de progresso
            Region oldClip = g.Clip;

            // Determina a cor de fundo efetiva (prioridade: célula, linha, grid)
            Color backColor = cellStyle.BackColor;
            if (this.DataGridView != null)
            {
                if (isSelected)
                    backColor = this.DataGridView.DefaultCellStyle.SelectionBackColor;
                else
                {
                    var row = this.DataGridView.Rows[rowIndex];
                    if (row.DefaultCellStyle.BackColor != Color.Empty)
                        backColor = row.DefaultCellStyle.BackColor;
                    else if (this.DataGridView.DefaultCellStyle.BackColor != Color.Empty)
                        backColor = this.DataGridView.DefaultCellStyle.BackColor;
                }
            }

            // Calcula a luminância para decidir a cor do texto
            Color textColorBackgound = GraphicsExtension.GetContrastingTextColor(backColor);
            Color textColorBar = GraphicsExtension.GetContrastingTextColor(barColor);

                        
            using (StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                // Desenha texto sobre a área do fundo
                g.SetClip(barDiff);
                using (var brushBg = new SolidBrush(textColorBackgound))
                    g.DrawString(txt, cellStyle.Font, brushBg, cellBounds, sf);

                // Desenha texto sobre a área da barra
                g.SetClip(barValue);
                using (var brushBar = new SolidBrush(textColorBar))
                    g.DrawString(txt, cellStyle.Font, brushBar, cellBounds, sf);


                // Restaura o clip original
                g.SetClip(oldClip, CombineMode.Replace);
            }
        }
    }

    public class ProgressBarColorRange
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public Color Color { get; set; }

        public bool IsInRange(float value) => value >= Min && value < Max;
    }
}