﻿#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// <summary> Element allows entry of the identifier for this item in the OCLC system </summary>
    /// <remarks> This class extends the <see cref="simpleTextBox_Element"/> class. </remarks>
    public class OCLC_Record_Number_Element : simpleTextBox_Element
    {
        /// <summary> Constructor for a new instance of the OCLC_Record_Number_Element class </summary>
        public OCLC_Record_Number_Element() : base("OCLC Number", "oclcnum")
        {
            Repeatable = false;
            Type = Element_Type.OCLC_Record_Number;
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
                const string defaultAcronym = "Identifier/number for this item in the OCLC system";
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

            const string identifierType = "OCLC";
            string identifier_value = String.Empty;
            foreach (Identifier_Info thisIdentifier in Bib.Bib_Info.Identifiers.Where(thisIdentifier => thisIdentifier.Type.ToUpper() == identifierType))
            {
                identifier_value = thisIdentifier.Identifier;
                break;
            }

            render_helper(Output, identifier_value, Skin_Code, Current_User, CurrentLanguage, Translator, Base_URL);
        }

        /// <summary> Prepares the bib object for the save, by clearing any existing data in this element's related field(s) </summary>
        /// <param name="Bib"> Existing digital resource object which may already have values for this element's data field(s) </param>
        /// <param name="Current_User"> Current user, who's rights may impact the way an element is rendered </param>
        /// <remarks> Clears any existing oclc record numbers </remarks>
        public override void Prepare_For_Save(SobekCM_Item Bib, User_Object Current_User)
        {
            const string identifierType = "OCLC";
            List<Identifier_Info> deletes = Bib.Bib_Info.Identifiers.Where(thisIdentifier => thisIdentifier.Type.ToUpper() == identifierType).ToList();
            foreach (Identifier_Info thisIdentifier in deletes)
            {
                Bib.Bib_Info.Remove_Identifier(thisIdentifier);
            }
        }

        /// <summary> Saves the data rendered by this element to the provided bibliographic object during postback </summary>
        /// <param name="Bib"> Object into which to save the user's data, entered into the html rendered by this element </param>
        public override void Save_To_Bib(SobekCM_Item Bib)
        {
            string[] getKeys = HttpContext.Current.Request.Form.AllKeys;
            foreach (string newNumber in from thisKey in getKeys where thisKey.IndexOf(html_element_name.Replace("_", "")) == 0 select HttpContext.Current.Request.Form[thisKey])
            {
                Bib.Bib_Info.Add_Identifier(newNumber, "OCLC");
                return;
            }
        }
    }
}