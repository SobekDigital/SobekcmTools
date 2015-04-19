using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Sandcastle_to_SobekCM
{
    /// <summary> Sandcastle TOC holds all the hierarchical navigation information from a Sandcastle
    /// created help documentation
    /// </summary>
    [Serializable]
    public class Sandcastle_TOC
    {
        private Sandcastle_TOC_Node rootNode;

        /// <summary> Constructor for a new instance of the SobekCM_SiteMap class </summary>
        public Sandcastle_TOC()
        {
            
        }

        /// <summary> Gets and sets the main root node for this site map </summary>
        public Sandcastle_TOC_Node RootNode
        {
            get { return rootNode; }
            set { rootNode = value; }
        }

        /// <summary> Save this Sandcastle TOC as a SobekCM sitemap file </summary>
        /// <param name="FileName"> Name of file to save </param>
        public void Save_SobekCM_SiteMap(string FileName, string base_url )
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter(FileName, false);

            writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");

            writer.WriteLine("<siteMap xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
            writer.WriteLine("		 xmlns=\"http://digital.uflib.ufl.edu/metadata/sobekcm_sitemap/\"");
            writer.WriteLine("		 xsi:schemaLocation=\"http://digital.uflib.ufl.edu/metadata/sobekcm_sitemap/");
            writer.WriteLine("							 http://digital.uflib.ufl.edu/metadata/sobekcm_sitemap/sobekcm_sitemap.xsd\"");
            writer.WriteLine("		 default_breadcrumb=\"Default\" width=\"280\" restrictedRobotUrl=\"http://ufdc.ufl.edu/\" >");
            writer.WriteLine("\t\t<siteMapNode url=\"" + base_url + "\" title=\"Code Details\" description=\"Details on each of the different classes, interfaces, properties, etc..\" >");

            foreach (Sandcastle_TOC_Node childNode in rootNode.Child_Nodes)
            {
                recursively_add_node_text(childNode, writer, "\t\t\t");		
            }

          

            writer.WriteLine("\t\t</siteMapNode>");
            writer.WriteLine("</siteMap>");
            writer.Flush();
            writer.Close();
        }

        private void recursively_add_node_text(Sandcastle_TOC_Node node, System.IO.StreamWriter writer, string indent )
        {
            // Don't write ALL MEMBERS, since we are collapsing that into the default class
            if (node.Node_Type == Code_Node_Type_Enum.All_Members)
                return;

            if (node.Child_Nodes_Count == 0)
            {
                if ((node.Node_Type != Code_Node_Type_Enum.Fields) && (node.Node_Type != Code_Node_Type_Enum.Methods) && (node.Node_Type != Code_Node_Type_Enum.Properties) && (node.Node_Type != Code_Node_Type_Enum.Events))
                {
                    writer.WriteLine(indent + "<siteMapNode url=\"" + node.New_URL.ToLower().Replace(".html", "") + "\" title=\"" + node.New_Title.Trim() + "\" description=\"" + node.Title.Trim() + "\" />");
                }
            }
            else
            {
                if ((node.Node_Type != Code_Node_Type_Enum.Fields) && (node.Node_Type != Code_Node_Type_Enum.Methods) && (node.Node_Type != Code_Node_Type_Enum.Properties) && (node.Node_Type != Code_Node_Type_Enum.Events))
                {
                    writer.WriteLine(indent + "<siteMapNode url=\"" + node.New_URL.ToLower().Replace(".html", "") + "\" title=\"" + node.New_Title.Trim() + "\" description=\"" + node.Title.Trim() + "\" >");
                }
                else
                {
                    writer.WriteLine(indent + "<siteMapNode title=\"" + node.New_Title.Trim() + "\" description=\"" + node.Title.Trim() + "\" >");
                }

                foreach (Sandcastle_TOC_Node childNode in node.Child_Nodes)
                {
                    recursively_add_node_text(childNode, writer, indent + "\t" );
                }

                writer.WriteLine(indent + "</siteMapNode>");

            }
        }
    }
}
