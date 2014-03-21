using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace KaraokeSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static MainWindow mWindow;
        public static string nowPlaying;
        private static List<string> mediaPath;
        private static List<string> mediaName;
        private static List<string> mediaAuthor;
        private static List<string> mediaAlbum;
        private static List<string> mediaType;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            mWindow = new MainWindow();
            Application.Current.MainWindow = mWindow;
            mWindow.Show();
            nowPlaying = "";
            reloadMediaList("");
        }

        void App_Exit(object sender, ExitEventArgs e) 
        {
            WavePlayerLib.StopWave();
        }

        public static string getNextMedia(string type, int suffle, int previous)
        {
            bool found = false;
            if (previous == 1)
            {
                int ne = 0;
                int i;
                for (i = 0; i < mediaType.Count; i++) { 
                    if(mediaPath[i].Equals(nowPlaying)){
                        break;
                    }
                }
                while (!found)
                {
                    ne = i - 1; 
                    if (ne < 0)
                    {
                        ne = (mediaType.Count - 1);
                    }
                    if (mediaType[ne].Equals(type))
                    {
                        found = true;
                    }
                }
                return mediaPath[ne];
            }else{
                if (suffle == 1)
                {
                    Random rnd = new Random();
                    int suf = 0;
                    while (!found)
                    {
                        suf = rnd.Next(0, mediaType.Count);
                        if (mediaType[suf].Equals(type))
                        {
                            if (!mediaPath[suf].Equals(nowPlaying))
                            {
                                found = true;
                            }
                        }
                    }
                    return mediaPath[suf];
                }
                else
                {
                    for (int i = 0; i < mediaType.Count; i++)
                    {
                        if (found)
                        {
                            if (mediaType[i].Equals(type))
                            {
                                return mediaPath[i];
                            }
                        }
                        else
                        {
                            if (mediaPath[i].Equals(nowPlaying))
                            {
                                found = true;
                            }
                        }
                    }
                    return mediaPath[0];
                }
            }
        }

        public static string findMediaName(string mediaPath) {
            for (int i = 0; i < mediaType.Count; i++)
            {
                    if (mediaPath[i].Equals(mediaPath))
                    {
                        return mediaName[i];
                    }
            }
            return "";
        }

        public static void reloadMediaList(string searchString) {
            MediaManagement media = new MediaManagement();
            media.importInfo();

            mediaPath = media.getFilePath();
            mediaName = media.getMediaName();
            mediaAuthor = media.getMediaAuthor();
            mediaAlbum = media.getMediaAlbum();
            mediaType = media.getMediaType();

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
                        newMedia.ElementBorder.BorderThickness = new Thickness(0,0,0,0);
                        if (mediaPath[i].Equals(nowPlaying)) 
                        {
                            newMedia.ElementBorder.BorderThickness = new Thickness(2, 2, 2, 2);
                        }

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
                        newMedia.ElementBorder.BorderThickness = new Thickness(0, 0, 0, 0);
                        if (mediaPath[i].Equals(nowPlaying))
                        {
                            newMedia.ElementBorder.BorderThickness = new Thickness(2, 2, 2, 2);
                        }
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
