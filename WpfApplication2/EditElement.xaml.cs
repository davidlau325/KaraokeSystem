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
    /// Interaction logic for EditElement.xaml
    /// </summary>
    public partial class EditElement : UserControl
    {
        public EditElement()
        {
            InitializeComponent();
        }

        private void Edit_btn_Click(object sender, RoutedEventArgs e)
        {
            MediaManagement mm = new MediaManagement();
            mm.importInfo();
            mm.editInformation(this.MediaPath.Text, this.MediaName.Text, this.MediaAuthor.Text, this.MediaAlbum.Text);
            mm.writeToInfo();
            App.reloadMediaList("");

            this.ElementGrid.Children.Clear();
            this.ElementGrid.Background = null;
        }

        private void Edit_btn_Click_1(object sender, RoutedEventArgs e)
        {
            this.ElementGrid.Children.Clear();
            this.ElementGrid.Background = null;
        }
    }
}
