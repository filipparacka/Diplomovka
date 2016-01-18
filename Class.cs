using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace Editing
{
    class Class
    {

        private string edgesString;
        private string input;
        private int count = 0;
        public static List<string> conceptList = new List<string>();

        public Class()
        {

        }

        public void SaveToLastLine(string fileName)
        {
            for (int i = 0; i < GraphEditor.edges - 1; i++)
                {
                    if ((GraphEditor.Edges[i, 0] != "null") || (GraphEditor.Edges[i, 1] != "null") || (GraphEditor.Edges[i, 2] != "null"))
                        edgesString+=(GraphEditor.Edges[i, 0] + " " + GraphEditor.Edges[i, 1] + " " + GraphEditor.Edges[i, 2]+";");
                }
                edgesString+=(GraphEditor.Edges[GraphEditor.edges - 1, 0] + " " + GraphEditor.Edges[GraphEditor.edges - 1, 1] + " " + GraphEditor.Edges[GraphEditor.edges - 1, 2]);
                File.AppendAllText(fileName, Environment.NewLine + "<!--" + edgesString + "-->" );
        }

        public void ReadFromLastLine(string fileName)
        {
            input = File.ReadLines(fileName).Last();
            input = input.Remove(0, 4);
            input = input.Remove(input.Length - 3);
            int i = 0, j = 0;
            foreach (string row in input.Split(';'))
            {
                j = 0;
                foreach (string col in row.Trim().Split(' '))
                {
                    GraphEditor.Edges[i, j] = (col.Trim());
                    j++;
                }
                i++; 
                count++;
                GraphEditor.edges++;
            }
            
        }

        public void CreateConceptList()
        {
            for (int k = 0; k < count; k++)
            {
                conceptList.Add(GraphEditor.Edges[k, 0]);
                conceptList.Add(GraphEditor.Edges[k, 2]);
            }
            conceptList.RemoveAll(item => item == "null");
            GraphEditor.concepts = conceptList.Distinct().ToList();
            GraphEditor.concepts2 = conceptList.Distinct().ToList();
            foreach (string concept in GraphEditor.concepts)
            {
                GraphEditor.ConceptNames.Add(concept.Substring(0,concept.IndexOf(':')));
            }
         }

        public static void UpdateEdges()
        {
            for (int i = 0; i < GraphEditor.edges; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    for (int j = 0; j < GraphEditor.ConceptNames.Count; j++)
                    {
                        if ((GraphEditor.Edges[i, 0] != "null") || (GraphEditor.Edges[i, 1] != "null") || (GraphEditor.Edges[i, 2] != "null"))
                        {
                            if (GraphEditor.Edges[i, k].Contains(GraphEditor.ConceptNames[j]))
                            {
                                GraphEditor.Edges[i, k] = (GraphEditor.ConceptNames[j] + ":" + Math.Round(GraphEditor.map.get(GraphEditor.ConceptNames[j]), 3, MidpointRounding.AwayFromZero).ToString());
                            }
                        }
                    }
                }
            }
        }

        public void CreateMapNodes()
        {
            for (int i = 0; i < GraphEditor.concepts.Count; i++)
            {
                GraphEditor.map.add(GraphEditor.concepts[i].Substring(0, GraphEditor.concepts[i].IndexOf(":")));
                GraphEditor.map.set(GraphEditor.concepts[i].Substring(0, GraphEditor.concepts[i].IndexOf(":")), Convert.ToDouble(GraphEditor.concepts[i].Substring(GraphEditor.concepts[i].LastIndexOf(":") + 1)));
            }
        }

        public void CreateMapEdges()
        {
            for (int i = 0; i < GraphEditor.edges; i++)
            {
                if ((GraphEditor.Edges[i, 0] != "null") || (GraphEditor.Edges[i, 1] != "null") || (GraphEditor.Edges[i, 2] != "null"))
                {
                    GraphEditor.map.connect(GraphEditor.Edges[i, 0].Substring(0, GraphEditor.Edges[i, 0].IndexOf(":")) ,  GraphEditor.Edges[i, 2].Substring(0, GraphEditor.Edges[i, 2].IndexOf(":")) );
                    GraphEditor.map.setRelation( GraphEditor.Edges[i, 0].Substring(0, GraphEditor.Edges[i, 0].IndexOf(":")) ,  GraphEditor.Edges[i, 2].Substring(0, GraphEditor.Edges[i, 2].IndexOf(":")) ,  GraphEditor.Edges[i, 1] );
                }
            }
        }

       
    }
}
