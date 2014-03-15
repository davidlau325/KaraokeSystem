using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KaraokeSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static MainWindow mWindow;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            mWindow = new MainWindow();
            Application.Current.MainWindow = mWindow;
            mWindow.Show();
            reloadMediaList("");
        }

        public static void reloadMediaList(string searchString) {
            MediaManagement media = new MediaManagement();
            media.importInfo();

            List<string> mediaPath = media.getFilePath();
            List<string> mediaName = media.getMediaName();
            List<string> mediaAuthor = media.getMediaAuthor();
            List<string> mediaAlbum = media.getMediaAlbum();
            List<string> mediaType = media.getMediaType();

            mWindow.AudioList.Children.Clear();
            mWindow.VideoList.Children.Clear();

            Boolean hasAudio = false;
            Boolean hasVideo = false;
            for (int i = 0; i < mediaType.Count; i++)
            {
                if (mediaType[i].Equals("audio"))
                {
                    if (mediaName[i].Contains(searchString) || mediaPath[i].Contains(searchString) || mediaAuthor[i].Contains(searchString) || mediaAlbum[i].Contains(searchString))
                    {
                        MediaElement newMedia = new MediaElement();
                        newMedia.MediaName.Text = mediaName[i] + " - " + mediaPath[i];
                        newMedia.MediaAuthor.Text = mediaAuthor[i];
                        newMedia.MediaAlbum.Text = mediaAlbum[i];
                        mWindow.AudioList.Children.Add(newMedia);
                        hasAudio = true;
                    }
                }
                else
                {
                    if (mediaName[i].Contains(searchString) || mediaPath[i].Contains(searchString) || mediaAuthor[i].Contains(searchString) || mediaAlbum[i].Contains(searchString))
                    {
                        MediaElement newMedia = new MediaElement();
                        newMedia.MediaName.Text = mediaName[i] + " - " + mediaPath[i];
                        newMedia.MediaAuthor.Text = mediaAuthor[i];
                        newMedia.MediaAlbum.Text = mediaAlbum[i];
                        mWindow.VideoList.Children.Add(newMedia);
                        hasVideo = true;
                    }
                }
            }

            if (hasAudio == false)
            {
                EmplyElement newEmply = new EmplyElement();
                newEmply.ErrorInfo.Text = "No Audio File Found...";
                mWindow.AudioList.Children.Add(newEmply);
            }
            if (hasVideo == false)
            {
                EmplyElement newEmply = new EmplyElement();
                newEmply.ErrorInfo.Text = "No Video File Found...";
                mWindow.VideoList.Children.Add(newEmply);
            }
        }
    }
}
