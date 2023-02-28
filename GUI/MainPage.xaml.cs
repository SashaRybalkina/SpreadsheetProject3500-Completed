using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using SS;
using Label = Microsoft.Maui.Controls.Label;

namespace GUI;

public partial class MainPage : ContentPage
{
    private static int colCount = 10;
    private static int rowCount = 10;

    char[] columns = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    int[] rows = Enumerable.Range(1, colCount + 1).ToArray();

    private Dictionary<string, Entry> cells = new();

    private AbstractSpreadsheet spreadsheet = new Spreadsheet();

    public MainPage()
    {
        InitializeComponent();
        BuildGUI();
    }
    private void BuildGUI()
    {
        for (int i = 0; i < colCount; i++)
        {
            string label;
            if (i == 0)
            {
                label = "_";
            }
            else
            {
                label = columns[i - 1] + "";
            }
            TopLabels.Add(
            new Border
            {
                Stroke = Color.FromRgb(150, 0, 0),
                StrokeThickness = 5,
                HeightRequest = 30,
                WidthRequest = 75,
                HorizontalOptions = LayoutOptions.Center,
                Content =
                new Label
                {
                    Text = $"{label}",
                    BackgroundColor = Color.FromRgb(250, 150, 200),
                    HorizontalTextAlignment = TextAlignment.Center
                }
            }
            );
        }
        for (int i = 0; i < rowCount; i++)
        {
            string label = rows[i] + "";
            LeftLabels.Add(
            new Border
            {
                Stroke = Color.FromRgb(150, 0, 0),
                StrokeThickness = 5,
                HeightRequest = 30,
                WidthRequest = 75,
                HorizontalOptions = LayoutOptions.Center,
                Content =
                new Label
                {
                    Text = $"{label}",
                    BackgroundColor = Color.FromRgb(250, 150, 200),
                    HorizontalTextAlignment = TextAlignment.Center
                }
            }
            );
        }
        for (int j = 0; j < rowCount; j++)
        {
            var stack = new HorizontalStackLayout();
            for (int i = 0; i < colCount - 1; i++)
            {
                string label = "" + columns[i] + rows[j];
                Entry entry = new Entry
                {
                    StyleId = label,
                    PlaceholderColor = Colors.Pink,
                    Text = "",
                    BackgroundColor = Colors.Pink,
                    HorizontalTextAlignment = TextAlignment.Center
                };
                entry.Focused += OnFocus;
                stack.Add(
                new Border
                {
                    Stroke = Color.FromRgb(150, 0, 0),
                    StrokeThickness = 5,
                    HeightRequest = 30,
                    WidthRequest = 75,
                    HorizontalOptions = LayoutOptions.Center,
                    Content = entry
                }
                ) ;
                cells.Add(label, entry);
            }
            Grid.Children.Add(stack);
        }
        Contents.Completed += OnEntryCompleted;
        ///Added
        cells[CellName.Text].Completed += OnEntryCompleted;
    }
    private void SetCell(object sender, EventArgs e)
    {
        cells[CellName.Text].Focus();
        spreadsheet.SetContentsOfCell(CellName.Text, cells[CellName.Text].Text);
        cells[CellName.Text].Text = "" + spreadsheet.GetCellValue(CellName.Text);
    }

    private void OnFocus(object sender, EventArgs e)
    {
        Entry entry = (Entry)sender;
        string name = entry.StyleId;
        CellName.Text = name;
        Contents.Text = spreadsheet.GetCellContents(name) + "";
        Value.Text = spreadsheet.GetCellValue(name) + "";
        ///Added
        entry.Text = spreadsheet.GetCellValue(name) + "";
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        Entry entry = (Entry)sender;
        spreadsheet.SetContentsOfCell(CellName.Text, entry.Text);
        cells[CellName.Text] = entry;
    }
}
