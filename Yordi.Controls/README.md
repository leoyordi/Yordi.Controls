## Thanks to the following authors for their work:
 ### GraphicsExtension - [Extended Graphics]
 * Author name:           Arun Reginald Zaheeruddin
 * Current version:       1.0.0.4 (12b)
 * Release documentation: http://www.codeproject.com
 * License information:   Microsoft Public License (Ms-PL) [http://www.opensource.org/licenses/ms-pl.html]
 * Link:				  https://www.codeproject.com/Articles/38436/Extended-Graphics-Rounded-rectangles-Font-metrics


 ### RJ Code Advance - [RJ Code Advance]
 * Author name:          RJ Code Advance                    
 * Oficial site:         https://rjcodeadvance.com
 * Link:                 https://rjcodeadvance.com/cuadro-de-mensaje-personalizado-c-winforms/  
 * Github:               https://github.com/RJCodeAdvance/CustomMessageBox/tree/main
 * License:              Unlicense (https://unlicense.org)


# Yordi.Controls - [Yordi]
 * Author name:          Leopoldo Yordi
 * Github:               https://github.com/leoyordi/Yordi.Controls


## O que há?
	* Ferramentas para lidar com controles do Windows Forms.
	* Controle base (ControlXYHL) para lidar com arrasto e dimensionamento em tempo de execução. A escolha do que fazer está no menu de contexto.
	* Controle de usuário (UserControlXYHL) com os mesmos recursos de ControlXYHL, lida com arrasto e dimensionamento.
	* Limpeza ou reset de controles, passando por lista ou ControlCollection (Panel, Form, ...).
	* Habilita ou desabilita controles, passando por lista ou ControlCollection (Panel, Form, ...).
	* Tooltip personalizado, usando o tema passado para CurrentTooltipTheme.
	* Linha horizontal ou vertical, com animação de blink, gradient e circle.
	* DataGridViewProgressbarColumn - coluna de progress bar para DataGridView, com range de cores a ser definida pelo programador.


## Exemplo de uso
```csharp
    using Yordi.Controls;
        private void LoadDataGridViewProgressColumn()
        {
            var cores = new List<ProgressBarColorRange>()
                        {
                            new ProgressBarColorRange { Min = 0, Max = 90, Color = Color.Red },
                            new ProgressBarColorRange { Min = 90, Max = 97, Color = Color.Yellow },
                            new ProgressBarColorRange { Min = 97.1f, Max = 103, Color = Color.Green },
                            new ProgressBarColorRange { Min = 103.1f, Max = 110, Color = Color.Yellow },
                            new ProgressBarColorRange { Min = 103.1f, Max = int.MaxValue, Color = Color.Red }
                        };
            DataGridViewProgressColumn column = new DataGridViewProgressColumn(){ ColorRanges = cores, DecrementViewerValue = false };

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
                if (row.Cells[row.Cells.Count -1] is DataGridViewProgressCell cell)
                    cell.DecrementViewerValue = chkDGVDecrement.Checked;
            }
            dgv.Refresh();
        }

```


### Version History
* 1.1.4 - Acréscimo de utilização da propriedade ```BorderRadius``` em ```YProgressBar```.
* 1.1.3 - Acréscimo de propriedade interna em ```YProgressBar``` para aceitar o valor real do progresso, e não apenas o limite de valor máximo. 
    Isso permite que o controle seja usado como um indicador de progresso, e não apenas como uma barra de progresso.
    Além disso, foram adicionadas as propriedades públicas ```ShowText``` e ```ShowPercentage``` que definem o que deverá ser mostrado em ```YProgressBar```: texto ou valor real.
* 1.1.2 - Acréscimo de propriedade DecrementViewerValue para DataGridViewProgressbarColumn, que indica como será a visualização do valor na barra de progresso, 
    se por incremento ou por decremento (100 - valor).
* 1.1.1 - atualização da biblioteca Yordi.Tools para a versão 1.0.10. Mudança de Listagem de cores em DataGridViewProgressbarColumn, de estático e único para Default e individual por célula.
* 1.1.0 - acréscimo de DataGridViewProgressbarColumn.
* 1.0.4 - acréscimo de dois tipos de progress bar, 1 como barra e outro como círculo. 
Ambos com a propriedade Infinite, para os casos em que não se sabe o máximo. Roda como se fosse um gif.*
* 1.0.3 - atualização da biblioteca Yordi.Tools para a versão 1.0.9
** LineControl possui animação: blink, gradient e circle
** Problema conhecido: Tooltip não aceita BackColor e ForeColor