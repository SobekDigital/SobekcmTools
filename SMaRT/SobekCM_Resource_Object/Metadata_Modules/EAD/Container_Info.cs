﻿#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

#endregion

namespace SobekCM.Resource_Object.Metadata_Modules.EAD
{
    /// <summary> Contains all of the information about a single component (or container) including
    /// the list of child components (or containers)  </summary>
    [Serializable]
    public class Container_Info
    {
        #region Constructor(s)

        /// <summary> Constructor for a new instance of the Container_Info class </summary>
        public Container_Info()
        {
            Did = new Descriptive_Identification();
        }

        #endregion

        /// <summary> Flag indicates if this component has complex data, or any children have complex data </summary>
        /// <remarks> A compex component is defined as one that has scope, biogHist, or extend information included, or has a child which has this type of data</remarks>
        public bool Is_Complex_Or_Has_Complex_Children
        {
            get
            {
                // If this has complex children, it is complex
                if (Has_Complex_Children)
                    return true;

                // Is this complex?
                if ((!String.IsNullOrEmpty(Scope_And_Content)) || (!String.IsNullOrEmpty(Biographical_History)) || ((Did != null) && (!String.IsNullOrEmpty(Did.Extent))))
                    return true;

                // Default return false
                return false;
            }
        }

        #region Public Properties

        /// <summary> Gets the level value associated with this container in the container list </summary>
        public string Level { get; set; }

        /// <summary> Flag indicates if any child containers are complex (i.e, have descriptive information, etc.. ) </summary>
        public bool Has_Complex_Children { get; set; }

        /// <summary> Gets the biogrpahical history value associated with this container in the container list </summary>
        public string Biographical_History { get; set; }

        /// <summary> Gets the scope and content value associated with this container in the container list </summary>
        public string Scope_And_Content { get; set; }

        /// <summary> Basic descriptive information included in this container </summary>
        public Descriptive_Identification Did { get; private set; }

        /// <summary> Gets the collection of child components </summary>
        public List<Container_Info> Children { get; private set; }

        /// <summary> Gets the number of child component tags to this component </summary>
        public int Children_Count
        {
            get { return Children == null ? 0 : Children.Count; }
        }
        
        #endregion

        #region Methods used (retained) for convenience, that actually reference DID properties

        /// <summary> Gets the number of container information objects included in the descriptive portion of this component </summary>
        public int Container_Count
        {
            get { return Did.Container_Count; }
        }

        /// <summary> Gets the number of container information objects in the descriptive portion of this component </summary>
        public List<Parent_Container_Info> Containers
        {
            get { return Did.Containers; }
        }

        /// <summary> Gets the unit title value associated with this  </summary>
        public string Unit_Title
        {
            get { return Did.Unit_Title; }
            set { Did.Unit_Title = value; }
        }

        /// <summary> Gets the unit date value associated with this  </summary>
        public string Unit_Date
        {
            get { return Did.Unit_Date; }
            set { Did.Unit_Date = value; }
        }

        /// <summary> Gets the link to the digital object  </summary>
        public string DAO_Link
        {
            get { return Did.DAO_Link; }
            set { Did.DAO_Link = value; }
        }

        /// <summary> Gets the title of the digital object  </summary>
        public string DAO_Title
        {
            get { return Did.DAO_Title; }
            set { Did.DAO_Title = value; }
        }

        /// <summary> Gets the dao information of the digital object  </summary>
        public string DAO
        {
            get { return Did.DAO; }
            set { Did.DAO = value; }
        }

        /// <summary> Gets the extent information </summary>
        public string Extent
        {
            get { return Did.Extent; }
            set { Did.Extent = value; }
        }

        #endregion

        /// <summary> Recursively adds the child component's information to the StringBuilder, in HTML format </summary>
        /// <param name="Builder"> Builder of all the HTML-formatted componenet information for this component</param>
        public void recursively_add_container_information(StringBuilder Builder)
        {
            // Write the information for this tage
            Builder.AppendLine(Did.Unit_Title + "<br />");
            if (Children_Count > 0 )
            {
                Builder.AppendLine("<blockquote>");
                foreach (Container_Info component in Children)
                {
                    component.recursively_add_container_information(Builder);
                }
                Builder.AppendLine("</blockquote>");
            }
        }

        /// <summary> Reads the information about this container in the container list from the EAD XML Reader</summary>
        /// <param name="Reader"> EAD XML Text Reader </param>
        public void Read(XmlTextReader Reader)
        {
            Regex rgx1 = new Regex("xmlns=\"[^\"]*\"");
            Regex rgx2 = new Regex("[ ]*>");
            Regex rgx3 = new Regex("[ ]*/>");

            string tagname = Reader.Name;
            Regex ctagPattern = new Regex("c[0-9][0-9]");
            if (Reader.MoveToAttribute("level"))
                Level = Reader.Value;

            // Read all the information under this component
            while (Reader.Read())
            {
                if (Reader.NodeType == XmlNodeType.Element)
                {
                    if (ctagPattern.IsMatch(Reader.Name))
                    {
                        // Read this child component
                        Container_Info c_tag = new Container_Info();
                        c_tag.Read(Reader);
                        if (Children == null)
                            Children = new List<Container_Info>();
                        Children.Add(c_tag);

                        // Set this flag to determine if this has a complex child
                        if (c_tag.Is_Complex_Or_Has_Complex_Children)
                            Has_Complex_Children = true;
                    }
                    else
                    {
                        switch (Reader.Name)
                        {
                            case "did":
                                Did = new Descriptive_Identification();
                                Did.Read(Reader);
                                break;

                            case "bioghist":
                                string innerXmlBioghist = Reader.ReadInnerXml();
                                string innerXmlBioghist2 = rgx1.Replace(innerXmlBioghist, "");
                                string innerXmlBioghist3 = rgx2.Replace(innerXmlBioghist2, ">");
                                Biographical_History = rgx3.Replace(innerXmlBioghist3, "/>");
                                break;

                            case "scopecontent":
                                string innerXmlScopecontent = Reader.ReadInnerXml();
                                string innerXmlScopecontent2 = rgx1.Replace(innerXmlScopecontent, "");
                                string innerXmlScopecontent3 = rgx2.Replace(innerXmlScopecontent2, ">");
                                Scope_And_Content = rgx3.Replace(innerXmlScopecontent3, "/>");
                                break;
                        }
                    }
                }
                else if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name.Equals(tagname))
                    break;
            }
        }
    }
}