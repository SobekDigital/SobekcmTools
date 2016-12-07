﻿#region Using directives

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ProtoBuf;
using SobekCM.Core.Settings;

#endregion

namespace SobekCM.Core.UI_Configuration
{
    /// <summary> Instance-wide configuration information for the map editor </summary>
    [Serializable, DataContract, ProtoContract]
    [XmlRoot("MapEditorConfig")]
    public class MapEditor_Configuration
    {
        /// <summary> Constructor for a new instance of the MapEditor_Configuration class  </summary>
        public MapEditor_Configuration()
        {
            Collections = new List<MapEditor_Configuration_Collection>();
        }

        /// <summary> List of map editor configuration collections </summary>
        [DataMember(Name = "settingCollections", EmitDefaultValue = false)]
        [XmlArray("settingCollections")]
        [XmlArrayItem("collection", typeof(MapEditor_Configuration_Collection))]
        [ProtoMember(1)]
        public List<MapEditor_Configuration_Collection> Collections { get; set; }

        /// <summary> Gets all settings from config file </summary>
        /// <param name="IdsFromPage"> The IDs from the page </param>
        /// <returns> List of settings to use for that ID(?) </returns>
        public List<Simple_Setting> GetSettings(List<string> IdsFromPage)
        {
            //get defaults as base
            List<Simple_Setting> settings = GetDefaultSettings();

            //determine if custom has settings
            if (HasCustomSettings(IdsFromPage))
            {
                List<Simple_Setting> newSettings =  GetCustomSettings(IdsFromPage);
                settings.AddRange(newSettings);
            }
            
            return settings;
        }

        /// <summary> Determines if there are custom settings </summary>
        /// <param name="IdsFromPage">The ids from page.</param>
        /// <returns></returns>
        private bool HasCustomSettings(List<string> IdsFromPage)
        {
            foreach (MapEditor_Configuration_Collection collection in Collections)
            {
                string[] tempIds = collection.Name.Split(',');
                foreach (string idFromConfig in tempIds)
                {
                    foreach (string thisIdFromPage in IdsFromPage)
                    {
                        if (thisIdFromPage.Replace(" ", "") == idFromConfig.Replace(" ", ""))
                            return true;
                    }
                }
            }
            
            return false;
        }

        /// <summary> Gets the default settings </summary>
        /// <returns></returns>
        private List<Simple_Setting> GetDefaultSettings()
        {
            foreach (MapEditor_Configuration_Collection thisCollection in Collections)
            {
                if (String.Equals(thisCollection.Name, "default", StringComparison.OrdinalIgnoreCase))
                {
                    return thisCollection.Settings;
                }
            }
            
            //return settings
            return null;
        }

        /// <summary> Gets all the custom settigns as defined from page itself </summary>
        /// <param name="IdsFromPage"> The ids from page.</param>
        /// <returns></returns>
        private List<Simple_Setting> GetCustomSettings(List<string> IdsFromPage)
        {
            foreach (MapEditor_Configuration_Collection collection in Collections)
            {
                string[] tempIds = collection.Name.Split(',');
                foreach (string idFromConfig in tempIds)
                {
                    foreach (string thisIdFromPage in IdsFromPage)
                    {
                        if (thisIdFromPage.Replace(" ", "") == idFromConfig.Replace(" ", ""))
                            return collection.Settings;
                    }
                }
            }

            return new List<Simple_Setting>();
        }
    }

    /// <summary> Collection of map editor configuration key/values </summary>
    [Serializable, DataContract, ProtoContract]
    public class MapEditor_Configuration_Collection
    {
        /// <summary> Name of this collection (often a set of collection names) </summary>
        [DataMember(Name = "name", EmitDefaultValue = false)]
        [XmlAttribute("name")]
        [ProtoMember(1)]
        public string Name { get; set; }

        /// <summary> List of settings for this collection </summary>
        [DataMember(Name = "settings", EmitDefaultValue = false)]
        [XmlArray("settings")]
        [XmlArrayItem("setting", typeof(Simple_Setting))]
        [ProtoMember(2)]
        public List<Simple_Setting> Settings { get; set; }

        /// <summary> Constructor for a new instance of the MapEditor_Configuration_Collection class </summary>
        public MapEditor_Configuration_Collection()
        {
            Settings = new List<Simple_Setting>();
        }
    }
}
