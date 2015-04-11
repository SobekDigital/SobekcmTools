﻿#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SobekCM.Resource_Object;
using SobekCM.Resource_Object.Bib_Info;
using SobekCM.Resource_Object.Database;
using SobekCM.Resource_Object.Divisions;
using SobekCM.Resource_Object.Behaviors;
using SobekCM.Resource_Object.Metadata_File_ReaderWriters;
using SobekCM.Library.Application_State;
using SobekCM.Library.Citation.Template;
using SobekCM.Library.Configuration;
using SobekCM.Library.HTML;
using SobekCM.Library.MainWriters;
using SobekCM.Library.MemoryMgmt;
using SobekCM.Library.Navigation;
using SobekCM.Library.Skins;
using SobekCM.Library.Users;
using darrenjohnstone.net.FileUpload;

#endregion

namespace SobekCM.Library.MySobekViewer
{
    /// <summary> Class allows an authenticated user to submit a new digital resource online, using various possible templates </summary>
    /// <remarks> This class extends the <see cref="abstract_MySobekViewer"/> class.<br /><br />
    /// MySobek Viewers are used for registration and authentication with mySobek, as well as performing any task which requires
    /// authentication, such as online submittal, metadata editing, and system administrative tasks.<br /><br />
    /// During a valid html request, the following steps occur:
    /// <ul>
    /// <li>Application state is built/verified by the <see cref="Application_State.Application_State_Builder"/> </li>
    /// <li>Request is analyzed by the <see cref="Navigation.SobekCM_QueryString_Analyzer"/> and output as a <see cref="Navigation.SobekCM_Navigation_Object"/> </li>
    /// <li>Main writer is created for rendering the output, in his case the <see cref="Html_MainWriter"/> </li>
    /// <li>The HTML writer will create the necessary subwriter.  Since this action requires authentication, an instance of the  <see cref="MySobek_HtmlSubwriter"/> class is created. </li>
    /// <li>The mySobek subwriter creates an instance of this viewer for submitting a new digital resource </li>
    /// <li>This viewer uses the <see cref="SobekCM.Library.Citation.Template.Template"/> class to display the correct elements for editing </li>
    /// </ul></remarks>
    public class New_Group_And_Item_MySobekViewer : abstract_MySobekViewer
    {
        private DJAccessibleProgressBar DJAccessibleProgrssBar1;
        private DJFileUpload DJFileUpload1;
        private DJUploadController DJUploadController1;

        private readonly Aggregation_Code_Manager codeManager;
        private bool criticalErrorEncountered;
        private readonly int currentProcessStep;
        private readonly Dictionary<string, Wordmark_Icon> iconList;
        private SobekCM_Item item;
        private readonly Item_Lookup_Object itemList;
        private readonly Template template;
        private readonly string templateCode = "ir";
        private readonly string toolTitle;
        private readonly int totalTemplatePages;
        private readonly string userInProcessDirectory;
        private readonly List<string> validationErrors;
        private readonly SobekCM_Skin_Object webSkin;

        #region Constructor

        /// <summary> Constructor for a new instance of the New_Group_And_Item_MySobekViewer class </summary>
        /// <param name="User"> Authenticated user information </param>
        /// <param name="Current_Mode"> Mode / navigation information for the current request</param>
        /// <param name="Item_List"> Allows individual items to be retrieved by various methods as <see cref="Single_Item"/> objects.</param>
        /// <param name="Code_Manager"> Code manager contains the list of all valid aggregation codes </param>
        /// <param name="HTML_Skin"> HTML Web skin which controls the overall appearance of this digital library </param>
        /// <param name="Icon_Table"> Dictionary of all the wordmark/icons which can be tagged to the items </param>
        /// <param name="Translator"> Language support object which handles simple translational duties </param>
        /// <param name="Tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
        public New_Group_And_Item_MySobekViewer(User_Object User, 
                                                SobekCM_Navigation_Object Current_Mode, Item_Lookup_Object Item_List,
                                                Aggregation_Code_Manager Code_Manager,
                                                Dictionary<string, Wordmark_Icon> Icon_Table,
                                                SobekCM_Skin_Object HTML_Skin,
                                                Language_Support_Info Translator,
                                                Custom_Tracer Tracer)
            : base(User)
        {
            Tracer.Add_Trace("New_Group_And_Item_MySobekViewer.Constructor", String.Empty);

            // Save the parameters
            currentMode = Current_Mode;
            codeManager = Code_Manager;
            iconList = Icon_Table;
            webSkin = HTML_Skin;
            base.Translator = Translator;

            // If the user cannot submit items, go back
            if (!user.Can_Submit)
            {
                currentMode.My_Sobek_Type = My_Sobek_Type_Enum.Home;
                HttpContext.Current.Response.Redirect(currentMode.Redirect_URL());
            }
            itemList = Item_List;

            // Determine the in process directory for this
            if (user.UFID.Trim().Length > 0)
                userInProcessDirectory = SobekCM_Library_Settings.In_Process_Submission_Location + "\\" + user.UFID + "\\newgroup";
            else
                userInProcessDirectory = SobekCM_Library_Settings.In_Process_Submission_Location + "\\" + user.UserName.Replace(".","").Replace("@","") + "\\newgroup";

            // Handle postback for changing the template or project
            templateCode = user.Current_Template;
            if (currentMode.isPostBack)
            {
                string action1 = HttpContext.Current.Request.Form["action"];
                if ((action1 != null) && ((action1 == "template") || (action1 == "project")))
                {
                    string newvalue = HttpContext.Current.Request.Form["phase"];
                    if ((action1 == "template") && ( newvalue != templateCode ))
                    {
                        user.Current_Template = newvalue;
                        templateCode = user.Current_Template;
                        if (File.Exists(userInProcessDirectory + "\\agreement.txt"))
                            File.Delete(userInProcessDirectory + "\\agreement.txt");
                    }
                    if ((action1 == "project") && (newvalue != user.Current_Project))
                    {
                        user.Current_Project = newvalue;
                    }
                    HttpContext.Current.Session["item"] = null;
                }
            }

            // Load the template
            templateCode = user.Current_Template;
            template = Cached_Data_Manager.Retrieve_Template(templateCode, Tracer);
            if ( template != null )
            {
                Tracer.Add_Trace("New_Group_And_Item_MySobekViewer.Constructor", "Found template in cache");
            }
            else
            {
                Tracer.Add_Trace("New_Group_And_Item_MySobekViewer.Constructor", "Reading template");

                // Read this template
                Template_XML_Reader reader = new Template_XML_Reader();
                template = new Template();
                reader.Read_XML( SobekCM_Library_Settings.Base_MySobek_Directory + "templates\\" + templateCode + ".xml", template, true);

                // Add the current codes to this template
                template.Add_Codes(Code_Manager);

                // Save this into the cache
                Cached_Data_Manager.Store_Template(templateCode, template, Tracer);
            }

            // Determine the number of total template pages
            totalTemplatePages = template.InputPages_Count;
            if (template.Permissions_Agreement.Length > 0)
                totalTemplatePages++;
            if (template.Upload_Types != Template.Template_Upload_Types.None)
                totalTemplatePages++;

            // Determine the title for this template, or use a default
            toolTitle = template.Title;
            if (toolTitle.Length == 0)
                toolTitle = "Self-Submittal Tool";


            // Determine the current phase
            currentProcessStep = 1;
            if ((currentMode.My_Sobek_SubMode.Length > 0) && (Char.IsNumber(currentMode.My_Sobek_SubMode[0])))
            {
                Int32.TryParse(currentMode.My_Sobek_SubMode.Substring(0), out currentProcessStep);
            }

            // If this is process step 1 and there is no permissions statement in the template,
            // just go to step 2
            if ((currentProcessStep == 1) && (template.Permissions_Agreement.Length == 0))
            {
                // Delete any pre-existing agreement from an earlier aborted submission process
                if (File.Exists(userInProcessDirectory + "\\agreement.txt"))
                    File.Delete(userInProcessDirectory + "\\agreement.txt");

                // Skip the permissions step
                currentProcessStep = 2;
            }

            // If there is a boundary infraction here, go back to step 2
            if (currentProcessStep < 0)
                currentProcessStep = 2;
            if ((currentProcessStep > template.InputPages.Count + 1 ) && ( currentProcessStep != 8 ) && ( currentProcessStep != 9 ))
                currentProcessStep = 2;

            // If this is to enter a file or URL, and the template does not include this, skip over this step
            if (( currentProcessStep == 8 ) && ( template.Upload_Types == Template.Template_Upload_Types.None ))
            {
                // For now, just forward to the next phase
                currentMode.My_Sobek_SubMode = "9";
                HttpContext.Current.Response.Redirect( currentMode.Redirect_URL());
            }

            // Look for the item in the session, then directory, then just create a new one
            if (HttpContext.Current.Session["Item"] == null)
            {
                // Clear any old files (older than 24 hours) that are in the directory
                if (!Directory.Exists(userInProcessDirectory))
                    Directory.CreateDirectory(userInProcessDirectory);
                else
                {
                    // Anything older than a day should be deleted
                    string[] files = Directory.GetFiles(userInProcessDirectory);
                    foreach (string thisFile in files)
                    {
                        DateTime modifiedDate = ((new FileInfo(thisFile)).LastWriteTime);
                        if (DateTime.Now.Subtract(modifiedDate).TotalHours > (24 * 7))
                        {
                            try
                            {
                                File.Delete(thisFile);
                            }
                            catch(Exception )
                            {
                                // Unable to delete existing file in the user's folder.
                                // This is an error, but how to report it?
                            }
                        }
                    }
                }

                // First, look for an existing METS file
                string[] existing_mets = Directory.GetFiles(userInProcessDirectory, "*.mets*");
                if (existing_mets.Length > 0)
                {
                    Tracer.Add_Trace("New_Group_And_Item_MySobekViewer.Constructor", "Reading existing METS file<br />(" + existing_mets[0] + ")");
                    item = SobekCM_Item.Read_METS(existing_mets[0]);

                    // Set the visibility information from the template
                    item.Behaviors.IP_Restriction_Membership = template.Default_Visibility;
                }

                // If there is still no item, just create a new one
                if (item == null)
                {
                    // Build a new empty METS file
                    new_item( Tracer );
                }

                // Save this to the session state now
                HttpContext.Current.Session["Item"] = item;
            }
            else
            {
                Tracer.Add_Trace("New_Group_And_Item_MySobekViewer.Constructor", "Item found in session cache");
                item = (SobekCM_Item)HttpContext.Current.Session["Item"];
            }

            #region Handle any other post back requests

            // If this is post-back, handle it
            if (currentMode.isPostBack)
            {
                // If this is a request from stage 8, save the new labels and url first
                if (currentProcessStep == 8)
                {
                    string[] getKeys = HttpContext.Current.Request.Form.AllKeys;
                    string file_name_from_keys = String.Empty;
                    string label_from_keys = String.Empty;
                    foreach (string thisKey in getKeys)
                    {
                        if (thisKey.IndexOf("upload_file") == 0)
                        {
                            file_name_from_keys = HttpContext.Current.Request.Form[thisKey];
                        }
                        if (thisKey.IndexOf("upload_label") == 0)
                        {
                            label_from_keys = HttpContext.Current.Request.Form[thisKey];
                        }
                        if ((file_name_from_keys.Length > 0) && (label_from_keys.Length > 0))
                        {
                            HttpContext.Current.Session["file_" + file_name_from_keys.Trim()] = label_from_keys.Trim();
                            file_name_from_keys = String.Empty;
                            label_from_keys = String.Empty;
                        }

                        if (thisKey == "url_input")
                        {
                            item.Bib_Info.Location.Other_URL = HttpContext.Current.Request.Form[thisKey];
                        }
                    }
                }

                string action = HttpContext.Current.Request.Form["action"];
                if (action == "cancel")
                {
                    // Clear all files in the user process folder
                    try
                    {
                        string[] all_files = Directory.GetFiles(userInProcessDirectory);
                        foreach (string thisFile in all_files)
                            Directory.Delete(thisFile);
                        Directory.Delete(userInProcessDirectory);
                    }
                    catch (Exception)
                    {
                        // Unable to delete existing file in the user's folder.
                        // This is an error, but how to report it?
                    }

                    // Clear all the information in memory
                    HttpContext.Current.Session["agreement_date"] = null;
                    HttpContext.Current.Session["item"] = null;

                    // Clear any temporarily assigned current project and template
                    user.Current_Project = null;
                    user.Current_Template = null;

                    // Forward back to my Sobek home
                    currentMode.My_Sobek_Type = My_Sobek_Type_Enum.Home;
                    HttpContext.Current.Response.Redirect(currentMode.Redirect_URL());
                }

                if ( action == "delete" )
                {
                    string filename = HttpContext.Current.Request.Form["phase"];
                    try
                    {
                        if (File.Exists(userInProcessDirectory + "\\" + filename))
                            File.Delete(userInProcessDirectory + "\\" + filename);

                        // Forward
                        HttpContext.Current.Response.Redirect( currentMode.Redirect_URL());
                    }
                    catch (Exception)
                    {
                        // Unable to delete existing file in the user's folder.
                        // This is an error, but how to report it?
                    }
                }

                if (action == "clear")
                {
                    // If there is an old METS file, delete it
                    if (File.Exists(userInProcessDirectory + "\\TEMP000001_00001.mets"))
                        File.Delete(userInProcessDirectory + "\\TEMP000001_00001.mets");

                    // Create the new METS file and add to the session
                    new_item(null);
                    HttpContext.Current.Session["Item"] = item;

                    // Forward back to the same URL
                    currentMode.My_Sobek_SubMode = "2";
                    HttpContext.Current.Response.Redirect( currentMode.Redirect_URL());
                }

                if (action == "next_phase")
                {

                    string next_phase = HttpContext.Current.Request.Form["phase"];

                    // If this goes from step 1 to step 2, write the permissions first
                    if ((currentProcessStep == 1) && (next_phase == "2") && ( template.Permissions_Agreement.Length > 0 ))
                    {
                        // Store this agreement in the session state
                        DateTime agreement_date = DateTime.Now;
                        HttpContext.Current.Session["agreement_date"] = agreement_date;

                        // Also, save this as a text file
                        string agreement_file = userInProcessDirectory + "\\agreement.txt";
                        StreamWriter writer = new StreamWriter(agreement_file, false);
                        writer.WriteLine("Permissions Agreement");
                        writer.WriteLine();
                        writer.WriteLine("User: " + user.Full_Name + " ( " + user.UFID + " )");
                        writer.WriteLine("Date: " + agreement_date.ToString());
                        writer.WriteLine();
                        writer.WriteLine(template.Permissions_Agreement);
                        writer.Flush();
                        writer.Close();
                    }

                    // If this is going from a step that includes the metadata entry portion, save this to the item
                    if ((currentProcessStep > 1) && (currentProcessStep < 8))
                    {
                        // Save to the item
                        template.Save_To_Bib(item, user, currentProcessStep - 1);
                        item.Save_METS();
                        HttpContext.Current.Session["Item"] = item;

                        // Save the pertinent data to the METS file package
                        item.METS_Header.Create_Date = DateTime.Now;
                        if ((HttpContext.Current.Session["agreement_date"] != null) && (HttpContext.Current.Session["agreement_date"].ToString().Length > 0))
                        {
                            DateTime asDateTime;
                            if (DateTime.TryParse(HttpContext.Current.Session["agreement_date"].ToString(), out asDateTime))
                                item.METS_Header.Create_Date = asDateTime;
                        }
                        HttpContext.Current.Session["Item"] = item;

                        // Save this item, just in case it gets lost somehow
                        item.Source_Directory = userInProcessDirectory;
                        string acquisition_append = "Submitted by " + user.Full_Name + ".";
                        if (item.Bib_Info.Notes_Count > 0)
                        {
                            foreach (Note_Info thisNote in item.Bib_Info.Notes.Where(thisNote => thisNote.Note_Type == Note_Type_Enum.acquisition))
                            {
                                if (thisNote.Note.IndexOf(acquisition_append) < 0)
                                    thisNote.Note = thisNote.Note.Trim() + "  " + acquisition_append;
                                break;
                            }
                        }

                        // Also, check all the authors to add the current users attribution information
                        if (user.Organization.Length > 0)
                        {
                            if ((item.Bib_Info.Main_Entity_Name.Full_Name.IndexOf(user.Family_Name) >= 0) && ((item.Bib_Info.Main_Entity_Name.Full_Name.IndexOf(user.Given_Name) >= 0) || ((user.Nickname.Length > 2) && (item.Bib_Info.Main_Entity_Name.Full_Name.IndexOf(user.Nickname) > 0))))
                            {
                                item.Bib_Info.Main_Entity_Name.Affiliation = user.Organization;
                                if (user.College.Length > 0)
                                    item.Bib_Info.Main_Entity_Name.Affiliation = item.Bib_Info.Main_Entity_Name.Affiliation + " -- " + user.College;
                                if (user.Department.Length > 0)
                                    item.Bib_Info.Main_Entity_Name.Affiliation = item.Bib_Info.Main_Entity_Name.Affiliation + " -- " + user.Department;
                                if (user.Unit.Length > 0)
                                    item.Bib_Info.Main_Entity_Name.Affiliation = item.Bib_Info.Main_Entity_Name.Affiliation + " -- " + user.Unit;
                            }
                            if (item.Bib_Info.Names_Count > 0)
                            {
                                foreach (Name_Info thisName in item.Bib_Info.Names)
                                {
                                    if ((thisName.Full_Name.IndexOf(user.Family_Name) >= 0) && ((thisName.Full_Name.IndexOf(user.Given_Name) >= 0) || ((user.Nickname.Length > 2) && (thisName.Full_Name.IndexOf(user.Nickname) > 0))))
                                    {
                                        thisName.Affiliation = user.Organization;
                                        if (user.College.Length > 0)
                                            thisName.Affiliation = thisName.Affiliation + " -- " + user.College;
                                        if (user.Department.Length > 0)
                                            thisName.Affiliation = thisName.Affiliation + " -- " + user.Department;
                                        if (user.Unit.Length > 0)
                                            thisName.Affiliation = thisName.Affiliation + " -- " + user.Unit;

                                    }
                                }
                            }
                        }
                        item.Save_METS();
                        HttpContext.Current.Session["Item"] = item;
                    }

                    // For now, just forward to the next phase
                    currentMode.My_Sobek_SubMode = next_phase;
                    HttpContext.Current.Response.Redirect( currentMode.Redirect_URL());
                }
            }

            #endregion

            #region Perform some validation to determine if the user should be at this step

            // If this is past the agreement phase, check that an agreement exists
            if (currentProcessStep > 1)
            {
                // Validate that an agreement.txt file exists, if the template has permissions
                if (( template.Permissions_Agreement.Length > 0 ) && (!File.Exists(userInProcessDirectory + "\\agreement.txt")))
                {
                    currentMode.My_Sobek_SubMode = "1";
                    HttpContext.Current.Response.Redirect( currentMode.Redirect_URL());
                }

                // Get the validation errors
                validationErrors = new List<string>();
                SobekCM_Item_Validator.Validate_SobekCM_Item(item, validationErrors);
            }

            // If this is to put up items or complete the item, validate the METS
            if ( currentProcessStep >= 8 )
            {
                // Validate that a METS file exists
                if (Directory.GetFiles( userInProcessDirectory, "*.mets*").Length == 0 )
                {
                    currentMode.My_Sobek_SubMode = "2";
                    HttpContext.Current.Response.Redirect( currentMode.Redirect_URL());
                }

                // Get the validation errors
                if ( validationErrors.Count == 0 )
                    item.Save_METS();
                else
                {
                    item.Web.Show_Validation_Errors = true;
                    currentMode.My_Sobek_SubMode = "2";
                    HttpContext.Current.Response.Redirect( currentMode.Redirect_URL());
                }
            }

            // If this is for step 8, ensure that this even takes this information, or go to step 9
            if (( currentProcessStep == 8 ) && ( template.Upload_Types == Template.Template_Upload_Types.None ))
            {
                currentMode.My_Sobek_SubMode = "9";
                HttpContext.Current.Response.Redirect( currentMode.Redirect_URL());
            }

            // If this is going into the last process step, check that any mandatory info (file, url, .. ) 
            // from the last step is present
            if ( currentProcessStep == 9 )
            {
                // Only check if this is mandatory
                if (( template.Upload_Mandatory ) && ( template.Upload_Types != Template.Template_Upload_Types.None ))
                {
                    // Does this require either a FILE or URL?
                    bool required_file_present = false;
                    bool required_url_present = false;

                    // If this accepts files, check for acceptable files
                    if (( template.Upload_Types == Template.Template_Upload_Types.File ) || ( template.Upload_Types == Template.Template_Upload_Types.URL ))
                    {
                        // Get list of files in this package
                        string[] all_files = Directory.GetFiles(userInProcessDirectory);
                        List<string> acceptable_files = all_files.Where(thisFile => (thisFile.IndexOf("agreement.txt") < 0) && (thisFile.IndexOf("TEMP000001_00001.mets") < 0)).ToList();

                        // Acceptable files found?
                        if ( acceptable_files.Count > 0 )
                            required_file_present = true;
                    }

                    // If this accepts URLs, check for a URL
                    if (( template.Upload_Types == Template.Template_Upload_Types.URL ) || ( template.Upload_Types == Template.Template_Upload_Types.File_or_URL ))
                    {
                        if ( item.Bib_Info.Location.Other_URL.Length > 0 )
                        {
                            required_url_present = true;
                        }
                    }

                    // If neither was present, go back to step 8
                    if (( !required_file_present ) && ( !required_url_present ))
                    {
                        currentMode.My_Sobek_SubMode = "8";
                        HttpContext.Current.Response.Redirect( currentMode.Redirect_URL());
                    }
                }

                // Complete the item submission
                complete_item_submission( item,  Tracer );
            }

            #endregion
        }

        #endregion

        #region Method commpletes the item submission on the way to the congratulations screen

        private bool complete_item_submission(SobekCM_Item Item_To_Complete,  Custom_Tracer Tracer )
        {
            // Set an initial flag 
            criticalErrorEncountered = false;

            string[] all_files = Directory.GetFiles(userInProcessDirectory);
            SortedList<string, List<string>> image_files = new SortedList<string, List<string>>();
            SortedList<string, List<string>> download_files = new SortedList<string, List<string>>();
            foreach (string thisFile in all_files)
            {
                FileInfo thisFileInfo = new FileInfo(thisFile);

                if ((thisFileInfo.Name.IndexOf("agreement.txt") != 0) && (thisFileInfo.Name.IndexOf("TEMP000001_00001.mets") != 0) && (thisFileInfo.Name.IndexOf("doc.xml") != 0) && (thisFileInfo.Name.IndexOf("ufdc_mets.xml") != 0) && (thisFileInfo.Name.IndexOf("marc.xml") != 0))
                {
                    // Get information about this files name and extension
                    string extension_upper = thisFileInfo.Extension.ToUpper();
                    string filename_sans_extension = thisFileInfo.Name.Replace(thisFileInfo.Extension, "");
                    string name_upper = thisFileInfo.Name.ToUpper();

                    // Is this a page image?
                    if ((extension_upper == ".JPG") || (extension_upper == ".TIF") || (extension_upper == ".JP2") || (extension_upper == ".JPX"))
                    {
                        // Exclude .QC.jpg files
                        if (name_upper.IndexOf(".QC.JPG") < 0)
                        {
                            // If this is a thumbnail, trim off the THM part on the file name
                            if (name_upper.IndexOf("THM.JPG") > 0)
                            {
                                filename_sans_extension = filename_sans_extension.Substring(0, filename_sans_extension.Length - 3);
                            }

                            // Is this the first image file with this name?
                            if (image_files.ContainsKey(filename_sans_extension.ToLower()))
                            {
                                image_files[filename_sans_extension.ToLower()].Add(thisFileInfo.Name);
                            }
                            else
                            {
                                List<string> newImageGrouping = new List<string> {thisFileInfo.Name};
                                image_files[filename_sans_extension.ToLower()] = newImageGrouping;
                            }
                        }
                    }
                    else
                    {
                        // If this does not match the exclusion regular expression, than add this
                        if (!Regex.Match(thisFileInfo.Name, SobekCM_Library_Settings.Files_To_Exclude_From_Downloads, RegexOptions.IgnoreCase).Success)
                        {
                            // Is this the first image file with this name?
                            if (download_files.ContainsKey(filename_sans_extension.ToLower()))
                            {
                                download_files[filename_sans_extension.ToLower()].Add(thisFileInfo.Name);
                            }
                            else
                            {
                                List<string> newDownloadGrouping = new List<string> {thisFileInfo.Name};
                                download_files[filename_sans_extension.ToLower()] = newDownloadGrouping;
                            }
                        }
                    }
                }
            }

            // This package is good to go, so build it, save, etc...
            try
            {
                // Save the METS file to the database and back to the directory
                Item_To_Complete.Source_Directory = userInProcessDirectory;

                // Step through and add each file 
                Item_To_Complete.Divisions.Download_Tree.Clear();
                if ((template.Upload_Types == Template.Template_Upload_Types.File_or_URL) || (template.Upload_Types == Template.Template_Upload_Types.File))
                {
                    // Step through each file

                    bool error_reading_file_occurred = false;

                    // Add the image files first
                    bool jpeg_added = false;
                    bool jp2_added = false;
                    foreach(string thisFileKey in image_files.Keys )
                    {
                        // Get the list of files
                        List<string> theseFiles = image_files[thisFileKey];

                        // Add each file
                        foreach (string thisFile in theseFiles)
                        {
                            // Create the new file object and compute a label
                            FileInfo fileInfo = new FileInfo(thisFile);
                            SobekCM_File_Info newFile = new SobekCM_File_Info(fileInfo.Name);
                            string label = fileInfo.Name.Replace(fileInfo.Extension, "");
                            if (HttpContext.Current.Session["file_" + thisFileKey] != null)
                            {
                                string possible_label = HttpContext.Current.Session["file_" + thisFileKey].ToString();
                                if (possible_label.Length > 0)
                                    label = possible_label;
                            }

                            // Add this file
                            Item_To_Complete.Divisions.Physical_Tree.Add_File(newFile, label);

                            // Seperate code for JP2 and JPEG type files
                            string extension = fileInfo.Extension.ToUpper();
                            if (extension.IndexOf("JP2") >= 0)
                            {
                                if (!error_reading_file_occurred)
                                {
                                    if (!newFile.Compute_Jpeg2000_Attributes(userInProcessDirectory))
                                        error_reading_file_occurred = true;
                                }
                                jp2_added = true;
                            }
                            else
                            {
                                if (!error_reading_file_occurred)
                                {
                                    if (!newFile.Compute_Jpeg_Attributes(userInProcessDirectory))
                                        error_reading_file_occurred = true;
                                }
                                jpeg_added = true;
                            }
                        }
                    }

                    // Add the download files next
                    foreach(string thisFileKey in download_files.Keys )
                    {
                        // Get the list of files
                        List<string> theseFiles = download_files[thisFileKey];

                        // Add each file
                        foreach (string thisFile in theseFiles)
                        {
                            // Create the new file object and compute a label
                            FileInfo fileInfo = new FileInfo(thisFile);
                            SobekCM_File_Info newFile = new SobekCM_File_Info(fileInfo.Name);
                            string label = fileInfo.Name.Replace( fileInfo.Extension, "");
                            if (HttpContext.Current.Session["file_" + thisFileKey] != null)
                            {
                                string possible_label = HttpContext.Current.Session["file_" + thisFileKey].ToString();
                                if (possible_label.Length > 0)
                                    label = possible_label;
                            }

                            // Add this file
                            Item_To_Complete.Divisions.Download_Tree.Add_File(newFile, label);
                        }
                    }

                    // Add the JPEG2000 and JPEG-specific viewers
                    Item_To_Complete.Behaviors.Clear_Views();
                    if (jpeg_added)
                    {
                        Item_To_Complete.Behaviors.Add_View(View_Enum.JPEG);
                    }
                    if (jp2_added)
                    {
                        Item_To_Complete.Behaviors.Add_View(View_Enum.JPEG2000);
                    }
                }

                // Determine the total size of the package before saving
                string[] all_files_final = Directory.GetFiles(userInProcessDirectory);
                double size = all_files_final.Aggregate<string, double>(0, (current, thisFile) => current + (((new FileInfo(thisFile)).Length)/1024));
                Item_To_Complete.DiskSize_KB = size;

                // BibID and VID will be automatically assigned
                Item_To_Complete.BibID = template.BibID_Root;
                Item_To_Complete.VID = String.Empty;

                // Set some values in the tracking portion
                if (Item_To_Complete.Divisions.Files.Count > 0)
                {
                    Item_To_Complete.Tracking.Born_Digital = true;
                }
                Item_To_Complete.Tracking.VID_Source = "SobekCM:" + templateCode;

                // Save to the database
                try
                {
                    SobekCM_Database.Save_New_Digital_Resource(Item_To_Complete, false, true, user.UserName, String.Empty, user.UserID);                    
                }
                catch (Exception ee)
                {
                    StreamWriter writer = new StreamWriter(userInProcessDirectory + "\\exception.txt", false);
                    writer.WriteLine( "ERROR CAUGHT WHILE SAVING NEW DIGITAL RESOURCE");
                    writer.WriteLine( DateTime.Now.ToString());
                    writer.WriteLine();
                    writer.WriteLine( ee.Message );
                    writer.WriteLine( ee.StackTrace );
                    writer.Flush();
                    writer.Close();
                    throw;
                }


                // Assign the file root and assoc file path
                Item_To_Complete.Web.File_Root = Item_To_Complete.BibID.Substring(0, 2) + "\\" + Item_To_Complete.BibID.Substring(2, 2) + "\\" + Item_To_Complete.BibID.Substring(4, 2) + "\\" + Item_To_Complete.BibID.Substring(6, 2) + "\\" + Item_To_Complete.BibID.Substring(8, 2);
                Item_To_Complete.Web.AssocFilePath = Item_To_Complete.Web.File_Root + "\\" + Item_To_Complete.VID + "\\";

                // Create the static html pages
                string base_url = currentMode.Base_URL;
                try
                {
                    Static_Pages_Builder staticBuilder = new Static_Pages_Builder(SobekCM_Library_Settings.System_Base_URL, SobekCM_Library_Settings.Base_Data_Directory, Translator, codeManager, itemList, iconList, webSkin);
                    string filename = userInProcessDirectory + "\\" + Item_To_Complete.BibID + "_" + Item_To_Complete.VID + ".html";
                    staticBuilder.Create_Item_Citation_HTML(Item_To_Complete, filename, String.Empty);
                }
                catch (Exception)
                {
                    // An error here is not catastrophic
                }

                currentMode.Base_URL = base_url;

                // Save the rest of the metadata
                Item_To_Complete.Save_SobekCM_METS();
                Item_To_Complete.Save_Citation_Only_METS();

                // Add this to the cache
                itemList.Add_SobekCM_Item(Item_To_Complete);

                //// Link this item and user
                //Database.SobekCM_Database.Add_User_Item_Link(user.UserID, item.Web.ItemID, 1, true);
                //Database.SobekCM_Database.Add_User_BibID_Link(user.UserID, item.Behaviors.GroupID);
                //Database.SobekCM_Database.Add_Item_To_User_Folder(user.UserID, "Submitted Items", item.BibID, item.VID, 0, String.Empty, Tracer);

                // Save Bib_Level METS?
                //SobekCM.Resource_Object.Writers.OAI_Writer oaiWriter = new SobekCM.Resource_Object.Writers.OAI_Writer();
                //oaiWriter.Save_OAI_File(bibPackage, resource_folder + "\\oai_dc.xml", bibPackage.Processing_Parameters.Collection_Primary.ToLower(), createDate);


                List<string> collectionnames = new List<string>();
                //// Get the collection names
                //if (item.Processing_Parameters.Collection_Primary.Length > 0)
                //{
                //    DataRow[] primCode = Collection_Codes.Select("collectioncode = '" + item.Processing_Parameters.Collection_Primary + "'");
                //    if (primCode.Length > 0)
                //    {
                //        collectionnames.Add(primCode[0]["ShortName"].ToString());
                //    }
                //}
                //foreach (string altCollection in bibPackage.Processing_Parameters.Collections_Alternate)
                //{
                //    DataRow[] altCode = Collection_Codes.Select("collectioncode = '" + altCollection + "'");
                //    if (altCode.Length > 0)
                //    {
                //        collectionnames.Add(altCode[0]["ShortName"].ToString());
                //    }
                //}
                // Save the marc xml file
                MarcXML_File_ReaderWriter marcWriter = new MarcXML_File_ReaderWriter();
                string Error_Message;
                Dictionary<string, object> options = new Dictionary<string, object>();
                options["MarcXML_File_ReaderWriter:Additional_Tags"] = Item_To_Complete.MARC_Sobek_Standard_Tags(collectionnames, true, SobekCM_Library_Settings.System_Name, SobekCM_Library_Settings.System_Abbreviation);
                marcWriter.Write_Metadata(Item_To_Complete.Source_Directory + "\\marc.xml", Item_To_Complete, options, out Error_Message);

                // Delete the TEMP mets file
                if (File.Exists(userInProcessDirectory + "\\TEMP000001_00001.mets"))
                    File.Delete(userInProcessDirectory + "\\TEMP000001_00001.mets");

                // Rename the METS file to the XML file                
                if ((!File.Exists(userInProcessDirectory + "\\" + Item_To_Complete.BibID + "_" + Item_To_Complete.VID + ".mets.xml")) &&
                    (File.Exists(userInProcessDirectory + "\\" + Item_To_Complete.BibID + "_" + Item_To_Complete.VID + ".mets")))
                {
                    File.Move(userInProcessDirectory + "\\" + Item_To_Complete.BibID + "_" + Item_To_Complete.VID + ".mets", userInProcessDirectory + "\\" + Item_To_Complete.BibID + "_" + Item_To_Complete.VID + ".mets.xml");
                }

                // Copy this to all the image servers
                SobekCM_Library_Settings.Refresh(Database.SobekCM_Database.Get_Settings_Complete(Tracer));
                string[] allFiles = Directory.GetFiles(userInProcessDirectory);


                string serverNetworkFolder = SobekCM_Library_Settings.Image_Server_Network + Item_To_Complete.Web.AssocFilePath;

                // Create the folder
                if (!Directory.Exists(serverNetworkFolder))
                    Directory.CreateDirectory(serverNetworkFolder);

                foreach (string thisFile in allFiles)
                {
                    string destination_file = serverNetworkFolder + "\\" + (new FileInfo(thisFile)).Name;
                    File.Copy(thisFile, destination_file, true);
                }

                // Copy the static HTML file as well
                try
                {
                    if (!Directory.Exists(SobekCM_Library_Settings.Static_Pages_Location + Item_To_Complete.BibID.Substring(0, 2) + "\\" + Item_To_Complete.BibID.Substring(2, 2) + "\\" + Item_To_Complete.BibID.Substring(4, 2) + "\\" + Item_To_Complete.BibID.Substring(6, 2) + "\\" + Item_To_Complete.BibID.Substring(8)))
                        Directory.CreateDirectory(SobekCM_Library_Settings.Static_Pages_Location + Item_To_Complete.BibID.Substring(0, 2) + "\\" + Item_To_Complete.BibID.Substring(2, 2) + "\\" + Item_To_Complete.BibID.Substring(4, 2) + "\\" + Item_To_Complete.BibID.Substring(6, 2) + "\\" + Item_To_Complete.BibID.Substring(8));
                    if (File.Exists(userInProcessDirectory + "\\" + Item_To_Complete.BibID + "_" + Item_To_Complete.VID + ".html"))
                        File.Copy(userInProcessDirectory + "\\" + Item_To_Complete.BibID + "_" + Item_To_Complete.VID + ".html", SobekCM_Library_Settings.Static_Pages_Location + Item_To_Complete.BibID.Substring(0, 2) + "\\" + Item_To_Complete.BibID.Substring(2, 2) + "\\" + Item_To_Complete.BibID.Substring(4, 2) + "\\" + Item_To_Complete.BibID.Substring(6, 2) + "\\" + Item_To_Complete.BibID.Substring(8) + "\\" + Item_To_Complete.BibID + "_" + Item_To_Complete.VID + ".html", true);
                }
                catch (Exception)
                {
                    // An error here is not catastrophic
                }

                // Add this to the cache
                itemList.Add_SobekCM_Item(Item_To_Complete);

                // Incrememnt the count of number of items submitted by this user
                user.Items_Submitted_Count++;
                if (!user.BibIDs.Contains(Item_To_Complete.BibID))
                    user.Add_BibID(Item_To_Complete.BibID);


                // Now, delete all the files here
                all_files = Directory.GetFiles(userInProcessDirectory);
                foreach (string thisFile in all_files)
                {
                    File.Delete(thisFile);
                }

                // Finally, set the item for more processing if there were any files
                if (((image_files.Count > 0) || (download_files.Count > 0)) && ( Item_To_Complete.Web.ItemID > 0 ))
                {
                    Database.SobekCM_Database.Update_Additional_Work_Needed_Flag(Item_To_Complete.Web.ItemID, true, Tracer);
                }

                // Clear any temporarily assigned current project and template
                user.Current_Project = null;
                user.Current_Template = null;

            }
            catch (Exception ee)
            {
                validationErrors.Add("Error encountered during item save!");
                validationErrors.Add(ee.ToString().Replace("\r", "<br />"));

                // Set an initial flag 
                criticalErrorEncountered = true;

                string error_body = "<strong>ERROR ENCOUNTERED DURING ONLINE SUBMITTAL PROCESS</strong><br /><br /><blockquote>Title: " + Item_To_Complete.Bib_Info.Main_Title.Title + "<br />Permanent Link: <a href=\"" + currentMode.Base_URL + "/" + Item_To_Complete.BibID + "/" + Item_To_Complete.VID + "\">" + currentMode.Base_URL + "/" + Item_To_Complete.BibID + "/" + Item_To_Complete.VID + "</a><br />User: " + user.Full_Name + "<br /><br /></blockquote>" + ee.ToString().Replace("\n", "<br />");
                string error_subject = "Error during submission for '" + Item_To_Complete.Bib_Info.Main_Title.Title + "'";
                string email_to = SobekCM_Library_Settings.System_Error_Email;
                if (email_to.Length == 0)
                    email_to = SobekCM_Library_Settings.System_Email;
                Database.SobekCM_Database.Send_Database_Email(email_to, error_subject, error_body, true, false, -1);
            }

            if (!criticalErrorEncountered)
            {
                // Send email to the email from the template, if one was provided

                if (template.Email_Upon_Receipt.Length > 0)
                {
                    string body = "New item submission complete!<br /><br /><blockquote>Title: " + Item_To_Complete.Bib_Info.Main_Title.Title + "<br />Submittor: " + user.Full_Name + " ( " + user.Email + " )<br />Link: <a href=\"" + currentMode.Base_URL + "/" + Item_To_Complete.BibID + "/" + Item_To_Complete.VID + "\">" + Item_To_Complete.BibID + ":" + Item_To_Complete.VID + "</a></blockquote>";
                    string subject = "Item submission complete for '" + Item_To_Complete.Bib_Info.Main_Title.Title + "'";
                    Database.SobekCM_Database.Send_Database_Email(template.Email_Upon_Receipt, subject, body, true, false, -1);
                }

                // If the user wants to have a message sent, send one
                if (user.Send_Email_On_Submission)
                {
                    // Create the mail message
                    string body2 = "<strong>CONGRATULATIONS!</strong><br /><br />Your item has been successfully added to the digital library and will appear immediately.  Search indexes may take a couple minutes to build, at which time this item will be discoverable through the search interface. <br /><br /><blockquote>Title: " + Item_To_Complete.Bib_Info.Main_Title.Title + "<br />Permanent Link: <a href=\"" + currentMode.Base_URL + "/" + Item_To_Complete.BibID + "/" + Item_To_Complete.VID + "\">" + currentMode.Base_URL + "/" + Item_To_Complete.BibID + "/" + Item_To_Complete.VID + "</a></blockquote>";
                    string subject2 = "Item submission complete for '" + Item_To_Complete.Bib_Info.Main_Title.Title + "'";
                    Database.SobekCM_Database.Send_Database_Email(user.Email, subject2, body2, true, false, -1 );
                }
            }

            return criticalErrorEncountered;
        }

        #endregion

        /// <summary> Gets the banner from the current template, if there is one </summary>
        public string Current_Template_Banner
        {
            get {
                return template != null ? template.Banner : String.Empty;
            }
        }

        /// <summary> Title for the page that displays this viewer, this is shown in the search box at the top of the page, just below the banner </summary>
        /// <value> This returns the title of the current <see cref="SobekCM.Library.Citation.Template.Template"/> object </value>
        public override string Web_Title
        {
            get 
            {
                return toolTitle;
            }
        }

        #region Methods to write the HTML directly to the output stream

        /// <summary> Add the HTML to be displayed in the main SobekCM viewer area </summary>
        /// <param name="Output"> Textwriter to write the HTML for this viewer</param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
        /// <remarks> This adds the template HTML for step 2 and the congratulations text for step 4 </remarks>
        public override void Write_HTML(TextWriter Output, Custom_Tracer Tracer)
        {
            Tracer.Add_Trace("New_Group_And_Item_MySobekViewer.Write_HTML", "Do nothing");

            if (currentProcessStep == 8)
            {
                Output.WriteLine("<script src=\"" + currentMode.Base_URL + "default/scripts/sobekcm_metadata.js\" type=\"text/javascript\"></script>");
                Output.WriteLine("<div class=\"SobekHomeText\">");
                Output.WriteLine("<br />");
                Output.Write("<strong>Step " + totalTemplatePages + " of " + totalTemplatePages + ": ");
                string explanation = String.Empty;
                switch( template.Upload_Types)
                {
                    case Template.Template_Upload_Types.File:
                        Output.Write("Upload Files");
                        explanation = "Upload the related files for your new item.  You can also provide labels for each file, once they are uploaded.";
                        break;

                    case Template.Template_Upload_Types.File_or_URL:
                        Output.Write("Upload Files or Enter URL");
                        explanation = "Upload the related files or enter a URL for your new item.  You can also provide labels for each file, once they are uploaded.";
                        break;

                    case Template.Template_Upload_Types.URL:
                        explanation = "Enter a URL for this new item.";
                        Output.Write("Enter URL");
                        break;
                }
                Output.WriteLine(template.Upload_Mandatory
                                     ? " ( <i>Required</i> )</strong><br />"
                                     : " ( Optional )</strong><br />");
                Output.WriteLine("<blockquote>" + explanation + "</blockquote><br />"); 
            }
        }

        /// <summary> This is an opportunity to write HTML directly into the main form, without
        /// using the pop-up html form architecture </summary>
        /// <param name="Output"> Textwriter to write the pop-up form HTML for this viewer </param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
        /// <remarks> This text will appear within the ItemNavForm form tags </remarks>
        public override void Add_HTML_In_Main_Form(TextWriter Output, Custom_Tracer Tracer)
        {
            if (Tracer != null)
            {
                Tracer.Add_Trace("New_Group_And_Item_MySobekViewer.Add_HTML_In_Main_Form", "");
            }

            string templateLabel = "Template";
            string projectLabel = "Project";
            const string col1Width = "15px";
            const string col2Width = "80px";
            const string col3Width = "625px";

            if (currentMode.Language == Web_Language_Enum.French)
            {
                templateLabel = "Modèle";
                projectLabel = "Projet";
            }

            if (currentMode.Language == Web_Language_Enum.Spanish)
            {
                templateLabel = "Plantilla";
                projectLabel = "Proyecto";
            }

            // Add the hidden fields first
            Output.WriteLine("<!-- Hidden field is used for postbacks to indicate what to save and reset -->");
            Output.WriteLine("<input type=\"hidden\" id=\"action\" name=\"action\" value=\"\" />");
            Output.WriteLine("<input type=\"hidden\" id=\"phase\" name=\"phase\" value=\"\" />");
            Output.WriteLine("<script type=\"text/javascript\" src=\"" + currentMode.Base_URL + "default/scripts/sobekcm_metadata.js\" ></script>");
            Output.WriteLine("<script type=\"text/javascript\" src=\"" + currentMode.Base_URL + "default/scripts/sobekcm_form.js\" ></script>");

            #region Add the agreement HTML for the first step

            if (currentProcessStep == 1)
            {
                Output.WriteLine("<div class=\"SobekHomeText\" >");
                Output.WriteLine("<br />");
                if (template.Permissions_Agreement.Length > 0)
                {
                    Output.WriteLine("<strong>Step 1 of " + totalTemplatePages + ": Grant of Permission</strong><br />");

                    Output.WriteLine("<blockquote>You must read and accept the below permissions to continue.<br /><br />");
                    Output.WriteLine(template.Permissions_Agreement.Replace("<%BASEURL%>", currentMode.Base_URL));
               //     Output.WriteLine("<p>Please review the <a href=\"?g=ufirg&amp;m=hitauthor_faq#policies&amp;n=gs\">Policies</A> if you have any questions or please contact us with any questions prior to submitting files. </p>\n");
                    Output.WriteLine("<table width=\"700\">");
                    Output.WriteLine("  <tr align=\"right\">");
                    Output.WriteLine("    <td>You must read and accept the above permissions agreement to continue. &nbsp; &nbsp; </td>");
                    Output.WriteLine("    <td>");
                    Output.WriteLine("      <a href=\"\" onclick=\"return new_item_cancel();\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/cancel_button_g.gif\" alt=\"CANCEL\" /></a>");
                    Output.WriteLine("      &nbsp; &nbsp; ");
                    Output.WriteLine("      <a href=\"\" onclick=\"return new_item_next_phase(2);\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/accept_button_22.gif\" alt=\"ACCEPT\" /></a>");
                    Output.WriteLine("    </td>");
                    Output.WriteLine("  </tr>");
                    Output.WriteLine("</table>");

                }
                else
                {
                    Output.WriteLine("<strong>Step 1 of " + totalTemplatePages + ": Confirm Template and Project</strong><br />");
                    Output.WriteLine("<blockquote>");
                    Output.WriteLine("<table width=\"720\">");
                    Output.WriteLine("  <tr>");
                    Output.WriteLine("    <td width=\"450px\">&nbsp;</td>");
                    Output.WriteLine("    <td>");
                    Output.WriteLine("      <a href=\"\" onclick=\"return new_item_cancel();\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/cancel_button_g.gif\" alt=\"CANCEL\" /></a>");
                    Output.WriteLine("      &nbsp; &nbsp; ");
                    Output.WriteLine("      <a href=\"\" onclick=\"return new_item_next_phase(2);\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/accept_button_22.gif\" alt=\"ACCEPT\" /></a>");
                    Output.WriteLine("    </td>");
                    Output.WriteLine("  </tr>");
                    Output.WriteLine("</table>");
                }

                if ((user.Templates.Count > 1) || (user.Projects.Count > 1))
                {
                    string changeable = "project and template";
                    if (user.Projects.Count == 0)
                        changeable = "project";
                    if (user.Templates.Count == 0)
                        changeable = "template";

                    string current_template = user.Current_Template;
                    string current_project = user.Current_Project;

                    Output.WriteLine("<br />");
                    Output.WriteLine("<hr />");
                    Output.WriteLine("You may also change your current " + changeable + " for this submission.");
                    Output.WriteLine("<table width=\"700px\" cellpadding=\"5px\" class=\"SobekCitationSection1\" >");

                    if (user.Templates.Count > 1)
                    {
                        Output.WriteLine("  <tr><td width=\"" + col1Width + "\">&nbsp;</td><td width=\"" + col2Width + "\"><b>" + templateLabel + ":</b></td>");
                        Output.WriteLine("    <td width=\"" + col3Width + "\">");
                        Output.WriteLine("      <select name=\"prefTemplate\" id=\"prefTemplate\" class=\"preferences_language_select\" onChange=\"template_changed()\" >");
                        foreach (string t in user.Templates)
                        {
                            if (t == current_template)
                            {
                                Output.WriteLine("        <option  selected=\"selected\" value=\"" + t + "\">" + t + "</option>");
                            }
                            else
                            {
                                Output.WriteLine("        <option value=\"" + t + "\">" + t + "</option>");
                            }
                        }
                        Output.WriteLine("      </select>");
                        Output.WriteLine("    </td>");
                        Output.WriteLine("  </tr>");
                    }
                    if (user.Projects.Count > 1)
                    {
                        Output.WriteLine("  <tr><td width=\"" + col1Width + "\">&nbsp;</td><td width=\"" + col2Width + "\"><b>" + projectLabel + ":</b></td>");
                        Output.WriteLine("    <td width=\"" + col3Width + "\">");
                        Output.WriteLine("      <select name=\"prefProject\" id=\"prefProject\" class=\"preferences_language_select\" onChange=\"project_changed()\" >");
                        foreach (string t in user.Projects)
                        {
                            if (t == current_project)
                            {
                                Output.WriteLine("        <option  selected=\"selected\" value=\"" + t + "\">" + t + "</option>");
                            }
                            else
                            {
                                Output.WriteLine("        <option value=\"" + t + "\">" + t + "</option>");
                            }
                        }
                        Output.WriteLine("      </select>");
                        Output.WriteLine("    </td>");
                        Output.WriteLine("  </tr>");
                    }

                    Output.WriteLine("</table>");
                    Output.WriteLine("To change your default " + changeable + ", select <a href=\"" + currentMode.Base_URL + "my/preferences\">My Account</a> above.<br /><br />");
                }


                Output.WriteLine("</blockquote><br />");

                Output.WriteLine("</div>");
            }

            #endregion

            #region Add the template and surrounding HTML for the template page step(s)

            if ((currentProcessStep >= 2) && (currentProcessStep <= (template.InputPages_Count + 1)))
            {
                Output.WriteLine("<script type=\"text/javascript\" src=\"" + currentMode.Base_URL + "default/scripts/jquery/jquery-1.6.2.min.js\"></script>");
                Output.WriteLine("<script type=\"text/javascript\" src=\"" + currentMode.Base_URL + "default/scripts/jquery/jquery-ui-1.8.16.custom.min.js\"></script>");
                Output.WriteLine("<script type=\"text/javascript\" src=\"" + currentMode.Base_URL + "default/scripts/sobekcm_form.js\" ></script>");

                Output.WriteLine("<div class=\"SobekHomeText\">");
                Output.WriteLine("<br />");
                string template_page_title = template.InputPages[currentProcessStep - 2].Title;
                if (template_page_title.Length == 0)
                    template_page_title = "Item Description";
                string template_page_instructions = template.InputPages[currentProcessStep - 2].Instructions;
                if (template_page_instructions.Length == 0)
                    template_page_instructions = "Enter the basic information to describe your new item";

                // Get the adjusted process step number ( for skipping permissions, usual step1 )
                int adjusted_process_step = currentProcessStep;
                if (template.Permissions_Agreement.Length == 0)
                    adjusted_process_step--;

                Output.WriteLine("<strong>Step " + adjusted_process_step + " of " + totalTemplatePages + ": " + template_page_title + "</strong><br />");
                Output.WriteLine("<blockquote>" + template_page_instructions + "</blockquote>");
                if ((validationErrors != null) && (validationErrors.Count > 0) && (item.Web.Show_Validation_Errors))
                {
                    Output.WriteLine("<span style=\"color: red;\"><b>The following errors were detected:</b>");
                    Output.WriteLine("<blockquote>");
                    foreach (string validation_error in validationErrors)
                    {
                        Output.WriteLine(validation_error + "<br />");
                    }
                    Output.WriteLine("</blockquote>");
                    Output.WriteLine("</span>");
                    Output.WriteLine("<br />");
                }

                int next_step = currentProcessStep + 1;
                if (currentProcessStep == template.InputPages_Count + 1)
                {
                    next_step = template.Upload_Types == Template.Template_Upload_Types.None ? 9 : 8;
                }
                Output.WriteLine("<div class=\"SobekEditPanel\">");
                Output.WriteLine("<table width=\"720\">");
                Output.WriteLine("  <tr>");
                Output.WriteLine("    <td width=\"375px\">&nbsp;</td>");
                Output.WriteLine("    <td>");
                if (adjusted_process_step == 1)
                {
                    Output.WriteLine("      <a href=\"\" onclick=\"return new_item_cancel();\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/cancel_button_g.gif\" alt=\"CANCEL\" /></a>");
                }
                else
                {
                    Output.WriteLine("      <a href=\"\" onclick=\"return new_item_next_phase(" + (currentProcessStep - 1) + ");\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/back_button_22_g.gif\" alt=\"BACK\" /></a>");
                }
                Output.WriteLine("      &nbsp; &nbsp; ");
                Output.WriteLine("      <a href=\"\" onclick=\"return new_item_clear();\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/clear_button_22_g.gif\" alt=\"CLEAR\" /></a>");
                Output.WriteLine("      &nbsp; &nbsp; ");
                Output.WriteLine("      <a href=\"\" onclick=\"return new_item_next_phase(" + next_step + ");\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/next_button_22_g.gif\" alt=\"NEXT\" /></a>");
                Output.WriteLine("    </td>");
                Output.WriteLine("  </tr>");
                Output.WriteLine("</table>");
                string popup_forms = template.Render_Template_HTML(Output, item, currentMode.Skin, currentMode.Browser_Type.ToUpper().IndexOf("FIREFOX") >= 0, user, currentMode.Language, Translator, currentMode.Base_URL, currentProcessStep - 1);
                Output.WriteLine("<table width=\"720\">");
                Output.WriteLine("  <tr>");
                Output.WriteLine("    <td width=\"375px\">&nbsp;</td>");
                Output.WriteLine("    <td>");
                if (adjusted_process_step == 1)
                {
                    Output.WriteLine("      <a href=\"\" onclick=\"return new_item_cancel();\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/cancel_button_g.gif\" alt=\"CANCEL\" /></a>");
                }
                else
                {
                    Output.WriteLine("      <a href=\"\" onclick=\"return new_item_next_phase(" + (currentProcessStep - 1) + ");\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/back_button_22_g.gif\" alt=\"BACK\" /></a>");
                } 
                Output.WriteLine("      &nbsp; &nbsp; ");
                Output.WriteLine("      <a href=\"\" onclick=\"return new_item_clear();\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/clear_button_22_g.gif\" alt=\"CLEAR\" /></a>");
                Output.WriteLine("      &nbsp; &nbsp; ");
                Output.WriteLine("      <a href=\"\" onclick=\"return new_item_next_phase(" + next_step + ");\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/next_button_22_g.gif\" alt=\"NEXT\" /></a>");
                Output.WriteLine("    </td>");
                Output.WriteLine("  </tr>");
                Output.WriteLine("</table>");
                Output.WriteLine("</div>");
                Output.WriteLine("<br />");


                if (popup_forms.Length > 0)
                    Output.WriteLine(popup_forms);

                Output.WriteLine("</div>");
            }

            #endregion

            #region Add the list of all existing files and the URL box for the upload file/enter URL step 

            if (currentProcessStep == 8)
            {
                if ((template.Upload_Types == Template.Template_Upload_Types.File) || (template.Upload_Types == Template.Template_Upload_Types.File_or_URL))
                {
                    string[] all_files = Directory.GetFiles(userInProcessDirectory);
                    SortedList<string, List<string>> image_files = new SortedList<string, List<string>>();
                    SortedList<string, List<string>> download_files = new SortedList<string, List<string>>();
                    foreach (string thisFile in all_files)
                    {
                        FileInfo thisFileInfo = new FileInfo(thisFile);

                        if ((thisFileInfo.Name.IndexOf("agreement.txt") != 0) && (thisFileInfo.Name.IndexOf("TEMP000001_00001.mets") != 0) && (thisFileInfo.Name.IndexOf("doc.xml") != 0) && (thisFileInfo.Name.IndexOf("ufdc_mets.xml") != 0) && (thisFileInfo.Name.IndexOf("marc.xml") != 0))
                        {
                            // Get information about this files name and extension
                            string extension_upper = thisFileInfo.Extension.ToUpper();
                            string filename_sans_extension = thisFileInfo.Name.Replace(thisFileInfo.Extension, "");
                            string name_upper = thisFileInfo.Name.ToUpper();

                            // Is this a page image?
                            if ((extension_upper == ".JPG") || (extension_upper == ".TIF") || (extension_upper == ".JP2") || (extension_upper == ".JPX"))
                            {
                                // Exclude .QC.jpg files
                                if (name_upper.IndexOf(".QC.JPG") < 0)
                                {
                                    // If this is a thumbnail, trim off the THM part on the file name
                                    if (name_upper.IndexOf("THM.JPG") > 0)
                                    {
                                        filename_sans_extension = filename_sans_extension.Substring(0, filename_sans_extension.Length - 3);
                                    }

                                    // Is this the first image file with this name?
                                    if (image_files.ContainsKey(filename_sans_extension.ToLower()))
                                    {
                                        image_files[filename_sans_extension.ToLower()].Add(thisFileInfo.Name);
                                    }
                                    else
                                    {
                                        List<string> newImageGrouping = new List<string> {thisFileInfo.Name};
                                        image_files[filename_sans_extension.ToLower()] = newImageGrouping;
                                    }
                                }
                            }
                            else
                            {
                                // If this does not match the exclusion regular expression, than add this
                                if (!Regex.Match(thisFileInfo.Name, SobekCM_Library_Settings.Files_To_Exclude_From_Downloads, RegexOptions.IgnoreCase).Success)
                                {
                                    // Is this the first image file with this name?
                                    if (download_files.ContainsKey(filename_sans_extension.ToLower()))
                                    {
                                        download_files[filename_sans_extension.ToLower()].Add(thisFileInfo.Name);
                                    }
                                    else
                                    {
                                        List<string> newDownloadGrouping = new List<string> {thisFileInfo.Name};
                                        download_files[filename_sans_extension.ToLower()] = newDownloadGrouping;
                                    }
                                }
                            }
                        }
                    }

                    // Any page images?
                    int file_counter = 0;
                    if ( image_files.Count > 0 )
                    {
                        Output.WriteLine("The following page images are already uploaded for this package:");
                        Output.WriteLine("<blockquote>");
                        Output.WriteLine("<table border=\"0px\" cellspacing=\"0px\">");
                        Output.WriteLine("  <tr align=\"left\" bgcolor=\"#0022a7\" height=\"22px\" >");
                        Output.WriteLine("    <th width=\"100px\" align=\"left\"><span style=\"color: White\">FILENAME</span></th>");
                        Output.WriteLine("    <th width=\"150px\" align=\"left\">&nbsp;</th>");
                        Output.WriteLine("    <th width=\"90px\"><span style=\"color: White\">SIZE</span></th>");
                        Output.WriteLine("    <th width=\"170px\"><span style=\"color: White\">DATE UPLOADED</span></th>");
                        Output.WriteLine("    <th width=\"90px\"><span style=\"color: White\">ACTION</span></th>");
                        Output.WriteLine("  </tr>");

                        // Step through all the page image file groups
                        foreach (string fileKey in image_files.Keys )
                        {
                            // Get this group of files
                            List<string> fileGroup = image_files[fileKey];

                            // Add each individual file
                            foreach (string thisFile in fileGroup)
                            {
                                file_counter++;

                                // Add the file name literal
                                FileInfo fileInfo = new FileInfo(userInProcessDirectory + "\\" + thisFile);
                                Output.WriteLine("<tr align=\"left\" >");
                                Output.WriteLine("<td colspan=\"2\">" + fileInfo.Name + "</td>");
                                if (fileInfo.Length < 1024)
                                    Output.WriteLine("<td>" + fileInfo.Length + "</td>");
                                else
                                {
                                    if (fileInfo.Length < (1024 * 1024))
                                        Output.WriteLine("<td>" + (fileInfo.Length / 1024) + " KB</td>");
                                    else
                                        Output.WriteLine("<td>" + (fileInfo.Length / (1024 * 1024)) + " MB</td>");
                                }

                                Output.WriteLine("<td>" + fileInfo.LastWriteTime + "</td>");
                                Output.WriteLine("<td align=\"center\"> <span class=\"SobekFolderActionLink\">( <a href=\"\" onclick=\"return file_delete('" + fileInfo.Name + "');\">delete</a> )</span></td></tr>");
                            }

                            // Now add the row to include the label
                            string input_name = "upload_label" + file_counter.ToString();
                            Output.WriteLine("<tr><td width=\"120px\" align=\"right\"><span style=\"color:gray\">Label:</span></td><td colspan=\"4\">");
                            Output.WriteLine("<input type=\"hidden\" id=\"upload_file" + file_counter.ToString() + "\" name=\"upload_file" + file_counter.ToString() + "\" value=\"" + fileKey + "\" />");
                            if (HttpContext.Current.Session["file_" + fileKey] == null)
                            {
                                Output.WriteLine("<input type=\"text\" class=\"upload_label_input\" id=\"" + input_name + "\" name=\"" + input_name + "\" value=\"\" onfocus=\"javascript:textbox_enter('" + input_name + "', 'upload_label_input_focused')\" onblur=\"javascript:textbox_leave('" + input_name + "', 'upload_label_input')\" ></input>");
                            }
                            else
                            {
                                string label_from_session = HttpContext.Current.Session["file_" + fileKey].ToString();
                                Output.WriteLine("<input type=\"text\" class=\"upload_label_input\" id=\"" + input_name + "\" name=\"" + input_name + "\" value=\"" + label_from_session + "\" onfocus=\"javascript:textbox_enter('" + input_name + "', 'upload_label_input_focused')\" onblur=\"javascript:textbox_leave('" + input_name + "', 'upload_label_input')\" ></input>");
                            }
                            Output.WriteLine("</td></tr>");
                            Output.WriteLine("<tr><td bgcolor=\"#0022a7\" colspan=\"5\"></td></tr>");
                            Output.WriteLine("<tr height=\"6px\"><td colspan=\"5\"></td></tr>");
                        }
                        Output.WriteLine("</table></blockquote><br />");
                    }

                    // Any download files?
                    if (download_files.Count > 0)
                    {
                        Output.WriteLine("The following files are already uploaded for this package and will be included as downloads:");
                        Output.WriteLine("<blockquote>");
                        Output.WriteLine("<table border=\"0px\" cellspacing=\"0px\">");
                        Output.WriteLine("  <tr align=\"left\" bgcolor=\"#0022a7\" height=\"22px\" >");
                        Output.WriteLine("    <th width=\"100px\" align=\"left\"><span style=\"color: White\">FILENAME</span></th>");
                        Output.WriteLine("    <th width=\"150px\" align=\"left\">&nbsp;</th>");
                        Output.WriteLine("    <th width=\"90px\"><span style=\"color: White\">SIZE</span></th>");
                        Output.WriteLine("    <th width=\"170px\"><span style=\"color: White\">DATE UPLOADED</span></th>");
                        Output.WriteLine("    <th width=\"90px\"><span style=\"color: White\">ACTION</span></th>");
                        Output.WriteLine("  </tr>");

                        // Step through all the download file groups
                        foreach (string fileKey in download_files.Keys)
                        {
                            // Get this group of files
                            List<string> fileGroup = download_files[fileKey];

                            // Add each individual file
                            foreach (string thisFile in fileGroup)
                            {
                                file_counter++;

                                // Add the file name literal
                                FileInfo fileInfo = new FileInfo(userInProcessDirectory + "\\" + thisFile);
                                Output.WriteLine("<tr align=\"left\" >");
                                Output.WriteLine("<td colspan=\"2\">" + fileInfo.Name + "</td>");
                                if (fileInfo.Length < 1024)
                                    Output.WriteLine("<td>" + fileInfo.Length + "</td>");
                                else
                                {
                                    if (fileInfo.Length < (1024 * 1024))
                                        Output.WriteLine("<td>" + (fileInfo.Length / 1024) + " KB</td>");
                                    else
                                        Output.WriteLine("<td>" + (fileInfo.Length / (1024 * 1024)) + " MB</td>");
                                }

                                Output.WriteLine("<td>" + fileInfo.LastWriteTime + "</td>");
                                Output.WriteLine("<td align=\"center\"> <span class=\"SobekFolderActionLink\">( <a href=\"\" onclick=\"return file_delete('" + fileInfo.Name + "');\">delete</a> )</span></td></tr>");
                            }

                            // Now add the row to include the label
                            string input_name = "upload_label" + file_counter.ToString();
                            Output.WriteLine("<tr><td width=\"120px\" align=\"right\"><span style=\"color:gray\">Label:</span></td><td colspan=\"4\">");
                            Output.WriteLine("<input type=\"hidden\" id=\"upload_file" + file_counter.ToString() + "\" name=\"upload_file" + file_counter.ToString() + "\" value=\"" + fileKey + "\" />");
                            if (HttpContext.Current.Session["file_" + fileKey] == null)
                            {
                                Output.WriteLine("<input type=\"text\" class=\"upload_label_input\" id=\"" + input_name + "\" name=\"" + input_name + "\" value=\"\" onfocus=\"javascript:textbox_enter('" + input_name + "', 'upload_label_input_focused')\" onblur=\"javascript:textbox_leave('" + input_name + "', 'upload_label_input')\" ></input>");
                            }
                            else
                            {
                                string label_from_session = HttpContext.Current.Session["file_" + fileKey].ToString();
                                Output.WriteLine("<input type=\"text\" class=\"upload_label_input\" id=\"" + input_name + "\" name=\"" + input_name + "\" value=\"" + label_from_session + "\" onfocus=\"javascript:textbox_enter('" + input_name + "', 'upload_label_input_focused')\" onblur=\"javascript:textbox_leave('" + input_name + "', 'upload_label_input')\" ></input>");
                            }
                            Output.WriteLine("</td></tr>");
                            Output.WriteLine("<tr><td bgcolor=\"#0022a7\" colspan=\"5\"></td></tr>");
                            Output.WriteLine("<tr height=\"6px\"><td colspan=\"5\"></td></tr>");
                        }
                        Output.WriteLine("</table></blockquote><br />");
                    }
                }

                if ((template.Upload_Types == Template.Template_Upload_Types.File_or_URL) || (template.Upload_Types == Template.Template_Upload_Types.URL))
                {
                    Output.WriteLine("Enter a URL for this digital resource:");
                    Output.WriteLine("<blockquote>");
                    Output.WriteLine("<input type=\"text\" class=\"upload_url_input\" id=\"url_input\" name=\"url_input\" value=\"" + HttpUtility.HtmlEncode(item.Bib_Info.Location.Other_URL) + "\" onfocus=\"javascript:textbox_enter('url_input', 'upload_url_input_focused')\" onblur=\"javascript:textbox_leave('url_input', 'upload_url_input')\" ></input>");
                    Output.WriteLine("</blockquote>");
                }

                string completion_message;
                switch (template.Upload_Types)
                {
                    case Template.Template_Upload_Types.URL:
                        completion_message = "Once the URL is entered, press SUBMIT to finish this item.";
                        break;

                    case Template.Template_Upload_Types.File_or_URL:
                        completion_message = "Once you enter any files and/or URL, press SUBMIT to finish this item.";
                        break;

                    case Template.Template_Upload_Types.File:
                        completion_message = "Once all files are uploaded, press SUBMIT to finish this item.";
                        break;

                    default:
                        completion_message = "Once complete, press SUBMIT to finish this item.";
                        break;
                }


                Output.WriteLine("<table width=\"750\">");
                Output.WriteLine("  <tr height=\"40px\" align=\"left\" valign=\"middle\" >");
                Output.WriteLine("    <td height=\"40px\" width=\"450\">" + completion_message + "</td>");
                Output.WriteLine("    <td height=\"40px\" align=\"right\">");
                Output.WriteLine("      <a href=\"\" onclick=\"return new_upload_next_phase(" + (template.InputPages.Count + 1) + ");\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/back_button_22.gif\" alt=\"BACK\" /></a>");
                Output.WriteLine("      &nbsp; ");
                Output.WriteLine("      <a href=\"\" onclick=\"return new_upload_next_phase(9);\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/submit_button_22.gif\" alt=\"NEXT\" /></a> &nbsp; ");
                Output.WriteLine("    </td>");
                Output.WriteLine("    <td height=\"40px\" width=\"65px\"><div id=\"circular_progress\" name=\"circular_progress\" class=\"hidden_progress\">&nbsp;</div></td>");
                Output.WriteLine("  </tr>");
                Output.WriteLine("</table>");
                Output.WriteLine("<br /><br />");
                Output.WriteLine("</div>");
            }

            //if (current_process_step == 8)
            //{
            //    if ((template.Upload_Types == SobekCM.Library.Citation.Template.Template.Template_Upload_Types.File) || (template.Upload_Types == SobekCM.Library.Citation.Template.Template.Template_Upload_Types.File_or_URL))
            //    {
            //        string[] all_files = System.IO.Directory.GetFiles(user_in_process_directory);
            //        List<string> acceptable_files = new List<string>();
            //        foreach (string thisFile in all_files)
            //        {
            //            System.IO.FileInfo thisFileInfo = new System.IO.FileInfo(thisFile);

            //            if ((thisFileInfo.Name.IndexOf("agreement.txt") != 0) && (thisFileInfo.Name.IndexOf("TEMP000001_00001.mets") != 0) && (thisFileInfo.Name.IndexOf("doc.xml") != 0) && (thisFileInfo.Name.IndexOf("ufdc_mets.xml") != 0) && (thisFileInfo.Name.IndexOf("marc.xml") != 0))
            //            {
            //                acceptable_files.Add(thisFile);
            //            }
            //        }

            //        if (acceptable_files.Count > 0)
            //        {
            //            Output.WriteLine("The following files are already uploaded for this package:");
            //            Output.WriteLine("<blockquote>");
            //            Output.WriteLine("<table border=\"0px\" cellspacing=\"0px\">");
            //            Output.WriteLine("  <tr align=\"left\" bgcolor=\"#0022a7\" height=\"22px\" >");
            //            Output.WriteLine("    <th width=\"100px\" align=\"left\"><span style=\"color: White\">FILENAME</span></th>");
            //            Output.WriteLine("    <th width=\"150px\" align=\"left\">&nbsp;</th>");
            //            Output.WriteLine("    <th width=\"90px\"><span style=\"color: White\">SIZE</span></th>");
            //            Output.WriteLine("    <th width=\"170px\"><span style=\"color: White\">DATE UPLOADED</span></th>");
            //            Output.WriteLine("    <th width=\"90px\"><span style=\"color: White\">ACTION</span></th>");
            //            Output.WriteLine("  </tr>");

            //            int file_counter = 0;
            //            foreach (string thisFile in acceptable_files)
            //            {
            //                file_counter++;

            //                // Add the file name literal
            //                System.IO.FileInfo fileInfo = new System.IO.FileInfo(thisFile);
            //                Output.WriteLine("<tr align=\"left\" >");
            //                Output.WriteLine("<td colspan=\"2\">" + fileInfo.Name + "</td>");
            //                if (fileInfo.Length < 1024)
            //                    Output.WriteLine("<td>" + fileInfo.Length + "</td>");
            //                else
            //                {
            //                    if (fileInfo.Length < (1024 * 1024))
            //                        Output.WriteLine("<td>" + (fileInfo.Length / 1024) + " KB</td>");
            //                    else
            //                        Output.WriteLine("<td>" + (fileInfo.Length / (1024 * 1024)) + " MB</td>");
            //                }

            //                Output.WriteLine("<td>" + fileInfo.LastWriteTime + "</td>");
            //                Output.WriteLine("<td align=\"center\"> <span class=\"SobekFolderActionLink\">( <a href=\"\" onclick=\"return file_delete('" + fileInfo.Name + "');\">delete</a> )</span></td></tr>");

            //                string input_name = "upload_label" + file_counter.ToString();
            //                Output.WriteLine("<tr><td width=\"120px\" align=\"right\"><span style=\"color:gray\">Label:</span></td><td colspan=\"4\">");
            //                Output.WriteLine("<input type=\"hidden\" id=\"upload_file" + file_counter.ToString() + "\" name=\"upload_file" + file_counter.ToString() + "\" value=\"" + fileInfo.Name + "\" />");
            //                if (System.Web.HttpContext.Current.Session["file_" + fileInfo.Name] == null)
            //                {
            //                    Output.WriteLine("<input type=\"text\" class=\"upload_label_input\" id=\"" + input_name + "\" name=\"" + input_name + "\" value=\"\" onfocus=\"javascript:textbox_enter('" + input_name + "', 'upload_label_input_focused')\" onblur=\"javascript:textbox_leave('" + input_name + "', 'upload_label_input')\" ></input>");
            //                }
            //                else
            //                {
            //                    string label_from_session = System.Web.HttpContext.Current.Session["file_" + fileInfo.Name].ToString();
            //                    Output.WriteLine("<input type=\"text\" class=\"upload_label_input\" id=\"" + input_name + "\" name=\"" + input_name + "\" value=\"" + label_from_session + "\" onfocus=\"javascript:textbox_enter('" + input_name + "', 'upload_label_input_focused')\" onblur=\"javascript:textbox_leave('" + input_name + "', 'upload_label_input')\" ></input>");
            //                }
            //                Output.WriteLine("</td></tr>");
            //                Output.WriteLine("<tr><td bgcolor=\"#0022a7\" colspan=\"5\"></td></tr>");
            //                Output.WriteLine("<tr height=\"6px\"><td colspan=\"5\"></td></tr>");
            //            }
            //            Output.WriteLine("</table></blockquote><br />");
            //        }
            //    }

            //    if ((template.Upload_Types == SobekCM.Library.Citation.Template.Template.Template_Upload_Types.File_or_URL) || (template.Upload_Types == SobekCM.Library.Citation.Template.Template.Template_Upload_Types.URL))
            //    {
            //        Output.WriteLine("Enter a URL for this digital resource:");
            //        Output.WriteLine("<blockquote>");
            //        Output.WriteLine("<input type=\"text\" class=\"upload_url_input\" id=\"url_input\" name=\"url_input\" value=\"" + System.Web.HttpUtility.HtmlEncode(item.Bib_Info.Location.Other_URL) + "\" onfocus=\"javascript:textbox_enter('url_input', 'upload_url_input_focused')\" onblur=\"javascript:textbox_leave('url_input', 'upload_url_input')\" ></input>");
            //        Output.WriteLine("</blockquote>");
            //    }

            //    string completion_message = String.Empty;
            //    switch (template.Upload_Types)
            //    {
            //        case SobekCM.Library.Citation.Template.Template.Template_Upload_Types.URL:
            //            completion_message = "Once the URL is entered, press SUBMIT to finish this item.";
            //            break;

            //        case SobekCM.Library.Citation.Template.Template.Template_Upload_Types.File_or_URL:
            //            completion_message = "Once you enter any files and/or URL, press SUBMIT to finish this item.";
            //            break;

            //        case SobekCM.Library.Citation.Template.Template.Template_Upload_Types.File:
            //            completion_message = "Once all files are uploaded, press SUBMIT to finish this item.";
            //            break;

            //        default:
            //            completion_message = "Once complete, press SUBMIT to finish this item.";
            //            break;
            //    }


            //    Output.WriteLine("<table width=\"750\">");
            //    Output.WriteLine("  <tr height=\"40px\" align=\"left\" valign=\"middle\" >");
            //    Output.WriteLine("    <td height=\"40px\" width=\"450\">" + completion_message + "</td>");
            //    Output.WriteLine("    <td height=\"40px\" align=\"right\">");
            //    Output.WriteLine("      <a href=\"\" onclick=\"return new_upload_next_phase(" + (template.InputPages.Count + 1) + ");\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/back_button_22.gif\" alt=\"BACK\" /></a>");
            //    Output.WriteLine("      &nbsp; ");
            //    Output.WriteLine("      <a href=\"\" onclick=\"return new_upload_next_phase(9);\"><img border=\"0\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/submit_button_22.gif\" alt=\"NEXT\" /></a> &nbsp; ");
            //    Output.WriteLine("    </td>");
            //    Output.WriteLine("    <td height=\"40px\" width=\"65px\"><div id=\"circular_progress\" name=\"circular_progress\" class=\"hidden_progress\">&nbsp;</div></td>");
            //    Output.WriteLine("  </tr>");
            //    Output.WriteLine("</table>");
            //    Output.WriteLine("<br /><br />");
            //    Output.WriteLine("</div>");
            //}

            #endregion

            if (currentProcessStep == 9)
            {
                add_congratulations_html(Output, Tracer);
            }
        }

        #endregion

        /// <summary> Add controls directly to the form, either to the main control area or to the file upload placeholder </summary>
        /// <param name="placeHolder"> Main place holder to which all main controls are added </param>
        /// <param name="uploadFilesPlaceHolder"> Place holder is used for uploading file </param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
        public override void Add_Controls(PlaceHolder placeHolder, PlaceHolder uploadFilesPlaceHolder, Custom_Tracer Tracer)
        {
            Tracer.Add_Trace("New_Group_And_Item_MySobekViewer.Add_Controls", String.Empty);

            // Do nothing if this is the very last step
            if (currentProcessStep == 8)
            {
                // Add the upload controls to the file place holder
                add_upload_controls(uploadFilesPlaceHolder, Tracer);
            }
        }

        #region Step 2: Metadata Template

        #endregion

        #region Step 3: Upload Related Files

        private void add_upload_controls(PlaceHolder placeHolder, Custom_Tracer Tracer)
        {
            Tracer.Add_Trace("New_Group_And_Item_MySobekViewer.add_upload_controls", String.Empty);

            StringBuilder filesBuilder = new StringBuilder(2000);
            filesBuilder.AppendLine("<script src=\"" + currentMode.Base_URL + "default/scripts/sobekcm_metadata.js\" type=\"text/javascript\"></script>");

            if ((template.Upload_Types == Template.Template_Upload_Types.File) || (template.Upload_Types == Template.Template_Upload_Types.File_or_URL))
            {
                filesBuilder.AppendLine("Add a new item for this package:");
                filesBuilder.AppendLine("<blockquote>");

                LiteralControl filesLiteral2 = new LiteralControl(filesBuilder.ToString());
                placeHolder.Controls.Add(filesLiteral2);
                filesBuilder.Remove(0, filesBuilder.Length);

                DJUploadController1 = new DJUploadController
                                          {
                                              CSSPath = currentMode.Base_URL + "default/scripts/upload_styles",
                                              ImagePath = currentMode.Base_URL + "default/scripts/upload_images",
                                              ScriptPath = currentMode.Base_URL + "default/scripts/upload_scripts"
                                          };
                placeHolder.Controls.Add(DJUploadController1);

                DJAccessibleProgrssBar1 = new DJAccessibleProgressBar();
                placeHolder.Controls.Add(DJAccessibleProgrssBar1);

                DJFileUpload1 = new DJFileUpload {ShowAddButton = false, ShowUploadButton = true, MaxFileUploads = 1};
                placeHolder.Controls.Add(DJFileUpload1);

                // Set the default processor
                FileSystemProcessor fs = new FileSystemProcessor {OutputPath = userInProcessDirectory};
                DJUploadController1.DefaultFileProcessor = fs;

                // Change the file processor and set it's properties.
                FieldTestProcessor fsd = new FieldTestProcessor {OutputPath = userInProcessDirectory};
                DJFileUpload1.FileProcessor = fsd;

                filesBuilder.AppendLine("</blockquote><br />");
            }

            LiteralControl literal1 = new LiteralControl(filesBuilder.ToString());
            placeHolder.Controls.Add(literal1);
        }

        #endregion

        #region Step 4 : Congratulations!

        private void add_congratulations_html(TextWriter Output, Custom_Tracer Tracer)
        {
            // default/images/home_button.gif = myUFDC Home
            // default/images/new_item.gif = Start a new item
            // default/images/submitted_items.gif = View all my submitted items


            Tracer.Add_Trace("New_Group_And_Item_MySobekViewer.add_congratulations_html", String.Empty);

            Output.WriteLine("<div class=\"SobekHomeText\">");
            Output.WriteLine("<br />");
            if (!criticalErrorEncountered)
            {
                Output.WriteLine("<strong><center><h1>CONGRATULATIONS!</h1></center></strong>");
                Output.WriteLine("<br />");
                Output.WriteLine("Your item has been successfully added to the digital library and will appear immediately.<br />");
                Output.WriteLine("<br />");
                Output.WriteLine("Search indexes may take a couple minutes to build, at which time this item will be discoverable through the search interface.<br />");
                Output.WriteLine("<br />");
                if (user.Send_Email_On_Submission)
                {
                    Output.WriteLine("An email has been sent to you with the new item information.<br />");
                    Output.WriteLine("<br />");
                }
            }
            else
            {
                Output.WriteLine("<strong><center><h1>Ooops!! We encountered a problem!</h1></center></strong>");
                Output.WriteLine("<br />");
                Output.WriteLine("An email has been sent to the programmer who will attempt to correct your issue.  You should be contacted within the next 24-48 hours regarding this issue.<br />");
                Output.WriteLine("<br />");
            }
            Output.WriteLine("<div style=\"font-size:larger\">");
            Output.WriteLine("<table width=\"700\"><tr><td width=\"100px\">&nbsp;</td>");
            Output.WriteLine("<td>");
            Output.WriteLine("What would you like to do next?<br />");
            Output.WriteLine("<blockquote>");

            currentMode.My_Sobek_Type = My_Sobek_Type_Enum.Home;
            Output.WriteLine("<a href=\"" + currentMode.Redirect_URL() + "\">Return to myUFDC Home</a><br/><br />");

            if (!criticalErrorEncountered)
            {
                Output.WriteLine("<a href=\"" + currentMode.Base_URL + "l/" + item.BibID + "/" + item.VID + "\">View this item</a><br/><br />");

                currentMode.My_Sobek_Type = My_Sobek_Type_Enum.Edit_Item_Metadata;
                currentMode.My_Sobek_SubMode = "1";
                currentMode.BibID = item.BibID;
                currentMode.VID = item.VID;
                Output.WriteLine("<a href=\"" + currentMode.Redirect_URL() + "\">Edit this item</a><br /><br />");


                currentMode.My_Sobek_Type = My_Sobek_Type_Enum.New_Item;
                currentMode.BibID = String.Empty;
                currentMode.VID = String.Empty;
                Output.WriteLine("<a href=\"" + currentMode.Redirect_URL() + "\">Add another item</a><br /><br />");
            }
            currentMode.My_Sobek_Type = My_Sobek_Type_Enum.Folder_Management;
            currentMode.Result_Display_Type = Result_Display_Type_Enum.Brief;
            currentMode.My_Sobek_SubMode = "Submitted Items";
            Output.WriteLine("<a href=\"" + currentMode.Redirect_URL() + "\">View all my submitted items</a><br /><br />");

            Output.WriteLine("</blockquote>");
            Output.WriteLine("</td></tr></table></div>");
            Output.WriteLine("<br />");
            item = null;
            HttpContext.Current.Session["Item"] = null;

            Output.WriteLine("</div>");
        }

        #endregion

        #region Private method for creating a new digital resource

        private void new_item(Custom_Tracer Tracer)
        {
            // Load the project
            item = null;
            string project_code = user.Current_Project;
            try
            {
                if (project_code.Length > 0)
                {
                    string project_name = SobekCM_Library_Settings.Base_MySobek_Directory + "projects\\" + project_code + ".pmets";

                    if (Tracer != null)
                    {
                        Tracer.Add_Trace("New_Group_And_Item_MySobekViewer.new_item()", "Loading project<br />(" + project_name + ")");
                    }

                    if (File.Exists(project_name))
                    {
                        item = SobekCM_Item.Read_METS(project_name);
                    }
                    else
                    {
                        if (File.Exists(project_name.Replace(".pmets", ".mets")))
                        {
                            item = SobekCM_Item.Read_METS(project_name.Replace(".pmets", ".mets"));
                        }
                    }

                    // Only do more if the item is not null
                    if (item != null)
                    {
                        // Transfer the default type note over to the type
                        item.Bib_Info.SobekCM_Type_String = String.Empty;
                        if (item.Bib_Info.Notes_Count > 0)
                        {
                            Note_Info deleteNote = null;
                            foreach (Note_Info thisNote in item.Bib_Info.Notes)
                            {
                                if (thisNote.Note_Type == Note_Type_Enum.default_type)
                                {
                                    deleteNote = thisNote;
                                    item.Bib_Info.SobekCM_Type_String = thisNote.Note;
                                    break;
                                }
                            }
                            if (deleteNote != null)
                                item.Bib_Info.Remove_Note(deleteNote);
                        }
                    }
                    else
                    {
                        project_code = String.Empty;
                    }
                }
            }
            catch (Exception ee)
            {
                if (Tracer != null)
                {
                    Tracer.Add_Trace("New_Group_And_Item_MySobekViewer.new_item()", "Error loading projects<br />" + ee);
                }
                project_code = String.Empty;
            }

            // Build a new empty METS file
            if (item == null)
            {
                item = new SobekCM_Item();
                item.Bib_Info.SobekCM_Type_String = String.Empty;
            }

            // Set the source directory first
            item.Source_Directory = userInProcessDirectory;
            string orgcode = user.Organization_Code;
            if ((orgcode.Length > 0) && ( orgcode.ToLower()[0] != 'i' ))
            {
                orgcode = "i" + orgcode;
            }

            // If there is no source code, use the user's code
            if (item.Bib_Info.Source.Code.Length == 0)
            {
                item.Bib_Info.Source.Code = orgcode;
                item.Bib_Info.Source.Statement = user.Organization;
            }

            // If there is no holding code, use the user's code
            if (item.Bib_Info.Location.Holding_Code.Length == 0)
            {
                item.Bib_Info.Location.Holding_Code = orgcode;
                item.Bib_Info.Location.Holding_Name = user.Organization;
            }

            // Set some values from the template
            if (template.Include_User_As_Author)
            {
                item.Bib_Info.Main_Entity_Name.Full_Name = user.Family_Name + ", " + user.Given_Name;
            }
            item.Behaviors.IP_Restriction_Membership = template.Default_Visibility;

            item.VID = "00001";
            item.BibID = "TEMP000001";
            if ((item.Behaviors.Aggregation_Count == 0) && (currentMode.Default_Aggregation == "dloc1"))
                item.Behaviors.Add_Aggregation("DLOC");
            item.METS_Header.Create_Date = DateTime.Now;
            item.METS_Header.Modify_Date = item.METS_Header.Create_Date;
            item.METS_Header.Creator_Individual = user.Full_Name;
            if ( project_code.Length == 0 )
                item.METS_Header.Add_Creator_Org_Notes("Created using template '" + templateCode + "'");
            else
                item.METS_Header.Add_Creator_Org_Notes("Created using template '" + templateCode + "' and project '" + project_code + "'.");

            if (item.Bib_Info.Main_Title.Title.ToUpper().IndexOf("PROJECT LEVEL METADATA") == 0)
                item.Bib_Info.Main_Title.Clear();
            if (user.Default_Rights.Length > 0)
                item.Bib_Info.Access_Condition.Text = user.Default_Rights.Replace("[name]", user.Full_Name).Replace("[year]", DateTime.Now.Year.ToString());
        }

        #endregion
    }
}
  


