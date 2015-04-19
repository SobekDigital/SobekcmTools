using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Sandcastle_to_SobekCM
{
    /// <summary> Single node within a hierarchical site map used to render a tree-view
    /// navigation control to the left on sandcastle documentation pages </summary>
    [Serializable]
    public class Sandcastle_TOC_Node
    {
        /// <summary> Relative URL for this single node within a hierarchical site map </summary>
        public readonly string URL;

        /// <summary> Title for this single node within a hierarchical site map, displayed in the
        /// navigation tree </summary>
        public readonly string Title;

        public string New_URL;
        public string New_Title;

        public Code_Node_Type_Enum Node_Type = Code_Node_Type_Enum.Unrecognized;

        private Sandcastle_TOC_Node parentNode;
        private List<Sandcastle_TOC_Node> childNodes;


        /// <summary> Constructor for a new instancee of the Sandcastle_TOC_Node class </summary>
        /// <param name="URL">Relative URL for this single node within a hierarchical site map</param>
        /// <param name="Title">Title for this single node within a hierarchical site map, displayed in the
        /// navigation tree </param>
        public Sandcastle_TOC_Node(string URL, string Title )
        {
            this.URL = URL;
            this.Title = Title;
            New_URL = String.Empty;
            New_Title = String.Empty;
        }

        /// <summary> Gets or sets the link back to the parent node </summary>
        public Sandcastle_TOC_Node Parent_Node
        {
            get { return parentNode; }
            set { parentNode = value; }
        }

        /// <summary> Add a child node to this node </summary>
        /// <param name="URL">Relative URL for the child node within a hierarchical site map</param>
        /// <param name="Title">Title for the child node within a hierarchical site map, displayed in the
        /// navigation tree </param>
        /// <returns> Fully built SobekCM_SiteMap_Node child object </returns>
        public Sandcastle_TOC_Node Add_Child_Node(string URL, string Title)
        {
            Sandcastle_TOC_Node newNode = new Sandcastle_TOC_Node(URL, Title);
            Add_Child_Node(newNode);
            newNode.Parent_Node = this;
            return newNode;
        }

        /// <summary> Add a child node to this node </summary>
        /// <param name="Child_Node"> Child node to add </param>
        public void Add_Child_Node(Sandcastle_TOC_Node Child_Node)
        {
            if (childNodes == null)
                childNodes = new List<Sandcastle_TOC_Node>();

            childNodes.Add(Child_Node);
            Child_Node.Parent_Node = this;
        }

        /// <summary> Gets the number of children nodes under this node </summary>
        public int Child_Nodes_Count
        {
            get
            {
                if (childNodes == null)
                    return 0;
                else
                    return childNodes.Count;
            }
        }

        /// <summary> Gets the read-only collection of child nodes under this node  </summary>
        public ReadOnlyCollection<Sandcastle_TOC_Node> Child_Nodes
        {
            get
            {
                return new ReadOnlyCollection<Sandcastle_TOC_Node>(childNodes);
            }
        }
    }
}
