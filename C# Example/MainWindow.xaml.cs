/// <header>
/// <file>MainWindow.xaml.cs</file>
/// <project>PROG2120 - Assignment 4</project>
/// <author>Shawn Coverini</author>
/// <date>2016-12-11</date>
/// <summary>Interaction logic for MainWindow.xaml</summary>
/// </header>
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Configuration;
using Microsoft.Win32;
using System.Windows.Markup;
using System.IO;

namespace SETPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Enumerators
        /// <summary>
        /// Representation of drawable objects
        /// </summary>
        enum PaintObject
        {
            NULL,
            LINE,
            RECTANGLE,
            ELIPSE
        }
        //Private
        #region
        private Point pointerCoordinates;                   //Ending point on canvas
        private Point startCoordinates;                     //Starting point on canvas
        private PaintObject currentDraw = PaintObject.NULL; //Current object to draw
        private int drawCount = 0;                          //Number of objects drawn
        private Line line;                                  //Line storage
        private Ellipse elip;                               //Elipse storage
        private Rectangle rect;                             //Rectangle storage
        private bool isChanged = false;                     //Flag if file was changed
        private double width;                               //Temporary storage for shape width
        private double height;                              //Temporary storage for shape height
        private string title = "SETPaint";                  //Program title
        private string file = "untitled.set";               //Name of current file
        private string filePath = string.Empty;             //Path to current file
        private string fileString;                          //XAML string of objects in canvas
        #endregion

        /// <summary>
        /// Start the program and look for Command argument for associated file
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            mousePosition.Content = "x : 0000 y : 0000";
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && File.Exists(args[1]))
            {
                filePath = args[1];
                file = args[1].Split('\\').Last();
                fileString = File.ReadAllText(filePath);
                string[] elements = fileString.Split('\n');
                drawCount = elements.Length - 1;
                for (int i = 0; i < drawCount; i++)
                {
                    paintArea.Children.Add((UIElement)XamlReader.Parse(elements[i]));
                }
            }
            Title = string.Format("{0} - {1}", title, file);
        }



        /// <summary>
        /// FUNCTION    : menuClose_Click
        /// DESCRIPTION : Event to close program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns>void</returns>
        private void menuClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// FUNCTION    : SETPaint_Loaded
        /// DESCRIPTION : Load default values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SETPaint_Loaded(object sender, RoutedEventArgs e)
        {
            //Appsettings
            var setting = ConfigurationManager.AppSettings;

            //Fill fill colour
            byte[] argb = translateColour(setting["defaultFill"]);
            colourFill.SelectedColor = Color.FromArgb(argb[0], argb[1], argb[2], argb[3]);
            //Fill stroke colour
            argb = translateColour(setting["defaultStroke"]);
            colourStroke.SelectedColor = Color.FromArgb(argb[0], argb[1], argb[2], argb[3]);
        }

        /// <summary>
        /// FUNCTION    : translateColour
        /// DESCRIPTION : take html hex string and convert to an array of bytes representing argb channels
        /// </summary>
        /// <param name="hex">string : ARGB hex representing string</param>
        /// <returns>byte[] : A,R,G,B channels</returns>
        public byte[] translateColour(string hex)
        {
            System.Drawing.Color colour = System.Drawing.ColorTranslator.FromHtml(hex);
            byte[] argb = { colour.A, colour.R, colour.G, colour.B };
            return argb;
        }

        /// <summary>
        /// FUNCTION    : paintArea_MouseMove
        /// DESCRIPTION : Update Coordinates of current shape being drawn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void paintArea_MouseMove(object sender, MouseEventArgs e)
        {
            //Get current position and change status label
            pointerCoordinates = Mouse.GetPosition(paintArea);
            mousePosition.Content = string.Format("x : {0} y : {1}", pointerCoordinates.X.ToString().PadLeft(4, '0'), pointerCoordinates.Y.ToString().PadLeft(4, '0'));
            switch (currentDraw)
            {
                //Update line
                case PaintObject.LINE:
                    if (line != null)
                    {
                        (paintArea.Children[drawCount - 1] as Line).X2 = pointerCoordinates.X;
                        (paintArea.Children[drawCount - 1] as Line).Y2 = pointerCoordinates.Y;
                    }
                    break;
                //Update rectangle
                case PaintObject.RECTANGLE:
                    if (rect != null)
                    {
                        width = pointerCoordinates.X - startCoordinates.X;
                        height = pointerCoordinates.Y - startCoordinates.Y;

                        if (width < 0)
                        {
                            Canvas.SetLeft(rect, pointerCoordinates.X);
                        }
                        else
                        {
                            Canvas.SetLeft(rect, startCoordinates.X);
                        }

                        if (height < 0)
                        {
                            Canvas.SetTop(rect, pointerCoordinates.Y);
                        }
                        else
                        {
                            Canvas.SetTop(rect, startCoordinates.Y);
                        }
                        (paintArea.Children[drawCount - 1] as Rectangle).Width = Math.Abs(width);
                        (paintArea.Children[drawCount - 1] as Rectangle).Height = Math.Abs(height);
                    }
                    break;
                //Update elipse
                case PaintObject.ELIPSE:
                    if (elip != null)
                    {
                        width = pointerCoordinates.X - startCoordinates.X;
                        height = pointerCoordinates.Y - startCoordinates.Y;

                        if (width < 0)
                        {
                            Canvas.SetLeft(elip, pointerCoordinates.X);
                        }
                        else
                        {
                            Canvas.SetLeft(elip, startCoordinates.X);
                        }

                        if (height < 0)
                        {
                            Canvas.SetTop(elip, pointerCoordinates.Y);
                        }
                        else
                        {
                            Canvas.SetTop(elip, startCoordinates.Y);
                        }
                        (paintArea.Children[drawCount - 1] as Ellipse).Width = Math.Abs(width);
                        (paintArea.Children[drawCount - 1] as Ellipse).Height = Math.Abs(height);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// FUNCTION    : paintArea_MouseDown
        /// DESCRIPTION : Create initaial rubber band shape and mark file as changed.
        /// finally update the total number of items drawn to the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void paintArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //get starting position of mouse and make visible in the status bar
            startCoordinates = Mouse.GetPosition(paintArea);
            mousePosition.Visibility = Visibility.Visible;
            switch (currentDraw)
            {
                //Create initaial rubber band line
                case PaintObject.LINE:
                    line = new Line();
                    line.X1 = startCoordinates.X;
                    line.Y1 = startCoordinates.Y;
                    line.Fill = new SolidColorBrush(Color.FromArgb(60,60,170,230));
                    line.Stroke = new SolidColorBrush(Colors.Gray);
                    line.StrokeThickness = 1;
                    line.StrokeDashArray = new DoubleCollection() { 2 };
                    paintArea.Children.Add(line);
                    isChanged = true;
                    drawCount++;
                    break;
                //Create initaial rubber band rectangle
                case PaintObject.RECTANGLE:
                    rect = new Rectangle();
                    Canvas.SetLeft(rect, startCoordinates.X);
                    Canvas.SetTop(rect, startCoordinates.Y);
                    rect.Fill = new SolidColorBrush(Color.FromArgb(60, 60, 170, 230));
                    rect.Stroke = new SolidColorBrush(Colors.Gray);
                    rect.StrokeThickness = 1;
                    rect.StrokeDashArray = new DoubleCollection() { 2 };
                    paintArea.Children.Add(rect);
                    isChanged = true;
                    drawCount++;
                    break;
                //Create initaial rubber band elipse
                case PaintObject.ELIPSE:
                    elip = new Ellipse();
                    Canvas.SetLeft(elip, startCoordinates.X);
                    Canvas.SetTop(elip, startCoordinates.Y);
                    elip.Fill = new SolidColorBrush(Color.FromArgb(60, 60, 170, 230));
                    elip.Stroke = new SolidColorBrush(Colors.Gray);
                    elip.StrokeThickness = 1;
                    elip.StrokeDashArray = new DoubleCollection() { 2 };
                    paintArea.Children.Add(elip);
                    isChanged = true;
                    drawCount++;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// FUNCTION    : paintArea_MouseUp
        /// DESCRIPTION : Apply final properties to the drawn shape
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void paintArea_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mousePosition.Visibility = Visibility.Hidden;
            switch (currentDraw)
            {
                case PaintObject.LINE:
                    line.Fill = new SolidColorBrush((Color)colourFill.SelectedColor);
                    line.Stroke = new SolidColorBrush((Color)colourStroke.SelectedColor);
                    line.StrokeThickness = ((Line)((ComboBoxItem)borderThikness.SelectedItem).Content).StrokeThickness;
                    line.StrokeDashArray = new DoubleCollection();
                    fileString += XamlWriter.Save(paintArea.Children[drawCount - 1]) + '\n';
                    line = null;
                    break;
                case PaintObject.RECTANGLE:
                    rect.Fill = new SolidColorBrush((Color)colourFill.SelectedColor);
                    rect.Stroke = new SolidColorBrush((Color)colourStroke.SelectedColor);
                    rect.StrokeThickness = ((Line)((ComboBoxItem)borderThikness.SelectedItem).Content).StrokeThickness;
                    rect.StrokeDashArray = new DoubleCollection();
                    fileString += XamlWriter.Save(paintArea.Children[drawCount - 1]) + '\n';
                    rect = null;
                    break;
                case PaintObject.ELIPSE:
                    elip.Fill = new SolidColorBrush((Color)colourFill.SelectedColor);
                    elip.Stroke = new SolidColorBrush((Color)colourStroke.SelectedColor);
                    elip.StrokeThickness = ((Line)((ComboBoxItem)borderThikness.SelectedItem).Content).StrokeThickness;
                    elip.StrokeDashArray = new DoubleCollection();
                    fileString += XamlWriter.Save(paintArea.Children[drawCount - 1]) + '\n';
                    elip = null;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// FUNCTION    : drawLine_Click
        /// DESCRIPTION : Set shape to draw to line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawLine_Click(object sender, RoutedEventArgs e)
        {
            currentDraw = PaintObject.LINE;
        }

        /// <summary>
        /// FUNCTION    : drawElipse_Click
        /// DESCRIPTION : Set shape to draw to elipse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawElipse_Click(object sender, RoutedEventArgs e)
        {
            currentDraw = PaintObject.ELIPSE;
        }

        /// <summary>
        /// FUNCTION    : drawSquare_Click
        /// DESCRIPTION : Set shape to draw to rectangle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawSquare_Click(object sender, RoutedEventArgs e)
        {
            currentDraw = PaintObject.RECTANGLE;
        }

        /// <summary>
        /// FUNCTION    : drawErase_Click
        /// DESCRIPTION : Erase content of Canvas after verifying with the user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawErase_Click(object sender, RoutedEventArgs e)
        {
            if (paintArea.Children.Count > 0)
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Are you sure you wish to erase the image?", "Erase Warning!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    paintArea.Children.Clear();
                    drawCount = 0;
                    isChanged = true;
                }
            }
        }

        /// <summary>
        /// FUNCTION    : menuAbout_Click
        /// DESCRIPTION : Open About Dialogue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuAbout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        /// <summary>
        /// FUNCTION    : menuNew_Click
        /// DESCRIPTION : Create a new file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuNew_Click(object sender, RoutedEventArgs e)
        {
            if(filePath == string.Empty)
            {
                SaveToFileAndClear();
            }
            else
            {
                ClearFileInfo();
            }
        }

        /// <summary>
        /// FUNCTION    : menuNew_Click
        /// DESCRIPTION : Clear infromation about the file
        /// </summary>
        private void ClearFileInfo()
        {
            paintArea.Children.Clear();
            filePath = string.Empty;
            fileString = string.Empty;
            file = "untitled.set";
            Title = string.Format("{0} - {1}", title, file);
            drawCount = 0;
        }

        /// <summary>
        /// FUNCTION    : SaveToFileAndClear
        /// DESCRIPTION : Prompt user to save the file and clear file information.
        /// </summary>
        private void SaveToFileAndClear()
        {
            if (isChanged)
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Save Changes?", "New Image", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    SaveFileDialog save = new SaveFileDialog();
                    save.FileName = file;
                    save.Filter = "SET Image (*.set)|*.set";
                    if (save.ShowDialog() == true)
                    {
                        File.WriteAllText(save.FileName, fileString);
                        ClearFileInfo();
                    } 
                }
                else if (result == MessageBoxResult.No)
                {
                    ClearFileInfo();
                }
                isChanged = false;
            }
        }

        /// <summary>
        /// FUNCTION    : menuOpen_Click
        /// DESCRIPTION : Prompt user to save the file and clear file information.
        /// Then open a file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            if (filePath == string.Empty)
            {
                SaveToFileAndClear();
            }
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "SET Image (*.set)|*.set";
            if (open.ShowDialog() == true)
            {
                fileString = File.ReadAllText(open.FileName);
                string[] elements = fileString.Split('\n');
                drawCount = elements.Length - 1;
                for (int i = 0; i < drawCount; i++)
                {
                    paintArea.Children.Add((UIElement)XamlReader.Parse(elements[i]));
                }
                filePath = open.FileName;
                file = filePath.Split('\\').Last();
                Title = string.Format("{0} - {1}", title, file);
            }
        }

        /// <summary>
        /// FUNCTION    : menuSave_Click
        /// DESCRIPTION : Save the file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuSave_Click(object sender, RoutedEventArgs e)
        {
            isChanged = false;
            SaveFile();
        }

        /// <summary>
        /// FUNCTION    : SaveFile
        /// DESCRIPTION : Prompt user for the
        /// </summary>
        private void SaveFile()
        {
            if (filePath == string.Empty)
            {
                SaveFileDialog save = new SaveFileDialog();
                save.FileName = file;
                save.Filter = "SET Image (*.set)|*.set";
                if (save.ShowDialog() == true)
                {
                    File.WriteAllText(save.FileName, fileString);
                    filePath = save.FileName;
                    file = filePath.Split('\\').Last();
                    Title = string.Format("{0} - {1}", title, file);
                }
            }
            else
            {
                File.WriteAllText(filePath, fileString);
            }
        }

        /// <summary>
        /// FUNCTION    : menuSaveAs_Click
        /// DESCRIPTION : Prompt user to save the file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.FileName = file;
            save.Filter = "SET Image (*.set)|*.set";
            if (save.ShowDialog() == true)
            {
                File.WriteAllText(save.FileName, fileString);
                filePath = save.FileName;
                file = filePath.Split('\\').Last();
                Title = string.Format("{0} - {1}", title, file);
            }
        }

        /// <summary>
        /// FUNCTION    : SETPaint_Closing
        /// DESCRIPTION : Prompt user to save file when closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SETPaint_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((filePath == string.Empty && paintArea.Children.Count != 0) || isChanged)
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Save Changes?", "New Image", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    SaveFile();
                }

            }
        }
    }
}
