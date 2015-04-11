﻿#region Using directives

using System;
using System.IO;
using System.Text;
using System.Web;
using SobekCM.Resource_Object;
using SobekCM.Resource_Object.Metadata_Modules;
using SobekCM.Resource_Object.Metadata_Modules.LearningObjects;
using SobekCM.Library.Application_State;
using SobekCM.Library.Configuration;
using SobekCM.Library.Users;

#endregion

namespace SobekCM.Library.Citation.Elements
{
    /// <summary> Element allows entry of the learning object metadata interactivity level field </summary>
    /// <remarks> This class extends the <see cref="comboBox_Element"/> class. </remarks>
    public class LOM_InteractivityLevel_Element : comboBox_Element
    {
        private const string level1_text = "very low";
        private const string level2_text = "low";
        private const string level3_text = "medium";
        private const string level4_text = "high";
        private const string level5_text = "very high";

        /// <summary> Constructor for a new instance of the LOM_InteractivityLevel_Element class </summary>
        public LOM_InteractivityLevel_Element()
            : base("Interactivity Level", "lom_interactlevel")
        {
            Repeatable = false;
            Type = Element_Type.LOM_Interactivity_Level;

            items.Clear();
            items.Add(String.Empty);
            items.Add(level1_text);
            items.Add(level2_text);
            items.Add(level3_text);
            items.Add(level4_text);
            items.Add(level5_text);
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
                const string defaultAcronym = "Degree of interactivity characterizing this learning object.  Refers to degree to which the learner can influence the aspect or behavior of the learning object.";
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

            // Determine the value from the enum
            string value = String.Empty;
            
            // Try to get the learning metadata here
            LearningObjectMetadata lomInfo = Bib.Get_Metadata_Module(GlobalVar.IEEE_LOM_METADATA_MODULE_KEY) as LearningObjectMetadata;
            if (lomInfo != null)
            {
                switch ( lomInfo.InteractivityLevel )
                {
                    case InteractivityLevelEnum.very_low:
                        value = level1_text;
                        break;

                    case InteractivityLevelEnum.low:
                        value = level2_text;
                        break;

                    case InteractivityLevelEnum.medium:
                        value = level3_text;
                        break;

                    case InteractivityLevelEnum.high:
                        value = level4_text;
                        break;

                    case InteractivityLevelEnum.very_high:
                        value = level5_text;
                        break;
                }
            }

            render_helper(Output, value, Skin_Code, Current_User, CurrentLanguage, Translator, Base_URL);
        }

        /// <summary> Prepares the bib object for the save, by clearing any existing data in this element's related field(s) </summary>
        /// <param name="Bib"> Existing digital resource object which may already have values for this element's data field(s) </param>
        /// <param name="Current_User"> Current user, who's rights may impact the way an element is rendered </param>
        /// <remarks> This does nothing since this is a singleton value, and is non-repeatable </remarks>
        public override void Prepare_For_Save(SobekCM_Item Bib, User_Object Current_User)
        {
            // Do nothing since there is only one corresponding value
        }

        /// <summary> Saves the data rendered by this element to the provided bibliographic object during postback </summary>
        /// <param name="Bib"> Object into which to save the user's data, entered into the html rendered by this element </param>
        public override void Save_To_Bib(SobekCM_Item Bib)
        {
            string[] getKeys = HttpContext.Current.Request.Form.AllKeys;
            foreach (string thisKey in getKeys)
            {
                if (thisKey.IndexOf(html_element_name.Replace("_","")) == 0)
                {
                    // Get the value from the combo box
                    string value = HttpContext.Current.Request.Form[thisKey].Trim();

                    // Try to get any existing learning object metadata module
                    LearningObjectMetadata lomInfo = Bib.Get_Metadata_Module(GlobalVar.IEEE_LOM_METADATA_MODULE_KEY) as LearningObjectMetadata;

                    if (value.Length == 0)
                    {
                        // I fhte learning object metadata does exist, set it to undefined
                        if ( lomInfo != null )
                            lomInfo.InteractivityLevel = InteractivityLevelEnum.UNDEFINED;
                    }
                    else
                    {
                        // There is a value, so ensure learning object metadata does exist
                        if (lomInfo == null)
                        {
                            lomInfo = new LearningObjectMetadata();
                            Bib.Add_Metadata_Module(GlobalVar.IEEE_LOM_METADATA_MODULE_KEY, lomInfo);
                        }

                        // Save the new value
                        switch ( value )
                        {
                            case level1_text:
                                lomInfo.InteractivityLevel = InteractivityLevelEnum.very_low;
                                break;

                            case level2_text:
                                lomInfo.InteractivityLevel = InteractivityLevelEnum.low;
                                break;

                            case level3_text:
                                lomInfo.InteractivityLevel = InteractivityLevelEnum.medium;
                                break;

                            case level4_text:
                                lomInfo.InteractivityLevel = InteractivityLevelEnum.high;
                                break;

                            case level5_text:
                                lomInfo.InteractivityLevel = InteractivityLevelEnum.very_high;
                                break;
                        }
                    }
                    return;
                }
            }            
        }
    }
}