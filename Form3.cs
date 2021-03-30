using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace laba3
{
    public partial class Form3 : Form
    {
        public Form3(int i)
        {
            InitializeComponent();
            Text += i+1;
        }

        private void Continue_Click(object sender, EventArgs e)
        {
            Form1.FlagOfContinue = true;
            Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Form1.FlagOfStop = true;
            Close();
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }
    }
}
