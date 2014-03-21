using System;
using System.IO;
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
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel;

namespace KaraokeSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>/// 

    public partial class MainWindow
    {
        //private static System.Threading.Thread wavPlayThread;
        private static bool isAlreadyPause;
        private static int playFileType; // 1 for wav, 0 for otherwise, -1 for haven't play
        private static bool isPlayingVideo;
        private static string currentMediaPath;
        private static string currentMediaName;
        public static MainWindow mainWindow;
        private int result;
        private static bool hasLyrics;

        private int timeOffset = 0;
        private DateTime timeStart;
        private Dictionary<TimeSpan, string> dict;
        private DispatcherTimer timer;
        private DispatcherTimer timerVideoTime;

        private bool isStop;
        private static bool isRepeat;
        private static bool isSuffle;

        public MainWindow()
        {
            InitializeComponent();
            isAlreadyPause = false;
            hasLyrics = false;
            isRepeat = false;
            isSuffle = false;
            isPlayingVideo = false;
            isStop = false;
            playFileType = -1;
        }

        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(MainWindow), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));

        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            mainWindow = o as MainWindow;
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

        private void Window_Drop(object sender, DragEventArgs e) 
        {
            string[] droppedFiles = null;
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) 
            {
                droppedFiles = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            }

            if ((null == droppedFiles) || (!droppedFiles.Any())) { return; }

            foreach (string s in droppedFiles) {
                Add_New_Media_Helper(s);
            }
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
                Add_New_Media_Helper(filename);
            }
        }

        private void Add_New_Media_Helper(string filename) {
            MediaManagement mm = new MediaManagement();
            mm.importInfo();
            if (mm.IsExistMedia(filename))
            {
                string messageBoxText = filename + " - You had already added this media file!";
                string caption = "Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
            }
            else
            {
                mm.addNewMedia(filename);
                mm.writeToInfo();
                App.reloadMediaList("");

                string messageBoxText = filename + " - New media added successfully!";
                string caption = "Success";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
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
                isRepeat = false;
            }
            else
            {
                this.Repeat_btn.Background = Brushes.CadetBlue;
                isRepeat = true;
            }
        }

        private void Suffle_btn_Click(object sender, RoutedEventArgs e)
        {
            if (this.Suffle_btn.Background == Brushes.CadetBlue)
            {
                var bc = new BrushConverter();
                this.Suffle_btn.Background = (Brush)bc.ConvertFrom("#FFCCCCCC");
                isSuffle = false;
            }
            else
            {
                this.Suffle_btn.Background = Brushes.CadetBlue;
                isSuffle = true;
            } 
        }

        private void mediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            timelineSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            if (timerVideoTime != null)
            {
                timerVideoTime.Stop();
            }
            timerVideoTime = new DispatcherTimer();
            timerVideoTime.Interval = TimeSpan.FromSeconds(1);
            timerVideoTime.Tick += new EventHandler(timer_Tick_player);
            timerVideoTime.Start();

            timelineSlider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(timelineSlider_MouseLeftButtonUp), true);
        }

        void timer_Tick_player(object sender, EventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                if (mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds > 0)
                {
                    timelineSlider.Value = mediaPlayer.Position.TotalSeconds;
                }
            }
        }

        private void Play_btn_Click(object sender, RoutedEventArgs e)
        {
                if (playFileType == 1)
                {
                    if (isAlreadyPause == true)
                    {
                        WavePlayerLib.ResumeWave();
                        NowPlayingMedia.Text = "Now Playing: " + currentMediaName;
                        Play_btn.Visibility = Visibility.Hidden;
                        Pause_btn.Visibility = Visibility.Visible;
                        if (hasLyrics)
                        {
                            this.timer.Start();
                        }
                    }
                    else 
                    {
                        playWaveFile(currentMediaName, currentMediaPath);
                    }
                }
                else if(playFileType == 0)
                {
                    if (isAlreadyPause == true)
                    {
                        mediaPlayer.Play();
                        NowPlayingMedia.Text = "Now Playing: " + currentMediaName;
                        Play_btn.Visibility = Visibility.Hidden;
                        Pause_btn.Visibility = Visibility.Visible;
                        if (hasLyrics)
                        {
                            this.timer.Start();
                        }
                    }
                    else 
                    {
                        playNonWaveFile(currentMediaName, currentMediaPath);
                    }
                }else{
                string messageBoxText = "Please select a media to play.";
                string caption = "Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
                }
        }

        private void Stop_btn_Click(object sender, RoutedEventArgs e)
        {
                if (playFileType == 1)
                {
                    isStop = true;
                    WavePlayerLib.StopWave();
                    NowPlayingMedia.Text = "Stop";
                    Play_btn.Visibility = Visibility.Visible;
                    Pause_btn.Visibility = Visibility.Hidden;
                    if (hasLyrics) {
                        stopLyrics();
                    }
                    timer.Stop();
                    timelineSlider.RemoveHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(timelineSlider_MouseLeftButtonUp));
                    timelineSlider.Value = 0;
                    isAlreadyPause = false;
                }
                else if(playFileType == 0)
                {
                    mediaPlayer.Stop();
                    NowPlayingMedia.Text = "Stop";
                    Play_btn.Visibility = Visibility.Visible;
                    Pause_btn.Visibility = Visibility.Hidden;
                    if (hasLyrics)
                    {
                        stopLyrics();
                    }
                    timelineSlider.RemoveHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(timelineSlider_MouseLeftButtonUp));
                    timelineSlider.Value = 0;
                    isAlreadyPause = false;
                }else{
                 string messageBoxText = "Please select a media to play.";
                string caption = "Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
                }
        }

        private void Volume_Slide_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (playFileType == 1)
            {
                int vol = (int)(Volume_Slider.Value * 100);
                WavePlayerLib.setVolume(vol);
            }
            else if (playFileType == 0)
            {
                try
                {
                    mediaPlayer.Volume = (double)Volume_Slider.Value;
                }
                catch (NullReferenceException)
                {

                }
            }
        }

        public void playNonWaveFile(string mediaName, string mediaPath) {
            playVideoFile(mediaName, mediaPath,1);
            hasLyrics = load_lyrics(mediaPath);
            if (hasLyrics == true)
            {
                this.timeStart = DateTime.Now;
                this.timer.Start();
            }
            isPlayingVideo = false;
        }

        public void playVideoFile(string mediaName, string mediaPath,int fn) 
        {
            currentMediaName = mediaName;
            currentMediaPath = mediaPath;

            stopPlaying();
            playFileType = 0;
            App.nowPlaying = mediaPath;
            App.reloadMediaList("");
            NowPlayingMedia.Text = "Now Playing: " + mediaName;

            Play_btn.Visibility = Visibility.Hidden;
            Pause_btn.Visibility = Visibility.Visible;

            mediaPlayer.Source = new Uri(mediaPath);
            mediaPlayer.Volume = (double)Volume_Slider.Value;
            mediaPlayer.Play();
            mediaPlayer.Visibility = Visibility.Visible;
            if (fn == 0) {
                sp_lyric.Children.Clear();
            }
            isPlayingVideo = true;
        }

        private void stopPlaying() {
            if (playFileType == 1 && isAlreadyPause == true)
            {
                WavePlayerLib.ResumeWave();
                WavePlayerLib.StopWave();
               // wavPlayThread.Abort();
            }
            else if (playFileType == 1)
            {
                WavePlayerLib.StopWave();
             //   wavPlayThread.Abort();
            }
            else if (playFileType == 0)
            {
                mediaPlayer.Stop();
                mediaPlayer.Close();
                mediaPlayer.Visibility = Visibility.Hidden;
            }
            timelineSlider.RemoveHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(timelineSlider_MouseLeftButtonUp));
            timelineSlider.Value = 0;
            if (hasLyrics) {
                stopLyrics();
            }
        }

        private void stopLyrics() {
            this.timer.Stop();
            this.timeOffset = 0;
            sv_lyric.ScrollToVerticalOffset(0);
            foreach (UIElement element in sp_lyric.Children)
            {
                if (element is TextBlock)
                {
                    ((TextBlock)element).Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 128));
                }
            }
        }

        private void sliderWave() {
            Thread.Sleep(1000);
            int totalDuration = WavePlayerLib.totalDuration();
            timelineSlider.Maximum = (totalDuration - 1);
            if (timerVideoTime != null) {
                timerVideoTime.Stop();
            }
            timerVideoTime = new DispatcherTimer();
            timerVideoTime.Interval = TimeSpan.FromSeconds(1);
            timerVideoTime.Tick += new EventHandler(timer_wav);
            timerVideoTime.Start();

           timelineSlider.RemoveHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(timelineSlider_MouseLeftButtonUp));
           timelineSlider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(timelineSlider_MouseLeftButtonUp), true);
        }

        void timer_wav(object sender, EventArgs e)
        {  
                timelineSlider.Value = WavePlayerLib.currentSec();
        }
        

        private void startWavThread(StringBuilder waveName) {
            var bw = new BackgroundWorker();

            bw.DoWork += (sender, args) =>
            {
                result = WavePlayerLib.PlayWave(waveName);
            };
            bw.RunWorkerCompleted += (sender, args) =>
            {
                if (!isStop)
                {
                    mediaEnd(0);
                }
            };
            bw.RunWorkerAsync();

        } 

        public void playWaveFile(string mediaName,string mediaPath) {
            StringBuilder waveName = new StringBuilder();
            waveName.Append(mediaPath);

            currentMediaName = mediaName;
            currentMediaPath = mediaPath;
            App.nowPlaying = mediaPath;
            App.reloadMediaList("");
            NowPlayingMedia.Text = "Now Playing: " + mediaName;

            stopPlaying();
            playFileType = 1;

            result = 0;
            /*
             wavPlayThread = new System.Threading.Thread(delegate()
             {
                 result = WavePlayerLib.PlayWave(waveName);
             });  */
            startWavThread(waveName);

             if (result == 0)
             {
                 playFileType = 1;
                 Play_btn.Visibility = Visibility.Hidden;
                 Pause_btn.Visibility = Visibility.Visible;
                 //wavPlayThread.Start();
                 hasLyrics = load_lyrics(mediaPath);
                 isPlayingVideo = false;
                 if (hasLyrics == true)
                 {
                     this.timeStart = DateTime.Now;
                     this.timer.Start();
                 }
                 sliderWave();
             }
             else {
                 string messageBoxText = "The media cannot be opened!";
                 string caption = "Error";
                 MessageBoxButton button = MessageBoxButton.OK;
                 MessageBoxImage icon = MessageBoxImage.Warning;
                 System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
             }
        }

        private void Pause_btn_Click(object sender, RoutedEventArgs e)
        {
            if (playFileType == 1)
            {
                WavePlayerLib.PauseWave();
            }
            else {
                mediaPlayer.Pause();
            }
            if (this.timer != null)
            {
                this.timer.Stop();
            }
            NowPlayingMedia.Text = "Pause";
            isAlreadyPause = true;
            Play_btn.Visibility = Visibility.Visible;
            Pause_btn.Visibility = Visibility.Hidden;
        }

        // Lyrics Control
        private bool load_lyrics(string mediaPath)
        {
            string lrcFile = Path.ChangeExtension(mediaPath, ".lrc");
            this.dict = new Dictionary<TimeSpan, string>();
            this.timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += timer_Tick;

            sp_lyric.Children.Clear();
            string lyric = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, lrcFile);
            if (File.Exists(lyric))
            {
                string[] lines = File.ReadAllLines(lyric, Encoding.UTF8);
                foreach (string line in lines)
                {
                    Regex rgx = new Regex(@"\[(\d\d):(\d\d)\.\d\d\](.*)");
                    if (rgx.IsMatch(line))
                    {
                        Match m = rgx.Match(line);

                        int minutes = int.Parse(m.Groups[1].Value);
                        int seconds = int.Parse(m.Groups[2].Value);

                        TimeSpan span = new TimeSpan(0, minutes, seconds);
                        if (!this.dict.ContainsKey(span))
                        {
                            this.dict.Add(span, m.Groups[3].Value);
                        }

                        TextBlock tb = new TextBlock();
                        tb.Margin = new Thickness(5);
                        tb.Name = String.Format("tb{0:00}m{1:00}s", minutes, seconds);
                        tb.Text = m.Groups[3].Value;
                        tb.TextWrapping = TextWrapping.Wrap;
                        sp_lyric.Children.Add(tb);
                    }
                }
                return true;
            }
            else {
                string messageBoxText = "No Lyrics File Found. Please include it at the same folder as the media file with same name (e.g. abc.mp3 = abc.lrc).";
                string caption = "Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
                return false;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            timeOffset++;
            int minutes = timeOffset / 60;
            int seconds = timeOffset % 60;

            if (timeOffset > 30)
            {
                sv_lyric.ScrollToVerticalOffset(5 * timeOffset - 150);
            }

            TimeSpan ts = new TimeSpan(0, minutes, seconds);
            if (this.dict.ContainsKey(ts))
            {
                foreach (UIElement element in sp_lyric.Children)
                {
                    if (element is TextBlock)
                    {
                        string name = String.Format("tb{0:00}m{1:00}s", minutes, seconds);
                        if (((TextBlock)element).Name == name)
                        {
                           // ((TextBlock)element).Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 192, 255));
                            ((TextBlock)element).Foreground = Brushes.Red;
                            ((TextBlock)element).FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                           // ((TextBlock)element).Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 128));
                            ((TextBlock)element).Foreground = Brushes.Black;
                            ((TextBlock)element).FontWeight = FontWeights.Normal;
                        }
                    }
                }
            }
        }

        private void timelineSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (playFileType == 1)
            {
                int sec = (int)timelineSlider.Value;
                WavePlayerLib.skipTo(sec);
                timeOffset = sec;
            }
            else
            {
                mediaPlayer.Position = TimeSpan.FromSeconds(timelineSlider.Value);
                timeOffset = (int)timelineSlider.Value;
            }
        }

        private void mediaEnd(int previous) {
            if (isRepeat && previous == 0)
            {
                    string ext = Path.GetExtension(currentMediaPath);
                    if (ext.Equals(".wav"))
                    {
                        playWaveFile(currentMediaName, currentMediaPath);
                    }
                    else if (ext.Equals(".wmv"))
                    {
                        playVideoFile(currentMediaName, currentMediaPath, 0);
                    }
                    else
                    {
                        playNonWaveFile(currentMediaName, currentMediaPath);
                    }
            }else{
                    string type = "audio";
                    int suffle = 0;
                    if (isPlayingVideo)
                    {
                        type = "video";
                    }
                    if (isSuffle)
                    {
                        suffle = 1;
                    }

                    string nextMediaPath = App.getNextMedia(type, suffle,previous);
                    string nextMediaName = App.findMediaName(nextMediaPath);
                    string ext = Path.GetExtension(nextMediaPath);
                    if (ext.Equals(".wav"))
                    {
                        playWaveFile(nextMediaName, nextMediaPath);
                    }
                    else if (ext.Equals(".wmv"))
                    {
                        playVideoFile(nextMediaName, nextMediaPath, 0);
                    }
                    else
                    {
                        playNonWaveFile(nextMediaName, nextMediaPath);
                    }
                }
        }

        private void mediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaEnd(0);
        }

        private void Forward_btn_Click(object sender, RoutedEventArgs e)
        {
            mediaEnd(2);
        }

        private void Rewind_btn_Click(object sender, RoutedEventArgs e)
        {
            mediaEnd(1);
        }
    }
}
