﻿#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI.WebControls;
using SobekCM.Library.Aggregations;
using SobekCM.Library.Application_State;
using SobekCM.Library.HTML;
using SobekCM.Library.MainWriters;
using SobekCM.Library.Results;
using SobekCM.Library.Users;

#endregion

namespace SobekCM.Library.AggregationViewer.Viewers
{
    /// <summary> Renders the item list matching a browse or search against an item aggregation </summary>
    /// <remarks> This class implements the <see cref="iAggregationViewer"/> interface and extends the <see cref="abstractAggregationViewer"/> class.<br /><br />
    /// Aggregation viewers are used when displaying aggregation home pages, searches, browses, and information pages.<br /><br />
    /// During a valid html request to display items matching a browse or search, the following steps occur:
    /// <ul>
    /// <li>Application state is built/verified by the <see cref="Application_State.Application_State_Builder"/> </li>
    /// <li>Request is analyzed by the <see cref="Navigation.SobekCM_QueryString_Analyzer"/> and output as a <see cref="Navigation.SobekCM_Navigation_Object"/> </li>
    /// <li>Main writer is created for rendering the output, in this case the <see cref="Html_MainWriter"/> </li>
    /// <li>The HTML writer will create the necessary subwriter.  For a collection-level request, an instance of the  <see cref="Aggregation_HtmlSubwriter"/> class is created. </li>
    /// <li>To display the requested collection view, the collection subwriter will creates an instance of this class </li>
    /// <li>To display the actual results, this class will create an instance of the <see cref="PagedResults_HtmlSubwriter"/> class</li>
    /// <li>That subwriter creates its own results viewer which extends the <see cref="ResultsViewer.abstract_ResultsViewer"/> class </li>
    /// </ul></remarks>
    public class DataSet_Browse_Info_AggregationViewer : abstractAggregationViewer
    {
        private readonly Item_Aggregation_Browse_Info browseObject;
        private readonly Aggregation_Code_Manager codeManager;
        private readonly Item_Lookup_Object itemList;
        private readonly List<iSearch_Title_Result> pagedResults;
        private readonly Search_Results_Statistics resultsStatistics;

        private PagedResults_HtmlSubwriter writeResult;


        /// <summary> Constructor for a new instance of the DataSet_Browse_Info_AggregationViewer class </summary>
        /// <param name="Browse_Object"> Browse or information object to be displayed </param>
        /// <param name="Results_Statistics"> Information about the entire set of results for a search or browse </param>
        /// <param name="Paged_Results"> Single page of results for a search or browse, within the entire set </param>
        /// <param name="Code_Manager"> Code manager object maintains mapping between SobekCM codes and greenstone codes (used by result_dataset_html_subwriter)</param>
        /// <param name="Item_List"> Object for pulling additional information about each item during display </param>
        /// <param name="Current_User"> Currently logged on user, or NULL </param>
        public DataSet_Browse_Info_AggregationViewer(Item_Aggregation_Browse_Info Browse_Object,
            Search_Results_Statistics Results_Statistics,
            List<iSearch_Title_Result> Paged_Results,
            Aggregation_Code_Manager Code_Manager,
            Item_Lookup_Object Item_List,
            User_Object Current_User):base(null, null )
        {
            browseObject = Browse_Object;
            codeManager = Code_Manager;
            itemList = Item_List;
            currentUser = Current_User;
            resultsStatistics = Results_Statistics;
            pagedResults = Paged_Results;
        }

        /// <summary> Gets the type of collection view or search supported by this collection viewer </summary>
        /// <value> This returns the <see cref="Item_Aggregation.CollectionViewsAndSearchesEnum.DataSet_Browse"/> enumerational value </value>
        public override Item_Aggregation.CollectionViewsAndSearchesEnum Type
        {
            get { return Item_Aggregation.CollectionViewsAndSearchesEnum.DataSet_Browse; }
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

        /// <summary> Flag which indicates whether to always use the home text as the secondary text </summary>
        /// <remarks> This property always returns the value FALSE</remarks>
        public override bool Always_Display_Home_Text
        {
            get
            {
                return false;
            }
        }

        /// <summary> Gets flag which indicates whether the secondary text requires controls </summary>
        /// <value> This property always returns the value TRUE</value>
        public override bool Secondary_Text_Requires_Controls
        {
            get
            {
                return true;
            }
        }

        /// <summary> Add the HTML to be displayed in the search box </summary>
        /// <param name="Output"> Textwriter to write the HTML for this viewer</param>
        /// <param name="Tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
        public override void Add_Search_Box_HTML(TextWriter Output, Custom_Tracer Tracer)
        {
            if (Tracer != null)
            {
                Tracer.Add_Trace("DataSet_Browse_Info_AggregationViewer.Write_HTML", "Writing HTML from result_dataset_html_subwriter ");
            }

            writeResult.Write_HTML(Output, Tracer);
        }

        /// <summary> Add controls to the placeholder below the search box </summary>
        /// <param name="placeHolder"> Placeholder into which to place controls to be rendered</param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
        /// <remarks> This adds the results from the dataset into the space below the search box.<br /><br />
        /// This creates and uses a <see cref="PagedResults_HtmlSubwriter"/> to write the results. </remarks>
        public override void Add_Secondary_Controls(PlaceHolder placeHolder, Custom_Tracer Tracer)
        {
            if (Tracer != null)
            {
                Tracer.Add_Trace("DataSet_Browse_Info_AggregationViewer.Add_Secondary_Controls", "Adding HTML");
            }

            writeResult = new PagedResults_HtmlSubwriter(resultsStatistics, pagedResults, codeManager, translator, itemList, currentUser, currentMode, Tracer)
                              {
                                  Skin = htmlSkin,
                                  Mode = currentMode,
                                  Hierarchy_Object = currentCollection,
                                  Browse_Title = browseObject.Get_Label(currentMode.Language)
                              };
            writeResult.Add_Controls(placeHolder, Tracer);


            if ( resultsStatistics.Total_Items > 0)
            {
                Literal literal = new Literal
                                      {
                                          Text ="<div class=\"SobekResultsNavBar\">" + Environment.NewLine + "  " + writeResult.Buttons +"" + Environment.NewLine + "  " + writeResult.Showing_Text  + Environment.NewLine + "</div>" + Environment.NewLine + "<br />" + Environment.NewLine 
                                      };
                placeHolder.Controls.Add(literal);
            }
        }

    }
}
