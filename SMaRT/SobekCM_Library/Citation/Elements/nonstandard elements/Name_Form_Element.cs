﻿#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using SobekCM.Resource_Object;
using SobekCM.Resource_Object.Bib_Info;
using SobekCM.Library.Application_State;
using SobekCM.Library.Configuration;
using SobekCM.Library.Users;

#endregion

namespace SobekCM.Library.Citation.Elements
{
    /// <summary> Element displays a form to allow for complete entry of named entity for an item </summary>
    /// <remarks> This class implements the <see cref="iElement"/> interface and extends the <see cref="abstract_Element"/> class. </remarks>
    class Name_Form_Element : abstract_Element
    {
        /// <summary> Constructor for a new instance of the Name_Form_Element class </summary>
        public Name_Form_Element()
        {
            Type = Element_Type.Creator;
            Repeatable = true;
            Display_SubType = "form";
            Title = "Creator";
            html_element_name = "form_creator";
        }

        #region iElement Members

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
        /// <remarks> This element appends a popup form to the popup_form_builder</remarks>
        public override void Render_Template_HTML(TextWriter Output, SobekCM_Item Bib, string Skin_Code, bool isMozilla, StringBuilder popup_form_builder, User_Object Current_User, Web_Language_Enum CurrentLanguage, Language_Support_Info Translator, string Base_URL )
        {
            // Check that an acronym exists
            if (Acronym.Length == 0)
            {
                const string defaultAcronym = "Enter each person or group which created this material. Personal names should be entered as [Family Name], [Given Name].";
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

            // Render this in HTML
            Output.WriteLine("  <!-- " + Title + " Form Element -->");
            Output.WriteLine("  <tr align=\"left\">");
            Output.WriteLine("    <td width=\"" + LEFT_MARGIN + "px\">&nbsp;</td>");
            if (Acronym.Length > 0)
            {
                Output.WriteLine("    <td valign=\"top\" class=\"metadata_label\"><a href=\"" + Help_URL(Skin_Code, Base_URL) + "\" target=\"_" + html_element_name.ToUpper() + "\"><acronym title=\"" + Acronym + "\">" + Translator.Get_Translation(Title, CurrentLanguage) + ":</acronym></a></td>");
            }
            else
            {
                Output.WriteLine("    <td valign=\"top\" class=\"metadata_label\"><a href=\"" + Help_URL(Skin_Code, Base_URL) + "\" target=\"_" + html_element_name.ToUpper() + "\">" + Translator.Get_Translation(Title, CurrentLanguage) + ":</a></td>");
            }

            Output.WriteLine("    <td>");
            Output.WriteLine("      <table>");
            Output.WriteLine("        <tr>");
            Output.WriteLine("          <td>");
            Output.WriteLine("            <div class=\"form_name_div\">");

            // Collect all the names
            int name_count = 1;
            bool first_is_main = false;
            List<Name_Info> names = new List<Name_Info>();
            if (( Bib.Bib_Info.hasMainEntityName ) && ( Bib.Bib_Info.Main_Entity_Name.hasData))
            {
                names.Add(Bib.Bib_Info.Main_Entity_Name);
                first_is_main = true;
            }
            if (Bib.Bib_Info.Names_Count > 0)
            {
                names.AddRange(Bib.Bib_Info.Names);
            }

            // There should always be one name at least
            if (names.Count == 0)
                names.Add(new Name_Info());

            // Step through and create the popup forms and inks
            foreach (Name_Info thisName in names)
            {
                // Add the link for the other title
                string thisNameText = thisName.ToString();
                if (!thisName.hasData)
                {
                    thisNameText = "<i>Empty Name</i>";
                    Output.WriteLine("              <a title=\"Click to edit this named entity\" href=\"" + Base_URL + "l/technical/javascriptrequired\" onfocus=\"link_focused2('form_name_line_" + name_count + "')\" onblur=\"link_blurred2('form_name_line_" + name_count + "')\" onkeypress=\"return popup_keypress_focus('form_name_" + name_count + "', 'form_name_line_" + name_count + "', 'form_name_full_" + name_count + "', 375, 700, '" + isMozilla.ToString() + "' );\" onclick=\"return popup_focus('form_name_" + name_count + "', 'form_name_line_" + name_count + "', 'form_name_full_" + name_count + "', 375, 700 );\"><div class=\"form_linkline_empty form_name_line\" id=\"form_name_line_" + name_count + "\">" + thisNameText + "</div></a>");

                }
                else
                {
                    Output.WriteLine("              <a title=\"Click to edit this named entity\" href=\"" + Base_URL + "l/technical/javascriptrequired\" onfocus=\"link_focused2('form_name_line_" + name_count + "')\" onblur=\"link_blurred2('form_name_line_" + name_count + "')\" onkeypress=\"return popup_keypress_focus('form_name_" + name_count + "', 'form_name_line_" + name_count + "', 'form_name_full_" + name_count + "', 375, 700, '" + isMozilla.ToString() + "' );\" onclick=\"return popup_focus('form_name_" + name_count + "', 'form_name_line_" + name_count + "', 'form_name_full_" + name_count + "', 375, 700 );\"><div class=\"form_linkline form_name_line\" id=\"form_name_line_" + name_count + "\">" + thisNameText + "</div></a>");
                }

                // Determine if this is a personal name
                string personal_values_display = "none";
                string description_location_display = "Location";
                string form_class = "name_popup_div";
                if ((thisName.Name_Type == Name_Info_Type_Enum.personal) || ( thisName.Name_Type == Name_Info_Type_Enum.UNKNOWN ))
                {
                    personal_values_display = "inline";
                    description_location_display = "Description";
                    form_class = "name_popup_div_personal";
                }

                // Add the popup form
                popup_form_builder.AppendLine("<!-- Name Form " + name_count + " -->");
                popup_form_builder.AppendLine("<div class=\"" + form_class + "\" id=\"form_name_" + name_count + "\" style=\"display:none;\">");
                popup_form_builder.AppendLine("  <div class=\"popup_title\"><table width=\"100%\"><tr><td align=\"left\">EDIT NAMED ENTITY</td><td align=\"right\"><a href=\"" + Help_URL(Skin_Code, Base_URL) + "\" alt=\"HELP\" target=\"_" + html_element_name.ToUpper() + "\" >?</a> &nbsp; <a href=\"#template\" alt=\"CLOSE\" onclick=\"close_name_form('form_name_" + name_count + "')\">X</a> &nbsp; </td></tr></table></div>");
                popup_form_builder.AppendLine("  <br />");
                popup_form_builder.AppendLine("  <table class=\"popup_table\">");

                // Add the name type combo box and radio buttons
                popup_form_builder.AppendLine("    <tr>");
                popup_form_builder.AppendLine("      <td>Name Type:</td>");
                popup_form_builder.AppendLine("      <td>");
                popup_form_builder.AppendLine("        <select class=\"form_name_select\" id=\"form_name_type_" + name_count + "\" name=\"form_name_type_" + name_count + "\" onChange=\"javascript:name_type_changed('" + name_count + "')\" >");
                popup_form_builder.AppendLine(thisName.Name_Type == Name_Info_Type_Enum.conference
                                                  ? "          <option value=\"conference\" selected=\"selected\" >Conference</option>"
                                                  : "          <option value=\"conference\">Conference</option>");

                popup_form_builder.AppendLine(thisName.Name_Type == Name_Info_Type_Enum.corporate
                                                  ? "          <option value=\"corporate\" selected=\"selected\" >Corporate</option>"
                                                  : "          <option value=\"corporate\">Corporate</option>");

                if (( thisName.Name_Type == Name_Info_Type_Enum.personal ) || ( thisName.Name_Type == Name_Info_Type_Enum.UNKNOWN ))
                    popup_form_builder.AppendLine("          <option value=\"personal\" selected=\"selected\" >Personal</option>");
                else
                    popup_form_builder.AppendLine("          <option value=\"personal\">Personal</option>");

                popup_form_builder.AppendLine("        </select>");
                popup_form_builder.AppendLine("      </td>");
                if (( name_count == 1 ) && ( first_is_main ))
                {

                    popup_form_builder.Append("      <td><input type=\"radio\" name=\"form_name_main_" + name_count + "\" id=\"form_name_main_main_" + name_count + "\" value=\"main\" checked=\"checked\" onclick=\"focus_element( 'form_name_full_" + name_count + "');\" /><label for=\"form_name_main_main_" + name_count + "\">Principal Author</label> &nbsp; &nbsp; &nbsp; &nbsp; ");
                    popup_form_builder.AppendLine("<input type=\"radio\" name=\"form_name_main_" + name_count + "\" id=\"form_name_main_added_" + name_count + "\" value=\"added\" onclick=\"focus_element( 'form_name_full_" + name_count + "');\" /><label for=\"form_name_main_added_" + name_count + "\">Other Author</label></td>");
                }
                else
                {
                    popup_form_builder.Append("      <td><input type=\"radio\" name=\"form_name_main_" + name_count + "\" id=\"form_name_main_main_" + name_count + "\" value=\"main\" onclick=\"focus_element( 'form_name_full_" + name_count + "');\" /><label for=\"form_name_main_main_" + name_count + "\">Main Entry</label> &nbsp; &nbsp; &nbsp; &nbsp; ");
                    popup_form_builder.AppendLine("<input type=\"radio\" name=\"form_name_main_" + name_count + "\" id=\"form_name_main_added_" + name_count + "\" value=\"added\" checked=\"checked\" onclick=\"focus_element( 'form_name_full_" + name_count + "');\" /><label for=\"form_name_main_added_" + name_count + "\">Added Entry</label></td>");
                }
                popup_form_builder.AppendLine("    </tr>");

                // Add the full name box
                popup_form_builder.AppendLine("    <tr><td>Full Name:</td><td colspan=\"2\"><input type=\"text\" class=\"form_name_large_input\" id=\"form_name_full_" + name_count + "\" name=\"form_name_full_" + name_count + "\" value=\"" + HttpUtility.HtmlEncode(thisName.Full_Name) + "\" onfocus=\"javascript:textbox_enter('form_name_full_" + name_count + "', 'form_name_large_input_focused')\" onblur=\"javascript:textbox_leave('form_name_full_" + name_count + "', 'form_name_large_input')\" /></td></tr>");

                // Add the given name and family name(s)
                popup_form_builder.Append("    <tr><td><span  id=\"name_personallabel1_" + name_count + "\" style=\"display:" + personal_values_display + ";\" >Given Names:</span></td><td><input type=\"text\" class=\"form_name_medium_input\" id=\"form_name_given_" + name_count + "\" name=\"form_name_given_" + name_count + "\" value=\"" + HttpUtility.HtmlEncode(thisName.Given_Name) + "\" onfocus=\"javascript:textbox_enter('form_name_given_" + name_count + "', 'form_name_medium_input_focused')\" onblur=\"javascript:textbox_leave('form_name_given_" + name_count + "', 'form_name_medium_input')\" style=\"display:" + personal_values_display + ";\" /></td>");
                popup_form_builder.AppendLine("<td><span  id=\"name_personallabel2_" + name_count + "\" style=\"display:" + personal_values_display + ";\" >Family Name:</span><input type=\"text\" class=\"form_name_medium_input\" id=\"form_name_family_" + name_count + "\" name=\"form_name_family_" + name_count + "\" value=\"" + HttpUtility.HtmlEncode(thisName.Family_Name) + "\" onfocus=\"javascript:textbox_enter('form_name_family_" + name_count + "', 'form_name_medium_input_focused')\" onblur=\"javascript:textbox_leave('form_name_family_" + name_count + "', 'form_name_medium_input')\" style=\"display:" + personal_values_display + ";\" /></td></tr>");

                // Add the display form and terms of address
                popup_form_builder.Append("    <tr><td><span  id=\"name_personallabel3_" + name_count + "\" style=\"display:" + personal_values_display + ";\" >Display Form:</span></td><td><input type=\"text\" class=\"form_name_medium_input\" id=\"form_name_display_" + name_count + "\" name=\"form_name_display_" + name_count + "\" value=\"" + HttpUtility.HtmlEncode(thisName.Display_Form) + "\" onfocus=\"javascript:textbox_enter('form_name_display_" + name_count + "', 'form_name_medium_input_focused')\" onblur=\"javascript:textbox_leave('form_name_display_" + name_count + "', 'form_name_medium_input')\" style=\"display:" + personal_values_display + ";\" /></td>");
                popup_form_builder.AppendLine("<td><span  id=\"name_personallabel4_" + name_count + "\" style=\"display:" + personal_values_display + ";\" >Terms of Address:</span><input type=\"text\" class=\"form_name_small_input\" id=\"form_name_terms_" + name_count + "\" name=\"form_name_terms_" + name_count + "\" value=\"" + HttpUtility.HtmlEncode(thisName.Terms_Of_Address) + "\" onfocus=\"javascript:textbox_enter('form_name_terms_" + name_count + "', 'form_name_small_input_focused')\" onblur=\"javascript:textbox_leave('form_name_terms_" + name_count + "', 'form_name_small_input')\" style=\"display:" + personal_values_display + ";\" /></td></tr>");

                // Add the dates
                popup_form_builder.AppendLine("    <tr><td>Dates:</td><td colspan=\"2\"><input type=\"text\" class=\"form_name_medium_input\" id=\"form_name_dates_" + name_count + "\" name=\"form_name_dates_" + name_count + "\" value=\"" + HttpUtility.HtmlEncode(thisName.Dates) + "\" onfocus=\"javascript:textbox_enter('form_name_dates_" + name_count + "', 'form_name_medium_input_focused')\" onblur=\"javascript:textbox_leave('form_name_dates_" + name_count + "', 'form_name_medium_input')\" /></td></tr>");

                // Add the description
                popup_form_builder.AppendLine("    <tr><td><span id=\"name_desc_location_span_" + name_count + "\">" + description_location_display + ":</span></td><td colspan=\"2\"><input type=\"text\" class=\"form_name_large_input\" id=\"form_name_desc_" + name_count + "\" name=\"form_name_desc_" + name_count + "\" value=\"" + HttpUtility.HtmlEncode(thisName.Description) + "\" onfocus=\"javascript:textbox_enter('form_name_desc_" + name_count + "', 'form_name_large_input_focused')\" onblur=\"javascript:textbox_leave('form_name_desc_" + name_count + "', 'form_name_large_input')\" /></td></tr>");

                // Add the affiliation
                popup_form_builder.AppendLine("    <tr><td>Affiliation:</td><td colspan=\"2\"><input type=\"text\" class=\"form_name_large_input\" id=\"form_name_affiliation_" + name_count + "\" name=\"form_name_affiliation_" + name_count + "\" value=\"" + HttpUtility.HtmlEncode(thisName.Affiliation) + "\" onfocus=\"javascript:textbox_enter('form_name_affiliation_" + name_count + "', 'form_name_large_input_focused')\" onblur=\"javascript:textbox_leave('form_name_affiliation_" + name_count + "', 'form_name_large_input')\" /></td></tr>");

                // Determine the roles to display
                string role1 = String.Empty;
                string role2 = String.Empty;
                string role3 = String.Empty;
                int role_index = 1;
                foreach (Name_Info_Role thisRole in thisName.Roles)
                {
                    if ((thisRole.Role.Length > 0) && (thisRole.Role_Type == Name_Info_Role_Type_Enum.text))
                    {
                        switch (role_index)
                        {
                            case 1:
                                role1 = thisRole.Role;
                                break;

                            case 2:
                                role2 = thisRole.Role;
                                break;

                            case 3:
                                role3 = thisRole.Role;
                                break;
                        }
                        role_index++;
                    }
                }

                // Add the roles
                popup_form_builder.Append("    <tr><td>Roles:</td><td colspan=\"2\">");
                popup_form_builder.Append("<input type=\"text\" class=\"form_name_small_input\" id=\"form_name_role1_" + name_count + "\" name=\"form_name_role1_" + name_count + "\" value=\"" + HttpUtility.HtmlEncode(role1) + "\" onfocus=\"javascript:textbox_enter('form_name_role1_" + name_count + "', 'form_name_small_input_focused')\" onblur=\"javascript:textbox_leave('form_name_role1_" + name_count + "', 'form_name_small_input')\" />");
                popup_form_builder.Append("<input type=\"text\" class=\"form_name_small_input\" id=\"form_name_role2_" + name_count + "\" name=\"form_name_role2_" + name_count + "\" value=\"" + HttpUtility.HtmlEncode(role2) + "\" onfocus=\"javascript:textbox_enter('form_name_role2_" + name_count + "', 'form_name_small_input_focused')\" onblur=\"javascript:textbox_leave('form_name_role2_" + name_count + "', 'form_name_small_input')\" />");
                popup_form_builder.Append("<input type=\"text\" class=\"form_name_small_input\" id=\"form_name_role3_" + name_count + "\" name=\"form_name_role3_" + name_count + "\" value=\"" + HttpUtility.HtmlEncode(role3) + "\" onfocus=\"javascript:textbox_enter('form_name_role3_" + name_count + "', 'form_name_small_input_focused')\" onblur=\"javascript:textbox_leave('form_name_role3_" + name_count + "', 'form_name_small_input')\" />");
                popup_form_builder.AppendLine("</td></tr>");
                

                popup_form_builder.AppendLine("  </table>");
                popup_form_builder.AppendLine("  <br />");

                // Add the close button
                popup_form_builder.AppendLine("  <center><a href=\"#template\" onclick=\"return close_name_form('form_name_" + name_count + "')\"><img border=\"0\" src=\"" + Close_Button_URL(Skin_Code, Base_URL ) + "\" alt=\"CLOSE\" /></a></center>");
                popup_form_builder.AppendLine("</div>");
                popup_form_builder.AppendLine();

                name_count++;
            }

            // Add the link to add a new name
            //<span class=\"add_new_link\"><a href=\"#template\" onclick=\"new_creator_link_clicked('" + Template_Page + "')\">Add New Creator</a></span>");

            Output.WriteLine("            </div>");
            Output.WriteLine("          </td>");

            Output.WriteLine("          <td valign=\"bottom\" >");
            if (Repeatable)
            {
                Output.WriteLine("            <img title=\"" + Translator.Get_Translation("Click to add a new named object to this resource", CurrentLanguage) + ".\" alt=\"+\" border=\"0px\" class=\"repeat_button\" src=\"" + Base_URL + REPEAT_BUTTON_URL + "\" onmousedown=\"return new_creator_link_clicked('" + Template_Page + "');\" />");
            }
            Output.WriteLine("            <a target=\"_" + html_element_name.ToUpper() + "\"  title=\"" + Translator.Get_Translation("Get help.", CurrentLanguage) + "\" href=\"" + Help_URL(Skin_Code, Base_URL) + "\" ><img border=\"0px\" class=\"help_button\" src=\"" + Base_URL + HELP_BUTTON_URL + "\" /></a>");
            Output.WriteLine("          </td>");

            Output.WriteLine("        </tr>");
            Output.WriteLine("      </table>");
            Output.WriteLine("    </td>");
            Output.WriteLine("  </tr>");
            Output.WriteLine();
        }

        /// <summary> Prepares the bib object for the save, by clearing any existing data in this element's related field(s) </summary>
        /// <param name="Bib"> Existing digital resource object which may already have values for this element's data field(s) </param>
        /// <param name="Current_User"> Current user, who's rights may impact the way an element is rendered </param>
        ///<remarks> This clears the main entity name and any other names associated with the digital resource </remarks>
        public override void Prepare_For_Save(SobekCM_Item Bib, User_Object Current_User)
        {
            if ( Bib.Bib_Info.hasMainEntityName )
                Bib.Bib_Info.Main_Entity_Name.Clear();
            Bib.Bib_Info.Clear_Names();
        }

        /// <summary> Saves the data rendered by this element to the provided bibliographic object during postback </summary>
        /// <param name="Bib"> Object into which to save the user's data, entered into the html rendered by this element </param>
        public override void Save_To_Bib(SobekCM_Item Bib)
        {
            bool main_is_found = false;
            string[] getKeys = HttpContext.Current.Request.Form.AllKeys;
            foreach (string thisKey in getKeys)
            {
                if (thisKey.IndexOf("form_name_type_") == 0)
                {
                    string type = HttpContext.Current.Request.Form[thisKey].Trim();
                    string diff = thisKey.Replace("form_name_type_", "");
                    string main = HttpContext.Current.Request.Form["form_name_main_" + diff].Trim();
                    string full = HttpContext.Current.Request.Form["form_name_full_" + diff ].Trim();
                    string given = HttpContext.Current.Request.Form["form_name_given_" + diff].Trim();
                    string family = HttpContext.Current.Request.Form["form_name_family_" + diff].Trim();
                    string display = HttpContext.Current.Request.Form["form_name_display_" + diff].Trim();
                    string terms = HttpContext.Current.Request.Form["form_name_terms_" + diff].Trim();
                    string dates = HttpContext.Current.Request.Form["form_name_dates_" + diff].Trim();
                    string desc = HttpContext.Current.Request.Form["form_name_desc_" + diff].Trim();
                    string affiliation = HttpContext.Current.Request.Form["form_name_affiliation_" + diff].Trim();
                    string role1 = HttpContext.Current.Request.Form["form_name_role1_" + diff].Trim();
                    string role2 = HttpContext.Current.Request.Form["form_name_role2_" + diff].Trim();
                    string role3 = HttpContext.Current.Request.Form["form_name_role3_" + diff].Trim();

                    if ((full.Length > 0) || (given.Length > 0) || (family.Length > 0))
                    {
                        Name_Info newName;
                        if ((main == "main") && ( !main_is_found ))
                        {
                            newName = Bib.Bib_Info.Main_Entity_Name;
                            main_is_found = true;
                        }
                        else
                        {
                            newName = new Name_Info();
                            Bib.Bib_Info.Add_Named_Entity(newName);
                        }

                        switch (type)
                        {
                            case "corporate":
                                newName.Name_Type = Name_Info_Type_Enum.corporate;
                                break;

                            case "conference":
                                newName.Name_Type = Name_Info_Type_Enum.conference;
                                break;

                            default:
                                newName.Name_Type = Name_Info_Type_Enum.personal;
                                break;
                        }
                        newName.Full_Name = full;
                        if (newName.Name_Type == Name_Info_Type_Enum.personal)
                        {
                            newName.Given_Name = given;
                            newName.Family_Name = family;
                            newName.Display_Form = display;
                            newName.Terms_Of_Address = terms;  
                        }

                        newName.Dates = dates;
                        newName.Affiliation = affiliation;
                        newName.Description = desc;

                        if (role1.Length > 0)
                            newName.Add_Role(role1);
                        if (role2.Length > 0)
                            newName.Add_Role(role2);
                        if (role3.Length > 0)
                            newName.Add_Role(role3);

                    }
                }
            }          

        }

        #endregion

        #region Methods Implementing the Abstract Methods from abstract_Element class

        /// <summary> Reads the inner data from the Template XML format </summary>
        /// <param name="xmlReader"> Current template xml configuration reader </param>
        /// <remarks> This procedure does not currently read any inner xml (not yet necessary) </remarks>
        protected override void Inner_Read_Data(XmlTextReader xmlReader)
        {
            // Do nothing
        }

        #endregion
    }
}