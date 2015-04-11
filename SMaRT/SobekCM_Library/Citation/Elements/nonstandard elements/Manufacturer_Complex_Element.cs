﻿#region Using directives

using System.Collections.ObjectModel;
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
    /// <summary> Element allows entry of the manufacturer(s) name and locations for an item </summary>
    /// <remarks> This class implements the <see cref="iElement"/> interface and extends the <see cref="abstract_Element"/> class. </remarks>
    public class Manufacturer_Complex_Element : abstract_Element
    {
        /// <summary> Constructor for a new instance of the Manufacturer_Complex_Element class </summary>
        public Manufacturer_Complex_Element()
        {
            Repeatable = true;
            Display_SubType = "complex";
            Type = Element_Type.Manufacturer;
            Title = "Manufacturer";
            html_element_name = "complex_manufacturer";
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
        /// <remarks> This simple element does not append any popup form to the popup_form_builder</remarks>
        public override void Render_Template_HTML(TextWriter Output, SobekCM_Item Bib, string Skin_Code, bool isMozilla, StringBuilder popup_form_builder, User_Object Current_User, Web_Language_Enum CurrentLanguage, Language_Support_Info Translator, string Base_URL )
        {
            // Check that an acronym exists
            if (Acronym.Length == 0)
            {
                const string defaultAcronym = "Enter the information about the manufacturer of the original item.";
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

            string id_name = html_element_name.Replace("_", "");

            Output.WriteLine("  <!-- " + Title + " Element -->");
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
            Output.WriteLine("            <div id=\"" + html_element_name + "_div\">");

            if (Bib.Bib_Info.Manufacturers_Count == 0)
            {
                const int i = 1;
                Output.WriteLine("              <span class=\"metadata_sublabel2\">" + Translator.Get_Translation("Name", CurrentLanguage) + ":</span>");
                Output.Write("              <input name=\"" + id_name + "_name" + i + "\" id=\"" + id_name + "_name" + i + "\" class=\"" + html_element_name + "_name_input\" type=\"text\" value=\"\" onfocus=\"javascript:textbox_enter('" + id_name + "_name" + i + "', '" + html_element_name + "_name_input_focused')\" onblur=\"javascript:textbox_leave('" + id_name + "_name" + i + "', '" + html_element_name + "_name_input')\" />");
                Output.WriteLine("<br />");
                Output.WriteLine("              <span class=\"metadata_sublabel2\">" + Translator.Get_Translation("Location(s)", CurrentLanguage) + ":</span>");
                Output.WriteLine("              <input name=\"" + id_name + "_firstloc" + i + "\" id=\"" + id_name + "_firstloc" + i + "\" class=\"" + html_element_name + "_location_input\" type=\"text\" value=\"\" onfocus=\"javascript:textbox_enter('" + id_name + "_firstloc" + i + "', '" + html_element_name + "_location_input_focused')\" onblur=\"javascript:textbox_leave('" + id_name + "_firstloc" + i + "', '" + html_element_name + "_location_input')\" />");
                Output.WriteLine("              <input name=\"" + id_name + "_secondloc" + i + "\" id=\"" + id_name + "_secondloc" + i + "\" class=\"" + html_element_name + "_location_input\" type=\"text\" value=\"\" onfocus=\"javascript:textbox_enter('" + id_name + "_secondloc" + i + "', '" + html_element_name + "_location_input_focused')\" onblur=\"javascript:textbox_leave('" + id_name + "_secondloc" + i + "', '" + html_element_name + "_location_input')\" />");
                Output.Write("              <input name=\"" + id_name + "_thirdloc" + i + "\" id=\"" + id_name + "_thirdloc" + i + "\" class=\"" + html_element_name + "_location_input\" type=\"text\" value=\"\" onfocus=\"javascript:textbox_enter('" + id_name + "_thirdloc" + i + "', '" + html_element_name + "_location_input_focused')\" onblur=\"javascript:textbox_leave('" + id_name + "_thirdloc" + i + "', '" + html_element_name + "_location_input')\" />");
                Output.WriteLine("</div>");
                Output.WriteLine("          </td>");
            }
            else
            {
                ReadOnlyCollection<Publisher_Info> manufacturers = Bib.Bib_Info.Manufacturers;
                for (int i = 1; i <= manufacturers.Count; i++)
                {
                    Output.WriteLine("              <span class=\"metadata_sublabel2\">" + Translator.Get_Translation("Name", CurrentLanguage) + ":</span>");
                    Output.Write("              <input name=\"" + id_name + "_name" + i + "\" id=\"" + id_name + "_name" + i + "\" class=\"" + html_element_name + "_name_input\" type=\"text\" value=\"" + HttpUtility.HtmlEncode(manufacturers[i - 1].Name) + "\" onfocus=\"javascript:textbox_enter('" + id_name + "_name" + i + "', '" + html_element_name + "_name_input_focused')\" onblur=\"javascript:textbox_leave('" + id_name + "_name" + i + "', '" + html_element_name + "_name_input')\" />");
                    Output.WriteLine("<br />");
                    Output.WriteLine("              <span class=\"metadata_sublabel2\">" + Translator.Get_Translation("Location(s)", CurrentLanguage) + ":</span>");
                    ReadOnlyCollection<Origin_Info_Place> places = manufacturers[i - 1].Places;
                    if ((places.Count > 0) && (places[0].Place_Text.Length > 0))
                    {
                        Output.WriteLine("              <input name=\"" + id_name + "_firstloc" + i + "\" id=\"" + id_name + "_firstloc" + i + "\" class=\"" + html_element_name + "_location_input\" type=\"text\" value=\"" + HttpUtility.HtmlEncode(places[0].Place_Text) + "\" onfocus=\"javascript:textbox_enter('" + id_name + "_firstloc" + i + "', '" + html_element_name + "_location_input_focused')\" onblur=\"javascript:textbox_leave('" + id_name + "_firstloc" + i + "', '" + html_element_name + "_location_input')\" />");
                    }
                    else
                    {
                        Output.WriteLine("              <input name=\"" + id_name + "_firstloc" + i + "\" id=\"" + id_name + "_firstloc" + i + "\" class=\"" + html_element_name + "_location_input\" type=\"text\" value=\"\" onfocus=\"javascript:textbox_enter('" + id_name + "_firstloc" + i + "', '" + html_element_name + "_location_input_focused')\" onblur=\"javascript:textbox_leave('" + id_name + "_firstloc" + i + "', '" + html_element_name + "_location_input')\" />");
                    }
                    if ((places.Count > 1) && (places[1].Place_Text.Length > 0))
                    {
                        Output.WriteLine("              <input name=\"" + id_name + "_secondloc" + i + "\" id=\"" + id_name + "_secondloc" + i + "\" class=\"" + html_element_name + "_location_input\" type=\"text\" value=\"" + HttpUtility.HtmlEncode(places[1].Place_Text) + "\" onfocus=\"javascript:textbox_enter('" + id_name + "_secondloc" + i + "', '" + html_element_name + "_location_input_focused')\" onblur=\"javascript:textbox_leave('" + id_name + "_secondloc" + i + "', '" + html_element_name + "_location_input')\" />");
                    }
                    else
                    {
                        Output.WriteLine("              <input name=\"" + id_name + "_secondloc" + i + "\" id=\"" + id_name + "_secondloc" + i + "\" class=\"" + html_element_name + "_location_input\" type=\"text\" value=\"\" onfocus=\"javascript:textbox_enter('" + id_name + "_secondloc" + i + "', '" + html_element_name + "_location_input_focused')\" onblur=\"javascript:textbox_leave('" + id_name + "_secondloc" + i + "', '" + html_element_name + "_location_input')\" />");
                    }
                    if ((places.Count > 2) && (places[2].Place_Text.Length > 0))
                    {
                        Output.Write("              <input name=\"" + id_name + "_thirdloc" + i + "\" id=\"" + id_name + "_thirdloc" + i + "\" class=\"" + html_element_name + "_location_input\" type=\"text\" value=\"" + HttpUtility.HtmlEncode(places[2].Place_Text) + "\" onfocus=\"javascript:textbox_enter('" + id_name + "_thirdloc" + i + "', '" + html_element_name + "_location_input_focused')\" onblur=\"javascript:textbox_leave('" + id_name + "_thirdloc" + i + "', '" + html_element_name + "_location_input')\" />");
                    }
                    else
                    {
                        Output.Write("              <input name=\"" + id_name + "_thirdloc" + i + "\" id=\"" + id_name + "_thirdloc" + i + "\" class=\"" + html_element_name + "_location_input\" type=\"text\" value=\"\" onfocus=\"javascript:textbox_enter('" + id_name + "_thirdloc" + i + "', '" + html_element_name + "_location_input_focused')\" onblur=\"javascript:textbox_leave('" + id_name + "_thirdloc" + i + "', '" + html_element_name + "_location_input')\" />");
                    }

                    if (i < manufacturers.Count)
                    {
                        Output.WriteLine("<br />");
                    }
                    else
                    {
                        Output.WriteLine("</div>");
                        Output.WriteLine("          </td>");
                    }
                }
            }

            Output.WriteLine("          <td valign=\"bottom\" >");
            if (Repeatable)
            {
                Output.WriteLine("            <a title=\"" + Translator.Get_Translation("Click to add a new manufacturer", CurrentLanguage) + ".\" href=\"" + Base_URL + "l/technical/javascriptrequired\" onmousedown=\"return add_publisher_element('" + html_element_name + "');\"><img border=\"0px\" class=\"repeat_button\" src=\"" + Base_URL + REPEAT_BUTTON_URL + "\" /></a>");
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
        /// <remarks> This clears any preexisting manufacturers </remarks>
        public override void Prepare_For_Save(SobekCM_Item Bib, User_Object Current_User)
        {
            Bib.Bib_Info.Clear_Manufacturers();
        }

        /// <summary> Saves the data rendered by this element to the provided bibliographic object during postback </summary>
        /// <param name="Bib"> Object into which to save the user's data, entered into the html rendered by this element </param>
        public override void Save_To_Bib(SobekCM_Item Bib)
        {
            const string HTML_ELEMENT_NAME = "complex_manufacturer";
            string id = HTML_ELEMENT_NAME.Replace("_", "");
            string[] getKeys = HttpContext.Current.Request.Form.AllKeys;
            foreach (string thisKey in getKeys)
            {
                if ((thisKey.IndexOf(id) == 0) && (thisKey.IndexOf("loc") < 0))
                {
                    string name = HttpContext.Current.Request.Form[thisKey];
                    if (name.Length > 0)
                    {
                        string key = thisKey.Replace(id + "_name", "");
                        string loc1_key = id + "_firstloc" + key;
                        string loc2_key = id + "_secondloc" + key;
                        string loc3_key = id + "_thirdloc" + key;

                        string loc1 = HttpContext.Current.Request.Form[loc1_key].Trim();
                        string loc2 = HttpContext.Current.Request.Form[loc2_key].Trim();
                        string loc3 = HttpContext.Current.Request.Form[loc3_key].Trim();

                        Publisher_Info publisher = Bib.Bib_Info.Add_Manufacturer(name);
                        if (loc1.Length > 0)
                            publisher.Add_Place(loc1);
                        if (loc2.Length > 0)
                            publisher.Add_Place(loc2);
                        if (loc3.Length > 0)
                            publisher.Add_Place(loc3);

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
