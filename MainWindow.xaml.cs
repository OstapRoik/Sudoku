using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;

namespace Game
{
    public partial class MainWindow : Window
    {
        private readonly int LevelSize = 9;
        private readonly int UserFontSize = 22;
        GridForSudoku Sudoku;
        private Stopwatch Timer = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_Initialized(object sender, EventArgs e)
        {
            for (int i = 0; i < LevelSize; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(1.0, GridUnitType.Star);
                Grid.RowDefinitions.Add(row);

                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(1.0, GridUnitType.Star);
                Grid.ColumnDefinitions.Add(col);
            }

            int Size = LevelSize * LevelSize;
            Label[] labels = new Label[Size];
            for (int i = 0, j = 0; i < Size; i++)
            {
                labels[i] = new Label();
                labels[i].Name = "label" + i.ToString();
                labels[i].VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                labels[i].HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                labels[i].FontSize = UserFontSize;
                Grid.Children.Add(labels[i]);
                Grid.SetColumn(labels[i], i % LevelSize);
                Grid.SetRow(labels[i], j);
                if (i % LevelSize == LevelSize - 1 && i != 0)
                    j++;
                labels[i].MouseEnter += Label_MouseEnter;
                labels[i].MouseLeave += Label_MouseLeave;
                labels[i].MouseDown += Label_MouseDown;

                int SqrtLevel = (int)Math.Sqrt(LevelSize);
                int PowLevel = SqrtLevel * SqrtLevel * SqrtLevel;
                labels[i].BorderBrush = Brushes.Black;
                labels[i].BorderThickness = new Thickness(
                    i % SqrtLevel == 0 ? 1 : 0,
                    ((i % PowLevel) >= 0 && (i % PowLevel) < LevelSize) ? 1 : 0,
                    (i + 1) % SqrtLevel == 0 ? 1 : 0,
                    ((i % PowLevel) >= (PowLevel - LevelSize) && (i % PowLevel) < PowLevel) ? 1 : 0
                    );
            }
        }

        private void GridNumber_Initialized(object sender, EventArgs e)
        {
            foreach (var item in GridNumber.Children)
            {
                ((Button)item).IsEnabled = false;
            }
        }

        private void Complexity_MouseEnter(object sender, MouseEventArgs e)
        {
            StartButton.IsEnabled = false;
        }

        private void Label_MouseEnter(object sender, RoutedEventArgs e)
        {
            if(((Label)sender).Background != Brushes.DarkGray)
                ((Label)sender).Background = Brushes.LightGray;
        }

        private void Complexity_MouseLeave(object sender, MouseEventArgs e)
        {
            StartButton.IsEnabled = true;
        }

        private void Label_MouseLeave(object sender, RoutedEventArgs e)
        {
            if (((Label)sender).Background != Brushes.DarkGray)
                ((Label)sender).Background = Brushes.Gray;
        }

        private void Label_MouseDown(object sender, RoutedEventArgs e)
        {
            foreach (var item in Grid.Children)
            {
                if (item is Label)
                {
                    ((Label)item).Background = Brushes.Gray;
                }
            }

            ((Label)sender).Background = Brushes.DarkGray;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Sudoku = new GridForSudoku(LevelSize, (int)Complexity.Value);
            Timer.Restart();

            int[][] Level = Sudoku.Value;
            int i = 0, j = 0;
            foreach (var item in Grid.Children)
            {
                if(item is Label)
                {
                    ((Label)item).Foreground = Brushes.Black;
                    if (Level[i][j % Sudoku.SizeLevel] == 0)
                        ((Label)item).Content = null;
                    else
                    {
                        ((Label)item).Content = Level[i][j % Sudoku.SizeLevel];
                        ((Label)item).Foreground = Brushes.White;
                    }
                    j++;
                    if (j % Sudoku.SizeLevel == 0)
                        i++;
                }
            }

            foreach (var item in GridNumber.Children)
            {
                ((Button)item).IsEnabled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int SelectLabel = 0;
            foreach (var item in Grid.Children)
            {
                if (item is Label)
                {
                    if (((Label)item).Background == Brushes.DarkGray)
                        break;
                    SelectLabel++;
                }
            }
            if (SelectLabel == LevelSize * LevelSize)
                return;
            if (Sudoku.ArrayConstElement[(int)(SelectLabel / Sudoku.SizeLevel), SelectLabel % Sudoku.SizeLevel] == false)
            {
                foreach (var item in Grid.Children)
                {
                    if (item is Label)
                    {
                        if (((Label)item).Name == "label" + SelectLabel.ToString())
                        {
                            ((Label)item).Content = ((Button)sender).Content;
                            break;
                        }
                    }
                }
                Sudoku.ChangeElement(SelectLabel, int.Parse(((Button)sender).Content.ToString()));
            }

            CheckGrid();
        }

        private void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            int SelectLabel = 0;
            foreach (var item in Grid.Children)
            {
                if (item is Label)
                {
                    if (((Label)item).Background == Brushes.DarkGray)
                        break;
                    SelectLabel++;
                }
            }
            if (SelectLabel == LevelSize * LevelSize)
                return;
            if (Sudoku.ArrayConstElement[(int)(SelectLabel / Sudoku.SizeLevel), SelectLabel % Sudoku.SizeLevel] == false)
            {
                foreach (var item in Grid.Children)
                {
                    if (item is Label)
                    {
                        if (((Label)item).Name == "label" + SelectLabel.ToString())
                        {
                            ((Label)item).Content = null;
                            break;
                        }
                    }
                }
                Sudoku.ChangeElement(SelectLabel, 0);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        private void CheckGrid()
        {
            Label[] labels = new Label[Sudoku.SizeLevel * Sudoku.SizeLevel];
            int Count = 0;
            foreach (var item in Grid.Children)
	        {
                labels[Count] = (Label)item;
                Count++;
	        }
            bool[] answerCol = Sudoku.CheckArrayCol;
            bool[] answerRow = Sudoku.CheckArrayRow;
            bool[] answerSquare = Sudoku.CheckArraySquare;
            if(!answerCol.Contains(false))
            {
                if(!answerRow.Contains(false))
                {
                    if(!answerSquare.Contains(false))
                    {
                        Timer.Stop();
                        TimeSpan Time = Timer.Elapsed;
                        string TimerFormat = string.Format("{0:00}:{1:00}:{2:00}", Time.Hours, Time.Minutes, Time.Seconds);
                        MessageBox.Show("Time: " + TimerFormat, "You Win!");
                    }
                }
            }
        }
    }
}