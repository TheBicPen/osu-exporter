using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            int skipCounter = 0;
            int fileCount = 0;
            int fileCounter = 0;

            if (SourceFolder != null && DestinationFolder != null && FileType != null)
            {
                if (mode == 1)
                {
                    string[] folders = Directory.GetDirectories(SourceFolder);
                    fileCount = folders.Length;
                    foreach (string folder in folders)
                    {
                        string songFolder = Path.Combine(SourceFolder, folder);
                        string[] textFiles = Directory.GetFiles(songFolder, "*.osu");
                        string textFile = textFiles[0];
                        string[] metadata = new string[4]; //title, artist, beatmap creator, tags
                        string line;
                        string file = "";
                        using (StreamReader textReader = new StreamReader(textFile))
                            {
                                do
                                {
                                    line = textReader.ReadLine();
                                    if (line.Contains("AudioFilename:"))
                                    {
                                        file = line;
                                    }
                                    else if (line.Contains("Title:"))
                                    {
                                        metadata[0] = line;
                                        metadata[0] = metadata[0].Remove(0, "Title:".ToCharArray().Length);
                                    }

                                    else if (line.Contains("Artist:"))
                                    {
                                        metadata[1] = line;
                                        metadata[1] = metadata[1].Remove(0, "Artist:".ToCharArray().Length);
                                    }

                                    else if (line.Contains("Creator:"))
                                    {
                                        metadata[2] = line;
                                        metadata[2] = metadata[2].Remove(0, "Creator:".ToCharArray().Length);
                                    }

                                    else if (line.Contains("Tags:"))
                                    {
                                        metadata[3] = line;
                                    }

                                } while (line != "[Difficulty]");

                            }
                        file = Path.Combine(songFolder, file.Remove(0, "Audio Filename: ".ToCharArray().Length - 1));
                        string fileName = metadata[0] + file.Substring(file.LastIndexOf("."));
                        bool fileCopied = CopyFile(file, fileName);
                        if (fileCopied == true)
                        { fileCounter++; }
                        else if (fileCopied == false)
                        { skipCounter++; }
                        else { throw new GenericException(); }
                    }
                }


                else if (mode == 2 || mode == 3)
                {
                    string[] files = Directory.GetFiles(SourceFolder, FileType, SearchOption.AllDirectories);
                    CopyFiles(files);
                }
                MessageBox.Show("Done!");
            }
            else
            {
                MessageBox.Show("Please select a source folder, a destination folder, and a file type", "Error");
            }
        }

        private bool PromptOverwrite()
        {
            DialogResult result = MessageBox.Show("This file already exists in the destination folder.\r\n Overwrite the file?", "", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            { return true; }
            else if (result == DialogResult.No)
            { return false; }
            else { throw new GenericException(); }
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

        private void CopyFiles(string[] files)
        {
            int skipCounter = 0;
            int fileCounter = files.Length;
            progressBar1.Maximum = fileCounter;

            bool TooMany = ManyFiles(fileCounter);
            if (TooMany == true)
            { }
            else if (TooMany == false)
            {
                fileCounter = 0;
                foreach (string file in files)
                {
                    string DestinationFile = DestinationFolder + "\\" + Path.GetFileName(file);
                    progressBar1.Value = fileCounter;
                    if (File.Exists(DestinationFile))
                    {
                        skipCounter++;
                        fileCounter++;
                    }
                    else
                    {
                        File.Copy(file, DestinationFile);
                        fileCounter++;
                    }
                }
                progressBar1.Value = 0;
                MessageBox.Show($"{(files.Length - skipCounter).ToString()} of {files.Length} files copied.");
            }
            else { throw new GenericException(); }
        }


        private bool CopyFile(string sourceFile, string fileName)
        {
            string DestinationFile = Path.Combine(DestinationFolder, fileName);
            if (File.Exists(DestinationFile))
            {
                bool overwrite = PromptOverwrite();
                if (overwrite == true)
                {
                    File.Copy(sourceFile, DestinationFile);
                    return true;
                }
                else if (overwrite == false)
                { return false; }
                else { throw new GenericException(); }

            }

            else if (!File.Exists(DestinationFile))
            {
                File.Copy(sourceFile, DestinationFile);
                return true;
            }
            else
            { throw new GenericException(); }
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
                        checkBox1.Show();
                        break;
                    }
                case 2:
                    {
                        mode = 2;
                        button1.Text = "Source folder";
                        label1.Show();
                        textBox3.Show();
                        checkBox1.Hide();
                        break;
                    }
                case 3:
                    {
                        mode = 3;
                        button1.Text = "Source folder";
                        label1.Show();
                        textBox3.Show();
                        checkBox1.Hide();
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
    }

    public class GenericException : Exception
    {
        public GenericException()
        {
            MessageBox.Show("Whoops.. An Error Occurred!");
        }
    }

}
