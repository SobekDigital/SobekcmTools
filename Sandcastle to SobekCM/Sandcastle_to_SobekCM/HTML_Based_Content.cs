#region Using directives

using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ProtoBuf;
using SobekCM.Tools;

#endregion

namespace SobekCM.Core.WebContent
{
    /// <summary> Base abstract class is used for all html content-based browse, info, or
    /// simple CMS-style web content objects.  These are objects which are (possibly) read from
    /// a static HTML file and much of the head information must be maintained </summary>
    [Serializable, DataContract, ProtoContract]
    [XmlRoot("webContentPage")]
    public class HTML_Based_Content
    {
        private string code;


        #region Constructors

        /// <summary> Constructor for a new instance of the base_HTML_Content_Object class  </summary>
        public HTML_Based_Content()
        {
            // Set the necessary values to empty initially
            code = String.Empty;
        }

        /// <summary> Constructor for a new instance of the base_HTML_Content_Object class  </summary>
        /// <param name="Code"> SobekCM code for this html content-based object </param>
        public HTML_Based_Content(string Code)
        {
            // Save the parameter
            code = Code;
        }

        /// <summary> Constructor for a new instance of the base_HTML_Content_Object class  </summary>
        /// <param name="Text"> Static text to use for this item </param>
        /// <param name="Title"> Title to display with this item </param>
        /// <remarks> This constructor is mostly used with passing back errors to be displayed. </remarks>
        public HTML_Based_Content(string Text, string Title)
        {
            // Set the necessary values to empty initially
            code = String.Empty;

            // Save the parameters
            Content = Text;
            this.Title = Title;
        }

        /// <summary> Constructor for a new instance of the base_HTML_Content_Object class  </summary>
        /// <param name="Text"> Static text to use for this item </param>
        /// <param name="Title"> Title to display with this item </param>
        /// <param name="Source"> Source file for this static web-based content object </param>
        /// <remarks> This constructor is mostly used with passing back errors to be displayed. </remarks>
        public HTML_Based_Content(string Text, string Title, string Source )
        {
            // Set the necessary values to empty initially
            code = String.Empty;

            // Save the parameters
            Content = Text;
            this.Title = Title;
            this.Source = Source;
        }

        #endregion

        #region Public properties and methods

        /// <summary> Code for this info or browse page </summary>
        /// <remarks> This is the code that is used in the URL to specify this info or browse page </remarks>
        [DataMember(EmitDefaultValue = false, Name = "code")]
        [XmlAttribute("code")]
        [ProtoMember(1)]
        public string Code
        {
            get { return code; }
            set { code = value.ToLower(); }
        }

        ///// <summary> Banner to display whenever this browse or information page is displayed, which overrides the standard collection banner </summary>
        /// <remarks> If this is [DEFAULT] then the default collection banner will be displayed, otherwise no banner should be shown</remarks>
        [DataMember(EmitDefaultValue = false, Name = "banner")]
        [XmlElement("banner")]
        [ProtoMember(2)]
        public string Banner { get; set; }

        /// <summary> Extra information which was read from the head value and should be included in the final html rendering </summary>
        [DataMember(EmitDefaultValue = false, Name = "extraHeadInfo")]
        [XmlElement("extraHeadInfo")]
        [ProtoMember(3)]
        public string Extra_Head_Info { get; set; }

        /// <summary> Site maps indicate a sitemap XML file to use to render the left navigation bar's tree view (MSDN-style) table of contents </summary>
        [DataMember(EmitDefaultValue = false, Name = "siteMap")]
        [XmlAttribute("siteMap")]
        [ProtoMember(4)]
        public string SiteMap { get; set; }

        /// <summary> Web skin in the head overrides any other web skin which may be present in the URL </summary>
        [DataMember(EmitDefaultValue = false, Name = "webSkin")]
        [XmlAttribute("webSkin")]
        [ProtoMember(5)]
        public string Web_Skin { get; set; }

        /// <summary> Thumbnail is used when displaying this info/browse page as a search result </summary>
        [DataMember(EmitDefaultValue = false, Name = "thumbnail")]
        [XmlElement("thumbnail")]
        [ProtoMember(6)]
        public string Thumbnail { get; set; }

        /// <summary> Title  is used when displaying this info/browse page as a search result </summary>
        [DataMember(EmitDefaultValue = false, Name = "title")]
        [XmlElement("title")]
        [ProtoMember(7)]
        public string Title { get; set; }

        /// <summary> Author is used when displaying this info/browse page as a search result </summary>
        [DataMember(EmitDefaultValue = false, Name = "author")]
        [XmlElement("author")]
        [ProtoMember(8)]
        public string Author { get; set; }

        /// <summary> Date is used when displaying this info/browse page as a search result </summary>
        [DataMember(EmitDefaultValue = false, Name = "date")]
        [XmlElement("date")]
        [ProtoMember(9)]
        public string Date { get; set; }

        /// <summary> Keywords are used when displaying this info/browse page as a search result </summary>
        [DataMember(EmitDefaultValue = false, Name = "keywords")]
        [XmlElement("keywords")]
        [ProtoMember(10)]
        public string Keywords { get; set; }

        /// <summary> Description is used when displaying this info/browse page as a search result </summary>
        [DataMember(EmitDefaultValue = false, Name = "description")]
        [XmlElement("description")]
        [ProtoMember(11)]
        public string Description { get; set; }

        /// <summary> Text to display in the primary display region </summary>
        [DataMember(EmitDefaultValue = false, Name = "content")]
        [XmlElement("content")]
        [ProtoMember(12)]
        public string Content { get; set; }

        /// <summary> Text to display in the primary display region </summary>
        [DataMember(EmitDefaultValue = false, Name = "includeMenu")]
        [XmlIgnore]
        [ProtoMember(13)]
        public bool? IncludeMenu { get; set; }

        /// <summary> Text to display in the primary display region </summary>
        /// <remarks> This is for the XML serialization portions </remarks>
        [IgnoreDataMember]
        [XmlAttribute("includeMenu")]
        public string IncludeMenu_AsString
        {
            get {
                return IncludeMenu.HasValue ? IncludeMenu.ToString() : null;
            }
            set
            {
                bool temp;
                if (Boolean.TryParse(value, out temp))
                    IncludeMenu = temp;
            }
        }

        /// <summary> Source for this html-based web content </summary>
        [DataMember(EmitDefaultValue = false, Name = "source")]
        [XmlAttribute("source")]
        [ProtoMember(14)]
        public string Source { get; set; }

        /// <summary> Static text included as the body of the static HTML file if item aggregation custom directives
        /// appears in the content source (otherwise NULL) </summary>
        [IgnoreDataMember]
        [XmlIgnore]
        public string ContentSource { get; set; }

        #endregion

    }
}
