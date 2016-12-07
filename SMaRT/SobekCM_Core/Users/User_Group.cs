﻿#region Using directives

using System.Collections.Generic;
using System.Runtime.Serialization;

#endregion

namespace SobekCM.Core.Users
{
    /// <summary> Represents a mySobek user group, which allows some permissions and information to be assigned
    /// to a collection of individual mySobek users.  This contains all the information about a single user group </summary>
    [DataContract]
    public class User_Group
    {
        #region Private class members

        private User_Aggregation_Permissions aggregations;
        private List<string> editableRegexes;
        private List<string> defaultMetadataSets;
        private List<string> templates;
        private List<User_Group_Member> users;

        #endregion

        #region Constructor

        /// <summary> Constructor for a new instance of the User_Group_Complete class </summary>
        /// <param name="Name">Name for this SobekCM user group</param>
        /// <param name="Description">Description for this SobekCM user group</param>
        /// <param name="UserGroupID">UserGroupID (or primary key) to this user group from the database</param>
        public User_Group(string Name, string Description, int UserGroupID)
        {
            this.Name = Name;
            this.Description = Description;
            this.UserGroupID = UserGroupID;
            CanSubmit = false;
            IsInternalUser = false;
            IsSystemAdmin = false;
            IsPortalAdmin = false;
            IsShibbolethDefault = false;
            IsSobekDefault = false;
            IsLdapDefault = false;
        }

        #endregion

        #region Public Properties

        /// <summary> Name for this SobekCM user group </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary> Description for this SobekCM user group </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary> UserGroupID (or primary key) to this user group from the database </summary>
        [DataMember]
        public int UserGroupID { get; set; }

        /// <summary> Simple flag indicates if this user group can submit items </summary>
        [DataMember]
        public bool CanSubmit { get; set; }

        /// <summary> Flag indicates if this is an internal user group </summary>
        /// <remarks>This grants access to various tracking elements in SobekCM</remarks>
        [DataMember]
        public bool IsInternalUser { get; set; }

        /// <summary> Flag indicates if this user group has general admin rights over the entire system </summary>
        [DataMember]
        public bool IsSystemAdmin { get; set; }

        /// <summary> Flag indicates if this is a special user group (reserved by the system), such as 'Everyone' </summary>
        [DataMember]
        public bool IsSpecialGroup { get; set;  }

        /// <summary> Flag indicates if this user group should be assigned, by default, to new users
        /// that register via Sobek authentication  </summary>
        [DataMember]
        public bool IsSobekDefault { get; set; }

        /// <summary> Flag indicates if this user group should be assigned, by default, to new users
        /// that register via Shibboleth authentication  </summary>
        [DataMember]
        public bool IsShibbolethDefault { get; set; }

        /// <summary> Flag indicates if this user group should be assigned, by default, to new users
        /// that register via LDAP authentication  </summary>
        [DataMember]
        public bool IsLdapDefault { get; set; }

        /// <summary> Flag indicates if this is a portal administrator, with top-level permissions on this instance </summary>
        /// <remarks> Portal administrators have less rights, in general, than system administrators </remarks>
        [DataMember]
        public bool IsPortalAdmin { get; set; }

        /// <summary> Flag is used when editing a users rights to indicate this user should be able to edit ALL items in the library </summary>
        public bool Should_Be_Able_To_Edit_All_Items { get; set; }

        /// <summary> Ordered list of submittal templates this user group has access to </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<string> Templates
        {
            get { return templates; }
        }

        /// <summary> Ordered list of default metadata sets this user group has access to </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<string> Default_Metadata_Sets
        {
            get { return defaultMetadataSets; }
        }

        /// <summary> List of item aggregationPermissions associated with this user group </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<User_Permissioned_Aggregation> Aggregations
        {
            get
            {
                if ( aggregations != null ) return aggregations.Aggregations;
                return null;
            }
        }

        /// <summary> List of regular expressions for checking for edit by bibid </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<string> Editable_Regular_Expressions
        {
            get { return editableRegexes; }
        }

        /// <summary> Gets the list of users associated with this user group </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<User_Group_Member> Users
        {
            get { return users; }
        }

        /// <summary> Return the number of templates tied to this user group </summary>
        public int Templates_Count
        {
            get { return templates == null ? 0 : templates.Count; }
        }

        /// <summary> Return the number of default metadata sets tied to this user group </summary>
        public int Default_Metadata_Sets_Count
        {
            get { return defaultMetadataSets == null ? 0 : defaultMetadataSets.Count; }
        }

        /// <summary> Return the number of aggregations tied to this user group </summary>
        public int Aggregations_Count
        {
            get { return aggregations == null ? 0 : aggregations.Aggregations.Count; }
        }

        /// <summary> Return the number of users tied to this user group </summary>
        public int Users_Count
        {
            get { return users == null ? 0 : users.Count; }
        }

        #endregion

        #region Methods for modifying the collections of editable objects ( bibid, templates, projects, aggregationPermissions, etc..)

        /// <summary> Adds a user to the list of users which belong to this user group </summary>
        /// <param name="User"> Small user object which holds the very basic information about this user </param>
        public void Add_User(User_Group_Member User)
        {
            if (users == null) users = new List<User_Group_Member>();

            users.Add(User);
        }

        /// <summary> Adds a user to the list of users which belong to this user group </summary>
        /// <param name="UserName">SobekCM username for this user</param>
        /// <param name="Full_Name">Returns the user's full name in [first name last name] order</param>
        /// <param name="Email">User's email address</param>
        /// <param name="UserID">serID (or primary key) to this user from the database</param>
        public void Add_User(string UserName, string Full_Name, string Email, int UserID)
        {
            if (users == null) users = new List<User_Group_Member>();

            users.Add(new User_Group_Member(UserName, Full_Name, Email, UserID));
        }

        /// <summary> Add a new item aggregation to this user group's collection of item aggregationPermissions </summary>
        /// <param name="Code">Code for this user editable item aggregation</param>
        /// <param name="Aggregation_Name">Name for this user editable item aggregation </param>
        /// <param name="CanSelect">Flag indicates if this user group can add items to this item aggregation</param>
        /// <param name="CanDelete"> Flag indicates if the user group can delete items in this aggregation </param>
        /// <param name="IsCurator"> Flag indicates if this user group is listed as the curator or collection manager for this given digital aggregation </param>
        /// <param name="CanEditMetadata"> Flag indicates if the user group can edit metadata for all items in this aggregation </param>
        /// <param name="CanEditBehaviors"> Flag indicates if the user group can edit behaviors for all items in this aggregation  </param>
        /// <param name="CanPerformQc"> Flag indicates if the user group can edit perform quality control for all items in this aggregation  </param>
        /// <param name="CanUploadFiles"> Flag indicates if the user group can edit upload files for all items in this aggregation  </param>
        /// <param name="CanChangeVisibility"> Flag indicates if the user group can change the visibility for all items in this aggregation  </param>
        /// <param name="IsAdmin"> Flag indicates if membership in this group are administrators on the aggregation </param>
        public void Add_Aggregation(string Code, string Aggregation_Name, bool CanSelect, bool CanEditMetadata, bool CanEditBehaviors, bool CanPerformQc, bool CanUploadFiles, bool CanChangeVisibility, bool CanDelete, bool IsCurator, bool IsAdmin)
        {
            if (aggregations == null) aggregations = new User_Aggregation_Permissions();

            aggregations.Add(Code, Aggregation_Name, CanSelect, CanEditMetadata, CanEditBehaviors, CanPerformQc, CanUploadFiles, CanChangeVisibility, CanDelete, IsCurator, false, IsAdmin, true );
        }

        /// <summary> Add a new item aggregation to this user group's collection of item aggregationPermissions </summary>
        /// <param name="PermissionedAggregation">Fully permissioned aggregation </param>
        public void Add_Aggregation(User_Permissioned_Aggregation PermissionedAggregation )
        {
            if (aggregations == null) aggregations = new User_Aggregation_Permissions();

            aggregations.Add(PermissionedAggregation);
        }

        /// <summary> Adds a template to the list of templates this user group can select </summary>
        /// <param name="Template">Code for this template</param>
        /// <remarks>This must match the name of one of the template XML files in the mySobek\templates folder</remarks>
        public void Add_Template(string Template)
        {
            if (templates == null) templates = new List<string>();

            templates.Add(Template);
        }

        /// <summary> Adds a default metadata set to the list of sets this user group can select </summary>
        /// <param name="MetadataSet">Code for this default metadata set</param>
        /// <remarks>This must match the name of one of the project METS (.pmets) files in the mySobek\projects folder</remarks>
        public void Add_Default_Metadata_Set(string MetadataSet)
        {
            if (defaultMetadataSets == null) defaultMetadataSets = new List<string>();

            defaultMetadataSets.Add(MetadataSet);
        }

        /// <summary> Adds a regular expression to this user group to determine which titles this user can edit </summary>
        /// <param name="Regular_Expression"> Regular expression used to compute if this user group can edit a title, by BibID</param>
        public void Add_Editable_Regular_Expression(string Regular_Expression)
        {
            if (editableRegexes == null) editableRegexes = new List<string>();

            editableRegexes.Add(Regular_Expression);
        }

        #endregion
    }


}
