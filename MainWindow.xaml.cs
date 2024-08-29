using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SP05_Semaphore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private ObservableCollection<Thread> createdThreads;

        public ObservableCollection<Thread> CreatedThreads
        {
            get { return createdThreads; }
            set { createdThreads = value; OnPropertyChanged(nameof(createdThreads)); }
        }

        private ObservableCollection<Thread> workingThreads;

        public ObservableCollection<Thread> WorkingThreads
        {
            get { return workingThreads; }
            set { workingThreads = value; OnPropertyChanged(nameof(workingThreads)); }
        }
        private ObservableCollection<Thread> waitingThreads;

        public ObservableCollection<Thread> WaitingThreads
        {
            get { return waitingThreads; }
            set { waitingThreads = value; OnPropertyChanged(nameof(waitingThreads)); }
        }

        private Thread selectedCreatedThread;

        public Thread SelectedCreatedThread
        {
            get { return selectedCreatedThread; }
            set { selectedCreatedThread = value; OnPropertyChanged(nameof(selectedCreatedThread)); }
        }
        private Thread selectedWaitingThread;

        public Thread SelectedWaitingThread
        {
            get { return selectedWaitingThread; }
            set { selectedWaitingThread = value; OnPropertyChanged(nameof(selectedWaitingThread)); }
        }
        public int currentThreadId { get; set; } = 0;

        private int sempahoreMaxNumber = 3;
        public int SempahoreMaxNumber
        {
            get { return sempahoreMaxNumber; }
            set { sempahoreMaxNumber = value; OnPropertyChanged(nameof(sempahoreMaxNumber)); SS = new SemaphoreSlim(sempahoreMaxNumber, sempahoreMaxNumber); }
        }
        private SemaphoreSlim SS { get; set; }


        public MainWindow()
        {
            SS = new SemaphoreSlim(3, 3);
            CreatedThreads = new ObservableCollection<Thread>();
            workingThreads = new ObservableCollection<Thread>();
            waitingThreads = new ObservableCollection<Thread>();
            InitializeComponent();
            DataContext = this;

        }


        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void Created_Threads_LV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {

                if (Created_Threads_LV.SelectedItem != null)
                {
                    Thread tempThread = SelectedCreatedThread;
                    createdThreads.RemoveAt(Created_Threads_LV.SelectedIndex);
                    tempThread.Start();
                    if (Semaphore_Place_Number_TB.IsEnabled == true)
                    {
                        Dispatcher.Invoke(() =>
                        {

                            Thread disableButton = new Thread(DisableButtons);
                            disableButton.IsBackground = true;
                            disableButton.Start();
                        });
                    }


                }
            }
            catch (Exception ex)
            {

                Dispatcher.Invoke(() => { MessageBox.Show(ex.Message); });
            }
        }
        private void WAITING_Threads_LV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (WAITING_Threads_LV.SelectedItem != null)
            {
                Thread tempThread = SelectedWaitingThread;
                waitingThreads.RemoveAt(WAITING_Threads_LV.SelectedIndex);
                tempThread.Abort();


            }
        }


        private void Create_Button_Click(object sender, RoutedEventArgs e)
        {
            Thread TempTHread = new Thread(MainMethod);
            TempTHread.IsBackground = true;
            TempTHread.Name = $"Thread:{++currentThreadId}";
            createdThreads.Add(TempTHread);
        }


        void DisableButtons()
        {
            SemaphoreSlim s = new SemaphoreSlim(1, 1);
            s.Wait();
            Thread.Sleep(1);
            while (workingThreads.Count > 0 || waitingThreads.Count > 0)
            {
                Dispatcher.Invoke(() =>
                {

                    Increment_Semaphore_Place_Button.IsEnabled = false;
                    Decrement_Semaphore_Place_Button.IsEnabled = false;
                    Semaphore_Place_Number_TB.IsEnabled = false;

                });
            }
            Dispatcher.Invoke(() =>
            {

                Increment_Semaphore_Place_Button.IsEnabled = true;
                Decrement_Semaphore_Place_Button.IsEnabled = true;
                Semaphore_Place_Number_TB.IsEnabled = true;
            });
            s.Release();
        }
        public void MainMethod(object state)
        {
            bool st = false;
            while (!st)
            {
                if (SS.Wait(200))
                {

                    try
                    {


                        Thread tempThread = Thread.CurrentThread;
                        if (waitingThreads.FirstOrDefault(t => t == tempThread) == null)
                        {


                            Dispatcher.Invoke(() =>
                            {
                                WorkingThreads.Add(tempThread);
                            });
                        }
                        else
                        {
                            Dispatcher.Invoke(() =>
                            {


                                waitingThreads.Remove(tempThread);
                                WorkingThreads.Add(tempThread);
                            });
                        }

                        Thread.Sleep(5000);
                    }
                    finally
                    {

                        Thread tempThread = Thread.CurrentThread;
                        Dispatcher.Invoke(() =>
                        {

                            st = true;
                            WorkingThreads.Remove(tempThread);
                            SS.Release();
                        });

                    }
                }
                else
                {
                    Thread tempthread = Thread.CurrentThread;
                    Dispatcher.Invoke(() =>
                    {

                        if (!(waitingThreads.IndexOf(tempthread) > -1))
                            WaitingThreads.Add(tempthread);
                    });
                }
            }

        }

        private void Increment_Semaphore_Place_Button_Click(object sender, RoutedEventArgs e)
        {

            int num1 = int.Parse(Semaphore_Place_Number_TB.Text);
            ++num1;
            Semaphore_Place_Number_TB.Text = num1.ToString();
            SS = new SemaphoreSlim(num1, num1);
        }

        private void Decrement_Semaphore_Place_Button_Click(object sender, RoutedEventArgs e)
        {
            int num1 = int.Parse(Semaphore_Place_Number_TB.Text);
            if (num1 > 1)
            {

                --num1;
                Semaphore_Place_Number_TB.Text = num1.ToString();
                SS = new SemaphoreSlim(num1, num1);
            }
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }


}
