﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using SobekCM.Library.ItemViewer.Viewers;

namespace SobekCM.Library.ItemViewer.Fragments
{
    /// <summary> Base class is used for all fragment item viewers, which just write a single fragment 
    /// to the stream, such as the print or send/email forms.  THis is used to allow the page to dynamically
    /// load these forms as needed. </summary>
    public abstract class baseFragment_ItemViewer : abstractItemViewer
    {
        /// <summary> Abstract method adds any viewer_specific information to the Navigation Bar Menu Section </summary>
        /// <param name="placeHolder"> Additional place holder ( &quot;navigationPlaceHolder&quot; ) in the itemNavForm form allows item-viewer-specific controls to be added to the left navigation bar</param>
        /// <param name="Internet_Explorer"> Flag indicates if the current browser is internet explorer </param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <returns> TRUE if this viewer added something to the left navigational bar, otherwise FALSE</returns>
        /// <remarks> This always return FALSE, and adds nothing to the nav bar</remarks>
        public override bool Add_Nav_Bar_Menu_Section(PlaceHolder placeHolder, bool Internet_Explorer, Custom_Tracer Tracer)
        {
            return false;
        }

        /// <summary> Gets any HTML for a Navigation Row above the image or text </summary>
        /// <value> Always returns the empty string </value>
        public override string NavigationRow
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary> Gets the number of pages for this viewer </summary>
        /// <value> This always returns 1, to suppress any pagination from occurring </value>
        public override int PageCount
        {
            get { return 1; }
        }

        /// <summary> Gets the url to go to the first page </summary>
        /// <value> Always returns an empty string </value>
        public override string First_Page_URL
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary> Gets the url to go to the previous page </summary>
        /// <value> Always returns an empty string </value>
        public override string Previous_Page_URL
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary> Gets the url to go to the next page </summary>
        /// <value> Always returns an empty string </value>
        public override string Next_Page_URL
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary> Gets the url to go to the last page </summary>
        /// <value> Always returns an empty string </value>
        public override string Last_Page_URL
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary> Gets the names to show in the Go To combo box </summary>
        /// <value> Always returns an empty string array </value>
        public override string[] Go_To_Names
        {
            get
            {
               return new string[] { };
            }
        }

        /// <summary> Flag indicates if the header (with the title, group title, etc..) should be displayed </summary>
        /// <value> Always returns FALSE </value>
        public override bool Show_Header
        {
            get
            {
                return false;
            }
        }
    }
}
