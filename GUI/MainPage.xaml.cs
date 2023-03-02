using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Windows.Input;
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

    private AbstractSpreadsheet spreadsheet = new Spreadsheet(s => true, s => s.ToUpper(), "six");

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
                    Text = "",
                    BackgroundColor = Colors.Pink,
                    HorizontalTextAlignment = TextAlignment.Center
                };
                entry.Focused += OnFocus;
                entry.Unfocused += OnUnFocus;
                entry.Completed += OnEntryCompletedCell;
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
                );
                cells.Add(label, entry);
            }
            Grid.Children.Add(stack);
        }
        Contents.Completed += OnEntryCompletedWidget;
    }

    private void OnFocus(object sender, EventArgs e)
    {
        try
        {
            Entry entry = (Entry)sender;
            string name = entry.StyleId;
            CellName.Text = name;
            cells[name].Text = spreadsheet.GetCellContents(name).ToString();
        }
        catch
        {
            InvalidFormulaDisplay();
        }
    }

    private void OnUnFocus(object sender, EventArgs e)
    {
        try
        {
            Entry entry = (Entry)sender;
            string name = entry.StyleId;
            CellName.Text = name;
            cells[name].Text = spreadsheet.GetCellValue(name).ToString();
        }
        catch
        {
            InvalidFormulaDisplay();
        }
    }

    private void OnEntryCompletedWidget(object sender, EventArgs e)
    {
        try
        {
            Entry entry = (Entry)sender;
            spreadsheet.SetContentsOfCell(CellName.Text, entry.Text);
            cells[CellName.Text].Text = spreadsheet.GetCellValue(CellName.Text).ToString();
            Contents.Text = spreadsheet.GetCellContents(CellName.Text).ToString();
            Value.Text = spreadsheet.GetCellValue(CellName.Text).ToString();
        }
        catch
        {
            InvalidFormulaDisplay();
        }
    }

    private void OnEntryCompletedCell(object sender, EventArgs e)
    {
        try
        {
            Entry entry = (Entry)sender;
            spreadsheet.SetContentsOfCell(CellName.Text, entry.Text);
            cells[CellName.Text].Text = spreadsheet.GetCellValue(CellName.Text).ToString();
            Contents.Text = spreadsheet.GetCellContents(CellName.Text).ToString();
            Value.Text = spreadsheet.GetCellValue(CellName.Text).ToString();
        }
        catch
        {
            InvalidFormulaDisplay();
        }
    }

    private async void FileOpen()
    {
        FileResult? fileResult = await FilePicker.Default.PickAsync();
        if (fileResult != null)
        {
            Debug.WriteLine("Successfully chose file: " + fileResult.FileName);
            string fileContents = File.ReadAllText(fileResult.FullPath);
            Debug.WriteLine("First 100 file chars:\n" + fileContents.Substring(0, 100));
            spreadsheet = new Spreadsheet(fileResult.FileName, s => true, s => s.ToUpper(), "six");
        }
    }

    private void FileMenuOpenAsync(object sender, EventArgs e)
    {
        FileOpen();
    }

    private void FileSave(object sender, EventArgs e)
    {
        spreadsheet.Save("spreadsheet.txt");
    }

    private async void FileMenuNew(object sender, EventArgs e)
    {
        if (spreadsheet.Changed)
        {
            bool response = await DisplayAlert("Warning", "Spreadsheet changed, do you want to continue?", "Yes", "No");
            if (response)
            {
                ClearSpreadsheet();
            }
        }
        else
        {
            ClearSpreadsheet();
        }
    }

    private void ClearSpreadsheet()
    {
        spreadsheet = new Spreadsheet(s => true, s => s.ToUpper(), "six");
        foreach (string name in cells.Keys)
        {
            cells[name].Text = "";
        }
        CellName.Text = "A1";
        Value.Text = "";
        Contents.Text = "";
    }

    private async void HelpChangeSelectionDisplay(object sender, EventArgs e)
    {
        //async bool DisplayAlert( … )
        await DisplayAlert(
        "How to change my selection",      // Title
        "Select a cell with your mouse that you want to focus on to see its contents" +
        " or to change its contents.", // Message 
        "Ok");
    }

    private async void HelpEditCellContentsDisplay(object sender, EventArgs e)
    {
        //async bool DisplayAlert( … )
        await DisplayAlert(
        "How to edit cell contents",      // Title
        " Once the cell is selected, enter either a digit" +
        " or a formula. If you would like the formula to be evaluated, add an equal" +
        " sign to the beginning of the formula and press enter. The calculated value" +
        " of the cell should then appear at the very top and right inside the cell" +
        " when it is not selected. The formula inside the cells may contain other cells." +
        " You can enter the digit or formula into a cell either by entering it into" +
        " the cell itself, or you can use the Contents widget at the top.", // Message 
        "Ok");
    }

    private async void HelpFileDisplay(object sender, EventArgs e)
    {
        await DisplayAlert(
        "How to save or open a file", // Title 
        " You can create a new spreadsheet, save your spreadsheet, or open a previously" +
        " created spreadsheet by clicking the \"File\" button at the top left of the" +
        " spreadsheet.", // Message
        "Ok");
    }

    private async void InvalidFormulaDisplay()
    {
        await DisplayAlert(
        "Error",      // Title
        "Cannot compute requested formula because the formula is invalid",
        "Ok");
    }

    private async void Randomize(object sender, EventArgs e)
    {
        bool response = await DisplayAlert("Warning", "Every cell in the spreadsheet will " +
            "be assigned a random value, and any unsaved changes will be lost. Would you like to continue?", "Yes", "No");

        if (response)
        {
            foreach (string name in cells.Keys)
            {
                Random random = new Random();
                int input = random.Next(0, 100);
                cells[name].Text = input + "";
                spreadsheet.SetContentsOfCell(name, input + "");
            }
        }
    }
}