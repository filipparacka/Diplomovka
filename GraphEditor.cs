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
 *
included in all copies or substantial portions of the Softwar
 *e.

THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using System.Threading;
using Microsoft.Msagl.GraphViewerGdi;
using Color = Microsoft.Msagl.Drawing.Color;
using DrawingNode = Microsoft.Msagl.Drawing.Node;
using Edge = Microsoft.Msagl.Drawing.Edge;
using Label = Microsoft.Msagl.Drawing.Label;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Editing {
    /// <summary>
    /// This class implements a graph editor control. It is a work in progress, especially with regards to allowing users to configure the editor's behaviour.
    /// It is designed to be easy to plug into a program. This control is a GDI control, based on GViewer.
    /// </summary>
    public class GraphEditor : Panel {
        public static libfcm.FCM map = new libfcm.FCM();
        readonly GViewer gViewer = new GViewer();
        readonly ToolTip toolTip = new ToolTip();
        readonly Image treeImage;
        //Microsoft.Msagl.Drawing.DrawingLayoutEditor drawingLayoutEditor;

        /// <summary>
        /// 
        /// </summary>
        internal CloseEditor CloseEditorDelegate;

        /// <summary>
        /// 
        /// </summary>
        internal ShowEditor ShowEditorDelegate;

        /// <summary>
        /// 
        /// </summary>
        internal ValidateEditor ValidateEditorDelegate;

        int idcounter;

        /// <summary>
        /// The object currently being edited
        /// </summary>
        protected DrawingObject m_EditingObject;

        /// <summary>
        /// The text box used as the default editor (edits the label).
        /// </summary>
        protected TextBox m_LabelBox = new TextBox();

        /// <summary>
        /// The point where the user called up the context menu.
        /// </summary>
        protected Point m_MouseRightButtonDownPoint;

        /// <summary>
        /// An ArrayList containing all the node type entries (custom node types for insetion).
        /// </summary>
        protected ArrayList m_NodeTypes = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        //   internal bool ZoomEnabled { get { return (gViewer.DrawingPanel as DrawingPanel).ZoomEnabled; } set { (gViewer.DrawingPanel as DrawingPanel).ZoomEnabled = value; } }
        /// <summary>
        /// Default constructor
        /// </summary>
        public GraphEditor() : this(null, null, null) {
            ShowEditorDelegate = DefaultShowEditor;
            ValidateEditorDelegate = DefaultValidateEditor;
            CloseEditorDelegate = DefaultCloseEditor;
            ShowEditorDelegate(null);
            
        }

        /// <summary>
        /// A constructor that allows the user to specify functions which deals with the label/property editing
        /// </summary>
        /// <param name="showEditorDelegate">A delegate which constructs and displays the editor for a given object.</param>
        /// <param name="validateEditorDelegate">A delegate which validates the content of the editor and copies it to the graph.</param>
        /// <param name="closeEditorDelegate">A delegate which closes the editor.</param>
        internal GraphEditor(ShowEditor showEditorDelegate, ValidateEditor validateEditorDelegate,
                             CloseEditor closeEditorDelegate) {
            ShowEditorDelegate = showEditorDelegate;
            ValidateEditorDelegate = validateEditorDelegate;
            CloseEditorDelegate = closeEditorDelegate;

            SuspendLayout();
            Controls.Add(gViewer);
            gViewer.Dock = DockStyle.Fill;

            m_LabelBox.Enabled = false;
            m_LabelBox.Dock = DockStyle.Top;
            m_LabelBox.KeyDown += m_LabelBox_KeyDown;
            
            ResumeLayout();

            /*            this.drawingLayoutEditor = new Microsoft.Msagl.Drawing.DrawingLayoutEditor(this.gViewer,
                           new Microsoft.Msagl.Drawing.DelegateForEdge(edgeDecorator),
                           new Microsoft.Msagl.Drawing.DelegateForEdge(edgeDeDecorator),
                           new Microsoft.Msagl.Drawing.DelegateForNode(nodeDecorator),
                           new Microsoft.Msagl.Drawing.DelegateForNode(nodeDeDecorator),
                           new Microsoft.Msagl.Drawing.MouseAndKeysAnalyser(mouseAndKeysAnalyserForDragToggle));
                        */

            (gViewer as IViewer).MouseDown += Form1_MouseDown;
            (gViewer as IViewer).MouseUp += Form1_MouseUp;
            (gViewer as IViewer).MouseUp += GViewerKeyDown;
            gViewer.SaveButtonVisible = false;
            gViewer.NavigationVisible = false;
            
             //   gViewer.DrawingPanel.Paint += new PaintEventHandler(gViewer_Paint);
            gViewer.NeedToCalculateLayout = false;

            toolTip.Active = true;
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 1000;
            toolTip.ReshowDelay = 500;

            ToolBar.ButtonClick += ToolBar_ButtonClick;
        }

        /// <summary>
        /// Returns the GViewer contained in the control.
        /// </summary>
        internal GViewer Viewer {
            get { return gViewer; }
        }

        /// <summary>
        /// Gets or sets the graph associated to the viewer.
        /// </summary>
        internal Graph Graph {
            get { return gViewer.Graph; }
            set { gViewer.Graph = value; }
        }

        /// <summary>
        /// The ToolBar contained in the viewer.
        /// </summary>
        internal ToolBar ToolBar {
            get {
                foreach (Control c in gViewer.Controls) {
                    var t = c as ToolBar;
                    if (t != null)
                        return t;
                }
                return null;
            }
        }

        void ToolBar_ButtonClick(object sender, ToolBarButtonClickEventArgs e) {
            foreach (NodeTypeEntry nte in m_NodeTypes)
                if (nte.Button == e.Button) {
                    var center = new Point();
                    var random = new Random(1);

                    var rect1 = gViewer.ClientRectangle; //gViewer.Graph.GeometryGraph.BoundingBox;
                    var rect2 = gViewer.Graph.BoundingBox;
                    Point p = gViewer.ScreenToSource(rect1.Location);
                    Point p2 = gViewer.ScreenToSource(rect1.Location + rect1.Size);
                    if (p.X < rect2.Left)
                        p.X = rect2.Left;
                    if (p2.X > rect2.Right)
                        p2.X = rect2.Right;
                    if (p.Y > rect2.Top)
                        p.Y = rect2.Top;
                    if (p2.Y < rect2.Bottom)
                        p2.Y = rect2.Bottom;
                    var rect = new Microsoft.Msagl.Core.Geometry.Rectangle(p, p2);

                    center.X = rect.Left + random.NextDouble()*rect.Width;
                    center.Y = rect.Bottom + random.NextDouble()*rect.Height;

                    
                }
        }

        string GetNewId() {
            string ret = "_ID" + idcounter++;
            if (gViewer.Graph.FindNode(ret) != null)
                return GetNewId();
            return ret;
        }

        /// <summary>
        /// Changes a node's label and updates the display
        /// </summary>
        /// <param name="n">The node whose label has to be changed</param>
        /// <param name="newLabel">The new label</param>
        internal void RelabelNode(DrawingNode n, string newLabel) {
            n.Label.Text = newLabel;
            
            gViewer.ResizeNodeToLabel(n);
            System.Diagnostics.Debug.WriteLine(n.Label.Text);
        }

        /// <summary>
        /// Changes an edge's label and updates the display
        /// </summary>
        /// <param name="e">The edge whose label has to be changed</param>
        /// <param name="newLabel">The new label</param>
        internal void RelabelEdge(Edge e, string newLabel) {
            if (e.Label == null)
                e.Label = new Label(newLabel);
            else

                e.Label.Text = newLabel;


            gViewer.SetEdgeLabel(e, e.Label);
            e.Label.GeometryLabel.InnerPoints = new List<Point>();
            var ep = new EdgeLabelPlacement(gViewer.Graph.GeometryGraph);
            ep.Run();
            gViewer.Graph.GeometryGraph.UpdateBoundingBox();
            gViewer.Invalidate();   
        }


        internal static PointF PointF(Point p) {
            return new PointF((float) p.X, (float) p.Y);
        }

        static void AddSegmentToPath(ICurve seg, ref GraphicsPath p) {
            const float radiansToDegrees = (float) (180.0/Math.PI);
            var line = seg as LineSegment;
            if (line != null)
                p.AddLine(PointF(line.Start), PointF(line.End));
            else {
                var cb = seg as CubicBezierSegment;
                if (cb != null)
                    p.AddBezier(PointF(cb.B(0)), PointF(cb.B(1)), PointF(cb.B(2)), PointF(cb.B(3)));
                else {
                    var ellipse = seg as Ellipse;
                    if (ellipse != null)
                        p.AddArc((float) (ellipse.Center.X - ellipse.AxisA.Length),
                                 (float) (ellipse.Center.Y - ellipse.AxisB.Length), (float) (2*ellipse.AxisA.Length),
                                 (float) (2*ellipse.AxisB.Length), (float) (ellipse.ParStart*radiansToDegrees),
                                 (float) ((ellipse.ParEnd - ellipse.ParStart)*radiansToDegrees));
                }
            }
        }

        internal bool DrawNode(DrawingNode node, object graphics) {
            var g = (Graphics) graphics;

            //flip the image around its center
            using (Matrix m = g.Transform) {
                using (Matrix saveM = m.Clone()) {
                    var clipNow = g.Clip;
                    g.Clip = new Region(Draw.CreateGraphicsPath(node.GeometryNode.BoundaryCurve));
                    var geomNode = node.GeometryNode;
                    var leftTop = geomNode.BoundingBox.LeftTop;
                    var scale =
                        (float)
                        Math.Min(node.BoundingBox.Height/treeImage.Height, node.BoundingBox.Width/treeImage.Width);
                    using (var m2 = new Matrix(scale, 0, 0, -scale, (float) leftTop.X, (float) leftTop.Y)) {
                        m.Multiply(m2);
                        g.Transform = m;
                        g.DrawImage(treeImage, 0, 0);
                        g.Transform = saveM;
                        g.Clip = clipNow;
                    }
                }
            }
            return false; //returning false would enable the default rendering

        }

        public static List<string> concepts = new List<string>();
        public static List<string> concepts2 = new List<string>();
        public static List<string> ConceptNames = new List<string>();
        public static int  count=0;
        public static int node_id = 0;

        internal void InsertNode(object sender, EventArgs e) {
            node_id++;
            Form2 f2 = new Form2();
            f2.ShowDialog(this);
            map.add(Form2.ConceptName);
            ConceptNames.Add(Form2.ConceptName);
            count++;
            concepts.Add(Form2.ConceptName + ":" + Form2.c);
            concepts2.Add(Form2.ConceptName + ":" + Form2.c);
            Graph.AddNode(Form2.ConceptName + ":" + Form2.c);
            RedoLayout();
            System.Diagnostics.Debug.WriteLine(map.list());
         }
         
       void deleteSelected_Click(object sender, EventArgs e) {
            
            var al = new ArrayList();
            foreach (IViewerObject ob in gViewer.Entities)
                if (ob.MarkedForDragging)
                    al.Add(ob);
            foreach (IViewerObject ob in al) {
                var edge = ob.DrawingObject as IViewerEdge;
                if (edge != null)
                    gViewer.RemoveEdge(edge, true);
                else {
                    var node = ob as IViewerNode;
                    if (node != null)
                        gViewer.RemoveNode(node, true);
                     for (int i = 0 ; i < ConceptNames.Count; i++) {
                        if (m_LabelBox.ToString().Contains(ConceptNames[i]))
                        {
                            for (int j = 0; j < edges; j++){
                                for(int k = 0; k < 3; k++){
                                    if (Edges[j, k].Contains(ConceptNames[i]))
                                    {
                                        Edges[j, 0] = "null";
                                        Edges[j, 1] = "null";
                                        Edges[j, 2] = "null";
                                    }
                            
                                }   
                            }
                            map.remove(ConceptNames[i]);
                            concepts.Remove(ConceptNames[i] + ":" + m_LabelBox.ToString().Substring(m_LabelBox.ToString().LastIndexOf(':') + 1));
                            concepts2.Remove(ConceptNames[i] + ":" + m_LabelBox.ToString().Substring(m_LabelBox.ToString().LastIndexOf(':') + 1));
                            ConceptNames.Remove(ConceptNames[i]);
                         }
                    }
                } 
            }
        }

        
        void GViewerKeyDown(object sender, MsaglMouseEventArgs e)
        {

            if (e.MiddleButtonIsPressed &&  !e.Handled)
            {
               
                if (gViewer.SelectedObject != null)
                {
                    var edge = gViewer.ObjectUnderMouseCursor as IViewerEdge;
                    gViewer.RemoveEdge(edge, true);
                    string Source = Graph.source.ToString().Substring(0, Graph.source.ToString().IndexOf(':'));
                    string Target = Graph.target.ToString().Substring(0, Graph.target.ToString().IndexOf(':'));
                    map.disconnect(Source,Target);
                    for (int j = 0; j < edges; j++) {
                        if (Edges[j, 0] == Graph.source.ToString() && Edges[j, 2] == Graph.target.ToString())
                        {
                            Edges[j, 0] = "null";
                            Edges[j, 1] = "null";
                            Edges[j, 2] = "null";
                        }
                    }
                }
            }
         }



              /*  var al = new System.Collections.ArrayList();
                foreach (IViewerObject ob in gViewer.Entities)
                    if (ob.MarkedForDragging)
                        al.Add(ob);
                
                foreach (IViewerObject ob in al)
                {

                    var edge = ob.DrawingObject as IViewerEdge;
                    if (edge != null)
                    {
                        gViewer.RemoveEdge(edge, true);
                    }
                      
                }*/
            
        

        /// <summary>
        /// Call this to recalculate the graph layout
        /// </summary>
        public virtual void RedoLayout() {
            gViewer.NeedToCalculateLayout = true;
            gViewer.Graph = gViewer.Graph;
            gViewer.NeedToCalculateLayout = false;
            gViewer.Graph = gViewer.Graph;
        }

        void redoLayout_Click(object sender, EventArgs e) {
            RedoLayout();
            
           }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        internal void DefaultShowEditor(DrawingObject obj) {
            m_EditingObject = obj;

            if (obj != null)
                m_LabelBox.Enabled = true;
            var node = obj as DrawingNode;
            if (node != null && node.Label != null)
                m_LabelBox.Text = node.Label.Text;
            else {
                var edge = obj as Edge;
                if (edge != null && edge.Label != null)
                    m_LabelBox.Text = edge.Label.Text;
            }
            Controls.Add(m_LabelBox);
            if (obj != null)
                m_LabelBox.Focus();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal bool DefaultValidateEditor() {
            if (m_EditingObject is DrawingNode) {
                RelabelNode((m_EditingObject as DrawingNode), m_LabelBox.Text);
            } else if (m_EditingObject is Edge) {
                RelabelEdge((m_EditingObject as Edge), m_LabelBox.Text);
            }

            gViewer.Invalidate();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        internal void DefaultCloseEditor() {
            m_LabelBox.Enabled = false;
            m_LabelBox.Text = "";
            m_EditingObject = null;
        }

        void m_LabelBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyValue == 13) {
                if (ValidateEditorDelegate())
                    CloseEditorDelegate();
            }
        }

        void Form1_MouseUp(object sender, MsaglMouseEventArgs e) {
            object obj = gViewer.GetObjectAt(e.X, e.Y);
            DrawingNode node = null;
            Edge edge = null;
            var dnode = obj as DNode;
            var dedge = obj as DEdge;
            var dl = obj as DLabel;
            if (dnode != null)
                node = dnode.DrawingNode;
            else if (dedge != null)
                edge = dedge.DrawingEdge;
            else if (dl != null) {
                if (dl.Parent is DNode)
                    node = (dl.Parent as DNode).DrawingNode;
                else if (dl.Parent is DEdge)
                    edge = (dl.Parent as DEdge).DrawingEdge;
            }
            if (node != null) {
                ShowEditorDelegate(node);
            } else if (edge != null) {
                ShowEditorDelegate(edge);
            } else {
                CloseEditorDelegate();
            }
        }

        /// <summary>
        /// Clears the editor and resets it to a default size.
        /// </summary>
        internal void ClearAll() {
            CloseEditorDelegate();
            var g = new Graph();
            g.GeometryGraph = new GeometryGraph();
            g.GeometryGraph.BoundingBox = new Microsoft.Msagl.Core.Geometry.Rectangle(0, 0, 200, 100);
            gViewer.Graph = g;
            gViewer.Refresh();
        }


        /// <summary>
        /// Overloaded. Adds a new node type to the list. If the parameter contains an image, a button with that image will be added to the toolbar.
        /// </summary>
        /// <param name="nte">The NodeTypeEntry structure containing the initial aspect of the node, type name, and additional parameters required
        /// for node insertion.</param>
        void AddNodeType(NodeTypeEntry nte) {
            m_NodeTypes.Add(nte);

            if (nte.ButtonImage != null) {
                ToolBar tb = ToolBar;
                var btn = new ToolBarButton();
                tb.ImageList.Images.Add(nte.ButtonImage);
                btn.ImageIndex = tb.ImageList.Images.Count - 1;
                tb.Buttons.Add(btn);
                nte.Button = btn;
            }
        }

        public static int i=0;
        public static int edges = 0;
        public static string [,] Edges = new string [20,3];

        internal void hrana(object sender, EventArgs e)
        {
            Form3 f3 = new Form3();
            f3.ShowDialog(this);
            Graph.AddEdge(Form3.a, Form3.c , Form3.b);
            Edges[i, 0] = Form3.a;
            Edges[i, 1] = Form3.c;
            Edges[i, 2] = Form3.b;
            i++;
            edges++;
            map.connect("" + Form3.a.Substring(0, Form3.a.IndexOf(':')) + "", "" + Form3.b.Substring(0, Form3.b.IndexOf(':')) + "");
            map.setRelation("" + Form3.a.Substring(0, Form3.a.IndexOf(':')) + "", "" + Form3.b.Substring(0, Form3.b.IndexOf(':')) + "", "" + Form3.c + "");
            RedoLayout();
            System.Diagnostics.Debug.WriteLine(Edges[0, 0]);
         }

        internal void NodeInfo(object sender, EventArgs e)
        {
            Form5 f5 = new Form5();
            f5.SetNodeValueText(m_LabelBox.ToString().Substring(m_LabelBox.ToString().LastIndexOf(':') + 1));
            int start = m_LabelBox.ToString().LastIndexOf(" ") + 1;
            int end = m_LabelBox.ToString().LastIndexOf(":");
            f5.SetNodeNameText(m_LabelBox.ToString().Substring(start,end-start));
            f5.ShowDialog(this);
        }
        
        
        internal void Recalculate(object sender, EventArgs e)
        {
            map.update();
            int pocet = 0;
            for(int i=0; i < Graph.NodeCount; i++){
                    foreach (DictionaryEntry entry in Graph.NodeMap)
                        {
                            if (Graph.FindNode(entry.Key.ToString()).LabelText.Contains(ConceptNames[i]))
                            {
                                Graph.FindNode(entry.Key.ToString()).LabelText = ConceptNames[i] + ":" + (Math.Round(map.get(ConceptNames[i]), 3, MidpointRounding.AwayFromZero)).ToString();
                            }
                         }

            concepts[pocet] = (ConceptNames[i] + ":" + Math.Round(map.get(ConceptNames[i]), 3, MidpointRounding.AwayFromZero).ToString());
            concepts2[pocet] = (ConceptNames[i] + ":" + Math.Round(map.get(ConceptNames[i]), 3, MidpointRounding.AwayFromZero).ToString());
            pocet++; 
               
            }
            Class.UpdateEdges();
            RedoLayout();
            
        }

        
        /// <summary>
        /// Overloaded. Adds a new node type to the list. If the parameters contain an image, a button with that image will be added to the toolbar.
        /// </summary>
        /// <param name="name">The name for the new node type</param>
        /// <param name="shape">The initial node shape</param>
        /// <param name="fillcolor">The initial node fillcolor</param>
        /// <param name="fontcolor">The initial node fontcolor</param>
        /// <param name="fontsize">The initial node fontsize</param>
        /// <param name="userdata">A string which will be copied into the node userdata</param>
        /// <param name="deflabel">The initial node label</param>
        internal void AddNodeType(string name, Shape shape, Color fillcolor, Color fontcolor, int fontsize,
                                  string userdata, string deflabel) {
            AddNodeType(new NodeTypeEntry(name, shape, fillcolor, fontcolor, fontsize, userdata, deflabel));
        }

        /// <summary>
        /// Builds the context menu for when the user right-clicks on the graph.
        /// </summary>
        /// <param name="point">The point where the user clicked</param>
        /// <returns>The context menu to be displayed</returns>
        protected virtual ContextMenu BuildContextMenu(Point point) {
            var cm = new ContextMenu();

            MenuItem mi;
            mi = new MenuItem();

            mi.MeasureItem += mi_MeasureItem;
            mi.DrawItem += mi_DrawItem;
            mi.Text = "Pridať hranu";
            mi.Click += hrana;
            cm.MenuItems.Add(mi);

            mi = new MenuItem();
            mi.Text = "-";
            cm.MenuItems.Add(mi);

            mi.MeasureItem += mi_MeasureItem;
            mi.DrawItem += mi_DrawItem;
            mi.Text = "Pridať koncept";
            mi.Click += InsertNode;
            cm.MenuItems.Add(mi);

            mi = new MenuItem();
            mi.Text = "Vymazať";
            mi.Click += deleteSelected_Click;
            cm.MenuItems.Add(mi);

            if (gViewer.SelectedObject != null)
            {
                mi = new MenuItem();
                mi.Text = "Informácie";
                mi.Click += NodeInfo;
                cm.MenuItems.Add(mi);
            }

            mi = new MenuItem();
            mi.Text = "Prepočítať koncepty";
            mi.Click += Recalculate;
            cm.MenuItems.Add(mi);

            mi = new MenuItem();
            mi.Text = "Resetovať";
            mi.Click += redoLayout_Click;
            cm.MenuItems.Add(mi);

            return cm;
        }

        void mi_DrawItem(object sender, DrawItemEventArgs e) {
            e.DrawBackground();
            if ((e.State & DrawItemState.Selected) != DrawItemState.Selected)
                e.Graphics.FillRectangle(SystemBrushes.Control, e.Bounds);

            var nte = (m_NodeTypes[e.Index] as NodeTypeEntry);
            int x = 14;
            if (nte.ButtonImage != null) {
                e.Graphics.DrawImage(nte.ButtonImage, e.Bounds.X, e.Bounds.Y);
                x = nte.ButtonImage.Width + 1;
            }
            var mi = sender as MenuItem;
            var h = (int) e.Graphics.MeasureString(mi.Text, Font).Height;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                e.Graphics.DrawString(mi.Text, Font, SystemBrushes.HighlightText, x,
                                      (e.Bounds.Height - h)/2 + e.Bounds.Y);
            else
                e.Graphics.DrawString(mi.Text, Font, SystemBrushes.ControlText, x, (e.Bounds.Height - h)/2 + e.Bounds.Y);
        }

        void mi_MeasureItem(object sender, MeasureItemEventArgs e) {
            if (e.Index < m_NodeTypes.Count) {
                var nte = (m_NodeTypes[e.Index] as NodeTypeEntry);

                var mi = sender as MenuItem;
                e.ItemHeight = SystemInformation.MenuHeight;
                e.ItemWidth = (int) e.Graphics.MeasureString(mi.Text, Font).Width;

                if (nte.ButtonImage != null) {
                    if (e.ItemHeight < nte.ButtonImage.Height)
                        e.ItemHeight = nte.ButtonImage.Height;
                    e.ItemWidth += nte.ButtonImage.Width + 1;
                }
            }
        }

        void Form1_MouseDown(object sender, MsaglMouseEventArgs e) {
            /*if (e.RightButtonIsPressed && this.gViewer.DrawingLayoutEditor.SelectedEdge != null && this.gViewer.ObjectUnderMouseCursor == this.gViewer.DrawingLayoutEditor.SelectedEdge)
                ProcessRightClickOnSelectedEdge(e);
            else*/
            if (e.RightButtonIsPressed && !e.Handled) {
                m_MouseRightButtonDownPoint = (gViewer).ScreenToSource(e);

                ContextMenu cm = BuildContextMenu(m_MouseRightButtonDownPoint);

                cm.Show(this, new System.Drawing.Point(e.X, e.Y));
            }
        }

        #region Nested type: CloseEditor

        /// <summary>
        /// Closes the object editor
        /// </summary>
        internal delegate void CloseEditor();

        #endregion

        #region Nested type: EdgeInsertedByUserHandler

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        internal delegate void EdgeInsertedByUserHandler(Edge edge);

        #endregion

        #region Nested type: NodeInsertedByUserHandler

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        internal delegate void NodeInsertedByUserHandler(DrawingNode node);

        #endregion

        #region Nested type: NodeTypeEntry

        /// <summary>
        /// Contains all the information needed to allow the user to insert a node of a specific type. This includes a name and, optionally, an image
        /// to associate with the type, as well as several default aspect factors for the node.
        /// </summary>
        internal class NodeTypeEntry {
            /// <summary>
            /// If this node type has an associated button, then this will contain a reference to the button.
            /// </summary>
            internal ToolBarButton Button;

            /// <summary>
            /// If this is not null, then a button will be created in the toolbar, which allows the user to insert a node.
            /// </summary>
            internal Image ButtonImage;

            /// <summary>
            /// The initial label for the node.
            /// </summary>
            internal string DefaultLabel;

            /// <summary>
            /// The initial fillcolor of the node.
            /// </summary>
            internal Color FillColor;

            /// <summary>
            /// The initial fontcolor of the node.
            /// </summary>
            internal Color FontColor;

            /// <summary>
            /// The initial fontsize of the node.
            /// </summary>
            internal int FontSize;

            /// <summary>
            /// This will contain the menu item to which this node type is associated.
            /// </summary>
            internal MenuItem MenuItem;

            /// <summary>
            /// The name for this type.
            /// </summary>
            internal string Name;

            /// <summary>
            /// The initial shape of the node.
            /// </summary>
            internal Shape Shape;

            /// <summary>
            /// A string which will be initially copied into the user data of the node.
            /// </summary>
            internal string UserData;

            /// <summary>
            /// Constructs a NodeTypeEntry with the supplied parameters.
            /// </summary>
            /// <param name="name">The name for the node type</param>
            /// <param name="shape">The initial node shape</param>
            /// <param name="fillcolor">The initial node fillcolor</param>
            /// <param name="fontcolor">The initial node fontcolor</param>
            /// <param name="fontsize">The initial node fontsize</param>
            /// <param name="userdata">A string which will be copied into the node userdata</param>
            /// <param name="deflabel">The initial label for the node</param>
            /// <param name="button">An image which will be used to create a button in the toolbar to insert a node</param>
            internal NodeTypeEntry(string name, Shape shape, Color fillcolor, Color fontcolor, int fontsize,
                                   string userdata, string deflabel, Image button) {
                Name = name;
                Shape = shape;
                FillColor = fillcolor;
                FontColor = fontcolor;
                FontSize = fontsize;
                UserData = userdata;
                ButtonImage = button;
                DefaultLabel = deflabel;
            }

            /// <summary>
            /// Constructs a NodeTypeEntry with the supplied parameters.
            /// </summary>
            /// <param name="name">The name for the node type</param>
            /// <param name="shape">The initial node shape</param>
            /// <param name="fillcolor">The initial node fillcolor</param>
            /// <param name="fontcolor">The initial node fontcolor</param>
            /// <param name="fontsize">The initial node fontsize</param>
            /// <param name="userdata">A string which will be copied into the node userdata</param>
            /// <param name="deflabel">The initial label for the node</param>
            internal NodeTypeEntry(string name, Shape shape, Color fillcolor, Color fontcolor, int fontsize,
                                   string userdata, string deflabel)
                : this(name, shape, fillcolor, fontcolor, fontsize, userdata, deflabel, null) {
            }
        }

        #endregion

        #region Nested type: ShowEditor

        /// <summary>
        /// Displays an editor for the selected object.
        /// </summary>
        /// <param name="obj"></param>
        internal delegate void ShowEditor(DrawingObject obj);

        #endregion

        #region Nested type: ValidateEditor

        /// <summary>
        /// Validates the data inserted by the user into the object editor, and if successful transfers it to the graph.
        /// </summary>
        /// <returns>Returns true if the data was successfully validated.</returns>
        internal delegate bool ValidateEditor();

        #endregion


        float radiusRatio = 0.3f;

        public ICurve GetNodeBoundary(DrawingNode node) {
            double width = treeImage.Width/2.0;
            double height = treeImage.Height/2.0;

            return CurveFactory.CreateRectangleWithRoundedCorners(width, height, width * radiusRatio, height * radiusRatio, new Point());
        }

        public bool DrawNode18(DrawingNode node, object graphics){
            var g = (Graphics)graphics;

            //flip the image around its center
            using (Matrix m = g.Transform){
                using (Matrix saveM = m.Clone()){
                    var clipNow = g.Clip;
                    g.Clip = new Region(Draw.CreateGraphicsPath(node.GeometryNode.BoundaryCurve));
                    var geomNode = node.GeometryNode;
                    var leftTop = geomNode.BoundingBox.LeftTop;
                    using (var m2 = new Matrix(1, 0, 0, -1, (float) leftTop.X, (float) leftTop.Y)){

                        m.Multiply(m2);
                        g.Transform = m;
                        var rect = new RectangleF(0, 0, (float)geomNode.Width, (float)geomNode.Height);
                        using (var lBrush = new LinearGradientBrush(rect, System.Drawing.Color.Red,
                                                                    System.Drawing.Color.Yellow,
                                                                    LinearGradientMode.BackwardDiagonal)
                            ){
                            g.FillRectangle(lBrush, rect);
                        }
                        g.Transform = saveM;
                        g.Clip = clipNow;
                    }
                }
            }
            return false; //returning false would enable the default rendering

        }
    }
}