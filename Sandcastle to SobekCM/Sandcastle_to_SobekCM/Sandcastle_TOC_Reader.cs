using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Sandcastle_to_SobekCM
{
    /// <summary> Class is used to read an existing sandcastle TOC file and return the fully built <see cref="Sandcastle_TOC" /> object </summary>
    public class Sandcastle_TOC_Reader
    {
        /// <summary> Reads an existing site map file and returns the fully built <see cref="Sandcastle_TOC" /> object </summary>
        /// <param name="Sandcastle_TOC_File"> Sandcastle TOC file to read </param>
        /// <returns> Fully built Sandcastle TOC object </returns>
        public static Sandcastle_TOC Read_TOC_File(string Sandcastle_TOC_File )
        {
            Stream reader = null;
            XmlTextReader nodeReader = null;
            Sandcastle_TOC siteMap = null;

            Stack<Sandcastle_TOC_Node> nodesStack = new Stack<Sandcastle_TOC_Node>();

            try
            {
                // Create and open the readonly file stream
                reader = new FileStream(Sandcastle_TOC_File, FileMode.Open, FileAccess.Read);

                // create the XML node reader
                nodeReader = new XmlTextReader(reader);

                // Read through the XML document
                while (nodeReader.Read())
                {
                    if (nodeReader.NodeType == XmlNodeType.Element)
                    {
                        // Handle the main sitemap tag
                        if (nodeReader.Name == "HelpTOC")
                        {
                            // This is the first node read, so it may have additional information
                            siteMap = new Sandcastle_TOC();
                        }

                        // Handle a new siteMapNode
                        if (nodeReader.Name == "HelpTOCNode")
                        {
                            string url = String.Empty;
                            string title = String.Empty;

                            // Before moving to any attributes, check to see if this is empty
                            bool empty = nodeReader.IsEmptyElement;

                            // Step through the attributes
                            while (nodeReader.MoveToNextAttribute())
                            {
                                switch (nodeReader.Name)
                                {
                                    case "Url":
                                        url = nodeReader.Value.Replace("html/","");
                                        break;

                                    case "Title":
                                        title = nodeReader.Value;
                                        break;
                                }
                            }

                            // Create the new node
                            Sandcastle_TOC_Node newNode = new Sandcastle_TOC_Node(url, title);

                            // Add to the parent
                            if (nodesStack.Count == 0)
                            {
                                // This is the first node read so it should be the root node
                                siteMap.RootNode = newNode;
                            }
                            else
                            {
                                nodesStack.Peek().Add_Child_Node(newNode);
                            }
                            
                            // Add this to the stack, at least until the end of this node is found 
                            // if this is not an empty element
                            if (!empty)
                                nodesStack.Push(newNode);
                        }
                    }
                    else if ((nodeReader.NodeType == XmlNodeType.EndElement) && ( nodeReader.Name == "HelpTOCNode" ))
                    {
                        nodesStack.Pop();
                    }
                }
            }
            catch
            {

            }
            finally
            {
                if (nodeReader != null)
                    nodeReader.Close();
                if (reader != null)
                    reader.Close();
            }

            return siteMap;
        }
    }
}
