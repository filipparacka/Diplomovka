/*
Microsoft Automatic Graph Layout,MSAGL 

Copyright (c) Microsoft Corporation

All rights reserved. 

MIT License 

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
""Software""), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
﻿using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Msagl.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Color = Microsoft.Msagl.Drawing.Color;
using Node = Microsoft.Msagl.Drawing.Node;
using Shape = Microsoft.Msagl.Drawing.Shape;
#if DEBUG
using Microsoft.Msagl.GraphViewerGdi;
#endif 
namespace Editing {
    public partial class Form1 : Form {
        
       public Form1() {
#if DEBUG
            DisplayGeometryGraph.SetShowFunctions();
#endif
             
             graphEditor = new GraphEditor();
             InitializeComponent();
             
             graphEditor.Viewer.NeedToCalculateLayout = true;
             CreateGraph();
             graphEditor.Viewer.NeedToCalculateLayout = false;
             SuspendLayout();
             button1.BringToFront();
             button2.BringToFront();
             button3.BringToFront();
             button4.BringToFront(); 
             graphEditor.Viewer.LayoutAlgorithmSettingsButtonVisible = false;
             ResumeLayout();
#if DEBUG
            //   Microsoft.Msagl.GraphViewerGdi.DisplayGeometryGraph.SetShowFunctions();
#endif
        }

        private Class c = new Class();
        private void button1_Click(object sender, EventArgs e)
        {
            graphEditor.ClearAll();
            GraphEditor.edges = 0;
            GraphEditor.concepts.Clear();
            GraphEditor.concepts2.Clear();
            GraphEditor.ConceptNames.Clear();
            Class.conceptList.Clear();
            
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
             {
                 this.graphEditor.Graph = Graph.Read(openFileDialog1.FileName);
                 c.ReadFromLastLine(openFileDialog1.FileName);
                 c.CreateConceptList();
             }

            c.CreateMapNodes();
            c.CreateMapEdges();
            System.Diagnostics.Debug.WriteLine(GraphEditor.ConceptNames[1]);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "msagl (*.msagl)|*.msagl|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                graphEditor.Graph.Write(saveFileDialog1.FileName);
                c.SaveToLastLine(saveFileDialog1.FileName);
            }
            
        }
        
        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
        private void button3_Click(object sender, EventArgs e)
        {
            Form4 f4 = new Form4();
            f4.ShowDialog(this);
            t.Interval = Convert.ToInt32(Form4.speed)*1000;
            t.Tick += new EventHandler(graphEditor.Recalculate);
            t.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            t.Stop();
            
        }

        void CreateGraph() 
        {
            var graph = new Graph();
            graphEditor.Graph = graph;
        }

        static Node Dn(Graph g, string s) 
        {
            return g.FindNode(s);
        }

        private void graphEditor_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}