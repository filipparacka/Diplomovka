using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Editing
{
    public partial class Form2 : Form
    {
        public static string c;
        public static string ConceptName;

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.ControlBox = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            c = textBox1.Text;
            ConceptName = textBox2.Text;
            try{
                GraphEditor.map.set(ConceptName, Convert.ToDouble(c));
                this.Hide();
            }
            catch
            {
                MessageBox.Show("Neplatná hodnota! Zadaj inú hodnotu.");
            }
        }

        
    }
}
