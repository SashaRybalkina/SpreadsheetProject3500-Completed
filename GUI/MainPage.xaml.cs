using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Windows.Input;
using SpreadsheetUtilities;
using SS;
using Label = Microsoft.Maui.Controls.Label;

namespace GUI;

/// <summary>
/// Author: Victoria Locke
/// Partner: Sasha Rybalkina
/// Date:   March 3, 2023
/// Course: CS 3500, University of Utah
/// Copyright: CS 3500 and Victoria Locke's work may not
///            be copied for use in Academic Coursework
/// I, Victoria Locke, certify that I wrote this code from scratch and
/// did not copy it in part or whole from another source.  All 
/// references used in the completion of the assignments are cited 
/// in my README file.
///
/// File Contents
///
///     This class creates a visual representation of a spreadsheet using .NET
///     MAUI. It can visually update cells' values and contents using the widgets
///     at the top or the cell itself. There is a help menu and a file menu. The
///     help menu gives the user specific options for getting information on
///     specific questions. The file menu allows for the user to save the file, open 
///     an older file, or create a new file. There is also a unique button, which 
///     randomizes each individual cell of the spreadsheet.
/// </summary>

public partial class MainPage : ContentPage
{
    private static int colCount = 26;
    private static int rowCount = 99;

    char[] columns = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    int[] rows = Enumerable.Range(1, rowCount + 1).ToArray();

    // keeps track of each cell's name and contents
    private Dictionary<string, Entry> cells = new();

    private AbstractSpreadsheet spreadsheet = new Spreadsheet(s => true, s => s.ToUpper(), "six");

    public MainPage()
    {
        InitializeComponent();
        BuildGUI();
    }

    /// <summary>
    /// Builds the visual representation of the Spreadsheet with the alphabet columns and 99 rows
    /// </summary>
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
                label = columns[i - 1].ToString();
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
            string label = rows[i].ToString();
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
                // if the entry is focused on then calls the method OnFocus()
                entry.Focused += OnFocus;
                // if the entry is not currently focused on then calls the method OnUnFocus()
                entry.Unfocused += OnUnFocus;
                // once the entry is completed then calls the method OnEntryCompleted()
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
        // calls the method OnEntryCompletedWidget() to update the widget contents 
        Contents.Completed += OnEntryCompletedWidget;
    }

    /// <summary>
    ///  A private helper method which displays the contents of the cell, when the cell 
    ///  is focused on. If there is an error with the formula a warning pops up explaining
    ///  the invalid formula error.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnFocus(object sender, EventArgs e)
    {
        try
        {
            Entry entry = (Entry)sender;
            entry.BackgroundColor = Colors.Yellow;
            string name = entry.StyleId;
            CellName.Text = name;
            // if the contents entered into the cell cause a FormulaError each cell will display "ERROR"
            if (spreadsheet.GetCellValue(CellName.Text) is FormulaError)
            {
                cells[CellName.Text].Text = "ERROR";
                Value.Text = "ERROR";
                Contents.Text = spreadsheet.GetCellContents(name).ToString();
            }
            // if the contents cause no problems then the cells and widgets will update appropriately
            else
            {
                // cells display contents when focused on
                cells[name].Text = spreadsheet.GetCellContents(name).ToString();
                Value.Text = spreadsheet.GetCellValue(name).ToString();
                Contents.Text = spreadsheet.GetCellContents(name).ToString();
            }
        }
        catch
        {
            InvalidFormulaDisplay();
        }
    }

    /// <summary>
    /// A private helper method which displays the value of the cell, when the cell
    /// is not focused on. If there is an error with the formula a warning pops up explaining
    ///  the invalid formula error.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnUnFocus(object sender, EventArgs e)
    {
        try
        {
            Entry entry = (Entry)sender;
            entry.BackgroundColor = Colors.Pink;
            string name = entry.StyleId;
            CellName.Text = name;
            // if the contents entered into the cell cause a FormulaError each cell will display "ERROR"
            if (spreadsheet.GetCellValue(CellName.Text) is FormulaError)
            {
                cells[CellName.Text].Text = "ERROR";
                Value.Text = "ERROR";
            }
            // if the contents cause no problems then everything will update normally
            else
            {
                // cells display the value if they are not focused on
                cells[name].Text = spreadsheet.GetCellValue(name).ToString();
            }
        }
        catch 
        {
            InvalidFormulaDisplay();
        }
    }

    /// <summary>
    /// A private helper method which updates the cells contents and value when the contents
    /// are entered directly into the widget, instead of the cell. If an invalid formula is entered 
    /// an error window is displayed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnEntryCompletedWidget(object sender, EventArgs e)
    {
        try
        {
            Entry entry = (Entry)sender;
            spreadsheet.SetContentsOfCell(CellName.Text, entry.Text);
            // if the contents entered into the cell cause a FormulaError each cell will display "ERROR" 
            if (spreadsheet.GetCellValue(CellName.Text) is FormulaError)
            {
                cells[CellName.Text].Text = "ERROR";
                Value.Text = "ERROR";
                Contents.Text = spreadsheet.GetCellContents(CellName.Text).ToString();
            }
            // if the contents cause no problems then everything will update normally
            else
            {
                cells[CellName.Text].Text = spreadsheet.GetCellValue(CellName.Text).ToString();
                Contents.Text = spreadsheet.GetCellContents(CellName.Text).ToString();
                Value.Text = spreadsheet.GetCellValue(CellName.Text).ToString();
            }
        }
        catch
        {
            InvalidFormulaDisplay();
        }
    }

    /// <summary>
    /// A private helper method which updates the cells contents to show correctly in the widgets
    /// when the contents are entered directly into the cell, instead of the widget. If an invalid
    /// formula is entered an error window is displayed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnEntryCompletedCell(object sender, EventArgs e)
    {
        try
        {
            Entry entry = (Entry)sender;
            IList<string> dependees = spreadsheet.SetContentsOfCell(CellName.Text, entry.Text);
            cells[CellName.Text].Text = spreadsheet.GetCellValue(CellName.Text).ToString();
            // if the contents entered into the cell cause a FormulaError each cell will display "ERROR" 
            if (spreadsheet.GetCellValue(CellName.Text) is FormulaError)
            {
                cells[CellName.Text].Text = "ERROR";
                Value.Text = "ERROR";
                Contents.Text = spreadsheet.GetCellContents(CellName.Text).ToString();
            }
            // if the contents cause no problems then everything will update normally
            else
            {
                Contents.Text = spreadsheet.GetCellContents(CellName.Text).ToString();
                Value.Text = spreadsheet.GetCellValue(CellName.Text).ToString();
                // updates the dependencies - if a cell updates its contents and another cell depends on that value
                // that cell will also update
                foreach (string cell in dependees)
                {
                    cells[cell].Text = spreadsheet.GetCellValue(cell).ToString();
                }
            }
        }
        catch
        {
            InvalidFormulaDisplay();
        }
    }

    /// <summary>
    /// A private helper method to open a file, when the user selects the 'Open' option in the
    /// file menu. If an incorrect file path is entered or any other error occurs, an error window
    /// is displayed explaining the error. 
    /// </summary>
    private async void FileMenuOpen(object sender, EventArgs e)
    {
        try
        {
            FileResult? fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null)
            {
                Debug.WriteLine("Successfully chose file: " + fileResult.FileName);
                string fileContents = File.ReadAllText(fileResult.FullPath);
                Debug.WriteLine("First 100 file chars:\n" + fileContents.Substring(0, 100));
                spreadsheet = new Spreadsheet(fileResult.FullPath, s => true, s => s.ToUpper(), "six");
                // updates each cell to display the correct information from the opened file
                foreach (string cell in spreadsheet.GetNamesOfAllNonemptyCells())
                {
                    cells[cell].Text = spreadsheet.GetCellValue(cell).ToString();
                }
            }
        }
        catch
        {
            FilePathOrNameIncorrectDisplay();
        }
    }

    /// <summary>
    /// A private helper method to save the spreadsheet file, when the user selects the 'Save' option
    /// in the file menu. If the user tries to save a file with an incorrect file path, or the file is
    /// not of type '.sprd', an error window is displayed explaining the error. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void FileMenuSave(object sender, EventArgs e)
    {
        string userPath;
        userPath = await DisplayPromptAsync(
            " Save File",
            " Please enter the full path for where you would like to save" +
            " your file to. " +
            " \nThe file must be of '.sprd' type.",
            " Save",
            " Cancel");
        if (userPath == null || userPath == "" || userPath.Substring(userPath.Length - 5) != ".sprd")
        {
            FilePathOrNameIncorrectDisplay();
        }
        else
        {
            spreadsheet.Save(userPath);
        }
    }

    /// <summary>
    /// A private helper method to open up a new spreadsheet file. Clears the spreadsheet.
    /// If the user has not saved the file before opening a new one a warning window is displayed
    /// asking the user if they are sure they want to move on without saving.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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

    /// <summary>
    /// A private helper method that clears the entire spreadsheet.
    /// </summary>
    private void ClearSpreadsheet()
    {
        spreadsheet = new Spreadsheet(s => true, s => s.ToUpper(), "six");
        foreach (string name in cells.Keys)
        {
            cells[name].Text = "";
        }
        // updates the widget back to default
        CellName.Text = "A1";
        Value.Text = "";
        Contents.Text = "";
    }

    /// <summary>
    /// A private helper method which does a unique task - randomizes every single cell value in
    /// the spreadsheet.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Randomize(object sender, EventArgs e)
    {
        bool response = await DisplayAlert("Warning", "Every cell in the spreadsheet will " +
            "be assigned a random value, and any unsaved changes will be lost. Would you like to continue?", "Yes", "No");

        if (response)
        {
            // adds a random number to every single cell in the spreadsheet
            foreach (string name in cells.Keys)
            {
                Random random = new Random();
                int input = random.Next(0, 100);
                cells[name].Text = input + "";
                spreadsheet.SetContentsOfCell(name, input + "");
                Value.Text = spreadsheet.GetCellValue(name).ToString();
                Contents.Text = spreadsheet.GetCellContents(name).ToString();
            }
        }
    }

    /// <summary>
    /// A private helper method that displays a window which explains how to change the user's
    /// cell selection, when the user opens the help menu and clicks the Change Selection help option
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void HelpChangeSelectionDisplay(object sender, EventArgs e)
    {
        //async bool DisplayAlert( … )
        await DisplayAlert(
        "How to change my selection",      // Title
        "Select a cell with your mouse that you want to focus on to see its contents" +
        " or to change its contents.", // Message 
        "Ok");
    }

    /// <summary>
    /// A private helper method that displays a window which explains how to edit the user's
    /// cell contents, when the user opens the help menu and clicks the Edit Cell Contents help option
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void HelpEditCellContentsDisplay(object sender, EventArgs e)
    {
        //async bool DisplayAlert( … )
        await DisplayAlert(
        "How to edit cell contents",      // Title
        " Once the cell is selected, enter either a digit" +
        " or a formula. If you would like the formula to be evaluated, add an equal" +
        " sign to the beginning of the formula and press enter. You must press enter for" +
        " your contents to be saved to the cell. The calculated value of the cell should" + 
        " then appear at the very top and right inside the cell when it is not selected." + 
        " The formula inside the cells may contain other cells. You can enter the digit" +
        " or formula into a cell either by entering it into the cell itself, or you can" +
        " use the Contents widget at the top.", // Message 
        "Ok");
    }

    /// <summary>
    /// A private helper method that displays a window which explains the file menu contents when
    /// the user opens the help menu and clicks the File help option
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void HelpFileDisplay(object sender, EventArgs e)
    {
        await DisplayAlert(
        "How to save or open a file", // Title 
        " You can create a new spreadsheet, save your spreadsheet, or open a previously" +
        " created spreadsheet by clicking the \"File\" button at the top left of the" +
        " spreadsheet." +
        " If you choose to save a file please save the file with the exact path name for" +
        " where you would like to save your file to, and what you would like to save your file" +
        " name as. " +
        " You must save your file with '.sprd'", // Message
        "Ok");
    }

    /// <summary>
    /// A private helper method that displays a window which explains the error which occurred when an 
    /// incorrect file path or name was entered by the user when trying to save the spreadsheet. 
    /// </summary>
    private async void FilePathOrNameIncorrectDisplay()
    {
        await DisplayAlert(
        " Uh oh!",
        " Your file name or path is incorrect. The file did not save. " +
        "\n Make sure your file is of '.sprd' type" +
        "\n Try saving again.", // Message
        "Ok") ;
    }

    /// <summary>
    /// A private helper method that displays a window which explains that the file the user chose
    /// to open cannot be opened. 
    /// </summary>
    private async void UnableToOpenFile()
    {
        await DisplayAlert(
            "Unable to open file",
            " The file you chose to open could not be opened. Try selecting" +
            " a different file.",
            "Ok");
    }
    /// <summary>
    /// A private helper method that displays a window which that tells the user their formula was
    /// invalid, instead of throwing an error and crashing the spreadsheet application.
    /// </summary>
    private async void InvalidFormulaDisplay()
    {
        await DisplayAlert(
        "Error",      // Title
        "Cannot compute requested formula because the formula is invalid",
        "Ok");
    }
}