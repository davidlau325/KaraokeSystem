using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace KaraokeSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(MainWindow), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));

        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            MainWindow mainWindow = o as MainWindow;
            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double)value);
            else
                return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MainWindow mainWindow = o as MainWindow;
            if (mainWindow != null)
                mainWindow.OnScaleValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual double OnCoerceScaleValue(double value)
        {
            if (double.IsNaN(value))
                return 1.0f;

            value = Math.Max(0.1, value);
            return value;
        }

        protected virtual void OnScaleValueChanged(double oldValue, double newValue)
        {

        }

        public double ScaleValue
        {
            get
            {
                return (double)GetValue(ScaleValueProperty);
            }
            set
            {
                SetValue(ScaleValueProperty, value);
            }
        }

        private void MainGrid_SizeChanged(object sender, EventArgs e)
        {
            CalculateScale();
        }

        private void CalculateScale()
        {
            double yScale = ActualHeight / 500f;
            double xScale = ActualWidth / 750f;
            double value = Math.Min(xScale, yScale);
            ScaleValue = (double)OnCoerceScaleValue(myMainWindow, value);
        }

        private void Add_New_Media(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".wav";
            dlg.Filter = "WAV Files (*.wav)|*.wav|WMA Files (*.wma)|*.wma|MP3 Files (*.mp3)|*.mp3|WMV Video Files (*.wmv)|*.wmv";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                MediaManagement mm = new MediaManagement();
                mm.importInfo();
                mm.addNewMedia(filename);
                mm.writeToInfo();
                App.reloadMediaList("");
            }
        }

        private void SearchAudio_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = this.SearchAudio.Text;
            App.reloadMediaList(searchString);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.Repeat_btn.Background == Brushes.CadetBlue)
            {
                var bc = new BrushConverter();
                this.Repeat_btn.Background = (Brush)bc.ConvertFrom("#FFCCCCCC");
            }
            else
            {
                this.Repeat_btn.Background = Brushes.CadetBlue;
            }
        }

        private void Suffle_btn_Click(object sender, RoutedEventArgs e)
        {
            if (this.Suffle_btn.Background == Brushes.CadetBlue)
            {
                var bc = new BrushConverter();
                this.Suffle_btn.Background = (Brush)bc.ConvertFrom("#FFCCCCCC");
            }
            else
            {
                this.Suffle_btn.Background = Brushes.CadetBlue;
            } 
        }
    }
}
