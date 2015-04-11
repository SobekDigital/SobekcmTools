namespace SobekCM.Library.Localization.Classes
{
    /// <summary> Localization class holds all the standard terms utilized by the PagedResults_HtmlSubwriter class </summary>
    public class PagedResults_HtmlSubwriter_LocalizationInfo : baseLocalizationInfo
    {
        /// <summary> Constructor for a new instance of the PagedResults_HtmlSubwriter_Localization class </summary>
        public PagedResults_HtmlSubwriter_LocalizationInfo()
        {
            // Set the source class name this localization file serves
            ClassName = "PagedResults_HtmlSubwriter";
        }

        /// <summary> Adds a localization string ( with key and value ) to this localization class </summary>
        /// <param name="Key"> Key for the new localization string being saved </param>
        /// <param name="Value"> Value for this localization string </param>
        /// <remarks> This overrides the base class's implementation </remarks>
        public override void Add_Localization_String(string Key, string Value)
        {
            // First, add to the localization string dictionary
            base.Add_Localization_String(Key, Value);

            // Assign to custom properties depending on the key
            switch (Key)
            {
                case "XXX XXX Of XXX Matching Coordinates":
                    XXXXXXOfXXXMatchingCoordinates = Value;
                    break;

                case "XXX XXX Of XXX Matching Flights":
                    XXXXXXOfXXXMatchingFlights = Value;
                    break;

                case "XXX XXX Of XXX Matching Titles":
                    XXXXXXOfXXXMatchingTitles = Value;
                    break;

                case "XXX Items For Export":
                    XXXItemsForExport = Value;
                    break;

                case "Add This To Your Saved Searches":
                    AddThisToYourSavedSearches = Value;
                    break;

                case "And":
                    And = Value;
                    break;

                case "Bibid Ascending":
                    BibidAscending = Value;
                    break;

                case "Bibid Descending":
                    BibidDescending = Value;
                    break;

                case "Bookshelf View":
                    BookshelfView = Value;
                    break;

                case "BRIEF":
                    BRIEF = Value;
                    break;

                case "Brief View":
                    BriefView = Value;
                    break;

                case "Browse":
                    Browse = Value;
                    break;

                case "Comments":
                    Comments = Value;
                    break;

                case "Counties":
                    Counties = Value;
                    break;

                case "Date Added":
                    DateAdded = Value;
                    break;

                case "Date Ascending":
                    DateAscending = Value;
                    break;

                case "Date Descending":
                    DateDescending = Value;
                    break;

                case "Description":
                    Description = Value;
                    break;

                case "Enter Notes For This Browse":
                    EnterNotesForThisBrowse = Value;
                    break;

                case "Enter Notes For This Public Bookshelf":
                    EnterNotesForThisPublicBookshelf = Value;
                    break;

                case "Enter Notes For This Search":
                    EnterNotesForThisSearch = Value;
                    break;

                case "Enter The Email Inforamtion Below":
                    EnterTheEmailInforamtionBelow = Value;
                    break;

                case "ERROR Encountered While Saving Your Search":
                    ERROREncounteredWhileSavingYourSearch = Value;
                    break;

                case "ERROR Encountered While Saving":
                    ERROREncounteredWhileSaving = Value;
                    break;

                case "Export":
                    Export = Value;
                    break;

                case "HTML":
                    HTML = Value;
                    break;

                case "In":
                    In = Value;
                    break;

                case "Map View":
                    MapView = Value;
                    break;

                case "NARROW RESULTS BY":
                    NARROWRESULTSBY = Value;
                    break;

                case "Not":
                    Not = Value;
                    break;

                case "One County":
                    OneCounty = Value;
                    break;

                case "One Title":
                    OneTitle = Value;
                    break;

                case "Or":
                    Or = Value;
                    break;

                case "Plain Text":
                    PlainText = Value;
                    break;

                case "Public Bookshelf":
                    PublicBookshelf = Value;
                    break;

                case "Rank":
                    Rank = Value;
                    break;

                case "Resulted In XXX Flights In":
                    ResultedInXXXFlightsIn = Value;
                    break;

                case "Resulted In XXX Items In":
                    ResultedInXXXItemsIn = Value;
                    break;

                case "Resulted In XXX Matching Flights":
                    ResultedInXXXMatchingFlights = Value;
                    break;

                case "Resulted In XXX Matching Records":
                    ResultedInXXXMatchingRecords = Value;
                    break;

                case "Resulted In No Matching Flights":
                    ResultedInNoMatchingFlights = Value;
                    break;

                case "Resulted In No Matching Records":
                    ResultedInNoMatchingRecords = Value;
                    break;

                case "Resulted In One Flight In":
                    ResultedInOneFlightIn = Value;
                    break;

                case "Resulted In One Item In":
                    ResultedInOneItemIn = Value;
                    break;

                case "Resulted In One Matching Flight":
                    ResultedInOneMatchingFlight = Value;
                    break;

                case "Resulted In One Matching Record":
                    ResultedInOneMatchingRecord = Value;
                    break;

                case "Search":
                    Search = Value;
                    break;

                case "Search Has Been Saved To Your Saved Searches":
                    SearchHasBeenSavedToYourSavedSearches = Value;
                    break;

                case "Show Less":
                    ShowLess = Value;
                    break;

                case "Show More":
                    ShowMore = Value;
                    break;

                case "Sort Alphabetically":
                    SortAlphabetically = Value;
                    break;

                case "Sort By":
                    SortBy = Value;
                    break;

                case "Sort By Frequency":
                    SortByFrequency = Value;
                    break;

                case "Table View":
                    TableView = Value;
                    break;

                case "THUMB":
                    THUMB = Value;
                    break;

                case "Thumbnail View":
                    ThumbnailView = Value;
                    break;

                case "Title":
                    Title = Value;
                    break;

                case "Titles":
                    Titles = Value;
                    break;

                case "To":
                    To = Value;
                    break;

                case "UNRECOGNIZED SEARCH":
                    UNRECOGNIZEDSEARCH = Value;
                    break;

                case "Your Email Has Been Sent":
                    YourEmailHasBeenSent = Value;
                    break;

                case "Your Geographic Search Of XXX":
                    YourGeographicSearchOfXXX = Value;
                    break;

                case "Your Search Of XXX For":
                    YourSearchOfXXXFor = Value;
                    break;

            }
        }
        /// <remarks> '{0} - {1} of {2} matching coordinates' localization string </remarks>
        public string XXXXXXOfXXXMatchingCoordinates { get; private set; }

        /// <remarks> '{0} - {1} of {2} matching flights' localization string </remarks>
        public string XXXXXXOfXXXMatchingFlights { get; private set; }

        /// <remarks> '{0} - {1} of {2} matching titles' localization string </remarks>
        public string XXXXXXOfXXXMatchingTitles { get; private set; }

        /// <remarks> "i.e., 12 items for export" </remarks>
        public string XXXItemsForExport { get; private set; }

        /// <remarks> Title for the saved searches pop-up form </remarks>
        public string AddThisToYourSavedSearches { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string And { get; private set; }

        /// <remarks> 'BibID Ascending' localization string </remarks>
        public string BibidAscending { get; private set; }

        /// <remarks> 'BibID Descending' localization string </remarks>
        public string BibidDescending { get; private set; }

        /// <remarks> 'Bookshelf View' localization string </remarks>
        public string BookshelfView { get; private set; }

        /// <remarks> 'BRIEF' localization string </remarks>
        public string BRIEF { get; private set; }

        /// <remarks> 'Brief View' localization string </remarks>
        public string BriefView { get; private set; }

        /// <remarks> 'Browse' localization string </remarks>
        public string Browse { get; private set; }

        /// <remarks> 'Comments:' localization string </remarks>
        public string Comments { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string Counties { get; private set; }

        /// <remarks> Sort value (rest are standard metadata terms) </remarks>
        public string DateAdded { get; private set; }

        /// <remarks> 'Date Ascending' localization string </remarks>
        public string DateAscending { get; private set; }

        /// <remarks> 'Date Descending' localization string </remarks>
        public string DateDescending { get; private set; }

        /// <remarks> Add description to the saved search </remarks>
        public string Description { get; private set; }

        /// <remarks> Prompt for adding notes to a browse during save </remarks>
        public string EnterNotesForThisBrowse { get; private set; }

        /// <remarks> Prompt for adding notes to a public bookshelf during save </remarks>
        public string EnterNotesForThisPublicBookshelf { get; private set; }

        /// <remarks> Prompt for adding notes to a search during save </remarks>
        public string EnterNotesForThisSearch { get; private set; }

        /// <remarks> 'Enter the email inforamtion below' localization string </remarks>
        public string EnterTheEmailInforamtionBelow { get; private set; }

        /// <remarks> Error occurs during save! </remarks>
        public string ERROREncounteredWhileSavingYourSearch { get; private set; }

        /// <remarks> 'ERROR encountered while saving!' localization string </remarks>
        public string ERROREncounteredWhileSaving { get; private set; }

        /// <remarks> 'Export' localization string </remarks>
        public string Export { get; private set; }

        /// <remarks> 'HTML' localization string </remarks>
        public string HTML { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string In { get; private set; }

        /// <remarks> 'Map View' localization string </remarks>
        public string MapView { get; private set; }

        /// <remarks> Title on the facet column </remarks>
        public string NARROWRESULTSBY { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string Not { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string OneCounty { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string OneTitle { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string Or { get; private set; }

        /// <remarks> 'Plain Text' localization string </remarks>
        public string PlainText { get; private set; }

        /// <remarks> 'Public Bookshelf' localization string </remarks>
        public string PublicBookshelf { get; private set; }

        /// <remarks> 'Rank' localization string </remarks>
        public string Rank { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string ResultedInXXXFlightsIn { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string ResultedInXXXItemsIn { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string ResultedInXXXMatchingFlights { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string ResultedInXXXMatchingRecords { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string ResultedInNoMatchingFlights { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string ResultedInNoMatchingRecords { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string ResultedInOneFlightIn { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string ResultedInOneItemIn { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string ResultedInOneMatchingFlight { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string ResultedInOneMatchingRecord { get; private set; }

        /// <remarks> 'Search' localization string </remarks>
        public string Search { get; private set; }

        /// <remarks> Message when a user saves a search from the search results screen </remarks>
        public string SearchHasBeenSavedToYourSavedSearches { get; private set; }

        /// <remarks> Used in the facet colum </remarks>
        public string ShowLess { get; private set; }

        /// <remarks> Used in the facet colum </remarks>
        public string ShowMore { get; private set; }

        /// <remarks> Used in the facet colum </remarks>
        public string SortAlphabetically { get; private set; }

        /// <remarks> 'Sort by' localization string </remarks>
        public string SortBy { get; private set; }

        /// <remarks> Used in the facet colum </remarks>
        public string SortByFrequency { get; private set; }

        /// <remarks> 'Table View' localization string </remarks>
        public string TableView { get; private set; }

        /// <remarks> 'THUMB' localization string </remarks>
        public string THUMB { get; private set; }

        /// <remarks> 'Thumbnail View' localization string </remarks>
        public string ThumbnailView { get; private set; }

        /// <remarks> 'Title' localization string </remarks>
        public string Title { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string Titles { get; private set; }

        /// <remarks> 'To:' localization string </remarks>
        public string To { get; private set; }

        /// <remarks> 'UNRECOGNIZED SEARCH' localization string </remarks>
        public string UNRECOGNIZEDSEARCH { get; private set; }

        /// <remarks> Message when a user sends search results via email to a friend </remarks>
        public string YourEmailHasBeenSent { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string YourGeographicSearchOfXXX { get; private set; }

        /// <remarks> Used to recreate the user's search in clear language </remarks>
        public string YourSearchOfXXXFor { get; private set; }

    }
}
