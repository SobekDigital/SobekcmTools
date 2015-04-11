﻿#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using SobekCM.Resource_Object;
using SobekCM.Resource_Object.Bib_Info;
using SobekCM.Library.Application_State;
using SobekCM.Library.Configuration;
using SobekCM.Library.Users;

#endregion

namespace SobekCM.Library.Citation.Elements
{
    /// <summary> Element allows entry of the physical container for an item (as if in an EAD)</summary>
    /// <remarks> This class extends the <see cref="comboBox_TextBox_Element"/> class. </remarks>
    public class Container_Element : comboBox_TextBox_Element
    {
        /// <summary> Constructor for a new instance of the Container_Element class </summary>
        public Container_Element()
            : base("Physical Container", "container")
        {
            Repeatable = true;
            possible_select_items.Clear();
            possible_select_items.Add(String.Empty);
            possible_select_items.Add("Folder");
            possible_select_items.Add("Divider");
            possible_select_items.Add("Box");
            possible_select_items.Add("Shelf");
            possible_select_items.Add("Room");
            Type = Element_Type.Container;
            second_label = "Label";

        }


        /// <summary> Renders the HTML for this element </summary>
        /// <param name="Output"> Textwriter to write the HTML for this element </param>
        /// <param name="Bib"> Object to populate this element from </param>
        /// <param name="Skin_Code"> Code for the current skin </param>
        /// <param name="isMozilla"> Flag indicates if the current browse is Mozilla Firefox (different css choices for some elements)</param>
        /// <param name="popup_form_builder"> Builder for any related popup forms for this element </param>
        /// <param name="Current_User"> Current user, who's rights may impact the way an element is rendered </param>
        /// <param name="CurrentLanguage"> Current user-interface language </param>
        /// <param name="Translator"> Language support object which handles simple translational duties </param>
        /// <param name="Base_URL"> Base URL for the current request </param>
        /// <remarks> This simple element does not append any popup form to the popup_form_builder</remarks>
        public override void Render_Template_HTML(TextWriter Output, SobekCM_Item Bib, string Skin_Code, bool isMozilla, StringBuilder popup_form_builder, User_Object Current_User, Web_Language_Enum CurrentLanguage, Language_Support_Info Translator, string Base_URL )
        {
            // Check that an acronym exists
            if (Acronym.Length == 0)
            {
                const string defaultAcronym = "Enter information about the physical container to which this item belongs";
                switch (CurrentLanguage)
                {
                    case Web_Language_Enum.English:
                        Acronym = defaultAcronym;
                        break;

                    case Web_Language_Enum.Spanish:
                        Acronym = defaultAcronym;
                        break;

                    case Web_Language_Enum.French:
                        Acronym = defaultAcronym;
                        break;

                    default:
                        Acronym = defaultAcronym;
                        break;
                }
            }

            List<string> type = new List<string>();
            List<string> levels = new List<string>();
            if (Bib.Bib_Info.Containers_Count > 0)
            {
                foreach (Finding_Guide_Container thisContainer in Bib.Bib_Info.Containers)
                {
                    type.Add(thisContainer.Type);
                    levels.Add(thisContainer.Name);
                }
            }
            if (type.Count == 0)
            {
                render_helper(Output, String.Empty, String.Empty, Skin_Code, Current_User, CurrentLanguage, Translator, Base_URL, false);
            }
            else
            {
                render_helper(Output, type, levels, Skin_Code, Current_User, CurrentLanguage, Translator, Base_URL);
            }
        }

        /// <summary> Prepares the bib object for the save, by clearing any existing data in this element's related field(s) </summary>
        /// <param name="Bib"> Existing digital resource object which may already have values for this element's data field(s) </param>
        /// <param name="Current_User"> Current user, who's rights may impact the way an element is rendered </param>
        /// <remarks> This clears the list of containers linked to this item </remarks>
        public override void Prepare_For_Save(SobekCM_Item Bib, User_Object Current_User)
        {
            Bib.Bib_Info.Clear_Containers();
        }

        /// <summary> Saves the data rendered by this element to the provided bibliographic object during postback </summary>
        /// <param name="Bib"> Object into which to save the user's data, entered into the html rendered by this element </param>
        public override void Save_To_Bib(SobekCM_Item Bib)
        {                    
            // Pull the standard values
            NameValueCollection form = HttpContext.Current.Request.Form;

            int i = 1;
            foreach (string thisKey in form.AllKeys)
            {
                if (thisKey.IndexOf("container_select") == 0)
                {
                    string diff = thisKey.Replace("container_select", "");
                    string select_value = form[thisKey];
                    string text_value = form["container_text" + diff];

                    if ((select_value.Length > 0) && (text_value.Length > 0))
                    {
                        Bib.Bib_Info.Add_Container(select_value, text_value, i++);
                    }
                }
            }
        }
    }
}



