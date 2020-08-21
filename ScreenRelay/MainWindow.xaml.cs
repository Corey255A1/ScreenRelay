//WunderVision 2020
//https://www.wundervisionenvisionthefuture.com/
//Display Monitor Thumbnails and Update a realtime Image of the selected monitor

using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ScreenRelay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify([CallerMemberName]string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private ObservableCollection<Screen> _screens;

        public ObservableCollection<Screen> Screens
        {
            get { return _screens; }
            set { _screens = value; }
        }

        private Screen _selected_screen;

        public Screen SelectedScreen
        {
            get { return _selected_screen; }
            set { _selected_screen = value; Notify(); }
        }

        private bool _screen_listview_hidden = false;
        public bool ScreenListViewHidden
        {
            get => _screen_listview_hidden;
            set{ _screen_listview_hidden = value; Notify(); Notify(nameof(ScreenListViewHiddenStr)); }
        }

        public string ScreenListViewHiddenStr => _screen_listview_hidden ? "Show" : "Collapse";

        DispatcherTimer _dispatcher_timer;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _screens = new ObservableCollection<Screen>();
            try
            {
                ScreenFinder.GetMonitors().ForEach(screen => { screen.UpdateImage(); _screens.Add(screen); });
            }
            catch(Exception e)
            {
                Console.WriteLine("WTF");
            }
            SelectedScreen = _screens[0];


            _dispatcher_timer = new DispatcherTimer();
            _dispatcher_timer.Tick += new EventHandler((o, e) => _selected_screen.UpdateImage());
            _dispatcher_timer.Interval = new TimeSpan(0, 0, 0, 0, 50);//20Hz
            _dispatcher_timer.Start();
        }

        private void ScreenListViewCollapse_Click(object sender, RoutedEventArgs e)
        {
            ScreenListViewHidden = !ScreenListViewHidden;
            screenListView.Visibility = _screen_listview_hidden ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
