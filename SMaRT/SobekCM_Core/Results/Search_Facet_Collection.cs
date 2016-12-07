﻿#region Using directives

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ProtoBuf;

#endregion

namespace SobekCM.Core.Results
{
    /// <summary> Single collection of facets, representing a single metadata type 
    /// (i.e., all genre terms for a search, or all subbject/keywords related to a result set, etc.. ) </summary>
    [Serializable, DataContract, ProtoContract]
    [XmlRoot("facetCollection")]
    public class Search_Facet_Collection
    {
        /// <summary> Metadata type id for the metadata represented by this facet collection in the results set </summary>
        [DataMember(Name = "id")]
        [XmlAttribute("id")]
        [ProtoMember(1)]
        public short MetadataTypeID { get; set; }

        /// <summary> Number of facets associated with the second facet list in this results set </summary>
        [IgnoreDataMember]
        [XmlIgnore]
        public int Count
        {
            get { return Facets == null ? 0 : Facets.Count; }
        }

        /// <summary> Metadata term for the metadata represented by this facet collection in the results set </summary>
        [DataMember(Name = "term")]
        [XmlAttribute("term")]
        [ProtoMember(2)]
        public string MetadataTerm { get; set; }

        /// <summary> Collection of facets associated with the second facet list in this results set </summary>
        [DataMember(Name = "facets")]
        [XmlArray("facets")]
        [XmlArrayItem("facet", typeof(Search_Facet))]
        [ProtoMember(3)]
        public List<Search_Facet> Facets { get; set; }

        /// <summary> Constructor for a new instance of the Search_Facet_Collection class </summary>
        public Search_Facet_Collection()
        {
            // Parameterless constuctor for deserialization
            Facets = new List<Search_Facet>();
        }

        /// <summary> Constructor for a new instance of the Search_Facet_Collection class </summary>
        /// <param name="MetadataTypeID"> Metadata type id for the metadata represented by this facet collection in the results set </param>
        public Search_Facet_Collection(short MetadataTypeID)
        {
            Facets = new List<Search_Facet>();
            this.MetadataTypeID = MetadataTypeID;
        }
    }
}
