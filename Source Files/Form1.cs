using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Picture_Naming_Helper
{
    public partial class Form1 : Form
    {
        public List<Photo> photos = new List<Photo> { }; //array of Photo Objects

        public int currentSelection = -1; //index for the list of photos
        public string currentDirectory; //where the files will be saved
        public ImageList iList = new ImageList();

        public Size smallSize = new System.Drawing.Size(25, 25);
        public Size mediumSize = new System.Drawing.Size(50, 50);
        public Size largeSize = new System.Drawing.Size(100, 100);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Run this code on start up
            listView.LargeImageList = iList;
            iList.ImageSize = new System.Drawing.Size(50, 50);
        }
        ///Forms and Buttons///
        private void addButton_Click(object sender, EventArgs e) //Add Button
        {
            //Open a file dialog
            using (var fileDia = new OpenFileDialog())
            {
                fileDia.Title = "Load Images";

                // Set the file dialog to filter for graphics files.
                fileDia.Filter = "Images (*.BMP;*.JPG;*.PNG;*.GIF)|*.BMP;*.JPG;*.PNG;*.GIF|" + "All files (*.*)|*.*";

                // Allow the user to select multiple images.
                fileDia.Multiselect = true;

                DialogResult dr = fileDia.ShowDialog();

                // Read the files
                //check if loading was okay
                if (dr == System.Windows.Forms.DialogResult.OK)
                    foreach (String file in fileDia.FileNames)
                    {
                        AddImage(file);

                        //Get File's extension
                        textBoxExt.Text = Path.GetExtension(file);

                        //Set Textbox
                        textBox.Text = Path.GetFileNameWithoutExtension(file);

                        statusLabel.Text = "Image(s) loaded";
                    }
                else
                {
                    statusLabel.Text = "Image loading has failed";
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)//Folder Button
        {
            //Create a Folder Browsing Dialog
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                //If there are no errors set the currentDirectory and update labels
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    currentDirectory = fbd.SelectedPath;
                    fileControlBox.Text = currentDirectory;
                    statusLabel.Text = "Images will now be saved to " + currentDirectory;
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)//Save Button
        {
            //If the currentSelection is a valid index, Save File, update labels
            if (currentSelection != -1)
            {
                photos[currentSelection].SaveNewName(currentDirectory, textBox.Text + textBoxExt.Text);
                listView.Items[currentSelection].Text = photos[currentSelection].FileName;
                statusLabel.Text = "Image Saved";
            }
        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)//clicking a thumbnail
        {
            //Check that ListView isnt empty to avoid errors
            if (listView.SelectedIndices.Count > 0)
            {
                //Change the currentSelection
                currentSelection = listView.SelectedIndices[0];
                string file = photos[currentSelection].FilePath;

                //Update the MainPicture
                if (File.Exists(file))
                    using (var bmpTemp = new Bitmap(file))
                    {
                        Bitmap imageFile = new Bitmap(bmpTemp);
                        if (mainPicture.Image != null)
                            mainPicture.Image.Dispose();
                        mainPicture.Image = imageFile;
                        updateNameBox(file);
                    }
                else
                {
                    removeImage();
                    statusLabel.Text = "Image not found";
                }
            }
        }
        private void removeButton_Click(object sender, EventArgs e)//Remove Button
        {
            removeImage();
        }
        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)//Thumbnail Size Menu
        {
            //Clear Image List and List View because changing imageSize naturally deletes the images
            iList.Images.Clear();
            listView.Items.Clear();

            //Check which size the user chose
            if (iconSizeBox.SelectedIndex == 0)
                iList.ImageSize = smallSize;

            if (iconSizeBox.SelectedIndex == 1)
                iList.ImageSize = mediumSize;

            if (iconSizeBox.SelectedIndex == 2)
                iList.ImageSize = largeSize;

            //Reference the list of Photo objects to rebuild the ImageList and ListView
            foreach (Photo p in photos)
            {
                using (var bmpTemp = new Bitmap(p.FilePath))
                {
                    Bitmap imageFile = new Bitmap(bmpTemp);

                    //Add image to List
                    iList.Images.Add(imageFile);
                    listView.Items.Add(new ListViewItem(Path.GetFileName(p.FilePath), iList.Images.Count - 1));
                }
            }

            //Call the Garbage Collector to keep memory usage low
            System.GC.Collect();
        }
        ///Methods///
        private void AddImage(string file)
        {
            // Create a Photo Object
            photos.Add(new Photo(file));

            //Update Current Save Directory
            if (currentDirectory == null)
                currentDirectory = Path.GetDirectoryName(file);
            fileControlBox.Text = currentDirectory;

            //Create an Image, cloning a bitmap rather than use an image so that the image file is not locked (can be moved)
            using (var bmpTemp = new Bitmap(file))
            {
                Bitmap imageFile = new Bitmap(bmpTemp);


                //Add image to List
                iList.Images.Add(imageFile);

                //Update the main picture box
                if (mainPicture.Image != null)
                    mainPicture.Image.Dispose();
                mainPicture.Image = imageFile;

                //Create Item from Image
                listView.Items.Add(new ListViewItem(Path.GetFileName(file), iList.Images.Count - 1));

                //Make the latest picture the currentSelection
                currentSelection = photos.Count() - 1;
            }
        }
        private void removeImage()
        {
            //Check if list of Photos is not empty
            if (photos.Count() > 0)
            {
                //Delete respective item from Image List...
                iList.Images[currentSelection].Dispose();

                //and remove it from the listView
                listView.Items.RemoveAt(currentSelection);

                //Make the respective item in the list of Photos null to "deactivate" it
                photos[currentSelection] = null;
                photos.RemoveAt(currentSelection);

                //Update the current Selection
                currentSelection = photos.Count() - 1;

                //Clear the Main Picture Box
                if (mainPicture.Image != null)
                {
                    mainPicture.Image.Dispose();
                    mainPicture.Image = null;
                }

                //Update label
                statusLabel.Text = "Image was removed";
            }
            else
            {
                statusLabel.Text = "No Image to remove";
            }
        }
        private void updateNameBox(string file)
        {
            //Get File's extension
            textBoxExt.Text = Path.GetExtension(file);

            //Set Textbox
            textBox.Text = Path.GetFileNameWithoutExtension(file);
        }
        ///Blank Forms, Do Not Delete///
        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void splitContainer1_Panel1_Paint(object sender, EventArgs e)
        {

        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
