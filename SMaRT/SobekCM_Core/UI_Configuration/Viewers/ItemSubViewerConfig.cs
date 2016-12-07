﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ProtoBuf;
using SobekCM.Tools;

namespace SobekCM.Core.UI_Configuration.Viewers
{
    /// <summary> Configuration for a single item subviewer, including some of how
    /// this subviewer maps to the database and digital resource requirements </summary>
    [Serializable, DataContract, ProtoContract]
    [XmlRoot("itemSubViewer")]
    public class ItemSubViewerConfig
    {
        /// <summary> Viewer code that is mapped to this subviewer </summary>
        [DataMember(Name = "code")]
        [XmlAttribute("code")]
        [ProtoMember(1)]
        public string ViewerCode { get; set; }

        /// <summary> Flag indicates if this subviewer is enabled or disabled </summary>
        [DataMember(Name = "enabled")]
        [XmlAttribute("enabled")]
        [ProtoMember(2)]
        public bool Enabled { get; set; }

        /// <summary> Fully qualified (including namespace) name of the class used 
        /// for this subviewer </summary>
        [DataMember(Name = "class")]
        [XmlAttribute("class")]
        [ProtoMember(3)]
        public string Class { get; set; }

        /// <summary> Name of the assembly within which this class resides, unless this
        /// is one of the default subviewers included in the core code </summary>
        [DataMember(Name = "assembly", EmitDefaultValue = false)]
        [XmlAttribute("assembly")]
        [ProtoMember(4)]
        public string Assembly { get; set; }

        /// <summary> Flag indicates if this should be added to items, regardless of what comes
        /// back as the viewers from the database, since this is a management view for the item.  
        /// It also means it should not be unselectable in the behaviors screen </summary>
        [DataMember(Name = "mgmtViewer")]
        [XmlAttribute("mgmtViewer")]
        [ProtoMember(5)]
        public bool ManagementViewer { get; set; }

        /// <summary> If this is a management menu option, this sets the order the viewer is added
        /// to the menu and the priority list of viewers.</summary>
        /// <remarks> Only the first management viewer would ever  be shown by default, and this 
        /// is only if there were no non-management views to show.  </remarks>
        [DataMember(Name = "mgmtOrder")]
        [XmlAttribute("mgmtOrder")]
        [ProtoMember(6)]
        public float ManagementOrder { get; set; }

        /// <summary> Name of this viewer, which matches the viewer name from the database and 
        /// in the configuration files as well.  This is actually populate by the configuration information </summary>
        [DataMember(Name = "type")]
        [XmlAttribute("type")]
        [ProtoMember(7)]
        public string ViewerType { get; set; }

        /// <summary> If this viewer is tied to certain files existing in the digital resource, this lists all the 
        /// possible file extensions this supports (from the configuration file usually) </summary>
        [DataMember(EmitDefaultValue = false, Name = "fileExtensions")]
        [XmlArray("fileExtensions")]
        [XmlArrayItem("fileExtension", typeof(string))]
        [ProtoMember(8)]
        public string[] FileExtensions { get; set; }

        /// <summary> If this viewer is tied to certain page files existing in the digital resource, this lists all the 
        /// possible file extensions this supports (from the configuration file usually) </summary>
        [DataMember(EmitDefaultValue = false, Name = "pageFileExtensions")]
        [XmlArray("pageFileExtensions")]
        [XmlArrayItem("fileExtension", typeof(string))]
        [ProtoMember(9)]
        public string[] PageExtensions { get; set; }

        /// <summary> List of options related to this viewer, from the configuration </summary>
        [DataMember(EmitDefaultValue = false, Name = "options")]
        [XmlArray("options")]
        [XmlArrayItem("option", typeof(StringKeyValuePair))]
        [ProtoMember(10)]
        public List<StringKeyValuePair> Options { get; set; }

        /// <summary> Constructor for a new instance of the <see cref="ItemSubViewerConfig"/> class </summary>
        public ItemSubViewerConfig()
        {
            // Empty constructor for serialzation purposes
            ManagementViewer = false;
        }

        /// <summary> Constructor for a new instance of the <see cref="ItemSubViewerConfig"/> class </summary>
        /// <param name="ViewerCode"> Viewer code that is mapped to this subviewer </param>
        /// <param name="Enabled"> Flag indicates if this subviewer is enabled or disabled </param>
        /// <param name="Class"> Fully qualified (including namespace) name of the class used 
        /// for this subviewer </param>
        /// <param name="Assembly"> Name of the assembly within which this class resides, unless this
        /// is one of the default subviewers included in the core code </param>
        public ItemSubViewerConfig(string ViewerCode, bool Enabled, string Class, string Assembly)
        {
            this.ViewerCode = ViewerCode;
            this.Enabled = Enabled;
            this.Class = Class;
            this.Assembly = Assembly;

            ManagementViewer = false;
        }

        #region Methods that controls XML serialization

        /// <summary> Method suppresses XML Serialization of the Assembly property if it is empty </summary>
        /// <returns> TRUE if the property should be serialized, otherwise FALSE </returns>
        public bool ShouldSerializeAssembly()
        {
            return (!String.IsNullOrEmpty(Assembly));
        }

        #endregion
    }
}
