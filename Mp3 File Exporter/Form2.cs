using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mp3_File_Exporter
{
    public partial class Form2 : Form
    {
        public int[] result = new int[2];

        public Form2(string newData, string oldData)
        {
            InitializeComponent();
            //todo: display data
            label1.Text = newData;
            label2.Text = oldData;
        }

        private void CheckCheckboxStatus(CheckBox checkBox)
        {
            if (checkBox.Checked)
            { result[1] = 1; }
            else
            { result[1] = 0; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            result[0] = 1;
            CheckCheckboxStatus(checkBox1);
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            result[0] = 3;
            CheckCheckboxStatus(checkBox1);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            result[0] = 2;
            CheckCheckboxStatus(checkBox1);
            this.Close();
        }

    }
}
