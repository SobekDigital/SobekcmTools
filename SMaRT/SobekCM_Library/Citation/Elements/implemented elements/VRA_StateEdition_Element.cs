﻿#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using SobekCM.Resource_Object;
using SobekCM.Resource_Object.Metadata_Modules;
using SobekCM.Resource_Object.Metadata_Modules.VRACore;
using SobekCM.Library.Application_State;
using SobekCM.Library.Configuration;
using SobekCM.Library.Users;

#endregion

namespace SobekCM.Library.Citation.Elements
{
    /// <summary> Element allows entry of the VRACore state/edition field</summary>
    /// <remarks> This class extends the <see cref="simpleTextBox_Element"/> class. </remarks>
    public class VRA_StateEdition_Element : simpleTextBox_Element
    {
        /// <summary> Constructor for a new instance of the VRA_StateEdition_Element class </summary>
        public VRA_StateEdition_Element() : base("State / Edition:", "vra_state")
        {
            Repeatable = true;
            Type = Element_Type.VRA_StateEdition;
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
                Acronym = "State, or edition, of this visual resource.";
            }

            // Start the list to collect all current instance values
            List<string> instanceValues = new List<string>();

            // Try to get any existing metadata module
            VRACore_Info vraInfo = Bib.Get_Metadata_Module(GlobalVar.VRACORE_METADATA_MODULE_KEY) as VRACore_Info;
            if (vraInfo != null)
            {
                foreach (  string thisValue in vraInfo.State_Editions )
                {
                    instanceValues.Add(thisValue);
                }
            }

            // Add to the current template (stream)
            render_helper(Output, instanceValues, Skin_Code, Current_User, CurrentLanguage, Translator, Base_URL);
        }

        /// <summary> Prepares the bib object for the save, by clearing any existing data in this element's related field(s) </summary>
        /// <param name="Bib"> Existing digital resource object which may already have values for this element's data field(s) </param>
        /// <param name="Current_User"> Current user, who's rights may impact the way an element is rendered </param>
        /// <remarks> This clears any preexisting resource type values </remarks>
        public override void Prepare_For_Save(SobekCM_Item Bib, User_Object Current_User)
        {
            // Try to get any existing metadata module
            VRACore_Info vraInfo = Bib.Get_Metadata_Module(GlobalVar.VRACORE_METADATA_MODULE_KEY) as VRACore_Info;
            if (vraInfo != null)
                vraInfo.Clear_State_Editions();
        }

        /// <summary> Saves the data rendered by this element to the provided bibliographic object during postback </summary>
        /// <param name="Bib"> Object into which to save the user's data, entered into the html rendered by this element </param>
        public override void Save_To_Bib(SobekCM_Item Bib)
        {
            // Try to get any existing metadata module
            VRACore_Info vraInfo = Bib.Get_Metadata_Module(GlobalVar.VRACORE_METADATA_MODULE_KEY) as VRACore_Info;


            string[] getKeys = HttpContext.Current.Request.Form.AllKeys;
            foreach (string thisKey in getKeys.Where(thisKey => thisKey.IndexOf(html_element_name.Replace("_", "")) == 0))
            {
                // Get the value from the form element
                string value = HttpContext.Current.Request.Form[thisKey].Trim();
                if (value.Length > 0)
                {
                    // There is a value, so ensure metadata does exist
                    if (vraInfo == null)
                    {
                        vraInfo = new VRACore_Info();
                        Bib.Add_Metadata_Module(GlobalVar.VRACORE_METADATA_MODULE_KEY, vraInfo);
                    }

                    // Add the value
                    vraInfo.Add_State_Edition(value);

                }
            }
        }
    }
}
