namespace SobekCM.Library.Navigation
{
	/// <summary> iSobekCM_Navigation_Object is the interface which any 
	/// Navigation Object must implement</summary>
	public interface iSobekCM_Navigation_Object
	{
		/// <summary> Returns the URL to redirect the user's browser, based on the current
		/// mode and specifics for this mode. </summary>
		/// <returns> String to be attached to the end of the main application name to redirect
		/// the current user's browser.  </returns>
		string Redirect_URL();

		/// <summary> Returns the URL to redirect the user's browser, based on the current
		/// mode and specifics for this mode. </summary>
		/// <param name="Item_View_Code">Item view code to display</param>
		/// <param name="Include_URL_Opts"> Flag indicates whether to include URL opts or not </param>
		/// <returns> String to be attached to the end of the main application name to redirect
		/// the current user's browser.  </returns>
		string Redirect_URL(string Item_View_Code, bool Include_URL_Opts );

		/// <summary> Returns the URL options the user has currently set </summary>
		/// <returns>URL options</returns>
		string URL_Options();
	}

	/// <summary> Writer type which determines what type of response is requested </summary>
	/// <remarks> This generally corresponds to which type of Main Writer is used, and which ASP application replies<br /><br />
	/// This value generally comes from the first character of the mode value in the URL query string ( m=X... ) </remarks>
	public enum Writer_Type_Enum : byte
	{
		/// <summary> Response should be in HTML </summary>
		HTML = 1,

		/// <summary> Simple writer just echoes through an existing HTML page through this application 
		/// with very little logic (used for robot search engines mostly) </summary>
		HTML_Echo,

		/// <summary> Response should be in HTML, but the user is logged in</summary>
		/// <remarks>This seperate writer type is implemented to force the user's system to refresh when
		/// the user's logon state changes.</remarks>
		HTML_LoggedIn,

		/// <summary> Response should be in microsoft compliant dataset format </summary>
		/// <remarks>This type of request is forwarded to sobekcm_data.aspx ( or ufdc_data.aspx)<br /><br />
		/// Generally, this is achieved by just using the standard write to xml routines in a DataSet</remarks>
		DataSet,

		/// <summary> Response should be in simplified JSON (Javan Simple Object Notation) format </summary>
		/// <remarks>This type of request is forwarded to sobekcm_data.aspx ( or ufdc_data.aspx)<br /><br />
		/// This is used to provide support to the iPhone application</remarks>
		JSON,

		/// <summary> Response should be compliant with the OAI-PMH standard </summary>
		/// <remarks>This type of request is forwarded to sobekcm_oai.aspx ( or ufdc_oai.aspx)</remarks>
		OAI,

		/// <summary> Response should be in HTML, but be simple text, not formatted </summary>
		Text,

		/// <summary> Response should be in simplified XML format </summary>
		/// <remarks>This type of request is forwarded to sobekcm_data.aspx ( or ufdc_data.aspx)</remarks>
		XML
	};

	/// <summary> Display mode which determines what major category of action is being requested </summary>
	/// <remarks> For HTML rendering, these roughly correspond to which HTML subwriter to use.<br /><br />
	/// This value generally comes from the second character of the mode value in the URL query string ( m=.X... ) </remarks>
	public enum Display_Mode_Enum : byte
	{
		/// <summary>No display mode set</summary>
		None,

		/// <summary>An error was encountered during processing</summary>
		Error,

        /// <summary> System and portal administrator tasks </summary>
        Administrative,

		/// <summary> Admin view of the aggregation information </summary>
		Aggregation_Admin_View,

		/// <summary>Browse item metadata values within an aggregation </summary>
		Aggregation_Browse_By,

		/// <summary>Browse items within an aggregation or static html collection pages</summary>
		Aggregation_Browse_Info,

		/// <summary> View all the coordinates linked to this item aggregation on a map </summary>
		Aggregation_Browse_Map,

		/// <summary>Display the home page for an aggregation (and default search option)</summary>
		Aggregation_Home,

		/// <summary> Display the item count information for a single item aggregation  </summary>
		Aggregation_Item_Count,

		/// <summary> Display the list of private items for a single item aggregation </summary>
		Aggregation_Private_Items,

		/// <summary> Display the usage statistics for a single item aggregation </summary>
		Aggregation_Usage_Statistics,

		/// <summary>Request to send an email and contact the digital library team</summary>
		Contact,

		/// <summary>Displays the congratulations screen after the user submits a contact us email</summary>
		Contact_Sent,

		/// <summary>Display internal information about this digital library</summary>
		Internal,

		/// <summary>Just reload the information about each of the items into the cache</summary>
		Item_Cache_Reload,

		/// <summary>Display a single item</summary>
		Item_Display,

		/// <summary>Renders a single item for printing </summary>
		Item_Print,

		/// <summary>Used to redirect from a legacy URL to a new URL, during URL transition times</summary>
		Legacy_URL,

		/// <summary>Display is something from the authenticated portion of mySobek</summary>
		My_Sobek,

		/// <summary>Simple preferences (not related to mySobek) are displayed, allowing user to select language, etc..</summary>
		Preferences,

		/// <summary> Display a user's public folder </summary>
		Public_Folder,

		/// <summary>Full memory reset was requested</summary>
		Reset,

		/// <summary>Display results of a search against items within an aggregation</summary>
		Results,

		/// <summary>Search items within an aggregation</summary>
		Search,

		/// <summary>Simply works as a content management system, displaying static html pages within an interface</summary>
		Simple_HTML_CMS,

		/// <summary>Display statistical information about item counts and usage</summary>
		Statistics
	};

	/// <summary> Type of search type to display </summary>
	/// <remarks> This roughly corresponds to the collection viewer used by the collection html subwriter</remarks>
	public enum Search_Type_Enum : byte
	{
		/// <summary> Advanced search type allows boolean searching with four different search fields </summary>
		Advanced,

		/// <summary> Basic search type allows metadata searching with one search field</summary>
		Basic,

		/// <summary> dLOC-specific full text search against the text of all documents in an item aggregation </summary>
		dLOC_Full_Text,

		/// <summary> Full text searches against the text of all the documents in an item aggregation </summary>
		Full_Text,

		/// <summary> Map searching employs a map to allow the user to select a rectangle of interest</summary>
		Map,

		/// <summary> Newspaper search type allows searching with one search field and suggests several metadata fields to search (i.e., newspaper title, full text, location, etc..)</summary>
		Newspaper,

	};

	/// <summary> Preciseness or exactness of the search </summary>
	public enum Search_Precision_Type_Enum : byte
	{
		/// <summary> Searches for inflectional forms of the search word(s), for example plural, past tense, etc.. </summary>
		Inflectional_Form = 1,

		/// <summary> Searches for synonmic forms of the search word(s) by using a thesaurus </summary>
		Synonmic_Form,

		/// <summary> Results must contain the actual word, with no stemming or alternative searches </summary>
		Contains,

		/// <summary> Results must exactly match the search term (which does not use the full-text indexing) </summary>
		Exact_Match
	}

	/// <summary> Format to display search or browse results </summary>
	/// <remarks> This roughly corresponds to the result viewer used by the results (or collection) html subwriter</remarks>
	public enum Result_Display_Type_Enum : byte
	{
		/// <summary> Default result display type means that not particular type was selected
		/// and the item aggregation default is utilized </summary>
		Default = 0,

		/// <summary> Displays the results in the bookshelf view, which allows the user to remove the item
		/// from the bookshelf, move the item, or edit the user notes </summary>
		Bookshelf = 1,

		/// <summary> Display the results in a brief metadata format (with thumbnails) </summary>
		Brief =2,

		/// <summary> Allows results to be exported as CSV or excel files </summary>
		Export = 3,

		/// <summary> Display the full citation of a single result</summary>
		Full_Citation = 4,

		/// <summary> Display the main image of a single result</summary>
		Full_Image = 5,

		/// <summary> Display the results according to their main coordinate information</summary>
		Map = 6,

		/// <summary> Static text-type browse/info mode </summary>
		Static_Text = 7,

		/// <summary> Display the results in a simple table format </summary>
		Table = 8,

		/// <summary> Display the results in a thumbnail format</summary>
		Thumbnails = 9
	};

	/// <summary> Type of administrative information requested for display </summary>
	public enum Internal_Type_Enum : byte
	{ 
		/// <summary> Gets the complete list of all aggregations </summary>
		Aggregations_List,

		/// <summary> Gets list of recent failures encountered during building </summary>
		Build_Failures,

		/// <summary> Display list of all items in memory; global, cache, and session </summary>
		Cache,

		/// <summary> Display list of aggregations of one particular type</summary>
		Aggregations,

		/// <summary> Display list of all new and modified items in last week (or so) </summary>
		New_Items,

		/// <summary> Gets thumbnails of all current wordmarks, along with the wordmark code </summary>
		/// <remarks> This is used during online metadata editing to aid the user in selecting a wordmark</remarks>
		Wordmarks 
	};

	/// <summary> Type of home page to display (for the main library home page)</summary>
	public enum Home_Type_Enum : byte
	{

		/// <summary> Display icons with full descriptions </summary>
		Descriptions, 

		/// <summary> Display standard list with icons [DEFAULT] </summary>
		List,

		/// <summary> Displays the list of all the institutional partners </summary>
		Partners_List,

		/// <summary> Displays the thumbnails of all the institutional partners </summary>
		Partners_Thumbnails,

		/// <summary> Display the personalized home page for logged on users </summary>
		Personalized,

		/// <summary> Display the hierarchical tree view, initially fully collapsed </summary>
		Tree_Collapsed,

		/// <summary> Display the hierarchical tree view, initially fully expanded </summary>
		Tree_Expanded
	};

	/// <summary> Type of statistical information to display </summary>
	public enum Statistics_Type_Enum : byte
	{ 
		/// <summary> Displays the item count for an arbitrary date and the growth from the first arbitrary date and the second date </summary>
		Item_Count_Arbitrary_View = 1,

		/// <summary> Displays the current number of items in each aggregation, as well as growth during the last FYTD </summary>
		Item_Count_Growth_View,

		/// <summary> Displays the current number of items in each aggregation </summary>
		Item_Count_Standard_View,

		/// <summary> Displays the current number of items in each aggregation, in comma-seperate value text</summary>
		Item_Count_Text,

		/// <summary> Displays list of recent searches performed against this digital library</summary>
		Recent_Searches,

		/// <summary> Displays the overall usage at the aggregation-level by date, in comma-seperate value text</summary>
		Usage_By_Date_Text,

		/// <summary> Displays the overall usage at the aggregation-level by date</summary>
		Usage_Collections_By_Date,

		/// <summary> Displays the overall usage by date of a single item aggregation</summary>
		Usage_Collection_History,

		/// <summary> Displays the overall usage by date of a single item aggregation, in comma-seperate value text</summary>
		Usage_Collection_History_Text,

		/// <summary> Displays the definitions of the terms used in the statistical screens</summary>
		Usage_Definitions,

		/// <summary> Displays the most often used items within a single item aggregation</summary>
		Usage_Items_By_Collection,

		/// <summary> Displays the overall usage at the item-level by date</summary>
		Usage_Item_Views_By_Date,

		/// <summary> Displays the most often used titles within a single item aggregation</summary>
		Usage_Titles_By_Collection,

		/// <summary> Displays the overall usage against the overall architecture</summary>
		Usage_Overall
	};

	/// <summary> Type of mySobek display or action requested by the user </summary>
	public enum My_Sobek_Type_Enum : byte
	{
		/// <summary> Allows system administrators the ability to delete an item online </summary>
		Delete_Item,
	
		/// <summary> Edit the behaviors of an existing item group within this digital library </summary>
		Edit_Group_Behaviors,

		/// <summary> Edit the serial hierarchy for all items under an existing item group </summary>
		Edit_Group_Serial_Hierarchy,

		/// <summary> Edits the behaviors of an existing item within this digital library </summary>
		Edit_Item_Behaviors,

		/// <summary> Edit an existing item through the online metadata editing process </summary>
		Edit_Item_Metadata,

		/// <summary> Edit the files related to a digial resource, by deleting or uploading new files </summary>
		File_Management,

		/// <summary> View a current folder (or bookshelf) or perform folder management on saved items </summary>
		Folder_Management,

		/// <summary> Page is used as the landing page when coming back from Shibboleth authentication </summary>
		Shibboleth_Landing,

		/// <summary> Add a new volume to an existing item group  </summary>
		Group_Add_Volume,

		/// <summary> Auto-fill multiple new volumes to an existing item group </summary>
		Group_AutoFill_Volumes,

		/// <summary> Mass update the behaviors for all the items within a particular item group </summary>
		Group_Mass_Update_Items,

		/// <summary> Display the mySobek home page, with possible courses of actions </summary>
		Home,

		/// <summary> Logout of mySobek and return to non-authenticated (public) mode </summary>
		Log_Out,

		/// <summary> Log on to mySobek and enter authenticated (private) mode </summary>
		Logon,

		/// <summary> Submit a new item through the online submittal process </summary>
		New_Item,

		/// <summary> Change your current password (or temporary password) </summary>
		New_Password,

		/// <summary> Edit user-based preferences and user information </summary>
		Preferences,

		/// <summary> List of all saved searches for this user </summary>
		Saved_Searches,

		/// <summary> Provides way to view all user lists entered by a user or by aggregation </summary>
		User_Tags,

		/// <summary> Provides a list of all items linked to a user along with usage statistics for a given month/year </summary>
		User_Usage_Stats
	};

    /// <summary> Type of admin display or action requested by the system or portal administrator </summary>
    public enum Admin_Type_Enum : byte
    {
        /// <summary> Allows all the information and behaviors for a single aggregation to be viewed / edited </summary>
        Aggregation_Single = 1,

        /// <summary> Provides list of all existing aggregations and allows admin to enter a new aggregation </summary>
        Aggregations_Mgmt,

        /// <summary> Gives the current SobekCM status and allows an authenticated system admin to temporarily halt the builder remotely via a database flag </summary>
        Builder_Status,

        /// <summary> Provides list of all forwards (or collection aliases) and allows admin to perform some very basic tasks </summary>
        Forwarding,

        /// <summary> Administrative home page with links to all the Admin tasks </summary>
        Home,

        /// <summary> Provides list of all existing interfaces and allows admin to enter a new interface or edit an existing interface </summary>
        Interfaces,

        /// <summary> Provides list of the IP restriction lists and allows admins to edit the single IPs within the range(s) </summary>
        IP_Restrictions,

        /// <summary> Provides list of all existing projects and allows admin to enter a new project or edit an existing project </summary>
        Projects,

        /// <summary> Allows admin to perform some limited cache reset functions </summary>
        Reset,

        /// <summary> Allows admins to view and edit system-wide settings from the database </summary>
        Settings,

        /// <summary> Allows the system administrator to add new thematic headings to the main home page </summary>
        Thematic_Headings,

        /// <summary> Allows admin to perform some limited actions against the URL Portals data </summary>
        URL_Portals,

        /// <summary> Provides list of all users and allows admin to perform some very basic tasks </summary>
        Users,

        /// <summary> Allows for editing and viewing of user groups </summary>
        User_Groups,

        /// <summary> Provides list of all existing wordmarks/icons and allows admin to enter a new wordmark or edit an existing wordmark </summary>
        Wordmarks
    };


	/// <summary> Flag is used to determine whether the table of contents should be displayed in the item viewer </summary>
	public enum TOC_Display_Type_Enum : byte
	{ 
		/// <summary> Display is not explicitly stated, so use the last value in the session state </summary>
		Undetermined = 1, 

		/// <summary> Display the table of contents </summary>
		Show,

		/// <summary> Hide the table of contents </summary>
		Hide 
	};

	/// <summary> Flag is used to determine whether the trace route should be included at the end of the html page </summary>
	public enum Trace_Flag_Type_Enum : byte
	{ 
		/// <summary> No data collection on whether to show the trace route</summary>
		Unspecified = 0, 

		/// <summary> Do not display the trace route information </summary>
		No = 1, 

		/// <summary> Show the trace route information; it was explicitely requested in the URL </summary>
		Explicit,

		/// <summary> Show the trace route information, although it was not requested in the URL </summary>
		/// <remarks> Trace route can be requested either by IP address, or by currently logged on user</remarks>
		Implied
	};
}
