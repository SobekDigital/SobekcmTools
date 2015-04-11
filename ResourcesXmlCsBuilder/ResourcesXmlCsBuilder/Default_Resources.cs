using System;
using System.IO;
using System.Xml;

namespace ResourcesXmlCsBuilder
{
    /// <summary> Gateway to all of the static resources (images, javascript, stylesheets, and included libraries ) 
    /// used by the standard SobekCM web user interface </summary>
    public static class Default_Resources
    {
        /// <summary> Static constructor for the Default_Resources class </summary>
        static Default_Resources()
        {
            // Set the default values, using the CDN
            Sixteen_Px_Feed_Icon_Svg_Png = "http://cdn.sobekrepository.org/images/misc/16px-Feed-icon.svg.png";
            Add_Geospatial_Icon_Png = "http://cdn.sobekrepository.org/images/misc/add_geospatial_icon.png";
            Add_Volume_Png = "http://cdn.sobekrepository.org/images/misc/add_volume.png";
            Add_Volume_Icon_Png = "http://cdn.sobekrepository.org/images/misc/add_volume_icon.png";
            Admin_View_Png = "http://cdn.sobekrepository.org/images/misc/admin_view.png";
            Ajax_Loader_Gif = "http://cdn.sobekrepository.org/images/mapedit/ajax-loader.gif";
            Arw05lt_Gif = "http://cdn.sobekrepository.org/images/qc/ARW05LT.gif";
            Arw05rt_Gif = "http://cdn.sobekrepository.org/images/qc/ARW05RT.gif";
            Autofill_Volumes_Png = "http://cdn.sobekrepository.org/images/misc/autofill_volumes.png";
            Bg1_Png = "http://cdn.sobekrepository.org/images/mapedit/bg1.png";
            Big_Bookshelf_Gif = "http://cdn.sobekrepository.org/images/misc/big_bookshelf.gif";
            Blue_Png = "http://cdn.sobekrepository.org/images/mapedit/mapIcons/blue.png";
            Blue_Pin_Png = "http://cdn.sobekrepository.org/images/mapsearch/blue-pin.png";
            Bookshelf_Jpg = "http://cdn.sobekrepository.org/images/misc/bookshelf.jpg";
            Bookshelf_Png = "http://cdn.sobekrepository.org/images/misc/bookshelf.png";
            Bookturner_Html = "http://cdn.sobekrepository.org/images/misc/bookturner.html";
            Bookturner_Js = "http://cdn.sobekrepository.org/includes/bookturner/1.0.0/bookturner.js";
            Brief_Blue_Png = "http://cdn.sobekrepository.org/images/mapsearch/brief_blue.png";
            Building_Gif = "http://cdn.sobekrepository.org/images/misc/building.gif";
            Button_Down_Arrow_Png = "http://cdn.sobekrepository.org/images/misc/button_down_arrow.png";
            Button_First_Arrow_Png = "http://cdn.sobekrepository.org/images/misc/button_first_arrow.png";
            Button_Last_Arrow_Png = "http://cdn.sobekrepository.org/images/misc/button_last_arrow.png";
            Button_Next_Arrow_Png = "http://cdn.sobekrepository.org/images/misc/button_next_arrow.png";
            Button_Next_Arrow2_Png = "http://cdn.sobekrepository.org/images/misc/button_next_arrow2.png";
            Button_Previous_Arrow_Png = "http://cdn.sobekrepository.org/images/misc/button_previous_arrow.png";
            Button_Up_Arrow_Png = "http://cdn.sobekrepository.org/images/misc/button_up_arrow.png";
            Button_Action1_Png = "http://cdn.sobekrepository.org/images/mapedit/button-action1.png";
            Button_Action2_Png = "http://cdn.sobekrepository.org/images/mapedit/button-action2.png";
            Button_Action3_Png = "http://cdn.sobekrepository.org/images/mapedit/button-action3.png";
            Button_Base_Png = "http://cdn.sobekrepository.org/images/mapedit/button-base.png";
            Button_Blocklot_Png = "http://cdn.sobekrepository.org/images/mapedit/button-blockLot.png";
            Button_Cancel_Png = "http://cdn.sobekrepository.org/images/mapedit/button-cancel.png";
            Button_Controls_Png = "http://cdn.sobekrepository.org/images/mapedit/button-controls.png";
            Button_Converttooverlay_Png = "http://cdn.sobekrepository.org/images/mapedit/button-convertToOverlay.png";
            Button_Drawcircle_Png = "http://cdn.sobekrepository.org/images/mapedit/button-drawCircle.png";
            Button_Drawline_Png = "http://cdn.sobekrepository.org/images/mapedit/button-drawLine.png";
            Button_Drawmarker_Png = "http://cdn.sobekrepository.org/images/mapedit/button-drawMarker.png";
            Button_Drawpolygon_Png = "http://cdn.sobekrepository.org/images/mapedit/button-drawPolygon.png";
            Button_Drawrectangle_Png = "http://cdn.sobekrepository.org/images/mapedit/button-drawRectangle.png";
            Button_Hybrid_Png = "http://cdn.sobekrepository.org/images/mapedit/button-hybrid.png";
            Button_Itemgetuserlocation_Png = "http://cdn.sobekrepository.org/images/mapedit/button-itemGetUserLocation.png";
            Button_Itemplace_Png = "http://cdn.sobekrepository.org/images/mapedit/button-itemPlace.png";
            Button_Itemreset_Png = "http://cdn.sobekrepository.org/images/mapedit/button-itemReset.png";
            Button_Layercustom_Png = "http://cdn.sobekrepository.org/images/mapedit/button-layerCustom.png";
            Button_Layerhybrid_Png = "http://cdn.sobekrepository.org/images/mapedit/button-layerHybrid.png";
            Button_Layerreset_Png = "http://cdn.sobekrepository.org/images/mapedit/button-layerReset.png";
            Button_Layerroadmap_Png = "http://cdn.sobekrepository.org/images/mapedit/button-layerRoadmap.png";
            Button_Layersatellite_Png = "http://cdn.sobekrepository.org/images/mapedit/button-layerSatellite.png";
            Button_Layerterrain_Png = "http://cdn.sobekrepository.org/images/mapedit/button-layerTerrain.png";
            Button_Manageitem_Png = "http://cdn.sobekrepository.org/images/mapedit/button-manageItem.png";
            Button_Manageoverlay_Png = "http://cdn.sobekrepository.org/images/mapedit/button-manageOverlay.png";
            Button_Managepoi_Png = "http://cdn.sobekrepository.org/images/mapedit/button-managePOI.png";
            Button_Overlayedit_Png = "http://cdn.sobekrepository.org/images/mapedit/button-overlayEdit.png";
            Button_Overlaygetuserlocation_Png = "http://cdn.sobekrepository.org/images/mapedit/button-overlayGetUserLocation.png";
            Button_Overlayplace_Png = "http://cdn.sobekrepository.org/images/mapedit/button-overlayPlace.png";
            Button_Overlayreset_Png = "http://cdn.sobekrepository.org/images/mapedit/button-overlayReset.png";
            Button_Overlayrotate_Png = "http://cdn.sobekrepository.org/images/mapedit/button-overlayRotate.png";
            Button_Overlaytoggle_Png = "http://cdn.sobekrepository.org/images/mapedit/button-overlayToggle.png";
            Button_Overlaytransparency_Png = "http://cdn.sobekrepository.org/images/mapedit/button-overlayTransparency.png";
            Button_Pandown_Png = "http://cdn.sobekrepository.org/images/mapedit/button-panDown.png";
            Button_Panleft_Png = "http://cdn.sobekrepository.org/images/mapedit/button-panLeft.png";
            Button_Panreset_Png = "http://cdn.sobekrepository.org/images/mapedit/button-panReset.png";
            Button_Panright_Png = "http://cdn.sobekrepository.org/images/mapedit/button-panRight.png";
            Button_Panup_Png = "http://cdn.sobekrepository.org/images/mapedit/button-panUp.png";
            Button_Poicircle_Png = "http://cdn.sobekrepository.org/images/mapedit/button-poiCircle.png";
            Button_Poiedit_Png = "http://cdn.sobekrepository.org/images/mapedit/button-poiEdit.png";
            Button_Poigetuserlocation_Png = "http://cdn.sobekrepository.org/images/mapedit/button-poiGetUserLocation.png";
            Button_Poiline_Png = "http://cdn.sobekrepository.org/images/mapedit/button-poiLine.png";
            Button_Poimarker_Png = "http://cdn.sobekrepository.org/images/mapedit/button-poiMarker.png";
            Button_Poiplace_Png = "http://cdn.sobekrepository.org/images/mapedit/button-poiPlace.png";
            Button_Poipolygon_Png = "http://cdn.sobekrepository.org/images/mapedit/button-poiPolygon.png";
            Button_Poirectangle_Png = "http://cdn.sobekrepository.org/images/mapedit/button-poiRectangle.png";
            Button_Poireset_Png = "http://cdn.sobekrepository.org/images/mapedit/button-poiReset.png";
            Button_Poitoggle_Png = "http://cdn.sobekrepository.org/images/mapedit/button-poiToggle.png";
            Button_Reset_Png = "http://cdn.sobekrepository.org/images/mapedit/button-reset.png";
            Button_Roadmap_Png = "http://cdn.sobekrepository.org/images/mapedit/button-roadmap.png";
            Button_Satellite_Png = "http://cdn.sobekrepository.org/images/mapedit/button-satellite.png";
            Button_Save_Png = "http://cdn.sobekrepository.org/images/mapedit/button-save.png";
            Button_Search_Png = "http://cdn.sobekrepository.org/images/mapedit/button-search.png";
            Button_Terrain_Png = "http://cdn.sobekrepository.org/images/mapedit/button-terrain.png";
            Button_Togglemapcontrols_Png = "http://cdn.sobekrepository.org/images/mapedit/button-toggleMapControls.png";
            Button_Toggletoolbar_Png = "http://cdn.sobekrepository.org/images/mapedit/button-toggleToolbar.png";
            Button_Toggletoolbox_Png = "http://cdn.sobekrepository.org/images/mapedit/button-toggleToolbox.png";
            Button_Toolbox_Png = "http://cdn.sobekrepository.org/images/mapedit/button-toolbox.png";
            Button_Usesearchaslocation_Png = "http://cdn.sobekrepository.org/images/mapedit/button-useSearchAsLocation.png";
            Button_Zoomin_Png = "http://cdn.sobekrepository.org/images/mapedit/button-zoomIn.png";
            Button_Zoomout_Png = "http://cdn.sobekrepository.org/images/mapedit/button-zoomOut.png";
            Button_Zoomreset_Png = "http://cdn.sobekrepository.org/images/mapedit/button-zoomReset.png";
            Button_Zoomreset2_Png = "http://cdn.sobekrepository.org/images/mapedit/button-zoomReset2.png";
            Calendar_Button_Png = "http://cdn.sobekrepository.org/images/misc/calendar_button.png";
            Cancel_Ico = "http://cdn.sobekrepository.org/images/qc/Cancel.ico";
            Cc_By_Png = "http://cdn.sobekrepository.org/images/misc/cc_by.png";
            Cc_By_Nc_Png = "http://cdn.sobekrepository.org/images/misc/cc_by_nc.png";
            Cc_By_Nc_Nd_Png = "http://cdn.sobekrepository.org/images/misc/cc_by_nc_nd.png";
            Cc_By_Nc_Sa_Png = "http://cdn.sobekrepository.org/images/misc/cc_by_nc_sa.png";
            Cc_By_Nd_Png = "http://cdn.sobekrepository.org/images/misc/cc_by_nd.png";
            Cc_By_Sa_Png = "http://cdn.sobekrepository.org/images/misc/cc_by_sa.png";
            Cc_Zero_Png = "http://cdn.sobekrepository.org/images/misc/cc_zero.png";
            Chart_Js = "http://cdn.sobekrepository.org/includes/chartjs/1.0.2/Chart.min.js";
            Chat_Png = "http://cdn.sobekrepository.org/images/misc/chat.png";
            Checkmark_Png = "http://cdn.sobekrepository.org/images/misc/checkmark.png";
            Checkmark2_Png = "http://cdn.sobekrepository.org/images/misc/checkmark2.png";
            Ckeditor_Js = "http://cdn.sobekrepository.org/includes/ckeditor/4.4.7/ckeditor.js";
            Closed_Folder_Jpg = "http://cdn.sobekrepository.org/images/misc/closed_folder.jpg";
            Closed_Folder_Public_Jpg = "http://cdn.sobekrepository.org/images/misc/closed_folder_public.jpg";
            Closed_Folder_Public_Big_Jpg = "http://cdn.sobekrepository.org/images/misc/closed_folder_public_big.jpg";
            Contentslider_Js = "http://cdn.sobekrepository.org/includes/contentslider/2.4/contentslider.min.js";
            Dark_Resource_Png = "http://cdn.sobekrepository.org/images/misc/dark_resource.png";
            Default_Banner_Jpg = "http://cdn.sobekrepository.org/images/misc/default_banner.jpg";
            Default_Button_Gif = "http://cdn.sobekrepository.org/images/misc/default_button.gif";
            Default_Button_Png = "http://cdn.sobekrepository.org/images/misc/default_button.png";
            Delete_Cursor_Cur = "http://cdn.sobekrepository.org/images/qc/delete_cursor.cur";
            Delete_Item_Png = "http://cdn.sobekrepository.org/images/misc/delete_item.png";
            Delete_Item_Icon_Png = "http://cdn.sobekrepository.org/images/misc/delete_item_icon.png";
            Digg_Share_Gif = "http://cdn.sobekrepository.org/images/misc/digg_share.gif";
            Digg_Share_H_Gif = "http://cdn.sobekrepository.org/images/misc/digg_share_h.gif";
            Dloc_Banner_700_Jpg = "http://cdn.sobekrepository.org/images/misc/dloc_banner_700.jpg";
            Drag1pg_Ico = "http://cdn.sobekrepository.org/images/qc/DRAG1PG.ICO";
            Edit_Gif = "http://cdn.sobekrepository.org/images/misc/edit.gif";
            Edit_Png = "http://cdn.sobekrepository.org/images/mapedit/edit.png";
            Edit_Behaviors_Png = "http://cdn.sobekrepository.org/images/misc/edit_behaviors.png";
            Edit_Behaviors_Icon_Png = "http://cdn.sobekrepository.org/images/misc/edit_behaviors_icon.png";
            Edit_Hierarchy_Png = "http://cdn.sobekrepository.org/images/misc/edit_hierarchy.png";
            Edit_Metadata_Png = "http://cdn.sobekrepository.org/images/misc/edit_metadata.png";
            Edit_Metadata_Icon_Png = "http://cdn.sobekrepository.org/images/misc/edit_metadata_icon.png";
            Email_Png = "http://cdn.sobekrepository.org/images/misc/email.png";
            Emptypage_Jpg = "http://cdn.sobekrepository.org/images/bookturner/emptypage.jpg";
            Exit_Gif = "http://cdn.sobekrepository.org/images/misc/exit.gif";
            Facebook_Share_Gif = "http://cdn.sobekrepository.org/images/misc/facebook_share.gif";
            Facebook_Share_H_Gif = "http://cdn.sobekrepository.org/images/misc/facebook_share_h.gif";
            Favorites_Share_Gif = "http://cdn.sobekrepository.org/images/misc/favorites_share.gif";
            Favorites_Share_H_Gif = "http://cdn.sobekrepository.org/images/misc/favorites_share_h.gif";
            File_Management_Png = "http://cdn.sobekrepository.org/images/misc/file_management.png";
            File_Management_Icon_Png = "http://cdn.sobekrepository.org/images/misc/file_management_icon.png";
            Firewall_Gif = "http://cdn.sobekrepository.org/images/misc/firewall.gif";
            Firewall_Png = "http://cdn.sobekrepository.org/images/misc/firewall.png";
            First2_Png = "http://cdn.sobekrepository.org/images/bookturner/first2.png";
            Forwarding_Gif = "http://cdn.sobekrepository.org/images/misc/forwarding.gif";
            Forwarding_Png = "http://cdn.sobekrepository.org/images/misc/forwarding.png";
            Gears_Png = "http://cdn.sobekrepository.org/images/misc/gears.png";
            Geo_Blue_Png = "http://cdn.sobekrepository.org/images/mapsearch/geo_blue.png";
            Get_Adobe_Reader_Png = "http://cdn.sobekrepository.org/images/misc/get_adobe_reader.png";
            Getuserlocation_Png = "http://cdn.sobekrepository.org/images/mapedit/getUserLocation.png";
            Go_Button_Png = "http://cdn.sobekrepository.org/images/misc/go_button.png";
            Go_Gray_Gif = "http://cdn.sobekrepository.org/images/misc/go_gray.gif";
            Google_Share_Gif = "http://cdn.sobekrepository.org/images/misc/google_share.gif";
            Google_Share_H_Gif = "http://cdn.sobekrepository.org/images/misc/google_share_h.gif";
            Help_Button_Jpg = "http://cdn.sobekrepository.org/images/misc/help_button.jpg";
            Help_Button_Darkgray_Jpg = "http://cdn.sobekrepository.org/images/misc/help_button_darkgray.jpg";
            Hide_Internal_Header_Png = "http://cdn.sobekrepository.org/images/misc/hide_internal_header.png";
            Hide_Internal_Header2_Png = "http://cdn.sobekrepository.org/images/misc/hide_internal_header2.png";
            Home_Png = "http://cdn.sobekrepository.org/images/misc/home.png";
            Home_Button_Gif = "http://cdn.sobekrepository.org/images/misc/home_button.gif";
            Home_Folder_Gif = "http://cdn.sobekrepository.org/images/misc/home_folder.gif";
            Html5shiv_Js = "http://cdn.sobekrepository.org/includes/html5shiv/3.7.3/html5shiv.js";
            Icon_Permission_Png = "http://cdn.sobekrepository.org/images/misc/icon_permission.png";
            Icons_Os_Png = "http://cdn.sobekrepository.org/images/mapedit/icons-os.png";
            Index_Html = "http://cdn.sobekrepository.org/images/misc/index.html";
            Item_Count_Png = "http://cdn.sobekrepository.org/images/misc/item_count.png";
            Jquery_Color_2_1_1_Js = "http://cdn.sobekrepository.org/includes/jquery-color/2.1.1/jquery.color-2.1.1.js";
            Jquery_Datatables_Js = "http://cdn.sobekrepository.org/includes/datatables/1.11.1/js/jquery.dataTables.min.js";
            Jquery_Easing_1_3_Js = "http://cdn.sobekrepository.org/includes/bookturner/1.0.0/jquery.easing.1.3.js";
            Jquery_Hovercard_Js = "http://cdn.sobekrepository.org/includes/jquery-hovercard/2.4/jquery.hovercard.min.js";
            Jquery_Mousewheel_Js = "http://cdn.sobekrepository.org/includes/jquery-mousewheel/3.1.3/jquery.mousewheel.js";
            Jquery_Qtip_Css = "http://cdn.sobekrepository.org/includes/jquery-qtip/2.2.0/jquery.qtip.min.css";
            Jquery_Qtip_Js = "http://cdn.sobekrepository.org/includes/jquery-qtip/2.2.0/jquery.qtip.min.js";
            Jquery_Timeentry_Css = "http://cdn.sobekrepository.org/includes/timeentry/1.5.2/jquery.timeentry.css";
            Jquery_Timeentry_Js = "http://cdn.sobekrepository.org/includes/timeentry/1.5.2/jquery.timeentry.min.js";
            Jquery_Timers_Js = "http://cdn.sobekrepository.org/includes/jquery-timers/1.2/jquery.timers.min.js";
            Jquery_Uploadifive_Js = "http://cdn.sobekrepository.org/includes/uploadifive/1.1.2/jquery.uploadifive.min.js";
            Jquery_Uploadify_Js = "http://cdn.sobekrepository.org/includes/uploadify/3.2.1/jquery.uploadify.min.js";
            Jquery_1_10_2_Js = "http://cdn.sobekrepository.org/includes/jquery/1.10.2/jquery-1.10.2.min.js";
            Jquery_1_2_6_Min_Js = "http://cdn.sobekrepository.org/includes/bookturner/1.0.0/jquery-1.2.6.min.js";
            Jquery_Json_2_4_Js = "http://cdn.sobekrepository.org/includes/jquery-json/2.4/jquery-json-2.4.min.js";
            Jquery_Knob_Js = "http://cdn.sobekrepository.org/includes/jquery-knob/1.2.0/jquery-knob.js";
            Jquery_Migrate_1_1_1_Js = "http://cdn.sobekrepository.org/includes/jquery-migrate/1.1.1/jquery-migrate-1.1.1.min.js";
            Jquery_Rotate_Js = "http://cdn.sobekrepository.org/includes/jquery-rotate/2.2/jquery-rotate.js";
            Jquery_Ui_1_10_1_Js = "http://cdn.sobekrepository.org/includes/jquery-ui/1.10.1/jquery-ui-1.10.1.js";
            Jquery_Ui_1_10_3_Custom_Js = "http://cdn.sobekrepository.org/includes/jquery-ui/1.10.3/jquery-ui-1.10.3.custom.min.js";
            Jquery_Ui_1_10_3_Draggable_Js = "http://cdn.sobekrepository.org/includes/jquery-ui-draggable/1.10.3/jquery-ui-1.10.3.draggable.min.js";
            Jsdatepick_Full_1_3_Js = "http://cdn.sobekrepository.org/includes/datepicker/1.3/jsDatePick.full.1.3.js";
            Jsdatepick_Min_1_3_Js = "http://cdn.sobekrepository.org/includes/datepicker/1.3/jsDatePick.min.1.3.js";
            Jsdatepick_Ltr_Css = "http://cdn.sobekrepository.org/includes/datepicker/1.3/jsDatePick_ltr.css";
            Jstree_Css = "http://cdn.sobekrepository.org/includes/jstree/3.0.9/themes/default/style.min.css";
            Jstree_Js = "http://cdn.sobekrepository.org/includes/jstree/3.0.9/jstree.min.js";
            Keydragzoom_Packed_Js = "http://cdn.sobekrepository.org/includes/keydragzoom/1.0/keydragzoom_packed.js";
            Last2_Png = "http://cdn.sobekrepository.org/images/bookturner/last2.png";
            Leftarrow_Png = "http://cdn.sobekrepository.org/images/misc/leftarrow.png";
            Legend_Nonselected_Polygon_Png = "http://cdn.sobekrepository.org/images/misc/legend_nonselected_polygon.png";
            Legend_Point_Interest_Png = "http://cdn.sobekrepository.org/images/misc/legend_point_interest.png";
            Legend_Red_Pushpin_Png = "http://cdn.sobekrepository.org/images/misc/legend_red_pushpin.png";
            Legend_Search_Area_Png = "http://cdn.sobekrepository.org/images/misc/legend_search_area.png";
            Legend_Selected_Polygon_Png = "http://cdn.sobekrepository.org/images/misc/legend_selected_polygon.png";
            Main_Information_Ico = "http://cdn.sobekrepository.org/images/qc/Main_Information.ICO";
            Map_Drag_Hand_Gif = "http://cdn.sobekrepository.org/images/misc/map_drag_hand.gif";
            Map_Point_Gif = "http://cdn.sobekrepository.org/images/misc/map_point.gif";
            Map_Point_Png = "http://cdn.sobekrepository.org/images/misc/map_point.png";
            Map_Polygon2_Gif = "http://cdn.sobekrepository.org/images/misc/map_polygon2.gif";
            Map_Rectangle2_Gif = "http://cdn.sobekrepository.org/images/misc/map_rectangle2.gif";
            Mapedit_Html = "http://cdn.sobekrepository.org/images/misc/mapedit.html";
            Mapsearch_Html = "http://cdn.sobekrepository.org/images/misc/mapsearch.html";
            Mass_Update_Png = "http://cdn.sobekrepository.org/images/misc/mass_update.png";
            Mass_Update_Icon_Png = "http://cdn.sobekrepository.org/images/misc/mass_update_icon.png";
            Minussign_Png = "http://cdn.sobekrepository.org/images/misc/minussign.png";
            Missingimage_Jpg = "http://cdn.sobekrepository.org/images/misc/MissingImage.jpg";
            Move_Pages_Cursor_Cur = "http://cdn.sobekrepository.org/images/qc/move_pages_cursor.cur";
            New_Element_Jpg = "http://cdn.sobekrepository.org/images/misc/new_element.jpg";
            New_Element_Demo_Jpg = "http://cdn.sobekrepository.org/images/misc/new_element_demo.jpg";
            New_Folder_Jpg = "http://cdn.sobekrepository.org/images/misc/new_folder.jpg";
            New_Item_Gif = "http://cdn.sobekrepository.org/images/misc/new_item.gif";
            Next_Png = "http://cdn.sobekrepository.org/images/bookturner/next.png";
            Next2_Png = "http://cdn.sobekrepository.org/images/bookturner/next2.png";
            No_Pages_Jpg = "http://cdn.sobekrepository.org/images/qc/no_pages.jpg";
            Nocheckmark_Png = "http://cdn.sobekrepository.org/images/misc/nocheckmark.png";
            Nothumb_Jpg = "http://cdn.sobekrepository.org/images/misc/NoThumb.jpg";
            Open_Folder_Jpg = "http://cdn.sobekrepository.org/images/misc/open_folder.jpg";
            Open_Folder_Public_Jpg = "http://cdn.sobekrepository.org/images/misc/open_folder_public.jpg";
            Pagenumbg_Gif = "http://cdn.sobekrepository.org/images/bookturner/pageNumBg.gif";
            Plussign_Png = "http://cdn.sobekrepository.org/images/misc/plussign.png";
            Pmets_Gif = "http://cdn.sobekrepository.org/images/misc/pmets.gif";
            Point02_Ico = "http://cdn.sobekrepository.org/images/qc/POINT02.ICO";
            Point04_Ico = "http://cdn.sobekrepository.org/images/qc/POINT04.ICO";
            Point13_Ico = "http://cdn.sobekrepository.org/images/qc/POINT13.ICO";
            Pointer_Blue_Gif = "http://cdn.sobekrepository.org/images/misc/pointer_blue.gif";
            Portals_Gif = "http://cdn.sobekrepository.org/images/misc/portals.gif";
            Portals_Png = "http://cdn.sobekrepository.org/images/misc/portals.png";
            Previous2_Png = "http://cdn.sobekrepository.org/images/bookturner/previous2.png";
            Print_Css = "http://cdn.sobekrepository.org/css/sobekcm-print/4.8.4/print.css";
            Printer_Png = "http://cdn.sobekrepository.org/images/misc/printer.png";
            Private_Items_Png = "http://cdn.sobekrepository.org/images/misc/private_items.png";
            Private_Resource_Png = "http://cdn.sobekrepository.org/images/misc/private_resource.png";
            Private_Resource_Icon_Png = "http://cdn.sobekrepository.org/images/misc/private_resource_icon.png";
            Public_Resource_Png = "http://cdn.sobekrepository.org/images/misc/public_resource.png";
            Public_Resource_Icon_Png = "http://cdn.sobekrepository.org/images/misc/public_resource_icon.png";
            Qc_Html = "http://cdn.sobekrepository.org/images/misc/qc.html";
            Qc_Addfiles_Png = "http://cdn.sobekrepository.org/images/qc/qc_addfiles.png";
            Qc_Button_Png = "http://cdn.sobekrepository.org/images/misc/qc_button.png";
            Qc_Button_Icon_Png = "http://cdn.sobekrepository.org/images/misc/qc_button_icon.png";
            Rect_Large_Ico = "http://cdn.sobekrepository.org/images/qc/rect_large.ico";
            Rect_Medium_Ico = "http://cdn.sobekrepository.org/images/qc/rect_medium.ico";
            Rect_Small_Ico = "http://cdn.sobekrepository.org/images/qc/rect_small.ico";
            Red_Pushpin_Png = "http://cdn.sobekrepository.org/images/mapedit/mapIcons/red-pushpin.png";
            Refresh_Gif = "http://cdn.sobekrepository.org/images/misc/refresh.gif";
            Refresh_Png = "http://cdn.sobekrepository.org/images/misc/refresh.png";
            Refresh_Folder_Jpg = "http://cdn.sobekrepository.org/images/misc/refresh_folder.jpg";
            Removeicon_Gif = "http://cdn.sobekrepository.org/images/mapsearch/removeIcon.gif";
            Restricted_Resource_Png = "http://cdn.sobekrepository.org/images/misc/restricted_resource.png";
            Restricted_Resource_Icon_Png = "http://cdn.sobekrepository.org/images/misc/restricted_resource_icon.png";
            Return_Gif = "http://cdn.sobekrepository.org/images/misc/return.gif";
            Return_Png = "http://cdn.sobekrepository.org/images/bookturner/return.png";
            Rotation_Clockwise_Png = "http://cdn.sobekrepository.org/images/mapedit/rotation-clockwise.png";
            Rotation_Counterclockwise_Png = "http://cdn.sobekrepository.org/images/mapedit/rotation-counterClockwise.png";
            Rotation_Reset_Png = "http://cdn.sobekrepository.org/images/mapedit/rotation-reset.png";
            Save_Ico = "http://cdn.sobekrepository.org/images/qc/Save.ico";
            Saved_Searches_Gif = "http://cdn.sobekrepository.org/images/misc/saved_searches.gif";
            Saved_Searches_Big_Gif = "http://cdn.sobekrepository.org/images/misc/saved_searches_big.gif";
            Search_Png = "http://cdn.sobekrepository.org/images/mapedit/search.png";
            Settings_Gif = "http://cdn.sobekrepository.org/images/misc/Settings.gif";
            Show_Internal_Header_Png = "http://cdn.sobekrepository.org/images/misc/show_internal_header.png";
            Skins_Gif = "http://cdn.sobekrepository.org/images/misc/skins.gif";
            Skins_Png = "http://cdn.sobekrepository.org/images/misc/skins.png";
            Sobekcm_Css = "http://cdn.sobekrepository.org/css/sobekcm/4.8.4/SobekCM.min.css";
            Sobekcm_Admin_Css = "http://cdn.sobekrepository.org/css/sobekcm-admin/4.8.4/SobekCM_Admin.min.css";
            Sobekcm_Admin_Js = "http://cdn.sobekrepository.org/js/sobekcm-admin/4.8.4/sobekcm_admin.js";
            Sobekcm_Bookturner_Css = "http://cdn.sobekrepository.org/css/sobekcm-bookturner/4.8.4/SobekCM_BookTurner.css";
            Sobekcm_Datatables_Css = "http://cdn.sobekrepository.org/css/sobekcm-datatables/4.8.4/SobekCM_DataTables.css";
            Sobekcm_Full_Js = "http://cdn.sobekrepository.org/js/sobekcm-full/4.8.4/sobekcm_full.min.js";
            Sobekcm_Item_Css = "http://cdn.sobekrepository.org/css/sobekcm-item/4.8.4/SobekCM_Item.min.css";
            Sobekcm_Map_Search_Js = "http://cdn.sobekrepository.org/js/sobekcm-map/4.8.4/sobekcm_map_search.js";
            Sobekcm_Map_Tool_Js = "http://cdn.sobekrepository.org/js/sobekcm-map/4.8.4/sobekcm_map_tool.js";
            Sobekcm_Mapeditor_Css = "http://cdn.sobekrepository.org/css/sobekcm-map/4.8.4/SobekCM_MapEditor.css";
            Sobekcm_Mapsearch_Css = "http://cdn.sobekrepository.org/css/sobekcm-map/4.8.4/SobekCM_MapSearch.css";
            Sobekcm_Metadata_Css = "http://cdn.sobekrepository.org/css/sobekcm-metadata/4.8.4/SobekCM_Metadata.min.css";
            Sobekcm_Metadata_Js = "http://cdn.sobekrepository.org/js/sobekcm-metadata/4.8.4/sobekcm_metadata.js";
            Sobekcm_Mysobek_Css = "http://cdn.sobekrepository.org/css/sobekcm-mysobek/4.8.4/SobekCM_MySobek.min.css";
            Sobekcm_Print_Css = "http://cdn.sobekrepository.org/css/sobekcm-print/4.8.4/SobekCM_Print.css";
            Sobekcm_Qc_Css = "http://cdn.sobekrepository.org/css/sobekcm-qc/4.8.4/SobekCM_QC.css";
            Sobekcm_Qc_Js = "http://cdn.sobekrepository.org/js/sobekcm-qc/4.8.4/sobekcm_qc.js";
            Sobekcm_Stats_Css = "http://cdn.sobekrepository.org/css/sobekcm-stats/4.8.4/SobekCM_Stats.css";
            Sobekcm_Thumb_Results_Js = "http://cdn.sobekrepository.org/js/sobekcm-thumb-results/4.8.4/sobekcm_thumb_results.js";
            Sobekcm_Track_Item_Js = "http://cdn.sobekrepository.org/js/sobekcm-track-item/4.8.4/sobekcm_track_item.js";
            Sobekcm_Trackingsheet_Css = "http://cdn.sobekrepository.org/css/sobekcm-tracking/4.8.4/SobekCM_TrackingSheet.css";
            Source_Html = "http://cdn.sobekrepository.org/images/misc/source.html";
            Spinner_Gif = "http://cdn.sobekrepository.org/images/misc/spinner.gif";
            Spinner_Gray_Gif = "http://cdn.sobekrepository.org/images/misc/spinner_gray.gif";
            Stumbleupon_Share_Gif = "http://cdn.sobekrepository.org/images/misc/stumbleupon_share.gif";
            Stumbleupon_Share_H_Gif = "http://cdn.sobekrepository.org/images/misc/stumbleupon_share_h.gif";
            Submitted_Items_Gif = "http://cdn.sobekrepository.org/images/misc/submitted_items.gif";
            Table_Blue_Png = "http://cdn.sobekrepository.org/images/mapsearch/table_blue.png";
            Thumb_Blue_Png = "http://cdn.sobekrepository.org/images/mapsearch/thumb_blue.png";
            Thumbnail_Cursor_Cur = "http://cdn.sobekrepository.org/images/qc/thumbnail_cursor.cur";
            Thumbnail_Large_Gif = "http://cdn.sobekrepository.org/images/misc/thumbnail_large.gif";
            Thumbs1_Gif = "http://cdn.sobekrepository.org/images/misc/thumbs1.gif";
            Thumbs1_Selected_Gif = "http://cdn.sobekrepository.org/images/misc/thumbs1_selected.gif";
            Thumbs2_Gif = "http://cdn.sobekrepository.org/images/misc/thumbs2.gif";
            Thumbs2_Selected_Gif = "http://cdn.sobekrepository.org/images/misc/thumbs2_selected.gif";
            Thumbs3_Gif = "http://cdn.sobekrepository.org/images/misc/thumbs3.gif";
            Thumbs3_Selected_Gif = "http://cdn.sobekrepository.org/images/misc/thumbs3_selected.gif";
            Toolbar_Toggle_Png = "http://cdn.sobekrepository.org/images/mapedit/toolbar-toggle.png";
            Toolbox_Close2_Png = "http://cdn.sobekrepository.org/images/mapedit/toolbox-close2.png";
            Toolbox_Icon_Png = "http://cdn.sobekrepository.org/images/mapedit/toolbox-icon.png";
            Toolbox_Maximize2_Png = "http://cdn.sobekrepository.org/images/mapedit/toolbox-maximize2.png";
            Toolbox_Minimize2_Png = "http://cdn.sobekrepository.org/images/mapedit/toolbox-minimize2.png";
            Top_Left_Jpg = "http://cdn.sobekrepository.org/images/bookturner/top_left.jpg";
            Top_Right_Jpg = "http://cdn.sobekrepository.org/images/bookturner/top_right.jpg";
            Track2_Gif = "http://cdn.sobekrepository.org/images/misc/track2.gif";
            Trash01_Ico = "http://cdn.sobekrepository.org/images/qc/TRASH01.ICO";
            Twitter_Share_Gif = "http://cdn.sobekrepository.org/images/misc/twitter_share.gif";
            Twitter_Share_H_Gif = "http://cdn.sobekrepository.org/images/misc/twitter_share_h.gif";
            Ufdc_Banner_700_Jpg = "http://cdn.sobekrepository.org/images/misc/ufdc_banner_700.jpg";
            Ui_Icons_Ffffff_256X240_Png = "http://cdn.sobekrepository.org/images/mapsearch/ui-icons_ffffff_256x240.png";
            Uploadifive_Css = "http://cdn.sobekrepository.org/includes/uploadifive/1.1.2/uploadifive.css";
            Uploadify_Css = "http://cdn.sobekrepository.org/includes/uploadify/3.2.1/uploadify.css";
            Uploadify_Swf = "http://cdn.sobekrepository.org/includes/uploadify/3.2.1/uploadify.swf";
            Usage_Png = "http://cdn.sobekrepository.org/images/misc/usage.png";
            Usage_Statistics_Png = "http://cdn.sobekrepository.org/images/misc/usage_statistics.png";
            Users_Gif = "http://cdn.sobekrepository.org/images/misc/Users.gif";
            Users_Png = "http://cdn.sobekrepository.org/images/misc/Users.png";
            View_Ico = "http://cdn.sobekrepository.org/images/qc/View.ico";
            View_Work_Log_Png = "http://cdn.sobekrepository.org/images/misc/view_work_log.png";
            View_Work_Log_Icon_Png = "http://cdn.sobekrepository.org/images/misc/view_work_log_icon.png";
            Wizard_Png = "http://cdn.sobekrepository.org/images/misc/wizard.png";
            Wordmarks_Gif = "http://cdn.sobekrepository.org/images/misc/wordmarks.gif";
            Wrench_Png = "http://cdn.sobekrepository.org/images/misc/wrench.png";
            Yahoo_Share_Gif = "http://cdn.sobekrepository.org/images/misc/yahoo_share.gif";
            Yahoo_Share_H_Gif = "http://cdn.sobekrepository.org/images/misc/yahoo_share_h.gif";
            Yahoobuzz_Share_Gif = "http://cdn.sobekrepository.org/images/misc/yahoobuzz_share.gif";
            Yahoobuzz_Share_H_Gif = "http://cdn.sobekrepository.org/images/misc/yahoobuzz_share_h.gif";
            Zoom_Tool_Cur = "http://cdn.sobekrepository.org/images/misc/zoom_tool.cur";
            Zoomin_Png = "http://cdn.sobekrepository.org/images/bookturner/zoomin.png";
            Zoomout_Png = "http://cdn.sobekrepository.org/images/bookturner/zoomout.png";
        }
        /// <summary> URL for the default resource '16px-feed-icon.svg.png' file ( http://cdn.sobekrepository.org/images/misc/16px-Feed-icon.svg.png by default)</summary>
        public static string Sixteen_Px_Feed_Icon_Svg_Png { get; private set; }

        /// <summary> URL for the default resource 'add_geospatial_icon.png' file ( http://cdn.sobekrepository.org/images/misc/add_geospatial_icon.png by default)</summary>
        public static string Add_Geospatial_Icon_Png { get; private set; }

        /// <summary> URL for the default resource 'add_volume.png' file ( http://cdn.sobekrepository.org/images/misc/add_volume.png by default)</summary>
        public static string Add_Volume_Png { get; private set; }

        /// <summary> URL for the default resource 'add_volume_icon.png' file ( http://cdn.sobekrepository.org/images/misc/add_volume_icon.png by default)</summary>
        public static string Add_Volume_Icon_Png { get; private set; }

        /// <summary> URL for the default resource 'admin_view.png' file ( http://cdn.sobekrepository.org/images/misc/admin_view.png by default)</summary>
        public static string Admin_View_Png { get; private set; }

        /// <summary> URL for the default resource 'ajax-loader.gif' file ( http://cdn.sobekrepository.org/images/mapedit/ajax-loader.gif by default)</summary>
        public static string Ajax_Loader_Gif { get; private set; }

        /// <summary> URL for the default resource 'arw05lt.gif' file ( http://cdn.sobekrepository.org/images/qc/ARW05LT.gif by default)</summary>
        public static string Arw05lt_Gif { get; private set; }

        /// <summary> URL for the default resource 'arw05rt.gif' file ( http://cdn.sobekrepository.org/images/qc/ARW05RT.gif by default)</summary>
        public static string Arw05rt_Gif { get; private set; }

        /// <summary> URL for the default resource 'autofill_volumes.png' file ( http://cdn.sobekrepository.org/images/misc/autofill_volumes.png by default)</summary>
        public static string Autofill_Volumes_Png { get; private set; }

        /// <summary> URL for the default resource 'bg1.png' file ( http://cdn.sobekrepository.org/images/mapedit/bg1.png by default)</summary>
        public static string Bg1_Png { get; private set; }

        /// <summary> URL for the default resource 'big_bookshelf.gif' file ( http://cdn.sobekrepository.org/images/misc/big_bookshelf.gif by default)</summary>
        public static string Big_Bookshelf_Gif { get; private set; }

        /// <summary> URL for the default resource 'blue.png' file ( http://cdn.sobekrepository.org/images/mapedit/mapIcons/blue.png by default)</summary>
        public static string Blue_Png { get; private set; }

        /// <summary> URL for the default resource 'blue-pin.png' file ( http://cdn.sobekrepository.org/images/mapsearch/blue-pin.png by default)</summary>
        public static string Blue_Pin_Png { get; private set; }

        /// <summary> URL for the default resource 'bookshelf.jpg' file ( http://cdn.sobekrepository.org/images/misc/bookshelf.jpg by default)</summary>
        public static string Bookshelf_Jpg { get; private set; }

        /// <summary> URL for the default resource 'bookshelf.png' file ( http://cdn.sobekrepository.org/images/misc/bookshelf.png by default)</summary>
        public static string Bookshelf_Png { get; private set; }

        /// <summary> URL for the default resource 'bookturner.html' file ( http://cdn.sobekrepository.org/images/misc/bookturner.html by default)</summary>
        public static string Bookturner_Html { get; private set; }

        /// <summary> URL for the default resource 'bookturner.js' file ( http://cdn.sobekrepository.org/includes/bookturner/1.0.0/bookturner.js by default)</summary>
        public static string Bookturner_Js { get; private set; }

        /// <summary> URL for the default resource 'brief_blue.png' file ( http://cdn.sobekrepository.org/images/mapsearch/brief_blue.png by default)</summary>
        public static string Brief_Blue_Png { get; private set; }

        /// <summary> URL for the default resource 'building.gif' file ( http://cdn.sobekrepository.org/images/misc/building.gif by default)</summary>
        public static string Building_Gif { get; private set; }

        /// <summary> URL for the default resource 'button_down_arrow.png' file ( http://cdn.sobekrepository.org/images/misc/button_down_arrow.png by default)</summary>
        public static string Button_Down_Arrow_Png { get; private set; }

        /// <summary> URL for the default resource 'button_first_arrow.png' file ( http://cdn.sobekrepository.org/images/misc/button_first_arrow.png by default)</summary>
        public static string Button_First_Arrow_Png { get; private set; }

        /// <summary> URL for the default resource 'button_last_arrow.png' file ( http://cdn.sobekrepository.org/images/misc/button_last_arrow.png by default)</summary>
        public static string Button_Last_Arrow_Png { get; private set; }

        /// <summary> URL for the default resource 'button_next_arrow.png' file ( http://cdn.sobekrepository.org/images/misc/button_next_arrow.png by default)</summary>
        public static string Button_Next_Arrow_Png { get; private set; }

        /// <summary> URL for the default resource 'button_next_arrow2.png' file ( http://cdn.sobekrepository.org/images/misc/button_next_arrow2.png by default)</summary>
        public static string Button_Next_Arrow2_Png { get; private set; }

        /// <summary> URL for the default resource 'button_previous_arrow.png' file ( http://cdn.sobekrepository.org/images/misc/button_previous_arrow.png by default)</summary>
        public static string Button_Previous_Arrow_Png { get; private set; }

        /// <summary> URL for the default resource 'button_up_arrow.png' file ( http://cdn.sobekrepository.org/images/misc/button_up_arrow.png by default)</summary>
        public static string Button_Up_Arrow_Png { get; private set; }

        /// <summary> URL for the default resource 'button-action1.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-action1.png by default)</summary>
        public static string Button_Action1_Png { get; private set; }

        /// <summary> URL for the default resource 'button-action2.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-action2.png by default)</summary>
        public static string Button_Action2_Png { get; private set; }

        /// <summary> URL for the default resource 'button-action3.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-action3.png by default)</summary>
        public static string Button_Action3_Png { get; private set; }

        /// <summary> URL for the default resource 'button-base.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-base.png by default)</summary>
        public static string Button_Base_Png { get; private set; }

        /// <summary> URL for the default resource 'button-blocklot.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-blockLot.png by default)</summary>
        public static string Button_Blocklot_Png { get; private set; }

        /// <summary> URL for the default resource 'button-cancel.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-cancel.png by default)</summary>
        public static string Button_Cancel_Png { get; private set; }

        /// <summary> URL for the default resource 'button-controls.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-controls.png by default)</summary>
        public static string Button_Controls_Png { get; private set; }

        /// <summary> URL for the default resource 'button-converttooverlay.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-convertToOverlay.png by default)</summary>
        public static string Button_Converttooverlay_Png { get; private set; }

        /// <summary> URL for the default resource 'button-drawcircle.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-drawCircle.png by default)</summary>
        public static string Button_Drawcircle_Png { get; private set; }

        /// <summary> URL for the default resource 'button-drawline.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-drawLine.png by default)</summary>
        public static string Button_Drawline_Png { get; private set; }

        /// <summary> URL for the default resource 'button-drawmarker.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-drawMarker.png by default)</summary>
        public static string Button_Drawmarker_Png { get; private set; }

        /// <summary> URL for the default resource 'button-drawpolygon.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-drawPolygon.png by default)</summary>
        public static string Button_Drawpolygon_Png { get; private set; }

        /// <summary> URL for the default resource 'button-drawrectangle.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-drawRectangle.png by default)</summary>
        public static string Button_Drawrectangle_Png { get; private set; }

        /// <summary> URL for the default resource 'button-hybrid.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-hybrid.png by default)</summary>
        public static string Button_Hybrid_Png { get; private set; }

        /// <summary> URL for the default resource 'button-itemgetuserlocation.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-itemGetUserLocation.png by default)</summary>
        public static string Button_Itemgetuserlocation_Png { get; private set; }

        /// <summary> URL for the default resource 'button-itemplace.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-itemPlace.png by default)</summary>
        public static string Button_Itemplace_Png { get; private set; }

        /// <summary> URL for the default resource 'button-itemreset.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-itemReset.png by default)</summary>
        public static string Button_Itemreset_Png { get; private set; }

        /// <summary> URL for the default resource 'button-layercustom.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-layerCustom.png by default)</summary>
        public static string Button_Layercustom_Png { get; private set; }

        /// <summary> URL for the default resource 'button-layerhybrid.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-layerHybrid.png by default)</summary>
        public static string Button_Layerhybrid_Png { get; private set; }

        /// <summary> URL for the default resource 'button-layerreset.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-layerReset.png by default)</summary>
        public static string Button_Layerreset_Png { get; private set; }

        /// <summary> URL for the default resource 'button-layerroadmap.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-layerRoadmap.png by default)</summary>
        public static string Button_Layerroadmap_Png { get; private set; }

        /// <summary> URL for the default resource 'button-layersatellite.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-layerSatellite.png by default)</summary>
        public static string Button_Layersatellite_Png { get; private set; }

        /// <summary> URL for the default resource 'button-layerterrain.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-layerTerrain.png by default)</summary>
        public static string Button_Layerterrain_Png { get; private set; }

        /// <summary> URL for the default resource 'button-manageitem.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-manageItem.png by default)</summary>
        public static string Button_Manageitem_Png { get; private set; }

        /// <summary> URL for the default resource 'button-manageoverlay.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-manageOverlay.png by default)</summary>
        public static string Button_Manageoverlay_Png { get; private set; }

        /// <summary> URL for the default resource 'button-managepoi.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-managePOI.png by default)</summary>
        public static string Button_Managepoi_Png { get; private set; }

        /// <summary> URL for the default resource 'button-overlayedit.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-overlayEdit.png by default)</summary>
        public static string Button_Overlayedit_Png { get; private set; }

        /// <summary> URL for the default resource 'button-overlaygetuserlocation.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-overlayGetUserLocation.png by default)</summary>
        public static string Button_Overlaygetuserlocation_Png { get; private set; }

        /// <summary> URL for the default resource 'button-overlayplace.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-overlayPlace.png by default)</summary>
        public static string Button_Overlayplace_Png { get; private set; }

        /// <summary> URL for the default resource 'button-overlayreset.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-overlayReset.png by default)</summary>
        public static string Button_Overlayreset_Png { get; private set; }

        /// <summary> URL for the default resource 'button-overlayrotate.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-overlayRotate.png by default)</summary>
        public static string Button_Overlayrotate_Png { get; private set; }

        /// <summary> URL for the default resource 'button-overlaytoggle.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-overlayToggle.png by default)</summary>
        public static string Button_Overlaytoggle_Png { get; private set; }

        /// <summary> URL for the default resource 'button-overlaytransparency.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-overlayTransparency.png by default)</summary>
        public static string Button_Overlaytransparency_Png { get; private set; }

        /// <summary> URL for the default resource 'button-pandown.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-panDown.png by default)</summary>
        public static string Button_Pandown_Png { get; private set; }

        /// <summary> URL for the default resource 'button-panleft.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-panLeft.png by default)</summary>
        public static string Button_Panleft_Png { get; private set; }

        /// <summary> URL for the default resource 'button-panreset.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-panReset.png by default)</summary>
        public static string Button_Panreset_Png { get; private set; }

        /// <summary> URL for the default resource 'button-panright.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-panRight.png by default)</summary>
        public static string Button_Panright_Png { get; private set; }

        /// <summary> URL for the default resource 'button-panup.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-panUp.png by default)</summary>
        public static string Button_Panup_Png { get; private set; }

        /// <summary> URL for the default resource 'button-poicircle.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-poiCircle.png by default)</summary>
        public static string Button_Poicircle_Png { get; private set; }

        /// <summary> URL for the default resource 'button-poiedit.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-poiEdit.png by default)</summary>
        public static string Button_Poiedit_Png { get; private set; }

        /// <summary> URL for the default resource 'button-poigetuserlocation.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-poiGetUserLocation.png by default)</summary>
        public static string Button_Poigetuserlocation_Png { get; private set; }

        /// <summary> URL for the default resource 'button-poiline.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-poiLine.png by default)</summary>
        public static string Button_Poiline_Png { get; private set; }

        /// <summary> URL for the default resource 'button-poimarker.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-poiMarker.png by default)</summary>
        public static string Button_Poimarker_Png { get; private set; }

        /// <summary> URL for the default resource 'button-poiplace.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-poiPlace.png by default)</summary>
        public static string Button_Poiplace_Png { get; private set; }

        /// <summary> URL for the default resource 'button-poipolygon.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-poiPolygon.png by default)</summary>
        public static string Button_Poipolygon_Png { get; private set; }

        /// <summary> URL for the default resource 'button-poirectangle.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-poiRectangle.png by default)</summary>
        public static string Button_Poirectangle_Png { get; private set; }

        /// <summary> URL for the default resource 'button-poireset.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-poiReset.png by default)</summary>
        public static string Button_Poireset_Png { get; private set; }

        /// <summary> URL for the default resource 'button-poitoggle.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-poiToggle.png by default)</summary>
        public static string Button_Poitoggle_Png { get; private set; }

        /// <summary> URL for the default resource 'button-reset.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-reset.png by default)</summary>
        public static string Button_Reset_Png { get; private set; }

        /// <summary> URL for the default resource 'button-roadmap.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-roadmap.png by default)</summary>
        public static string Button_Roadmap_Png { get; private set; }

        /// <summary> URL for the default resource 'button-satellite.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-satellite.png by default)</summary>
        public static string Button_Satellite_Png { get; private set; }

        /// <summary> URL for the default resource 'button-save.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-save.png by default)</summary>
        public static string Button_Save_Png { get; private set; }

        /// <summary> URL for the default resource 'button-search.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-search.png by default)</summary>
        public static string Button_Search_Png { get; private set; }

        /// <summary> URL for the default resource 'button-terrain.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-terrain.png by default)</summary>
        public static string Button_Terrain_Png { get; private set; }

        /// <summary> URL for the default resource 'button-togglemapcontrols.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-toggleMapControls.png by default)</summary>
        public static string Button_Togglemapcontrols_Png { get; private set; }

        /// <summary> URL for the default resource 'button-toggletoolbar.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-toggleToolbar.png by default)</summary>
        public static string Button_Toggletoolbar_Png { get; private set; }

        /// <summary> URL for the default resource 'button-toggletoolbox.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-toggleToolbox.png by default)</summary>
        public static string Button_Toggletoolbox_Png { get; private set; }

        /// <summary> URL for the default resource 'button-toolbox.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-toolbox.png by default)</summary>
        public static string Button_Toolbox_Png { get; private set; }

        /// <summary> URL for the default resource 'button-usesearchaslocation.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-useSearchAsLocation.png by default)</summary>
        public static string Button_Usesearchaslocation_Png { get; private set; }

        /// <summary> URL for the default resource 'button-zoomin.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-zoomIn.png by default)</summary>
        public static string Button_Zoomin_Png { get; private set; }

        /// <summary> URL for the default resource 'button-zoomout.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-zoomOut.png by default)</summary>
        public static string Button_Zoomout_Png { get; private set; }

        /// <summary> URL for the default resource 'button-zoomreset.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-zoomReset.png by default)</summary>
        public static string Button_Zoomreset_Png { get; private set; }

        /// <summary> URL for the default resource 'button-zoomreset2.png' file ( http://cdn.sobekrepository.org/images/mapedit/button-zoomReset2.png by default)</summary>
        public static string Button_Zoomreset2_Png { get; private set; }

        /// <summary> URL for the default resource 'calendar_button.png' file ( http://cdn.sobekrepository.org/images/misc/calendar_button.png by default)</summary>
        public static string Calendar_Button_Png { get; private set; }

        /// <summary> URL for the default resource 'cancel.ico' file ( http://cdn.sobekrepository.org/images/qc/Cancel.ico by default)</summary>
        public static string Cancel_Ico { get; private set; }

        /// <summary> URL for the default resource 'cc_by.png' file ( http://cdn.sobekrepository.org/images/misc/cc_by.png by default)</summary>
        public static string Cc_By_Png { get; private set; }

        /// <summary> URL for the default resource 'cc_by_nc.png' file ( http://cdn.sobekrepository.org/images/misc/cc_by_nc.png by default)</summary>
        public static string Cc_By_Nc_Png { get; private set; }

        /// <summary> URL for the default resource 'cc_by_nc_nd.png' file ( http://cdn.sobekrepository.org/images/misc/cc_by_nc_nd.png by default)</summary>
        public static string Cc_By_Nc_Nd_Png { get; private set; }

        /// <summary> URL for the default resource 'cc_by_nc_sa.png' file ( http://cdn.sobekrepository.org/images/misc/cc_by_nc_sa.png by default)</summary>
        public static string Cc_By_Nc_Sa_Png { get; private set; }

        /// <summary> URL for the default resource 'cc_by_nd.png' file ( http://cdn.sobekrepository.org/images/misc/cc_by_nd.png by default)</summary>
        public static string Cc_By_Nd_Png { get; private set; }

        /// <summary> URL for the default resource 'cc_by_sa.png' file ( http://cdn.sobekrepository.org/images/misc/cc_by_sa.png by default)</summary>
        public static string Cc_By_Sa_Png { get; private set; }

        /// <summary> URL for the default resource 'cc_zero.png' file ( http://cdn.sobekrepository.org/images/misc/cc_zero.png by default)</summary>
        public static string Cc_Zero_Png { get; private set; }

        /// <summary> URL for the default resource 'chart.js' file ( http://cdn.sobekrepository.org/includes/chartjs/1.0.2/Chart.min.js by default)</summary>
        public static string Chart_Js { get; private set; }

        /// <summary> URL for the default resource 'chat.png' file ( http://cdn.sobekrepository.org/images/misc/chat.png by default)</summary>
        public static string Chat_Png { get; private set; }

        /// <summary> URL for the default resource 'checkmark.png' file ( http://cdn.sobekrepository.org/images/misc/checkmark.png by default)</summary>
        public static string Checkmark_Png { get; private set; }

        /// <summary> URL for the default resource 'checkmark2.png' file ( http://cdn.sobekrepository.org/images/misc/checkmark2.png by default)</summary>
        public static string Checkmark2_Png { get; private set; }

        /// <summary> URL for the default resource 'ckeditor.js' file ( http://cdn.sobekrepository.org/includes/ckeditor/4.4.7/ckeditor.js by default)</summary>
        public static string Ckeditor_Js { get; private set; }

        /// <summary> URL for the default resource 'closed_folder.jpg' file ( http://cdn.sobekrepository.org/images/misc/closed_folder.jpg by default)</summary>
        public static string Closed_Folder_Jpg { get; private set; }

        /// <summary> URL for the default resource 'closed_folder_public.jpg' file ( http://cdn.sobekrepository.org/images/misc/closed_folder_public.jpg by default)</summary>
        public static string Closed_Folder_Public_Jpg { get; private set; }

        /// <summary> URL for the default resource 'closed_folder_public_big.jpg' file ( http://cdn.sobekrepository.org/images/misc/closed_folder_public_big.jpg by default)</summary>
        public static string Closed_Folder_Public_Big_Jpg { get; private set; }

        /// <summary> URL for the default resource 'contentslider.js' file ( http://cdn.sobekrepository.org/includes/contentslider/2.4/contentslider.min.js by default)</summary>
        public static string Contentslider_Js { get; private set; }

        /// <summary> URL for the default resource 'dark_resource.png' file ( http://cdn.sobekrepository.org/images/misc/dark_resource.png by default)</summary>
        public static string Dark_Resource_Png { get; private set; }

        /// <summary> URL for the default resource 'default_banner.jpg' file ( http://cdn.sobekrepository.org/images/misc/default_banner.jpg by default)</summary>
        public static string Default_Banner_Jpg { get; private set; }

        /// <summary> URL for the default resource 'default_button.gif' file ( http://cdn.sobekrepository.org/images/misc/default_button.gif by default)</summary>
        public static string Default_Button_Gif { get; private set; }

        /// <summary> URL for the default resource 'default_button.png' file ( http://cdn.sobekrepository.org/images/misc/default_button.png by default)</summary>
        public static string Default_Button_Png { get; private set; }

        /// <summary> URL for the default resource 'delete_cursor.cur' file ( http://cdn.sobekrepository.org/images/qc/delete_cursor.cur by default)</summary>
        public static string Delete_Cursor_Cur { get; private set; }

        /// <summary> URL for the default resource 'delete_item.png' file ( http://cdn.sobekrepository.org/images/misc/delete_item.png by default)</summary>
        public static string Delete_Item_Png { get; private set; }

        /// <summary> URL for the default resource 'delete_item_icon.png' file ( http://cdn.sobekrepository.org/images/misc/delete_item_icon.png by default)</summary>
        public static string Delete_Item_Icon_Png { get; private set; }

        /// <summary> URL for the default resource 'digg_share.gif' file ( http://cdn.sobekrepository.org/images/misc/digg_share.gif by default)</summary>
        public static string Digg_Share_Gif { get; private set; }

        /// <summary> URL for the default resource 'digg_share_h.gif' file ( http://cdn.sobekrepository.org/images/misc/digg_share_h.gif by default)</summary>
        public static string Digg_Share_H_Gif { get; private set; }

        /// <summary> URL for the default resource 'dloc_banner_700.jpg' file ( http://cdn.sobekrepository.org/images/misc/dloc_banner_700.jpg by default)</summary>
        public static string Dloc_Banner_700_Jpg { get; private set; }

        /// <summary> URL for the default resource 'drag1pg.ico' file ( http://cdn.sobekrepository.org/images/qc/DRAG1PG.ICO by default)</summary>
        public static string Drag1pg_Ico { get; private set; }

        /// <summary> URL for the default resource 'edit.gif' file ( http://cdn.sobekrepository.org/images/misc/edit.gif by default)</summary>
        public static string Edit_Gif { get; private set; }

        /// <summary> URL for the default resource 'edit.png' file ( http://cdn.sobekrepository.org/images/mapedit/edit.png by default)</summary>
        public static string Edit_Png { get; private set; }

        /// <summary> URL for the default resource 'edit_behaviors.png' file ( http://cdn.sobekrepository.org/images/misc/edit_behaviors.png by default)</summary>
        public static string Edit_Behaviors_Png { get; private set; }

        /// <summary> URL for the default resource 'edit_behaviors_icon.png' file ( http://cdn.sobekrepository.org/images/misc/edit_behaviors_icon.png by default)</summary>
        public static string Edit_Behaviors_Icon_Png { get; private set; }

        /// <summary> URL for the default resource 'edit_hierarchy.png' file ( http://cdn.sobekrepository.org/images/misc/edit_hierarchy.png by default)</summary>
        public static string Edit_Hierarchy_Png { get; private set; }

        /// <summary> URL for the default resource 'edit_metadata.png' file ( http://cdn.sobekrepository.org/images/misc/edit_metadata.png by default)</summary>
        public static string Edit_Metadata_Png { get; private set; }

        /// <summary> URL for the default resource 'edit_metadata_icon.png' file ( http://cdn.sobekrepository.org/images/misc/edit_metadata_icon.png by default)</summary>
        public static string Edit_Metadata_Icon_Png { get; private set; }

        /// <summary> URL for the default resource 'email.png' file ( http://cdn.sobekrepository.org/images/misc/email.png by default)</summary>
        public static string Email_Png { get; private set; }

        /// <summary> URL for the default resource 'emptypage.jpg' file ( http://cdn.sobekrepository.org/images/bookturner/emptypage.jpg by default)</summary>
        public static string Emptypage_Jpg { get; private set; }

        /// <summary> URL for the default resource 'exit.gif' file ( http://cdn.sobekrepository.org/images/misc/exit.gif by default)</summary>
        public static string Exit_Gif { get; private set; }

        /// <summary> URL for the default resource 'facebook_share.gif' file ( http://cdn.sobekrepository.org/images/misc/facebook_share.gif by default)</summary>
        public static string Facebook_Share_Gif { get; private set; }

        /// <summary> URL for the default resource 'facebook_share_h.gif' file ( http://cdn.sobekrepository.org/images/misc/facebook_share_h.gif by default)</summary>
        public static string Facebook_Share_H_Gif { get; private set; }

        /// <summary> URL for the default resource 'favorites_share.gif' file ( http://cdn.sobekrepository.org/images/misc/favorites_share.gif by default)</summary>
        public static string Favorites_Share_Gif { get; private set; }

        /// <summary> URL for the default resource 'favorites_share_h.gif' file ( http://cdn.sobekrepository.org/images/misc/favorites_share_h.gif by default)</summary>
        public static string Favorites_Share_H_Gif { get; private set; }

        /// <summary> URL for the default resource 'file_management.png' file ( http://cdn.sobekrepository.org/images/misc/file_management.png by default)</summary>
        public static string File_Management_Png { get; private set; }

        /// <summary> URL for the default resource 'file_management_icon.png' file ( http://cdn.sobekrepository.org/images/misc/file_management_icon.png by default)</summary>
        public static string File_Management_Icon_Png { get; private set; }

        /// <summary> URL for the default resource 'firewall.gif' file ( http://cdn.sobekrepository.org/images/misc/firewall.gif by default)</summary>
        public static string Firewall_Gif { get; private set; }

        /// <summary> URL for the default resource 'firewall.png' file ( http://cdn.sobekrepository.org/images/misc/firewall.png by default)</summary>
        public static string Firewall_Png { get; private set; }

        /// <summary> URL for the default resource 'first2.png' file ( http://cdn.sobekrepository.org/images/bookturner/first2.png by default)</summary>
        public static string First2_Png { get; private set; }

        /// <summary> URL for the default resource 'forwarding.gif' file ( http://cdn.sobekrepository.org/images/misc/forwarding.gif by default)</summary>
        public static string Forwarding_Gif { get; private set; }

        /// <summary> URL for the default resource 'forwarding.png' file ( http://cdn.sobekrepository.org/images/misc/forwarding.png by default)</summary>
        public static string Forwarding_Png { get; private set; }

        /// <summary> URL for the default resource 'gears.png' file ( http://cdn.sobekrepository.org/images/misc/gears.png by default)</summary>
        public static string Gears_Png { get; private set; }

        /// <summary> URL for the default resource 'geo_blue.png' file ( http://cdn.sobekrepository.org/images/mapsearch/geo_blue.png by default)</summary>
        public static string Geo_Blue_Png { get; private set; }

        /// <summary> URL for the default resource 'get_adobe_reader.png' file ( http://cdn.sobekrepository.org/images/misc/get_adobe_reader.png by default)</summary>
        public static string Get_Adobe_Reader_Png { get; private set; }

        /// <summary> URL for the default resource 'getuserlocation.png' file ( http://cdn.sobekrepository.org/images/mapedit/getUserLocation.png by default)</summary>
        public static string Getuserlocation_Png { get; private set; }

        /// <summary> URL for the default resource 'go_button.png' file ( http://cdn.sobekrepository.org/images/misc/go_button.png by default)</summary>
        public static string Go_Button_Png { get; private set; }

        /// <summary> URL for the default resource 'go_gray.gif' file ( http://cdn.sobekrepository.org/images/misc/go_gray.gif by default)</summary>
        public static string Go_Gray_Gif { get; private set; }

        /// <summary> URL for the default resource 'google_share.gif' file ( http://cdn.sobekrepository.org/images/misc/google_share.gif by default)</summary>
        public static string Google_Share_Gif { get; private set; }

        /// <summary> URL for the default resource 'google_share_h.gif' file ( http://cdn.sobekrepository.org/images/misc/google_share_h.gif by default)</summary>
        public static string Google_Share_H_Gif { get; private set; }

        /// <summary> URL for the default resource 'help_button.jpg' file ( http://cdn.sobekrepository.org/images/misc/help_button.jpg by default)</summary>
        public static string Help_Button_Jpg { get; private set; }

        /// <summary> URL for the default resource 'help_button_darkgray.jpg' file ( http://cdn.sobekrepository.org/images/misc/help_button_darkgray.jpg by default)</summary>
        public static string Help_Button_Darkgray_Jpg { get; private set; }

        /// <summary> URL for the default resource 'hide_internal_header.png' file ( http://cdn.sobekrepository.org/images/misc/hide_internal_header.png by default)</summary>
        public static string Hide_Internal_Header_Png { get; private set; }

        /// <summary> URL for the default resource 'hide_internal_header2.png' file ( http://cdn.sobekrepository.org/images/misc/hide_internal_header2.png by default)</summary>
        public static string Hide_Internal_Header2_Png { get; private set; }

        /// <summary> URL for the default resource 'home.png' file ( http://cdn.sobekrepository.org/images/misc/home.png by default)</summary>
        public static string Home_Png { get; private set; }

        /// <summary> URL for the default resource 'home_button.gif' file ( http://cdn.sobekrepository.org/images/misc/home_button.gif by default)</summary>
        public static string Home_Button_Gif { get; private set; }

        /// <summary> URL for the default resource 'home_folder.gif' file ( http://cdn.sobekrepository.org/images/misc/home_folder.gif by default)</summary>
        public static string Home_Folder_Gif { get; private set; }

        /// <summary> URL for the default resource 'html5shiv.js' file ( http://cdn.sobekrepository.org/includes/html5shiv/3.7.3/html5shiv.js by default)</summary>
        public static string Html5shiv_Js { get; private set; }

        /// <summary> URL for the default resource 'icon_permission.png' file ( http://cdn.sobekrepository.org/images/misc/icon_permission.png by default)</summary>
        public static string Icon_Permission_Png { get; private set; }

        /// <summary> URL for the default resource 'icons-os.png' file ( http://cdn.sobekrepository.org/images/mapedit/icons-os.png by default)</summary>
        public static string Icons_Os_Png { get; private set; }

        /// <summary> URL for the default resource 'index.html' file ( http://cdn.sobekrepository.org/images/misc/index.html by default)</summary>
        public static string Index_Html { get; private set; }

        /// <summary> URL for the default resource 'item_count.png' file ( http://cdn.sobekrepository.org/images/misc/item_count.png by default)</summary>
        public static string Item_Count_Png { get; private set; }

        /// <summary> URL for the default resource 'jquery.color-2.1.1.js' file ( http://cdn.sobekrepository.org/includes/jquery-color/2.1.1/jquery.color-2.1.1.js by default)</summary>
        public static string Jquery_Color_2_1_1_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery.datatables.js' file ( http://cdn.sobekrepository.org/includes/datatables/1.11.1/js/jquery.dataTables.min.js by default)</summary>
        public static string Jquery_Datatables_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery.easing.1.3.js' file ( http://cdn.sobekrepository.org/includes/bookturner/1.0.0/jquery.easing.1.3.js by default)</summary>
        public static string Jquery_Easing_1_3_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery.hovercard.js' file ( http://cdn.sobekrepository.org/includes/jquery-hovercard/2.4/jquery.hovercard.min.js by default)</summary>
        public static string Jquery_Hovercard_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery.mousewheel.js' file ( http://cdn.sobekrepository.org/includes/jquery-mousewheel/3.1.3/jquery.mousewheel.js by default)</summary>
        public static string Jquery_Mousewheel_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery.qtip.css' file ( http://cdn.sobekrepository.org/includes/jquery-qtip/2.2.0/jquery.qtip.min.css by default)</summary>
        public static string Jquery_Qtip_Css { get; private set; }

        /// <summary> URL for the default resource 'jquery.qtip.js' file ( http://cdn.sobekrepository.org/includes/jquery-qtip/2.2.0/jquery.qtip.min.js by default)</summary>
        public static string Jquery_Qtip_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery.timeentry.css' file ( http://cdn.sobekrepository.org/includes/timeentry/1.5.2/jquery.timeentry.css by default)</summary>
        public static string Jquery_Timeentry_Css { get; private set; }

        /// <summary> URL for the default resource 'jquery.timeentry.js' file ( http://cdn.sobekrepository.org/includes/timeentry/1.5.2/jquery.timeentry.min.js by default)</summary>
        public static string Jquery_Timeentry_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery.timers.js' file ( http://cdn.sobekrepository.org/includes/jquery-timers/1.2/jquery.timers.min.js by default)</summary>
        public static string Jquery_Timers_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery.uploadifive.js' file ( http://cdn.sobekrepository.org/includes/uploadifive/1.1.2/jquery.uploadifive.min.js by default)</summary>
        public static string Jquery_Uploadifive_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery.uploadify.js' file ( http://cdn.sobekrepository.org/includes/uploadify/3.2.1/jquery.uploadify.min.js by default)</summary>
        public static string Jquery_Uploadify_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery-1.10.2.js' file ( http://cdn.sobekrepository.org/includes/jquery/1.10.2/jquery-1.10.2.min.js by default)</summary>
        public static string Jquery_1_10_2_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery-1.2.6.min.js' file ( http://cdn.sobekrepository.org/includes/bookturner/1.0.0/jquery-1.2.6.min.js by default)</summary>
        public static string Jquery_1_2_6_Min_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery-json-2.4.js' file ( http://cdn.sobekrepository.org/includes/jquery-json/2.4/jquery-json-2.4.min.js by default)</summary>
        public static string Jquery_Json_2_4_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery-knob.js' file ( http://cdn.sobekrepository.org/includes/jquery-knob/1.2.0/jquery-knob.js by default)</summary>
        public static string Jquery_Knob_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery-migrate-1.1.1.js' file ( http://cdn.sobekrepository.org/includes/jquery-migrate/1.1.1/jquery-migrate-1.1.1.min.js by default)</summary>
        public static string Jquery_Migrate_1_1_1_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery-rotate.js' file ( http://cdn.sobekrepository.org/includes/jquery-rotate/2.2/jquery-rotate.js by default)</summary>
        public static string Jquery_Rotate_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery-ui-1.10.1.js' file ( http://cdn.sobekrepository.org/includes/jquery-ui/1.10.1/jquery-ui-1.10.1.js by default)</summary>
        public static string Jquery_Ui_1_10_1_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery-ui-1.10.3.custom.js' file ( http://cdn.sobekrepository.org/includes/jquery-ui/1.10.3/jquery-ui-1.10.3.custom.min.js by default)</summary>
        public static string Jquery_Ui_1_10_3_Custom_Js { get; private set; }

        /// <summary> URL for the default resource 'jquery-ui-1.10.3.draggable.js' file ( http://cdn.sobekrepository.org/includes/jquery-ui-draggable/1.10.3/jquery-ui-1.10.3.draggable.min.js by default)</summary>
        public static string Jquery_Ui_1_10_3_Draggable_Js { get; private set; }

        /// <summary> URL for the default resource 'jsdatepick.full.1.3.js' file ( http://cdn.sobekrepository.org/includes/datepicker/1.3/jsDatePick.full.1.3.js by default)</summary>
        public static string Jsdatepick_Full_1_3_Js { get; private set; }

        /// <summary> URL for the default resource 'jsdatepick.min.1.3.js' file ( http://cdn.sobekrepository.org/includes/datepicker/1.3/jsDatePick.min.1.3.js by default)</summary>
        public static string Jsdatepick_Min_1_3_Js { get; private set; }

        /// <summary> URL for the default resource 'jsdatepick_ltr.css' file ( http://cdn.sobekrepository.org/includes/datepicker/1.3/jsDatePick_ltr.css by default)</summary>
        public static string Jsdatepick_Ltr_Css { get; private set; }

        /// <summary> URL for the default resource 'jstree.css' file ( http://cdn.sobekrepository.org/includes/jstree/3.0.9/themes/default/style.min.css by default)</summary>
        public static string Jstree_Css { get; private set; }

        /// <summary> URL for the default resource 'jstree.js' file ( http://cdn.sobekrepository.org/includes/jstree/3.0.9/jstree.min.js by default)</summary>
        public static string Jstree_Js { get; private set; }

        /// <summary> URL for the default resource 'keydragzoom_packed.js' file ( http://cdn.sobekrepository.org/includes/keydragzoom/1.0/keydragzoom_packed.js by default)</summary>
        public static string Keydragzoom_Packed_Js { get; private set; }

        /// <summary> URL for the default resource 'last2.png' file ( http://cdn.sobekrepository.org/images/bookturner/last2.png by default)</summary>
        public static string Last2_Png { get; private set; }

        /// <summary> URL for the default resource 'leftarrow.png' file ( http://cdn.sobekrepository.org/images/misc/leftarrow.png by default)</summary>
        public static string Leftarrow_Png { get; private set; }

        /// <summary> URL for the default resource 'legend_nonselected_polygon.png' file ( http://cdn.sobekrepository.org/images/misc/legend_nonselected_polygon.png by default)</summary>
        public static string Legend_Nonselected_Polygon_Png { get; private set; }

        /// <summary> URL for the default resource 'legend_point_interest.png' file ( http://cdn.sobekrepository.org/images/misc/legend_point_interest.png by default)</summary>
        public static string Legend_Point_Interest_Png { get; private set; }

        /// <summary> URL for the default resource 'legend_red_pushpin.png' file ( http://cdn.sobekrepository.org/images/misc/legend_red_pushpin.png by default)</summary>
        public static string Legend_Red_Pushpin_Png { get; private set; }

        /// <summary> URL for the default resource 'legend_search_area.png' file ( http://cdn.sobekrepository.org/images/misc/legend_search_area.png by default)</summary>
        public static string Legend_Search_Area_Png { get; private set; }

        /// <summary> URL for the default resource 'legend_selected_polygon.png' file ( http://cdn.sobekrepository.org/images/misc/legend_selected_polygon.png by default)</summary>
        public static string Legend_Selected_Polygon_Png { get; private set; }

        /// <summary> URL for the default resource 'main_information.ico' file ( http://cdn.sobekrepository.org/images/qc/Main_Information.ICO by default)</summary>
        public static string Main_Information_Ico { get; private set; }

        /// <summary> URL for the default resource 'map_drag_hand.gif' file ( http://cdn.sobekrepository.org/images/misc/map_drag_hand.gif by default)</summary>
        public static string Map_Drag_Hand_Gif { get; private set; }

        /// <summary> URL for the default resource 'map_point.gif' file ( http://cdn.sobekrepository.org/images/misc/map_point.gif by default)</summary>
        public static string Map_Point_Gif { get; private set; }

        /// <summary> URL for the default resource 'map_point.png' file ( http://cdn.sobekrepository.org/images/misc/map_point.png by default)</summary>
        public static string Map_Point_Png { get; private set; }

        /// <summary> URL for the default resource 'map_polygon2.gif' file ( http://cdn.sobekrepository.org/images/misc/map_polygon2.gif by default)</summary>
        public static string Map_Polygon2_Gif { get; private set; }

        /// <summary> URL for the default resource 'map_rectangle2.gif' file ( http://cdn.sobekrepository.org/images/misc/map_rectangle2.gif by default)</summary>
        public static string Map_Rectangle2_Gif { get; private set; }

        /// <summary> URL for the default resource 'mapedit.html' file ( http://cdn.sobekrepository.org/images/misc/mapedit.html by default)</summary>
        public static string Mapedit_Html { get; private set; }

        /// <summary> URL for the default resource 'mapsearch.html' file ( http://cdn.sobekrepository.org/images/misc/mapsearch.html by default)</summary>
        public static string Mapsearch_Html { get; private set; }

        /// <summary> URL for the default resource 'mass_update.png' file ( http://cdn.sobekrepository.org/images/misc/mass_update.png by default)</summary>
        public static string Mass_Update_Png { get; private set; }

        /// <summary> URL for the default resource 'mass_update_icon.png' file ( http://cdn.sobekrepository.org/images/misc/mass_update_icon.png by default)</summary>
        public static string Mass_Update_Icon_Png { get; private set; }

        /// <summary> URL for the default resource 'minussign.png' file ( http://cdn.sobekrepository.org/images/misc/minussign.png by default)</summary>
        public static string Minussign_Png { get; private set; }

        /// <summary> URL for the default resource 'missingimage.jpg' file ( http://cdn.sobekrepository.org/images/misc/MissingImage.jpg by default)</summary>
        public static string Missingimage_Jpg { get; private set; }

        /// <summary> URL for the default resource 'move_pages_cursor.cur' file ( http://cdn.sobekrepository.org/images/qc/move_pages_cursor.cur by default)</summary>
        public static string Move_Pages_Cursor_Cur { get; private set; }

        /// <summary> URL for the default resource 'new_element.jpg' file ( http://cdn.sobekrepository.org/images/misc/new_element.jpg by default)</summary>
        public static string New_Element_Jpg { get; private set; }

        /// <summary> URL for the default resource 'new_element_demo.jpg' file ( http://cdn.sobekrepository.org/images/misc/new_element_demo.jpg by default)</summary>
        public static string New_Element_Demo_Jpg { get; private set; }

        /// <summary> URL for the default resource 'new_folder.jpg' file ( http://cdn.sobekrepository.org/images/misc/new_folder.jpg by default)</summary>
        public static string New_Folder_Jpg { get; private set; }

        /// <summary> URL for the default resource 'new_item.gif' file ( http://cdn.sobekrepository.org/images/misc/new_item.gif by default)</summary>
        public static string New_Item_Gif { get; private set; }

        /// <summary> URL for the default resource 'next.png' file ( http://cdn.sobekrepository.org/images/bookturner/next.png by default)</summary>
        public static string Next_Png { get; private set; }

        /// <summary> URL for the default resource 'next2.png' file ( http://cdn.sobekrepository.org/images/bookturner/next2.png by default)</summary>
        public static string Next2_Png { get; private set; }

        /// <summary> URL for the default resource 'no_pages.jpg' file ( http://cdn.sobekrepository.org/images/qc/no_pages.jpg by default)</summary>
        public static string No_Pages_Jpg { get; private set; }

        /// <summary> URL for the default resource 'nocheckmark.png' file ( http://cdn.sobekrepository.org/images/misc/nocheckmark.png by default)</summary>
        public static string Nocheckmark_Png { get; private set; }

        /// <summary> URL for the default resource 'nothumb.jpg' file ( http://cdn.sobekrepository.org/images/misc/NoThumb.jpg by default)</summary>
        public static string Nothumb_Jpg { get; private set; }

        /// <summary> URL for the default resource 'open_folder.jpg' file ( http://cdn.sobekrepository.org/images/misc/open_folder.jpg by default)</summary>
        public static string Open_Folder_Jpg { get; private set; }

        /// <summary> URL for the default resource 'open_folder_public.jpg' file ( http://cdn.sobekrepository.org/images/misc/open_folder_public.jpg by default)</summary>
        public static string Open_Folder_Public_Jpg { get; private set; }

        /// <summary> URL for the default resource 'pagenumbg.gif' file ( http://cdn.sobekrepository.org/images/bookturner/pageNumBg.gif by default)</summary>
        public static string Pagenumbg_Gif { get; private set; }

        /// <summary> URL for the default resource 'plussign.png' file ( http://cdn.sobekrepository.org/images/misc/plussign.png by default)</summary>
        public static string Plussign_Png { get; private set; }

        /// <summary> URL for the default resource 'pmets.gif' file ( http://cdn.sobekrepository.org/images/misc/pmets.gif by default)</summary>
        public static string Pmets_Gif { get; private set; }

        /// <summary> URL for the default resource 'point02.ico' file ( http://cdn.sobekrepository.org/images/qc/POINT02.ICO by default)</summary>
        public static string Point02_Ico { get; private set; }

        /// <summary> URL for the default resource 'point04.ico' file ( http://cdn.sobekrepository.org/images/qc/POINT04.ICO by default)</summary>
        public static string Point04_Ico { get; private set; }

        /// <summary> URL for the default resource 'point13.ico' file ( http://cdn.sobekrepository.org/images/qc/POINT13.ICO by default)</summary>
        public static string Point13_Ico { get; private set; }

        /// <summary> URL for the default resource 'pointer_blue.gif' file ( http://cdn.sobekrepository.org/images/misc/pointer_blue.gif by default)</summary>
        public static string Pointer_Blue_Gif { get; private set; }

        /// <summary> URL for the default resource 'portals.gif' file ( http://cdn.sobekrepository.org/images/misc/portals.gif by default)</summary>
        public static string Portals_Gif { get; private set; }

        /// <summary> URL for the default resource 'portals.png' file ( http://cdn.sobekrepository.org/images/misc/portals.png by default)</summary>
        public static string Portals_Png { get; private set; }

        /// <summary> URL for the default resource 'previous2.png' file ( http://cdn.sobekrepository.org/images/bookturner/previous2.png by default)</summary>
        public static string Previous2_Png { get; private set; }

        /// <summary> URL for the default resource 'print.css' file ( http://cdn.sobekrepository.org/css/sobekcm-print/4.8.4/print.css by default)</summary>
        public static string Print_Css { get; private set; }

        /// <summary> URL for the default resource 'printer.png' file ( http://cdn.sobekrepository.org/images/misc/printer.png by default)</summary>
        public static string Printer_Png { get; private set; }

        /// <summary> URL for the default resource 'private_items.png' file ( http://cdn.sobekrepository.org/images/misc/private_items.png by default)</summary>
        public static string Private_Items_Png { get; private set; }

        /// <summary> URL for the default resource 'private_resource.png' file ( http://cdn.sobekrepository.org/images/misc/private_resource.png by default)</summary>
        public static string Private_Resource_Png { get; private set; }

        /// <summary> URL for the default resource 'private_resource_icon.png' file ( http://cdn.sobekrepository.org/images/misc/private_resource_icon.png by default)</summary>
        public static string Private_Resource_Icon_Png { get; private set; }

        /// <summary> URL for the default resource 'public_resource.png' file ( http://cdn.sobekrepository.org/images/misc/public_resource.png by default)</summary>
        public static string Public_Resource_Png { get; private set; }

        /// <summary> URL for the default resource 'public_resource_icon.png' file ( http://cdn.sobekrepository.org/images/misc/public_resource_icon.png by default)</summary>
        public static string Public_Resource_Icon_Png { get; private set; }

        /// <summary> URL for the default resource 'qc.html' file ( http://cdn.sobekrepository.org/images/misc/qc.html by default)</summary>
        public static string Qc_Html { get; private set; }

        /// <summary> URL for the default resource 'qc_addfiles.png' file ( http://cdn.sobekrepository.org/images/qc/qc_addfiles.png by default)</summary>
        public static string Qc_Addfiles_Png { get; private set; }

        /// <summary> URL for the default resource 'qc_button.png' file ( http://cdn.sobekrepository.org/images/misc/qc_button.png by default)</summary>
        public static string Qc_Button_Png { get; private set; }

        /// <summary> URL for the default resource 'qc_button_icon.png' file ( http://cdn.sobekrepository.org/images/misc/qc_button_icon.png by default)</summary>
        public static string Qc_Button_Icon_Png { get; private set; }

        /// <summary> URL for the default resource 'rect_large.ico' file ( http://cdn.sobekrepository.org/images/qc/rect_large.ico by default)</summary>
        public static string Rect_Large_Ico { get; private set; }

        /// <summary> URL for the default resource 'rect_medium.ico' file ( http://cdn.sobekrepository.org/images/qc/rect_medium.ico by default)</summary>
        public static string Rect_Medium_Ico { get; private set; }

        /// <summary> URL for the default resource 'rect_small.ico' file ( http://cdn.sobekrepository.org/images/qc/rect_small.ico by default)</summary>
        public static string Rect_Small_Ico { get; private set; }

        /// <summary> URL for the default resource 'red-pushpin.png' file ( http://cdn.sobekrepository.org/images/mapedit/mapIcons/red-pushpin.png by default)</summary>
        public static string Red_Pushpin_Png { get; private set; }

        /// <summary> URL for the default resource 'refresh.gif' file ( http://cdn.sobekrepository.org/images/misc/refresh.gif by default)</summary>
        public static string Refresh_Gif { get; private set; }

        /// <summary> URL for the default resource 'refresh.png' file ( http://cdn.sobekrepository.org/images/misc/refresh.png by default)</summary>
        public static string Refresh_Png { get; private set; }

        /// <summary> URL for the default resource 'refresh_folder.jpg' file ( http://cdn.sobekrepository.org/images/misc/refresh_folder.jpg by default)</summary>
        public static string Refresh_Folder_Jpg { get; private set; }

        /// <summary> URL for the default resource 'removeicon.gif' file ( http://cdn.sobekrepository.org/images/mapsearch/removeIcon.gif by default)</summary>
        public static string Removeicon_Gif { get; private set; }

        /// <summary> URL for the default resource 'restricted_resource.png' file ( http://cdn.sobekrepository.org/images/misc/restricted_resource.png by default)</summary>
        public static string Restricted_Resource_Png { get; private set; }

        /// <summary> URL for the default resource 'restricted_resource_icon.png' file ( http://cdn.sobekrepository.org/images/misc/restricted_resource_icon.png by default)</summary>
        public static string Restricted_Resource_Icon_Png { get; private set; }

        /// <summary> URL for the default resource 'return.gif' file ( http://cdn.sobekrepository.org/images/misc/return.gif by default)</summary>
        public static string Return_Gif { get; private set; }

        /// <summary> URL for the default resource 'return.png' file ( http://cdn.sobekrepository.org/images/bookturner/return.png by default)</summary>
        public static string Return_Png { get; private set; }

        /// <summary> URL for the default resource 'rotation-clockwise.png' file ( http://cdn.sobekrepository.org/images/mapedit/rotation-clockwise.png by default)</summary>
        public static string Rotation_Clockwise_Png { get; private set; }

        /// <summary> URL for the default resource 'rotation-counterclockwise.png' file ( http://cdn.sobekrepository.org/images/mapedit/rotation-counterClockwise.png by default)</summary>
        public static string Rotation_Counterclockwise_Png { get; private set; }

        /// <summary> URL for the default resource 'rotation-reset.png' file ( http://cdn.sobekrepository.org/images/mapedit/rotation-reset.png by default)</summary>
        public static string Rotation_Reset_Png { get; private set; }

        /// <summary> URL for the default resource 'save.ico' file ( http://cdn.sobekrepository.org/images/qc/Save.ico by default)</summary>
        public static string Save_Ico { get; private set; }

        /// <summary> URL for the default resource 'saved_searches.gif' file ( http://cdn.sobekrepository.org/images/misc/saved_searches.gif by default)</summary>
        public static string Saved_Searches_Gif { get; private set; }

        /// <summary> URL for the default resource 'saved_searches_big.gif' file ( http://cdn.sobekrepository.org/images/misc/saved_searches_big.gif by default)</summary>
        public static string Saved_Searches_Big_Gif { get; private set; }

        /// <summary> URL for the default resource 'search.png' file ( http://cdn.sobekrepository.org/images/mapedit/search.png by default)</summary>
        public static string Search_Png { get; private set; }

        /// <summary> URL for the default resource 'settings.gif' file ( http://cdn.sobekrepository.org/images/misc/Settings.gif by default)</summary>
        public static string Settings_Gif { get; private set; }

        /// <summary> URL for the default resource 'show_internal_header.png' file ( http://cdn.sobekrepository.org/images/misc/show_internal_header.png by default)</summary>
        public static string Show_Internal_Header_Png { get; private set; }

        /// <summary> URL for the default resource 'skins.gif' file ( http://cdn.sobekrepository.org/images/misc/skins.gif by default)</summary>
        public static string Skins_Gif { get; private set; }

        /// <summary> URL for the default resource 'skins.png' file ( http://cdn.sobekrepository.org/images/misc/skins.png by default)</summary>
        public static string Skins_Png { get; private set; }

        /// <summary> URL for the default resource 'sobekcm.css' file ( http://cdn.sobekrepository.org/css/sobekcm/4.8.4/SobekCM.min.css by default)</summary>
        public static string Sobekcm_Css { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_admin.css' file ( http://cdn.sobekrepository.org/css/sobekcm-admin/4.8.4/SobekCM_Admin.min.css by default)</summary>
        public static string Sobekcm_Admin_Css { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_admin.js' file ( http://cdn.sobekrepository.org/js/sobekcm-admin/4.8.4/sobekcm_admin.js by default)</summary>
        public static string Sobekcm_Admin_Js { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_bookturner.css' file ( http://cdn.sobekrepository.org/css/sobekcm-bookturner/4.8.4/SobekCM_BookTurner.css by default)</summary>
        public static string Sobekcm_Bookturner_Css { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_datatables.css' file ( http://cdn.sobekrepository.org/css/sobekcm-datatables/4.8.4/SobekCM_DataTables.css by default)</summary>
        public static string Sobekcm_Datatables_Css { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_full.js' file ( http://cdn.sobekrepository.org/js/sobekcm-full/4.8.4/sobekcm_full.min.js by default)</summary>
        public static string Sobekcm_Full_Js { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_item.css' file ( http://cdn.sobekrepository.org/css/sobekcm-item/4.8.4/SobekCM_Item.min.css by default)</summary>
        public static string Sobekcm_Item_Css { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_map_search.js' file ( http://cdn.sobekrepository.org/js/sobekcm-map/4.8.4/sobekcm_map_search.js by default)</summary>
        public static string Sobekcm_Map_Search_Js { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_map_tool.js' file ( http://cdn.sobekrepository.org/js/sobekcm-map/4.8.4/sobekcm_map_tool.js by default)</summary>
        public static string Sobekcm_Map_Tool_Js { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_mapeditor.css' file ( http://cdn.sobekrepository.org/css/sobekcm-map/4.8.4/SobekCM_MapEditor.css by default)</summary>
        public static string Sobekcm_Mapeditor_Css { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_mapsearch.css' file ( http://cdn.sobekrepository.org/css/sobekcm-map/4.8.4/SobekCM_MapSearch.css by default)</summary>
        public static string Sobekcm_Mapsearch_Css { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_metadata.css' file ( http://cdn.sobekrepository.org/css/sobekcm-metadata/4.8.4/SobekCM_Metadata.min.css by default)</summary>
        public static string Sobekcm_Metadata_Css { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_metadata.js' file ( http://cdn.sobekrepository.org/js/sobekcm-metadata/4.8.4/sobekcm_metadata.js by default)</summary>
        public static string Sobekcm_Metadata_Js { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_mysobek.css' file ( http://cdn.sobekrepository.org/css/sobekcm-mysobek/4.8.4/SobekCM_MySobek.min.css by default)</summary>
        public static string Sobekcm_Mysobek_Css { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_print.css' file ( http://cdn.sobekrepository.org/css/sobekcm-print/4.8.4/SobekCM_Print.css by default)</summary>
        public static string Sobekcm_Print_Css { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_qc.css' file ( http://cdn.sobekrepository.org/css/sobekcm-qc/4.8.4/SobekCM_QC.css by default)</summary>
        public static string Sobekcm_Qc_Css { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_qc.js' file ( http://cdn.sobekrepository.org/js/sobekcm-qc/4.8.4/sobekcm_qc.js by default)</summary>
        public static string Sobekcm_Qc_Js { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_stats.css' file ( http://cdn.sobekrepository.org/css/sobekcm-stats/4.8.4/SobekCM_Stats.css by default)</summary>
        public static string Sobekcm_Stats_Css { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_thumb_results.js' file ( http://cdn.sobekrepository.org/js/sobekcm-thumb-results/4.8.4/sobekcm_thumb_results.js by default)</summary>
        public static string Sobekcm_Thumb_Results_Js { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_track_item.js' file ( http://cdn.sobekrepository.org/js/sobekcm-track-item/4.8.4/sobekcm_track_item.js by default)</summary>
        public static string Sobekcm_Track_Item_Js { get; private set; }

        /// <summary> URL for the default resource 'sobekcm_trackingsheet.css' file ( http://cdn.sobekrepository.org/css/sobekcm-tracking/4.8.4/SobekCM_TrackingSheet.css by default)</summary>
        public static string Sobekcm_Trackingsheet_Css { get; private set; }

        /// <summary> URL for the default resource 'source.html' file ( http://cdn.sobekrepository.org/images/misc/source.html by default)</summary>
        public static string Source_Html { get; private set; }

        /// <summary> URL for the default resource 'spinner.gif' file ( http://cdn.sobekrepository.org/images/misc/spinner.gif by default)</summary>
        public static string Spinner_Gif { get; private set; }

        /// <summary> URL for the default resource 'spinner_gray.gif' file ( http://cdn.sobekrepository.org/images/misc/spinner_gray.gif by default)</summary>
        public static string Spinner_Gray_Gif { get; private set; }

        /// <summary> URL for the default resource 'stumbleupon_share.gif' file ( http://cdn.sobekrepository.org/images/misc/stumbleupon_share.gif by default)</summary>
        public static string Stumbleupon_Share_Gif { get; private set; }

        /// <summary> URL for the default resource 'stumbleupon_share_h.gif' file ( http://cdn.sobekrepository.org/images/misc/stumbleupon_share_h.gif by default)</summary>
        public static string Stumbleupon_Share_H_Gif { get; private set; }

        /// <summary> URL for the default resource 'submitted_items.gif' file ( http://cdn.sobekrepository.org/images/misc/submitted_items.gif by default)</summary>
        public static string Submitted_Items_Gif { get; private set; }

        /// <summary> URL for the default resource 'table_blue.png' file ( http://cdn.sobekrepository.org/images/mapsearch/table_blue.png by default)</summary>
        public static string Table_Blue_Png { get; private set; }

        /// <summary> URL for the default resource 'thumb_blue.png' file ( http://cdn.sobekrepository.org/images/mapsearch/thumb_blue.png by default)</summary>
        public static string Thumb_Blue_Png { get; private set; }

        /// <summary> URL for the default resource 'thumbnail_cursor.cur' file ( http://cdn.sobekrepository.org/images/qc/thumbnail_cursor.cur by default)</summary>
        public static string Thumbnail_Cursor_Cur { get; private set; }

        /// <summary> URL for the default resource 'thumbnail_large.gif' file ( http://cdn.sobekrepository.org/images/misc/thumbnail_large.gif by default)</summary>
        public static string Thumbnail_Large_Gif { get; private set; }

        /// <summary> URL for the default resource 'thumbs1.gif' file ( http://cdn.sobekrepository.org/images/misc/thumbs1.gif by default)</summary>
        public static string Thumbs1_Gif { get; private set; }

        /// <summary> URL for the default resource 'thumbs1_selected.gif' file ( http://cdn.sobekrepository.org/images/misc/thumbs1_selected.gif by default)</summary>
        public static string Thumbs1_Selected_Gif { get; private set; }

        /// <summary> URL for the default resource 'thumbs2.gif' file ( http://cdn.sobekrepository.org/images/misc/thumbs2.gif by default)</summary>
        public static string Thumbs2_Gif { get; private set; }

        /// <summary> URL for the default resource 'thumbs2_selected.gif' file ( http://cdn.sobekrepository.org/images/misc/thumbs2_selected.gif by default)</summary>
        public static string Thumbs2_Selected_Gif { get; private set; }

        /// <summary> URL for the default resource 'thumbs3.gif' file ( http://cdn.sobekrepository.org/images/misc/thumbs3.gif by default)</summary>
        public static string Thumbs3_Gif { get; private set; }

        /// <summary> URL for the default resource 'thumbs3_selected.gif' file ( http://cdn.sobekrepository.org/images/misc/thumbs3_selected.gif by default)</summary>
        public static string Thumbs3_Selected_Gif { get; private set; }

        /// <summary> URL for the default resource 'toolbar-toggle.png' file ( http://cdn.sobekrepository.org/images/mapedit/toolbar-toggle.png by default)</summary>
        public static string Toolbar_Toggle_Png { get; private set; }

        /// <summary> URL for the default resource 'toolbox-close2.png' file ( http://cdn.sobekrepository.org/images/mapedit/toolbox-close2.png by default)</summary>
        public static string Toolbox_Close2_Png { get; private set; }

        /// <summary> URL for the default resource 'toolbox-icon.png' file ( http://cdn.sobekrepository.org/images/mapedit/toolbox-icon.png by default)</summary>
        public static string Toolbox_Icon_Png { get; private set; }

        /// <summary> URL for the default resource 'toolbox-maximize2.png' file ( http://cdn.sobekrepository.org/images/mapedit/toolbox-maximize2.png by default)</summary>
        public static string Toolbox_Maximize2_Png { get; private set; }

        /// <summary> URL for the default resource 'toolbox-minimize2.png' file ( http://cdn.sobekrepository.org/images/mapedit/toolbox-minimize2.png by default)</summary>
        public static string Toolbox_Minimize2_Png { get; private set; }

        /// <summary> URL for the default resource 'top_left.jpg' file ( http://cdn.sobekrepository.org/images/bookturner/top_left.jpg by default)</summary>
        public static string Top_Left_Jpg { get; private set; }

        /// <summary> URL for the default resource 'top_right.jpg' file ( http://cdn.sobekrepository.org/images/bookturner/top_right.jpg by default)</summary>
        public static string Top_Right_Jpg { get; private set; }

        /// <summary> URL for the default resource 'track2.gif' file ( http://cdn.sobekrepository.org/images/misc/track2.gif by default)</summary>
        public static string Track2_Gif { get; private set; }

        /// <summary> URL for the default resource 'trash01.ico' file ( http://cdn.sobekrepository.org/images/qc/TRASH01.ICO by default)</summary>
        public static string Trash01_Ico { get; private set; }

        /// <summary> URL for the default resource 'twitter_share.gif' file ( http://cdn.sobekrepository.org/images/misc/twitter_share.gif by default)</summary>
        public static string Twitter_Share_Gif { get; private set; }

        /// <summary> URL for the default resource 'twitter_share_h.gif' file ( http://cdn.sobekrepository.org/images/misc/twitter_share_h.gif by default)</summary>
        public static string Twitter_Share_H_Gif { get; private set; }

        /// <summary> URL for the default resource 'ufdc_banner_700.jpg' file ( http://cdn.sobekrepository.org/images/misc/ufdc_banner_700.jpg by default)</summary>
        public static string Ufdc_Banner_700_Jpg { get; private set; }

        /// <summary> URL for the default resource 'ui-icons_ffffff_256x240.png' file ( http://cdn.sobekrepository.org/images/mapsearch/ui-icons_ffffff_256x240.png by default)</summary>
        public static string Ui_Icons_Ffffff_256X240_Png { get; private set; }

        /// <summary> URL for the default resource 'uploadifive.css' file ( http://cdn.sobekrepository.org/includes/uploadifive/1.1.2/uploadifive.css by default)</summary>
        public static string Uploadifive_Css { get; private set; }

        /// <summary> URL for the default resource 'uploadify.css' file ( http://cdn.sobekrepository.org/includes/uploadify/3.2.1/uploadify.css by default)</summary>
        public static string Uploadify_Css { get; private set; }

        /// <summary> URL for the default resource 'uploadify.swf' file ( http://cdn.sobekrepository.org/includes/uploadify/3.2.1/uploadify.swf by default)</summary>
        public static string Uploadify_Swf { get; private set; }

        /// <summary> URL for the default resource 'usage.png' file ( http://cdn.sobekrepository.org/images/misc/usage.png by default)</summary>
        public static string Usage_Png { get; private set; }

        /// <summary> URL for the default resource 'usage_statistics.png' file ( http://cdn.sobekrepository.org/images/misc/usage_statistics.png by default)</summary>
        public static string Usage_Statistics_Png { get; private set; }

        /// <summary> URL for the default resource 'users.gif' file ( http://cdn.sobekrepository.org/images/misc/Users.gif by default)</summary>
        public static string Users_Gif { get; private set; }

        /// <summary> URL for the default resource 'users.png' file ( http://cdn.sobekrepository.org/images/misc/Users.png by default)</summary>
        public static string Users_Png { get; private set; }

        /// <summary> URL for the default resource 'view.ico' file ( http://cdn.sobekrepository.org/images/qc/View.ico by default)</summary>
        public static string View_Ico { get; private set; }

        /// <summary> URL for the default resource 'view_work_log.png' file ( http://cdn.sobekrepository.org/images/misc/view_work_log.png by default)</summary>
        public static string View_Work_Log_Png { get; private set; }

        /// <summary> URL for the default resource 'view_work_log_icon.png' file ( http://cdn.sobekrepository.org/images/misc/view_work_log_icon.png by default)</summary>
        public static string View_Work_Log_Icon_Png { get; private set; }

        /// <summary> URL for the default resource 'wizard.png' file ( http://cdn.sobekrepository.org/images/misc/wizard.png by default)</summary>
        public static string Wizard_Png { get; private set; }

        /// <summary> URL for the default resource 'wordmarks.gif' file ( http://cdn.sobekrepository.org/images/misc/wordmarks.gif by default)</summary>
        public static string Wordmarks_Gif { get; private set; }

        /// <summary> URL for the default resource 'wrench.png' file ( http://cdn.sobekrepository.org/images/misc/wrench.png by default)</summary>
        public static string Wrench_Png { get; private set; }

        /// <summary> URL for the default resource 'yahoo_share.gif' file ( http://cdn.sobekrepository.org/images/misc/yahoo_share.gif by default)</summary>
        public static string Yahoo_Share_Gif { get; private set; }

        /// <summary> URL for the default resource 'yahoo_share_h.gif' file ( http://cdn.sobekrepository.org/images/misc/yahoo_share_h.gif by default)</summary>
        public static string Yahoo_Share_H_Gif { get; private set; }

        /// <summary> URL for the default resource 'yahoobuzz_share.gif' file ( http://cdn.sobekrepository.org/images/misc/yahoobuzz_share.gif by default)</summary>
        public static string Yahoobuzz_Share_Gif { get; private set; }

        /// <summary> URL for the default resource 'yahoobuzz_share_h.gif' file ( http://cdn.sobekrepository.org/images/misc/yahoobuzz_share_h.gif by default)</summary>
        public static string Yahoobuzz_Share_H_Gif { get; private set; }

        /// <summary> URL for the default resource 'zoom_tool.cur' file ( http://cdn.sobekrepository.org/images/misc/zoom_tool.cur by default)</summary>
        public static string Zoom_Tool_Cur { get; private set; }

        /// <summary> URL for the default resource 'zoomin.png' file ( http://cdn.sobekrepository.org/images/bookturner/zoomin.png by default)</summary>
        public static string Zoomin_Png { get; private set; }

        /// <summary> URL for the default resource 'zoomout.png' file ( http://cdn.sobekrepository.org/images/bookturner/zoomout.png by default)</summary>
        public static string Zoomout_Png { get; private set; }

        /// <summary> Add a single file, with key and source </summary>
        /// <param name="Key"> Key for this file, from the default resources config file </param>
        /// <param name="Source"> Source (i.e., URL) for this file </param>
        public static void Add_File(string Key, string Source)
        {
            switch (Key)
            {
                case "16px-feed-icon.svg.png":
                    Sixteen_Px_Feed_Icon_Svg_Png = Source;
                    break;

                case "add_geospatial_icon.png":
                    Add_Geospatial_Icon_Png = Source;
                    break;

                case "add_volume.png":
                    Add_Volume_Png = Source;
                    break;

                case "add_volume_icon.png":
                    Add_Volume_Icon_Png = Source;
                    break;

                case "admin_view.png":
                    Admin_View_Png = Source;
                    break;

                case "ajax-loader.gif":
                    Ajax_Loader_Gif = Source;
                    break;

                case "arw05lt.gif":
                    Arw05lt_Gif = Source;
                    break;

                case "arw05rt.gif":
                    Arw05rt_Gif = Source;
                    break;

                case "autofill_volumes.png":
                    Autofill_Volumes_Png = Source;
                    break;

                case "bg1.png":
                    Bg1_Png = Source;
                    break;

                case "big_bookshelf.gif":
                    Big_Bookshelf_Gif = Source;
                    break;

                case "blue.png":
                    Blue_Png = Source;
                    break;

                case "blue-pin.png":
                    Blue_Pin_Png = Source;
                    break;

                case "bookshelf.jpg":
                    Bookshelf_Jpg = Source;
                    break;

                case "bookshelf.png":
                    Bookshelf_Png = Source;
                    break;

                case "bookturner.html":
                    Bookturner_Html = Source;
                    break;

                case "bookturner.js":
                    Bookturner_Js = Source;
                    break;

                case "brief_blue.png":
                    Brief_Blue_Png = Source;
                    break;

                case "building.gif":
                    Building_Gif = Source;
                    break;

                case "button_down_arrow.png":
                    Button_Down_Arrow_Png = Source;
                    break;

                case "button_first_arrow.png":
                    Button_First_Arrow_Png = Source;
                    break;

                case "button_last_arrow.png":
                    Button_Last_Arrow_Png = Source;
                    break;

                case "button_next_arrow.png":
                    Button_Next_Arrow_Png = Source;
                    break;

                case "button_next_arrow2.png":
                    Button_Next_Arrow2_Png = Source;
                    break;

                case "button_previous_arrow.png":
                    Button_Previous_Arrow_Png = Source;
                    break;

                case "button_up_arrow.png":
                    Button_Up_Arrow_Png = Source;
                    break;

                case "button-action1.png":
                    Button_Action1_Png = Source;
                    break;

                case "button-action2.png":
                    Button_Action2_Png = Source;
                    break;

                case "button-action3.png":
                    Button_Action3_Png = Source;
                    break;

                case "button-base.png":
                    Button_Base_Png = Source;
                    break;

                case "button-blocklot.png":
                    Button_Blocklot_Png = Source;
                    break;

                case "button-cancel.png":
                    Button_Cancel_Png = Source;
                    break;

                case "button-controls.png":
                    Button_Controls_Png = Source;
                    break;

                case "button-converttooverlay.png":
                    Button_Converttooverlay_Png = Source;
                    break;

                case "button-drawcircle.png":
                    Button_Drawcircle_Png = Source;
                    break;

                case "button-drawline.png":
                    Button_Drawline_Png = Source;
                    break;

                case "button-drawmarker.png":
                    Button_Drawmarker_Png = Source;
                    break;

                case "button-drawpolygon.png":
                    Button_Drawpolygon_Png = Source;
                    break;

                case "button-drawrectangle.png":
                    Button_Drawrectangle_Png = Source;
                    break;

                case "button-hybrid.png":
                    Button_Hybrid_Png = Source;
                    break;

                case "button-itemgetuserlocation.png":
                    Button_Itemgetuserlocation_Png = Source;
                    break;

                case "button-itemplace.png":
                    Button_Itemplace_Png = Source;
                    break;

                case "button-itemreset.png":
                    Button_Itemreset_Png = Source;
                    break;

                case "button-layercustom.png":
                    Button_Layercustom_Png = Source;
                    break;

                case "button-layerhybrid.png":
                    Button_Layerhybrid_Png = Source;
                    break;

                case "button-layerreset.png":
                    Button_Layerreset_Png = Source;
                    break;

                case "button-layerroadmap.png":
                    Button_Layerroadmap_Png = Source;
                    break;

                case "button-layersatellite.png":
                    Button_Layersatellite_Png = Source;
                    break;

                case "button-layerterrain.png":
                    Button_Layerterrain_Png = Source;
                    break;

                case "button-manageitem.png":
                    Button_Manageitem_Png = Source;
                    break;

                case "button-manageoverlay.png":
                    Button_Manageoverlay_Png = Source;
                    break;

                case "button-managepoi.png":
                    Button_Managepoi_Png = Source;
                    break;

                case "button-overlayedit.png":
                    Button_Overlayedit_Png = Source;
                    break;

                case "button-overlaygetuserlocation.png":
                    Button_Overlaygetuserlocation_Png = Source;
                    break;

                case "button-overlayplace.png":
                    Button_Overlayplace_Png = Source;
                    break;

                case "button-overlayreset.png":
                    Button_Overlayreset_Png = Source;
                    break;

                case "button-overlayrotate.png":
                    Button_Overlayrotate_Png = Source;
                    break;

                case "button-overlaytoggle.png":
                    Button_Overlaytoggle_Png = Source;
                    break;

                case "button-overlaytransparency.png":
                    Button_Overlaytransparency_Png = Source;
                    break;

                case "button-pandown.png":
                    Button_Pandown_Png = Source;
                    break;

                case "button-panleft.png":
                    Button_Panleft_Png = Source;
                    break;

                case "button-panreset.png":
                    Button_Panreset_Png = Source;
                    break;

                case "button-panright.png":
                    Button_Panright_Png = Source;
                    break;

                case "button-panup.png":
                    Button_Panup_Png = Source;
                    break;

                case "button-poicircle.png":
                    Button_Poicircle_Png = Source;
                    break;

                case "button-poiedit.png":
                    Button_Poiedit_Png = Source;
                    break;

                case "button-poigetuserlocation.png":
                    Button_Poigetuserlocation_Png = Source;
                    break;

                case "button-poiline.png":
                    Button_Poiline_Png = Source;
                    break;

                case "button-poimarker.png":
                    Button_Poimarker_Png = Source;
                    break;

                case "button-poiplace.png":
                    Button_Poiplace_Png = Source;
                    break;

                case "button-poipolygon.png":
                    Button_Poipolygon_Png = Source;
                    break;

                case "button-poirectangle.png":
                    Button_Poirectangle_Png = Source;
                    break;

                case "button-poireset.png":
                    Button_Poireset_Png = Source;
                    break;

                case "button-poitoggle.png":
                    Button_Poitoggle_Png = Source;
                    break;

                case "button-reset.png":
                    Button_Reset_Png = Source;
                    break;

                case "button-roadmap.png":
                    Button_Roadmap_Png = Source;
                    break;

                case "button-satellite.png":
                    Button_Satellite_Png = Source;
                    break;

                case "button-save.png":
                    Button_Save_Png = Source;
                    break;

                case "button-search.png":
                    Button_Search_Png = Source;
                    break;

                case "button-terrain.png":
                    Button_Terrain_Png = Source;
                    break;

                case "button-togglemapcontrols.png":
                    Button_Togglemapcontrols_Png = Source;
                    break;

                case "button-toggletoolbar.png":
                    Button_Toggletoolbar_Png = Source;
                    break;

                case "button-toggletoolbox.png":
                    Button_Toggletoolbox_Png = Source;
                    break;

                case "button-toolbox.png":
                    Button_Toolbox_Png = Source;
                    break;

                case "button-usesearchaslocation.png":
                    Button_Usesearchaslocation_Png = Source;
                    break;

                case "button-zoomin.png":
                    Button_Zoomin_Png = Source;
                    break;

                case "button-zoomout.png":
                    Button_Zoomout_Png = Source;
                    break;

                case "button-zoomreset.png":
                    Button_Zoomreset_Png = Source;
                    break;

                case "button-zoomreset2.png":
                    Button_Zoomreset2_Png = Source;
                    break;

                case "calendar_button.png":
                    Calendar_Button_Png = Source;
                    break;

                case "cancel.ico":
                    Cancel_Ico = Source;
                    break;

                case "cc_by.png":
                    Cc_By_Png = Source;
                    break;

                case "cc_by_nc.png":
                    Cc_By_Nc_Png = Source;
                    break;

                case "cc_by_nc_nd.png":
                    Cc_By_Nc_Nd_Png = Source;
                    break;

                case "cc_by_nc_sa.png":
                    Cc_By_Nc_Sa_Png = Source;
                    break;

                case "cc_by_nd.png":
                    Cc_By_Nd_Png = Source;
                    break;

                case "cc_by_sa.png":
                    Cc_By_Sa_Png = Source;
                    break;

                case "cc_zero.png":
                    Cc_Zero_Png = Source;
                    break;

                case "chart.js":
                    Chart_Js = Source;
                    break;

                case "chat.png":
                    Chat_Png = Source;
                    break;

                case "checkmark.png":
                    Checkmark_Png = Source;
                    break;

                case "checkmark2.png":
                    Checkmark2_Png = Source;
                    break;

                case "ckeditor.js":
                    Ckeditor_Js = Source;
                    break;

                case "closed_folder.jpg":
                    Closed_Folder_Jpg = Source;
                    break;

                case "closed_folder_public.jpg":
                    Closed_Folder_Public_Jpg = Source;
                    break;

                case "closed_folder_public_big.jpg":
                    Closed_Folder_Public_Big_Jpg = Source;
                    break;

                case "contentslider.js":
                    Contentslider_Js = Source;
                    break;

                case "dark_resource.png":
                    Dark_Resource_Png = Source;
                    break;

                case "default_banner.jpg":
                    Default_Banner_Jpg = Source;
                    break;

                case "default_button.gif":
                    Default_Button_Gif = Source;
                    break;

                case "default_button.png":
                    Default_Button_Png = Source;
                    break;

                case "delete_cursor.cur":
                    Delete_Cursor_Cur = Source;
                    break;

                case "delete_item.png":
                    Delete_Item_Png = Source;
                    break;

                case "delete_item_icon.png":
                    Delete_Item_Icon_Png = Source;
                    break;

                case "digg_share.gif":
                    Digg_Share_Gif = Source;
                    break;

                case "digg_share_h.gif":
                    Digg_Share_H_Gif = Source;
                    break;

                case "dloc_banner_700.jpg":
                    Dloc_Banner_700_Jpg = Source;
                    break;

                case "drag1pg.ico":
                    Drag1pg_Ico = Source;
                    break;

                case "edit.gif":
                    Edit_Gif = Source;
                    break;

                case "edit.png":
                    Edit_Png = Source;
                    break;

                case "edit_behaviors.png":
                    Edit_Behaviors_Png = Source;
                    break;

                case "edit_behaviors_icon.png":
                    Edit_Behaviors_Icon_Png = Source;
                    break;

                case "edit_hierarchy.png":
                    Edit_Hierarchy_Png = Source;
                    break;

                case "edit_metadata.png":
                    Edit_Metadata_Png = Source;
                    break;

                case "edit_metadata_icon.png":
                    Edit_Metadata_Icon_Png = Source;
                    break;

                case "email.png":
                    Email_Png = Source;
                    break;

                case "emptypage.jpg":
                    Emptypage_Jpg = Source;
                    break;

                case "exit.gif":
                    Exit_Gif = Source;
                    break;

                case "facebook_share.gif":
                    Facebook_Share_Gif = Source;
                    break;

                case "facebook_share_h.gif":
                    Facebook_Share_H_Gif = Source;
                    break;

                case "favorites_share.gif":
                    Favorites_Share_Gif = Source;
                    break;

                case "favorites_share_h.gif":
                    Favorites_Share_H_Gif = Source;
                    break;

                case "file_management.png":
                    File_Management_Png = Source;
                    break;

                case "file_management_icon.png":
                    File_Management_Icon_Png = Source;
                    break;

                case "firewall.gif":
                    Firewall_Gif = Source;
                    break;

                case "firewall.png":
                    Firewall_Png = Source;
                    break;

                case "first2.png":
                    First2_Png = Source;
                    break;

                case "forwarding.gif":
                    Forwarding_Gif = Source;
                    break;

                case "forwarding.png":
                    Forwarding_Png = Source;
                    break;

                case "gears.png":
                    Gears_Png = Source;
                    break;

                case "geo_blue.png":
                    Geo_Blue_Png = Source;
                    break;

                case "get_adobe_reader.png":
                    Get_Adobe_Reader_Png = Source;
                    break;

                case "getuserlocation.png":
                    Getuserlocation_Png = Source;
                    break;

                case "go_button.png":
                    Go_Button_Png = Source;
                    break;

                case "go_gray.gif":
                    Go_Gray_Gif = Source;
                    break;

                case "google_share.gif":
                    Google_Share_Gif = Source;
                    break;

                case "google_share_h.gif":
                    Google_Share_H_Gif = Source;
                    break;

                case "help_button.jpg":
                    Help_Button_Jpg = Source;
                    break;

                case "help_button_darkgray.jpg":
                    Help_Button_Darkgray_Jpg = Source;
                    break;

                case "hide_internal_header.png":
                    Hide_Internal_Header_Png = Source;
                    break;

                case "hide_internal_header2.png":
                    Hide_Internal_Header2_Png = Source;
                    break;

                case "home.png":
                    Home_Png = Source;
                    break;

                case "home_button.gif":
                    Home_Button_Gif = Source;
                    break;

                case "home_folder.gif":
                    Home_Folder_Gif = Source;
                    break;

                case "html5shiv.js":
                    Html5shiv_Js = Source;
                    break;

                case "icon_permission.png":
                    Icon_Permission_Png = Source;
                    break;

                case "icons-os.png":
                    Icons_Os_Png = Source;
                    break;

                case "index.html":
                    Index_Html = Source;
                    break;

                case "item_count.png":
                    Item_Count_Png = Source;
                    break;

                case "jquery.color-2.1.1.js":
                    Jquery_Color_2_1_1_Js = Source;
                    break;

                case "jquery.datatables.js":
                    Jquery_Datatables_Js = Source;
                    break;

                case "jquery.easing.1.3.js":
                    Jquery_Easing_1_3_Js = Source;
                    break;

                case "jquery.hovercard.js":
                    Jquery_Hovercard_Js = Source;
                    break;

                case "jquery.mousewheel.js":
                    Jquery_Mousewheel_Js = Source;
                    break;

                case "jquery.qtip.css":
                    Jquery_Qtip_Css = Source;
                    break;

                case "jquery.qtip.js":
                    Jquery_Qtip_Js = Source;
                    break;

                case "jquery.timeentry.css":
                    Jquery_Timeentry_Css = Source;
                    break;

                case "jquery.timeentry.js":
                    Jquery_Timeentry_Js = Source;
                    break;

                case "jquery.timers.js":
                    Jquery_Timers_Js = Source;
                    break;

                case "jquery.uploadifive.js":
                    Jquery_Uploadifive_Js = Source;
                    break;

                case "jquery.uploadify.js":
                    Jquery_Uploadify_Js = Source;
                    break;

                case "jquery-1.10.2.js":
                    Jquery_1_10_2_Js = Source;
                    break;

                case "jquery-1.2.6.min.js":
                    Jquery_1_2_6_Min_Js = Source;
                    break;

                case "jquery-json-2.4.js":
                    Jquery_Json_2_4_Js = Source;
                    break;

                case "jquery-knob.js":
                    Jquery_Knob_Js = Source;
                    break;

                case "jquery-migrate-1.1.1.js":
                    Jquery_Migrate_1_1_1_Js = Source;
                    break;

                case "jquery-rotate.js":
                    Jquery_Rotate_Js = Source;
                    break;

                case "jquery-ui-1.10.1.js":
                    Jquery_Ui_1_10_1_Js = Source;
                    break;

                case "jquery-ui-1.10.3.custom.js":
                    Jquery_Ui_1_10_3_Custom_Js = Source;
                    break;

                case "jquery-ui-1.10.3.draggable.js":
                    Jquery_Ui_1_10_3_Draggable_Js = Source;
                    break;

                case "jsdatepick.full.1.3.js":
                    Jsdatepick_Full_1_3_Js = Source;
                    break;

                case "jsdatepick.min.1.3.js":
                    Jsdatepick_Min_1_3_Js = Source;
                    break;

                case "jsdatepick_ltr.css":
                    Jsdatepick_Ltr_Css = Source;
                    break;

                case "jstree.css":
                    Jstree_Css = Source;
                    break;

                case "jstree.js":
                    Jstree_Js = Source;
                    break;

                case "keydragzoom_packed.js":
                    Keydragzoom_Packed_Js = Source;
                    break;

                case "last2.png":
                    Last2_Png = Source;
                    break;

                case "leftarrow.png":
                    Leftarrow_Png = Source;
                    break;

                case "legend_nonselected_polygon.png":
                    Legend_Nonselected_Polygon_Png = Source;
                    break;

                case "legend_point_interest.png":
                    Legend_Point_Interest_Png = Source;
                    break;

                case "legend_red_pushpin.png":
                    Legend_Red_Pushpin_Png = Source;
                    break;

                case "legend_search_area.png":
                    Legend_Search_Area_Png = Source;
                    break;

                case "legend_selected_polygon.png":
                    Legend_Selected_Polygon_Png = Source;
                    break;

                case "main_information.ico":
                    Main_Information_Ico = Source;
                    break;

                case "map_drag_hand.gif":
                    Map_Drag_Hand_Gif = Source;
                    break;

                case "map_point.gif":
                    Map_Point_Gif = Source;
                    break;

                case "map_point.png":
                    Map_Point_Png = Source;
                    break;

                case "map_polygon2.gif":
                    Map_Polygon2_Gif = Source;
                    break;

                case "map_rectangle2.gif":
                    Map_Rectangle2_Gif = Source;
                    break;

                case "mapedit.html":
                    Mapedit_Html = Source;
                    break;

                case "mapsearch.html":
                    Mapsearch_Html = Source;
                    break;

                case "mass_update.png":
                    Mass_Update_Png = Source;
                    break;

                case "mass_update_icon.png":
                    Mass_Update_Icon_Png = Source;
                    break;

                case "minussign.png":
                    Minussign_Png = Source;
                    break;

                case "missingimage.jpg":
                    Missingimage_Jpg = Source;
                    break;

                case "move_pages_cursor.cur":
                    Move_Pages_Cursor_Cur = Source;
                    break;

                case "new_element.jpg":
                    New_Element_Jpg = Source;
                    break;

                case "new_element_demo.jpg":
                    New_Element_Demo_Jpg = Source;
                    break;

                case "new_folder.jpg":
                    New_Folder_Jpg = Source;
                    break;

                case "new_item.gif":
                    New_Item_Gif = Source;
                    break;

                case "next.png":
                    Next_Png = Source;
                    break;

                case "next2.png":
                    Next2_Png = Source;
                    break;

                case "no_pages.jpg":
                    No_Pages_Jpg = Source;
                    break;

                case "nocheckmark.png":
                    Nocheckmark_Png = Source;
                    break;

                case "nothumb.jpg":
                    Nothumb_Jpg = Source;
                    break;

                case "open_folder.jpg":
                    Open_Folder_Jpg = Source;
                    break;

                case "open_folder_public.jpg":
                    Open_Folder_Public_Jpg = Source;
                    break;

                case "pagenumbg.gif":
                    Pagenumbg_Gif = Source;
                    break;

                case "plussign.png":
                    Plussign_Png = Source;
                    break;

                case "pmets.gif":
                    Pmets_Gif = Source;
                    break;

                case "point02.ico":
                    Point02_Ico = Source;
                    break;

                case "point04.ico":
                    Point04_Ico = Source;
                    break;

                case "point13.ico":
                    Point13_Ico = Source;
                    break;

                case "pointer_blue.gif":
                    Pointer_Blue_Gif = Source;
                    break;

                case "portals.gif":
                    Portals_Gif = Source;
                    break;

                case "portals.png":
                    Portals_Png = Source;
                    break;

                case "previous2.png":
                    Previous2_Png = Source;
                    break;

                case "print.css":
                    Print_Css = Source;
                    break;

                case "printer.png":
                    Printer_Png = Source;
                    break;

                case "private_items.png":
                    Private_Items_Png = Source;
                    break;

                case "private_resource.png":
                    Private_Resource_Png = Source;
                    break;

                case "private_resource_icon.png":
                    Private_Resource_Icon_Png = Source;
                    break;

                case "public_resource.png":
                    Public_Resource_Png = Source;
                    break;

                case "public_resource_icon.png":
                    Public_Resource_Icon_Png = Source;
                    break;

                case "qc.html":
                    Qc_Html = Source;
                    break;

                case "qc_addfiles.png":
                    Qc_Addfiles_Png = Source;
                    break;

                case "qc_button.png":
                    Qc_Button_Png = Source;
                    break;

                case "qc_button_icon.png":
                    Qc_Button_Icon_Png = Source;
                    break;

                case "rect_large.ico":
                    Rect_Large_Ico = Source;
                    break;

                case "rect_medium.ico":
                    Rect_Medium_Ico = Source;
                    break;

                case "rect_small.ico":
                    Rect_Small_Ico = Source;
                    break;

                case "red-pushpin.png":
                    Red_Pushpin_Png = Source;
                    break;

                case "refresh.gif":
                    Refresh_Gif = Source;
                    break;

                case "refresh.png":
                    Refresh_Png = Source;
                    break;

                case "refresh_folder.jpg":
                    Refresh_Folder_Jpg = Source;
                    break;

                case "removeicon.gif":
                    Removeicon_Gif = Source;
                    break;

                case "restricted_resource.png":
                    Restricted_Resource_Png = Source;
                    break;

                case "restricted_resource_icon.png":
                    Restricted_Resource_Icon_Png = Source;
                    break;

                case "return.gif":
                    Return_Gif = Source;
                    break;

                case "return.png":
                    Return_Png = Source;
                    break;

                case "rotation-clockwise.png":
                    Rotation_Clockwise_Png = Source;
                    break;

                case "rotation-counterclockwise.png":
                    Rotation_Counterclockwise_Png = Source;
                    break;

                case "rotation-reset.png":
                    Rotation_Reset_Png = Source;
                    break;

                case "save.ico":
                    Save_Ico = Source;
                    break;

                case "saved_searches.gif":
                    Saved_Searches_Gif = Source;
                    break;

                case "saved_searches_big.gif":
                    Saved_Searches_Big_Gif = Source;
                    break;

                case "search.png":
                    Search_Png = Source;
                    break;

                case "settings.gif":
                    Settings_Gif = Source;
                    break;

                case "show_internal_header.png":
                    Show_Internal_Header_Png = Source;
                    break;

                case "skins.gif":
                    Skins_Gif = Source;
                    break;

                case "skins.png":
                    Skins_Png = Source;
                    break;

                case "sobekcm.css":
                    Sobekcm_Css = Source;
                    break;

                case "sobekcm_admin.css":
                    Sobekcm_Admin_Css = Source;
                    break;

                case "sobekcm_admin.js":
                    Sobekcm_Admin_Js = Source;
                    break;

                case "sobekcm_bookturner.css":
                    Sobekcm_Bookturner_Css = Source;
                    break;

                case "sobekcm_datatables.css":
                    Sobekcm_Datatables_Css = Source;
                    break;

                case "sobekcm_full.js":
                    Sobekcm_Full_Js = Source;
                    break;

                case "sobekcm_item.css":
                    Sobekcm_Item_Css = Source;
                    break;

                case "sobekcm_map_search.js":
                    Sobekcm_Map_Search_Js = Source;
                    break;

                case "sobekcm_map_tool.js":
                    Sobekcm_Map_Tool_Js = Source;
                    break;

                case "sobekcm_mapeditor.css":
                    Sobekcm_Mapeditor_Css = Source;
                    break;

                case "sobekcm_mapsearch.css":
                    Sobekcm_Mapsearch_Css = Source;
                    break;

                case "sobekcm_metadata.css":
                    Sobekcm_Metadata_Css = Source;
                    break;

                case "sobekcm_metadata.js":
                    Sobekcm_Metadata_Js = Source;
                    break;

                case "sobekcm_mysobek.css":
                    Sobekcm_Mysobek_Css = Source;
                    break;

                case "sobekcm_print.css":
                    Sobekcm_Print_Css = Source;
                    break;

                case "sobekcm_qc.css":
                    Sobekcm_Qc_Css = Source;
                    break;

                case "sobekcm_qc.js":
                    Sobekcm_Qc_Js = Source;
                    break;

                case "sobekcm_stats.css":
                    Sobekcm_Stats_Css = Source;
                    break;

                case "sobekcm_thumb_results.js":
                    Sobekcm_Thumb_Results_Js = Source;
                    break;

                case "sobekcm_track_item.js":
                    Sobekcm_Track_Item_Js = Source;
                    break;

                case "sobekcm_trackingsheet.css":
                    Sobekcm_Trackingsheet_Css = Source;
                    break;

                case "source.html":
                    Source_Html = Source;
                    break;

                case "spinner.gif":
                    Spinner_Gif = Source;
                    break;

                case "spinner_gray.gif":
                    Spinner_Gray_Gif = Source;
                    break;

                case "stumbleupon_share.gif":
                    Stumbleupon_Share_Gif = Source;
                    break;

                case "stumbleupon_share_h.gif":
                    Stumbleupon_Share_H_Gif = Source;
                    break;

                case "submitted_items.gif":
                    Submitted_Items_Gif = Source;
                    break;

                case "table_blue.png":
                    Table_Blue_Png = Source;
                    break;

                case "thumb_blue.png":
                    Thumb_Blue_Png = Source;
                    break;

                case "thumbnail_cursor.cur":
                    Thumbnail_Cursor_Cur = Source;
                    break;

                case "thumbnail_large.gif":
                    Thumbnail_Large_Gif = Source;
                    break;

                case "thumbs1.gif":
                    Thumbs1_Gif = Source;
                    break;

                case "thumbs1_selected.gif":
                    Thumbs1_Selected_Gif = Source;
                    break;

                case "thumbs2.gif":
                    Thumbs2_Gif = Source;
                    break;

                case "thumbs2_selected.gif":
                    Thumbs2_Selected_Gif = Source;
                    break;

                case "thumbs3.gif":
                    Thumbs3_Gif = Source;
                    break;

                case "thumbs3_selected.gif":
                    Thumbs3_Selected_Gif = Source;
                    break;

                case "toolbar-toggle.png":
                    Toolbar_Toggle_Png = Source;
                    break;

                case "toolbox-close2.png":
                    Toolbox_Close2_Png = Source;
                    break;

                case "toolbox-icon.png":
                    Toolbox_Icon_Png = Source;
                    break;

                case "toolbox-maximize2.png":
                    Toolbox_Maximize2_Png = Source;
                    break;

                case "toolbox-minimize2.png":
                    Toolbox_Minimize2_Png = Source;
                    break;

                case "top_left.jpg":
                    Top_Left_Jpg = Source;
                    break;

                case "top_right.jpg":
                    Top_Right_Jpg = Source;
                    break;

                case "track2.gif":
                    Track2_Gif = Source;
                    break;

                case "trash01.ico":
                    Trash01_Ico = Source;
                    break;

                case "twitter_share.gif":
                    Twitter_Share_Gif = Source;
                    break;

                case "twitter_share_h.gif":
                    Twitter_Share_H_Gif = Source;
                    break;

                case "ufdc_banner_700.jpg":
                    Ufdc_Banner_700_Jpg = Source;
                    break;

                case "ui-icons_ffffff_256x240.png":
                    Ui_Icons_Ffffff_256X240_Png = Source;
                    break;

                case "uploadifive.css":
                    Uploadifive_Css = Source;
                    break;

                case "uploadify.css":
                    Uploadify_Css = Source;
                    break;

                case "uploadify.swf":
                    Uploadify_Swf = Source;
                    break;

                case "usage.png":
                    Usage_Png = Source;
                    break;

                case "usage_statistics.png":
                    Usage_Statistics_Png = Source;
                    break;

                case "users.gif":
                    Users_Gif = Source;
                    break;

                case "users.png":
                    Users_Png = Source;
                    break;

                case "view.ico":
                    View_Ico = Source;
                    break;

                case "view_work_log.png":
                    View_Work_Log_Png = Source;
                    break;

                case "view_work_log_icon.png":
                    View_Work_Log_Icon_Png = Source;
                    break;

                case "wizard.png":
                    Wizard_Png = Source;
                    break;

                case "wordmarks.gif":
                    Wordmarks_Gif = Source;
                    break;

                case "wrench.png":
                    Wrench_Png = Source;
                    break;

                case "yahoo_share.gif":
                    Yahoo_Share_Gif = Source;
                    break;

                case "yahoo_share_h.gif":
                    Yahoo_Share_H_Gif = Source;
                    break;

                case "yahoobuzz_share.gif":
                    Yahoobuzz_Share_Gif = Source;
                    break;

                case "yahoobuzz_share_h.gif":
                    Yahoobuzz_Share_H_Gif = Source;
                    break;

                case "zoom_tool.cur":
                    Zoom_Tool_Cur = Source;
                    break;

                case "zoomin.png":
                    Zoomin_Png = Source;
                    break;

                case "zoomout.png":
                    Zoomout_Png = Source;
                    break;

            }
        }
        /// <summary> Read the indicated configuration file for these default statis resources </summary>
        /// <param name="ConfigFile"> Configuration file to read </param>
        /// <returns> TRUE if successful, otherwise FALSE </returns>
        public static bool Read_Config(string ConfigFile)
        {
            // Streams used for reading
            Stream readerStream = null;
            XmlTextReader readerXml = null;
            bool returnValue = true;
            string base_url = "[%BASEURL%]";

            try
            {
                // Open a link to the file
                readerStream = new FileStream(ConfigFile, FileMode.Open, FileAccess.Read);

                // Open a XML reader connected to the file
                readerXml = new XmlTextReader(readerStream);

                while (readerXml.Read())
                {
                    if (readerXml.NodeType == XmlNodeType.Element)
                    {
                        switch (readerXml.Name.ToLower())
                        {
                            case "default_resources":
                                if (readerXml.MoveToAttribute("baseUrl")) base_url = readerXml.Value;
                                break;

                            case "file":
                                string key = (readerXml.MoveToAttribute("key")) ? readerXml.Value.Trim() : null;
                                string source = (readerXml.MoveToAttribute("source")) ? readerXml.Value.Trim() : null;
                                if ((!String.IsNullOrEmpty(key)) && (!String.IsNullOrEmpty(source))) Add_File(key.ToLower(), source.Replace("[%BASEURL%]", base_url));
                                break;
                        }
                    }
                }
            }
            catch
            {
                returnValue = false;
            }
            finally
            {
                if (readerXml != null) readerXml.Close();
                if (readerStream != null) readerStream.Close();
            }

            return returnValue;
        }
    }
}
