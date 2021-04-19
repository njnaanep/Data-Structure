using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Naanep_FExer01
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region DataSource
        public static List<string> InstructionsList = new List<string>();
        public static Stack<string> PrevInstructionStack = new Stack<string>();

        public static Stack<Rectangle> LeftPegStack = new Stack<Rectangle>();
        public static Stack<Rectangle> MidPegStack = new Stack<Rectangle>();
        public static Stack<Rectangle> RightPegStack = new Stack<Rectangle>();
        #endregion


        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnSolve_Click(object sender, RoutedEventArgs e)
        {
            DisableTextChange(true);

            if (CmbStartPeg.SelectedIndex==CmbEndPeg.SelectedIndex)
            {
                MessageBox.Show("Starting Peg and Ending Peg should not be the same");
                return;
            }
            InstructionsList.Clear();
            var tmpPeg = "";

            if ((CmbStartPeg.SelectedIndex==0 && CmbEndPeg.SelectedIndex==2) || (CmbStartPeg.SelectedIndex == 2 && CmbEndPeg.SelectedIndex == 0))
                tmpPeg = TxtMidPeg.Text;

            if ((CmbStartPeg.SelectedIndex == 0 && CmbEndPeg.SelectedIndex == 1) || (CmbStartPeg.SelectedIndex == 1 && CmbEndPeg.SelectedIndex == 0))
                tmpPeg = TxtRightPeg.Text;

            if ((CmbStartPeg.SelectedIndex == 1 && CmbEndPeg.SelectedIndex == 2) || (CmbStartPeg.SelectedIndex == 2 && CmbEndPeg.SelectedIndex == 1))
                tmpPeg = TxtLeftPeg.Text;

            GenerateInstructions(Convert.ToInt32(TxtNumOfDisk.Text), CmbStartPeg.Text, CmbEndPeg.Text, tmpPeg);

            LstInstruction.ItemsSource = InstructionsList;
            LstInstruction.Items.Refresh();


            //if (ChkAutomatic.IsChecked == true) AutomaticSolver();

            DisableActionButtons(false);
            BtnPrevAction.IsEnabled = false;
            BtnRestartAction.IsEnabled = false;
        }

        private async Task AutomaticSolver()
        {
            for (var index = 0; index < InstructionsList.Count; index++)
            {
                await Task.Delay(150);
                LstInstruction.SelectedIndex = index;
                MoveDisk(true, BlkCurrentExecution.Text);
                //Thread.Sleep(1000);
            }
        }

        private void SldNumOfDisk_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtNumOfDisk == null) return; //prevent error on startup

            try
            {
                GenerateDisks(Convert.ToInt32(TxtNumOfDisk.Text), CmbStartPeg.SelectedIndex);
                PrevInstructionStack.Clear();

                InstructionsList.Clear();
                LstInstruction.Items.Refresh();

                DisableActionButtons(true);
            }
            catch (Exception)
            {
                return;
                //MessageBox.Show(exception.ToString());
            }
        }

        

        private void CmbStartPeg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TxtNumOfDisk == null) return;
            GenerateDisks(Convert.ToInt32(TxtNumOfDisk.Text), CmbStartPeg.SelectedIndex);
            InstructionsList.Clear();
            LstInstruction.Items.Refresh();


            DisableActionButtons(true);
        }

        private void CmbEndPeg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstInstruction==null) return;
            InstructionsList.Clear();
            LstInstruction.Items.Refresh();

            DisableActionButtons(true);
        }

        private void TxtNumOfDisk_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text); //text box only accept numbers
        }

        #region InstructionCommand

        private void BtnNextAction_Click(object sender, RoutedEventArgs e)
        {
            BtnPrevAction.IsEnabled = true;
            BtnRestartAction.IsEnabled = true;

            LstInstruction.SelectedIndex += 1;
            LstInstruction.ScrollIntoView(LstInstruction.SelectedItem);

            if (BlkCurrentExecution == null) return;

            MoveDisk(true, BlkCurrentExecution.Text);

            if (LstInstruction.SelectedIndex == LstInstruction.Items.Count - 1)
            {
                BtnNextAction.IsEnabled = false;
                MessageBox.Show("Solved!");
            }
        }

        private void BtnPrevAction_Click(object sender, RoutedEventArgs e)
        {
            if (LstInstruction.SelectedIndex == -1)
            {
                BtnPrevAction.IsEnabled = false;
                MessageBox.Show("Starting Execution");
                return;
            }

            BtnNextAction.IsEnabled = true;

            LstInstruction.SelectedIndex -= 1;
            LstInstruction.ScrollIntoView(LstInstruction.SelectedItem);

            if (BlkCurrentExecution == null) return;

            MoveDisk(false);

            if (LstInstruction.SelectedIndex == -1) BtnPrevAction.IsEnabled = false;
        }

        private void BtnRestartAction_Click(object sender, RoutedEventArgs e)
        {
            if (InstructionsList.Count!=0)
            {
                LstInstruction.SelectedIndex = -1;

                BtnPrevAction.IsEnabled = false;
                BtnNextAction.IsEnabled = true;

                LstInstruction.ScrollIntoView(LstInstruction.SelectedIndex);

                GenerateDisks(Convert.ToInt32(TxtNumOfDisk.Text), CmbStartPeg.SelectedIndex);
                DisableTextChange(true);
                PrevInstructionStack.Clear();
            }

        }

        #endregion

        #region Methods

        private void GenerateInstructions(int numOfDisk, string startPeg, string endPeg, string tempPeg)
        {
            if (numOfDisk > 0)
            {
                GenerateInstructions(numOfDisk - 1, startPeg, tempPeg, endPeg);
                InstructionsList.Add($"{InstructionsList.Count+1})Move disk {numOfDisk} from {startPeg} to {endPeg}");
                GenerateInstructions(numOfDisk - 1, tempPeg, endPeg, startPeg);
            }
        }

        private void GenerateDisks(int numOfDisc,int pegIndex ,int width = 240)
        {
            BtnSolve.IsEnabled = true;
            DisableTextChange(false);

            //clear all list box for new disks
            LeftPegStack.Clear();
            MidPegStack.Clear();
            RightPegStack.Clear();
            var colors = new SolidColorBrush[]
            {
                Brushes.Indigo,
                Brushes.DarkSlateBlue,
                Brushes.BlueViolet,
                Brushes.Blue,
                Brushes.MediumSlateBlue,
                Brushes.RoyalBlue,
                Brushes.DodgerBlue,
                Brushes.SteelBlue,
                Brushes.Cyan,
                Brushes.MediumTurquoise,
                Brushes.PaleTurquoise,
                Brushes.LightSkyBlue,
            };

            //colors for disk from 
            switch (pegIndex)
            {
                case 0: //left peg
                {
                    for (var i = 0; i < numOfDisc; i++)
                    {
                        LeftPegStack.Push(new Rectangle {Width = width, Fill = colors[i] });
                        width -= 15;
                    }
                    break;
                }
                case 1: //mid peg
                {
                    for (var i = 0; i < numOfDisc; i++)
                    {
                        MidPegStack.Push(new Rectangle {Width = width, Fill = colors[i] });
                        width -= 15;
                    }
                    break;
                }
                case 2: //right peg
                {
                    for (var i = 0; i < numOfDisc; i++)
                    {
                        RightPegStack.Push(new Rectangle {Width = width, Fill = colors[i] });
                        width -= 15;
                    }
                    break;
                }
            }

            //update Peg Listbox
            LsBoxLeftPeg.ItemsSource = LeftPegStack;
            LsBoxMidPeg.ItemsSource = MidPegStack;
            LsBoxRightPeg.ItemsSource = RightPegStack;

            RefreshPegsItems();
            LstInstruction.Items.Refresh();
        }

        //optimized code
        private void MoveDisk(bool movement,string currentInstruction="")
        {
            string moveFrom;
            string moveTo;
            string[] instruction;

            if (movement) //next instruction
            {
                PrevInstructionStack.Push(currentInstruction);
                instruction = currentInstruction.Split(' ');

                moveFrom = instruction[4];
                moveTo = instruction[6];
            }
            else //prev instruction
            {
                instruction = PrevInstructionStack.Pop().Split(' ');

                moveFrom = instruction[6];
                moveTo = instruction[4];
            }

            if (moveFrom == TxtLeftPeg.Text)
            {
                if (moveTo == TxtMidPeg.Text)
                    MidPegStack.Push(LeftPegStack.Pop());
                if (moveTo == TxtRightPeg.Text)
                    RightPegStack.Push(LeftPegStack.Pop());
            }
            if (moveFrom == TxtMidPeg.Text)
            {
                if (moveTo == TxtLeftPeg.Text)
                    LeftPegStack.Push(MidPegStack.Pop());
                if (moveTo == TxtRightPeg.Text)
                    RightPegStack.Push(MidPegStack.Pop());
            }
            if (moveFrom == TxtRightPeg.Text)
            {
                if (moveTo == TxtLeftPeg.Text)
                    LeftPegStack.Push(RightPegStack.Pop());
                if (moveTo == TxtMidPeg.Text)
                    MidPegStack.Push(RightPegStack.Pop());
            }

            //update Peg Listbox
            RefreshPegsItems();
        }

        /// <summary>
        /// Disable or enable clicking of action button
        /// </summary>
        /// <param name="disabled">if true disable if false, otherwise</param>
        private void DisableActionButtons(bool disabled)
        {
            if (disabled)
            {
                BtnNextAction.IsEnabled = false;
                BtnPrevAction.IsEnabled = false;
                BtnRestartAction.IsEnabled = false;
            }
            else
            {
                BtnNextAction.IsEnabled = true;
                BtnPrevAction.IsEnabled = true;
                BtnRestartAction.IsEnabled = true;
            }
        }

        /// <summary>
        /// Disable or enable editing of peg name
        /// </summary>
        /// <param name="disabled">if true disable, otherwise if false</param>
        private void DisableTextChange(bool disabled)
        {
            if (disabled)
            {
                TxtLeftPeg.IsReadOnly = true;
                TxtMidPeg.IsReadOnly = true;
                TxtRightPeg.IsReadOnly = true;
            }
            else
            {
                TxtLeftPeg.IsReadOnly = false;
                TxtMidPeg.IsReadOnly = false;
                TxtRightPeg.IsReadOnly = false;
            }
        }

        private void RefreshPegsItems()
        {
            LsBoxLeftPeg.Items.Refresh();
            LsBoxMidPeg.Items.Refresh();
            LsBoxRightPeg.Items.Refresh();
        }



        #endregion

        private void ChkManual_Checked(object sender, RoutedEventArgs e)
        {
            if (ChkAutomatic==null) return;
            ChkAutomatic.IsChecked = false;
        }

        private void ChkAutomatic_Checked(object sender, RoutedEventArgs e)
        {
            ChkManual.IsChecked = false;
        }
    }
}
