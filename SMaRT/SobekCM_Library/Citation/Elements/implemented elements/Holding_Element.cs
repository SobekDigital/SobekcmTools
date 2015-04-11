﻿#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Web;
using SobekCM.Resource_Object;
using SobekCM.Library.Aggregations;
using SobekCM.Library.Application_State;
using SobekCM.Library.Configuration;
using SobekCM.Library.Users;

#endregion

namespace SobekCM.Library.Citation.Elements
{
    /// <summary> Element allows entry of the holding location (code and statement) for an item </summary>
    /// <remarks> This class extends the <see cref="comboBox_TextBox_Element"/> class. </remarks>
    public class Holding_Element : comboBox_TextBox_Element
    {
        private Dictionary<string, string> codeToNameDictionary;

        /// <summary> Constructor for a new instance of the Holding_Element class </summary>
        public Holding_Element()
            : base("Holding Location", "holding")
        {
            Repeatable = false;
            possible_select_items.Add("");
            possible_select_items.Add("UF");
            Type = Element_Type.Holding;
            clear_textbox_on_combobox_change = true;
        }

        /// <summary> Sets the list of all valid codes for this element from the main aggregation table </summary>
        /// <param name="codeManager"> Code manager with list of all aggregations </param>
        internal void Add_Codes(Aggregation_Code_Manager codeManager)
        {
            codeToNameDictionary = new Dictionary<string, string>();

            if (possible_select_items.Count <= 2)
            {
                SortedList<string, string> tempItemList = new SortedList<string, string>();
                foreach (string thisType in codeManager.All_Types)
                {
                    if (thisType.IndexOf("Institution") >= 0)
                    {
                        ReadOnlyCollection<Item_Aggregation_Related_Aggregations> matchingAggr = codeManager.Aggregations_By_Type(thisType);
                        foreach (Item_Aggregation_Related_Aggregations thisAggr in matchingAggr)
                        {
                            if (thisAggr.Code.Length > 1)
                            {
                                if ((thisAggr.Code[0] == 'i') || (thisAggr.Code[0] == 'I'))
                                {
                                    if (!tempItemList.ContainsKey(thisAggr.Code.Substring(1)))
                                    {
                                        codeToNameDictionary[thisAggr.Code.Substring(1).ToUpper()] = thisAggr.Name;
                                        tempItemList.Add(thisAggr.Code.Substring(1), thisAggr.Code.Substring(1));
                                    }
                                }
                                else
                                {
                                    if (!tempItemList.ContainsKey(thisAggr.Code))
                                    {
                                        codeToNameDictionary[thisAggr.Code.ToUpper()] = thisAggr.Name;
                                        tempItemList.Add(thisAggr.Code, thisAggr.Code);
                                    }
                                }
                            }
                        }
                    }
                }

                IList<string> keys = tempItemList.Keys;
                foreach (string thisKey in keys)
                {
                    possible_select_items.Add(tempItemList[thisKey].ToUpper());
                }
            }
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
                const string defaultAcronym = "Holding location for the physical material, if this is a digital manifestation of a physical item.  Otherwise, the institution holding the digital version.";
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

            // This should always have a blank value
            if (!possible_select_items.Contains(String.Empty))
                possible_select_items.Insert(0,String.Empty);

            // Check the user to see if this should be limited
            bool some_set_as_selectable = false;
            List<string> possibles = new List<string> {Bib.Bib_Info.Location.Holding_Code.ToUpper()};
            if (!Current_User.Is_Internal_User)
            {
                // Are there aggregations set aside for the user?
                ReadOnlyCollection<User_Editable_Aggregation> allAggrs = Current_User.Aggregations;

                foreach (User_Editable_Aggregation thisAggr in allAggrs)
                {
                    if (thisAggr.CanSelect)
                    {
                        some_set_as_selectable = true;
                        string code = thisAggr.Code.ToUpper();
                        if ((code.Length > 1) && (code[0] == 'I'))
                            code = code.Substring(1);
                        if ((possible_select_items.Contains(code)) && (!possibles.Contains(code)))
                            possibles.Add(code);
                    }
                }
            }

            string holding_code = Bib.Bib_Info.Location.Holding_Code.ToUpper();
            if (some_set_as_selectable)
            {
                render_helper(Output, holding_code, possibles, Bib.Bib_Info.Location.Holding_Name, Skin_Code, Current_User, CurrentLanguage, Translator, Base_URL, false);
            }
            else
            {
                render_helper(Output, holding_code, Bib.Bib_Info.Location.Holding_Name, Skin_Code, Current_User, CurrentLanguage, Translator, Base_URL);
            }
        }

        /// <summary> Prepares the bib object for the save, by clearing any existing data in this element's related field(s) </summary>
        /// <param name="Bib"> Existing digital resource object which may already have values for this element's data field(s) </param>
        /// <param name="Current_User"> Current user, who's rights may impact the way an element is rendered </param>
        /// <remarks> This does nothing since there is only one holding location </remarks>
        public override void Prepare_For_Save(SobekCM_Item Bib, User_Object Current_User)
        {
            // Do nothing since there is only one holding location
        }

        /// <summary> Saves the data rendered by this element to the provided bibliographic object during postback </summary>
        /// <param name="Bib"> Object into which to save the user's data, entered into the html rendered by this element </param>
        public override void Save_To_Bib(SobekCM_Item Bib)
        {
            string[] getKeys = HttpContext.Current.Request.Form.AllKeys;
            foreach (string thisKey in getKeys)
            {
                if (thisKey.IndexOf(html_element_name.Replace("_", "") + "_select") == 0)
                {
                    Bib.Bib_Info.Location.Holding_Code = HttpContext.Current.Request.Form[thisKey].ToUpper();
                }

                if (thisKey.IndexOf(html_element_name.Replace("_", "") + "_text") == 0)
                {
                    string temp = HttpContext.Current.Request.Form[thisKey];
                    if ((temp.Trim().Length == 0) && (Bib.Bib_Info.Location.Holding_Code.Length > 0))
                    {
                        if ((codeToNameDictionary != null) && (codeToNameDictionary.ContainsKey(Bib.Bib_Info.Location.Holding_Code)))
                        {
                            Bib.Bib_Info.Location.Holding_Name = codeToNameDictionary[Bib.Bib_Info.Location.Holding_Code];
                        }
                    }
                    else
                    {
                        Bib.Bib_Info.Location.Holding_Name = temp;
                    }
                }
            }
        }
    }
}
