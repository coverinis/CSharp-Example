/**
* \file MainWindow.xaml.cs
* \version PROG2120 - Assignment 4
* \author Shawn Coverini
* \date 2016-11-16
* \brief Interaction logic for MainWindow.xaml
*/

using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace SharpDraw
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        //Enumerators
        /// <summary>
        /// Representation of drawable objects
        /// </summary>
        enum PaintObject
        {
            Null,
            Line,
            Rectangle,
            Elipse
        }
        //Private
        #region
        private Point _pointerCoordinates;                   //Ending point on canvas
        private Point _startCoordinates;                     //Starting point on canvas
        private PaintObject _currentDraw = PaintObject.Null; //Current object to draw
        private int _drawCount;                              //Number of objects drawn
        private Line _line;                                  //Line storage
        private Ellipse _elip;                               //Elipse storage
        private Rectangle _rect;                             //Rectangle storage
        private bool _isChanged;                             //Flag if file was changed
        private double _width;                               //Temporary storage for shape width
        private double _height;                              //Temporary storage for shape height
        private const string ProgramTitle = "SharpDraw";     //Program title
        private string _file = "untitled.sdraw";             //Name of current file
        private string _filePath = string.Empty;             //Path to current file
        private string _fileString;                          //XAML string of objects in canvas
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
                _filePath = args[1];
                _file = args[1].Split('\\').Last();
                _fileString = File.ReadAllText(_filePath);
                string[] elements = _fileString.Split('\n');
                _drawCount = elements.Length - 1;
                for (int i = 0; i < _drawCount; i++)
                {
                    paintArea.Children.Add((UIElement)XamlReader.Parse(elements[i]));
                }
            }
            Title = string.Format("{0} - {1}", ProgramTitle, _file);
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
            byte[] argb = TranslateColour(setting["defaultFill"]);
            colourFill.SelectedColor = Color.FromArgb(argb[0], argb[1], argb[2], argb[3]);
            //Fill stroke colour
            argb = TranslateColour(setting["defaultStroke"]);
            colourStroke.SelectedColor = Color.FromArgb(argb[0], argb[1], argb[2], argb[3]);
        }

        /// <summary>
        /// FUNCTION    : translateColour
        /// DESCRIPTION : take html hex string and convert to an array of bytes representing argb channels
        /// </summary>
        /// <param name="hex">string : ARGB hex representing string</param>
        /// <returns>byte[] : A,R,G,B channels</returns>
        public byte[] TranslateColour(string hex)
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
            _pointerCoordinates = Mouse.GetPosition(paintArea);
            mousePosition.Content = string.Format("x : {0} y : {1}", _pointerCoordinates.X.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0'), _pointerCoordinates.Y.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0'));
            switch (_currentDraw)
            {
                //Update line
                case PaintObject.Line:
                    if (_line != null)
                    {
                        var line = paintArea.Children[_drawCount - 1] as Line;
                        if (line != null)
                            line.X2 = _pointerCoordinates.X;
                        var o = paintArea.Children[_drawCount - 1] as Line;
                        if (o != null)
                            o.Y2 = _pointerCoordinates.Y;
                    }
                    break;
                //Update rectangle
                case PaintObject.Rectangle:
                    if (_rect != null)
                    {
                        _width = _pointerCoordinates.X - _startCoordinates.X;
                        _height = _pointerCoordinates.Y - _startCoordinates.Y;

                        if (_width < 0)
                        {
                            Canvas.SetLeft(_rect, _pointerCoordinates.X);
                        }
                        else
                        {
                            Canvas.SetLeft(_rect, _startCoordinates.X);
                        }

                        if (_height < 0)
                        {
                            Canvas.SetTop(_rect, _pointerCoordinates.Y);
                        }
                        else
                        {
                            Canvas.SetTop(_rect, _startCoordinates.Y);
                        }
                        var rectangle = paintArea.Children[_drawCount - 1] as Rectangle;
                        if (rectangle != null)
                            rectangle.Width = Math.Abs(_width);
                        var o = paintArea.Children[_drawCount - 1] as Rectangle;
                        if (o != null)
                            o.Height = Math.Abs(_height);
                    }
                    break;
                //Update elipse
                case PaintObject.Elipse:
                    if (_elip != null)
                    {
                        _width = _pointerCoordinates.X - _startCoordinates.X;
                        _height = _pointerCoordinates.Y - _startCoordinates.Y;

                        if (_width < 0)
                        {
                            Canvas.SetLeft(_elip, _pointerCoordinates.X);
                        }
                        else
                        {
                            Canvas.SetLeft(_elip, _startCoordinates.X);
                        }

                        if (_height < 0)
                        {
                            Canvas.SetTop(_elip, _pointerCoordinates.Y);
                        }
                        else
                        {
                            Canvas.SetTop(_elip, _startCoordinates.Y);
                        }
                        var ellipse = paintArea.Children[_drawCount - 1] as Ellipse;
                        if (ellipse != null)
                            ellipse.Width = Math.Abs(_width);
                        var o = paintArea.Children[_drawCount - 1] as Ellipse;
                        if (o != null)
                            o.Height = Math.Abs(_height);
                    }
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
            _startCoordinates = Mouse.GetPosition(paintArea);
            mousePosition.Visibility = Visibility.Visible;
            switch (_currentDraw)
            {
                //Create initaial rubber band line
                case PaintObject.Line:
                    _line = new Line();
                    _line.X1 = _startCoordinates.X;
                    _line.Y1 = _startCoordinates.Y;
                    _line.Fill = new SolidColorBrush(Color.FromArgb(60,60,170,230));
                    _line.Stroke = new SolidColorBrush(Colors.Gray);
                    _line.StrokeThickness = 1;
                    _line.StrokeDashArray = new DoubleCollection() { 2 };
                    paintArea.Children.Add(_line);
                    _isChanged = true;
                    _drawCount++;
                    break;
                //Create initaial rubber band rectangle
                case PaintObject.Rectangle:
                    _rect = new Rectangle();
                    Canvas.SetLeft(_rect, _startCoordinates.X);
                    Canvas.SetTop(_rect, _startCoordinates.Y);
                    _rect.Fill = new SolidColorBrush(Color.FromArgb(60, 60, 170, 230));
                    _rect.Stroke = new SolidColorBrush(Colors.Gray);
                    _rect.StrokeThickness = 1;
                    _rect.StrokeDashArray = new DoubleCollection() { 2 };
                    paintArea.Children.Add(_rect);
                    _isChanged = true;
                    _drawCount++;
                    break;
                //Create initaial rubber band elipse
                case PaintObject.Elipse:
                    _elip = new Ellipse();
                    Canvas.SetLeft(_elip, _startCoordinates.X);
                    Canvas.SetTop(_elip, _startCoordinates.Y);
                    _elip.Fill = new SolidColorBrush(Color.FromArgb(60, 60, 170, 230));
                    _elip.Stroke = new SolidColorBrush(Colors.Gray);
                    _elip.StrokeThickness = 1;
                    _elip.StrokeDashArray = new DoubleCollection() { 2 };
                    paintArea.Children.Add(_elip);
                    _isChanged = true;
                    _drawCount++;
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
            switch (_currentDraw)
            {
                case PaintObject.Line:
                    if (colourFill.SelectedColor != null && colourStroke.SelectedColor != null)
                    {
                        _line.Fill = new SolidColorBrush((Color)colourFill.SelectedColor);
                        _line.Stroke = new SolidColorBrush((Color)colourStroke.SelectedColor);
                    }
                    _line.StrokeThickness = ((Line)((ComboBoxItem)borderThikness.SelectedItem).Content).StrokeThickness;
                    _line.StrokeDashArray = new DoubleCollection();
                    _fileString += XamlWriter.Save(paintArea.Children[_drawCount - 1]) + '\n';
                    _line = null;
                    break;
                case PaintObject.Rectangle:
                    if (colourFill.SelectedColor != null && colourStroke.SelectedColor != null)
                    {
                        _rect.Fill = new SolidColorBrush((Color)colourFill.SelectedColor);
                        _rect.Stroke = new SolidColorBrush((Color)colourStroke.SelectedColor);
                    }
                    _rect.StrokeThickness = ((Line)((ComboBoxItem)borderThikness.SelectedItem).Content).StrokeThickness;
                    _rect.StrokeDashArray = new DoubleCollection();
                    _fileString += XamlWriter.Save(paintArea.Children[_drawCount - 1]) + '\n';
                    _rect = null;
                    break;
                case PaintObject.Elipse:
                    if (colourFill.SelectedColor != null && colourStroke.SelectedColor != null)
                    {
                        _elip.Fill = new SolidColorBrush((Color)colourFill.SelectedColor);
                        _elip.Stroke = new SolidColorBrush((Color)colourStroke.SelectedColor);
                    }
                    _elip.StrokeThickness = ((Line)((ComboBoxItem)borderThikness.SelectedItem).Content).StrokeThickness;
                    _elip.StrokeDashArray = new DoubleCollection();
                    _fileString += XamlWriter.Save(paintArea.Children[_drawCount - 1]) + '\n';
                    _elip = null;
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
            _currentDraw = PaintObject.Line;
        }

        /// <summary>
        /// FUNCTION    : drawElipse_Click
        /// DESCRIPTION : Set shape to draw to elipse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawElipse_Click(object sender, RoutedEventArgs e)
        {
            _currentDraw = PaintObject.Elipse;
        }

        /// <summary>
        /// FUNCTION    : drawSquare_Click
        /// DESCRIPTION : Set shape to draw to rectangle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawSquare_Click(object sender, RoutedEventArgs e)
        {
            _currentDraw = PaintObject.Rectangle;
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
                    _drawCount = 0;
                    _isChanged = true;
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
            if(_filePath == string.Empty)
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
            _filePath = string.Empty;
            _fileString = string.Empty;
            _file = "untitled.sdraw";
            Title = string.Format("{0} - {1}", ProgramTitle, _file);
            _drawCount = 0;
        }

        /// <summary>
        /// FUNCTION    : SaveToFileAndClear
        /// DESCRIPTION : Prompt user to save the file and clear file information.
        /// </summary>
        private void SaveToFileAndClear()
        {
            if (_isChanged)
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Save Changes?", "New Image", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    SaveFileDialog save = new SaveFileDialog();
                    save.FileName = _file;
                    save.Filter = "SET Image (*.sdraw)|*.sdraw";
                    if (save.ShowDialog() == true)
                    {
                        File.WriteAllText(save.FileName, _fileString);
                        ClearFileInfo();
                    } 
                }
                else if (result == MessageBoxResult.No)
                {
                    ClearFileInfo();
                }
                _isChanged = false;
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
            if (_filePath == string.Empty)
            {
                SaveToFileAndClear();
            }
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "SET Image (*.sdraw)|*.sdraw";
            if (open.ShowDialog() == true)
            {
                _fileString = File.ReadAllText(open.FileName);
                string[] elements = _fileString.Split('\n');
                _drawCount = elements.Length - 1;
                for (int i = 0; i < _drawCount; i++)
                {
                    paintArea.Children.Add((UIElement)XamlReader.Parse(elements[i]));
                }
                _filePath = open.FileName;
                _file = _filePath.Split('\\').Last();
                Title = string.Format("{0} - {1}", ProgramTitle, _file);
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
            _isChanged = false;
            SaveFile();
        }

        /// <summary>
        /// FUNCTION    : SaveFile
        /// DESCRIPTION : Prompt user for the
        /// </summary>
        private void SaveFile()
        {
            if (_filePath == string.Empty)
            {
                SaveFileDialog save = new SaveFileDialog();
                save.FileName = _file;
                save.Filter = "SET Image (*.sdraw)|*.sdraw";
                if (save.ShowDialog() == true)
                {
                    File.WriteAllText(save.FileName, _fileString);
                    _filePath = save.FileName;
                    _file = _filePath.Split('\\').Last();
                    Title = string.Format("{0} - {1}", ProgramTitle, _file);
                }
            }
            else
            {
                File.WriteAllText(_filePath, _fileString);
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
            save.FileName = _file;
            save.Filter = "SET Image (*.sdraw)|*.sdraw";
            if (save.ShowDialog() == true)
            {
                File.WriteAllText(save.FileName, _fileString);
                _filePath = save.FileName;
                _file = _filePath.Split('\\').Last();
                Title = string.Format("{0} - {1}", ProgramTitle, _file);
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
            if ((_filePath == string.Empty && paintArea.Children.Count != 0) || _isChanged)
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
