using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KaraokeSystem
{
    public class MediaManagement
    {
        private static string infoFilename = "info.txt";
        private List<string> filePath;
        private List<string> mediaName;
        private List<string> mediaAuthor;
        private List<string> mediaAlbum;
        private List<string> mediaType;

        public MediaManagement() 
        {
            this.filePath = new List<string>();
            this.mediaName = new List<string>();
            this.mediaAuthor = new List<string>();
            this.mediaAlbum = new List<string>();
            this.mediaType = new List<string>();
        }

        public List<string> getFilePath()
        {
            return this.filePath;
        }
        public List<string> getMediaName()
        {
            return this.mediaName;
        }
        public List<string> getMediaAuthor()
        {
            return this.mediaAuthor;
        }
        public List<string> getMediaAlbum()
        {
            return this.mediaAlbum;
        }
        public List<string> getMediaType()
        {
            return this.mediaType;
        }

        public void importInfo() 
        {
            using (StreamReader infoFile = new StreamReader(infoFilename, Encoding.Unicode))
            {
                string read;
                string[] data;

                while ((read = infoFile.ReadLine()) != null)
                {
                    if (read != "")
                    {
                        data = read.Split('|');

                        this.filePath.Add(data[0]);
                        this.mediaName.Add(data[1]);
                        this.mediaAuthor.Add(data[2]);
                        this.mediaAlbum.Add(data[3]);
                        this.mediaType.Add(data[4]);
                    }
                }
                infoFile.Close();
            }
        }

        public void addNewMedia(string mediaPath) 
        {
            this.filePath.Add(mediaPath);
            this.mediaName.Add("N/A");
            this.mediaAuthor.Add("N/A");
            this.mediaAlbum.Add("N/A");
            string extension = Path.GetExtension(mediaPath);
            if (extension.Equals(".wmv"))
            {
                this.mediaType.Add("video");
            }
            else 
            {
                this.mediaType.Add("audio");
            }
        }

        public void deleteMedia(string mediaPath) 
        {
            int i;
            for (i = 0; i < filePath.Count; i++)
            {
                if (filePath[i].Equals(mediaPath))
                {
                    this.mediaName.RemoveAt(i);
                    this.mediaAuthor.RemoveAt(i);
                    this.mediaAlbum.RemoveAt(i);
                    this.mediaType.RemoveAt(i);
                    break;
                }
            }
            this.filePath.RemoveAt(i);
        }

        public void editInformation(string mediaPath, string name, string author, string album) 
        {
            for(int i=0;i<filePath.Count;i++) 
            {
                if (filePath[i].Equals(mediaPath)) {
                    this.mediaName[i] = name;
                    this.mediaAuthor[i] = author;
                    this.mediaAlbum[i] = album;
                    break;
                }
            }
        }

        public void writeToInfo()
        {
            StreamWriter infoFile = new StreamWriter(infoFilename,false, Encoding.Unicode);
            for (int i = 0; i < filePath.Count; i++) {
                infoFile.Write(this.filePath[i]);
                infoFile.Write("|");
                infoFile.Write(this.mediaName[i]);
                infoFile.Write("|");
                infoFile.Write(this.mediaAuthor[i]);
                infoFile.Write("|");
                infoFile.Write(this.mediaAlbum[i]);
                infoFile.Write("|");
                infoFile.Write(this.mediaType[i]);
                infoFile.Write("\r\n");
            }
            infoFile.Close();
        }
    }
}