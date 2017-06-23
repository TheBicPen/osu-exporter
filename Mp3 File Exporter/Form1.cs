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

        public Form1()
        {
            InitializeComponent();
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
            CopyFiles(SourceFolder, DestinationFolder, FileType);
        }

        private void CopyFiles(string SourceFolder, string DestinationFolder, string FileType)
        {
            int fileCounter = 0;
            int skipCounter = 0;

            if (SourceFolder != null && DestinationFolder != null && FileType != null)
            {
                string[] files = Directory.GetFiles(SourceFolder, FileType, SearchOption.AllDirectories);
                fileCounter = files.Length;
                progressBar1.Maximum = fileCounter;
                if (fileCounter >= 1000)
                {
                    DialogResult result;
                    result = MessageBox.Show($"There are {fileCounter} files of the type {FileType} in this folder! \r\n Continue?", "Large number of files", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.OK)
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
                    else
                    { }

                }
            }
            else
            {
                MessageBox.Show("Error", "Please select a source folder, a destination folder, and a file type");
            }
        }
    }
}
