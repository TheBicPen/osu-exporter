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
        string result;

        public Form2(string[] data1, string[] data2)
        {
            InitializeComponent();
            InitTable(2);
            FillTable(data1, 1);
            FillTable(data2, 2);
        }

        DataTable songInfo = new DataTable();

        private void InitTable(int number)
        {
            for (int counter = 0; counter <= number; counter++)
            {
                songInfo.Columns.Add("Song" + counter);
            }

        }

        public void FillTable(string[] data, int columnNumber)
        {
            object[] row = new object[data.Length];
            row = data;
            songInfo.BeginLoadData();

            foreach (string value in data)
            {
                songInfo.LoadDataRow(row, true);
            }

            songInfo.EndLoadData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            result = button1.Text;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            result = button3.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            result = button2.Text;
        }
    }
}
