using System.Diagnostics;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Game_Of_Life_bitmap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int version_number = 3;

        cel[,] grid;
        int ratio = 20;
        int width = Convert.ToInt32(680 / 20);
        int height = Convert.ToInt32(420 / 20);


        bool paused = true;
        bool spawnGlider = false;
        bool showGrid = true;

        int[,] glider = new int[,] { { 0, 1, 0 }, { 0, 0, 1 }, { 1, 1, 1 } };

        public System.Timers.Timer timer;
        WriteableBitmap bmp = BitmapFactory.New(680 / 20, 420 / 20);

        int cycles = 0;
        public MainWindow()
        {
            InitializeComponent();

            //TIMER
            timer = new System.Timers.Timer(250);
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Stop();

            // IMAGE/BITMAP FOR THE GRID
            ImageControl.Source = bmp;
            RenderOptions.SetBitmapScalingMode(ImageControl, BitmapScalingMode.NearestNeighbor);

            setup();
        }

        //SETS UP THE CELLIST AND GRID
        void setup()
        {
            grid = new cel[width, height];

            bmp = BitmapFactory.New(width, height);
            ImageControl.Source = bmp;

            MyCanvas.Children.Clear();
            if (showGrid)
            {
                drawGrid();
            }


            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    grid[i, j] = new cel();

                    grid[i, j].x = i;
                    grid[i, j].y = j;
                    grid[i, j].status = false;
                    grid[i, j].compare_status = false;
                }
            }

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    grid[i, j].vulBuren(grid, width, height);
                }
            }
        }

        //DRAWS THE GRIDLINES
        void drawGrid()
        {
            int offsetX = 109;
            int offsetY = 10;
            for (int i = 0; i < height + 1; i++)
            {
                Line line = new Line();
                line.X1 = offsetX;
                line.Y1 = ratio * i + offsetY;
                line.X2 = 680 + offsetX;
                line.Y2 = ratio * i + offsetY;
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 1;
                MyCanvas.Children.Add(line);
            }
            for (int i = 0; i < width + 1; i++)
            {
                Line line = new Line();
                line.X1 = ratio * i + offsetX;
                line.Y1 = offsetY;
                line.X2 = ratio * i + offsetX;
                line.Y2 = 420 + offsetY;
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 1;
                MyCanvas.Children.Add(line);
            }
        }

        //WHEN TIMER RUNS OUT
        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            //NEEDS TO BE SEPERATE THREAD FOR GUI REASONS
            this.Dispatcher.Invoke(() =>
            {
                Update();
            });
        }

        //UPDATE GRID
        void Update()
        {
            cycles++;
            cycleCounter.Content = "Cycle " + Convert.ToString(cycles);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    grid[i, j].compare_status = grid[i, j].status;
                }
            }
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    cel tempcel = grid[i, j];
                    tempcel.UpdateCell();
                    if (tempcel.status)
                    {
                        bmp.SetPixel(tempcel.x, tempcel.y, Colors.Black);
                    }
                    else
                    {
                        bmp.SetPixel(tempcel.x, tempcel.y, Colors.White);
                    }
                }
            }

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    grid[i, j].compare_status = grid[i, j].status;
                }
            }
        }

        //PAUSE EXECUTION
        void Pause()
        {
            Debug.WriteLine("paused");
            pause_unpause.Content = "Unpause";
            timer.Stop();
        }

        //UNPAUSE EXECUTION
        void Unpause()
        {
            Debug.WriteLine("unpaused");
            pause_unpause.Content = "Pause";
            timer.Start();
        }

        //CHANGE TIMER INTERVAL
        void changeTimeInterval(double interval)
        {
            timer.Interval = interval;
        }

        //CHANGES THE SIZE OF THE GRID AND REDRAWS IT
        void change_size(int pxSize)
        {
            width = 680 / pxSize;
            height = 420 / pxSize;
            ratio = pxSize;
            setup();
        }

        //LOAD FROM SAVE FILE, COULD MAKE THIS A BITMAP
        bool Load()
        {
            string fileLocation = "";
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            //Set filter for file extension and default file extension 
            dlg.DefaultExt = ".gol";
            dlg.Filter = "Game_Of_Life_bitmap save files (.gol)|*.gol| Any|*";

            //Display OpenFileDialog
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                fileLocation = dlg.FileName;
            }
            else
            {
                return false;
            }



            string[] text = File.ReadAllLines(fileLocation);

            if (Convert.ToInt32(text[1]) != version_number)
            {
                Debug.WriteLine("Wrong version number");
                return false;
            }

            int checksum = 0;

            change_size(Convert.ToInt32(text[0]));

            for (int rij = 2; rij < width + 2; rij++)
            {
                for (int kol = 0; kol < height; kol++)
                {
                    if (rij == Convert.ToInt32(text[0]) - 1)
                    {
                        break;
                    }
                    if (Convert.ToInt32(text[rij][kol]) == '1')
                    {
                        checksum += 1;
                        grid[rij - 2, kol].status = true;
                        bmp.SetPixel(rij - 2, kol, Colors.Black);
                    }
                    else
                    {
                        grid[rij - 2, kol].status = false;
                        bmp.SetPixel(rij - 2, kol, Colors.White);
                    }
                }
            }

            //have not tested checksum and version number extensively
            if (Convert.ToInt32(text[^1]) == checksum)
            {
                Debug.WriteLine("checksum passed");
            }
            else
            {
                Debug.WriteLine("CHECKSUM FAILED. Will still load.");
                Debug.WriteLine($"Counted {checksum} but checksum has {text[^1]}");

            }

            return true;
        }

        //SAVE TO SAVE FILE, COULD MAKE THIS A BITMAP
        bool Save()
        {
            string fileLocation = "";
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Save";
            dlg.DefaultExt = ".gol";
            dlg.Filter = "Game_Of_Life_bitmap save files (.gol)|*.gol";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                fileLocation = dlg.FileName;
            }
            else
            {
                return false;
            }


            var csv = new StringBuilder();

            csv.AppendLine(Convert.ToString(ratio));
            csv.AppendLine(Convert.ToString(version_number));

            int checksum = 0;
            for (int rij = 0; rij < width; rij++)
            {
                string row = "";
                for (int kol = 0; kol < height; kol++)
                {
                    if (grid[rij, kol].status)
                    {
                        row += 1;
                        checksum += 1;
                    }
                    else
                    {
                        row += 0;
                    }

                }
                csv.AppendLine(row);
            }
            csv.AppendLine(Convert.ToString(checksum));
            File.WriteAllText(fileLocation, csv.ToString());
            return true;
        }

        //PLACES A SPECIFIC FIGURE ON THE GRID, ONLY USEF FOR GLIDER ATM
        void placeFigure(int x, int y, int[,] figure, int wd, int hd)
        {
            for (int i = 0; i < wd; i++)
            {
                for (int j = 0; j < hd; j++)
                {
                    if (figure[i, j] == 1)
                    {
                        if (i + x >= width || j + y >= height)
                        {
                            Debug.WriteLine("out of bounds");
                        }
                        else
                        {
                            grid[i + x, j + y].status = true;
                            bmp.SetPixel(i + x, j + y, Colors.Black);
                        }
                    }
                }
            }
        }

        //CLEARS THE ENTIRE GRID
        void clear()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    grid[i, j].status = false;
                    bmp.SetPixel(i, j, Colors.White);
                }
            }
            cycleCounter.Content = "Cycles";
            cycles = 0;
        }

        //TRIGGERS WHEN THE MOUSE CLICKS ON THE IMAGE/BITMAP
        private void ImageControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = e.GetPosition(ImageControl);

            int rij = Convert.ToInt32(clickPoint.X) / ratio;
            int kol = Convert.ToInt32(clickPoint.Y) / ratio;

            if (spawnGlider)
            {
                placeFigure(rij, kol, glider, 3, 3);
            }
            else
            {
                grid[rij, kol].status = !grid[rij, kol].status;
                if (grid[rij, kol].status)
                {
                    bmp.SetPixel(rij, kol, Colors.Black);
                }
                else
                {
                    bmp.SetPixel(rij, kol, Colors.White);
                }
            }
        }

        //TRIGGERS WHEN THE PAUSE/UNPAUSE BUTTON GETS CLICKED 
        private void pause_unpause_Click(object sender, RoutedEventArgs e)
        {
            paused = !paused;
            if (paused)
            {
                Pause();
            }
            else
            {
                Unpause();
            }
        }

        //TRIGGERS WHEN THE SAVE BUTTON IS PRESSED
        private void save_button_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        //TRIGGERS WHEN THE LOAD BUTTON IS PRESSED
        private void load_button_Click(object sender, RoutedEventArgs e)
        {
            Load();
        }

        //TRIGGERS WHEN THE SIZE BOX LOST FOCUS
        private void width_box_LostFocus(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).Text == "")
            {
                ((TextBox)sender).Text = Convert.ToString(width);
            }
            change_size(Convert.ToInt32(((TextBox)sender).Text));
        }

        //TRIGGERS WHEN ANY INPUT HAPPENS IN THE SIZE BOX
        private void width_box_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (((TextBox)sender).Text == "")
                {
                    ((TextBox)sender).Text = Convert.ToString(width);
                }
                change_size(Convert.ToInt32(((TextBox)sender).Text));
            }
        }

        //TRIGGERS WHEN THE CHANGE TIME BOX LOST FOCUS
        private void timeBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).Text == "")
            {
                ((TextBox)sender).Text = Convert.ToString(timer.Interval);
            }
            changeTimeInterval(Convert.ToDouble(((TextBox)sender).Text));
        }

        //TRIGGERS WHEN ANY INPUT HAPPENS IN THE CHANGE TIME BOX
        private void timeBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (((TextBox)sender).Text == "")
                {
                    ((TextBox)sender).Text = Convert.ToString(timer.Interval);
                }
                changeTimeInterval(Convert.ToDouble(((TextBox)sender).Text));
            }
        }

        //TRIGGERS WHEN THE SPAWN GLIDER BUTTON GETS CLICKED
        private void gliderSpawn_Click(object sender, RoutedEventArgs e)
        {
            if (spawnGlider)
            {
                spawnGlider = false;
                ((Button)sender).Background = Brushes.LightGray;
            }
            else
            {
                spawnGlider = true;
                ((Button)sender).Background = Brushes.LightBlue;
            }
        }

        //TRIGGERS WHEN THE CLEAR BUTTON GETS CLICKED
        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            clear();
        }


        private void timeBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+"); //REGEX EXPESSION TO ONLY ACCEPT NUMBERS
            e.Handled = regex.IsMatch(e.Text);
        }

        private void height_box_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+"); //REGEX EXPESSION TO ONLY ACCEPT NUMBERS
            e.Handled = regex.IsMatch(e.Text);
        }

        private void width_box_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //SHOWS OR HIDES THE GRID
        private void SetGridbtn_Click(object sender, RoutedEventArgs e)
        {
            showGrid = !showGrid;
            if (showGrid)
            {
                SetGridbtn.Content = "Hide grid";
                drawGrid();
            }
            else
            {
                MyCanvas.Children.Clear();
                SetGridbtn.Content = "Show grid";
            }
        }


        //RANDOMIZES THE GRID
        //TRIGGERS WHEN THE RANDOMIZE BUTTON GETS CLICKED
        private void randomize_btn_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            for (int rij = 0; rij < width; rij++)
            {
                for (int kol = 0; kol < height; kol++)
                {
                    if (random.NextInt64(0, 2) == 0)
                    {
                        grid[rij, kol].status = true;
                        bmp.SetPixel(rij, kol, Colors.Black);
                    }
                    else
                    {
                        grid[rij, kol].status = false;
                        bmp.SetPixel(rij, kol, Colors.White);
                    }
                }
            }
        }
    }
}