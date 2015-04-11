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
    /// <summary> Element allows entry of subject terms and a subject scheme for an item </summary>
    /// <remarks> This class extends the <see cref="textBox_TextBox_Element"/> class. </remarks>
    public class Subject_Scheme_Element : textBox_TextBox_Element
    {
        /// <summary> Constructor for a new instance of the Subject_Scheme_Element class </summary>
        public Subject_Scheme_Element()
            : base("Subject Keywords:", "subject_scheme")
        {
            second_label = "Scheme";
            Repeatable = true;
            Display_SubType = "scheme";
            Type = Element_Type.Subject;
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
                const string defaultAcronym = "Enter any subject keyword to describe your material here, along with the vocabulary from which this subject term was pulled.";
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

            List<string> terms = new List<string>();
            List<string> schemes = new List<string>();
            if (Bib.Bib_Info.Subjects_Count > 0)
            {
                foreach (Subject_Info_Standard standSubject in Bib.Bib_Info.Subjects.Where(thisSubject => thisSubject.Class_Type == Subject_Info_Type.Standard).Cast<Subject_Info_Standard>())
                {
                    terms.Add(standSubject.ToString().Replace("<i>", "").Replace("</i>", "").Replace("( " + standSubject.Authority + " )", "").Trim());
                    schemes.Add(standSubject.Authority.ToLower());
                }
            }


            render_helper(Output, terms, schemes, Skin_Code, Current_User, CurrentLanguage, Translator, Base_URL);
        }

        /// <summary> Prepares the bib object for the save, by clearing any existing data in this element's related field(s) </summary>
        /// <param name="Bib"> Existing digital resource object which may already have values for this element's data field(s) </param>
        /// <param name="Current_User"> Current user, who's rights may impact the way an element is rendered </param>
        /// <remarks> This clears any preexisting standard subjects ( not hierarchical geographic, title, etc..) </remarks>
        public override void Prepare_For_Save(SobekCM_Item Bib, User_Object Current_User)
        {
            if (Bib.Bib_Info.Subjects_Count > 0)
            {
                List<Subject_Info_Standard> clears = Bib.Bib_Info.Subjects.Where(thisSubject => thisSubject.Class_Type == Subject_Info_Type.Standard).Cast<Subject_Info_Standard>().ToList();
                foreach (Subject_Info_Standard clearSubject in clears)
                {
                    Bib.Bib_Info.Remove_Subject(clearSubject);
                }
            }
        }

        /// <summary> Saves the data rendered by this element to the provided bibliographic object during postback </summary>
        /// <param name="Bib"> Object into which to save the user's data, entered into the html rendered by this element </param>
        public override void Save_To_Bib(SobekCM_Item Bib)
        {
            Dictionary<string,string> terms = new Dictionary<string,string>();
            Dictionary<string,string> schemes = new Dictionary<string,string>();

            string[] getKeys = HttpContext.Current.Request.Form.AllKeys;
            foreach (string thisKey in getKeys)
            {
                if (thisKey.IndexOf(html_element_name.Replace("_", "") + "_first") == 0)
                {
                    string term = HttpContext.Current.Request.Form[thisKey];
                    string index = thisKey.Replace(html_element_name.Replace("_", "") + "_first", "");
                    terms[index] = term;
                }

                if (thisKey.IndexOf(html_element_name.Replace("_", "") + "_second") == 0)
                {
                    string scheme = HttpContext.Current.Request.Form[thisKey];
                    string index = thisKey.Replace(html_element_name.Replace("_", "") + "_second", "");
                    schemes[index] = scheme;
                }
            }

            foreach (string index in terms.Keys)
            {
                Bib.Bib_Info.Add_Subject(terms[index], schemes.ContainsKey(index) ? schemes[index] : String.Empty);
            }
        }
    }
}
