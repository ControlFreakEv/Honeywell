using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using Honeywell.HMIWeb;
using System.Diagnostics;

namespace Honeywell.GUI.ExperionGfx
{
    public partial class GfxRefForm : Form
    {
        public GfxRefForm()
        {
            InitializeComponent();
        }

        private void GoButton_Click(object sender, EventArgs e)
        {
            ScanDisplays();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            ExportToXlsx();
        }

        private string GetXlsxPath() => $@"{GfxPath.Text}\Associated Displays.xlsx";

        private void ExportToXlsx()
        {
            if (AssociatedDisplayDataGrid.Rows.Count == 0 || AssociatedDisplayDataGrid.DataSource is not DataTable table)
            {
                MessageBox.Show("Nothing to Export. Click 'Go' first.");
                return;
            }

            string filePath = GetXlsxPath();
            try
            {
                ShapeWorkbook.Export(table, GetDataTableMapping(), filePath);
                MessageBox.Show($"File saved at \n{filePath}");
            }
            catch (IOException)
            {
                MessageBox.Show($"Close {filePath} and try again.");
            }
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            ImportXlxs();
        }

        private void ImportXlxs()
        {
            string filePath = GetXlsxPath();
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"{filePath} does not exist");
                return;
            }

            try
            {
                var shapes = ShapeWorkbook.Import(filePath, GetDataTableMapping(), header =>
                    MessageBox.Show($"Could not find header {header}", "Warning: Missing Column", MessageBoxButtons.OKCancel) != DialogResult.Cancel);
                if (shapes != null)
                    PopulateDataGrid(shapes);
            }
            catch (IOException)
            {
                MessageBox.Show($"Close {filePath} and try again.");
            }
            catch (InvalidDataException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PopulateDataGrid(IEnumerable<Shape> shapeList)
        {
            AssociatedDisplayDataGrid.Columns.Clear();
            AssociatedDisplayDataGrid.DataSource = GetDataTable(shapeList);
        }

        private List<Shape> GetShapesFromDataGrid()
        {
            if (AssociatedDisplayDataGrid.DataSource is not DataTable table)
                return new List<Shape>();

            return table.AsEnumerable()
                .Select(ShapeFromRow)
                .Where(shape => shape.ReplaceShapeName != ReplaceShapeColumn.HeaderText)
                .ToList();
        }

        private Shape ShapeFromRow(DataRow row)
        {
            int.TryParse(row[CsvIndexColumn.Index]?.ToStringOrNull(), out int csvIndex);
            int.TryParse(row[UniqueShapeIdColumn.Index]?.ToStringOrNull(), out int uniqueShapeIndex);

            return new Shape
            {
                UniqueShapeId = uniqueShapeIndex,
                Graphic = row[GraphicColumn.Index]?.ToStringOrNull(),
                ShapeSource = row[ShapeSourceColumn.Index]?.ToStringOrNull(),
                ShapeName = row[ShapeNameColumn.Index]?.ToStringOrNull(),
                ShapePath = row[ShapePathColumn.Index]?.ToStringOrNull(),
                CustomParamPointName = row[CustomParameterPointNameColumn.Index]?.ToStringOrNull(),
                CustomParamParameterName = row[CustomParameterParamNameColumn.Index]?.ToStringOrNull(),
                CustomPropMiscName = row[CustomPropMiscNameColumn.Index]?.ToStringOrNull(),
                Point = row[PointColumn.Index]?.ToStringOrNull(),
                Parameter = row[ParameterColumn.Index]?.ToStringOrNull(),
                MiscPropValue = row[CustomPropMiscValueColumn.Index]?.ToStringOrNull(),
                Tooltip = row[ToolTipColumn.Index]?.ToStringOrNull(),
                Css = row[CssColumn.Index]?.ToStringOrNull(),
                DataType = Shape.ParseObjectType(row[TypeColumn.Index]?.ToStringOrNull()),
                NewPoint = row[NewPointColumn.Index]?.ToStringOrNull(),
                NewParameter = row[NewParameterColumn.Index]?.ToStringOrNull(),
                NewMiscPropValue = row[ReplaceMiscPropColumn.Index]?.ToStringOrNull(),
                NewTooltip = row[NewToolTipColumn.Index]?.ToStringOrNull(),
                NewCss = row[NewCssColumn.Index]?.ToStringOrNull(),
                ReplaceShapeName = row[ReplaceShapeColumn.Index]?.ToStringOrNull(),
                CustomParamType = Shape.ParseObjectType(row[CustomParameterTypeColumn.Index]?.ToStringOrNull()),
                GfxAndShape = row[GfxShapeColumn.Index]?.ToStringOrNull(),
                DataObjectId = row[DataObjectIdColumn.Index]?.ToStringOrNull(),
                BindingId = row[BindingIdColumn.Index]?.ToStringOrNull(),
                CsvIndex = csvIndex,
            };
        }

        private void ScanDisplays()
        {
            if (!Directory.Exists(GfxPath.Text))
            {
                MessageBox.Show("Invalid Path");
                return;
            }

            ShowingProblems(() =>
            {
                var scan = DisplayScanner.Scan(GfxPath.Text);
                if (scan.Problems.Count > 0)
                    MessageBox.Show(string.Join("\n", scan.Problems));
                PopulateDataGrid(scan.Shapes);
            });
        }

        private void ReplaceButton_Click(object sender, EventArgs e)
        {
            ShowingProblems(() =>
            {
                DisplayReplacer.ReplaceReferences(GfxPath.Text, GetShapesFromDataGrid(), GhostRefCheckBox.Checked);
                MessageBox.Show($"Done Replacing. Files saved at \n {GfxPath.Text}");
            });
        }

        private void ReplaceShapeButton_Click(object sender, EventArgs e)
        {
            ShowingProblems(() =>
            {
                DisplayReplacer.SwapShapes(GfxPath.Text, ShapeReplacementTextBox.Text, GetShapesFromDataGrid(), ResizeCheckBox.Checked);
                MessageBox.Show("Shapes have been replaced");
            });
        }

        /// <summary>Runs display work, telling the user what went wrong instead of crashing.</summary>
        private static void ShowingProblems(Action work)
        {
            try
            {
                work();
            }
            catch (DisplayEditException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (AggregateException ex)
            {
                MessageBox.Show(string.Join("\n", ex.Flatten().InnerExceptions.Select(x => x.Message).Distinct()));
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public DataTable GetDataTable(IEnumerable<Shape> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(Shape));
            DataTable table = new DataTable();

            var dict = GetDataTableMapping();
            foreach (KeyValuePair<string, string> item in dict)
            {
                PropertyDescriptor? prop = properties[item.Key];
                var value = item.Value.ToString();
                table.Columns.Add(value);
            }

            foreach (var item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[dict[prop.Name]] = prop.GetValue(item) ?? DBNull.Value;

                table.Rows.Add(row);
            }
            return table;
        }

        /// <summary>
        /// Shape property is key, column header is value
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetDataTableMapping()
        {
            var dict = new Dictionary<string, string>
            {
                { nameof(Shape.UniqueShapeId), UniqueShapeIdColumn.HeaderText },
                { nameof(Shape.Graphic), GraphicColumn.HeaderText },
                { nameof(Shape.ShapeSource), ShapeSourceColumn.HeaderText },
                { nameof(Shape.ShapeName), ShapeNameColumn.HeaderText },
                { nameof(Shape.ShapePath), ShapePathColumn.HeaderText },
                { nameof(Shape.CustomParamPointName), CustomParameterPointNameColumn.HeaderText },
                { nameof(Shape.CustomParamParameterName), CustomParameterParamNameColumn.HeaderText },
                { nameof(Shape.CustomPropMiscName), CustomPropMiscNameColumn.HeaderText },
                { nameof(Shape.Point), PointColumn.HeaderText },
                { nameof(Shape.Parameter), ParameterColumn.HeaderText },
                { nameof(Shape.MiscPropValue), CustomPropMiscValueColumn.HeaderText },
                { nameof(Shape.Tooltip), ToolTipColumn.HeaderText },
                { nameof(Shape.Css), CssColumn.HeaderText },
                { nameof(Shape.DataType), TypeColumn.HeaderText },
                { nameof(Shape.NewPoint), NewPointColumn.HeaderText },
                { nameof(Shape.NewParameter), NewParameterColumn.HeaderText },
                { nameof(Shape.NewMiscPropValue), ReplaceMiscPropColumn.HeaderText },
                { nameof(Shape.NewTooltip), NewToolTipColumn.HeaderText },
                { nameof(Shape.NewCss), NewCssColumn.HeaderText },
                { nameof(Shape.ReplaceShapeName), ReplaceShapeColumn.HeaderText },
                { nameof(Shape.CustomParamType), CustomParameterTypeColumn.HeaderText },
                { nameof(Shape.GfxAndShape), GfxShapeColumn.HeaderText },
                { nameof(Shape.DataObjectId), DataObjectIdColumn.HeaderText },
                { nameof(Shape.BindingId), BindingIdColumn.HeaderText },
                { nameof(Shape.CsvIndex), CsvIndexColumn.HeaderText },
            };

            return dict;
        }

        private void HelpToolStripButton_Click(object sender, EventArgs e)
        {
            var readMe = AppDomain.CurrentDomain.BaseDirectory + "ReadMe.pdf";

            ProcessStartInfo startInfo = new()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C start \"\" /max \"{readMe}\""
            };
            Process process = new() { StartInfo = startInfo};
            process.Start();
        }
    }
}