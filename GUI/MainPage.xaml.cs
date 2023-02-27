using System.Reflection.Emit;
using SS;
using Label = Microsoft.Maui.Controls.Label;

namespace GUI;

public partial class MainPage : ContentPage
{
    private static int colCount = 25;
    private static int rowCount = 25;

    char[] columns = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    int[] rows = Enumerable.Range(1, colCount + 1).ToArray();

    private Dictionary<string, string> cells = new();

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
                stack.Add(
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
                        BackgroundColor = Color.FromRgb(250, 175, 200),
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                }
                );
            }
            Grid.Children.Add(stack);
        }
    }
}
