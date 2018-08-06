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
    public partial class Form2 : Form
    {
        public int[] result = new int[3];

        public Form2(TagLib.File newData, TagLib.File oldData, string[] newMetadata, bool displayImages) //metadata: title, artist, beatmap creator, tags
        {
            InitializeComponent();
            newData.Tag.Title = newMetadata[0];
            newData.Tag.Performers = new string[] { newMetadata[1] };
            newData.Tag.Comment = newData.Tag.Comment + newMetadata[2] + newMetadata[3]; //apply the new metadata without saving it
            DisplayInfo(newData, oldData, newMetadata, displayImages);
        }

       

        private void DisplayInfo(TagLib.File newData, TagLib.File oldData, string[] newMetadata, bool displayImages) //title, performers, album artists, comment, picture
        {
            label5.Text = oldData.Tag.Title;
            label6.Text = newData.Tag.Title;

            label8.Text = "";
            label9.Text = "";
            foreach (string performer in oldData.Tag.Performers)
            { label8.Text += performer; }
            foreach (string performer in newData.Tag.Performers)
            { label9.Text += performer; }

            label11.Text = "";
            label12.Text = "";
            foreach (string artist in oldData.Tag.AlbumArtists)
            { label11.Text += artist; }
            foreach (string artist in newData.Tag.AlbumArtists)
            { label12.Text += artist; }

            label14.Text = oldData.Tag.Comment;
            label15.Text = newData.Tag.Comment;
            label17.Text = oldData.Tag.Year.ToString();
            label18.Text = newData.Tag.Year.ToString();
            label20.Text = oldData.Properties.Duration.ToString();
            label21.Text = newData.Properties.Duration.ToString();

            if (displayImages)
            {
                try
                {
                    MemoryStream MSOld = new MemoryStream(oldData.Tag.Pictures[0].Data.Data);
                    // MemoryStream MSNew = new MemoryStream(newData.Tag.Pictures[0].Data.Data); //useless since newData does not contain an image
                    pictureBox1.Image = Image.FromStream(MSOld);
                    // pictureBox2.Image = Image.FromStream(MSNew); //also useless
                    pictureBox2.Image = Image.FromFile(newMetadata[4]);
                }
                catch (Exception PictureDisplayException)
                {
                    MessageBox.Show(PictureDisplayException.Message);
                }
            }
            else { }

        }

        private void CheckCheckboxStatus(CheckBox checkBox, int index)
        {
            if (checkBox.Checked)
            { result[index] = 1; }
            else if (checkBox.Checked == false)
            { result[index] = 0; }
            else { throw new GenericException(); }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            result[0] = 1;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            result[0] = 3;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            result[0] = 2;
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckCheckboxStatus((CheckBox)sender, 1);
        }


    }
}
