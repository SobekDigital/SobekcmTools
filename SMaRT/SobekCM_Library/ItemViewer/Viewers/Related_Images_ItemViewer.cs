#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using SobekCM.Resource_Object.Divisions;
using SobekCM.Library.Application_State;
using SobekCM.Library.Configuration;

#endregion

namespace SobekCM.Library.ItemViewer.Viewers
{
    /// <summary> Item viewer displays thumbnails of all the page images related to this digital resource. </summary>
	/// <remarks> This class extends the abstract class <see cref="abstractItemViewer"/> and implements the 
	/// <see cref="iItemViewer" /> interface. </remarks>
	public class Related_Images_ItemViewer : abstractItemViewer
	{
       
        private readonly string title;
        private int thumbnailsPerPage;
        private int thumbnailSize;

	
		/// <summary> Constructor for a new instance of the Flash_ItemViewer class </summary>
		/// <param name="Title"> Title to display for these related images (varies slightly by material type)</param>
		public Related_Images_ItemViewer(string Title)
		{
			title = String.IsNullOrEmpty(Title) ? "Related Images" : Title;
		}

		/// <summary> Gets the type of item viewer this object represents </summary>
		/// <value> This property always returns the enumerational value <see cref="ItemViewer_Type_Enum.Related_Images"/>. </value>
		public override ItemViewer_Type_Enum ItemViewer_Type
		{
			get { return ItemViewer_Type_Enum.Related_Images; }
		}

		/// <summary> Flag indicates if this view should be overriden if the item is checked out by another user </summary>
		/// <remarks> This always returns the value TRUE for this viewer </remarks>
		public override bool Override_On_Checked_Out
		{
			get
			{
				return true;
			}
		}

		/// <summary> Width for the main viewer section to adjusted to accomodate this viewer</summary>
		/// <value> This always returns the value -1</value>
		public override int Viewer_Width
		{
			get
			{
				return -1;
			 }
		}

		/// <summary> Gets the number of pages for this viewer </summary>
		///<remarks>Edit: The default number of items per page is 50. If a diferent number is selected by the user, this is fetched from the query string</remarks>
        public override int PageCount
		{
			get
			{
                return ((CurrentItem.Web.Static_PageCount - 1) / thumbnailsPerPage) + 1;
			}
		}

		/// <summary> Gets the url to go to the first page of thumbnails </summary>
		public override string First_Page_URL
		{
			get 
			{
                //Get the querystring, if any, from the current url
                string curr_url = HttpContext.Current.Request.RawUrl;
                string queryString = null;

                //Check if query string variables exist in the url
                int index_queryString = curr_url.IndexOf('?');

                if (index_queryString > 0)
                {
                    queryString = (index_queryString < curr_url.Length - 1) ? curr_url.Substring(index_queryString) : String.Empty;
                }
                      
				return ((PageCount > 1) && (CurrentMode.Page > 1)) ? CurrentMode.Redirect_URL("1thumbs")+queryString : String.Empty;
			}
		}

		/// <summary> Gets the url to go to the preivous page of thumbnails </summary>
		public override string Previous_Page_URL
		{
			get
            {                
				return ((PageCount > 1) && ( CurrentMode.Page > 1 )) ? CurrentMode.Redirect_URL( (CurrentMode.Page - 1).ToString() + "thumbs" ) : String.Empty;
			}
		}

		/// <summary> Gets the url to go to the next page of thumbnails </summary>
		public override string Next_Page_URL
		{
			get
			{
				int temp_page_count = PageCount;
				return  ( temp_page_count > 1 ) && (CurrentMode.Page < temp_page_count) ? CurrentMode.Redirect_URL( (CurrentMode.Page + 1).ToString() + "thumbs" ) :  String.Empty;
			}
		}

		/// <summary> Gets the url to go to the last page of thumbnails </summary>
		public override string Last_Page_URL
		{
			get
			{
				int temp_page_count = PageCount;
				return (temp_page_count > 1) && (CurrentMode.Page < temp_page_count) ? CurrentMode.Redirect_URL(temp_page_count.ToString() + "thumbs") : String.Empty;
			}
		}


		/// <summary> Gets the names to show in the Go To combo box </summary>
		public override string[] Go_To_Names
		{
			get
			{
			    List<string> goToUrls = new List<string>();
                for (int i = 1; i <= PageCount; i++)
                {
                    goToUrls.Add(CurrentMode.Redirect_URL(i + "thumbs"));
                }
			    return goToUrls.ToArray();
			}
		}

  
		/// <summary> Adds the main view section to the page turner </summary>
		/// <param name="placeHolder"> Main place holder ( &quot;mainPlaceHolder&quot; ) in the itemNavForm form into which the the bulk of the item viewer's output is displayed</param>
		/// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
		public override void Add_Main_Viewer_Section(PlaceHolder placeHolder, Custom_Tracer Tracer)
		{
			if (Tracer != null)
			{
				Tracer.Add_Trace("Related_Images_ItemViewer.Add_Main_Viewer_Section", "Adds one literal with all the html");
			}

			int images_per_page = thumbnailsPerPage;
            int size_of_thumbnails = thumbnailSize;


			// Build the value
			StringBuilder builder = new StringBuilder(5000);

			// Save the current viewer code
			string current_view_code = CurrentMode.ViewerCode;
			ushort current_view_page = CurrentMode.Page;

			// Start the citation table
			builder.AppendLine( "\t\t<!-- RELATED IMAGES VIEWER OUTPUT -->" );
			builder.AppendLine("\t\t<td>" );

			// Start the main div for the thumbnails
	
			ushort page = (ushort)(CurrentMode.Page - 1);
			if (page > (CurrentItem.Web.Static_PageCount - 1) / images_per_page)
				page = (ushort)((CurrentItem.Web.Static_PageCount - 1) / images_per_page);

			//Outer div which contains all the thumbnails
			builder.AppendLine("<div align=\"center\" style=\"margin:5px;\"><span align=\"left\" style=\"float:left\">");

			// Step through each page in the item
		    for (int page_index = page * images_per_page; (page_index < (page + 1) * images_per_page) && (page_index < CurrentItem.Web.Static_PageCount); page_index++)
			{
				Page_TreeNode thisPage = CurrentItem.Web.Pages_By_Sequence[page_index];
				
				// Find the jpeg image
				foreach (SobekCM_File_Info thisFile in thisPage.Files.Where(thisFile => thisFile.System_Name.IndexOf(".jpg") > 0))
				{
					// Get the image URL
					CurrentMode.Page = (ushort)(page_index + 1);
					CurrentMode.ViewerCode = (page_index + 1).ToString();
					
                    //set the image url to fetch the small thumbnail .thm image
                    string image_url = (CurrentItem.Web.Source_URL + "/" + thisFile.System_Name.Replace(".jpg", "thm.jpg")).Replace("\\", "/").Replace("//", "/").Replace("http:/", "http://");
					
                    //If thumbnail size selected is large, get the full-size jpg image
                    if(size_of_thumbnails==2 || size_of_thumbnails==3 || size_of_thumbnails==4)
						image_url = (CurrentItem.Web.Source_URL + "/" + thisFile.System_Name).Replace("\\", "/").Replace("//", "/").Replace("http:/", "http://");
					string url = CurrentMode.Redirect_URL().Replace("&", "&amp;").Replace("\"", "&quot;");

          
						//builder.Append("<span  id=\"span"+thisPage.Label+"\" align=\"left\" style=\"display:inline-block;\" onmouseover=\"this.className='thumbnailHighlight'\" onmouseout=\"this.className='thumbnailNormal'\" onmousedown=\"window.location.href='" + url + "';\">");
                    builder.Append("<span  id=\"span" + (page_index + 1) + "\" align=\"left\" style=\"display:inline-block;\" onmouseover=\"this.className='thumbnailHighlight'\" onmouseout=\"this.className='thumbnailNormal'\" onmousedown=\"window.location.href='" + url + "';\">");   
			      
                    if(size_of_thumbnails==2)
                       builder.AppendLine("<span style=\"display:inline-block;\" align=\"center\" id=\"parent" + image_url + "\"><table style=\"display:inline-block;\"><tr><td><a id=\"" + thisPage.Label + "\" href=\"" + url + "\"><img id=\"child" + image_url + "\"  src=\"" + image_url + "\" width=\"315px\" height=\"50%\" alt=\"MISSING THUMBNAIL\" class=\"itemThumbnails\" /></a></td></tr><tr><td><span align=\"center\"><span class=\"SobekThumbnailText\" style=\"display:inline-block;\">" + thisPage.Label + "</span></span></td></tr></table></span></span>");
                    else if(size_of_thumbnails==3)
                        builder.AppendLine("<span style=\"display:inline-block;\" align=\"center\" id=\"parent" + image_url + "\"><table style=\"display:inline-block;\"><tr><td><a id=\"" + thisPage.Label + "\" href=\"" + url + "\"><img id=\"child" + image_url + "\" src=\"" + image_url + "\" width=\"472.5px\" height=\"75%\" alt=\"MISSING THUMBNAIL\" class=\"itemThumbnails\" /></a></td></tr><tr><td><span align=\"center\"><span class=\"SobekThumbnailText\" style=\"display:inline-block;\">" + thisPage.Label + "</span></span></td></tr></table></span></span>");
                    else if(size_of_thumbnails==4)
                        builder.AppendLine("<span style=\"display:inline-block;\" align=\"center\" id=\"parent" + image_url + "\"><table style=\"display:inline-block;\"><tr><td><a id=\"" + thisPage.Label + "\" href=\"" + url + "\"><img id=\"child" + image_url + "\" src=\"" + image_url + "\"  alt=\"MISSING THUMBNAIL\" class=\"itemThumbnails\" /></a></td></tr><tr><td><span align=\"center\"><span class=\"SobekThumbnailText\" style=\"display:inline-block;\">" + thisPage.Label + "</span></span></td></tr></table></span></span>");
                    else
                    {
                        builder.AppendLine("<span align=\"center\" style=\"display:inline-block;\"><table style=\"display:inline-block;\"><tr><td><a id=\"" + thisPage.Label + "\" href=\"" + url + "\"><img src=\"" + image_url + "\" alt=\"MISSING THUMBNAIL\" class=\"itemThumbnails\" /></a></td></tr><tr><td><span align=\"center\" style=\"display:inline-block;\"><span class=\"SobekThumbnailText\" style=\"display:inline-block;\">" + thisPage.Label + "</span></span></td></tr></table></span></span>");
                    }
					break;
				}
			 
			}

			//Close the outer div
			builder.AppendLine("</span></div>");

			// Restore the mode
			CurrentMode.ViewerCode = current_view_code;
			CurrentMode.Page = current_view_page;


            // Finish the citation table
            builder.AppendLine("\t\t</td>");
            builder.AppendLine("\t\t<!-- END RELATED IMAGES VIEWER OUTPUT -->");

            //If the current url has an anchor, call the javascript function to animate the corresponding span background color
            builder.AppendLine("<script type=\"text/javascript\">window.onload=MakeSpanFlashOnPageLoad();</script>");
		    builder.AppendLine("<script type=\"text/javascript\"> WindowResizeActions();</script>");

            // Add the HTML for the images
			Literal mainLiteral = new Literal {Text = builder.ToString()};
			placeHolder.Controls.Add( mainLiteral );

        }
	   
		/// <summary>
		/// Add the Viewer specific information to the top navigation row
		/// This nav row adds the different thumbnail viewing options(# of thumbnails, size of thumbnails, list of all related item thumbnails)
		/// </summary>
		public override string NavigationRow
		{
			get
			{
			    string Num_Of_Thumbnails = "Thumbnails per page";
			    string Size_Of_Thumbnail = "Thumbnail size";
			    string Go_To_Thumbnail = "Go to thumbnail";

			    if (CurrentMode.Language == Web_Language_Enum.French)
			    {
			        Num_Of_Thumbnails = "Vignettes par page";
			        Size_Of_Thumbnail = "la taille des vignettes";
			        Go_To_Thumbnail = "Aller � l'Vignette";
			    }

			    if (CurrentMode.Language == Web_Language_Enum.Spanish)
			    {
			        Num_Of_Thumbnails = "Miniaturas por p�gina";
			        Size_Of_Thumbnail = "Miniatura de tama�o";
			        Go_To_Thumbnail = "Ir a la miniatura";
			    }


			    // Build the value
			    StringBuilder navRowBuilder = new StringBuilder(5000);

                //Start building the top nav bar
                navRowBuilder.AppendLine("\t\t<!-- RELATED IMAGES VIEWER TOP NAV ROW -->");

			    //Include the js files
			    navRowBuilder.AppendLine("<script language=\"JavaScript\" src=\"" + CurrentMode.Base_URL + "default/scripts/sobekcm_related_items.js\"></script>");
			    navRowBuilder.AppendLine("<script type=\"text/javascript\" src=\"" + CurrentMode.Base_URL + "default/scripts/jquery-1.9.1.js\"></script>");
			    navRowBuilder.AppendLine("<script type=\"text/javascript\" src=\"" + CurrentMode.Base_URL + "default/scripts/jquery-ui-1.10.1.js\"></script>");
			    navRowBuilder.AppendLine("<script type=\"text/javascript\" src=\"" + CurrentMode.Base_URL + "default/scripts/jquery.color-2.1.1.js\"></script>");
                navRowBuilder.AppendLine("<table width=\"100%\"><tr align=\"center\">");
                
			    //Add the dropdown for the number of thumbnails per page, only if there are >25 thumbnails
			    if (CurrentItem.Web.Static_PageCount > 25)
			    {
			        //Redirect to the first page of results when the number of thumbnails option is changed by the user
			        string current_viewercode = CurrentMode.ViewerCode;
			        CurrentMode.ViewerCode = "1thumbs";
			        string current_Page_url = CurrentMode.Redirect_URL("1thumbs");

			        //   CurrentMode.Thumbnails_Per_Page = -1;
			        //  string current_Page_url = CurrentMode.Redirect_URL("1thumbs");

			        // Collect the list of options to display
			        List<int> thumbsOptions = new List<int>();
			        thumbsOptions.Add(25);
			        if (CurrentItem.Web.Static_PageCount > 50) thumbsOptions.Add(50);
			        if (CurrentItem.Web.Static_PageCount > 100) thumbsOptions.Add(100);
			        if (CurrentItem.Web.Static_PageCount > 250) thumbsOptions.Add(250);
			        if (CurrentItem.Web.Static_PageCount > 500) thumbsOptions.Add(500);

			        // Start the drop down select list 
			        navRowBuilder.AppendLine("<td valign=\"top\">");
			        navRowBuilder.AppendLine("<span><select id=\"selectNumOfThumbnails\" onchange=\"location=this.options[this.selectedIndex].value;\">");
			        //   navRowBuilder.AppendLine("<br/><br/><span><select id=\"selectNumOfThumbnails\" onchange=current_Page_url>");

			        // Step through all the options
			        foreach (int thumbOption in thumbsOptions)
			        {
			            CurrentMode.Thumbnails_Per_Page = (short) thumbOption;
			            if (thumbnailsPerPage == thumbOption)
			            {
			                navRowBuilder.AppendLine("<option value=\"" + CurrentMode.Redirect_URL() + "\" selected=\"selected\">" + thumbOption + " thumbnails per page</option>");
			            }
			            else
			            {

			                navRowBuilder.AppendLine("<option value=\"" + CurrentMode.Redirect_URL() + "\">" + thumbOption + " thumbnails per page</option>");
			            }
			        }

			        CurrentMode.Thumbnails_Per_Page = -1;
			        if (thumbnailsPerPage == int.MaxValue)
			        {
			            navRowBuilder.AppendLine("<option value=\"" + CurrentMode.Redirect_URL() + "\" selected=\"selected\">All thumbnails</option>");
			        }
			        else
			        {
			            navRowBuilder.AppendLine("<option value=\"" + CurrentMode.Redirect_URL() + "\">All thumbnails</option>");
			        }

			        //Reset the Current Mode Thumbnails_Per_Page

			        CurrentMode.ViewerCode = current_viewercode;
			        navRowBuilder.AppendLine("</select></span>");
                    navRowBuilder.AppendLine("</td>");

			    }
			    CurrentMode.Thumbnails_Per_Page = -100;


			    //Add the control for the thumbnail size

			    //Get the icons for the thumbnail sizes
			    string image_location = CurrentMode.Default_Images_URL;
			    navRowBuilder.AppendLine("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

			    //Get the querystring, if any, from the current url
			    string curr_url1 = HttpContext.Current.Request.RawUrl;
			    string queryString1 = null;

			    //Check if query string variables exist in the url
			    int index_queryString1 = curr_url1.IndexOf('?');

			    if (index_queryString1 > 0)
			    {
			        queryString1 = (index_queryString1 < curr_url1.Length - 1) ? curr_url1.Substring(index_queryString1) : String.Empty;
			    }

			    //Redirect to the first page of results when the number of thumbnails option is changed by the user

			    //   CurrentMode.Size_Of_Thumbnails= -1;

                navRowBuilder.AppendLine("<td valign=\"top\">");
			    if (thumbnailSize == 1)
			        //  navRowBuilder.Append("<a href=\"#\" onclick=\"location=UpdateQueryString('ts','1','" + current_Page_url1 + "')\"><img src=\"" + image_location + "sizetools1.gif\"/></a>");
			        navRowBuilder.Append("<a href=\"" + CurrentMode.Redirect_URL("1thumbs") + "\"><img src=\"" + image_location + "thumbs3.gif\"/></a>");
			    else
			    {
			        CurrentMode.Size_Of_Thumbnails = 1;
                    navRowBuilder.Append("<a href=\"" + CurrentMode.Redirect_URL("1thumbs") + "\"><img src=\"" + image_location + "thumbs3.gif\"/></a>");
			    }
			    if (thumbnailSize == 2)
                    navRowBuilder.Append("<a href=\"" + CurrentMode.Redirect_URL("1thumbs") + "\"><img src=\"" + image_location + "thumbs2.gif\"/></a>");
			    else
			    {
			        CurrentMode.Size_Of_Thumbnails = 2;
                    navRowBuilder.Append("<a href=\"" + CurrentMode.Redirect_URL("1thumbs") + "\"><img src=\"" + image_location + "thumbs2.gif\"/></a>");
			    }
			    if (thumbnailSize == 3)
                    navRowBuilder.Append("<a href=\"" + CurrentMode.Redirect_URL("1thumbs") + "\"><img src=\"" + image_location + "thumbs1.gif\"/></a>");
			    else
			    {
			        CurrentMode.Size_Of_Thumbnails = 3;
                    navRowBuilder.Append("<a href=\"" + CurrentMode.Redirect_URL("1thumbs") + "\"><img src=\"" + image_location + "thumbs1.gif\"/></a>");
			    }
			    //Reset the current mode
			    CurrentMode.Size_Of_Thumbnails = -1;
                navRowBuilder.AppendLine("</td>");


			    //Add the dropdown for the thumbnail anchor within the page to directly navigate to
                navRowBuilder.AppendLine("<td valign=\"top\">");
			    navRowBuilder.AppendLine("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
			    navRowBuilder.AppendLine("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>" + Go_To_Thumbnail + ":</b>");
			    navRowBuilder.AppendLine("<span><select id=\"selectGoToThumbnail\" onchange=\"location=this.options[this.selectedIndex].value; AddAnchorDivEffect(this.options[this.selectedIndex].value);\" >");

			    //iterate through the page items
			    if (CurrentItem.Web.Static_PageCount > 0)
			    {
			        int thumbnail_count = 0;
			        foreach (Page_TreeNode thisFile in CurrentItem.Web.Pages_By_Sequence)
			        {
			            thumbnail_count++;

			            string current_Page_url1 = CurrentMode.Redirect_URL((thumbnail_count/thumbnailsPerPage + (thumbnail_count%thumbnailsPerPage == 0 ? 0 : 1)).ToString() + "thumbs");

			          //  navRowBuilder.AppendLine("<option value=\"" + current_Page_url1 + "#" + thisFile.Label + "\">" + thisFile.Label + "</option>");
                        if (String.IsNullOrEmpty(thisFile.Label))
                            navRowBuilder.AppendLine("<option value=\"" + current_Page_url1 + "#" + thumbnail_count + "\">" + "(page " + thumbnail_count + ")" + "</option>");
                        else
                        {
                            navRowBuilder.AppendLine("<option value=\"" + current_Page_url1 + "#" + thumbnail_count + "\">" + thisFile.Label + "</option>");
                        }

			        }
			    }
			    navRowBuilder.AppendLine("</select></span>");
             //   navRowBuilder.AppendLine("<br /><br />");


			    navRowBuilder.AppendLine("</td></tr></table>");

			    // Finish the nav row controls
			    navRowBuilder.AppendLine("\t\t<!-- END RELATED IMAGES VIEWER NAV ROW -->");

			    // Return the html string
			    return navRowBuilder.ToString();

			}
		}
 

		/// <summary> Adds any viewer_specific information to the Navigation Bar Menu Section </summary>
		/// <param name="placeHolder"> Additional place holder ( &quot;navigationPlaceHolder&quot; ) in the itemNavForm form allows item-viewer-specific controls to be added to the left navigation bar</param>
		/// <param name="Internet_Explorer"> Flag indicates if the current browser is internet explorer </param>
		/// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
		/// <returns> Returns FALSE since nothing was added to the left navigational bar </returns>
		/// <remarks> For this item viewer, this method does nothing except return FALSE </remarks>
		public override bool Add_Nav_Bar_Menu_Section(PlaceHolder placeHolder, bool Internet_Explorer, Custom_Tracer Tracer)
		{
			if (Tracer != null)
			{
				Tracer.Add_Trace("Related_Images_ItemViewer.Add_Nav_Bar_Menu_Section", "Nothing added to placeholder");
			 }
			
		   return false;
		}


        /// <summary> This provides an opportunity for the viewer to perform any pre-display work
        /// which is necessary before entering any of the rendering portions </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        public override void Perform_PreDisplay_Work(Custom_Tracer Tracer)
        {
            // Get the proper number of thumbnails per page
            if (CurrentUser != null)
            {
                // First, pull the thumbnails per page from the user options
                thumbnailsPerPage = CurrentUser.Get_Option("Related_Images_ItemViewer:ThumbnailsPerPage", 50);

                // Or was there a new value in the URL?
                if (CurrentMode.Thumbnails_Per_Page >= -1)
                {
                    CurrentUser.Add_Option("Related_Images_ItemViewer:ThumbnailsPerPage", CurrentMode.Thumbnails_Per_Page);
                    thumbnailsPerPage = CurrentMode.Thumbnails_Per_Page;
                }
            }
            else
            {
                int tempValue = 50;
                object sessionValue = HttpContext.Current.Session["Related_Images_ItemViewer:ThumbnailsPerPage"];
                if (sessionValue != null)
                {
                    int.TryParse(sessionValue.ToString(), out tempValue);
                }
                thumbnailsPerPage = tempValue;

                // Or was there a new value in the URL?
                if (CurrentMode.Thumbnails_Per_Page >= -1)
                {
                    HttpContext.Current.Session["Related_Images_ItemViewer:ThumbnailsPerPage"] = CurrentMode.Thumbnails_Per_Page;
                    thumbnailsPerPage = CurrentMode.Thumbnails_Per_Page;
                }
            }

            // -1 means to display all thumbnails
            if (thumbnailsPerPage == -1 )
                thumbnailsPerPage = int.MaxValue;

            // Now, reset the value in the navigation object, since we won't need to set it again
            CurrentMode.Thumbnails_Per_Page = -100;

            // Get the proper size of thumbnails per page
            if (CurrentUser != null)
            {
                // First, pull the thumbnails per page from the user options
                thumbnailSize = CurrentUser.Get_Option("Related_Images_ItemViewer:ThumbnailSize", 1);

                // Or was there a new value in the URL?
                if (CurrentMode.Size_Of_Thumbnails > -1)
                {
                    CurrentUser.Add_Option("Related_Images_ItemViewer:ThumbnailSize", CurrentMode.Size_Of_Thumbnails);
                    thumbnailSize = CurrentMode.Size_Of_Thumbnails;
                }
            }
            else
            {
                int tempValue = 1;
                object sessionValue = HttpContext.Current.Session["Related_Images_ItemViewer:ThumbnailSize"];
                if (sessionValue != null)
                {
                    int.TryParse(sessionValue.ToString(), out tempValue);
                }
                thumbnailSize = tempValue;

                // Or was there a new value in the URL?
                if (CurrentMode.Size_Of_Thumbnails > -1)
                {
                    HttpContext.Current.Session["Related_Images_ItemViewer:ThumbnailSize"] = CurrentMode.Size_Of_Thumbnails;
                    thumbnailSize = CurrentMode.Size_Of_Thumbnails;
                }
            }

            // Now, reset the value in the navigation object, since we won't need to set it again
            CurrentMode.Size_Of_Thumbnails = -1;

        }

        /// <summary> Gets the flag that indicates if the page selector should be shown </summary>
        /// <value> This is a single page viewer, so this property always returns NONE</value>
        public override ItemViewer_PageSelector_Type_Enum Page_Selector
        {
            get
            {
                return ItemViewer_PageSelector_Type_Enum.PageLinks;
            }
        }
	}
}
