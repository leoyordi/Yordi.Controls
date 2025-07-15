using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
                float progressVal = Convert.ToSingle(value);
                float percentage = (progressVal / 100.0f);

                using var backColorBrush = new SolidBrush(cellStyle.BackColor);
                using var foreColorBrush = new SolidBrush(cellStyle.ForeColor);

                base.Paint(g, clipBounds, cellBounds,
                 rowIndex, cellState, value, formattedValue, errorText,
                 cellStyle, advancedBorderStyle, (paintParts & ~DataGridViewPaintParts.ContentForeground));

                if (percentage > 0.0)
                {
                    var limit = Math.Min(percentage, 1.0f);
                    var barColor = GetColorForValue(progressVal);
                    using var barBrush = new SolidBrush(barColor);
                    g.FillRectangle(barBrush, cellBounds.X + 2, cellBounds.Y + 2, Convert.ToInt32((limit * cellBounds.Width - 4)), cellBounds.Height - 4);

                    // Desenhar o texto do botão
                    //g.DrawString(progressVal.ToString("0.##") + "%", cellStyle.Font, foreColorBrush, cellBounds.X + (cellBounds.Width / 2) - 5, cellBounds.Y + 2);
                    TextFormatFlags align = TextFormatFlags.WordBreak | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                    string txt;
                    if (!GetValueViewPattern())
                        txt = $"{progressVal.ToString("0.##")}%";
                    else
                        txt = $"{(100 - progressVal).ToString("0.##")}%";
                    TextRenderer.DrawText(g, txt, cellStyle.Font, cellBounds, cellStyle.ForeColor, align);
                }
                //else
                //{
                //if (this.DataGridView?.CurrentRow?.Index == rowIndex)
                //    g.DrawString(txt, cellStyle.Font, new SolidBrush(cellStyle.SelectionForeColor), cellBounds.X + 6, cellBounds.Y + 2);
                //else
                //    g.DrawString(txt, cellStyle.Font, foreColorBrush, cellBounds.X + 6, cellBounds.Y + 2);

                //}
            }
            catch (Exception) { }
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