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
            int skipCounter = 0;
            FileType = "*" + textBox3.Text;
            string[] files = Directory.GetFiles(SourceFolder, FileType, SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string DestinationFile = DestinationFolder + "\\" + Path.GetFileName(file);
                if(File.Exists(DestinationFile))
                { skipCounter++; }
                else
                {
                    File.Copy(file, DestinationFile);
                }               
            }
            MessageBox.Show("{0} Files copied.", (files.Length - skipCounter).ToString());
        }

    }
}
