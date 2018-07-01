using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace Mp3_File_Exporter
{
    public partial class Form1 : Form
    {

        string SourceFolder;
        string DestinationFolder;
        string FileType = "*.mp3";
        int mode = 1;


        public Form1()
        {
            InitializeComponent();
            ChangeMode(1);
            radioButton2.Hide(); // currently does nothing


        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog source = new FolderBrowserDialog();
            if (source.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SourceFolder = source.SelectedPath;
                textBox1.Text = SourceFolder.ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog destination = new FolderBrowserDialog();
            if (destination.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DestinationFolder = destination.SelectedPath;
                textBox2.Text = DestinationFolder.ToString();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FileType = "*" + textBox3.Text;
            FindFiles(SourceFolder, DestinationFolder, FileType, mode);
        }

        private void FindFiles(string SourceFolder, string DestinationFolder, string FileType, int mode)
        {
            DateTime beginOperationTime = DateTime.Now;
            button4.Show();
            int skipCounter = 0;
            int fileCount = 0;
            int fileCounter = 0;
            int invalidFolders = 0;

            if (SourceFolder != null && DestinationFolder != null && FileType != null)
            {
                if (mode == 1)
                {
                    string[] folders = Directory.GetDirectories(SourceFolder);
                    fileCount = folders.Length;
                    progressBar1.Maximum = fileCount;

                    int choice = 0; //what to do when the file exists at the destination
                    bool rememberChoice = false; //use choice for all files?

                    foreach (string folder in folders)
                    {
                        progressBar1.Value = fileCounter + skipCounter + invalidFolders + 1;
                        string songFolder = Path.Combine(SourceFolder, folder);
                        string[] textFiles = Directory.GetFiles(songFolder, "*.osu");
                        string textFile;
                        string[] metadata = new string[5]; //title, artist, beatmap creator, tags, background image
                        string line;
                        string file = "";
                        if (textFiles.Length != 0)
                        {
                            textFile = textFiles[0];


                            using (StreamReader textReader = new StreamReader(textFile))
                            {
                                do
                                {
                                    line = textReader.ReadLine();
                                    if (line.StartsWith("AudioFilename:"))
                                    {
                                        file = line;
                                    }
                                    else if (line.StartsWith("Title:"))
                                    {
                                        metadata[0] = line; //song title
                                        metadata[0] = metadata[0].Remove(0, "Title:".ToCharArray().Length);
                                    }

                                    else if (line.StartsWith("Artist:"))
                                    {
                                        metadata[1] = line; //song artist
                                        metadata[1] = metadata[1].Remove(0, "Artist:".ToCharArray().Length);
                                    }

                                    else if (line.StartsWith("Creator:"))
                                    {
                                        metadata[2] = line; //beatmap creator
                                        //    metadata[2] = metadata[2].Remove(0, "Creator:".ToCharArray().Length); 
                                    }

                                    else if (line.StartsWith("Tags:"))
                                    {
                                        metadata[3] = line; //beatmap tags
                                    }

                                    else if (line.StartsWith("0,0,"))
                                    {
                                        metadata[4] = line; //beatmap background image file path

                                        //take only the parts of the string between quotes
                                        string[] splitLine = new string[3];
                                        splitLine = metadata[4].Split('"');
                                        metadata[4] = songFolder + "\\" + splitLine[1];
                                    }


                                } while (line != "[TimingPoints]");
                                /*end file read when the timing section is reached.
                                see documentation on the .osu file structure for more detail
                                "https://osu.ppy.sh/help/wiki/osu!_File_Formats/Osu_(file_format)" */

                            }
                            file = Path.Combine(songFolder, file.Remove(0, "Audio Filename: ".ToCharArray().Length - 1));

                            try
                            {
                                string fileName = metadata[0] + file.Substring(file.LastIndexOf("."));
                                string newFile = CopyFile(file, fileName, false);

                                if (newFile != null) //file copied
                                {
                                    ApplyMetadata(newFile, metadata, true);
                                    fileCounter++;
                                }

                                else if (newFile == null) //file exists at destination 
                                {
                                    string destinationFile = Path.Combine(DestinationFolder, GetSafePathname(GetSafeFilename(fileName)));
                                    DateTime fileAccessed = File.GetLastAccessTime(destinationFile); //get when the destination file was accessed
                                    File.SetLastAccessTime(destinationFile, DateTime.Now); //set last access time to now so that source files with the same name are not skipped

                                    if (fileAccessed < beginOperationTime && checkBox2.Checked)  //if the file already existed and already-existing files are to be skipped
                                    { skipCounter++; }

                                    else if (fileAccessed >= beginOperationTime || checkBox2.Checked == false) //file exists at destination and "Skip files that existed at the destination BEFORE this copy" is false
                                    {
                                        TagLib.File sourceFile = TagLib.File.Create(file);
                                        TagLib.File destFile = TagLib.File.Create(destinationFile);


                                        if (rememberChoice == true) { }

                                        else if (rememberChoice == false)
                                        {

                                            var overwrite = PromptOverwrite(sourceFile, destFile, metadata);
                                            choice = overwrite[0];

                                            if (overwrite[1] == 1) //use this choice for all files
                                            { rememberChoice = true; }
                                            else if (overwrite[1] == 0)
                                            { rememberChoice = false; }
                                            else { throw new GenericException(); }

                                        }

                                        switch (choice) //skip, replace, keep both files
                                        {
                                            case 1:         // skip this file
                                                skipCounter++;
                                                break;

                                            case 2:         //force copying the file ie. overwrite
                                                ApplyMetadata(CopyFile(file, fileName, true), metadata, true);
                                                fileCounter++;
                                                break;

                                            case 3:         // keep adding numbers until there is no file with the same name
                                                string check;
                                                int counter = 1;
                                                string newName;
                                                do
                                                {
                                                    newName = metadata[0] + "_" + counter + file.Substring(file.LastIndexOf("."));
                                                    check = CopyFile(file, newName, false);
                                                    counter++;
                                                } while (check == null);
                                                ApplyMetadata(check, metadata, true);
                                                fileCounter++;
                                                break;
                                            default:
                                                goto case 1;
                                        }

                                    }
                                    else { throw new GenericException(); }
                                }
                                else { throw new GenericException(); }
                            }
                            catch (Exception InvalidFileException)
                            {
                                skipCounter++;
                                MessageBox.Show(String.Format("A file specified in the '.osu' file is invalid. {0}", InvalidFileException.Message));
                            }

                        } //if (textFiles.Length != 0)

                        else
                        {
                            invalidFolders++;
                            fileCount--;
                        }

                    } //foreach (string folder in folders)

                } //if (mode == 1)


                else if (mode == 2 || mode == 3)
                {
                    
                    string[] files = Directory.GetFiles(SourceFolder, FileType, SearchOption.AllDirectories);
                    int[] counter = new int[2];
                    counter = CopyFiles(files);
                    fileCount = counter[0];
                    skipCounter = counter[1];

                }
                MessageBox.Show($"{(fileCount - skipCounter).ToString()} of {fileCount} files copied.\r\n{invalidFolders} invalid folders.");
            }
            else
            {
                MessageBox.Show("Please select a valid source folder, destination folder, and file type", "Error");
            }
        }

        private int[] PromptOverwrite(TagLib.File newFileData, TagLib.File existingFileData, string[] metadata)
        {
            Form2 form2 = new Form2(newFileData, existingFileData, metadata);
            form2.ShowDialog();
            return form2.result;
        } 

        public void ApplyMetadata(string file, string[] metadata, bool save)
        {
            TagLib.File musicFile = TagLib.File.Create(file);
            musicFile.Tag.Title = metadata[0];
            musicFile.Tag.Performers = new string[] { metadata[1] };
            musicFile.Tag.Comment += metadata[2] + metadata[3];

            TagLib.IPicture[] pictures = new TagLib.IPicture[musicFile.Tag.Pictures.Length + 1];

            try
            {
                for(int i = 0; i < musicFile.Tag.Pictures.Length; i++) //add the pictures already present in the music file
                {
                    pictures[i] = musicFile.Tag.Pictures[i];
                }
                pictures[pictures.Length - 1] = new TagLib.Picture(metadata[4]);
                musicFile.Tag.Pictures = pictures;
            }

            catch (Exception PictureException)
            {
                MessageBox.Show(String.Format("There was an issue setting the image. {0}", PictureException.Message), file);
            }



            if(save) { musicFile.Save(); }
        }

        private bool ManyFiles(int fileCounter)
        {
            if (fileCounter >= 1000)
            {
                DialogResult result;
                result = MessageBox.Show($"There are {fileCounter} files of the type {FileType} in this folder! \r\n Continue?", "Large number of files", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.OK)
                { return false; }
                else if (result == DialogResult.Cancel)
                { return true; }
                else { throw new GenericException(); }
            }
            else { return false; }
        }

        private int[] CopyFiles(string[] files)
        {
            int[] counter = new int[2];
            counter[0] = files.Length;
            progressBar1.Maximum = counter[0];

            bool TooMany = ManyFiles(counter[0]);
            if (TooMany == true)
            { }
            else if (TooMany == false)
            {
                counter[0] = 0;
                foreach (string file in files)
                {
                    string DestinationFile = DestinationFolder + "\\" + Path.GetFileName(file);
                    progressBar1.Value = counter[0];
                    if (File.Exists(DestinationFile))
                    {
                        counter[1]++; //skipcounter
                        counter[0]++; //filecounter
                    }
                    else
                    {
                        File.Copy(file, DestinationFile);
                        counter[0]++;
                    }
                }
                progressBar1.Value = 0;
              //  {(counter[0] - counter[1]).ToString()} of {counter[0]} files copied."); //former messagebox
            }
            else { throw new GenericException(); }
            return counter;
        }

        public string GetSafeFilename(string fileName)
        {
            char[] illegal = new char[50];
            illegal = Path.GetInvalidFileNameChars();
            int counter = illegal.Count();

            return string.Join("_", fileName.Split(illegal));
        }

        public string GetSafePathname(string pathName)
        {
            return string.Join("_", pathName.Split(Path.GetInvalidPathChars()));
        }

        public string CopyFile(string sourceFile, string fileName, bool forceCopy)
        {
            fileName = GetSafePathname(GetSafeFilename(fileName));
            string DestinationFile = Path.Combine(DestinationFolder, fileName);

            if (File.Exists(DestinationFile) && forceCopy == false)
            {
               return DestinationFile = null;
            }

            else if (!File.Exists(DestinationFile)) //copy the file if it does not exist at the destination
            {
                File.Copy(sourceFile, DestinationFile);
                return DestinationFile;
            }

            else if (forceCopy) //if forceCopy, then delete the destination file and copy the new one in its place
            {
                File.Delete(DestinationFile);
                File.Copy(sourceFile, DestinationFile);
                return DestinationFile;
            }

            else { throw new GenericException(); }
        }

        private void ChangeMode(int Mode1)
        {
            switch (Mode1)
            {
                case 1: 
                    {
                        mode = 1;
                        button1.Text = "osu! Songs folder";
                        label1.Hide();
                        textBox3.Hide();

                        break;
                    }
                case 2:
                    {
                        mode = 2;
                        button1.Text = "Source folder";
                        label1.Show();
                        textBox3.Show();

                        break;
                    }
                case 3:
                    {
                        mode = 3;
                        button1.Text = "Source folder";
                        label1.Show();
                        textBox3.Show();

                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            ChangeMode(1);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            ChangeMode(2);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            ChangeMode(3);
        }

        private void button4_Click(object sender, EventArgs e)
        {

            Application.Exit();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(textBox1.Text)) { SourceFolder = textBox1.Text; }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(textBox2.Text)) { DestinationFolder = textBox2.Text; }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }
    }

    public class GenericException : Exception
    {
        public GenericException()
        {
            MessageBox.Show("Whoops.. An Error Occurred!");
        }
    }

}
