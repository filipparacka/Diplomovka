using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Color = Microsoft.Msagl.Drawing.Color;
using Node = Microsoft.Msagl.Drawing.Node;
using Shape = Microsoft.Msagl.Drawing.Shape;
#if DEBUG
using Microsoft.Msagl.GraphViewerGdi;
#endif 

namespace Editing
{
      public partial class Form3 : Form
    {
          public static string a ;
          public static string b ;
          public static string c ;
        
          
        public Form3()
        {
            InitializeComponent();
            comboBox1.DataSource = GraphEditor.concepts ;
            comboBox2.DataSource = GraphEditor.concepts2;
            
        }
       
         public void Form3_Load(object sender, EventArgs e)
        {

            this.ControlBox = false;
            
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }
       
        private void button1_Click(object sender, EventArgs e)

        {
            a = Convert.ToString(comboBox1.SelectedValue);
            b = Convert.ToString(comboBox2.SelectedValue);
            c = Convert.ToString(textBox1.Text);
            this.Hide();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
    
}
