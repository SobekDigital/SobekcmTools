﻿#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using SobekCM.Resource_Object;
using SobekCM.Resource_Object.Bib_Info;
using SobekCM.Resource_Object.Behaviors;
using SobekCM.Tools;

#endregion

namespace SobekCM.Library.Users
{
    /// <summary> Represents a single mySobek user, including personal information, permissions,
    /// and preferences.  </summary>
    public class User_Object
    {
        #region Private class members 

        private readonly User_Editable_Collection aggregations;
        private readonly List<string> bibids;
        private readonly List<string> bookshelfObjectIds;
        private string currentProject;
        private string currentTemplate;
        private readonly List<string> editableRegexes;
        private readonly SortedList<string, User_Folder> folders;
        private readonly List<string> projects;
        private readonly List<string> templates;
        private readonly List<string> userGroups;
        private readonly Dictionary<string, object> userOptions; 

        #endregion

        #region Constructor

        /// <summary> Constructor for a new instance of the User_Object class </summary>
        public User_Object()
        {
            Family_Name = String.Empty;
            Given_Name = String.Empty;
            UFID = String.Empty;
            Email = String.Empty;
            Department = String.Empty;
            Nickname = String.Empty;
            Can_Submit = false;
            Is_Just_Registered = false;
            Send_Email_On_Submission = true;
            Is_Temporary_Password = false;
            Is_Internal_User = false;
            UserName = String.Empty;
            Preferred_Language = String.Empty;
            templates = new List<string>();
            projects = new List<string>();
            bibids = new List<string>();
            bookshelfObjectIds = new List<string>();
            Items_Submitted_Count = 0;
            Organization = String.Empty;
            Department = String.Empty;
            Unit = String.Empty;
            College = String.Empty;
            Organization_Code = String.Empty;
            Edit_Template_Code = String.Empty;
            Edit_Template_MARC_Code = String.Empty;
            aggregations = new User_Editable_Collection();
            editableRegexes = new List<string>();
            folders = new SortedList<string, User_Folder>();
            Default_Rights = "All rights reserved by the submitter.";
            Is_System_Admin = false;
            Is_Portal_Admin = false;
            Has_Descriptive_Tags = false;
            userGroups = new List<string>();
            Receive_Stats_Emails = true;
            Has_Item_Stats = false;
            Include_Tracking_In_Standard_Forms = true;
            userOptions = new Dictionary<string, object>();
        }

        #endregion

        /// <summary> Get the user option as an object, by option key </summary>
        /// <param name="Option_Key"> Key for the user option </param>
        /// <returns> Option, as an uncast object, or NULL </returns>
        public object Get_Option( string Option_Key )
        {
            if (userOptions.ContainsKey(Option_Key))
                return userOptions[Option_Key];
            return null;
        }

        /// <summary> Get the user option as an integer, by option key </summary>
        /// <param name="Option_Key"> Key for the user option </param>
        /// <param name="Default_Value"> Default value to return, if no value is present </param>
        /// <returns> Either the value from the user options, or else the default value </returns>
        public int Get_Option(string Option_Key, int Default_Value )
        {
            if (userOptions.ContainsKey(Option_Key))
            {
                int tempValue;
                if (int.TryParse(userOptions[Option_Key].ToString(), out tempValue))
                    return tempValue;
            }
            return Default_Value;
        }

        /// <summary> Get the user option as a string, by option key </summary>
        /// <param name="Option_Key"> Key for the user option </param>
        /// <param name="Default_Value"> Default value to return, if no value is present </param>
        /// <returns> Either the value from the user options, or else the default value </returns>
        public string Get_Option(string Option_Key, string Default_Value)
        {
            if (userOptions.ContainsKey(Option_Key))
            {
                return userOptions[Option_Key].ToString();
            }
            return Default_Value;
        }


        /// <summary> Add a new user option </summary>
        /// <param name="Option_Key"> Key for the user option </param>
        /// <param name="Option_Value"> Value for this user option </param>
        public void Add_Option( string Option_Key, object Option_Value )
        {
            userOptions[Option_Key] = Option_Value;
        }

        #region Public properties of this user object

        /// <summary> Flag indicates this user has chosen to receive statistics emails about their items </summary>
        public bool Receive_Stats_Emails { get; set; }

        /// <summary> Flag indicates this user has item statistics linked to their account </summary>
        public bool Has_Item_Stats { get; set; }

        /// <summary> Checks to see if this user is a collection manager or collection admin </summary>
        public bool Is_A_Collection_Manager_Or_Admin
        {
            get
            {
                return aggregations != null && aggregations.Collection.Any(aggregation => aggregation.IsCurator);
            }
        }

        /// <summary> Flag indicates if this user has descriptive tags associated with them </summary>
        public bool Has_Descriptive_Tags { get; set; }

        /// <summary> Flag is used when editing a users rights to indicate this user should be able to edit ALL items in the library </summary>
        public bool Should_Be_Able_To_Edit_All_Items { get; set; }

        /// <summary> Ordered list of submittal templates this user has access to </summary>
        /// <remarks>The first item in this list is the default template for this user </remarks>
        public ReadOnlyCollection<string> Templates
        {
            get { return new ReadOnlyCollection<string>(templates); }
        }

        /// <summary> Returns the current template for this user </summary>
        public string Current_Template
        {
            get 
            {
                if (!String.IsNullOrEmpty(currentTemplate))
                    return currentTemplate;
                if ((templates != null) && (templates.Count > 0))
                    return templates[0];
                return String.Empty;
            }
            set
            {
                if ((templates == null) || (templates.Count <= 0)) return;

                if (( String.IsNullOrEmpty(value)) || ( templates.Contains( value )))
                    currentTemplate = value;
            }
        }

        /// <summary> Ordered list of projects this user has access to </summary>
        /// <remarks>The first item in this list is the default project for this user </remarks>
        public ReadOnlyCollection<string> Projects
        {
            get { return new ReadOnlyCollection<string>(projects); }
        }

        /// <summary> Returns the current project for this user </summary>
        public string Current_Project
        {
            get
            {
                if (!String.IsNullOrEmpty(currentProject))
                    return currentProject;
                if ((projects != null) && (projects.Count > 0))
                    return projects[0];
                return String.Empty;
            }
            set
            {
                if ((projects == null) || (projects.Count <= 0)) return;

                if ((String.IsNullOrEmpty(value)) || (projects.Contains(value)))
                    currentProject = value;
            }
        }

        /// <summary> List of the BibID's for every item this user has submitted or been directly 
        /// granted edit permissions against. </summary>
        public ReadOnlyCollection<string> BibIDs
        {
            get { return new ReadOnlyCollection<string>(bibids); }
        }

        /// <summary> Number of items this user has submitted </summary>
        public int Items_Submitted_Count { get; set; }

        /// <summary> SobekCM username for this user </summary>
        public string UserName { get; set; }

        /// <summary> UserID (or primary key) to this user from the database </summary>
        public int UserID { get; set; }

        /// <summary> User's preferred language </summary>
        public string Preferred_Language { get; set; }

        /// <summary> Simple flag indicates if this user can submit items </summary>
        public bool Can_Submit { get; set; }

        /// <summary> Default rights statement for this user  </summary>
        public string Default_Rights { get; set; }

        /// <summary> Flag indicates whether user wishes to receive an email after submission </summary>
        public bool Send_Email_On_Submission { get; set; }

        /// <summary> Flag indicates if this is a temporary password </summary>
        /// <remarks>Temporary passwords must be changed once the user logs on </remarks>
        public bool Is_Temporary_Password { get; set; }

        /// <summary> Flag indicates if this is an internal user </summary>
        /// <remarks>This grants access to various tracking elements in SobekCM</remarks>
        public bool Is_Internal_User { get; set; }

        /// <summary> Flag indicates if this user has general admin rights over the entire system, including very basic settings which impactt how the system runs </summary>
        public bool Is_System_Admin { get; set; }

        /// <summary> Flag indicates if this user has general admin rights over the appearance of portions of the system </summary>
        public bool Is_Portal_Admin { get; set; }

        /// <summary> Flag indicates if users should see the tracking information when adding a new volume 
        /// or performing standard operations within the system </summary>
        public bool Include_Tracking_In_Standard_Forms { get; set; }

        /// <summary> User's family (or last) name </summary>
        public string Family_Name { get; set; }

        /// <summary> User's given (or first) name </summary>
        public string Given_Name { get; set; }

        /// <summary> User's nickname </summary>
        public string Nickname { get; set; }

        /// <summary> Returns the user's full name in [first name last name] order</summary>
        public string Full_Name
        {
            get {   return Given_Name + " " + Family_Name;  }
        }

        /// <summary> Returns the user's full name in [last name, last name] format</summary>
        public string Reversed_Full_Name
        {
            get { return Family_Name + ", " + Given_Name; }
        }

        /// <summary> User's UFID  </summary>
        public string UFID { get; set; }

        /// <summary> User's organization affiliation information   </summary>
        public string Organization { get; set; }

        /// <summary> User's organization code  </summary>
        /// <remarks> This is used to tag any newly submitted items to the institution's aggregation</remarks>
        public string Organization_Code { get; set; }

        /// <summary> User's college affiliation information  </summary>
        public string College { get; set; }

        /// <summary> User's department affiliation information  </summary>
        public string Department { get; set; }

        /// <summary> User's unit affiliation information  </summary>
        public string Unit { get; set; }

        /// <summary> User's email address </summary>
        public string Email { get; set; }

        /// <summary>User's template code for editing non-MARC records </summary>
        public string Edit_Template_Code { get; set; }

        /// <summary> User's template code editing MARC records  </summary>
        public string Edit_Template_MARC_Code { get; set; }

        /// <summary> List of item aggregations associated with this user </summary>
        public ReadOnlyCollection<User_Editable_Aggregation> Aggregations
        {
            get { return aggregations.Collection; }
        }

        /// <summary> List of regular expressions for checking for edit by bibid </summary>
        public ReadOnlyCollection<string> Editable_Regular_Expressions
        {
            get { return new ReadOnlyCollection<string>(editableRegexes); }
        }

        /// <summary> List of user groups to which this user belongs </summary>
        public ReadOnlyCollection<string> User_Groups
        {
            get { return new ReadOnlyCollection<string>(userGroups); }
        }

        /// <summary> List of folders associated with this user </summary>
        public ReadOnlyCollection<User_Folder> Folders
        {
            get { return new ReadOnlyCollection<User_Folder>(folders.Values); }
        }

        /// <summary> Flag indicates if this user was just registered </summary>
        /// <remarks> This flag is just used so mySobek does not say 'Welcome Back' the first time a user logs on </remarks>
        public bool Is_Just_Registered { get; set; }

        /// <summary> Gets the list of all folders, in alphabetical order </summary>
        public ReadOnlyCollection<User_Folder> All_Folders
        {
            get
            {
                SortedList<string, User_Folder> folder_builder = new SortedList<string, User_Folder>();
                foreach (User_Folder thisFolder in folders.Values)
                {
                    folder_builder.Add(thisFolder.Folder_Name, thisFolder);
                    recurse_through_children(thisFolder, folder_builder);
                }

                if (folder_builder.Count == 0)
                    folder_builder.Add("My Bookshelf", new User_Folder("My Bookshelf", -1));

                return new ReadOnlyCollection<User_Folder>(folder_builder.Values);
            }
        }

        /// <summary> Removes an item from the list of items in the user's bookshelves </summary>
        /// <param name="BibID"> BibID for this item in a bookshelf</param>
        /// <param name="VID"> VID for this item in a bookshelf</param>
        public void Remove_From_Bookshelves(string BibID, string VID)
        {
            string objID = BibID.ToUpper() + "_" + VID;
            if (bookshelfObjectIds.Contains(objID))
                bookshelfObjectIds.Remove(objID);
        }

        /// <summary> Checks to see if an item exists in this user's bookshelf </summary>
        /// <param name="BibID"> BibID for this item in a bookshelf</param>
        /// <param name="VID"> VID for this item in a bookshelf</param>
        /// <returns> TRUE if the item is in the bookshelf, otherwise FALSE </returns>
        public bool Is_In_Bookshelf(string BibID, string VID)
        {
            return bookshelfObjectIds.Contains(BibID.ToUpper() + "_" + VID);
        }

        /// <summary> Sets the flag that a particular aggregation exists on this user's home page </summary>
        /// <param name="Code"> Code for this item aggregation </param>
        /// <param name="Name"> Name of this item aggregation </param>
        /// <param name="Flag"> New flag </param>
        public void Set_Aggregation_Home_Page_Flag(string Code, string Name, bool Flag)
        {
            string aggrCodeUpper = Code.ToUpper();
            foreach (User_Editable_Aggregation thisAggregation in aggregations.Collection.Where(thisAggregation => thisAggregation.Code == aggrCodeUpper))
            {
                thisAggregation.OnHomePage = Flag;
                return;
            }

            if (Flag)
            {
                aggregations.Add(Code, Name, false, false, false, true );
            }
        }

        /// <summary> Checks to see if an aggregation is currently listed on the user's personalized home page </summary>
        /// <param name="AggregationCode"> Code for this item aggregation </param>
        /// <returns> TRUE if on the home page currently, otherwise FALSE </returns>
        public bool Is_On_Home_Page(string AggregationCode)
        {
            string aggrCodeUpper = AggregationCode.ToUpper();
            return (from thisAggregation in aggregations.Collection where thisAggregation.Code == aggrCodeUpper select thisAggregation.OnHomePage).FirstOrDefault();
        }

        /// <summary> Checks to see if this user can perform administrative tasks against an item aggregation </summary>
        /// <param name="AggregationCode"> Code for this item aggregation </param>
        /// <returns> TRUE if this user is curator on either this aggregation or all of this library, otherwise FALSE </returns>
        public bool Is_Aggregation_Curator(string AggregationCode)
        {
            if (Is_System_Admin)
                return true;

            string aggrCodeUpper = AggregationCode.ToUpper();
            return (from thisAggregation in aggregations.Collection where thisAggregation.Code == aggrCodeUpper select thisAggregation.IsCurator).FirstOrDefault();
        }

        /// <summary> Checks to see if this user can edit all the items within this aggregation </summary>
        /// <param name="AggregationCode"> Code for this item aggregation </param>
        /// <returns> TRUE if this user is set to edit all items either this aggregation or all of this library, otherwise FALSE </returns>
        public bool Can_Edit_All_Items(string AggregationCode)
        {
            if (Is_System_Admin)
                return true;

            string aggrCodeUpper = AggregationCode.ToUpper();
            return (from thisAggregation in aggregations.Collection where thisAggregation.Code == aggrCodeUpper select thisAggregation.CanEditItems).FirstOrDefault();
        }

        /// <summary> This checks that the folder name exists, and returns the proper format </summary>
        /// <param name="name_version"> Version of the folder name to check </param>
        /// <returns> Folder name in proper format </returns>
        public string Folder_Name(string name_version)
        {
            User_Folder folderObject = Get_Folder(name_version);
            return folderObject == null ? String.Empty : folderObject.Folder_Name;
        }

        private void recurse_through_children(User_Folder parentFolder, SortedList<string, User_Folder> folder_builder)
        {
            foreach (User_Folder thisFolder in parentFolder.Children)
            {
                folder_builder.Add(thisFolder.Folder_Name, thisFolder);
                recurse_through_children(thisFolder, folder_builder);
            }
        }

        /// <summary> Get a folder obejct by folder name </summary>
        /// <param name="Folder_Name"> Name of the folder object to retrieve</param>
        /// <returns> Folder object by name </returns>
        public User_Folder Get_Folder(string Folder_Name)
        {
            string name_version_lower = Folder_Name.ToLower();
            return folders.Values.Select(thisFolder => recurse_to_get_folder(thisFolder, name_version_lower)).FirstOrDefault(returnValue => returnValue != null);
        }

        private User_Folder recurse_to_get_folder(User_Folder parentFolder, string folderName)
        {
            if ( parentFolder.Folder_Name.ToLower() == folderName )
                return parentFolder;

            return parentFolder.Children.Select(childFolder => recurse_to_get_folder(childFolder, folderName)).FirstOrDefault(returnValue => returnValue != null);
        }

        #endregion

        #region Internal methods for modifying the collections of editable objects ( bibid, templates, projects, aggregations, etc..)

        /// <summary> Clear all the user groups associated with this user  </summary>
        internal void Clear_UserGroup_Membership()
        {
            userGroups.Clear();
        }

        /// <summary> Adds a user group to the list of user groups this user belongs to </summary>
        /// <param name="GroupName"> Name of the user group</param>
        internal void Add_User_Group(string GroupName)
        {
            if ( !userGroups.Contains(GroupName ))
                userGroups.Add(GroupName);
        }

        /// <summary> Add an item to the list of items on the bookshelf for this user </summary>
        /// <param name="BibID"> Bibliographic identifier (BibID) for this item </param>
        /// <param name="VID"> Volume identifier (VID) for this item </param>
        internal void Add_Bookshelf_Item(string BibID, string VID)
        {
            string objid = BibID.ToUpper() + "_" + VID;
            if (!bookshelfObjectIds.Contains(objid))
                bookshelfObjectIds.Add(objid);
        }

        internal void Clear_Aggregations()
        {
            aggregations.Clear();
        }

        /// <summary> Add a new item aggregation to this user's collection of item aggregations </summary>
        /// <param name="Code">Code for this user editable item aggregation</param>
        /// <param name="Name">Name for this user editable item aggregation </param>
        /// <param name="CanSelect">Flag indicates if this user can add items to this item aggregation</param>
        /// <param name="CanEditItems">Flag indicates if this user can edit any items in this item aggregation</param>
        /// <param name="IsCurator"> Flag indicates if this user is listed as the curator or collection manager for this given digital aggregation </param>
        /// <param name="OnHomePage"> Flag indicates if this user has asked to have this aggregation appear on their personalized home page</param>
        internal void Add_Aggregation(string Code, string Name, bool CanSelect, bool CanEditItems, bool IsCurator, bool OnHomePage)
        {
            aggregations.Add(Code, Name, CanSelect, CanEditItems, IsCurator, OnHomePage );
        }

        /// <summary> Adds a BibID to the list of bibid's this user can edit </summary>
        /// <param name="BibID">New BibID this user can edit</param>
        internal void Add_BibID(string BibID)
        {
            bibids.Add(BibID);
        }

        /// <summary> Clears the list of templates associated with this user </summary>
        internal void Clear_Templates()
        {
            templates.Clear();
        }

        /// <summary> Adds a template to the list of templates this user can select </summary>
        /// <param name="Template">Code for this template</param>
        /// <remarks>This must match the name of one of the template XML files in the mySobek\templates folder</remarks>
        internal void Add_Template(string Template)
        {
            templates.Add(Template);
        }

        /// <summary> Sets the default template for this user </summary>
        /// <param name="Template">Code for this template</param>
        /// <remarks>This only sets this as the default template if it currently exists in the list of possible templates for this uers </remarks>
        internal void Set_Default_Template(string Template)
        {
            if ((!templates.Contains(Template)) || (templates.IndexOf(Template) == 0)) return;

            templates.Remove(Template);
            templates.Insert(0, Template);
        }

        /// <summary> Clears all projects associated with this user </summary>
        internal void Clear_Projects()
        {
            projects.Clear();
        }

        /// <summary> Adds a project to the list of projects this user can select </summary>
        /// <param name="Project">Code for this project</param>
        /// <remarks>This must match the name of one of the project METS (.pmets) files in the mySobek\projects folder</remarks>
        internal void Add_Project(string Project)
        {
            projects.Add(Project);
        }

        /// <summary> Sets the default project for this user </summary>
        /// <param name="Project">Code for this project</param>
        /// <remarks>This only sets this as the default project if it currently exists in the list of possible projects for this uers </remarks>
        internal void Set_Default_Project(string Project)
        {
            if ((!projects.Contains(Project)) || (projects.IndexOf(Project) == 0)) return;

            projects.Remove(Project);
            projects.Insert(0, Project);
        }

        /// <summary> Adds a regular expression to this user to determine which titles this user can edit </summary>
        /// <param name="Regular_Expression"> Regular expression used to compute if this user can edit a title, by BibID</param>
        internal void Add_Editable_Regular_Expression(string Regular_Expression)
        {
            editableRegexes.Add(Regular_Expression);
        }

        /// <summary> Adds a folder to the list of folders associated with this user </summary>
        /// <param name="Folder"> Built folder object </param>
        internal void Add_Folder(User_Folder Folder)
        {
            folders[Folder.Folder_Name] = Folder;
        }

        /// <summary> Adds a folder name to the list of folders associated with this user </summary>
        /// <param name="Folder_Name"> Name of the folder to add </param>
        /// <param name="Folder_ID"> Primary key for this folder </param>
        internal void Add_Folder(string Folder_Name, int Folder_ID )
        {
            folders[Folder_Name] = new User_Folder(Folder_Name, Folder_ID);
        }

        /// <summary> Removes a folder name from the list of folders associated with this user </summary>
        /// <param name="Folder_Name"> Name of the folder to remove </param>
        internal void Remove_Folder(string Folder_Name)
        {
            string delete_name_lower = Folder_Name.ToLower();
            for (int i = 0; i < folders.Count; i++)
            {
                if (folders.Values[i].Folder_Name.ToLower() != delete_name_lower) continue;

                folders.RemoveAt(i);
                break;
            }
        }

        /// <summary> Clear all the folders linked to this user object </summary>
        internal void Clear_Folders()
        {
            folders.Clear();
        }

        #endregion




        /// <summary> Determines if this user can edit this item, based on several different criteria </summary>
        /// <param name="Item">SobekCM Item to check</param>
        /// <returns>TRUE if the user can edit this item, otherwise FALSE</returns>
        public bool Can_Edit_This_Item(SobekCM_Item Item)
        {
            if (!SobekCM_Library_Settings.Online_Edit_Submit_Enabled)
                return false;

            if (Item.Bib_Info.SobekCM_Type == TypeOfResource_SobekCM_Enum.Project )
                return Is_System_Admin;

            if (bibids.Contains( Item.BibID.ToUpper()))
                return true;

            if (aggregations.Can_Edit("i" + Item.Bib_Info.Source.Code.ToUpper()))
                return true;

            if (Item.Bib_Info.hasLocationInformation)
            {
                if (aggregations.Can_Edit("i" + Item.Bib_Info.Location.Holding_Code.ToUpper()))
                    return true;
            }

            if (Item.Behaviors.Aggregation_Count > 0)
            {
                ReadOnlyCollection<Aggregation_Info> colls = Item.Behaviors.Aggregations;
                if (colls.Any(thisCollection => aggregations.Can_Edit(thisCollection.Code.ToUpper())))
                {
                    return true;
                }
            }

            return editableRegexes.Select(regex_string => new Regex(regex_string)).Any(myReg => myReg.IsMatch(Item.BibID.ToUpper()));
        }

        /// <summary> Returns the security hash based on IP for this user </summary>
        /// <param name="IP">IP Address for this user request</param>
        /// <returns>Security hash for comparison purposes or for encoding in the cookie</returns>
        /// <remarks>This is used to add another level of security on cookies coming in from a user request </remarks>
        public string Security_Hash(string IP)
        {
            return SecurityInfo.DES_EncryptString(Given_Name + "sobekh" + Family_Name, IP.Replace(".", "").PadRight(8, '%').Substring(0, 8), Email.Length > 8 ? Email.Substring(0, 8) : Email.PadLeft(8, 'd'));
        }
    }
}
