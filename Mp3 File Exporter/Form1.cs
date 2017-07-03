﻿using System;
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

            if (SourceFolder != null && DestinationFolder != null && FileType != null)
            {
                if (mode == 1)
                {
                    string[] folders = Directory.GetDirectories(SourceFolder);
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
                                    if (line.Contains("Audio Filename:"))
                                    {
                                        file = line;
                                    }
                                    else if (line.Contains("Title:"))
                                    {
                                        metadata[0] = line;        
                                    }

                                    else if (line.Contains("Artist:"))
                                    {
                                        metadata[1] = line;
                                    }

                                    else if (line.Contains("Creator:"))
                                    {
                                        metadata[2] = line;
                                    }

                                    else if (line.Contains("Tags:"))
                                    {
                                        metadata[3] = line;
                                    }

                                } while (line != "[Difficulty]");

                            }
                        CopyFile(file);
                    }
                }


                else if (mode == 2 || mode == 3)
                {
                    string[] files = Directory.GetFiles(SourceFolder, FileType, SearchOption.AllDirectories);
                    CopyFiles(files);
                }
            }
            else
            {
                MessageBox.Show("Error", "Please select a source folder, a destination folder, and a file type");
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            ChangeMode(1);
        }

        private void CopyFiles(string[] files)
        {
            int skipCounter = 0;
            int fileCounter = files.Length;
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


        private bool CopyFile(string sourceFile)
        {
            string DestinationFile = Path.Combine(DestinationFolder, Path.GetFileName(sourceFile));
            if (File.Exists(DestinationFile))
            {
                return false;
            }

            else if (!File.Exists(DestinationFile))
            {
                File.Copy(sourceFile, DestinationFile);
                return true;
            }
            else
            { throw new GenericException(); }
        }

        private void ChangeMode(int mode)
        {
            switch (mode)
            {
                case 1: 
                    {
                        button1.Text = "osu! Songs folder";
                        label1.Hide();
                        textBox3.Hide();
                        checkBox1.Show();
                        break;
                    }
                case 2:
                    {
                        button1.Text = "Source folder";
                        label1.Show();
                        textBox3.Show();
                        checkBox1.Hide();
                        break;
                    }
                case 3:
                    {
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
