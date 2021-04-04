using System.IO;

namespace Picture_Naming_Helper
{
    public class Photo
    {
        private string fileName;
        private string filePath;

        public string FileName //accessors
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        public Photo()
        {

        }

        public Photo(string fp)
        {
            filePath = fp;
            var result = Path.GetFileName(fp);

            fileName = result;
        }

        public void SaveNewName(string direct, string text)
        {
            string tempPath = direct + "\\" + text;

            File.Move(FilePath, tempPath);
            filePath = tempPath;
            fileName = text;
            //should prolly update the file name
        }
    }
}
