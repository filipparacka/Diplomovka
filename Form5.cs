using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Editing
{
    public partial class Form5 : Form
    {

        

        public Form5()
        {
            InitializeComponent();
                
        }

        public void SetNodeValueText(string myText)
        {
            this.label5.Text = myText;
        }

        public void SetNodeNameText(string myText)
        {
            this.label4.Text = myText;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        public void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
    }
}
