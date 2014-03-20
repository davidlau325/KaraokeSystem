using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;


namespace KaraokeSystem
{
    /// <summary>
    /// Interaction logic for MediaElement.xaml
    /// </summary>
    public partial class MediaElement : UserControl
    {
        public MediaElement()
        {
            InitializeComponent();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            var bc = new BrushConverter();
            this.ElementGrid.Background = (Brush)bc.ConvertFrom("#FF1E608F");
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            var bc = new BrushConverter();
            this.ElementGrid.Background = (Brush)bc.ConvertFrom("#FF1A4A6C");
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string mediaName = this.MediaName.Text;
            string[] data = mediaName.Split(new string[] {" - "},StringSplitOptions.None);
            string messageBoxText = "Do you want to delete " + data[1] + "?";
            string caption = "Word Processor";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

            switch (result)
            { 
                case MessageBoxResult.Yes:
                    MediaManagement mm = new MediaManagement();
                    mm.importInfo();
                    mm.deleteMedia(data[1]);
                    mm.writeToInfo();
                    this.ElementGrid.Children.Clear();
                    App.reloadMediaList("");
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            EditElement ee = new EditElement();
            string name = this.MediaName.Text;
            string[] data = name.Split(new string[] {" - "}, StringSplitOptions.None);

            ee.MediaPath.Text = data[1];
            ee.MediaName.Text = data[0];
            ee.MediaAuthor.Text = this.MediaAuthor.Text;
            ee.MediaAlbum.Text = this.MediaAlbum.Text;

            this.ElementGrid.Children.Add(ee); 
        }

        private void ElementGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string name = this.MediaName.Text;
            string[] data = name.Split(new string[] { " - " }, StringSplitOptions.None);

            string extension = Path.GetExtension(data[1]);
            if (extension.Equals(".wav"))
            {
                MainWindow.mainWindow.playWaveFile(data[0], data[1]);
            }
            else if (extension.Equals(".wmv"))
            {
                MainWindow.mainWindow.playVideoFile(data[0], data[1],0);
            }
            else 
            {
                MainWindow.mainWindow.playNonWaveFile(data[0], data[1]);
            }
        }

    }
}
