﻿#region Using directives

using System.Web.UI.WebControls;

#endregion

namespace SobekCM.Library.ItemViewer.Viewers
{
    /// <summary> Item viewer is substituted for the regular image/download viewers if an item is 
    /// restricted to single fair use and someone else is currently viewing the item.</summary>
    /// <remarks> This class extends the abstract class <see cref="abstractItemViewer"/> and implements the 
    /// <see cref="iItemViewer" /> interface. </remarks>
    public class Checked_Out_ItemViewer : abstractItemViewer
    {
        /// <summary> Width for the main viewer section to adjusted to accomodate this viewer</summary>
        /// <value> This always returns the value 600 </value>
        public override int Viewer_Width
        {
            get
            {
                return 600;
            }
        }

        /// <summary> Gets the type of item viewer this object represents </summary>
        /// <value> This property always returns the enumerational value <see cref="ItemViewer_Type_Enum.Checked_Out"/>. </value>
        public override ItemViewer_Type_Enum ItemViewer_Type
        {
            get { return ItemViewer_Type_Enum.Checked_Out;  }
        }

        /// <summary> Gets the number of pages for this viewer </summary>
        /// <value> This is a single page viewer, so this property always returns the value 1</value>
        public override int PageCount
        {
            get
            {
                return 1;
            }            
        }

        /// <summary> Gets the flag that indicates if the page selector should be shown </summary>
        /// <value> This is a single page viewer, so this property always returns NONE</value>
        public override ItemViewer_PageSelector_Type_Enum Page_Selector
        {
            get
            {
                return ItemViewer_PageSelector_Type_Enum.NONE;
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
            return false;
        }

        /// <summary> Adds the main view section to the page turner </summary>
        /// <param name="placeHolder"> Main place holder ( &quot;mainPlaceHolder&quot; ) in the itemNavForm form into which the the bulk of the item viewer's output is displayed</param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        public override void Add_Main_Viewer_Section(PlaceHolder placeHolder, Custom_Tracer Tracer)
        {
            Literal mainLiteral = new Literal { Text = "<td align=\"center\" colspan=\"3\"><br /><br /><b>The item you have requested contains copyright material and is reserved for single-use.  <br /><br />Someone has currently checked out this digital copy for viewing.  <br /><br />Please try again in several minutes.</b><br /><br /><br /></td>" };
            placeHolder.Controls.Add(mainLiteral);
        }
    }
}
