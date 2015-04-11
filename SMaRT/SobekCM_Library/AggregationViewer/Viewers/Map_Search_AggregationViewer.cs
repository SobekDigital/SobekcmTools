﻿#region Using directives

using System;
using System.IO;
using System.Text;
using SobekCM.Library.Aggregations;
using SobekCM.Library.Application_State;
using SobekCM.Library.Configuration;
using SobekCM.Library.HTML;
using SobekCM.Library.MainWriters;
using SobekCM.Library.Navigation;

#endregion

namespace SobekCM.Library.AggregationViewer.Viewers
{
    /// <summary> Renders the google map search page for a given item aggregation </summary>
    /// <remarks> This class implements the <see cref="iAggregationViewer"/> interface and extends the <see cref="abstractAggregationViewer"/> class.<br /><br />
    /// Collection viewers are used when displaying collection home pages, searches, browses, and information pages.<br /><br />
    /// During a valid html request to display the google map search page, the following steps occur:
    /// <ul>
    /// <li>Application state is built/verified by the <see cref="Application_State.Application_State_Builder"/> </li>
    /// <li>Request is analyzed by the <see cref="Navigation.SobekCM_QueryString_Analyzer"/> and output as a <see cref="SobekCM_Navigation_Object"/> </li>
    /// <li>Main writer is created for rendering the output, in this case the <see cref="Html_MainWriter"/> </li>
    /// <li>The HTML writer will create the necessary subwriter.  For a collection-level request, an instance of the  <see cref="Aggregation_HtmlSubwriter"/> class is created. </li>
    /// <li>To display the requested collection view, the collection subwriter will creates an instance of this class </li>
    /// </ul></remarks>
    public class Map_Search_AggregationViewer : abstractAggregationViewer
    {
        private readonly int mapHeight;
        private readonly string text1 = String.Empty;
        private readonly string text2 = String.Empty;
        private readonly string text3 = String.Empty;
        private readonly string text4 = String.Empty;

        /// <summary> Constructor for a new instance of the Map_Search_AggregationViewer class </summary>
        /// <param name="Current_Aggregation"> Current item aggregation object </param>
        /// <param name="Current_Mode"> Mode / navigation information for the current request</param>
        public Map_Search_AggregationViewer(Item_Aggregation Current_Aggregation, SobekCM_Navigation_Object Current_Mode): base(Current_Aggregation, Current_Mode)
        {
            // Compute the redirect stem to use
            string fields = currentMode.Search_Fields;
            string search_string = currentMode.Search_String;
            currentMode.Search_String = String.Empty;
            currentMode.Search_Fields = String.Empty;
            currentMode.Mode = Display_Mode_Enum.Results;
            currentMode.Search_Type = Search_Type_Enum.Map;
            currentMode.Search_Precision = Search_Precision_Type_Enum.Inflectional_Form;
            string redirect_stem = currentMode.Redirect_URL();
            currentMode.Mode = Display_Mode_Enum.Search;
            currentMode.Search_String = search_string;
            currentMode.Search_Fields = fields;
            // Now, populate the search terms, if there was one or some
            text1 = String.Empty;
            text2 = String.Empty;
            text3 = String.Empty;
            text4 = String.Empty;
            if (currentMode.Search_String.Length > 0)
            {
                string[] splitter = currentMode.Search_String.Split(",".ToCharArray());
                bool isNumber = true;
                foreach (char thisChar in splitter[0])
                {
                    if ((!Char.IsDigit(thisChar)) && ( thisChar != '.' ) && (thisChar != '-' ))
                        isNumber = false;
                }
                if (isNumber)
                {
                    text1 = splitter[0].Replace(" =", " or ");


                    if (splitter.Length > 1)
                    {
                        foreach (char thisChar in splitter[1])
                        {
                            if ((!Char.IsDigit(thisChar)) && (thisChar != '.') && (thisChar != '-'))
                                isNumber = false;
                        }
                        if (isNumber)
                        {
                            text2 = splitter[1].Replace(" =", " or ");

                            if (splitter.Length > 2)
                            {
                                foreach (char thisChar in splitter[2])
                                {
                                    if ((!Char.IsDigit(thisChar)) && (thisChar != '.') && (thisChar != '-'))
                                        isNumber = false;
                                }

                                if (isNumber)
                                {
                                    text3 = splitter[2].Replace(" =", " or ");

                                    foreach (char thisChar in splitter[3])
                                    {
                                        if ((!Char.IsDigit(thisChar)) && (thisChar != '.') && (thisChar != '-'))
                                            isNumber = false;
                                    }

                                    if (isNumber)
                                    {
                                        text4 = splitter[3].Replace(" =", " or ");
                                    }
                                }
                            }

                        }
                    }
                }
            }

            // Add the google script information
            mapHeight = 500;
            StringBuilder scriptBuilder = new StringBuilder();
            
            scriptBuilder.AppendLine("<script type=\"text/javascript\" src=\"http://maps.google.com/maps/api/js?v=3.2&sensor=false\"></script>");
            scriptBuilder.AppendLine("<script type=\"text/javascript\" src=\"" + currentMode.Base_URL + "default/scripts/sobekcm_map_search.js\"></script>");
            scriptBuilder.AppendLine("<script type=\"text/javascript\" src=\"" + currentMode.Base_URL + "default/scripts/sobekcm_map_tool.js\"></script>");
            
            scriptBuilder.AppendLine("<script type=\"text/javascript\">");
            scriptBuilder.AppendLine("  //<![CDATA[");
            scriptBuilder.AppendLine("  function load() { ");

            switch (Current_Aggregation.Map_Search % 100)
            {
                case 0:  // WORLD
                    scriptBuilder.AppendLine("    load_search_map(0, 0, 1, \"map1\");");
                    break;

                case 1:  // FLORIDA
                    scriptBuilder.AppendLine("    load_search_map(28, -84.5, 6, \"map1\");");
                    break;

                case 2:  // UNITED STATES
                    scriptBuilder.AppendLine("    load_search_map(48, -95, 3, \"map1\");");
                    break;

                case 3:  // NORTH AMERICA
                    scriptBuilder.AppendLine("    load_search_map(48, -95, 3, \"map1\");");
                    mapHeight = 600;
                    break;

                case 4:  // CARIBBEAN
                    scriptBuilder.AppendLine("    load_search_map(19, -74, 4, \"map1\");");
                    break;

                case 5:  // SOUTH AMERICA
                    scriptBuilder.AppendLine("    load_search_map(-22, -60, 3, \"map1\");");
                    mapHeight = 600;
                    break;

                case 6:   // AFRICA
                    scriptBuilder.AppendLine("    load_search_map(6, 19.5, 3, \"map1\");");
                    mapHeight = 600;
                    break;

                case 7:   // EUROPE
                    scriptBuilder.AppendLine("    load_search_map(49.5, 13.35, 4, \"map1\");");
                    break;

                case 8:   // ASIA
                    scriptBuilder.AppendLine("    load_search_map(36, 96, 3, \"map1\");");
                    break;

                case 9:   // MIDDLE EAST
                    scriptBuilder.AppendLine("    load_search_map(31, 39, 4, \"map1\");");
                    break;
            }

            // If no point searching is allowed, disable it
            if (currentCollection.Map_Search >= 100)
            {
                scriptBuilder.AppendLine("    disable_point_searching();");
            }

            if ((text1.Length > 0) && (text2.Length > 0) && (text3.Length > 0) && (text4.Length > 0))
            {
                scriptBuilder.AppendLine("    add_selection_rectangle( " + text1 + ", " + text2 + ", " + text3 + ", " + text4 + " );");
                scriptBuilder.AppendLine("    zoom_to_bounds();");
            }
            else if ((text1.Length > 0) && (text2.Length > 0))
            {
                scriptBuilder.AppendLine("    add_selection_point( " + text1 + ", " + text2 + ", 8 );");
            }

            scriptBuilder.AppendLine("  }");
            scriptBuilder.AppendLine("  //]]>");
            scriptBuilder.AppendLine("</script>");
            scriptIncludeName = scriptBuilder.ToString();

            // Get the action name for the button
            scriptActionName = "map_search_sobekcm('" + redirect_stem + "');";
        }

        /// <summary> Gets the type of collection view or search supported by this collection viewer </summary>
        /// <value> This returns the <see cref="Item_Aggregation.CollectionViewsAndSearchesEnum.Map_Search"/> enumerational value </value>
        public override Item_Aggregation.CollectionViewsAndSearchesEnum Type
        {
            get { return Item_Aggregation.CollectionViewsAndSearchesEnum.Map_Search; }
        }

        /// <summary>Flag indicates whether the subaggregation selection panel is displayed for this collection viewer</summary>
        /// <value> This property always returns the <see cref="Selection_Panel_Display_Enum.Never"/> enumerational value </value>
        public override Selection_Panel_Display_Enum Selection_Panel_Display
        {
            get
            {
                return Selection_Panel_Display_Enum.Never;
            }
        }

        /// <summary> Gets flag which indicates whether to always use the home text as the secondary text </summary>
        /// <value> This property always returns the value FALSE </value>
        public override bool Always_Display_Home_Text
        {
            get
            {
                return false;
            }
        }

        /// <summary> Add the HTML to be displayed in the search box </summary>
        /// <param name="Output"> Textwriter to write the HTML for this viewer</param>
        /// <param name="Tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
        /// <remarks> This addes the map search panel which holds the google map, as well as the coordinate entry boxes </remarks>
        public override void Add_Search_Box_HTML(TextWriter Output, Custom_Tracer Tracer)
        {
            if (Tracer != null)
            {
                Tracer.Add_Trace("Map_Search_AggregationViewer.Add_Search_Box_HTML", "Adding html for search box");
            }

            string search_button_text = "Search";
            string find_button_text = "Find Address";
            string address_text = "Address";
            const string locateText = "Locate";

            if (currentMode.Language == Web_Language_Enum.Spanish)
            {
                search_button_text = "Buscar";
                find_button_text = "Localizar";
                address_text = "Dirección";
            }


            bool show_coordinates = false;
            int width = 740;
            if (currentMode.Info_Browse_Mode == "1")
            {
                show_coordinates = true;
                width = 550;
            }

            Output.WriteLine("  <table width=\"100%\" id=\"MapSearchPanel\" >");
            Output.WriteLine("  <tr>");
            Output.WriteLine("    <td colspan=\"2\">");
            switch( currentMode.Language )
            {

                case Web_Language_Enum.Spanish:
                    if (currentCollection.Map_Search >= 100)
                    {
                        Output.WriteLine("          <table cellspacing=\"2px\">");
                        Output.WriteLine("            <tr><td><span style=\"line-height:160%\"> &nbsp; &nbsp; 1. Use the <i>Select Area</i> button and click to select opposite corners to draw a search box on the map &nbsp; &nbsp; <br /> &nbsp; &nbsp; 2. Press the <i>Search</i> button to see results &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; ( <a href=\"#FAQ\">more help</a> )</span> </td>");
                        Output.WriteLine("                <td><input type=\"button\" name=\"searchButton\" value=\"" + search_button_text + "\" id=\"searchButton\" class=\"SobekSearchButton\" onclick=\"" + scriptActionName + "\" /></td></tr>");
                        Output.WriteLine("          </table>");
                    }
                    else
                    {
                        Output.WriteLine("        <div class=\"map_instructions_link\" id=\"MapInstructionsLink\" style=\"display:block\" >");
                        Output.WriteLine("          <a href=\"\" onclick=\"return show_map_instructions();\">Haga click aquí para ver las instrucciones para la interfase de esta búsqueda.</a>");
                        Output.WriteLine("        </div>");
                        Output.WriteLine("        <div class=\"map_instructions\" id=\"MapInstructions\" style=\"display:none\" >");
                        Output.WriteLine("          <table cellspacing=\"3px\">");
                        Output.WriteLine("            <tr><td colspan=\"2\">1. Utilice uno de los siguientes métodos para definir su búsqueda geográfica:</td></tr>");
                        Output.WriteLine("            <tr><td width=\"50px\">&nbsp;</td><td>a. Escriba una dirección y haga click en el botón <i>Localizar</a> para localizarla, <i>o</i></td></tr>");
                        Output.WriteLine("            <tr><td>&nbsp;</td><td>b. Haga click sobre el botón <i>Presione para Selecionar Area</i> para seleccionar dos esquinas opuestas, <i>o</i></td></tr>");
                        Output.WriteLine("            <tr><td>&nbsp;</td><td>c. Haga click sobre el botón <i>Presione para Seleccionar un Punto</i> para seleccionar un punto individual</td></tr>");
                        Output.WriteLine("            <tr><td colspan=\"2\">2. Presione el botón <i>Buscar</i> para ver los resultados. &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; ( <a href=\"#FAQ\">more help</a> )</td></tr>");
                        Output.WriteLine("          </table>");
                        Output.WriteLine("        </div>");
                    }
                    break;

                default:
                    if (currentCollection.Map_Search >= 100)
                    {
                        Output.WriteLine("          <table cellspacing=\"2px\">");
                        Output.WriteLine("            <tr><td><span style=\"line-height:160%\"> &nbsp; &nbsp; 1. Use the <i>Select Area</i> button and click to select opposite corners to draw a search box on the map &nbsp; &nbsp; <br /> &nbsp; &nbsp; 2. Press the <i>Search</i> button to see results &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; ( <a href=\"#FAQ\">more help</a> )</span> </td>");
                        Output.WriteLine("                <td><input type=\"button\" name=\"searchButton\" value=\"" + search_button_text + "\" id=\"searchButton\" class=\"SobekSearchButton\" onclick=\"" + scriptActionName + "\" /></td></tr>");
                        Output.WriteLine("          </table>");
                    }
                    else
                    {
                        Output.WriteLine("        <div class=\"map_instructions_link\" id=\"MapInstructionsLink\" style=\"display:block\" >");
                        Output.WriteLine("          <a href=\"\" onclick=\"return show_map_instructions();\">Click here to view instructions for this search interface</a>");
                        Output.WriteLine("        </div>");
                        Output.WriteLine("        <div class=\"map_instructions\" id=\"MapInstructions\" style=\"display:none\" >");
                        Output.WriteLine("          <table cellspacing=\"3px\">");
                        Output.WriteLine("            <tr><td colspan=\"2\">1. Use one of the methods below to define your geographic search:</td></tr>");
                        Output.WriteLine("            <tr><td width=\"50px\">&nbsp;</td><td>a. Enter an address and press <i>Find Address</i> to locate, <i>or</i></td></tr>");
                        Output.WriteLine("            <tr><td>&nbsp;</td><td>b. Press the <i>Select Area</i> button and click to select two opposite corners, <i>or</i></td></tr>");
                        Output.WriteLine("            <tr><td>&nbsp;</td><td>c. Press the <i>Select Point</i> button and click to select a single point</td></tr>");
                        Output.WriteLine("            <tr><td colspan=\"2\">2. Press the <i>Search</i> button to see results &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; ( <a href=\"#FAQ\">more help</a> )</td></tr>");
                        Output.WriteLine("          </table>");
                        Output.WriteLine("        </div>");
                    }
                    break;
            }


            //Output.WriteLine("&nbsp; &nbsp; &nbsp; 1) Use one of the methods below to define your geographic search:<br />");
            //Output.WriteLine("&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; a) Enter an address and press <i>Find Address</i><br />");
            //Output.WriteLine("&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; b) Press the <i>Select Area</i> button and click to select two opposite corners<br />");
            //Output.WriteLine("&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; c) Press the <i>Select Point</i> button and click to select a single point<br />");
            //Output.WriteLine("&nbsp; &nbsp; &nbsp; 2) Press the <i>Search</i> button to see results<br />");

            if (currentCollection.Map_Search < 100)
            {
                Output.WriteLine("        <div class=\"map_address_div\">");
                Output.WriteLine("          <label for=\"AddressTextBox\">" + address_text + ":</label> &nbsp; ");
                Output.Write("          <input name=\"AddressTextBox\" type=\"text\" id=\"AddressTextBox\" class=\"MapAddressBox_initial\" value=\"Enter address ( i.e., 12 Main Street, Gainesville Florida )\" onfocus=\"enter_address_box(this);\" onblur=\"leave_address_box(this);\" ");
                Output.Write(currentMode.Browser_Type.IndexOf("IE") >= 0 ? " onkeydown=\"address_keydown(event, this);\" " : " onkeydown=\"return address_keydown(event, this);\" ");
                Output.WriteLine(" onchange=\"address_box_changed(this);\" /> &nbsp; ");
                Output.WriteLine("          <input type=\"button\" name=\"findButton\" value=\"" + find_button_text + "\" id=\"findButton\" class=\"SobekSearchButton\" onclick=\"map_address_geocode();\" /> &nbsp; ");
                Output.WriteLine("          <input type=\"button\" name=\"searchButton\" value=\"" + search_button_text + "\" id=\"searchButton\" class=\"SobekSearchButton\" onclick=\"" + scriptActionName + "\" />");
                Output.WriteLine("        </div>");
            }
            Output.WriteLine("    </td>");
            Output.WriteLine("  </tr>");

            Output.WriteLine("  <tr valign=\"top\">");
            Output.WriteLine("    <td>");
            if (!show_coordinates)
            {
                Output.WriteLine("      <div id=\"coordinate_tab_button\" class=\"ShowCoordinateTab\">");
                currentMode.Info_Browse_Mode = "1";
                Output.WriteLine("        <a href=\"" + currentMode.Redirect_URL() + "\" ><img src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/tabs/cLG.gif\" border=\"0\" class=\"tab_image\" alt=\"\" /><span class=\"tab\">SHOW COORDINATES</span><img src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/tabs/cRG.gif\" border=\"0\" class=\"tab_image\" alt=\"\" /></a>");
                currentMode.Info_Browse_Mode = "0";
                Output.WriteLine("      </div>");
            }
            else
            {
                Output.WriteLine("      <div id=\"coordinate_tab_button\" class=\"HideCoordinateTab\">");
                currentMode.Info_Browse_Mode = "0";
                Output.WriteLine("        <a href=\"" + currentMode.Redirect_URL() + "\" ><img src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/tabs/cLG.gif\" border=\"0\" class=\"tab_image\" alt=\"\" /><span class=\"tab\">HIDE COORDINATES</span><img src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/tabs/cRG.gif\" border=\"0\" class=\"tab_image\" alt=\"\" /></a>");
                currentMode.Info_Browse_Mode = "1";
                Output.WriteLine("      </div>");
            }
            Output.WriteLine("      <div id=\"map1\" style=\"width: " + width + "px; height: " + mapHeight + "px\"></div>");
            Output.WriteLine("    </td>");
            Output.WriteLine("    <td>");
            Output.WriteLine(show_coordinates ? "      <div id=\"map_coordinates_div\" >" : "      <div id=\"map_coordinates_div\" style=\"display: none;\" >");
            Output.WriteLine("        <table>");
            Output.WriteLine("          <tr><td colspan=\"2\"><br /><br /><br /><b>Search Coordinates</b><br /><br /></td></tr>");
            Output.WriteLine("          <tr><td colspan=\"2\">Point 1</td></tr>");
            Output.WriteLine("          <tr><td><label for=\"Textbox1\">Latitude:</label> </td><td><input name=\"Textbox1\" type=\"text\" id=\"Textbox1\" class=\"MapSearchBox\" value=\"" + text1 + "\" onfocus=\"javascript:textbox_enter('Textbox1', 'MapSearchBox_focused')\" onblur=\"javascript:textbox_leave('Textbox1', 'MapSearchBox')\"/></td></tr>");
            Output.WriteLine("          <tr><td><label for=\"Textbox2\">Longitude:</label> </td><td><input name=\"Textbox2\" type=\"text\" id=\"Textbox2\" class=\"MapSearchBox\" value=\"" + text2 + "\" onfocus=\"javascript:textbox_enter('Textbox2', 'MapSearchBox_focused')\" onblur=\"javascript:textbox_leave('Textbox2', 'MapSearchBox')\"/><br /><br /></td></tr>");
            Output.WriteLine("          <tr><td colspan=\"2\"><br />Point 2</td></tr>");
            Output.WriteLine("          <tr><td><label for=\"Textbox3\">Latitude:</label> </td><td><input name=\"Textbox3\" type=\"text\" id=\"Textbox3\" class=\"MapSearchBox\" value=\"" + text3 + "\" onfocus=\"javascript:textbox_enter('Textbox3', 'MapSearchBox_focused')\" onblur=\"javascript:textbox_leave('Textbox3', 'MapSearchBox')\"/></td></tr>");
            Output.WriteLine("          <tr><td><label for=\"Textbox4\">Longitude:</label> </td><td><input name=\"Textbox4\" type=\"text\" id=\"Textbox4\" class=\"MapSearchBox\" value=\"" + text4 + "\" onfocus=\"javascript:textbox_enter('Textbox4', 'MapSearchBox_focused')\" onblur=\"javascript:textbox_leave('Textbox4', 'MapSearchBox')\"/></td></tr>");
            Output.WriteLine("          <tr><td colspan=\"2\" align=\"right\"><br /></td></tr>");
            Output.WriteLine("          <tr><td colspan=\"2\" align=\"right\"><input type=\"button\" name=\"locateButton\" value=\"" + locateText + "\" id=\"locateButton\" class=\"SobekSearchButton\" onclick=\"locate_by_coordinates();\" /></td></tr>");
            Output.WriteLine("         </table>");
            Output.WriteLine("       </div>");
            Output.WriteLine("      </td>");
            Output.WriteLine("    </tr>");
            Output.WriteLine("  </table>");
            Output.WriteLine();

        }

        /// <summary> Add the HTML to be displayed below the search box </summary>
        /// <param name="Output"> Textwriter to write the HTML for this viewer</param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
        /// <remarks> This adds the search tips by calling the base method <see cref="abstractAggregationViewer.Add_Simple_Search_Tips"/> </remarks>
        public override void Add_Secondary_HTML(TextWriter Output, Custom_Tracer Tracer)
        {
            if (Tracer != null)
            {
                Tracer.Add_Trace("Map_Search_AggregationViewer.Add_Secondary_HTML", "Adds map search-specific search tips");
            }     

            // Write the quick tips
            Output.WriteLine("<a name=\"FAQ\" ></a>");
            Output.WriteLine("<div id=\"SobekQuickTips\">");

            // See if the FAQ is present for this collection
            string language_code = currentMode.Language_Code;
            if (language_code.Length > 0)
                language_code = "_" + language_code;
            string directory = SobekCM_Library_Settings.Base_Design_Location + "\\aggregations\\" + currentMode.Aggregation + "\\extra";
            string aggregation_specific_faq = String.Empty;
            if ( Directory.Exists( directory ))
            {
                if (File.Exists(directory + "\\map_faq" + language_code + ".txt"))
                {
                    if (Tracer != null)
                    {
                        Tracer.Add_Trace("Map_Search_AggregationViewer.Add_Secondary_HTML", "Reading aggregation specific map search faq");
                    }

                    try
                    {
                        StreamReader faq_reader = new StreamReader(directory + "\\map_faq" + language_code + ".txt");
                        aggregation_specific_faq = faq_reader.ReadToEnd();
                        faq_reader.Close();
                    }
                    catch (Exception)
                    {
                        // No error thrown in this case; the default value will be used
                    }
                }
            }

            // If no aggregation level FAQ was found, look for a collection wide
            if (aggregation_specific_faq.Length == 0)
            {
                directory = SobekCM_Library_Settings.Base_Design_Location + "\\extra\\searchtips";
                if (Directory.Exists(directory))
                {
                    if (File.Exists(directory + "\\map_faq" + language_code + ".txt"))
                    {
                        if (Tracer != null)
                        {
                            Tracer.Add_Trace("Map_Search_AggregationViewer.Add_Secondary_HTML", "Reading application-wide map search faq");
                        }

                        try
                        {
                            StreamReader faq_reader = new StreamReader(directory + "\\map_faq" + language_code + ".txt");
                            aggregation_specific_faq = faq_reader.ReadToEnd();
                            faq_reader.Close();
                        }
                        catch (Exception)
                        {
                            // No error thrown in this case; the default value will be used
                        }
                    }
                }
            }

            // Now, render the faq
            if ( aggregation_specific_faq.Length > 0 )
            {
                Output.WriteLine( aggregation_specific_faq );
            }
            else
            {
                Output.WriteLine("  <h1>Map Search FAQ</h1>");
                Output.WriteLine("  <ul>");
                Output.WriteLine("    <li><strong>How do I search?</strong>");
                if (currentCollection.Map_Search < 100)
                {
                    Output.WriteLine("      <p class=\"tagline\">To perform a search, you first need to define your area or point of interest and then perform the search.  There are several ways to define your area of interest.  You can either enter an address to search or you can draw either a region or point on the map.  Once you have defined your search, click the <i>Search</i> button to discover any matches to that location.</p>");
                    Output.WriteLine("      <p class=\"tagline\">To search for addresses, type in an address and click on the <i>Find Address</i> button.   You may also use major landmark names, although addresses may work better.  Be sure to include the city and state in your search as well.   Once the address is located on the map, click <i>Search</i> to discover aerials which include that location. </p>");
                    Output.WriteLine("      <p class=\"tagline\">To search by map, click either <i>Press to Select Area</i> or <i>Press to Select Point</i> on the map.  When using Select Area, click on a starting point on the map and then click on the opposite corner to create a rectangle.  By clicking on <i>Search</i> above the map, you will retrieve matches for that area.  With Select Point, choose a specific location and click on Search.</p>");
                }
                else
                {
                    Output.WriteLine("      <p class=\"tagline\">To perform a search, you first need to define your area of interest and then perform the search.  To define an area, click <i>Press to Select Area</i> on the map.  Then click on a starting point on the map and then click on the opposite corner to create a rectangle.  By clicking on <i>Search</i> above the map, you will retrieve matches for that area.</p>");

                }
                Output.WriteLine("    </li>");

                Output.WriteLine("    <li><strong>I am having difficulty selecting an area</strong>");
                Output.WriteLine("      <p class=\"tagline\">Selecting a rectangular area to search is simple once you understand the technique.  First, select the <i>Press to Select Area</i> button on the map.  Then, move to the top left corner of the region you wish to search and click and release the left mouse button.  As you move the mouse now you will notice a rectangle is being drawn which represents your region.  When you click and release the mouse again, you define the lower right corner of the region to search.  Do not press the mouse button and drag the mouse, as this will drag the map around, and will not define a region to select.  Once your region is correctly identified, press the <i>Search</i> button to view matching results.</p>");
                Output.WriteLine("    </li>");

                if (currentCollection.Map_Search < 100)
                {
                    Output.WriteLine("    <li><strong>I am having difficulty searching by address</strong>");
                    Output.WriteLine("      <p class=\"tagline\">Be sure to enter the complete address, including state and country.  You can also try to use the name of a major landmark, but using an address often works better.  Once you enter the address or major landmark name, press the <i>Find Address</i> button.  Look at the map and verify that the location found on the map matches your desired search.  Then, press the <i>Search</i> button to view matching results.</p>");
                    Output.WriteLine("    </li>");
                }
                else
                {
                    Output.WriteLine("    <li><strong>Why can't I search by address or by a point?</strong>");
                    Output.WriteLine("      <p class=\"tagline\">This collection is composed primarily of points, so area searching is much more effective than point searching.");
                    Output.WriteLine("      </p>");
                    Output.WriteLine("    </li>");

                }



                Output.WriteLine("    <li><strong>What is being searched?</strong>");
                Output.WriteLine("      <p class=\"tagline\">The data searched depends upon the material type.  For newspapers, the place of publication is searched.  For maps, the geographic coverage is searched.  And for photographs, the location the photograph was taken is searched.");
                Output.WriteLine("      </p>");
                Output.WriteLine("    </li>");

                Output.WriteLine("    <li><strong>Why so few hits?</strong>");
                Output.WriteLine("      <p class=\"tagline\"> This interface is in beta testing, as we add more coordinate information to our database to search.  Check back often to see our progress!</p>");
                Output.WriteLine("    </li>");

                Output.WriteLine("  </ul>");
            }

            Output.WriteLine("</div>");
            Output.WriteLine("  <br />");
            Output.WriteLine();
        }
    }
}
