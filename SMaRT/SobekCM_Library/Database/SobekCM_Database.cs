#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.ApplicationBlocks.Data;
using SobekCM.Resource_Object;
using SobekCM.Library.Aggregations;
using SobekCM.Library.Application_State;
using SobekCM.Library.HTML;
using SobekCM.Library.Items;
using SobekCM.Library.Items.Authority;
using SobekCM.Library.MainWriters;
using SobekCM.Library.Results;
using SobekCM.Library.Settings;
using SobekCM.Library.Users;
using SobekCM.Tools;
using SobekCM.Tools.FDA;

#endregion

namespace SobekCM.Library.Database
{
	/// <summary> Gateway to the databases used by SobekCM </summary>
	public class SobekCM_Database
	{
		private const int MAX_PAGE_LOOKAHEAD = 4;
		private const int MIN_PAGE_LOOKAHEAD = 2;
		private const int LOOKAHEAD_FACTOR = 5;
		private const int ALL_AGGREGATIONS_METADATA_COUNT_TO_USE_CACHED = 1000;

		private static string connectionString;
		private static Exception lastException;

		private static readonly Object itemListPopulationLock = new Object();

		/// <summary> Gets the last exception caught by a database call through this gateway class  </summary>
		public static Exception Last_Exception
		{
			get { return lastException; }
		}

		/// <summary> Connection string to the main SobekCM databaase </summary>
		/// <remarks> This database hold all the information about items, item aggregations, statistics, and tracking information</remarks>
		public static string Connection_String
		{
			set	{	connectionString = value;	}
			get	{	return connectionString;	}
		}

        /// <summary> Test connectivity to the database </summary>
        /// <returns> TRUE if connection can be made, otherwise FALSE </returns>
        public static bool Test_Connection()
        {

            try
            {
                SqlConnection newConnection = new SqlConnection(connectionString);
                newConnection.Open();

                newConnection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary> Test connectivity to the database </summary>
        /// <returns> TRUE if connection can be made, otherwise FALSE </returns>
        public static bool Test_Connection( string Test_Connection_String )
        {

            try
            {
                SqlConnection newConnection = new SqlConnection(Test_Connection_String);
                newConnection.Open();

                newConnection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

		/// <summary> Gets the datatable containging all possible disposition types </summary>
		/// <remarks> This calls the 'Tracking_Get_All_Possible_Disposition_Types' stored procedure. </remarks>
		public static DataTable All_Possible_Disposition_Types
		{
			get
			{
				DataSet returnSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "Tracking_Get_All_Possible_Disposition_Types");
				return returnSet.Tables[0];
			}
		}

		/// <summary> Gets the datatable containging all work flow types </summary>
		/// <remarks> This calls the 'Tracking_Get_All_Possible_Workflows' stored procedure. </remarks>
		public static DataTable All_WorkFlow_Types
		{
			get
			{
				DataSet returnSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "Tracking_Get_All_Possible_Workflows");
				return returnSet.Tables[0];
			}
		}


		/// <summary> Get the list of all tracking boxes from the database </summary>
		/// <remarks> This calls the 'Tracking_Box_List' stored procedure. </remarks>
		public static List<string> All_Tracking_Boxes
		{
			get
			{
				DataSet returnSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "Tracking_Box_List");
				List<string> returnValue = new List<string>();
				if (returnSet != null)
				{
					returnValue.AddRange(from DataRow thisRow in returnSet.Tables[0].Rows where thisRow["Tracking_Box"] != DBNull.Value select thisRow["Tracking_Box"].ToString() into trackingBox where trackingBox.Length > 0 select trackingBox);
				}
				return returnValue;
			}
		}

		/// <summary> Sets the main thumbnail for a given digital resource </summary>
		/// <param name="BibID"> Bibliographic identifier for the item </param>
		/// <param name="VID"> Volume identifier for the item </param>
		/// <param name="MainThumbnail"> Filename for the new main thumbnail </param>
		/// <returns>TRUE if successful, otherwise FALSE</returns>
		/// <remarks> This calls the 'SobekCM_Set_Main_Thumbnail' stored procedure </remarks>
		public static bool Set_Item_Main_Thumbnail(string BibID, string VID, string MainThumbnail)
		{

			try
			{
				// build the parameter list
				SqlParameter[] paramList = new SqlParameter[3];
				paramList[0] = new SqlParameter("@bibid", BibID);
				paramList[1] = new SqlParameter("@vid", VID);
				paramList[2] = new SqlParameter("@mainthumb", MainThumbnail);

				//Execute this non-query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Set_Main_Thumbnail", paramList);
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Updates the cached links between aggregations and metadata, used by larger collections </summary>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'Admin_Update_Cached_Aggregation_Metadata_Links' stored procedure.<br /><br />This runs asychronously as this routine may run for a minute or more.</remarks>
		public static bool Admin_Update_Cached_Aggregation_Metadata_Links()
		{
			try
			{
				// Create the connection
				using (SqlConnection connect = new SqlConnection(connectionString))
				{
					// Create the command 
					SqlCommand executeCommand = new SqlCommand("Admin_Update_Cached_Aggregation_Metadata_Links", connect)
													{CommandType = CommandType.StoredProcedure};

					// Create the data reader
					connect.Open();
					executeCommand.BeginExecuteNonQuery();
				}

				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;

			}
		}

		#region Methods relating to the build error logs

		/// <summary> Gets the list of build errors that have been encountered between two dates </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <param name="startDate"> Beginning of the date range </param>
		/// <param name="endDate"> End of the date range</param>
		/// <returns> Datatable of all the build errors encountered </returns>
		/// <remarks> This calls the 'SobekCM_Get_Build_Error_Logs' stored procedure </remarks>
		public static DataTable Get_Build_Error_Logs(Custom_Tracer tracer, DateTime startDate, DateTime endDate )
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Build_Error_Logs", "Pulling data from database");
			}

			try
			{
				// Execute this query stored procedure
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@firstdate", startDate);
				paramList[1] = new SqlParameter("@seconddate", endDate);
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Build_Error_Logs", paramList);
				return tempSet.Tables[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				if (tracer != null)
				{
					tracer.Add_Trace("SobekCM_Database.Get_Build_Error_Logs", "Exception encounted", Custom_Trace_Type_Enum.Error);
				}
				return null;
			}
		}

		/// <summary> Adds an error while processing during execution of the SobekCM Builder </summary>
		/// <param name="BibID"> Bibliographic identifier for the item (or name of failed process)</param>
		/// <param name="VID"> Volume identifier for the item </param>
		/// <param name="METS_Type"> Type of METS or action during error</param>
		/// <param name="ErrorDescription"> Description of the error encountered </param>
		/// <returns>TRUE if successful, otherwise FALSE</returns>
		/// <remarks> This calls the 'SobekCM_Add_Item_Error_Log' stored procedure </remarks>
		public static bool Add_Item_Error_Log(string BibID, string VID, string METS_Type, string ErrorDescription)
		{

			try
			{
				// build the parameter list
				SqlParameter[] paramList = new SqlParameter[4];
				paramList[0] = new SqlParameter("@BibID", BibID);
				paramList[1] = new SqlParameter("@VID", VID);
				paramList[2] = new SqlParameter("@METS_Type", METS_Type);
				paramList[3] = new SqlParameter("@ErrorDescription", ErrorDescription);
				//Execute this non-query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Add_Item_Error_Log", paramList);
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Clears the item error log associated with a particular bibid / vid </summary>
		/// <param name="BibID"> Bibliographic identifier for the item (or name of failed process)</param>
		/// <param name="VID"> Volume identifier for the item </param>
		/// <param name="ClearedBy"> Name of user or process that cleared the error </param>
		/// <returns>TRUE if successful, otherwise FALSE</returns>
		/// <remarks> No error is deleted, but this does set a flag on the error indicating it was cleared so it will no longer appear in the list<br /><br />
		/// This calls the 'SobekCM_Clear_Item_Error_Log' stored procedure </remarks>
		public static bool Clear_Item_Error_Log(string BibID, string VID, string ClearedBy)
		{

			try
			{
				// build the parameter list
				SqlParameter[] paramList = new SqlParameter[3];
				paramList[0] = new SqlParameter("@BibID", BibID);
				paramList[1] = new SqlParameter("@VID", VID);
				paramList[2] = new SqlParameter("@ClearedBy", ClearedBy);

				//Execute this non-query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Clear_Item_Error_Log", paramList);
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		#endregion

		#region Methods relating to usage statistics and item aggregation count statistics

		/// <summary> Pulls the most often hit titles and items, by item aggregation  </summary>
		/// <param name="AggregationCode"> Code for the item aggregation of interest </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataSet with the most often hit items and titles and the number of hits </returns>
		/// <remarks> This calls the 'SobekCM_Statistics_Aggregation_Titles' stored procedure <br /><br />
		/// This is used by the <see cref="Statistics_HtmlSubwriter"/> class</remarks>
		public static DataSet Statistics_Aggregation_Titles( string AggregationCode, Custom_Tracer tracer )
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Statistics_Aggregation_Titles", "Pulling data from database");
			}

			try
			{
				// Execute this query stored procedure
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@code", AggregationCode);
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Statistics_Aggregation_Titles", paramList);
				return tempSet;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Pulls the complete usage statistics, broken down by each level of the item aggregation hierarchy, between two dates </summary>
		/// <param name="Early_Year">Year portion of the start date</param>
		/// <param name="Early_Month">Month portion of the start date</param>
		/// <param name="Last_Year">Year portion of the last date</param>
		/// <param name="Last_Month">Month portion of the last date</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Complete usage statistics, broken down by each level of the item aggregation hierarchy, between the provided dates</returns>
		/// <remarks> This calls the 'SobekCM_Statistics_By_Date_Range' stored procedure <br /><br />
		/// This is used by the <see cref="Statistics_HtmlSubwriter"/> class</remarks>
		public static DataTable Statistics_By_Date_Range(int Early_Year, int Early_Month, int Last_Year, int Last_Month, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Statistics_By_Date_Range", "Pulling data from database");
			}

			try
			{
				// Execute this query stored procedure
				SqlParameter[] paramList = new SqlParameter[4];
				paramList[0] = new SqlParameter("@year1", Early_Year);
				paramList[1] = new SqlParameter("@month1", Early_Month);
				paramList[2] = new SqlParameter("@year2", Last_Year);
				paramList[3] = new SqlParameter("@month2", Last_Month);
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Statistics_By_Date_Range", paramList);
				return tempSet.Tables[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Populates the date range from the database for which statistical information exists </summary>
		/// <param name="Stats_Date_Object"> Statistical range object to hold the beginning and ending of the statistical information </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Statistics_By_Date_Range' stored procedure <br /><br />
		/// This is used by the <see cref="Statistics_HtmlSubwriter"/> class</remarks>
		public static bool Populate_Statistics_Dates(Statistics_Dates Stats_Date_Object, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Populate_Statistics_Dates", "Pulling statistics date information from database");
			}

			try
			{
				// Execute this query stored procedure
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Statistics_Dates");

				// Reset the values in the object and then set from the database result
				Stats_Date_Object.Clear();
				Stats_Date_Object.Set_Statistics_Dates(tempSet.Tables[0]);

				// No error encountered
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}


		/// <summary> Returns the month-by-month usage statistics details by item aggregation </summary>
		/// <param name="AggregationCode"> Code for the item aggregation of interest </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Month-by-month usage statistics for item aggregation of interest </returns>
		/// <remarks> Passing 'ALL' in as the aggregation code returns the statistics for all item aggregations within this library <br /><br />
		/// This calls the 'SobekCM_Get_Collection_Statistics_History' stored procedure <br /><br />
		/// This is used by the <see cref="Statistics_HtmlSubwriter"/> class</remarks>
		public static DataTable Get_Aggregation_Statistics_History(string AggregationCode, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Collection_Statistics_History", "Pulling history for '" + AggregationCode + "' from database");
			}

			try
			{
				// Execute this query stored procedure
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@code", AggregationCode);
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Collection_Statistics_History", paramList);
				return tempSet.Tables[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Returns the month-by-month usage statistics details by item and item group </summary>
		/// <param name="BibID"> Bibliographic identifier for the item group of interest </param>
		/// <param name="VID"> Volume identifier for the item of interest </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Month-by-month usage statistics for item and item-group </returns>
		/// <remarks> This calls the 'SobekCM_Get_Item_Statistics' stored procedure  </remarks>
		public static DataSet Get_Item_Statistics_History(string BibID, string VID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Item_Statistics_History", "Pulling history for '" + BibID + "_" + VID + "' from database");
			}

			try
			{
				// Execute this query stored procedure
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@BibID", BibID);
				paramList[1] = new SqlParameter("@VID", VID);
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Item_Statistics", paramList);
				return tempSet;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Gets the current title, item, and page count for each item aggregation in the item aggregation hierarchy </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Datatable with all the current title, item, and page count for each item aggregation</returns>
		/// <remarks> This calls the 'SobekCM_Item_Count_By_Collection' stored procedure  <br /><br />
		/// This is used by the <see cref="Internal_HtmlSubwriter"/> class</remarks>
		public static DataTable Get_Item_Aggregation_Count(Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Item_Aggregation_Count", "Pulling list from database");
			}

			try
			{
				// Execute this query stored procedure
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Item_Count_By_Collection");
				return tempSet.Tables[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Gets the title, item, and page count for each item aggregation currently and at some previous point of time </summary>
		/// <param name="date1"> Date from which to additionally include item count </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Datatable with all the  title, item, and page count for each item aggregation currently and at some previous point of time </returns>
		/// <remarks> This calls the 'SobekCM_Item_Count_By_Collection_By_Dates' stored procedure  <br /><br />
		/// This is used by the <see cref="Internal_HtmlSubwriter"/> class</remarks>
		public static DataTable Get_Item_Aggregation_Count(DateTime date1, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Item_Aggregation_Count", "Pulling from database ( includes fytd starting " + date1.ToShortDateString() + ")");
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@date1", date1);
				paramList[1] = new SqlParameter("@date2", DBNull.Value);

				// Execute this query stored procedure
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Item_Count_By_Collection_By_Dates", paramList);
				return tempSet.Tables[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Gets the title, item, and page count for each item aggregation currently and at some previous point of time </summary>
		/// <param name="date1"> Date from which to additionally include item count </param>
		/// <param name="date2"> Date to which to additionally include item count </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Datatable with all the  title, item, and page count for each item aggregation at some previous point of time and then the increase in these counts between the two provided dates </returns>
		/// <remarks> This calls the 'SobekCM_Item_Count_By_Collection_By_Date_Range' stored procedure  <br /><br />
		/// This is used by the <see cref="Internal_HtmlSubwriter"/> class</remarks>
		public static DataTable Get_Item_Aggregation_Count_DateRange(DateTime date1, DateTime date2, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Item_Aggregation_Count_DateRange", "Pulling from database");
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@date1", date1);
				paramList[1] = new SqlParameter("@date2", date2);

				// Execute this query stored procedure
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Item_Count_By_Collection_By_Date_Range", paramList);
				return tempSet.Tables[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary>Method used to get the hierarchical relationship between all aggregations, to be displayed in the 'aggregations' tab in the internal screen</summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataTable with relationships between all aggregations</returns>
		/// <remarks> This calls the 'SobekCM_Get_Collection_Hierarchies' stored procedure <br /><br />
		/// This is used by the <see cref="Internal_HtmlSubwriter"/> class</remarks>
		public static DataTable Get_Aggregation_Hierarchies(Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Aggregation_Hierarchies", "Pulling from database");
			}

			try
			{
				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Collection_Hierarchies");

				// If there was no data for this collection and entry point, return null (an ERROR occurred)
				if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
				{
					return null;
				}

				// Return the first table from the returned dataset
				return tempSet.Tables[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Gets the item and page count loaded to this digital library by month and year </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataTable of the count of all items and pages loaded to this digital library by month and year </returns>
		/// <remarks> This calls the 'SobekCM_Page_Item_Count_History' stored procedure </remarks>
		public static DataTable Get_Page_Item_Count_History( Custom_Tracer tracer )
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Page_Item_Count_History", "Pulling from database");
			}

			try
			{
				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Page_Item_Count_History");

				// If there was no data for this collection and entry point, return null (an ERROR occurred)
				if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
				{
					return null;
				}

				// Return the first table from the returned dataset
				return tempSet.Tables[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}


		/// <summary> Gets the list of all users that are linked to items which may have usage statistics  </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataTable of all the users linked to items </returns>
		/// <remarks> This calls the 'SobekCM_Stats_Get_Users_Linked_To_Items' stored procedure </remarks>
		public static DataTable Get_Users_Linked_To_Items( Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Users_Linked_To_Items", "Pulling from database");
			}

			try
			{
				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Stats_Get_Users_Linked_To_Items" );

				// If there was no data for this collection and entry point, return null (an ERROR occurred)
				if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
				{
					return null;
				}

				// Return the first table from the returned dataset
				return tempSet.Tables[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}


		/// <summary> Gets the basic usage statistics for all items linked to a user </summary>
		/// <param name="UserID"> Primary key for the user of interest, for which to pull the item usage stats </param>
		/// <param name="Month"> Month for which to pull the usage information </param>
		/// <param name="Year"> Year for which to pull the usage information </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataTable of the basic usage statistics for all items linked to a user for a single month and year </returns>
		/// <remarks> This calls the 'SobekCM_Stats_Get_User_Linked_Items_Stats' stored procedure </remarks>
		public static DataTable Get_User_Linked_Items_Stats( int UserID, int Month, int Year, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_User_Linked_Items_Stats", "Pulling from database");
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[3];
				paramList[0] = new SqlParameter("@userid", UserID);
				paramList[1] = new SqlParameter("@month", Month);
				paramList[2] = new SqlParameter("@year", Year);

				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Stats_Get_User_Linked_Items_Stats", paramList);

				// If there was no data for this collection and entry point, return null (an ERROR occurred)
				if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
				{
					return null;
				}

				// Return the first table from the returned dataset
				return tempSet.Tables[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}


		#endregion

		#region Methods to retrieve the BROWSE information for the entire library

		/// <summary> Gets the collection of all (public) items in the library </summary>
		/// <param name="Only_New_Items"> Flag indicates to only pull items added in the last two weeks</param>
		/// <param name="Include_Private_Items"> Flag indicates whether to include private items in the result set </param>
		/// <param name="ResultsPerPage"> Number of results to return per "page" of results </param>
		/// <param name="ResultsPage"> Which page of results to return ( one-based, so the first page is page number of one )</param>
		/// <param name="Sort"> Current sort to use ( 0 = default by search or browse, 1 = title, 10 = date asc, 11 = date desc )</param>
		/// <param name="Include_Facets"> Flag indicates if facets should be included in the final result set</param>
		/// <param name="Facet_Types"> Primary key for the metadata types to include as facets (up to eight)</param>
		/// <param name="Return_Search_Statistics"> Flag indicates whether to create and return statistics about the overall search results, generally set to TRUE for the first page requested and subsequently set to FALSE </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Table with all of the item and item group information </returns>
		/// <remarks> This calls either the 'SobekCM_Get_All_Browse_Paged' stored procedure </remarks>
		public static Multiple_Paged_Results_Args Get_All_Browse_Paged( bool Only_New_Items, bool Include_Private_Items, int ResultsPerPage, int ResultsPage, int Sort, bool Include_Facets, List<short> Facet_Types, bool Return_Search_Statistics, Custom_Tracer tracer)
		{
			if (Only_New_Items)
			{
				// Get the date string to use
				DateTime sinceDate = DateTime.Now.Subtract(new TimeSpan(14, 0, 0, 0));
				string dateString = sinceDate.Year.ToString().PadLeft(4, '0') + "-" + sinceDate.Month.ToString().PadLeft(2, '0') + "-" + sinceDate.Day.ToString().PadLeft(2, '0');
				return Get_All_Browse_Paged( dateString, Include_Private_Items, ResultsPerPage, ResultsPage, Sort, Include_Facets, Facet_Types, Return_Search_Statistics, tracer);
			}
			
			// 1/1/2000 is a special date in the database, which means NO DATE
			return Get_All_Browse_Paged( String.Empty, Include_Private_Items, ResultsPerPage, ResultsPage, Sort, Include_Facets, Facet_Types, Return_Search_Statistics, tracer);
		}

		/// <summary> Gets the collection of all (public) items in the library </summary>
		/// <param name="Since_Date"> Date from which to pull the data </param>
		/// <param name="Include_Private_Items"> Flag indicates whether to include private items in the result set </param>
		/// <param name="ResultsPerPage"> Number of results to return per "page" of results </param>
		/// <param name="ResultsPage"> Which page of results to return ( one-based, so the first page is page number of one )</param>
		/// <param name="Sort"> Current sort to use ( 0 = default by search or browse, 1 = title, 10 = date asc, 11 = date desc )</param>
		/// <param name="Include_Facets"> Flag indicates if facets should be included in the final result set</param>
		/// <param name="Facet_Types"> Primary key for the metadata types to include as facets (up to eight)</param>
		/// <param name="Return_Search_Statistics"> Flag indicates whether to create and return statistics about the overall search results, generally set to TRUE for the first page requested and subsequently set to FALSE </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Table with all of the item and item group information </returns>
		/// <remarks> This calls the 'SobekCM_Get_All_Browse_Paged' stored procedure </remarks>
		public static Multiple_Paged_Results_Args Get_All_Browse_Paged( string Since_Date, bool Include_Private_Items, int ResultsPerPage, int ResultsPage, int Sort, bool Include_Facets, List<short> Facet_Types, bool Return_Search_Statistics, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.SobekCM_Get_All_Browse_Paged", "Pulling browse from database");
			}

			Multiple_Paged_Results_Args returnArgs;

			// Create the connection
			using (SqlConnection connect = new SqlConnection(connectionString + "Connection Timeout=45"))
			{

				// Create the command 
				SqlCommand executeCommand = new SqlCommand("SobekCM_Get_All_Browse_Paged", connect)
												{CommandTimeout = 45, CommandType = CommandType.StoredProcedure};

				if ( Since_Date.Length > 0 )
					executeCommand.Parameters.AddWithValue("@date", Since_Date);
				else
					executeCommand.Parameters.AddWithValue("@date", DBNull.Value);
				executeCommand.Parameters.AddWithValue("@include_private", Include_Private_Items);
				executeCommand.Parameters.AddWithValue("@pagesize", ResultsPerPage);
				executeCommand.Parameters.AddWithValue("@pagenumber", ResultsPage);
				executeCommand.Parameters.AddWithValue("@sort", Sort);
				executeCommand.Parameters.AddWithValue("@minpagelookahead", MIN_PAGE_LOOKAHEAD);
				executeCommand.Parameters.AddWithValue("@maxpagelookahead", MAX_PAGE_LOOKAHEAD);
				executeCommand.Parameters.AddWithValue("@lookahead_factor", LOOKAHEAD_FACTOR);
				executeCommand.Parameters.AddWithValue("@include_facets", Include_Facets);
				if ((Include_Facets) && (Facet_Types != null))
				{
					if (Facet_Types.Count > 0)
						executeCommand.Parameters.AddWithValue("@facettype1", Facet_Types[0]);
					else
						executeCommand.Parameters.AddWithValue("@facettype1", -1);
					if (Facet_Types.Count > 1)
						executeCommand.Parameters.AddWithValue("@facettype2", Facet_Types[1]);
					else
						executeCommand.Parameters.AddWithValue("@facettype2", -1);
					if (Facet_Types.Count > 2)
						executeCommand.Parameters.AddWithValue("@facettype3", Facet_Types[2]);
					else
						executeCommand.Parameters.AddWithValue("@facettype3", -1);
					if (Facet_Types.Count > 3)
						executeCommand.Parameters.AddWithValue("@facettype4", Facet_Types[3]);
					else
						executeCommand.Parameters.AddWithValue("@facettype4", -1);
					if (Facet_Types.Count > 4)
						executeCommand.Parameters.AddWithValue("@facettype5", Facet_Types[4]);
					else
						executeCommand.Parameters.AddWithValue("@facettype5", -1);
					if (Facet_Types.Count > 5)
						executeCommand.Parameters.AddWithValue("@facettype6", Facet_Types[5]);
					else
						executeCommand.Parameters.AddWithValue("@facettype6", -1);
					if (Facet_Types.Count > 6)
						executeCommand.Parameters.AddWithValue("@facettype7", Facet_Types[6]);
					else
						executeCommand.Parameters.AddWithValue("@facettype7", -1);
					if (Facet_Types.Count > 7)
						executeCommand.Parameters.AddWithValue("@facettype8", Facet_Types[7]);
					else
						executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}
				else
				{
					executeCommand.Parameters.AddWithValue("@facettype1", -1);
					executeCommand.Parameters.AddWithValue("@facettype2", -1);
					executeCommand.Parameters.AddWithValue("@facettype3", -1);
					executeCommand.Parameters.AddWithValue("@facettype4", -1);
					executeCommand.Parameters.AddWithValue("@facettype5", -1);
					executeCommand.Parameters.AddWithValue("@facettype6", -1);
					executeCommand.Parameters.AddWithValue("@facettype7", -1);
					executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}
				executeCommand.Parameters.AddWithValue("@item_count_to_use_cached", 1000);

				// Add parameters for total items and total titles
				SqlParameter totalItemsParameter = executeCommand.Parameters.AddWithValue("@total_items", 0);
				totalItemsParameter.Direction = ParameterDirection.InputOutput;

				SqlParameter totalTitlesParameter = executeCommand.Parameters.AddWithValue("@total_titles", 0);
				totalTitlesParameter.Direction = ParameterDirection.InputOutput;


				// Create the data reader
				connect.Open();
				using (SqlDataReader reader = executeCommand.ExecuteReader())
				{

					// Create the return argument object
					returnArgs = new Multiple_Paged_Results_Args
									 {Paged_Results = DataReader_To_Result_List_With_LookAhead(reader, ResultsPerPage)};

					// Create the overall search statistics?
					if (Return_Search_Statistics)
					{
						Search_Results_Statistics stats = new Search_Results_Statistics(reader, Facet_Types);
						returnArgs.Statistics = stats;
						reader.Close();
						stats.Total_Items = Convert.ToInt32(totalItemsParameter.Value);
						stats.Total_Titles = Convert.ToInt32(totalTitlesParameter.Value);
					}
					else
					{
						reader.Close();
					}
				}
				connect.Close();
			}

			// Return the built result arguments
			return returnArgs;
		}

		#endregion

		#region Method to retrieve the BROWSE information from the database for an item aggregation

		/// <summary> Gets the collection of all (public) items linked to an item aggregation </summary>
		/// <param name="AggregationCode"> Code for the item aggregation of interest </param>
		/// <param name="Only_New_Items"> Flag indicates to only pull items added in the last two weeks</param>
		/// <param name="Include_Private_Items"> Flag indicates whether to include private items in the result set </param>
		/// <param name="ResultsPerPage"> Number of results to return per "page" of results </param>
		/// <param name="ResultsPage"> Which page of results to return ( one-based, so the first page is page number of one )</param>
		/// <param name="Sort"> Current sort to use ( 0 = default by search or browse, 1 = title, 10 = date asc, 11 = date desc )</param>
		/// <param name="Include_Facets"> Flag indicates if facets should be included in the final result set</param>
		/// <param name="Facet_Types"> Primary key for the metadata types to include as facets (up to eight)</param>
		/// <param name="Return_Search_Statistics"> Flag indicates whether to create and return statistics about the overall search results, generally set to TRUE for the first page requested and subsequently set to FALSE </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Table with all of the item and item group information </returns>
		/// <remarks> This calls either the 'SobekCM_Get_Aggregation_Browse_Paged' stored procedure </remarks>
		public static Multiple_Paged_Results_Args Get_Item_Aggregation_Browse_Paged(string AggregationCode, bool Only_New_Items, bool Include_Private_Items, int ResultsPerPage, int ResultsPage, int Sort, bool Include_Facets, List<short> Facet_Types, bool Return_Search_Statistics, Custom_Tracer tracer)
		{
			if (Only_New_Items)
			{
				// Get the date string to use
				DateTime sinceDate = DateTime.Now.Subtract(new TimeSpan(14, 0, 0, 0));
				string dateString = sinceDate.Year.ToString().PadLeft(4, '0') + "-" + sinceDate.Month.ToString().PadLeft(2, '0') + "-" + sinceDate.Day.ToString().PadLeft(2, '0');
				return Get_Item_Aggregation_Browse_Paged(AggregationCode, dateString, Include_Private_Items, ResultsPerPage, ResultsPage, Sort, Include_Facets, Facet_Types, Return_Search_Statistics, tracer);
			}
			
			// 1/1/2000 is a special date in the database, which means NO DATE
			return Get_Item_Aggregation_Browse_Paged(AggregationCode, "2000-01-01", Include_Private_Items, ResultsPerPage, ResultsPage, Sort, Include_Facets, Facet_Types, Return_Search_Statistics, tracer);
		}

		/// <summary> Gets the collection of all (public) items linked to an item aggregation </summary>
		/// <param name="AggregationCode"> Code for the item aggregation of interest </param>
		/// <param name="Since_Date"> Date from which to pull the data </param>
		/// <param name="Include_Private_Items"> Flag indicates whether to include private items in the result set </param>
		/// <param name="ResultsPerPage"> Number of results to return per "page" of results </param>
		/// <param name="ResultsPage"> Which page of results to return ( one-based, so the first page is page number of one )</param>
		/// <param name="Sort"> Current sort to use ( 0 = default by search or browse, 1 = title, 10 = date asc, 11 = date desc )</param>
		/// <param name="Include_Facets"> Flag indicates if facets should be included in the final result set</param>
		/// <param name="Facet_Types"> Primary key for the metadata types to include as facets (up to eight)</param>
		/// <param name="Return_Search_Statistics"> Flag indicates whether to create and return statistics about the overall search results, generally set to TRUE for the first page requested and subsequently set to FALSE </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Table with all of the item and item group information </returns>
		/// <remarks> This calls the 'SobekCM_Get_Aggregation_Browse_Paged' stored procedure </remarks>
		public static Multiple_Paged_Results_Args Get_Item_Aggregation_Browse_Paged(string AggregationCode, string Since_Date, bool Include_Private_Items, int ResultsPerPage, int ResultsPage, int Sort, bool Include_Facets, List<short> Facet_Types, bool Return_Search_Statistics, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Item_Aggregation_Browse_Paged", "Pulling browse from database");
			}

			Multiple_Paged_Results_Args returnArgs;

			// Create the connection
			using (SqlConnection connect = new SqlConnection(connectionString + "Connection Timeout=45"))
			{

				// Create the command 
				SqlCommand executeCommand = new SqlCommand("SobekCM_Get_Aggregation_Browse_Paged", connect)
												{CommandTimeout = 45, CommandType = CommandType.StoredProcedure};

				executeCommand.Parameters.AddWithValue("@code", AggregationCode);
				executeCommand.Parameters.AddWithValue("@date", Since_Date);
				executeCommand.Parameters.AddWithValue("@include_private", Include_Private_Items);
				executeCommand.Parameters.AddWithValue("@pagesize", ResultsPerPage);
				executeCommand.Parameters.AddWithValue("@pagenumber", ResultsPage);
				executeCommand.Parameters.AddWithValue("@sort", Sort);

				if (ResultsPerPage > 100)
				{
					executeCommand.Parameters.AddWithValue("@minpagelookahead", 1);
					executeCommand.Parameters.AddWithValue("@maxpagelookahead", 1);
					executeCommand.Parameters.AddWithValue("@lookahead_factor", LOOKAHEAD_FACTOR);
				}
				else
				{
					executeCommand.Parameters.AddWithValue("@minpagelookahead", MIN_PAGE_LOOKAHEAD);
					executeCommand.Parameters.AddWithValue("@maxpagelookahead", MAX_PAGE_LOOKAHEAD);
					executeCommand.Parameters.AddWithValue("@lookahead_factor", LOOKAHEAD_FACTOR);
				}


				if ((Include_Facets) && (Facet_Types != null))
				{
					executeCommand.Parameters.AddWithValue("@include_facets", Include_Facets);
					if (Facet_Types.Count > 0)
						executeCommand.Parameters.AddWithValue("@facettype1", Facet_Types[0]);
					else
						executeCommand.Parameters.AddWithValue("@facettype1", -1);
					if (Facet_Types.Count > 1)
						executeCommand.Parameters.AddWithValue("@facettype2", Facet_Types[1]);
					else
						executeCommand.Parameters.AddWithValue("@facettype2", -1);
					if (Facet_Types.Count > 2)
						executeCommand.Parameters.AddWithValue("@facettype3", Facet_Types[2]);
					else
						executeCommand.Parameters.AddWithValue("@facettype3", -1);
					if (Facet_Types.Count > 3)
						executeCommand.Parameters.AddWithValue("@facettype4", Facet_Types[3]);
					else
						executeCommand.Parameters.AddWithValue("@facettype4", -1);
					if (Facet_Types.Count > 4)
						executeCommand.Parameters.AddWithValue("@facettype5", Facet_Types[4]);
					else
						executeCommand.Parameters.AddWithValue("@facettype5", -1);
					if (Facet_Types.Count > 5)
						executeCommand.Parameters.AddWithValue("@facettype6", Facet_Types[5]);
					else
						executeCommand.Parameters.AddWithValue("@facettype6", -1);
					if (Facet_Types.Count > 6)
						executeCommand.Parameters.AddWithValue("@facettype7", Facet_Types[6]);
					else
						executeCommand.Parameters.AddWithValue("@facettype7", -1);
					if (Facet_Types.Count > 7)
						executeCommand.Parameters.AddWithValue("@facettype8", Facet_Types[7]);
					else
						executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}
				else
				{
					executeCommand.Parameters.AddWithValue("@include_facets", false);
					executeCommand.Parameters.AddWithValue("@facettype1", -1);
					executeCommand.Parameters.AddWithValue("@facettype2", -1);
					executeCommand.Parameters.AddWithValue("@facettype3", -1);
					executeCommand.Parameters.AddWithValue("@facettype4", -1);
					executeCommand.Parameters.AddWithValue("@facettype5", -1);
					executeCommand.Parameters.AddWithValue("@facettype6", -1);
					executeCommand.Parameters.AddWithValue("@facettype7", -1);
					executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}
				executeCommand.Parameters.AddWithValue("@item_count_to_use_cached", 1000);

				// Add parameters for total items and total titles
				SqlParameter totalItemsParameter = executeCommand.Parameters.AddWithValue("@total_items", 0);
				totalItemsParameter.Direction = ParameterDirection.InputOutput;

				SqlParameter totalTitlesParameter = executeCommand.Parameters.AddWithValue("@total_titles", 0);
				totalTitlesParameter.Direction = ParameterDirection.InputOutput;


				// Create the data reader
				connect.Open();
				using (SqlDataReader reader = executeCommand.ExecuteReader())
				{

					// Create the return argument object
					returnArgs = new Multiple_Paged_Results_Args
									 {Paged_Results = DataReader_To_Result_List_With_LookAhead(reader, ResultsPerPage)};

					// Create the overall search statistics?
					if (Return_Search_Statistics)
					{
						Search_Results_Statistics stats = new Search_Results_Statistics(reader, Facet_Types);
						returnArgs.Statistics = stats;
						reader.Close();
						stats.Total_Items = Convert.ToInt32(totalItemsParameter.Value);
						stats.Total_Titles = Convert.ToInt32(totalTitlesParameter.Value);
					}
					else
					{
						reader.Close();
					}
				}
				connect.Close();
			}

			// Return the built result arguments
			return returnArgs;
		}

		/// <summary> Gets the list of all data for a particular metadata field in a particular aggregation </summary>
		/// <param name="Aggregation_Code"> Code for the item aggregation </param>
		/// <param name="Metadata_Code"> Metadata code for the field of interest </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> List with all the metadata fields in alphabetical order </returns>
		/// <remarks> This calls the 'SobekCM_Get_Metadata_Browse' stored procedure </remarks>
		public static List<string> Get_Item_Aggregation_Metadata_Browse(string Aggregation_Code, string Metadata_Code, Custom_Tracer tracer )
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Item_Aggregation_Metadata_Browse", "Pull the metadata browse");
			}

			// Build the parameter list
			SqlParameter[] paramList = new SqlParameter[3];
			paramList[0] = new SqlParameter("@aggregation_code", Aggregation_Code);
			paramList[1] = new SqlParameter("@metadata_name", Metadata_Code);
			paramList[2] = new SqlParameter("@item_count_to_use_cached", 100);

			// Define a temporary dataset
			DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Metadata_Browse", paramList);

			if (tempSet == null)
				return null;

			DataColumn column = tempSet.Tables[0].Columns[1];
			DataTable table = tempSet.Tables[0];
			return (from DataRow thisRow in table.Rows select thisRow[column].ToString()).ToList();
		}

		/// <summary> Gets the list of unique coordinate points and associated bibid and group title for a single 
		/// item aggregation </summary>
		/// <param name="Aggregation_Code"> Code for the item aggregation </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataTable with all the coordinate values </returns>
		/// <remarks> This calls the 'SobekCM_Coordinate_Points_By_Aggregation' stored procedure </remarks>
		public static DataTable Get_All_Coordinate_Points_By_Aggregation(string Aggregation_Code, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_All_Coordinate_Points_By_Aggregation", "Pull the coordinate list");
			}

			// Build the parameter list
			SqlParameter[] paramList = new SqlParameter[1];
			paramList[0] = new SqlParameter("@aggregation_code", Aggregation_Code);

			// Define a temporary dataset
			DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Coordinate_Points_By_Aggregation", paramList);
			return tempSet == null ? null : tempSet.Tables[0];
		}

		#endregion

		#region Method to perform a metadata search of items in the database

		/// <summary> Gets the list of metadata fields searchable in the database, along with field number </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataTable with all of the search fields and search field id's for metadata searching </returns>
		/// <remarks> This calls the 'SobekCM_Get_Metadata_Fields' stored procedure  </remarks>
		public static DataTable Get_Metadata_Fields(Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Metadata_Fields", "Pulling from database");
			}

			try
			{
				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Metadata_Fields");

				// If there was no data for this collection and entry point, return null (an ERROR occurred)
				if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
				{
					return null;
				}

				// Return the first table from the returned dataset
				return tempSet.Tables[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Perform a metadata search against items in the database and return one page of results </summary>
		/// <param name="Term1"> First search term for this metadata search </param>
		/// <param name="Field1"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link2"> Link between the first and second terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term2"> Second search term for this metadata search </param>
		/// <param name="Field2"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link3">Link between the second and third search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term3"> Third search term for this metadata search </param>
		/// <param name="Field3"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link4">Link between the third and fourth search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term4"> Fourth search term for this metadata search </param>
		/// <param name="Field4"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link5">Link between the fourth and fifth search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term5"> Fifth search term for this metadata search </param>
		/// <param name="Field5"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link6">Link between the fifth and sixth search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term6"> Sixth search term for this metadata search </param>
		/// <param name="Field6"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link7">Link between the sixth and seventh search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term7"> Seventh search term for this metadata search </param>
		/// <param name="Field7"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link8">Link between the seventh and eighth search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term8"> Eighth search term for this metadata search </param>
		/// <param name="Field8"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link9">Link between the eighth and ninth search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term9"> Ninth search term for this metadata search </param>
		/// <param name="Field9"> FIeld number to search for (or -1 to search all fields)</param>
		/// <param name="Link10">Link between the ninth and tenth search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term10"> Tenth search term for this metadata search </param>
		/// <param name="Field10"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Include_Private_Items"> Flag indicates whether to include private items in the result set </param>
		/// <param name="ResultsPerPage"> Number of results to return per "page" of results </param>
		/// <param name="ResultsPage"> Which page of results to return ( one-based, so the first page is page number of one )</param>
		/// <param name="Sort"> Current sort to use ( 0 = default by search or browse, 1 = title, 10 = date asc, 11 = date desc )</param>
		/// <param name="AggregationCode"> Code for the aggregation of interest ( or empty string to search all aggregations )</param>
		/// <param name="Include_Facets"> Flag indicates whether to include facets </param>
		/// <param name="Facet_Types"> Primary key for the metadata types to include as facets (up to eight)</param>
		/// <param name="Return_Search_Statistics"> Flag indicates whether to create and return statistics about the overall search results, generally set to TRUE for the first page requested and subsequently set to FALSE </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Small arguments object which contains the page of results and optionally statistics about results for the entire search, including complete counts and facet information </returns>
		/// <remarks> This calls the 'SobekCM_Metadata_Search_Paged' stored procedure </remarks>
		public static Multiple_Paged_Results_Args Perform_Metadata_Search_Paged(string Term1, int Field1,
																				int Link2, string Term2, int Field2, int Link3, string Term3, int Field3, int Link4, string Term4, int Field4,
																				int Link5, string Term5, int Field5, int Link6, string Term6, int Field6, int Link7, string Term7, int Field7,
																				int Link8, string Term8, int Field8, int Link9, string Term9, int Field9, int Link10, string Term10, int Field10,
																				bool Include_Private_Items, string AggregationCode, int ResultsPerPage, int ResultsPage, int Sort, bool Include_Facets, 
																				List<short> Facet_Types, bool Return_Search_Statistics, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Perform_Metadata_Search_Paged", "Performing search in database");
			}

			Multiple_Paged_Results_Args returnArgs;

			// Create the connection
			using (SqlConnection connect = new SqlConnection(connectionString + "Connection Timeout=45"))
			{

				// Create the command 
				SqlCommand executeCommand = new SqlCommand("SobekCM_Metadata_Search_Paged", connect)
												{CommandTimeout = 45, CommandType = CommandType.StoredProcedure};

				executeCommand.Parameters.AddWithValue("@term1", Term1);
				executeCommand.Parameters.AddWithValue("@field1", Field1);
				executeCommand.Parameters.AddWithValue("@link2", Link2);
				executeCommand.Parameters.AddWithValue("@term2", Term2);
				executeCommand.Parameters.AddWithValue("@field2", Field2);
				executeCommand.Parameters.AddWithValue("@link3", Link3);
				executeCommand.Parameters.AddWithValue("@term3", Term3);
				executeCommand.Parameters.AddWithValue("@field3", Field3);
				executeCommand.Parameters.AddWithValue("@link4", Link4);
				executeCommand.Parameters.AddWithValue("@term4", Term4);
				executeCommand.Parameters.AddWithValue("@field4", Field4);
				executeCommand.Parameters.AddWithValue("@link5", Link5);
				executeCommand.Parameters.AddWithValue("@term5", Term5);
				executeCommand.Parameters.AddWithValue("@field5", Field5);
				executeCommand.Parameters.AddWithValue("@link6", Link6);
				executeCommand.Parameters.AddWithValue("@term6", Term6);
				executeCommand.Parameters.AddWithValue("@field6", Field6);
				executeCommand.Parameters.AddWithValue("@link7", Link7);
				executeCommand.Parameters.AddWithValue("@term7", Term7);
				executeCommand.Parameters.AddWithValue("@field7", Field7);
				executeCommand.Parameters.AddWithValue("@link8", Link8);
				executeCommand.Parameters.AddWithValue("@term8", Term8);
				executeCommand.Parameters.AddWithValue("@field8", Field8);
				executeCommand.Parameters.AddWithValue("@link9", Link9);
				executeCommand.Parameters.AddWithValue("@term9", Term9);
				executeCommand.Parameters.AddWithValue("@field9", Field9);
				executeCommand.Parameters.AddWithValue("@link10", Link10);
				executeCommand.Parameters.AddWithValue("@term10", Term10);
				executeCommand.Parameters.AddWithValue("@field10", Field10);
				executeCommand.Parameters.AddWithValue("@include_private", Include_Private_Items);
				if (AggregationCode.ToUpper() == "ALL")
					AggregationCode = String.Empty;
				executeCommand.Parameters.AddWithValue("@aggregationcode", AggregationCode);
				executeCommand.Parameters.AddWithValue("@pagesize", ResultsPerPage);
				executeCommand.Parameters.AddWithValue("@pagenumber", ResultsPage);
				executeCommand.Parameters.AddWithValue("@sort", Sort);

				// If this is for more than 100 results, don't look ahead
				if (ResultsPerPage > 100)
				{
					executeCommand.Parameters.AddWithValue("@minpagelookahead", 1);
					executeCommand.Parameters.AddWithValue("@maxpagelookahead", 1);
					executeCommand.Parameters.AddWithValue("@lookahead_factor", LOOKAHEAD_FACTOR);
				}
				else
				{
					executeCommand.Parameters.AddWithValue("@minpagelookahead", MIN_PAGE_LOOKAHEAD);
					executeCommand.Parameters.AddWithValue("@maxpagelookahead", MAX_PAGE_LOOKAHEAD);
					executeCommand.Parameters.AddWithValue("@lookahead_factor", LOOKAHEAD_FACTOR);
				}

				if ((Include_Facets) && (Facet_Types != null) && ( Facet_Types.Count > 0 ) && (Return_Search_Statistics))
				{
					executeCommand.Parameters.AddWithValue("@include_facets", Include_Facets);
					if (Facet_Types.Count > 0)
						executeCommand.Parameters.AddWithValue("@facettype1", Facet_Types[0]);
					else
						executeCommand.Parameters.AddWithValue("@facettype1", -1);
					if (Facet_Types.Count > 1)
						executeCommand.Parameters.AddWithValue("@facettype2", Facet_Types[1]);
					else
						executeCommand.Parameters.AddWithValue("@facettype2", -1);
					if (Facet_Types.Count > 2)
						executeCommand.Parameters.AddWithValue("@facettype3", Facet_Types[2]);
					else
						executeCommand.Parameters.AddWithValue("@facettype3", -1);
					if (Facet_Types.Count > 3)
						executeCommand.Parameters.AddWithValue("@facettype4", Facet_Types[3]);
					else
						executeCommand.Parameters.AddWithValue("@facettype4", -1);
					if (Facet_Types.Count > 4)
						executeCommand.Parameters.AddWithValue("@facettype5", Facet_Types[4]);
					else
						executeCommand.Parameters.AddWithValue("@facettype5", -1);
					if (Facet_Types.Count > 5)
						executeCommand.Parameters.AddWithValue("@facettype6", Facet_Types[5]);
					else
						executeCommand.Parameters.AddWithValue("@facettype6", -1);
					if (Facet_Types.Count > 6)
						executeCommand.Parameters.AddWithValue("@facettype7", Facet_Types[6]);
					else
						executeCommand.Parameters.AddWithValue("@facettype7", -1);
					if (Facet_Types.Count > 7)
						executeCommand.Parameters.AddWithValue("@facettype8", Facet_Types[7]);
					else
						executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}
				else
				{
					executeCommand.Parameters.AddWithValue("@include_facets", false);
					executeCommand.Parameters.AddWithValue("@facettype1", -1);
					executeCommand.Parameters.AddWithValue("@facettype2", -1);
					executeCommand.Parameters.AddWithValue("@facettype3", -1);
					executeCommand.Parameters.AddWithValue("@facettype4", -1);
					executeCommand.Parameters.AddWithValue("@facettype5", -1);
					executeCommand.Parameters.AddWithValue("@facettype6", -1);
					executeCommand.Parameters.AddWithValue("@facettype7", -1);
					executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}

				// Add parameters for total items and total titles
				SqlParameter totalItemsParameter = executeCommand.Parameters.AddWithValue("@total_items", 0);
				totalItemsParameter.Direction = ParameterDirection.InputOutput;

				SqlParameter totalTitlesParameter = executeCommand.Parameters.AddWithValue("@total_titles", 0);
				totalTitlesParameter.Direction = ParameterDirection.InputOutput;

				// Add parameters for items and titles if this search is expanded to include all aggregations
				SqlParameter expandedItemsParameter = executeCommand.Parameters.AddWithValue("@all_collections_items", 0);
				expandedItemsParameter.Direction = ParameterDirection.InputOutput;

				SqlParameter expandedTitlesParameter = executeCommand.Parameters.AddWithValue("@all_collections_titles", 0);
				expandedTitlesParameter.Direction = ParameterDirection.InputOutput;

				// Create the data reader
				connect.Open();
				using (SqlDataReader reader = executeCommand.ExecuteReader())
				{

					// Create the return argument object
					returnArgs = new Multiple_Paged_Results_Args
									 {Paged_Results = DataReader_To_Result_List_With_LookAhead(reader, ResultsPerPage)};

					// Create the overall search statistics?
					if (Return_Search_Statistics)
					{
						Search_Results_Statistics stats = new Search_Results_Statistics(reader, Facet_Types);
						returnArgs.Statistics = stats;
						reader.Close();
						stats.Total_Items = Convert.ToInt32(totalItemsParameter.Value);
						stats.Total_Titles = Convert.ToInt32(totalTitlesParameter.Value);
						stats.All_Collections_Items = Convert.ToInt32(expandedItemsParameter.Value);
						stats.All_Collections_Titles = Convert.ToInt32(expandedTitlesParameter.Value);
					}
					else
					{
						reader.Close();
					}
				}
				connect.Close();
			}

			// Return the built result arguments
			return returnArgs;
		}

		/// <summary> Performs a basic metadata search over the entire citation, given a search condition, and returns one page of results </summary>
		/// <param name="Search_Condition"> Search condition string to be run against the databasse </param>
		/// <param name="Include_Private_Items"> Flag indicates whether to include private items in the result set </param>
		/// <param name="AggregationCode"> Code for the aggregation of interest ( or empty string to search all aggregations )</param>
		/// <param name="ResultsPerPage"> Number of results to return per "page" of results </param>
		/// <param name="ResultsPage"> Which page of results to return ( one-based, so the first page is page number of one )</param>
		/// <param name="Sort"> Current sort to use ( 0 = default by search or browse, 1 = title, 10 = date asc, 11 = date desc )</param>
		/// <param name="Include_Facets"> Flag indicates whether to include facets in the result set </param>
		/// <param name="Facet_Types"> Primary key for the metadata types to include as facets (up to eight)</param>
		/// <param name="Return_Search_Statistics"> Flag indicates whether to create and return statistics about the overall search results, generally set to TRUE for the first page requested and subsequently set to FALSE </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Small arguments object which contains the page of results and optionally statistics about results for the entire search, including complete counts and facet information </returns>
		/// <remarks> This calls the 'SobekCM_Metadata_Basic_Search_Paged' stored procedure </remarks>
		public static Multiple_Paged_Results_Args Perform_Metadata_Search_Paged( string Search_Condition, bool Include_Private_Items, string AggregationCode, int ResultsPerPage, int ResultsPage, int Sort, bool Include_Facets, List<short> Facet_Types, bool Return_Search_Statistics, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Perform_Basic_Search_Paged", "Performing basic search in database");
			}

			Multiple_Paged_Results_Args returnArgs;

			// Create the connection
			using (SqlConnection connect = new SqlConnection(connectionString + "Connection Timeout=45"))
			{

				// Create the command 
				SqlCommand executeCommand = new SqlCommand("SobekCM_Metadata_Basic_Search_Paged", connect)
												{CommandTimeout = 45, CommandType = CommandType.StoredProcedure};

				executeCommand.Parameters.AddWithValue("@searchcondition", Search_Condition.Replace("''","'"));
				executeCommand.Parameters.AddWithValue("@include_private", Include_Private_Items);
				if (AggregationCode.ToUpper() == "ALL")
					AggregationCode = String.Empty;
				executeCommand.Parameters.AddWithValue("@aggregationcode", AggregationCode);
				executeCommand.Parameters.AddWithValue("@pagesize", ResultsPerPage);
				executeCommand.Parameters.AddWithValue("@pagenumber", ResultsPage);
				executeCommand.Parameters.AddWithValue("@sort", Sort);

				// If this is for more than 100 results, don't look ahead
				if (ResultsPerPage > 100)
				{
					executeCommand.Parameters.AddWithValue("@minpagelookahead", 1);
					executeCommand.Parameters.AddWithValue("@maxpagelookahead", 1);
					executeCommand.Parameters.AddWithValue("@lookahead_factor", LOOKAHEAD_FACTOR);
				}
				else
				{
					executeCommand.Parameters.AddWithValue("@minpagelookahead", MIN_PAGE_LOOKAHEAD);
					executeCommand.Parameters.AddWithValue("@maxpagelookahead", MAX_PAGE_LOOKAHEAD);
					executeCommand.Parameters.AddWithValue("@lookahead_factor", LOOKAHEAD_FACTOR);
				}

				if ((Include_Facets) && (Facet_Types != null) && ( Facet_Types.Count > 0 ) && (Return_Search_Statistics))
				{
					executeCommand.Parameters.AddWithValue("@include_facets", Include_Facets);
					if (Facet_Types.Count > 0)
						executeCommand.Parameters.AddWithValue("@facettype1", Facet_Types[0]);
					else
						executeCommand.Parameters.AddWithValue("@facettype1", -1);
					if (Facet_Types.Count > 1)
						executeCommand.Parameters.AddWithValue("@facettype2", Facet_Types[1]);
					else
						executeCommand.Parameters.AddWithValue("@facettype2", -1);
					if (Facet_Types.Count > 2)
						executeCommand.Parameters.AddWithValue("@facettype3", Facet_Types[2]);
					else
						executeCommand.Parameters.AddWithValue("@facettype3", -1);
					if (Facet_Types.Count > 3)
						executeCommand.Parameters.AddWithValue("@facettype4", Facet_Types[3]);
					else
						executeCommand.Parameters.AddWithValue("@facettype4", -1);
					if (Facet_Types.Count > 4)
						executeCommand.Parameters.AddWithValue("@facettype5", Facet_Types[4]);
					else
						executeCommand.Parameters.AddWithValue("@facettype5", -1);
					if (Facet_Types.Count > 5)
						executeCommand.Parameters.AddWithValue("@facettype6", Facet_Types[5]);
					else
						executeCommand.Parameters.AddWithValue("@facettype6", -1);
					if (Facet_Types.Count > 6)
						executeCommand.Parameters.AddWithValue("@facettype7", Facet_Types[6]);
					else
						executeCommand.Parameters.AddWithValue("@facettype7", -1);
					if (Facet_Types.Count > 7)
						executeCommand.Parameters.AddWithValue("@facettype8", Facet_Types[7]);
					else
						executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}
				else
				{
					executeCommand.Parameters.AddWithValue("@include_facets", false);
					executeCommand.Parameters.AddWithValue("@facettype1", -1);
					executeCommand.Parameters.AddWithValue("@facettype2", -1);
					executeCommand.Parameters.AddWithValue("@facettype3", -1);
					executeCommand.Parameters.AddWithValue("@facettype4", -1);
					executeCommand.Parameters.AddWithValue("@facettype5", -1);
					executeCommand.Parameters.AddWithValue("@facettype6", -1);
					executeCommand.Parameters.AddWithValue("@facettype7", -1);
					executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}

				// Add parameters for total items and total titles
				SqlParameter totalItemsParameter = executeCommand.Parameters.AddWithValue("@total_items", 0);
				totalItemsParameter.Direction = ParameterDirection.InputOutput;

				SqlParameter totalTitlesParameter = executeCommand.Parameters.AddWithValue("@total_titles", 0);
				totalTitlesParameter.Direction = ParameterDirection.InputOutput;

				// Add parameters for items and titles if this search is expanded to include all aggregations
				SqlParameter expandedItemsParameter = executeCommand.Parameters.AddWithValue("@all_collections_items", 0);
				expandedItemsParameter.Direction = ParameterDirection.InputOutput;

				SqlParameter expandedTitlesParameter = executeCommand.Parameters.AddWithValue("@all_collections_titles", 0);
				expandedTitlesParameter.Direction = ParameterDirection.InputOutput;

				// Create the data reader
				connect.Open();
				using (SqlDataReader reader = executeCommand.ExecuteReader())
				{
					// Create the return argument object
					returnArgs = new Multiple_Paged_Results_Args
									 {Paged_Results = DataReader_To_Result_List_With_LookAhead(reader, ResultsPerPage)};

					// Create the overall search statistics?
					if (Return_Search_Statistics)
					{
						Search_Results_Statistics stats = new Search_Results_Statistics(reader, Facet_Types);
						returnArgs.Statistics = stats;
						reader.Close();
						stats.Total_Items = Convert.ToInt32(totalItemsParameter.Value);
						stats.Total_Titles = Convert.ToInt32(totalTitlesParameter.Value);
						stats.All_Collections_Items = Convert.ToInt32(expandedItemsParameter.Value);
						stats.All_Collections_Titles = Convert.ToInt32(expandedTitlesParameter.Value);
					}
					else
					{
						reader.Close();
					}
				}
				connect.Close();
			}

			// Return the built result arguments
			return returnArgs;
		}

		/// <summary> Performs a metadata search for a piece of metadata that EXACTLY matches the provided search term and return one page of results </summary>
		/// <param name="Search_Term"> Search condition string to be run against the databasse </param>
		/// <param name="FieldID"> Primary key for the field to search in the database </param>
		/// <param name="Include_Private_Items"> Flag indicates whether to include private items in the result set </param>
		/// <param name="AggregationCode"> Code for the aggregation of interest ( or empty string to search all aggregations )</param>
		/// <param name="ResultsPerPage"> Number of results to return per "page" of results </param>
		/// <param name="ResultsPage"> Which page of results to return ( one-based, so the first page is page number of one )</param>
		/// <param name="Sort"> Current sort to use ( 0 = default by search or browse, 1 = title, 10 = date asc, 11 = date desc )</param>
		/// <param name="Include_Facets"> Flag indicates whether to include facets in the result set </param>
		/// <param name="Facet_Types"> Primary key for the metadata types to include as facets (up to eight)</param>
		/// <param name="Return_Search_Statistics"> Flag indicates whether to create and return statistics about the overall search results, generally set to TRUE for the first page requested and subsequently set to FALSE </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Small arguments object which contains the page of results and optionally statistics about results for the entire search, including complete counts and facet information </returns>
		/// <remarks> This calls the 'SobekCM_Metadata_Exact_Search_Paged' stored procedure </remarks>
		public static Multiple_Paged_Results_Args Perform_Metadata_Exact_Search_Paged(string Search_Term, int FieldID, bool Include_Private_Items, string AggregationCode, int ResultsPerPage, int ResultsPage, int Sort, bool Include_Facets, List<short> Facet_Types, bool Return_Search_Statistics, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Perform_Metadata_Exact_Search_Paged", "Performing exact search in database");
			}

			Multiple_Paged_Results_Args returnArgs;

			// Create the connection
			using (SqlConnection connect = new SqlConnection(connectionString + "Connection Timeout=45"))
			{

				// Create the command 
				SqlCommand executeCommand = new SqlCommand("SobekCM_Metadata_Exact_Search_Paged", connect)
												{CommandTimeout = 45, CommandType = CommandType.StoredProcedure};

				executeCommand.Parameters.AddWithValue("@term1", Search_Term);
				executeCommand.Parameters.AddWithValue("@field1", FieldID);
				executeCommand.Parameters.AddWithValue("@include_private", Include_Private_Items);
				if (AggregationCode.ToUpper() == "ALL")
					AggregationCode = String.Empty;
				executeCommand.Parameters.AddWithValue("@aggregationcode", AggregationCode);
				executeCommand.Parameters.AddWithValue("@pagesize", ResultsPerPage);
				executeCommand.Parameters.AddWithValue("@pagenumber", ResultsPage);
				executeCommand.Parameters.AddWithValue("@sort", Sort);

				// If this is for more than 100 results, don't look ahead
				if (ResultsPerPage > 100)
				{
					executeCommand.Parameters.AddWithValue("@minpagelookahead", 1);
					executeCommand.Parameters.AddWithValue("@maxpagelookahead", 1);
					executeCommand.Parameters.AddWithValue("@lookahead_factor", LOOKAHEAD_FACTOR);
				}
				else
				{
					executeCommand.Parameters.AddWithValue("@minpagelookahead", MIN_PAGE_LOOKAHEAD);
					executeCommand.Parameters.AddWithValue("@maxpagelookahead", MAX_PAGE_LOOKAHEAD);
					executeCommand.Parameters.AddWithValue("@lookahead_factor", LOOKAHEAD_FACTOR);
				}

				if ((Include_Facets) && (Facet_Types != null) && ( Facet_Types.Count > 0 ) && (Return_Search_Statistics))
				{
					executeCommand.Parameters.AddWithValue("@include_facets", Include_Facets);
					if (Facet_Types.Count > 0)
						executeCommand.Parameters.AddWithValue("@facettype1", Facet_Types[0]);
					else
						executeCommand.Parameters.AddWithValue("@facettype1", -1);
					if (Facet_Types.Count > 1)
						executeCommand.Parameters.AddWithValue("@facettype2", Facet_Types[1]);
					else
						executeCommand.Parameters.AddWithValue("@facettype2", -1);
					if (Facet_Types.Count > 2)
						executeCommand.Parameters.AddWithValue("@facettype3", Facet_Types[2]);
					else
						executeCommand.Parameters.AddWithValue("@facettype3", -1);
					if (Facet_Types.Count > 3)
						executeCommand.Parameters.AddWithValue("@facettype4", Facet_Types[3]);
					else
						executeCommand.Parameters.AddWithValue("@facettype4", -1);
					if (Facet_Types.Count > 4)
						executeCommand.Parameters.AddWithValue("@facettype5", Facet_Types[4]);
					else
						executeCommand.Parameters.AddWithValue("@facettype5", -1);
					if (Facet_Types.Count > 5)
						executeCommand.Parameters.AddWithValue("@facettype6", Facet_Types[5]);
					else
						executeCommand.Parameters.AddWithValue("@facettype6", -1);
					if (Facet_Types.Count > 6)
						executeCommand.Parameters.AddWithValue("@facettype7", Facet_Types[6]);
					else
						executeCommand.Parameters.AddWithValue("@facettype7", -1);
					if (Facet_Types.Count > 7)
						executeCommand.Parameters.AddWithValue("@facettype8", Facet_Types[7]);
					else
						executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}
				else
				{
					executeCommand.Parameters.AddWithValue("@include_facets", false);
					executeCommand.Parameters.AddWithValue("@facettype1", -1);
					executeCommand.Parameters.AddWithValue("@facettype2", -1);
					executeCommand.Parameters.AddWithValue("@facettype3", -1);
					executeCommand.Parameters.AddWithValue("@facettype4", -1);
					executeCommand.Parameters.AddWithValue("@facettype5", -1);
					executeCommand.Parameters.AddWithValue("@facettype6", -1);
					executeCommand.Parameters.AddWithValue("@facettype7", -1);
					executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}

				// Add parameters for total items and total titles
				SqlParameter totalItemsParameter = executeCommand.Parameters.AddWithValue("@total_items", 0);
				totalItemsParameter.Direction = ParameterDirection.InputOutput;

				SqlParameter totalTitlesParameter = executeCommand.Parameters.AddWithValue("@total_titles", 0);
				totalTitlesParameter.Direction = ParameterDirection.InputOutput;

				// Add parameters for items and titles if this search is expanded to include all aggregations
				SqlParameter expandedItemsParameter = executeCommand.Parameters.AddWithValue("@all_collections_items", 0);
				expandedItemsParameter.Direction = ParameterDirection.InputOutput;

				SqlParameter expandedTitlesParameter = executeCommand.Parameters.AddWithValue("@all_collections_titles", 0);
				expandedTitlesParameter.Direction = ParameterDirection.InputOutput;

				// Create the data reader
				connect.Open();
				using (SqlDataReader reader = executeCommand.ExecuteReader())
				{
					// Create the return argument object
					returnArgs = new Multiple_Paged_Results_Args
									 {Paged_Results = DataReader_To_Result_List_With_LookAhead(reader, ResultsPerPage)};

					// Create the overall search statistics?
					if (Return_Search_Statistics)
					{
						Search_Results_Statistics stats = new Search_Results_Statistics(reader, Facet_Types);
						returnArgs.Statistics = stats;
						reader.Close();
						stats.Total_Items = Convert.ToInt32(totalItemsParameter.Value);
						stats.Total_Titles = Convert.ToInt32(totalTitlesParameter.Value);
						stats.All_Collections_Items = Convert.ToInt32(expandedItemsParameter.Value);
						stats.All_Collections_Titles = Convert.ToInt32(expandedTitlesParameter.Value);
					}
					else
					{
						reader.Close();
					}
				}
				connect.Close();
			}

			// Return the built result arguments
			return returnArgs;
		}

		private static List<List<iSearch_Title_Result>> DataReader_To_Result_List_With_LookAhead(SqlDataReader reader, int ResultsPerPage )
		{
			// Create return list
			List<List<iSearch_Title_Result>> returnValue = new List<List<iSearch_Title_Result>>();

			Dictionary<int, iSearch_Title_Result> lookup = new Dictionary<int, iSearch_Title_Result>();

			// May have not values returned
			if (reader.FieldCount < 5)
				return null;

			// Get all the main title values first
			int minimumRownumber = -1;
			while (reader.Read())
			{
				// Create new database title object for this
				Database_Title_Result result = new Database_Title_Result
												   {
													   RowNumber = reader.GetInt32(0),
													   BibID = reader.GetString(1),
													   GroupTitle = reader.GetString(2),
													   ALEPH_Number = reader.GetInt32(3),
													   OCLC_Number = reader.GetInt64(4),
													   GroupThumbnail = reader.GetString(5),
													   MaterialType = reader.GetString(6)
												   };
				if (reader.FieldCount > 7)
				{
					result.Primary_Identifier_Type = reader.GetString(7);
					result.Primary_Identifier = reader.GetString(8);
				}

				lookup.Add(result.RowNumber, result);

				if (minimumRownumber == -1)
				{
					minimumRownumber = result.RowNumber;
				}
			}

			// Move to the item table
			reader.NextResult();

			// If there were no titles, then there are no results
			if (lookup.Count == 0)
				return returnValue;

			// Set some values for checking for uniformity of values
			const int itemsToCheckInEachTitle = 20;
			bool checkingPublisher = true;
			bool checkingAuthor = true;
			bool checkingFormat = true;
			bool checkingSpatial = true;
			bool checkingEdition = true;
			bool checkingInstitution = true;
			bool checkingMaterial = true;
			bool checkingMeasurement = true;
			bool checkingStyleperiod = true;
			bool checkingTechnique = true;
			bool checkingDonor = true;
			bool checkingSubjects = true;
			bool checkingCoordinates = true;

			// Step through all the item rows, build the item, and add to the title 
			Database_Title_Result titleResult = (Database_Title_Result)lookup[minimumRownumber];
			List<iSearch_Title_Result> currentList = new List<iSearch_Title_Result> {titleResult};
			returnValue.Add(currentList);
			int lastRownumber = titleResult.RowNumber;
			int itemcount = 0;
			int titlesInCurrentList = 1;
			while (reader.Read())
			{
				// Ensure this is the right title for this item 
				int thisRownumber = reader.GetInt32(0);
				if (thisRownumber != lastRownumber)
				{
					titleResult = (Database_Title_Result)lookup[thisRownumber];
					lastRownumber = thisRownumber;
					itemcount = 0;

					// Reset some values
					checkingPublisher = true;
					checkingAuthor = true;
					checkingFormat = true;
					checkingSpatial = true;
					checkingEdition = true;
					checkingInstitution = true;
					checkingMaterial = true;
					checkingMeasurement = true;
					checkingStyleperiod = true;
					checkingTechnique = true;
					checkingDonor = true;
					checkingSubjects = true;
					checkingCoordinates = true;

					// If this is now twenty in the current list, add this to the returnvalue
					if (titlesInCurrentList == ResultsPerPage)
					{
						currentList = new List<iSearch_Title_Result>();
						returnValue.Add(currentList);
						titlesInCurrentList = 0;
					}

					// Add this title to the paged list
					currentList.Add(titleResult);
					titlesInCurrentList++;
				}

				// Create new database item object for this
				Database_Item_Result result = new Database_Item_Result
												  {
													  VID = reader.GetString(1),
													  Title = reader.GetString(2),
													  IP_Restriction_Mask = reader.GetInt16(3),
													  MainThumbnail = reader.GetString(4),
													  Level1_Index = (short) reader.GetInt32(5),
													  Level1_Text = reader.GetString(6),
													  Level2_Index = (short) reader.GetInt32(7),
													  Level2_Text = reader.GetString(8),
													  Level3_Index = (short) reader.GetInt32(9),
													  Level3_Text = reader.GetString(10),
													  PubDate = reader.GetString(11),
													  PageCount = reader.GetInt32(12),
													  Link = reader.GetString(13)
												  };

				if (itemcount == 0)
				{
					titleResult.Publisher = reader.GetString(14);
					titleResult.Author = reader.GetString(15);
					titleResult.Format = reader.GetString(16);
					titleResult.Donor = reader.GetString(17);
					titleResult.Spatial_Coverage = reader.GetString(18);
					titleResult.Edition = reader.GetString(19);
					titleResult.Institution = reader.GetString(20);
					titleResult.Material = reader.GetString(21);
					titleResult.Measurement = reader.GetString(22);
					titleResult.Style_Period = reader.GetString(23);
					titleResult.Technique = reader.GetString(24);
					titleResult.Subjects = reader.GetString(25);
					titleResult.Spatial_Coordinates = reader.GetString(26);
				}
				else if (itemcount < itemsToCheckInEachTitle)
				{
					if (checkingPublisher)
					{
						if (titleResult.Publisher != reader.GetString(14))
						{
							titleResult.Publisher = "*";
							checkingPublisher = false;
						}
					}

					if (checkingAuthor)
					{
						if (titleResult.Author != reader.GetString(15))
						{
							titleResult.Author = "*";
							checkingAuthor = false;
						}
					}

					if (checkingFormat)
					{
						if (titleResult.Format != reader.GetString(16))
						{
							titleResult.Format = "*";
							checkingFormat = false;
						}
					}

					if (checkingDonor)
					{
						if (titleResult.Donor != reader.GetString(17))
						{
							titleResult.Donor = "*";
							checkingDonor = false;
						}
					}

					if (checkingSpatial)
					{
						if (titleResult.Spatial_Coverage != reader.GetString(18))
						{
							titleResult.Spatial_Coverage = "*";
							checkingSpatial = false;
						}
					}

					if (checkingEdition)
					{
						if (titleResult.Edition != reader.GetString(19))
						{
							titleResult.Edition = "*";
							checkingEdition = false;
						}
					}

					if (checkingInstitution)
					{
						if (titleResult.Institution != reader.GetString(20))
						{
							titleResult.Institution = "*";
							checkingInstitution = false;
						}
					}

					if (checkingMaterial)
					{
						if (titleResult.Material != reader.GetString(21))
						{
							titleResult.Material = "*";
							checkingMaterial = false;
						}
					}

					if (checkingMeasurement)
					{
						if (titleResult.Measurement != reader.GetString(22))
						{
							titleResult.Measurement = "*";
							checkingMeasurement = false;
						}
					}

					if (checkingStyleperiod)
					{
						if (titleResult.Style_Period != reader.GetString(23))
						{
							titleResult.Style_Period = "*";
							checkingStyleperiod = false;
						}
					}

					if (checkingTechnique)
					{
						if (titleResult.Technique != reader.GetString(24))
						{
							titleResult.Technique = "*";
							checkingTechnique = false;
						}
					}

					if (checkingSubjects)
					{
						if (titleResult.Subjects != reader.GetString(25))
						{
							titleResult.Subjects = "*";
							checkingSubjects = false;
						}
					}

					if (checkingCoordinates)
					{
						if (titleResult.Spatial_Coordinates != reader.GetString(26))
						{
							titleResult.Spatial_Coordinates = "*";
							checkingCoordinates = false;
						}
					}
				}


				// Add this to the title object
				titleResult.Add_Item_Result(result);

				// Increment the item count
				itemcount++;
			}

			return returnValue;
		}

		private static List<iSearch_Title_Result> DataReader_To_Simple_Result_List(SqlDataReader reader)
		{
			// Create return list
			List<iSearch_Title_Result> returnValue = new List<iSearch_Title_Result>();

			Dictionary<int, int> lookup = new Dictionary<int, int>();

			// Get all the main title values first
			while (reader.Read())
			{
				// Create new database title object for this
				Database_Title_Result result = new Database_Title_Result
												   {
													   RowNumber = (short) reader.GetInt32(0),
													   BibID = reader.GetString(1),
													   GroupTitle = reader.GetString(2),
													   ALEPH_Number = reader.GetInt32(3),
													   OCLC_Number = reader.GetInt64(4),
													   GroupThumbnail = reader.GetString(5),
													   MaterialType = reader.GetString(6)
												   };
				if (reader.FieldCount > 7)
				{
					result.Primary_Identifier_Type = reader.GetString(7);
					result.Primary_Identifier = reader.GetString(8);
				}
				else
				{
					result.Primary_Identifier = String.Empty;
					result.Primary_Identifier_Type = String.Empty;
				}
				returnValue.Add(result);

				lookup.Add(result.RowNumber, returnValue.Count - 1);
			}

			// Move to the item table
			reader.NextResult();

			// If there were no titles, then there are no results
			if (returnValue.Count == 0)
				return returnValue;

			// Set some values for checking for uniformity of values
			const int itemsToCheckInEachTitle = 20;
			bool checkingPublisher = true;
			bool checkingAuthor = true;
			bool checkingFormat = true;
			bool checkingSpatial = true;
			bool checkingEdition = true;
			bool checkingInstitution = true;
			bool checkingMaterial = true;
			bool checkingMeasurement = true;
			bool checkingStyleperiod = true;
			bool checkingTechnique = true;
			bool checkingDonor = true;
			bool checkingSubjects = true;
			bool checkingCoordinates = true;

			// Step through all the item rows, build the item, and add to the title 
			Database_Title_Result titleResult = (Database_Title_Result)returnValue[0];
			int lastRowNumber = titleResult.RowNumber;
			int itemcount = 0;
			while (reader.Read())
			{
				// Ensure this is the right title for this item 
				int thisRowNumber = reader.GetInt32(0);
				if (thisRowNumber != lastRowNumber)
				{
					titleResult = (Database_Title_Result)returnValue[lookup[thisRowNumber]];
					lastRowNumber = thisRowNumber;
					itemcount = 0;

					// Reset some values
					checkingPublisher = true;
					checkingAuthor = true;
					checkingFormat = true;
					checkingSpatial = true;
					checkingEdition = true;
					checkingInstitution = true;
					checkingMaterial = true;
					checkingMeasurement = true;
					checkingStyleperiod = true;
					checkingTechnique = true;
					checkingDonor = true;
					checkingSubjects = true;
					checkingCoordinates = true;
				}

				// Create new database item object for this
				Database_Item_Result result = new Database_Item_Result
												  {
													  VID = reader.GetString(1),
													  Title = reader.GetString(2),
													  IP_Restriction_Mask = reader.GetInt16(3),
													  MainThumbnail = reader.GetString(4),
													  Level1_Index = (short) reader.GetInt32(5),
													  Level1_Text = reader.GetString(6),
													  Level2_Index = (short) reader.GetInt32(7),
													  Level2_Text = reader.GetString(8),
													  Level3_Index = (short) reader.GetInt32(9),
													  Level3_Text = reader.GetString(10),
													  PubDate = reader.GetString(11),
													  PageCount = reader.GetInt32(12),
													  Link = reader.GetString(13)
												  };

				if (itemcount == 0)
				{
					titleResult.Publisher = reader.GetString(14);
					titleResult.Author = reader.GetString(15);
					titleResult.Format = reader.GetString(16);
					titleResult.Donor = reader.GetString(17);
					titleResult.Spatial_Coverage = reader.GetString(18);
					titleResult.Edition = reader.GetString(19);
					titleResult.Institution = reader.GetString(20);
					titleResult.Material = reader.GetString(21);
					titleResult.Measurement = reader.GetString(22);
					titleResult.Style_Period = reader.GetString(23);
					titleResult.Technique = reader.GetString(24);
					titleResult.Subjects = reader.GetString(25);
					titleResult.Spatial_Coordinates = reader.GetString(26);

					if (reader.FieldCount > 27)
						titleResult.UserNotes = reader.GetString(27);
				}
				else if ( itemcount < itemsToCheckInEachTitle )
				{
					if (checkingPublisher)
					{
						if (titleResult.Publisher != reader.GetString(14))
						{
							titleResult.Publisher = "*";
							checkingPublisher = false;
						}
					}

					if (checkingAuthor)
					{
						if (titleResult.Author != reader.GetString(15))
						{
							titleResult.Author = "*";
							checkingAuthor = false;
						}
					}

					if (checkingFormat)
					{
						if (titleResult.Format != reader.GetString(16))
						{
							titleResult.Format = "*";
							checkingFormat = false;
						}
					}

					if (checkingDonor)
					{
						if (titleResult.Donor != reader.GetString(17))
						{
							titleResult.Donor = "*";
							checkingDonor = false;
						}
					}

					if (checkingSpatial)
					{
						if (titleResult.Spatial_Coverage != reader.GetString(18))
						{
							titleResult.Spatial_Coverage = "*";
							checkingSpatial = false;
						}
					}

					if (checkingEdition)
					{
						if (titleResult.Edition != reader.GetString(19))
						{
							titleResult.Edition = "*";
							checkingEdition = false;
						}
					}

					if (checkingInstitution)
					{
						if (titleResult.Institution != reader.GetString(20))
						{
							titleResult.Institution = "*";
							checkingInstitution = false;
						}
					}

					if (checkingMaterial)
					{
						if (titleResult.Material != reader.GetString(21))
						{
							titleResult.Material = "*";
							checkingMaterial = false;
						}
					}

					if (checkingMeasurement)
					{
						if (titleResult.Measurement != reader.GetString(22))
						{
							titleResult.Measurement = "*";
							checkingMeasurement = false;
						}
					}

					if (checkingStyleperiod)
					{
						if (titleResult.Style_Period != reader.GetString(23))
						{
							titleResult.Style_Period = "*";
							checkingStyleperiod = false;
						}
					}

					if (checkingTechnique)
					{
						if (titleResult.Technique != reader.GetString(24))
						{
							titleResult.Technique = "*";
							checkingTechnique = false;
						}
					}

					if (checkingSubjects)
					{
						if (titleResult.Subjects != reader.GetString(25))
						{
							titleResult.Subjects = "*";
							checkingSubjects = false;
						}
					}

					if (checkingCoordinates)
					{
						if (titleResult.Spatial_Coordinates != reader.GetString(26))
						{
							titleResult.Spatial_Coordinates = "*";
							checkingCoordinates = false;
						}
					}
				}


				// Add this to the title object
				titleResult.Add_Item_Result(result);

				// Increment the item count
				itemcount++;
			}

			return returnValue;
		}

		#endregion

		#region Method to perform a coordinate/geographic search of items in the database

		/// <summary> Performs geographic search for items within provided rectangular bounding box and linked to item aggregation of interest </summary>
		/// <param name="AggregationCode"> Code for the item aggregation of interest </param>
		/// <param name="Latitude_1"> Latitudinal portion of the first point making up the rectangular bounding box</param>
		/// <param name="Longitude_1"> Longitudinal portion of the first point making up the rectangular bounding box</param>
		/// <param name="Latitude_2"> Latitudinal portion of the second point making up the rectangular bounding box</param>
		/// <param name="Longitude_2"> Longitudinal portion of the second point making up the rectangular bounding box</param>
		/// <param name="Include_Private_Items"> Flag indicates whether to include private items in the result set </param>
		/// <param name="ResultsPerPage"> Number of results to return per "page" of results </param>
		/// <param name="ResultsPage"> Which page of results to return ( one-based, so the first page is page number of one )</param>
		/// <param name="Sort"> Current sort to use ( 0 = default by search or browse, 1 = title, 10 = date asc, 11 = date desc )</param>
		/// <param name="Include_Facets"> Flag indicates if facets should be included in the result set </param>
		/// <param name="Facet_Types"> Primary key for the metadata types to include as facets (up to eight)</param>
		/// <param name="Return_Search_Statistics"> Flag indicates whether to create and return statistics about the overall search results, generally set to TRUE for the first page requested and subsequently set to FALSE </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Table with all of the item and item group information within provided bounding box </returns>
		/// <remarks> This calls the 'SobekCM_Get_Items_By_Coordinates' stored procedure </remarks>
		public static Multiple_Paged_Results_Args Get_Items_By_Coordinates(string AggregationCode, double Latitude_1, double Longitude_1, double Latitude_2, double Longitude_2, bool Include_Private_Items, int ResultsPerPage, int ResultsPage, int Sort, bool Include_Facets, List<short> Facet_Types, bool Return_Search_Statistics, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Items_By_Coordinates", "Pulling data from database");
			}

			Multiple_Paged_Results_Args returnArgs;

			// Create the connection
			using (SqlConnection connect = new SqlConnection(connectionString + "Connection Timeout=45"))
			{

				// Create the command 
				SqlCommand executeCommand = new SqlCommand("SobekCM_Get_Items_By_Coordinates", connect)
												{CommandType = CommandType.StoredProcedure};
				executeCommand.Parameters.AddWithValue("@lat1", Latitude_1);
				executeCommand.Parameters.AddWithValue("@long1", Longitude_1);
				executeCommand.Parameters.AddWithValue("@lat2", Latitude_2);
				executeCommand.Parameters.AddWithValue("@long2", Longitude_2);
				executeCommand.Parameters.AddWithValue("@include_private", Include_Private_Items);
				executeCommand.Parameters.AddWithValue("@pagesize", ResultsPerPage);
				executeCommand.Parameters.AddWithValue("@pagenumber", ResultsPage);
				executeCommand.Parameters.AddWithValue("@sort", Sort);
				executeCommand.Parameters.AddWithValue("@minpagelookahead", MIN_PAGE_LOOKAHEAD);
				executeCommand.Parameters.AddWithValue("@maxpagelookahead", MAX_PAGE_LOOKAHEAD);
				executeCommand.Parameters.AddWithValue("@lookahead_factor", LOOKAHEAD_FACTOR);
				executeCommand.Parameters.AddWithValue("@aggregationcode", AggregationCode);
				executeCommand.Parameters.AddWithValue("@include_facets", Include_Facets);

				if ((Include_Facets) && (Facet_Types != null) && (Return_Search_Statistics))
				{
					if (Facet_Types.Count > 0)
						executeCommand.Parameters.AddWithValue("@facettype1", Facet_Types[0]);
					else
						executeCommand.Parameters.AddWithValue("@facettype1", -1);
					if (Facet_Types.Count > 1)
						executeCommand.Parameters.AddWithValue("@facettype2", Facet_Types[1]);
					else
						executeCommand.Parameters.AddWithValue("@facettype2", -1);
					if (Facet_Types.Count > 2)
						executeCommand.Parameters.AddWithValue("@facettype3", Facet_Types[2]);
					else
						executeCommand.Parameters.AddWithValue("@facettype3", -1);
					if (Facet_Types.Count > 3)
						executeCommand.Parameters.AddWithValue("@facettype4", Facet_Types[3]);
					else
						executeCommand.Parameters.AddWithValue("@facettype4", -1);
					if (Facet_Types.Count > 4)
						executeCommand.Parameters.AddWithValue("@facettype5", Facet_Types[4]);
					else
						executeCommand.Parameters.AddWithValue("@facettype5", -1);
					if (Facet_Types.Count > 5)
						executeCommand.Parameters.AddWithValue("@facettype6", Facet_Types[5]);
					else
						executeCommand.Parameters.AddWithValue("@facettype6", -1);
					if (Facet_Types.Count > 6)
						executeCommand.Parameters.AddWithValue("@facettype7", Facet_Types[6]);
					else
						executeCommand.Parameters.AddWithValue("@facettype7", -1);
					if (Facet_Types.Count > 7)
						executeCommand.Parameters.AddWithValue("@facettype8", Facet_Types[7]);
					else
						executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}
				else
				{
					executeCommand.Parameters.AddWithValue("@facettype1", -1);
					executeCommand.Parameters.AddWithValue("@facettype2", -1);
					executeCommand.Parameters.AddWithValue("@facettype3", -1);
					executeCommand.Parameters.AddWithValue("@facettype4", -1);
					executeCommand.Parameters.AddWithValue("@facettype5", -1);
					executeCommand.Parameters.AddWithValue("@facettype6", -1);
					executeCommand.Parameters.AddWithValue("@facettype7", -1);
					executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}

				// Add parameters for total items and total titles
				SqlParameter totalItemsParameter = executeCommand.Parameters.AddWithValue("@total_items", 0);
				totalItemsParameter.Direction = ParameterDirection.InputOutput;

				SqlParameter totalTitlesParameter = executeCommand.Parameters.AddWithValue("@total_titles", 0);
				totalTitlesParameter.Direction = ParameterDirection.InputOutput;

				// Create the data reader
				connect.Open();
				using (SqlDataReader reader = executeCommand.ExecuteReader())
				{

					// Create the return argument object
					returnArgs = new Multiple_Paged_Results_Args
									 {Paged_Results = DataReader_To_Result_List_With_LookAhead(reader, ResultsPerPage)};

					// Create the overall search statistics?
					if (Return_Search_Statistics)
					{
						Search_Results_Statistics stats = new Search_Results_Statistics(reader, Facet_Types);
						returnArgs.Statistics = stats;
						reader.Close();
						stats.Total_Items = Convert.ToInt32(totalItemsParameter.Value);
						stats.Total_Titles = Convert.ToInt32(totalTitlesParameter.Value);
					}
					else
					{
						reader.Close();
					}
				}
				connect.Close();
			}

			// Return the built result arguments
			return returnArgs;
		}

		#endregion

		#region Methods to retrieve item list by OCLC or ALEPH number

		/// <summary> Returns the list of all items/titles which match a given OCLC number </summary>
		/// <param name="OCLC_Number"> OCLC number to look for matching items </param>
		/// <param name="Include_Private_Items"> Flag indicates whether to include private items in the result set </param>
		/// <param name="ResultsPerPage"> Number of results to return per "page" of results </param>
		/// <param name="Sort"> Current sort to use ( 0 = default by search or browse, 1 = title, 10 = date asc, 11 = date desc )</param>
		/// <param name="Return_Search_Statistics"> Flag indicates whether to create and return statistics about the overall search results, generally set to TRUE for the first page requested and subsequently set to FALSE </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Table with all of the item and item group information which matches the OCLC number </returns>
		/// <remarks> This calls the 'SobekCM_Items_By_OCLC' stored procedure </remarks>
		public static Multiple_Paged_Results_Args Items_By_OCLC_Number(long OCLC_Number, bool Include_Private_Items, int ResultsPerPage, int Sort, bool Return_Search_Statistics, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Items_By_OCLC_Number", "Searching by OCLC in the database");
			}

			// Build the parameter list
			SqlParameter[] paramList = new SqlParameter[5];
			paramList[0] = new SqlParameter("@oclc_number", OCLC_Number);
			paramList[1] = new SqlParameter("@include_private", Include_Private_Items);
			paramList[2] = new SqlParameter("@sort", Sort);
			paramList[3] = new SqlParameter("@total_items", 0) {Direction = ParameterDirection.InputOutput};
			paramList[4] = new SqlParameter("@total_titles", 0) {Direction = ParameterDirection.InputOutput};

			// Get the matching reader            
			Multiple_Paged_Results_Args returnArgs;
			using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, "SobekCM_Items_By_OCLC", paramList))
			{
				// Create the return argument object
				returnArgs = new Multiple_Paged_Results_Args
								 {Paged_Results = DataReader_To_Result_List_With_LookAhead(reader, ResultsPerPage)};

				// Create the overall search statistics?
				if (Return_Search_Statistics)
				{
					Search_Results_Statistics stats = new Search_Results_Statistics(reader, null);
					returnArgs.Statistics = stats;
					reader.Close();
					stats.Total_Items = Convert.ToInt32(paramList[3].Value);
					stats.Total_Titles = Convert.ToInt32(paramList[4].Value);
				}
				else
				{
					reader.Close();
				}
			}

			// Return the built results
			return returnArgs;
		}

		/// <summary> Returns the list of all items/titles which match a given ALEPH number </summary>
		/// <param name="ALEPH_Number"> ALEPH number to look for matching items </param>
		/// <param name="Include_Private_Items"> Flag indicates whether to include private items in the result set </param>
		/// <param name="ResultsPerPage"> Number of results to return per "page" of results </param>
		/// <param name="Sort"> Current sort to use ( 0 = default by search or browse, 1 = title, 10 = date asc, 11 = date desc )</param>
		/// <param name="Return_Search_Statistics"> Flag indicates whether to create and return statistics about the overall search results, generally set to TRUE for the first page requested and subsequently set to FALSE </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Table with all of the item and item group information which matches the ALEPH number </returns>
		/// <remarks> This calls the 'SobekCM_Items_By_ALEPH' stored procedure </remarks>
		public static Multiple_Paged_Results_Args Items_By_ALEPH_Number(int ALEPH_Number, bool Include_Private_Items, int ResultsPerPage, int Sort, bool Return_Search_Statistics, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Items_By_ALEPH_Number", "Searching by ALEPH in the database");
			}

			// Build the parameter list
			SqlParameter[] paramList = new SqlParameter[5];
			paramList[0] = new SqlParameter("@aleph_number", ALEPH_Number);
			paramList[1] = new SqlParameter("@include_private", Include_Private_Items);
			paramList[2] = new SqlParameter("@sort", Sort);
			paramList[3] = new SqlParameter("@total_items", 0) {Direction = ParameterDirection.InputOutput};
			paramList[4] = new SqlParameter("@total_titles", 0) {Direction = ParameterDirection.InputOutput};

			// Get the matching reader            
			Multiple_Paged_Results_Args returnArgs;
			using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, "SobekCM_Items_By_ALEPH", paramList))
			{
				// Create the return argument object
				returnArgs = new Multiple_Paged_Results_Args
								 {Paged_Results = DataReader_To_Result_List_With_LookAhead(reader, ResultsPerPage)};

				// Create the overall search statistics?
				if (Return_Search_Statistics)
				{
					Search_Results_Statistics stats = new Search_Results_Statistics(reader, null);
					returnArgs.Statistics = stats;
					reader.Close();
					stats.Total_Items = Convert.ToInt32(paramList[3].Value);
					stats.Total_Titles = Convert.ToInt32(paramList[4].Value);
				}
				else
				{
					reader.Close();
				}
			}

			// Return the built results
			return returnArgs;
		}

		#endregion

		#region Methods to get the information about an ITEM

		/// <summary> Gets some basic information about an item group before displaying it, such as the descriptive notes from the database, ability to add notes, etc.. </summary>
		/// <param name="BibID"> Bibliographic identifier for the item group to retrieve </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataSet with detailed information about this item group from the database </returns>
		/// <remarks> This calls the 'SobekCM_Get_Item_Details2' stored procedure, passing in NULL for the volume id </remarks> 
		public static DataSet Get_Item_Group_Details(string BibID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Item_Group_Details", "");
			}

			try
			{
				SqlParameter[] parameters = new SqlParameter[2];
				parameters[0] = new SqlParameter("@BibID", BibID);
				parameters[1] = new SqlParameter("@VID", DBNull.Value);

				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Item_Details2", parameters);

				// Return the first table from the returned dataset
				return tempSet;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Gets some basic information about an item before displaying it, such as the descriptive notes from the database, ability to add notes, etc.. </summary>
		/// <param name="BibID"> Bibliographic identifier for the volume to retrieve </param>
		/// <param name="VID"> Volume identifier for the volume to retrieve </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataSet with detailed information about this item from the database </returns>
		/// <remarks> This calls the 'SobekCM_Get_Item_Details' stored procedure </remarks> 
		public static DataSet Get_Item_Details(string BibID, string VID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Item_Details", "");
			}

			try
			{
				SqlParameter[] parameters = new SqlParameter[2];
				parameters[0] = new SqlParameter("@BibID", BibID);
				parameters[1] = new SqlParameter("@VID", VID);
				

				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Item_Details2", parameters);

				// Return the first table from the returned dataset
				return tempSet;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Gets some basic information about an item before displaing it, such as the descriptive notes from the database, ability to add notes, etc.. </summary>
		/// <param name="ItemID"> Bibliographic identifier for the volume to retrieve </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataSet with detailed information about this item from the database </returns>
		/// <remarks> This calls the 'SobekCM_Get_BibID_VID_From_ItemID' stored procedure </remarks> 
		public static DataRow Lookup_Item_By_ItemID( int ItemID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Lookup_Item_By_ItemID", "Trying to pull information for " + ItemID );
			}

			try
			{
				SqlParameter[] parameters = new SqlParameter[1];
				parameters[0] = new SqlParameter("@itemid", ItemID);

				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_BibID_VID_From_ItemID", parameters);

				// If there was no data for this collection and entry point, return null (an ERROR occurred)
				if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
				{
					return null;
				}

				// Return the first table from the returned dataset
				return tempSet.Tables[0].Rows[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Pulls the item id, main thumbnail, and aggregation codes and adds them to the resource object </summary>
		/// <param name="Resource"> Digital resource object </param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'Builder_Get_Minimum_Item_Information' stored procedure </remarks> 
		public static bool Add_Minimum_Builder_Information(SobekCM_Item Resource)
		{
			try
			{
				SqlParameter[] parameters = new SqlParameter[2];
				parameters[0] = new SqlParameter("@bibid", Resource.BibID);
				parameters[1] = new SqlParameter("@vid", Resource.VID);

				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "Builder_Get_Minimum_Item_Information", parameters);

				// If there was no data for this collection and entry point, return null (an ERROR occurred)
				if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
				{
					return false;
				}

				// Get the item id and the thumbnail from the first table
				Resource.Web.ItemID = Convert.ToInt32(tempSet.Tables[0].Rows[0][0]);
				Resource.Behaviors.Main_Thumbnail = tempSet.Tables[0].Rows[0][1].ToString();
				Resource.Behaviors.IP_Restriction_Membership = Convert.ToInt16(tempSet.Tables[0].Rows[0][2]);
				Resource.Tracking.Born_Digital = Convert.ToBoolean(tempSet.Tables[0].Rows[0][3]);
                Resource.Web.Siblings = Convert.ToInt32(tempSet.Tables[0].Rows[0][4]) - 1;
                Resource.Behaviors.Dark_Flag = Convert.ToBoolean(tempSet.Tables[0].Rows[0]["Dark"]);

				// Add the aggregation codes
				Resource.Behaviors.Clear_Aggregations();
				foreach( DataRow thisRow in tempSet.Tables[1].Rows )
				{
					string code = thisRow[0].ToString();
					Resource.Behaviors.Add_Aggregation(code);
				}

				// Return the first table from the returned dataset
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}            
		}

		#endregion

		#region Method to get the information about an ITEM GROUP

		//// THIS HAS BEEN REPLACED BY ITEM GROUP DETAILS (WHICH IS VERY SIMILAR)
		///// <summary> Gets the information about a title (item group) by BibID, including volumes, icons, and skins </summary>
		///// <param name="BibID"> Bibliographic identifier for the title of interest </param>
		///// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		///// <returns> Strongly typed dataset with information about the title (item group), including volumes, icons, and skins</returns>
		///// <remarks> This calls the 'SobekCM_Get_Multiple_Volumes' stored procedure </remarks>
		//public static Group_Information Get_Multiple_Volumes(string BibID, Custom_Tracer tracer)
		//{
		//    if (tracer != null)
		//    {
		//        tracer.Add_Trace("SobekCM_Database.Get_Multiple_Volumes", "List of volumes for " + BibID + " pulled from database");
		//    }

		//    try
		//    {
		//        // Create the connection
		//        SqlConnection connect = new SqlConnection(connectionString);

		//        // Create the command 
		//        SqlCommand executeCommand = new SqlCommand("SobekCM_Get_Multiple_Volumes", connect);
		//        executeCommand.CommandType = CommandType.StoredProcedure;
		//        executeCommand.Parameters.AddWithValue("@bibid", BibID);

		//        // Create the adapter
		//        SqlDataAdapter adapter = new SqlDataAdapter(executeCommand);

		//        // Add appropriate table mappings
		//        adapter.TableMappings.Add("Table", "Group");
		//        adapter.TableMappings.Add("Table1", "Item");
		//        adapter.TableMappings.Add("Table2", "Icon");

		//        // Fill the strongly typed dataset
		//        Group_Information thisGroup = new Group_Information();
		//        adapter.Fill(thisGroup);

		//        // If there was either no match, or more than one, return null
		//        if ((thisGroup == null) || (thisGroup.Tables.Count == 0) || (thisGroup.Tables[0] == null) || (thisGroup.Tables[0].Rows.Count == 0))
		//        {
		//            return null;
		//        }


		//        // Return the fully built object
		//        return thisGroup;
		//    }
		//    catch (Exception ee)
		//    {
		//        last_exception = ee;
		//        return null;
		//    }
		//}


		/// <summary> Gets the list of all items within this item group, indicated by BibID </summary>
		/// <param name="BibID"> Bibliographic identifier for the title of interest </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Strongly typed dataset with information about the title (item group), including volumes, icons, and skins</returns>
		/// <remarks> This calls the 'SobekCM_Get_Multiple_Volumes' stored procedure </remarks>
		public static SobekCM_Items_In_Title Get_Multiple_Volumes(string BibID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Multiple_Volumes", "List of volumes for " + BibID + " pulled from database");
			}

			try
			{
				// Create the connection
				SqlConnection connect = new SqlConnection(connectionString);

				// Create the command 
				SqlCommand executeCommand = new SqlCommand("SobekCM_Get_Multiple_Volumes", connect)
												{CommandType = CommandType.StoredProcedure};
				executeCommand.Parameters.AddWithValue("@bibid", BibID);

				// Create the adapter
				SqlDataAdapter adapter = new SqlDataAdapter(executeCommand);

				// Get the datatable
				DataSet valueSet = new DataSet();
				adapter.Fill(valueSet);

				// If there was either no match, or more than one, return null
				if ((valueSet.Tables.Count == 0) || (valueSet.Tables[0] == null) || (valueSet.Tables[0].Rows.Count == 0))
				{
					return null;
				}

				// Create the object
				SobekCM_Items_In_Title returnValue = new SobekCM_Items_In_Title(valueSet.Tables[0]);

				// Return the fully built object
				return returnValue;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		#endregion

		#region Methods to get the item aggregations

		/// <summary> Adds the title, item, and page counts to this item aggregation object </summary>
		/// <param name="Aggregation"> Mostly built item aggregation object </param>
		/// <param name="tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This method calls the stored procedure 'SobekCM_Get_Item_Aggregation2'. </remarks>
		public static bool Get_Item_Aggregation_Counts(Item_Aggregation Aggregation, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Item_Aggregation_Counts", "Add the title, item, and page count to the item aggregation object");
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[3];
				paramList[0] = new SqlParameter("@code", Aggregation.Code);
				paramList[1] = new SqlParameter("@include_counts", true);
				paramList[2] = new SqlParameter("@is_robot", false);

				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Item_Aggregation2", paramList);

				// Add the counts for this item aggregation
				if (tempSet.Tables.Count > 4)
				{
					add_counts(Aggregation, tempSet.Tables[4]);
				}


				// Return the built argument set
				return true;
			}
			catch 
			{
				return false;
			}

		}

		/// <summary> Gets the database information about a single item aggregation </summary>
		/// <param name="Code"> Code specifying the item aggregation to retrieve </param>
		/// <param name="Include_Counts"> Flag indicates whether to pull the title/item/page counts for this aggregation </param>
		/// <param name="tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <param name="Is_Robot"> Flag indicates if this is a request from an indexing robot, which leaves out a good bit of the work </param>
		/// <returns> Arguments which include the <see cref="Item_Aggregation"/> object and a DataTable of the search field information</returns>
		/// <remarks> This method calls the stored procedure 'SobekCM_Get_Item_Aggregation2'. </remarks>
		public static Item_Aggregation Get_Item_Aggregation(string Code, bool Include_Counts, bool Is_Robot, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Item_Aggregation", "Pulling item aggregation data from database");
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[3];
				paramList[0] = new SqlParameter("@code", Code);
				paramList[1] = new SqlParameter("@include_counts", Include_Counts);
				paramList[2] = new SqlParameter("@is_robot", Is_Robot);

				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Item_Aggregation2", paramList);

				// If there was no data for this collection and entry point, return null (an ERROR occurred)
				if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
				{
					return null;
				}

				// Build the collection group object
				Item_Aggregation aggrInfo = create_basic_aggregation_from_datatable(tempSet.Tables[0]);

				// Add the child objects from that table
				add_children(aggrInfo, tempSet.Tables[1]);

				// Add the advanced search values
				if (!Is_Robot)
				{
					add_advanced_terms(aggrInfo, tempSet.Tables[2]);
				}

				// Add the counts for this item aggregation
				if (Include_Counts)
				{
					add_counts(aggrInfo, tempSet.Tables[ tempSet.Tables.Count - 2 ]);
				}

                // If this is not a robot, add the parents
                if (!Is_Robot)
                {
                    add_parents(aggrInfo, tempSet.Tables[tempSet.Tables.Count - 1]);
                }

				// Return the built argument set
				return aggrInfo;
			}
			catch (Exception ee)
			{
				if (tracer != null)
				{
					tracer.Add_Trace("SobekCM_Database.Get_Item_Aggregation", ee.Message, Custom_Trace_Type_Enum.Error);
				}
				throw;
			}
		}

		/// <summary> Gets the database information about the main aggregation, representing the entire web page </summary>
		/// <param name="tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Arguments which include the <see cref="Item_Aggregation"/> object and a DataTable of the search field information</returns>
		/// <remarks> This method calls the stored procedure 'SobekCM_Get_All_Groups'. </remarks>
		public static Item_Aggregation Get_Main_Aggregation(Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Main_Aggregation", "Pulling item aggregation data from database");
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@metadata_count_to_use_cache", ALL_AGGREGATIONS_METADATA_COUNT_TO_USE_CACHED);

				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_All_Groups", paramList);

				// If there was no data for this collection and entry point, return null (an ERROR occurred)
				if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
				{
					return null;  
				}

				// Build the collection group object
				Item_Aggregation aggrInfo = create_basic_aggregation_from_datatable(tempSet.Tables[0]);

				// Add the advanced search values
				add_advanced_terms(aggrInfo, tempSet.Tables[1]);

				// Return the built argument set
				return aggrInfo;
			}
			catch (Exception ee)
			{
				if (tracer != null)
				{
					tracer.Add_Trace("SobekCM_Database.Get_Main_Aggregation", ee.Message, Custom_Trace_Type_Enum.Error);
				}
				throw;
			}
		}

		/// <summary> Adds the entire collection hierarchy under the ALL aggregation object </summary>
		/// <param name="allInfoObject"> All aggregations object within which to populate the hierarchy </param>
		/// <param name="Tracer">Trace object keeps a list of each method executed and important milestones in rendering </param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This is postponed until it is needed for the TREE VIEW on the home page, to allow the system to start
		/// faster, even with a great number of item aggregations in the hierarchy </remarks>
		public static bool Add_Children_To_Main_Agg( Item_Aggregation allInfoObject, Custom_Tracer Tracer )
		{
			DataTable childInfo = Get_Aggregation_Hierarchies(Tracer);
			if ( childInfo == null )
				return false;

			add_children(allInfoObject,  childInfo);
			return true;
		}

		/// <summary> Creates the item aggregation object from the datatable extracted from the database </summary>
		/// <param name="basicInfo">Datatable from database calls ( either SobekCM_Get_Item_Aggregation or SobekCM_Get_All_Groups )</param>
		/// <returns>Minimally built aggregation object</returns>
		/// <remarks>The child and parent information is not yet added to the returned object </remarks>
		private static Item_Aggregation create_basic_aggregation_from_datatable( DataTable basicInfo )
		{
			// Pull out this row
			DataRow thisRow = basicInfo.Rows[0];

			string displayOptions = thisRow[15].ToString();
			DateTime lastAdded = new DateTime(2000, 1, 1);
			if ( thisRow[16] != DBNull.Value )
				lastAdded = Convert.ToDateTime(thisRow[16]);

			// Build the collection group object
			Item_Aggregation aggrInfo = new Item_Aggregation(thisRow[1].ToString().ToLower(), thisRow[4].ToString(),
															 Convert.ToInt32(thisRow[0]), displayOptions, lastAdded)
											{
												Name = thisRow[2].ToString(),
												ShortName = thisRow[3].ToString(),
												Is_Active = Convert.ToBoolean(thisRow[5]),
												Hidden = Convert.ToBoolean(thisRow[6]),
												Has_New_Items = Convert.ToBoolean(thisRow[7]),
												Contact_Email = thisRow[8].ToString(),
												Default_Skin = thisRow[9].ToString(),
												Description = thisRow[10].ToString(),
												Map_Display = Convert.ToUInt16(thisRow[11]),
												Map_Search = Convert.ToUInt16(thisRow[12]),
												OAI_Flag = Convert.ToBoolean(thisRow[13]),
												OAI_Metadata = thisRow[14].ToString(),
												Items_Can_Be_Described = Convert.ToInt16(thisRow[18]),
												External_Link = thisRow[19].ToString()
											};

            if (basicInfo.Columns.Contains("ThematicHeadingID"))
                aggrInfo.Thematic_Heading_ID = Convert.ToInt32(thisRow["ThematicHeadingID"]);

			// return the built object
			return aggrInfo;            
		}

		/// <summary> Adds the child information to the item aggregation object from the datatable extracted from the database </summary>
		/// <param name="aggrInfo">Partially built item aggregation object</param>
		/// <param name="childInfo">Datatable from database calls with child item aggregation information ( either SobekCM_Get_Item_Aggregation or SobekCM_Get_All_Groups )</param>
		private static void add_children(Item_Aggregation aggrInfo, DataTable childInfo)
		{
			// Build a dictionary of nodes while building this tree
			Dictionary<string, Item_Aggregation_Related_Aggregations> nodes = new Dictionary<string, Item_Aggregation_Related_Aggregations>(childInfo.Rows.Count);
			
			// Step through each row of children
			foreach (DataRow thisRow in childInfo.Rows)
			{
				// pull some of the basic data out
				int hierarchyLevel = Convert.ToInt16(thisRow[5]);
				string code = thisRow[0].ToString().ToLower();
				string parentCode = thisRow[1].ToString().ToLower();

				// If this does not already exist, create it
				if (!nodes.ContainsKey(code))
				{
					// Create the object
					Item_Aggregation_Related_Aggregations childObject = new Item_Aggregation_Related_Aggregations(code, thisRow[2].ToString(), thisRow[4].ToString(), Convert.ToBoolean(thisRow[6]), Convert.ToBoolean(thisRow[7]));

					// Add this object to the node dictionary
					nodes.Add(code, childObject);

					// If this is not ALL, no need to add the full hierarchy
					if ((aggrInfo.Code == "all") || (hierarchyLevel == -1))
					{
						// Check for parent in the node list
						if ((parentCode.Length > 0) && (aggrInfo.Code != parentCode) && (nodes.ContainsKey(parentCode)))
						{
							nodes[parentCode].Add_Child_Aggregation(childObject);
						}
					}

					// If this is the first hierarchy, add to the main item aggregation object
					if (hierarchyLevel == -1)
					{
						aggrInfo.Add_Child_Aggregation(childObject);
					}
				}
			}
		}

        /// <summary> Adds the child information to the item aggregation object from the datatable extracted from the database </summary>
		/// <param name="aggrInfo">Partially built item aggregation object</param>
        /// <param name="parentInfo">Datatable from database calls with parent item aggregation information ( from  SobekCM_Get_Item_Aggregation only )</param>
        private static void add_parents(Item_Aggregation aggrInfo, DataTable parentInfo)
        {
            foreach (DataRow parentRow in parentInfo.Rows)
            {
                Item_Aggregation_Related_Aggregations parentObject = new Item_Aggregation_Related_Aggregations(parentRow[0].ToString(), parentRow[1].ToString(), parentRow[3].ToString(), Convert.ToBoolean(parentRow[4]), false);
                aggrInfo.Add_Parent_Aggregation(parentObject);
            }
        }

		/// <summary> Adds the search terms to display under advanced search from the datatable extracted from the database 
		/// and also the list of browseable fields for this collection </summary>
		/// <param name="aggrInfo">Partially built item aggregation object</param>
		/// <param name="searchTermsTable"> Table of all advanced search values </param>
		private static void add_advanced_terms(Item_Aggregation aggrInfo, DataTable searchTermsTable )
		{
			// Add ANYWHERE first
			aggrInfo.Advanced_Search_Fields.Add(-1);

			// Add values either default values or from the table
			if (( searchTermsTable == null ) || ( searchTermsTable.Rows.Count == 0 ))
			{
				aggrInfo.Advanced_Search_Fields.Add(4);
				aggrInfo.Advanced_Search_Fields.Add(3);
				aggrInfo.Advanced_Search_Fields.Add(6);
				aggrInfo.Advanced_Search_Fields.Add(5);
				aggrInfo.Advanced_Search_Fields.Add(7);
				aggrInfo.Advanced_Search_Fields.Add(1);
				aggrInfo.Advanced_Search_Fields.Add(2);

                aggrInfo.Browseable_Fields.Add(4);
                aggrInfo.Browseable_Fields.Add(3);
                aggrInfo.Browseable_Fields.Add(6);
                aggrInfo.Browseable_Fields.Add(5);
                aggrInfo.Browseable_Fields.Add(7);
                aggrInfo.Browseable_Fields.Add(1);
                aggrInfo.Browseable_Fields.Add(2);   

			}
			else
			{
				short lastTypeId = -1;
				foreach( DataRow thisRow in searchTermsTable.Rows )
				{
					short thisTypeId = Convert.ToInt16(thisRow[0]);
					if ((thisTypeId != lastTypeId) && (!aggrInfo.Advanced_Search_Fields.Contains(thisTypeId)))
					{
						aggrInfo.Advanced_Search_Fields.Add(thisTypeId);
						lastTypeId = thisTypeId;
					}
				    bool canBrowse = Convert.ToBoolean(thisRow[1]);
                    if ((canBrowse) && (!aggrInfo.Browseable_Fields.Contains(thisTypeId)))
                    {
                        aggrInfo.Browseable_Fields.Add(thisTypeId);
                    }
				}
			}
		}

		/// <summary> Adds the page count, item count, and title count to the item aggregation object from the datatable extracted from the database </summary>
		/// <param name="aggrInfo">Partially built item aggregation object</param>
		/// <param name="countInfo">Datatable from database calls with page count, item count, and title count ( from either SobekCM_Get_Item_Aggregation or SobekCM_Get_All_Groups )</param>
		private static void add_counts(Item_Aggregation aggrInfo, DataTable countInfo)
		{
			if (countInfo.Rows.Count > 0)
			{
				aggrInfo.Page_Count = Convert.ToInt32(countInfo.Rows[0]["Page_Count"]);
				aggrInfo.Item_Count = Convert.ToInt32(countInfo.Rows[0]["Item_Count"]);
				aggrInfo.Title_Count = Convert.ToInt32(countInfo.Rows[0]["Title_Count"]);
			}
		}

		#endregion

		#region Methods to get the support objects.. Interfaces, Portals, Search stop words, and Search Fields

		/// <summary> Gets the list of all search stop words which are ignored during searching ( such as 'The', 'A', etc.. ) </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> List of all the search stop words from the database </returns>
		/// <remarks> This calls the 'SobekCM_Get_Search_Stop_Words' stored procedure </remarks>
		public static List<string> Search_Stop_Words( Custom_Tracer tracer )
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Search_Stop_Words", "Pull search stop words from the database");
			}

			// Build return list
			List<string> returnValue = new List<string>();

			try
			{
				// Create the connection
				using (SqlConnection connect = new SqlConnection(connectionString))
				{
					SqlCommand executeCommand = new SqlCommand("SobekCM_Get_Search_Stop_Words", connect)
													{CommandType = CommandType.StoredProcedure};

					// Create the data reader
					connect.Open();
					using (SqlDataReader reader = executeCommand.ExecuteReader())
					{
						while (reader.Read())
						{
							// Grab the values out
							returnValue.Add( reader.GetString(1));
						}
						reader.Close();
					}
					connect.Close();
				}

				// Return the first table from the returned dataset
				return returnValue;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Populates the collection of the thematic headings for the main home page </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <param name="Thematic_Heading_List"> List to populate with the thematic headings from the database</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Manager_Get_Thematic_Headings' stored procedure </remarks> 
		public static bool Populate_Thematic_Headings(List<Thematic_Heading> Thematic_Heading_List, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Populate_Thematic_Headings", "Pull thematic heading information from the database");
			}

			try
			{

				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Manager_Get_Thematic_Headings");

				// If there was no data for this collection and entry point, return null (an ERROR occurred)
				if (tempSet.Tables.Count > 0)
				{
					// Clear the current list
					Thematic_Heading_List.Clear();

					// Add them back
					Thematic_Heading_List.AddRange(from DataRow thisRow in tempSet.Tables[0].Rows select new Thematic_Heading(Convert.ToInt16(thisRow["ThematicHeadingID"]), thisRow["ThemeName"].ToString()));
				}

				// Return the built collection as readonly
				return true;
			}
            catch (Exception ee)
            {
                lastException = ee;
                return false;
            }
		}

		/// <summary> Populates the lookup tables for aliases which point to live aggregations </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <param name="Aggregation_Alias_List"> List of aggregation aliases to populate from the database</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Get_Item_Aggregation_Aliases' stored procedure </remarks> 
		public static bool Populate_Aggregation_Aliases(Dictionary<string, string> Aggregation_Alias_List, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Populate_Aggregation_Aliases", "Pull item aggregation aliases from the database");
			}

			try
			{
				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Item_Aggregation_Aliases");

				// If there was no data for this collection and entry point, return null (an ERROR occurred)
				if ((tempSet.Tables.Count > 0) || (tempSet.Tables[0].Rows.Count > 0))
				{
					// Clear the old list
					Aggregation_Alias_List.Clear();

					foreach (DataRow thisRow in tempSet.Tables[0].Rows)
					{
						Aggregation_Alias_List[thisRow["AggregationAlias"].ToString()] = thisRow["Code"].ToString().ToLower();
					}
				}

				return true;
			}
			catch ( Exception ee )
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Datatable with the information for every html skin from the database </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Datatable with all the html skin information to be loaded into the <see cref="Skins.SobekCM_Skin_Collection"/> object. </returns>
		/// <remarks> This calls the 'SobekCM_Get_Web_Skins' stored procedure </remarks> 
		public static DataTable Get_All_Web_Skins(Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_All_Skins", "Pull display skin information from the database");
			}

			// Define a temporary dataset
			DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Web_Skins");

			// If there was no data for this collection and entry point, return null (an ERROR occurred)
			if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
			{
				return null;
			}

			// Return the built search fields object
			return tempSet.Tables[0];
		}

	
		#endregion

		#region Methods to get the URL portals, edit and save new URL portals, and delete URL portals


		/// <summary> Populates the collection of possible portals from the database </summary>
		/// <param name="Portals"> List of possible URL portals into this library/cms </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successul, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Get_All_Portals' stored procedure </remarks>
		public static bool Populate_URL_Portals(Portal_List Portals, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Populate_URL_Portals", "Pull URL portal information from the database");
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@activeonly", true);

				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_All_Portals", paramList);

				lock (Portals)
				{
					// Clear the current list
					Portals.Clear();

					// If there was no data for this collection and entry point, return null (an ERROR occurred)
					if (tempSet.Tables.Count > 0)
					{
						// Add each provided portal
						foreach (DataRow thisRow in tempSet.Tables[0].Rows)
						{
							// Pull the basic data for this portal
							int portalId = Convert.ToInt16(thisRow[0]);
							string baseUrl = thisRow[1].ToString().Trim();
							bool isDefault = Convert.ToBoolean(thisRow[3]);
							string abbreviation = thisRow[4].ToString().Trim();
							string name = thisRow[5].ToString().Trim();
							string basePurl = thisRow[6].ToString().Trim();

							if (isDefault)
							{
								if ((baseUrl == "*") || (baseUrl == "default"))
									baseUrl = String.Empty;
							}

							// Get matching skins and aggregations
							DataRow[] aggrs = tempSet.Tables[1].Select("PortalID=" + portalId);
							DataRow[] skins = tempSet.Tables[2].Select("PortalID=" + portalId);

							// Find the default aggregation
							string defaultAggr = String.Empty;
							if (aggrs.Length > 0)
								defaultAggr = aggrs[0][1].ToString().ToLower();

							// Find the default skin
							string defaultSkin = String.Empty;
							if (skins.Length > 0)
								defaultSkin = skins[0][1].ToString().ToLower();

							// Add this portal
							Portal newPortal = Portals.Add_Portal(portalId, name, abbreviation, defaultAggr, defaultSkin, baseUrl, basePurl);

							// If this is default, set it
							if (isDefault)
								Portals.Default_Portal = newPortal;
						}
					}
				}

				if (Portals.Count == 0)
				{
					// Add the default url portal then
					Portals.Default_Portal = Portals.Add_Portal(-1, "Default SobekCM Library", "Sobek", "all", "sobek", "", "");
				}

				// Return the built collection as readonly
				return true;
			}
			catch
			{
				// Add the default url portal then
				Portals.Default_Portal = Portals.Add_Portal(-1, "Default SobekCM Library", "Sobek", "all", "sobek", "", "");

				return false;
			}
		}

		/// <summary> Delete a URL Portal from the database, by primary key </summary>
		/// <param name="Portal_ID"> Primary key for the URL portal to be deleted </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successul, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Delete_Portal' stored procedure </remarks>
		public static bool Delete_URL_Portal( int Portal_ID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Delete_URL_Portal", "Delete a URL Portal by portal id ( " + Portal_ID + " )");
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@portalid", Portal_ID);

				// Define a temporary dataset
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Delete_Portal", paramList);

				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary> Edit an existing URL Portal or add a new URL portal, by primary key </summary>
		/// <param name="Portal_ID"> Primary key for the URL portal to be edited, or -1 if this is a new URL portal </param>
		/// <param name="Default_Aggregation"> Default aggregation for this URL portal </param>
		/// <param name="Default_Web_Skin"> Default web skin for this URL portal </param>
		/// <param name="Base_PURL"> Base PURL , used to override the default PURL built from the current URL</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <param name="Base_URL"> URL used to match the incoming request with this URL portal</param>
		/// <param name="isActive"> Flag indicates if this URL portal is active</param>
		/// <param name="isDefault"> Flag indicates if this is the default URL portal, if no other portal match is found</param>
		/// <param name="Abbreviation"> Abbreviation for this system, when referenced by this URL portal</param>
		/// <param name="Name"> Name of this system, when referenced by this URL portal </param>
		/// <returns> New primary key (or existing key) for the URL portal added or edited </returns>
		/// <remarks> This calls the 'SobekCM_Edit_Portal' stored procedure </remarks>
		public static int Edit_URL_Portal(int Portal_ID, string Base_URL, bool isActive, bool isDefault, string Abbreviation, string Name, string Default_Aggregation, string Default_Web_Skin, string Base_PURL, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Edit_URL_Portal", "Edit an existing URL portal, or add a new one");
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[10];
				paramList[0] = new SqlParameter("@PortalID", Portal_ID);
				paramList[1] = new SqlParameter("@Base_URL", Base_URL);
				paramList[2] = new SqlParameter("@isActive", isActive);
				paramList[3] = new SqlParameter("@isDefault", isDefault);
				paramList[4] = new SqlParameter("@Abbreviation", Abbreviation);
				paramList[5] = new SqlParameter("@Name", Name);
				paramList[6] = new SqlParameter("@Default_Aggregation", Default_Aggregation);
				paramList[7] = new SqlParameter("@Base_PURL", Base_PURL);
				paramList[8] = new SqlParameter("@Default_Web_Skin", Default_Web_Skin);
				paramList[9] = new SqlParameter("@NewID", Portal_ID) {Direction = ParameterDirection.InputOutput};

				// Define a temporary dataset
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Edit_Portal", paramList);

				return Convert.ToInt32( paramList[9].Value );
			}
			catch (Exception)
			{
				return -1;
			}
		}

		#endregion

		#region Methods to get all of the Application-Wide values

		/// <summary> Populates the translation / language support object for translating common UI terms </summary>
		/// <param name="Translations"> Translations object to populate from the database </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Get_Translation' stored procedure </remarks> 
		public static bool Populate_Translations( Language_Support_Info Translations, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Populate_Translations", String.Empty);
			}

			try
			{
				// Create the connection
				using (SqlConnection connect = new SqlConnection(connectionString))
				{
					SqlCommand executeCommand = new SqlCommand("SobekCM_Get_Translation", connect)
													{CommandType = CommandType.StoredProcedure};

					// Clear the translation information
					Translations.Clear();

					// Create the data reader
					connect.Open();
					using (SqlDataReader reader = executeCommand.ExecuteReader())
					{
						while (reader.Read())
						{
							Translations.Add_French( reader.GetString(1), reader.GetString(2));
							Translations.Add_Spanish( reader.GetString(1), reader.GetString(3));
						}
						reader.Close();
					}
					connect.Close();
				}

				// Return the first table from the returned dataset
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Populates the code manager object for translating SobekCM codes to greenstone collection codes </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <param name="Codes"> Code object to populate with the all the code and aggregation information</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Get_Codes' stored procedure </remarks> 
		public static bool Populate_Code_Manager(Aggregation_Code_Manager Codes, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Populate_Code_Manager", String.Empty);
			}

			// Create the connection
			using (SqlConnection connect = new SqlConnection(connectionString))
			{
				SqlCommand executeCommand = new SqlCommand("SobekCM_Get_Codes", connect);

				// Create the data reader
				connect.Open();
				using (SqlDataReader reader = executeCommand.ExecuteReader())
				{
					// Clear the codes list and then move in the new data
					Codes.Clear();

					// get the column indexes out
					const int codeCol = 0;
					const int typeCol = 1;
					const int nameCol = 2;
					const int shortNameCol = 3;
					const int isActiveCol = 4;
					const int hiddenCol = 5;
					const int idCol = 6;
					const int descCol = 7;
					const int themeCol = 8;
					const int linkCol = 9;

					while (reader.Read())
					{
						// Get the list key values out 
						string code = reader.GetString(codeCol).ToUpper();
						string type = reader.GetString( typeCol);
						int theme = reader.GetInt32(themeCol);

						// Only do anything else if this is not somehow a repeat
						if ( !Codes.isValidCode(code))
						{
							// Create the object
							Item_Aggregation_Related_Aggregations thisAggr =
								new Item_Aggregation_Related_Aggregations(code, reader.GetString(nameCol),
																		  reader.GetString(shortNameCol), type,
																		  reader.GetBoolean(isActiveCol),
																		  reader.GetBoolean(hiddenCol),
																		  reader.GetString(descCol),
																		  (ushort) reader.GetInt32(idCol))
									{External_Link = reader.GetString(linkCol)};

							// Add this to the codes manager
							Codes.Add_Collection(thisAggr, theme );
						}
					}
					reader.Close();
				}
				connect.Close();
			}

			// Succesful
			return true;
		}

		/// <summary> Populates the dictionary of all icons from the database </summary>
		/// <param name="Icon_List"> List of icons to be populated with a successful database pulll </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Icon_List' stored procedure <br /><br />
		/// The lookup values in this dictionary are the icon code uppercased.</remarks> 
		public static bool Populate_Icon_List(Dictionary<string, Wordmark_Icon> Icon_List, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Populate_Icon_List", String.Empty);
			}

			// Create the connection
			using (SqlConnection connect = new SqlConnection(connectionString))
			{
				SqlCommand executeCommand = new SqlCommand("SobekCM_Icon_List", connect)
												{CommandType = CommandType.StoredProcedure};


				// Create the data reader
				connect.Open();
				using (SqlDataReader reader = executeCommand.ExecuteReader())
				{
					// Clear existing icons
					Icon_List.Clear();

					while (reader.Read())
					{
						string code = reader.GetString(0).ToUpper();
    					Icon_List[code] = new Wordmark_Icon(code, reader.GetString(1), reader.GetString(2), reader.GetString(3));
					}
					reader.Close();
				}
				connect.Close();
			}

			// Succesful
			return true;
		}

        /// <summary> Populates the dictionary of all files and MIME types from the database </summary>
        /// <param name="MIME_List"> List of files and MIME types to be populated with a successful database pulll </param>
        /// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
        /// <returns> TRUE if successful, otherwise FALSE </returns>
        /// <remarks> This calls the 'SobekCM_Get_Mime_Types' stored procedure <br /><br />
        /// The lookup values in this dictionary are the file extensions in lower case.</remarks> 
        public static bool Populate_MIME_List(Dictionary<string, Mime_Type_Info> MIME_List, Custom_Tracer tracer)
        {
            if (tracer != null)
            {
                tracer.Add_Trace("SobekCM_Database.Populate_MIME_List", String.Empty);
            }

            // Create the connection
            using (SqlConnection connect = new SqlConnection(connectionString))
            {
                SqlCommand executeCommand = new SqlCommand("SobekCM_Get_Mime_Types", connect) { CommandType = CommandType.StoredProcedure };


                // Create the data reader
                connect.Open();
                using (SqlDataReader reader = executeCommand.ExecuteReader())
                {
                    // Clear existing icons
                    MIME_List.Clear();

                    while (reader.Read())
                    {
                        string extension = reader.GetString(0).ToLower();
                        MIME_List[extension] = new Mime_Type_Info(extension, reader.GetString(1), reader.GetBoolean(2), reader.GetBoolean(3));
                    }
                    reader.Close();
                }
                connect.Close();
            }

            // Succesful
            return true;
        }

		/// <summary> Gets complete information for an item which may be missing from the complete list of items </summary>
		/// <param name="BibID"> Bibliographic identifiers for the item of interest </param>
		/// <param name="VID"> Volume identifiers for the item of interest </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Datarow with additional information about an item, including spatial details, publisher, donor, etc.. </returns>
		/// <remarks> This calls the 'SobekCM_Get_Item_Brief_Info' stored procedure </remarks> 
		public static DataRow Get_Item_Information(string BibID, string VID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Item_Information", "Trying to pull information for " + BibID + "_" + VID);
			}

			try
			{
				SqlParameter[] parameters = new SqlParameter[3];
				parameters[0] = new SqlParameter("@bibid", BibID);
				parameters[1] = new SqlParameter("@vid", VID);
				parameters[2] = new SqlParameter("@include_aggregations", false);

				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Item_Brief_Info", parameters);

				// If there was no data for this collection and entry point, return null (an ERROR occurred)
				if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
				{
					return null;
				}

				// Return the first table from the returned dataset
				return tempSet.Tables[0].Rows[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Gets complete information for an item which may be missing from the complete list of items </summary>
		/// <param name="BibID"> Bibliographic identifiers for the item of interest </param>
		/// <param name="VID"> Volume identifiers for the item of interest </param>
		/// <param name="Include_Aggregations"> Flag indicates whether to include the aggregations </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Datarow with additional information about an item, including spatial details, publisher, donor, etc.. </returns>
		/// <remarks> This calls the 'SobekCM_Get_Item_Brief_Info' stored procedure </remarks> 
		public static DataSet Get_Item_Information(string BibID, string VID, bool Include_Aggregations, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Item_Information", "Trying to pull information for " + BibID + "_" + VID);
			}

			try
			{
				SqlParameter[] parameters = new SqlParameter[3];
				parameters[0] = new SqlParameter("@bibid", BibID);
				parameters[1] = new SqlParameter("@vid", VID);
				parameters[2] = new SqlParameter("@include_aggregations", Include_Aggregations);

				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Item_Brief_Info", parameters);

				// If there was no data for this collection and entry point, return null (an ERROR occurred)
				if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
				{
					return null;
				}

				// Return the first table from the returned dataset
				return tempSet;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Verified the item lookup object is filled, or populates the item lookup object with all the valid bibids and vids in the system </summary>
		/// <param name="Include_Private"> Flag indicates whether to include private items in this list </param>
		/// <param name="itemLookupObject"> [REF] Item lookup object to directly populate from the database </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful or if the object is already filled, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Item_List_Web' stored procedure </remarks> 
		public static bool Verify_Item_Lookup_Object(bool Include_Private, ref Item_Lookup_Object itemLookupObject, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Verify_Item_Lookup_Object", String.Empty);
			}

            // If no database string, don't try to connect
            if (String.IsNullOrEmpty(connectionString))
                return false;

			lock (itemListPopulationLock)
			{
				bool updateList = true;
				if (itemLookupObject != null)
				{
					TimeSpan sinceLastUpdate = DateTime.Now.Subtract(itemLookupObject.Last_Updated);
					if (sinceLastUpdate.TotalMinutes <= 1)
						updateList = false;
				}

				if (!updateList)
				{
					return true;
				}
				
				if (itemLookupObject == null)
					itemLookupObject = new Item_Lookup_Object();

				// Have the database popoulate the little bit of bibid/vid information we retain
				bool returnValue = Populate_Item_Lookup_Object(Include_Private, itemLookupObject, tracer);
				if (returnValue)
					itemLookupObject.Last_Updated = DateTime.Now;
				return returnValue;
			}
		}


		/// <summary> Populates the item lookup object with all the valid bibids and vids in the system </summary>
		/// <param name="Include_Private"> Flag indicates whether to include private items in this list </param>
		/// <param name="itemLookupObject"> Item lookup object to directly populate from the database </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Item_List_Web' stored procedure </remarks> 
		public static bool Populate_Item_Lookup_Object(bool Include_Private, Item_Lookup_Object itemLookupObject, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Populate_Item_Lookup_Object", String.Empty);
			}

			try
			{

				// Create the connection
				using (SqlConnection connect = new SqlConnection(connectionString + "Connection Timeout=45"))
				{

					SqlCommand executeCommand = new SqlCommand("SobekCM_Item_List_Web", connect)
													{CommandTimeout = 45, CommandType = CommandType.StoredProcedure};
					executeCommand.Parameters.AddWithValue("@include_private", Include_Private);

					// Create the data reader
					connect.Open();
					using (SqlDataReader reader = executeCommand.ExecuteReader())
					{
						// Clear existing volumes
						itemLookupObject.Clear();
						itemLookupObject.Last_Updated = DateTime.Now;

						string currentBibid = String.Empty;
						Multiple_Volume_Item currentVolume = null;
						while (reader.Read())
						{
							// Grab the values out
							string newBib = reader.GetString(0);
							string newVid = reader.GetString(1);
							short newMask = reader.GetInt16(2);
							string title = reader.GetString(3);

							// Create a new multiple volume object?
							if (newBib != currentBibid)
							{
								currentBibid = newBib;
								currentVolume = new Multiple_Volume_Item(newBib);
								itemLookupObject.Add_Title(currentVolume);
							}

							// Add this volume
							Single_Item newItem = new Single_Item( newVid, newMask, title);
							if (currentVolume != null) currentVolume.Add_Item(newItem);
						}
						reader.Close();
					}
					connect.Close();
				}

				// Return the first table from the returned dataset
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

        /// <summary> Get the list of groups, with the top item (VID) </summary>
        /// <returns> List of groups, with the top item (VID) </returns>
        public static DataTable Get_All_Groups_First_VID()
        {
            // Define a temporary dataset
            SqlConnection connection = new SqlConnection(connectionString + "Connection Timeout=45");
            SqlCommand command = new SqlCommand("SobekCM_Get_All_Groups_First_VID", connection) { CommandTimeout = 45, CommandType = CommandType.StoredProcedure };
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataSet tempSet = new DataSet();
            adapter.Fill(tempSet);

            // If there was no data for this collection and entry point, return null (an ERROR occurred)
            if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
            {
                return null;
            }

            // Return the first table from the returned dataset
            return tempSet.Tables[0];
        }


		/// <summary> Gets the dataset of all public items and item groups </summary>
		/// <param name="Include_Private"> Flag indicates whether to include private items in this list </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Dataset of all items and item groups </returns>
		/// <remarks> This calls the 'SobekCM_Item_List_Brief2' stored procedure </remarks> 
		public static DataSet Get_Item_List( bool Include_Private, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Item_List", String.Empty);
			}

			// Define a temporary dataset
			SqlConnection connection = new SqlConnection(connectionString + "Connection Timeout=45");
			SqlCommand command = new SqlCommand("SobekCM_Item_List_Brief2", connection)
									 {CommandTimeout = 45, CommandType = CommandType.StoredProcedure};
			command.Parameters.AddWithValue("@include_private", Include_Private);
			SqlDataAdapter adapter = new SqlDataAdapter(command);
			DataSet tempSet = new DataSet();
			adapter.Fill(tempSet);
				
			// If there was no data for this collection and entry point, return null (an ERROR occurred)
			if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
			{
				return null;
			}

			// Return the first table from the returned dataset
			return tempSet;
		}

		/// <summary> Gets the simple list of items for a single item aggregation, or the list of all items in the library </summary>
		/// <param name="Aggregation_Code"> Code for the item aggregation of interest, or an empty string</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Dataset with the simple list of items, including BibID, VID, Title, CreateDate, and Resource Link </returns>
		/// <remarks> This calls the 'SobekCM_Simple_Item_List' stored procedure </remarks> 
		public static DataSet Simple_Item_List(string Aggregation_Code, Custom_Tracer tracer )
		{
			if (tracer != null)
			{
				if ( Aggregation_Code.Length == 0 )
					tracer.Add_Trace("SobekCM_Database.Simple_Item_List", "Pulling simple item list for all items");
				else
					tracer.Add_Trace("SobekCM_Database.Simple_Item_List", "Pulling simple item list for '" + Aggregation_Code + "'");
			}

			// Define a temporary dataset
			SqlParameter[] parameters = new SqlParameter[1];
			parameters[0] = new SqlParameter("@collection_code", Aggregation_Code);
			DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Simple_Item_List", parameters);
			return tempSet;
		}

		#endregion

		#region Methods to support the restriction by IP addresses

		/// <summary> Gets the list of all the IP ranges for restriction, including each single IP information in those ranges </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataTable with all the data about the IP ranges used for restrictions </returns>
		/// <remarks> This calls the 'SobekCM_Get_All_IP_Restrictions' stored procedure </remarks> 
		public static DataTable Get_IP_Restriction_Ranges( Custom_Tracer tracer )
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_IP_Restriction_Range", "Pulls all the IP restriction range inforamtion");
			}

			try
			{
				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_All_IP_Restrictions");

				// Return the first table from the returned dataset
				return tempSet.Tables[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Gets the details for a single IP restriction ranges, including each single IP and the complete notes associated with the range </summary>
		/// <param name="PrimaryID"> Primary id to the IP restriction range to pull details for </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataTable with all the data about the IP ranges used for restrictions </returns>
		/// <remarks> This calls the 'SobekCM_Get_IP_Restriction_Range' stored procedure </remarks> 
		public static DataSet Get_IP_Restriction_Range_Details(int PrimaryID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_IP_Restriction_Range_Details", "Pulls all the IP restriction range details for range #" + PrimaryID );
			}

			try
			{
				SqlParameter[] parameters = new SqlParameter[1];
				parameters[0] = new SqlParameter("@ip_rangeid", PrimaryID);

				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_IP_Restriction_Range", parameters);

				// If there was no data for this collection and entry point, return null (an ERROR occurred)
				if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
				{
					return null;
				}

				// Return the first table from the returned dataset
				return tempSet;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Deletes a single IP address information from an IP restriction range </summary>
		/// <param name="PrimaryID"> Primary key for this single IP address information to delete </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Delete_Single_IP' stored procedure </remarks> 
		public static bool Delete_Single_IP(int PrimaryID, Custom_Tracer tracer )
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Delete_Single_IP", "Delete single IP information within a range");
			}

			try
			{
				SqlParameter[] parameters = new SqlParameter[1];
				parameters[0] = new SqlParameter("@ip_singleid", PrimaryID);

				// Define a temporary dataset
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Delete_Single_IP", parameters);

				// Return the first table from the returned dataset
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Adds or edits a single IP address information in an IP restriction range </summary>
		/// <param name="PrimaryID"> Primary key for this single IP address information to add, or -1 to add a new IP address </param>
		/// <param name="IP_RangeID"> Primary key for the IP restriction range to add this single IP address information </param>
		/// <param name="Start_IP"> Beginning of the IP range, or the complete IP address </param>
		/// <param name="End_IP"> End of the IP range, if this was a true range </param>
		/// <param name="Note"> Any note associated with this single IP information </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Primary key for the single IP address information, if no primary key was originally provided </returns>
		/// <remarks> This calls the 'SobekCM_Edit_Single_IP' stored procedure </remarks> 
		public static int Edit_Single_IP(int PrimaryID, int IP_RangeID, string Start_IP, string End_IP, string Note, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Edit_Single_IP", "Edit a single IP within a restriction range");
			}

			try
			{
				SqlParameter[] parameters = new SqlParameter[6];
				parameters[0] = new SqlParameter("@ip_singleid", PrimaryID);
				parameters[1] = new SqlParameter("@ip_rangeid", IP_RangeID );
				parameters[2] = new SqlParameter("@startip", Start_IP);
				parameters[3] = new SqlParameter("@endip", End_IP);
				parameters[4] = new SqlParameter("@notes", Note );
				parameters[5] = new SqlParameter("@new_ip_singleid", -1) {Direction = ParameterDirection.InputOutput};

				// Define a temporary dataset
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Edit_Single_IP", parameters);

				// Return the first table from the returned dataset
				return Convert.ToInt32(parameters[5].Value);
			}
			catch (Exception ee)
			{
				lastException = ee;
				return -1;
			}
		}


		/// <summary> Edits an existing IP restriction range </summary>
		/// <param name="IP_RangeID"> Primary key for the IP restriction range  </param>
		/// <param name="Title"> Title for this IP Restriction Range </param>
		/// <param name="Notes"> Notes about this IP Restriction Range (for system admins)</param>
		/// <param name="Item_Restricted_Statement"> Statement used when a user directly requests an item for which they do not the pre-requisite access </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Edit_IP_Range' stored procedure </remarks> 
		public static bool Edit_IP_Range(int IP_RangeID, string Title, string Notes, string Item_Restricted_Statement, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Edit_IP_Range", "Edit an existing IP restriction range");
			}

			try
			{
				SqlParameter[] parameters = new SqlParameter[4];
				parameters[0] = new SqlParameter("@rangeid", IP_RangeID);
				parameters[1] = new SqlParameter("@title", Title);
				parameters[2] = new SqlParameter("@notes", Notes);
				parameters[3] = new SqlParameter("@not_valid_statement", Item_Restricted_Statement);

				// Execute the stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Edit_IP_Range", parameters);

				// Return true if successful
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}


		#endregion

		#region Methods to get authority type information

		/// <summary> Gets the list of all map features linked to a particular item  </summary>
		/// <param name="ItemID"> ItemID for the item of interest</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> List of all features linked to the item of interest </returns>
		/// <remarks> This calls the 'Auth_Get_All_Features_By_Item' stored procedure </remarks> 
		public static Map_Features_DataSet Get_All_Features_By_Item(int ItemID, Custom_Tracer tracer)
		{
			try
			{
				// Create the connection
				SqlConnection connect = new SqlConnection( connectionString );

				// Create the command 
				SqlCommand executeCommand = new SqlCommand("Auth_Get_All_Features_By_Item", connect)
												{CommandType = CommandType.StoredProcedure};
				executeCommand.Parameters.AddWithValue( "@itemid", ItemID );
				executeCommand.Parameters.AddWithValue( "@filter", 1 );

				// Create the adapter
				SqlDataAdapter adapter = new SqlDataAdapter( executeCommand );

				// Add appropriate table mappings
				adapter.TableMappings.Add("Table", "Features");
				adapter.TableMappings.Add("Table1", "Types");

				// Fill the strongly typed dataset
				Map_Features_DataSet features = new Map_Features_DataSet();
				adapter.Fill( features );

				// Return the fully built object
				return features;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Gets the list of all streets linked to a particular item  </summary>
		/// <param name="ItemID"> ItemID for the item of interest</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> List of all streets linked to the item of interest </returns>
		/// <remarks> This calls the 'Auth_Get_All_Streets_By_Item' stored procedure </remarks> 
		public static Map_Streets_DataSet Get_All_Streets_By_Item(int ItemID, Custom_Tracer tracer)
		{
			try
			{
				// Create the connection
				SqlConnection connect = new SqlConnection( connectionString );

				// Create the command 
				SqlCommand executeCommand = new SqlCommand("Auth_Get_All_Streets_By_Item", connect)
												{CommandType = CommandType.StoredProcedure};
				executeCommand.Parameters.AddWithValue( "@itemid", ItemID );

				// Create the adapter
				SqlDataAdapter adapter = new SqlDataAdapter( executeCommand );

				// Add appropriate table mappings
				adapter.TableMappings.Add("Table", "Streets");


				// Fill the strongly typed dataset
				Map_Streets_DataSet streets = new Map_Streets_DataSet();
				adapter.Fill( streets );

				// Return the fully built object
				return streets;
			}
			catch ( Exception ee )
			{
				lastException = ee;
				return null;
			}
		}


		#endregion

		#region My Sobek database calls

		/// <summary> Saves information about a single user </summary>
		/// <param name="user"> <see cref="Users.User_Object"/> with all the information about the single user</param>
		/// <param name="Password"> Plain-text password, which is then encrypted prior to saving</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE</returns>
		/// <remarks> This calls the 'mySobek_Save_User2' stored procedure</remarks> 
		public static bool Save_User(User_Object user, string Password, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Save_User", String.Empty);
			}

			const string salt = "This is my salt to add to the password";
			string encryptedPassword = SecurityInfo.SHA1_EncryptString(Password + salt);

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[20];
				paramList[0] = new SqlParameter("@userid", user.UserID);
				paramList[1] = new SqlParameter("@ufid", user.UFID);
				paramList[2] = new SqlParameter("@username", user.UserName);
				paramList[3] = new SqlParameter("@password", encryptedPassword);
				paramList[4] = new SqlParameter("@emailaddress", user.Email);
				paramList[5] = new SqlParameter("@firstname", user.Given_Name);
				paramList[6] = new SqlParameter("@lastname", user.Family_Name);
				paramList[7] = new SqlParameter("@cansubmititems", user.Can_Submit);
				paramList[8] = new SqlParameter("@nickname", user.Nickname);
				paramList[9] = new SqlParameter("@organization", user.Organization);
				paramList[10] = new SqlParameter("@college", user.College);
				paramList[11] = new SqlParameter("@department", user.Department);
				paramList[12] = new SqlParameter("@unit", user.Unit);
				paramList[13] = new SqlParameter("@rights", user.Default_Rights);
				paramList[14] = new SqlParameter("@sendemail", user.Send_Email_On_Submission);
				paramList[15] = new SqlParameter("@language", user.Preferred_Language);
				if (user.Templates.Count > 0)
				{
					paramList[16] = new SqlParameter("@default_template", user.Templates[0]);
				}
				else
				{
					paramList[16] = new SqlParameter("@default_template", String.Empty);
				}
				if (user.Projects.Count > 0)
				{
					paramList[17] = new SqlParameter("@default_project", user.Projects[0]);
				}
				else
				{
					paramList[17] = new SqlParameter("@default_project", String.Empty);
				}
				paramList[18] = new SqlParameter("@organization_code", user.Organization_Code);
				paramList[19] = new SqlParameter("@receivestatsemail", user.Receive_Stats_Emails);
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Save_User2", paramList);
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}


		}

        /// <summary> Links a single user to a user group  </summary>
        /// <param name="UserID"> Primary key for the user </param>
        /// <param name="UserGroupID"> Primary key for the user group </param>
        /// <returns> TRUE if successful, otherwise FALSE </returns>
        /// <remarks> This calls the 'mySobek_Link_User_To_User_Group' stored procedure</remarks> 
        public static bool Link_User_To_User_Group( int UserID, int UserGroupID )
        {
            try
            {
                // Execute this non-query stored procedure
                SqlParameter[] paramList = new SqlParameter[2];
                paramList[0] = new SqlParameter("@userid", UserID);
                paramList[1] = new SqlParameter("@usergroupip", UserGroupID);

                SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Link_User_To_User_Group", paramList);
                return true;
            }
            catch (Exception ee)
            {
                lastException = ee;
                return false;
            }
        }

		/// <summary> Change an existing user's password </summary>
		/// <param name="username"> Username for the user </param>
		/// <param name="current_password"> Old plain-text password, which is then encrypted prior to saving</param>
		/// <param name="new_password"> New plain-text password, which is then encrypted prior to saving</param>
		/// <param name="is_temporary"> Flag indicates if the new password is temporary and must be changed on the next logon</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE</returns>
		/// <remarks> This calls the 'mySobek_Change_Password' stored procedure</remarks> 
		public static bool Change_Password(string username, string current_password, string new_password, bool is_temporary,  Custom_Tracer tracer )
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Change_Password", String.Empty);
			}

			const string salt = "This is my salt to add to the password";
			string encryptedCurrentPassword = SecurityInfo.SHA1_EncryptString(current_password + salt);
			string encryptedNewPassword = SecurityInfo.SHA1_EncryptString(new_password + salt);
			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[5];
				paramList[0] = new SqlParameter("@username", username);
				paramList[1] = new SqlParameter("@current_password", encryptedCurrentPassword);
				paramList[2] = new SqlParameter("@new_password", encryptedNewPassword);
				paramList[3] = new SqlParameter("@isTemporaryPassword", is_temporary);
				paramList[4] = new SqlParameter("@password_changed", false) {Direction = ParameterDirection.InputOutput};

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Change_Password", paramList);


				return Convert.ToBoolean(paramList[4].Value);
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}

		}

		/// <summary> Checks to see if a username or email exist </summary>
		/// <param name="userName"> Username to check</param>
		/// <param name="Email"> Email address to check</param>
		/// <param name="username_exists"> [OUT] Flag indicates if the username exists</param>
		/// <param name="email_exists"> [OUT] Flag indicates if the email exists </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE</returns>
		/// <remarks> This calls the 'mySobek_UserName_Exists' stored procedure<br /><br />
		/// This is used to enforce uniqueness during registration </remarks> 
		public static bool UserName_Exists(string userName, string Email, out bool username_exists, out bool email_exists, Custom_Tracer tracer )
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.UserName_Exists", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[4];
				paramList[0] = new SqlParameter("@username", userName);
				paramList[1] = new SqlParameter("@email", Email);
				paramList[2] = new SqlParameter("@username_exists", true) {Direction = ParameterDirection.InputOutput};
				paramList[3] = new SqlParameter("@email_exists", true) {Direction = ParameterDirection.InputOutput};

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_UserName_Exists", paramList);

				username_exists = Convert.ToBoolean(paramList[2].Value);
				email_exists = Convert.ToBoolean(paramList[3].Value);
				return true;
			}
			catch ( Exception ee )
			{
				lastException = ee;
				username_exists = true;
				email_exists = true;
				return false;
			}
		}

		/// <summary> Updates the flag that indicates the user would like to receive a monthly usage statistics email </summary>
		/// <param name="UserID"> Primary key for this user in the database </param>
		/// <param name="New_Flag"> New value for the flag </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE</returns>
		/// <remarks> This calls the 'mySobek_Set_Receive_Stats_Email_Flag' stored procedure</remarks> 
		public static bool Set_User_Receive_Stats_Email( int UserID, bool New_Flag, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Set_Receive_Stats_Email_Flag", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@userid", UserID);
				paramList[1] = new SqlParameter("@newflag", New_Flag);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Set_Receive_Stats_Email_Flag", paramList);
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Gets basic user information by UserID </summary>
		/// <param name="UserID"> Primary key for this user in the database </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Fully built <see cref="Users.User_Object"/> object </returns>
		/// <remarks> This calls the 'mySobek_Get_User_By_UserID' stored procedure<br /><br />
		/// This is called when a user's cookie exists in a web request</remarks> 
		public static User_Object Get_User(int UserID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_User", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@userid", UserID);

				DataSet resultSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "mySobek_Get_User_By_UserID", paramList);

				if ((resultSet.Tables.Count > 0) && (resultSet.Tables[0].Rows.Count > 0))
				{
					return build_user_object_from_dataset(resultSet);
				}

				// Return the browse id
				return null;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Gets basic user information by UFID </summary>
		/// <param name="UFID"> UFID for the user </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Fully built <see cref="Users.User_Object"/> object </returns>
		/// <remarks> This calls the 'mySobek_Get_User_By_UFID' stored procedure<br /><br />
		/// This method is called when user's logon through the Gatorlink Shibboleth service</remarks> 
		public static User_Object Get_User(string UFID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_User", String.Empty);
			}

			try
			{

				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@ufid", UFID);

				DataSet resultSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "mySobek_Get_User_By_UFID", paramList);

				if ((resultSet.Tables.Count > 0) && (resultSet.Tables[0].Rows.Count > 0))
				{
					return build_user_object_from_dataset(resultSet);
				}

				// Return the browse id
				return null;

			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Gets basic user information by Username (or email) and Password </summary>
		/// <param name="UserName"> UserName (or email address) for the user </param>
		/// <param name="Password"> Plain-text password, which is then encrypted prior to sending to database</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Fully built <see cref="Users.User_Object"/> object </returns>
		/// <remarks> This calls the 'mySobek_Get_User_By_UserName_Password' stored procedure<br /><br />
		/// This is used when a user logs on through the mySobek authentication</remarks> 
		public static User_Object Get_User(string UserName, string Password, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_User", String.Empty);
			}

			try
			{
				const string salt = "This is my salt to add to the password";
				string encryptedPassword = SecurityInfo.SHA1_EncryptString(Password + salt);


				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@username", UserName);
				paramList[1] = new SqlParameter("@password", encryptedPassword);

				DataSet resultSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "mySobek_Get_User_By_UserName_Password", paramList);

				if ((resultSet.Tables.Count > 0) && (resultSet.Tables[0].Rows.Count > 0))
				{
					return build_user_object_from_dataset(resultSet);
				}

				// Return the browse id
				return null;

			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		private static User_Object build_user_object_from_dataset(DataSet resultSet )
		{
			User_Object user = new User_Object();

			DataRow userRow = resultSet.Tables[0].Rows[0];
			user.UFID = userRow["UFID"].ToString();
			user.UserID = Convert.ToInt32(userRow["UserID"]);
			user.UserName = userRow["username"].ToString();
			user.Email = userRow["EmailAddress"].ToString();
			user.Given_Name = userRow["FirstName"].ToString();
			user.Family_Name = userRow["LastName"].ToString();
			user.Send_Email_On_Submission = Convert.ToBoolean(userRow["SendEmailOnSubmission"]);
			user.Can_Submit = Convert.ToBoolean(userRow["Can_Submit_Items"]);
			user.Is_Temporary_Password = Convert.ToBoolean(userRow["isTemporary_Password"]);
			user.Nickname = userRow["Nickname"].ToString();
			user.Organization = userRow["Organization"].ToString();
			user.Organization_Code = userRow["OrganizationCode"].ToString();
			user.Department = userRow["Department"].ToString();
			user.College = userRow["College"].ToString();
			user.Unit = userRow["Unit"].ToString();
			user.Default_Rights = userRow["Rights"].ToString();
			user.Preferred_Language = userRow["Language"].ToString();
			user.Is_Internal_User = Convert.ToBoolean(userRow["Internal_User"]);
			user.Edit_Template_Code = userRow["EditTemplate"].ToString();
			user.Edit_Template_MARC_Code = userRow["EditTemplateMarc"].ToString();
			user.Is_System_Admin = Convert.ToBoolean(userRow["IsSystemAdmin"]);
			user.Is_Portal_Admin = Convert.ToBoolean(userRow["IsPortalAdmin"]);
			user.Include_Tracking_In_Standard_Forms = Convert.ToBoolean(userRow["Include_Tracking_Standard_Forms"]);
			user.Receive_Stats_Emails = Convert.ToBoolean(userRow["Receive_Stats_Emails"]);
			user.Has_Item_Stats = Convert.ToBoolean(userRow["Has_Item_Stats"]);

			if (Convert.ToInt32(userRow["descriptions"]) > 0)
				user.Has_Descriptive_Tags = true;

			foreach (DataRow thisRow in resultSet.Tables[1].Rows)
			{
				user.Add_Template(thisRow["TemplateCode"].ToString());
			}

			foreach (DataRow thisRow in resultSet.Tables[2].Rows)
			{
				user.Add_Project(thisRow["ProjectCode"].ToString());
			}

			user.Items_Submitted_Count = resultSet.Tables[3 ].Rows.Count;
			foreach (DataRow thisRow in resultSet.Tables[3 ].Rows)
			{
				if (!user.BibIDs.Contains(thisRow["BibID"].ToString().ToUpper()))
					user.Add_BibID(thisRow["BibID"].ToString().ToUpper());
			}

			// Add links to regular expressions
			foreach (DataRow thisRow in resultSet.Tables[4 ].Rows)
			{
				user.Add_Editable_Regular_Expression(thisRow["EditableRegex"].ToString());
			}

			// Add links to aggregations
			foreach (DataRow thisRow in resultSet.Tables[5 ].Rows)
			{
				user.Add_Aggregation(thisRow["Code"].ToString(), thisRow["Name"].ToString(), Convert.ToBoolean(thisRow["CanSelect"]), Convert.ToBoolean(thisRow["CanEditItems"]), Convert.ToBoolean(thisRow["IsAggregationAdmin"]), Convert.ToBoolean(thisRow["OnHomePage"]));
			}

			// Add the current folder names
			Dictionary<int, User_Folder> folderNodes = new Dictionary<int, User_Folder>();
			List<User_Folder> parentNodes = new List<User_Folder>();
			foreach (DataRow folderRow in resultSet.Tables[6 ].Rows)
			{
				string folderName = folderRow["FolderName"].ToString();
				int folderid = Convert.ToInt32(folderRow["UserFolderID"]);
				int parentid = Convert.ToInt32(folderRow["ParentFolderID"]);
				bool isPublic = Convert.ToBoolean(folderRow["isPublic"]);

				User_Folder newFolderNode = new User_Folder(folderName, folderid) {isPublic = isPublic};
				if (parentid == -1)
					parentNodes.Add(newFolderNode);
				folderNodes.Add(folderid, newFolderNode);
			}
			foreach (DataRow folderRow in resultSet.Tables[6 ].Rows)
			{
				int folderid = Convert.ToInt32(folderRow["UserFolderID"]);
				int parentid = Convert.ToInt32(folderRow["ParentFolderID"]);
				if (parentid > 0)
				{
					folderNodes[parentid].Add_Child_Folder(folderNodes[folderid]);
				}
			}
			foreach (User_Folder rootFolder in parentNodes)
				user.Add_Folder(rootFolder);

			foreach (DataRow itemRow in resultSet.Tables[7 ].Rows)
			{
				user.Add_Bookshelf_Item(itemRow["BibID"].ToString(), itemRow["VID"].ToString());
			}

			foreach (DataRow groupRow in resultSet.Tables[8].Rows)
			{
				user.Add_User_Group(groupRow[0].ToString());
			}

			return user;
		}

		/// <summary> Add a link between a user and an existing item group (by GroupID) </summary>
		/// <param name="UserID"> Primary key for this user in the database </param>
		/// <param name="GroupID"> Primary key for the item group to link this user to</param>
		/// <returns>TRUE if successful, otherwise FALSE</returns>
		/// <remarks> This calls the 'mySobek_Link_User_To_Item' stored procedure</remarks> 
		public static bool Add_User_BibID_Link(int UserID, int GroupID)
		{
			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@userid", UserID);
				paramList[1] = new SqlParameter("@groupid", GroupID);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Link_User_To_Item", paramList);

				// Return the browse id
				return true;

			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Add a link between a user and an existing item and include the type of relationship </summary>
		/// <param name="UserID"> Primary key for this user in the database </param>
		/// <param name="ItemID"> Primary key for the item to link this user to</param>
		/// <param name="RelationshipID"> Primary key for the type of relationship to use </param>
		/// <param name="Change_Existing"> If a relationship already exists, should this override it? </param>
		/// <returns>TRUE if successful, otherwise FALSE</returns>
		/// <remarks> This calls the 'SobekCM_Link_User_To_Item' stored procedure</remarks> 
		public static bool Add_User_Item_Link(int UserID, int ItemID, int RelationshipID, bool Change_Existing )
		{
			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[4];
				paramList[0] = new SqlParameter("@itemid", ItemID);
				paramList[1] = new SqlParameter("@userid", UserID);
				paramList[2] = new SqlParameter("@relationshipid", RelationshipID);
				paramList[3] = new SqlParameter("@change_existing", Change_Existing);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Link_User_To_Item", paramList);

				// Return the browse id
				return true;

			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}
		
		/// <summary> Gets basic information about all the folders and searches saved for a single user </summary>
		/// <param name="UserID"> Primary key for this user in the database </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Information about all folders (and number of items) and saved searches for a user </returns>
		/// <remarks> This calls the 'mySobek_Get_Folder_Search_Information' stored procedure</remarks> 
		public static DataSet Get_Folder_Search_Information(int UserID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Folder_Search_Information", String.Empty);
			}

			try
			{

				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@userid", UserID);

				DataSet resultSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "mySobek_Get_Folder_Search_Information", paramList);

				return resultSet;

			}
			catch ( Exception ee )
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Deletes a user search from the collection of saved searches </summary>
		/// <param name="UserSearchID"> Primary key for this saved search </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'mySobek_Delete_User_Search' stored procedure</remarks> 
		public static bool Delete_User_Search(int UserSearchID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Delete_User_Search", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@usersearchid", UserSearchID);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Delete_User_Search", paramList);

				// Return TRUE
				return true;

			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Gets the list of all saved user searches and any user comments </summary>
		/// <param name="UserID"> Primary key for this user in the database </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Table of all the saved searches for this user </returns>
		/// <remarks> This calls the 'mySobek_Get_User_Searches' stored procedure</remarks> 
		public static DataTable Get_User_Searches(int UserID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_User_Searches", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@userid", UserID);

				DataSet resultSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "mySobek_Get_User_Searches", paramList);

				return resultSet.Tables[0];

			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Saves a search to the user's saved searches </summary>
		/// <param name="UserID"> Primary key for this user in the database </param>
		/// <param name="Search_URL"> SobekCM search URL </param>
		/// <param name="Search_Description"> Programmatic description of this search</param>
		/// <param name="ItemOrder"> Order for this search within the folder</param>
		/// <param name="UserNotes"> Notes from the user about this search </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> New UserSearchID, or -1 if this edits an existing one </returns>
		/// <remarks> This calls the 'mySobek_Save_User_Search' stored procedure</remarks> 
		public static int Save_User_Search(int UserID, string Search_URL, string Search_Description, int ItemOrder, string UserNotes, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Save_User_Search", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[6];
				paramList[0] = new SqlParameter("@userid", UserID);
				paramList[1] = new SqlParameter("@searchurl", Search_URL);
				paramList[2] = new SqlParameter("@searchdescription", Search_Description);
				paramList[3] = new SqlParameter("@itemorder", ItemOrder);
				paramList[4] = new SqlParameter("@usernotes", UserNotes);
				paramList[5] = new SqlParameter("@new_usersearchid", -1) {Direction = ParameterDirection.InputOutput};

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Save_User_Search", paramList);


				// Return TRUE
				return Convert.ToInt32(paramList[5].Value);

			}
			catch (Exception ee)
			{
				lastException = ee;
				return -1000;
			}
		}

		/// <summary> Remove an item from the user's folder </summary>
		/// <param name="UserID"> Primary key for this user in the database </param>
		/// <param name="FolderName"> Name of this user's folder </param>
		/// <param name="BibID"> Bibliographic identifier for this title / item group </param>
		/// <param name="VID"> Volume identifier for this one volume within a title / item group </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'mySobek_Delete_Item_From_User_Folder' stored procedure</remarks> 
		public static bool Delete_Item_From_User_Folder(int UserID, string FolderName, string BibID, string VID, Custom_Tracer tracer )
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Delete_Item_From_User_Folder", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[4];
				paramList[0] = new SqlParameter("@userid", UserID);
				paramList[1] = new SqlParameter("@foldername", FolderName);
				paramList[2] = new SqlParameter("@bibid", BibID);
				paramList[3] = new SqlParameter("@vid", VID);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Delete_Item_From_User_Folder", paramList);

				// Return TRUE
				return true;

			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Remove an item from any user folder it currently resides in (besides Submitted Items)</summary>
		/// <param name="UserID"> Primary key for this user in the database </param>
		/// <param name="BibID"> Bibliographic identifier for this title / item group </param>
		/// <param name="VID"> Volume identifier for this one volume within a title / item group </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'mySobek_Delete_Item_From_All_User_Folders' stored procedure</remarks> 
		public static bool Delete_Item_From_User_Folders(int UserID, string BibID, string VID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Delete_Item_From_User_Folder", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[3];
				paramList[0] = new SqlParameter("@userid", UserID);
				paramList[1] = new SqlParameter("@bibid", BibID);
				paramList[2] = new SqlParameter("@vid", VID);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Delete_Item_From_All_User_Folders", paramList);

				// Return TRUE
				return true;

			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Adds a digital resource to a user folder, or edits an existing item </summary>
		/// <param name="UserID"> Primary key for this user in the database </param>
		/// <param name="FolderName"> Name of this user's folder </param>
		/// <param name="BibID"> Bibliographic identifier for this title / item group </param>
		/// <param name="VID"> Volume identifier for this one volume within a title / item group </param>
		/// <param name="ItemOrder"> Order for this item within the folder</param>
		/// <param name="UserNotes"> Notes from the user about this item </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'mySobek_Add_Item_To_User_Folder' stored procedure</remarks> 
		public static bool Add_Item_To_User_Folder(int UserID, string FolderName, string BibID, string VID, int ItemOrder, string UserNotes, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Add_Item_To_User_Folder", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[6];
				paramList[0] = new SqlParameter("@userid", UserID);
				paramList[1] = new SqlParameter("@foldername", FolderName);
				paramList[2] = new SqlParameter("@bibid", BibID);
				paramList[3] = new SqlParameter("@vid", VID);
				paramList[4] = new SqlParameter("@itemorder", ItemOrder);
				paramList[5] = new SqlParameter("@usernotes", UserNotes);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Add_Item_To_User_Folder", paramList);

				// Return TRUE
				return true;

			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Get a browse of all items in a user's folder </summary>
		/// <param name="UserID"> Primary key for this user in the database </param>
		/// <param name="FolderName"> Name of this user's folder </param>
		/// <param name="ResultsPerPage"> Number of results to return per "page" of results </param>
		/// <param name="ResultsPage"> Which page of results to return ( one-based, so the first page is page number of one )</param>
		/// <param name="Include_Facets"> Flag indicates if facets should be included in the final result set</param>
		/// <param name="Facet_Types"> Primary key for the metadata types to include as facets (up to eight)</param>
		/// <param name="Return_Search_Statistics"> Flag indicates whether to create and return statistics about the overall search results, generally set to TRUE for the first page requested and subsequently set to FALSE </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> List of items matching search </returns>
		/// <remarks> This calls the 'mySobek_Get_User_Folder_Browse' stored procedure</remarks> 
		public static Single_Paged_Results_Args Get_User_Folder_Browse(int UserID, string FolderName, int ResultsPerPage, int ResultsPage, bool Include_Facets, List<short> Facet_Types, bool Return_Search_Statistics, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_User_Folder_Browse", String.Empty);
			}

			Single_Paged_Results_Args returnArgs;

			// Create the connection
			using (SqlConnection connect = new SqlConnection(connectionString + "Connection Timeout=45"))
			{

				// Create the command 
				SqlCommand executeCommand = new SqlCommand("mySobek_Get_User_Folder_Browse", connect)
												{CommandTimeout = 45, CommandType = CommandType.StoredProcedure};

				executeCommand.Parameters.AddWithValue("@userid", UserID);
				executeCommand.Parameters.AddWithValue("@foldername", FolderName);
				executeCommand.Parameters.AddWithValue("@pagesize", ResultsPerPage);
				executeCommand.Parameters.AddWithValue("@pagenumber", ResultsPage);
				executeCommand.Parameters.AddWithValue("@include_facets", Include_Facets);
				if ((Include_Facets) && (Facet_Types != null))
				{
					if (Facet_Types.Count > 0)
						executeCommand.Parameters.AddWithValue("@facettype1", Facet_Types[0]);
					else
						executeCommand.Parameters.AddWithValue("@facettype1", -1);
					if (Facet_Types.Count > 1)
						executeCommand.Parameters.AddWithValue("@facettype2", Facet_Types[1]);
					else
						executeCommand.Parameters.AddWithValue("@facettype2", -1);
					if (Facet_Types.Count > 2)
						executeCommand.Parameters.AddWithValue("@facettype3", Facet_Types[2]);
					else
						executeCommand.Parameters.AddWithValue("@facettype3", -1);
					if (Facet_Types.Count > 3)
						executeCommand.Parameters.AddWithValue("@facettype4", Facet_Types[3]);
					else
						executeCommand.Parameters.AddWithValue("@facettype4", -1);
					if (Facet_Types.Count > 4)
						executeCommand.Parameters.AddWithValue("@facettype5", Facet_Types[4]);
					else
						executeCommand.Parameters.AddWithValue("@facettype5", -1);
					if (Facet_Types.Count > 5)
						executeCommand.Parameters.AddWithValue("@facettype6", Facet_Types[5]);
					else
						executeCommand.Parameters.AddWithValue("@facettype6", -1);
					if (Facet_Types.Count > 6)
						executeCommand.Parameters.AddWithValue("@facettype7", Facet_Types[6]);
					else
						executeCommand.Parameters.AddWithValue("@facettype7", -1);
					if (Facet_Types.Count > 7)
						executeCommand.Parameters.AddWithValue("@facettype8", Facet_Types[7]);
					else
						executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}
				else
				{
					executeCommand.Parameters.AddWithValue("@facettype1", -1);
					executeCommand.Parameters.AddWithValue("@facettype2", -1);
					executeCommand.Parameters.AddWithValue("@facettype3", -1);
					executeCommand.Parameters.AddWithValue("@facettype4", -1);
					executeCommand.Parameters.AddWithValue("@facettype5", -1);
					executeCommand.Parameters.AddWithValue("@facettype6", -1);
					executeCommand.Parameters.AddWithValue("@facettype7", -1);
					executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}

				// Add parameters for total items and total titles
				SqlParameter totalItemsParameter = executeCommand.Parameters.AddWithValue("@total_items", 0);
				totalItemsParameter.Direction = ParameterDirection.InputOutput;

				SqlParameter totalTitlesParameter = executeCommand.Parameters.AddWithValue("@total_titles", 0);
				totalTitlesParameter.Direction = ParameterDirection.InputOutput;


				// Create the data reader
				connect.Open();
				using (SqlDataReader reader = executeCommand.ExecuteReader())
				{

					// Create the return argument object
					returnArgs = new Single_Paged_Results_Args
									 {Paged_Results = DataReader_To_Simple_Result_List(reader)};

					// Create the overall search statistics?
					if (Return_Search_Statistics)
					{
						Search_Results_Statistics stats = new Search_Results_Statistics(reader, Facet_Types);
						returnArgs.Statistics = stats;
						reader.Close();
						stats.Total_Items = Convert.ToInt32(totalItemsParameter.Value);
						stats.Total_Titles = Convert.ToInt32(totalTitlesParameter.Value);
					}
					else
					{
						reader.Close();
					}
				}
				connect.Close();
			}

			// Return the built result arguments
			return returnArgs;
		}


		/// <summary> Get a browse of all items in a user's public folder </summary>
		/// <param name="UserFolderID"> Primary key for this user's folder which should be public in the database </param>
		/// <param name="ResultsPerPage"> Number of results to return per "page" of results </param>
		/// <param name="ResultsPage"> Which page of results to return ( one-based, so the first page is page number of one )</param>
		/// <param name="Include_Facets"> Flag indicates if facets should be included in the final result set</param>
		/// <param name="Facet_Types"> Primary key for the metadata types to include as facets (up to eight)</param>
		/// <param name="Return_Search_Statistics"> Flag indicates whether to create and return statistics about the overall search results, generally set to TRUE for the first page requested and subsequently set to FALSE </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> List of items matching search </returns>
		/// <remarks> This calls the 'mySobek_Get_User_Folder_Browse' stored procedure</remarks> 
		public static Single_Paged_Results_Args Get_Public_Folder_Browse(int UserFolderID, int ResultsPerPage, int ResultsPage, bool Include_Facets, List<short> Facet_Types, bool Return_Search_Statistics, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Public_Folder_Browse", String.Empty);
			}

			Single_Paged_Results_Args returnArgs;

			// Create the connection
			using (SqlConnection connect = new SqlConnection(connectionString + "Connection Timeout=45"))
			{
				// Create the command 
				SqlCommand executeCommand = new SqlCommand("mySobek_Get_Public_Folder_Browse", connect)
												{CommandTimeout = 45, CommandType = CommandType.StoredProcedure};

				executeCommand.Parameters.AddWithValue("@folderid", UserFolderID);
				executeCommand.Parameters.AddWithValue("@pagesize", ResultsPerPage);
				executeCommand.Parameters.AddWithValue("@pagenumber", ResultsPage);
				executeCommand.Parameters.AddWithValue("@include_facets", Include_Facets);
				if ((Include_Facets) && (Facet_Types != null))
				{
					if (Facet_Types.Count > 0)
						executeCommand.Parameters.AddWithValue("@facettype1", Facet_Types[0]);
					else
						executeCommand.Parameters.AddWithValue("@facettype1", -1);
					if (Facet_Types.Count > 1)
						executeCommand.Parameters.AddWithValue("@facettype2", Facet_Types[1]);
					else
						executeCommand.Parameters.AddWithValue("@facettype2", -1);
					if (Facet_Types.Count > 2)
						executeCommand.Parameters.AddWithValue("@facettype3", Facet_Types[2]);
					else
						executeCommand.Parameters.AddWithValue("@facettype3", -1);
					if (Facet_Types.Count > 3)
						executeCommand.Parameters.AddWithValue("@facettype4", Facet_Types[3]);
					else
						executeCommand.Parameters.AddWithValue("@facettype4", -1);
					if (Facet_Types.Count > 4)
						executeCommand.Parameters.AddWithValue("@facettype5", Facet_Types[4]);
					else
						executeCommand.Parameters.AddWithValue("@facettype5", -1);
					if (Facet_Types.Count > 5)
						executeCommand.Parameters.AddWithValue("@facettype6", Facet_Types[5]);
					else
						executeCommand.Parameters.AddWithValue("@facettype6", -1);
					if (Facet_Types.Count > 6)
						executeCommand.Parameters.AddWithValue("@facettype7", Facet_Types[6]);
					else
						executeCommand.Parameters.AddWithValue("@facettype7", -1);
					if (Facet_Types.Count > 7)
						executeCommand.Parameters.AddWithValue("@facettype8", Facet_Types[7]);
					else
						executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}
				else
				{
					executeCommand.Parameters.AddWithValue("@facettype1", -1);
					executeCommand.Parameters.AddWithValue("@facettype2", -1);
					executeCommand.Parameters.AddWithValue("@facettype3", -1);
					executeCommand.Parameters.AddWithValue("@facettype4", -1);
					executeCommand.Parameters.AddWithValue("@facettype5", -1);
					executeCommand.Parameters.AddWithValue("@facettype6", -1);
					executeCommand.Parameters.AddWithValue("@facettype7", -1);
					executeCommand.Parameters.AddWithValue("@facettype8", -1);
				}

				// Add parameters for total items and total titles
				SqlParameter totalItemsParameter = executeCommand.Parameters.AddWithValue("@total_items", 0);
				totalItemsParameter.Direction = ParameterDirection.InputOutput;

				SqlParameter totalTitlesParameter = executeCommand.Parameters.AddWithValue("@total_titles", 0);
				totalTitlesParameter.Direction = ParameterDirection.InputOutput;


				// Create the data reader
				connect.Open();
				using (SqlDataReader reader = executeCommand.ExecuteReader())
				{

					// Create the return argument object
					returnArgs = new Single_Paged_Results_Args
									 {Paged_Results = DataReader_To_Simple_Result_List(reader)};

					// Create the overall search statistics?
					if (Return_Search_Statistics)
					{
						Search_Results_Statistics stats = new Search_Results_Statistics(reader, Facet_Types);
						returnArgs.Statistics = stats;
						reader.Close();
						stats.Total_Items = Convert.ToInt32(totalItemsParameter.Value);
						stats.Total_Titles = Convert.ToInt32(totalTitlesParameter.Value);
					}
					else
					{
						reader.Close();
					}
				}
				connect.Close();
			}

			// Return the built result arguments
			return returnArgs;
		}

		/// <summary> Deletes a folder from a user </summary>
		/// <param name="UserID"> Primary key for this user from the database</param>
		/// <param name="UserFolderID"> Primary key for this folder from the database</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'mySobek_Delete_User_Folder' stored procedure</remarks> 
		public static bool Delete_User_Folder(int UserID, int UserFolderID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Delete_User_Folder", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@userfolderid", UserFolderID);
				paramList[1] = new SqlParameter("@userid", UserID);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Delete_User_Folder", paramList);

				// Return TRUE
				return true;

			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Edit an existing user folder, or add a new user folder </summary>
		/// <param name="UserFolderID"> Primary key for the folder, if this is an edit, otherwise -1</param>
		/// <param name="UserID"> Primary key for this user from the database</param>
		/// <param name="ParentFolderID"> Key for the parent folder for this new folder</param>
		/// <param name="FolderName"> Name for this new folder</param>
		/// <param name="isPublic"> Flag indicates if this folder is public </param>
		/// <param name="Description"> Description for this folder </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Primary key for this new folder, or -1 if an error occurred </returns>
		/// <remarks> This calls the 'mySobek_Edit_User_Folder' stored procedure</remarks> 
		public static int Edit_User_Folder(int UserFolderID, int UserID, int ParentFolderID, string FolderName, bool isPublic, string Description, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Edit_User_Folder", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[7];
				paramList[0] = new SqlParameter("@userfolderid", UserFolderID);
				paramList[1] = new SqlParameter("@userid", UserID);
				paramList[2] = new SqlParameter("@parentfolderid", ParentFolderID);
				paramList[3] = new SqlParameter("@foldername", FolderName);
				paramList[4] = new SqlParameter("@is_public", isPublic);
				paramList[5] = new SqlParameter("@description", Description);
				paramList[6] = new SqlParameter("@new_folder_id", 0) {Direction = ParameterDirection.InputOutput};

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Edit_User_Folder", paramList);

				// Return TRUE
				return Convert.ToInt32(paramList[6].Value);

			}
			catch (Exception ee)
			{
				lastException = ee;
				return -1;
			}
		}

		/// <summary> Sets the flag indicating an aggregation should appear on a user's home page </summary>
		/// <param name="UserID"> Primary key for the user</param>
		/// <param name="AggregationID"> Primary key for the aggregation </param>
		/// <param name="NewFlag"> New flag indicates if this should be on the home page </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'mySobek_Set_Aggregation_Home_Page_Flag' stored procedure</remarks> 
		public static bool User_Set_Aggregation_Home_Page_Flag(int UserID, int AggregationID, bool NewFlag, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.User_Set_Aggregation_Home_Page_Flag", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[3];
				paramList[0] = new SqlParameter("@userid", UserID);
				paramList[1] = new SqlParameter("@aggregationid", AggregationID);
				paramList[2] = new SqlParameter("@onhomepage", NewFlag);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Set_Aggregation_Home_Page_Flag", paramList);

				// Return TRUE
				return true;

			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Gets the information about a folder which should be public </summary>
		/// <param name="UserFolderID"> ID for the user folder to retrieve </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Built public user folder regardless if it is public or not.  A non-public folder will only be populated with FALSE for the isPublic field. </returns>
		/// <remarks> This calls the 'mySobek_Get_Folder_Information' stored procedure</remarks> 
		public static Public_User_Folder Get_Public_User_Folder(int UserFolderID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Public_User_Folder", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@folderid", UserFolderID);

				DataSet resultSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "mySobek_Get_Folder_Information", paramList);

				// Build the returnvalue
				if ((resultSet == null) || (resultSet.Tables.Count == 0) || (resultSet.Tables[0].Rows.Count == 0))
					return new Public_User_Folder(false);

				// Check that it is really public
				bool isPublic = Convert.ToBoolean(resultSet.Tables[0].Rows[0]["isPublic"]);
				if ( !isPublic )
					return new Public_User_Folder(false);

				// Pull out the row and all the values
				DataRow thisRow = resultSet.Tables[0].Rows[0];
				string folderName = thisRow["FolderName"].ToString();
				string folderDescription = thisRow["FolderDescription"].ToString();
				int userID = Convert.ToInt32(thisRow["UserID"]);
				string firstName = thisRow["FirstName"].ToString();
				string lastName = thisRow["LastName"].ToString();
				string nickname = thisRow["NickName"].ToString();
				string email = thisRow["EmailAddress"].ToString();               

				// Return the folder object
				Public_User_Folder returnValue = new Public_User_Folder(UserFolderID, folderName, folderDescription, userID, firstName, lastName, nickname, email, true);
				return returnValue;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}


		/// <summary> Gets the information about a single user group </summary>
		/// <param name="UserGroupID"> Primary key for this user group from the database </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Fully built <see cref="Users.User_Group"/> object </returns>
		/// <remarks> This calls the 'mySobek_Get_User_Group' stored procedure </remarks> 
		public static User_Group Get_User_Group(int UserGroupID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_User_Group", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@usergroupid", UserGroupID);

				DataSet resultSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "mySobek_Get_User_Group", paramList);

				if ((resultSet.Tables.Count > 0) && (resultSet.Tables[0].Rows.Count > 0))
				{


					DataRow userRow = resultSet.Tables[0].Rows[0];
					string name = userRow["GroupName"].ToString();
					string description = userRow["GroupDescription"].ToString();
					int usergroupid = Convert.ToInt32(userRow["UserGroupID"]);
					User_Group group = new User_Group(name, description, usergroupid);
					group.Can_Submit = Convert.ToBoolean(userRow["Can_Submit_Items"]);
					group.Is_Internal_User = Convert.ToBoolean(userRow["Internal_User"]);
					group.Is_System_Admin = Convert.ToBoolean(userRow["IsSystemAdmin"]);

					foreach (DataRow thisRow in resultSet.Tables[1].Rows)
					{
						group.Add_Template(thisRow["TemplateCode"].ToString());
					}

					foreach (DataRow thisRow in resultSet.Tables[2].Rows)
					{
						group.Add_Project(thisRow["ProjectCode"].ToString());
					}

					// Add links to regular expressions
					foreach (DataRow thisRow in resultSet.Tables[3].Rows)
					{
						group.Add_Editable_Regular_Expression(thisRow["EditableRegex"].ToString());
					}

					// Add links to aggregations
					foreach (DataRow thisRow in resultSet.Tables[4].Rows)
					{
						group.Add_Aggregation(thisRow["Code"].ToString(), thisRow["Name"].ToString(), Convert.ToBoolean(thisRow["CanSelect"]), Convert.ToBoolean(thisRow["CanEditItems"]), Convert.ToBoolean(thisRow["IsCurator"]));
					}

					// Add the basic information about users in this user group
					foreach (DataRow thisRow in resultSet.Tables[5].Rows)
					{
						int userid = Convert.ToInt32(thisRow["UserID"]);
						string username = thisRow["UserName"].ToString();
						string email = thisRow["EmailAddress"].ToString();
						string firstname = thisRow["FirstName"].ToString();
						string nickname = thisRow["NickName"].ToString();
						string lastname = thisRow["LastName"].ToString();
						string fullname = firstname + " " + lastname;
						if (nickname.Length > 0)
						{
							fullname = nickname + " " + lastname;
						}

						group.Add_User(username, fullname, email, userid);
					}

					return group;
				}

				// Return NULL if there was an error
				return null;

			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		#endregion

		#region Methods used in support of semi-public descriptive tagging

		/// <summary> Adds a descriptive tag to an existing item by a logged-in user </summary>
		/// <param name="UserID"> Primary key for the user adding the descriptive tag </param>
		/// <param name="TagID"> Primary key for a descriptive tag, if this is an edit </param>
		/// <param name="ItemID"> Primary key for the digital resource to tag </param>
		/// <param name="Added_Description"> User-entered descriptive tag </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> New tag id if this is a new descriptive tag </returns>
		/// <remarks> This calls the 'mySobek_Add_Description_Tag' stored procedure</remarks> 
		public static int Add_Description_Tag(int UserID, int TagID, int ItemID, string Added_Description, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Add_Description_Tag", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[5];
				paramList[0] = new SqlParameter("@UserID", UserID);
				paramList[1] = new SqlParameter("@TagID", TagID);
				paramList[2] = new SqlParameter("@ItemID", ItemID);
				paramList[3] = new SqlParameter("@Description ", Added_Description);
				paramList[4] = new SqlParameter("@new_TagID", -1) {Direction = ParameterDirection.InputOutput};

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Add_Description_Tag", paramList);

				// Return TRUE
				int returnValue = Convert.ToInt32(paramList[4].Value);
				return returnValue;

			}
			catch (Exception ee)
			{
				lastException = ee;
				return -1;
			}
		}

		/// <summary> Delete's a user's descriptive tage </summary>
		/// <param name="TagID"> Primary key for the entered the descriptive tag </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successul, otherwise FALSE </returns>
		/// <remarks> This calls the 'mySobek_Delete_Description_Tag' stored procedure</remarks> 
		public static bool Delete_Description_Tag(int TagID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Delete_Description_Tag", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@TagID", TagID);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Delete_Description_Tag", paramList);

				// Return TRUE
				return true;

			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> List all descriptive tags added by a single user </summary>
		/// <param name="UserID"> Primary key for the user that entered the descriptive tags (or -1 to get ALL tags)</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataTable with all of the user's descriptive tags </returns>
		/// <remarks> This calls the 'mySobek_View_All_User_Tags' stored procedure</remarks> 
		public static DataTable View_Tags_By_User(int UserID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.View_Tags_By_User", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@UserID", UserID);

				DataSet resultSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "mySobek_View_All_User_Tags", paramList);

				return resultSet.Tables[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}


		/// <summary> List all descriptive tags added by a single user </summary>
		/// <param name="AggregationCode"> Aggregation code for which to pull all descriptive tags added </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataTable with all of the descriptive tags added to items within the aggregation of interest </returns>
		/// <remarks> This calls the 'SobekCM_Get_Description_Tags_By_Aggregation' stored procedure  </remarks> 
		public static DataTable View_Tags_By_Aggregation( string AggregationCode, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.View_Tags_By_Aggregation", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@aggregationcode", AggregationCode);

				DataSet resultSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Description_Tags_By_Aggregation", paramList);

				return resultSet.Tables[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}


		#endregion

		#region Properties used by the SobekCM Builder (moved from the bib package)

		/// <summary> Gets the script from the database used for collection building </summary>
		/// <remarks> This calls the 'SobekCM_Get_ColBuild_Script' stored procedure </remarks> 
		public static DataTable CollectionBuild_Script
		{
			get
			{
				try
				{
					DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_ColBuild_Script");
					return tempSet.Tables[0];
				}
				catch (Exception ee)
				{
					lastException = ee;
					return null;
				}
			}
		}

		/// <summary> Gets all the data necessary for the Builder, including file destination information,
		/// general settings, server information, and the list of each BibID and File_Root </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataSet with all the data necessary for the Builder, including file destination information,
		/// general settings, server information, and the list of each BibID and File_Root</returns>
		/// <remarks> This calls the 'SobekCM_Get_Builder_Settings' stored procedure </remarks> 
		public static DataSet Get_Builder_Settings_Complete(Custom_Tracer tracer)
		{
			try
			{
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@include_items", true);
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Builder_Settings", paramList);
				return tempSet;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Gets all the data necessary for the Builder, including file destination information,
		/// general settings, server information, and the list of each BibID and File_Root </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataSet with all the data necessary for the Builder, including file destination information,
		/// general settings, server information, and the list of each BibID and File_Root</returns>
		/// <remarks> This calls the 'SobekCM_Get_Settings' stored procedure </remarks> 
		public static DataSet Get_Settings_Complete(Custom_Tracer tracer)
		{
			try
			{
				SqlParameter[] paramList = new SqlParameter[1];
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Settings", paramList);
				return tempSet;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Gets the values from the builder settings table in the database </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Dictionary of all the keys and values in the builder settings table </returns>
		/// <remarks> This calls the 'SobekCM_Get_Settings' stored procedure </remarks> 
		public static Dictionary<string,string> Get_Settings(Custom_Tracer tracer)
		{
			Dictionary<string, string> returnValue = new Dictionary<string, string>();

			try
			{
				SqlParameter[] paramList = new SqlParameter[1];
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Settings", paramList);
				if ((tempSet.Tables.Count > 0) && (tempSet.Tables[0].Rows.Count > 0))
				{
					foreach (DataRow thisRow in tempSet.Tables[0].Rows)
					{
						returnValue[thisRow["Setting_Key"].ToString()] = thisRow["Setting_Value"].ToString();
					}
				}
			}
			catch (Exception ee)
			{
				lastException = ee;
			}

			return returnValue;
		}

		/// <summary> Sets a value in the settings table </summary>
		/// <param name="Setting_Key"> Key for the setting to update or insert </param>
		/// <param name="Setting_Value"> Value for the setting to update or insert </param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Set_Setting_Value' stored procedure </remarks> 
		public static bool Set_Setting(string Setting_Key, string Setting_Value)
		{
			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@Setting_Key", Setting_Key);
				paramList[1] = new SqlParameter("@Setting_Value", Setting_Value);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Set_Setting_Value", paramList);
				return true;
			}
			catch ( Exception ee )
			{
				lastException = ee;
				return false;
			}
		}

		#endregion

		#region Methods used by the SobekCM Builder  (moved from bib package)

		/// <summary> Gets list of item groups (BibID's) for inclusion in the production MarcXML load </summary>
		/// <value> Datatable with the list of records </value>
		/// <remarks> This calls the 'SobekCM_MarcXML_Production_Feed' stored procedure </remarks>
		public static DataTable MarcXML_Production_Feed_Records
		{
			get
			{
                lastException = null;
				try
				{
					// Define a temporary dataset
					DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_MarcXML_Production_Feed");

					// Return the first table from the returned dataset
					return tempSet.Tables[0];
				}
				catch (Exception ee)
				{
					lastException = ee;
					return null;
				}
			}
		}

		/// <summary> Gets list of item groups (BibID's) for inclusion in the test MarcXML load </summary>
		/// <value> Datatable with the list of records </value>
		/// <remarks> This calls the 'SobekCM_MarcXML_Test_Feed' stored procedure </remarks>
		public static DataTable MarcXML_Test_Feed_Records
		{
    		get
			{
                lastException = null;
				try
				{
					// Define a temporary dataset
					DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_MarcXML_Test_Feed");

					// Return the first table from the returned dataset
					return tempSet.Tables[0];
				}
				catch (Exception ee)
				{
					lastException = ee;
					return null;
				}
			}
		}

		/// <summary>method used to set the new items flag of a specified item aggregation</summary>
		/// <param name="AggregationCode">Code for this SobekCM item aggregation</param>
		/// <param name="newItemFlag">Status for the new item flag</param>
		/// <returns>TRUE if successful, otherwise FALSE</returns>
		/// <remarks> This method calls the stored procedure 'SobekCM_Set_Aggregation_NewItem_Flag'. </remarks>
		public static bool Set_Aggregation_NewItem_Flag(string AggregationCode, bool newItemFlag)
		{
			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@code", AggregationCode);
				paramList[1] = new SqlParameter("@newitemflag", newItemFlag);
				// Execute this non-query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Set_Aggregation_NewItem_Flag", paramList);
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}


		/// <summary> Get a list of collections which have new items and are ready to be built</summary>
		/// <returns> DataTable of collections waiting to be built</returns>
		/// <remarks> This method calls the stored procedure 'SobekCM_Get_CollectionList_toBuild'. </remarks>
		public static DataTable Get_CollectionList_ReadyToBuild()
		{
			try
			{
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_CollectionList_toBuild");
				return tempSet.Tables[0];
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}


		/// <summary> Deletes an item from the SobekCM database</summary>
		/// <param name="BibID"> Bibliographic identifier for the item to delete</param>
		/// <param name="VID"> Volume identifier for the item to delete</param>
		/// <param name="As_Admin"> Indicates this is an admin, who can delete ANY item, not just those without archives attached </param>
		/// <param name="Delete_Message"> Message to include on any archive remnants after an admin delete </param>
		/// <returns> TRUE if successful, otherwise FALSE</returns>
		/// <remarks> This method calls the stored procedure 'SobekCM_Delete_Item'. <br /><br />
		/// This just marks a flag on the item (and item group) as deleted, it does not actually remove the data from the database</remarks>
		public static bool Delete_SobekCM_Item(string BibID, string VID, bool As_Admin, string Delete_Message )
		{
			try
			{
				// build the parameter list
				SqlParameter[] paramList = new SqlParameter[4];
				paramList[0] = new SqlParameter("@bibid", BibID);
				paramList[1] = new SqlParameter("@vid", VID);
				paramList[2] = new SqlParameter("@as_admin", As_Admin);
				paramList[3] = new SqlParameter("@delete_message", Delete_Message);

				//Execute this non-query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Delete_Item", paramList);
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		#endregion

		#region Methods used to mark an item as needing additional work in the builder and pulling that list for the builder

		/// <summary> Gets the list of all items currently flagged for needing additional work </summary>
		/// <remarks> This calls the 'SobekCM_Get_Items_Needing_Aditional_Work' stored procedure. </remarks>
		public static DataTable Items_Needing_Aditional_Work
		{
			get
			{
				try
				{
					DataSet returnSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Items_Needing_Aditional_Work");
					return returnSet.Tables[0];
				}
				catch
				{
					return null;
				}
			}
		}

		/// <summary> Update the additional work neeed flag, which flag an item for additional follow up work in the builder </summary>
		/// <param name="ItemID"> Primary key for the item for which to update the additional work needed flag</param>
		/// <param name="New_Flag"> New flag for the additional follow up work </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successul, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Update_Additional_Work_Needed_Flag' stored procedure</remarks> 
		public static bool Update_Additional_Work_Needed_Flag(int ItemID, bool New_Flag, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Update_Additional_Work_Needed_Flag", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@itemid", ItemID);
				paramList[1] = new SqlParameter("@newflag", New_Flag);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Update_Additional_Work_Needed_Flag", paramList);

				// Return TRUE
				return true;

			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		#endregion

		#region Methods used for SobekCM Administrative Tasks (moved from SobekCM Manager )

		#region Methods related to the Thematic Heading values

		/// <summary> Saves a new thematic heading or updates an existing thematic heading </summary>
		/// <param name="ThematicHeadingID"> Primary key for the existing thematic heading, or -1 for a new heading </param>
		/// <param name="ThemeOrder"> Order of this thematic heading, within the rest of the headings </param>
		/// <param name="ThemeName"> Display name for this thematic heading</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Thematic heading id, or -1 if there was an error </returns>
		/// <remarks> This calls the 'SobekCM_Edit_Thematic_Heading' stored procedure </remarks> 
		public static int Edit_Thematic_Heading( int ThematicHeadingID, int ThemeOrder, string ThemeName, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Edit_Thematic_Heading", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[4];
				paramList[0] = new SqlParameter("@thematicheadingid", ThematicHeadingID);
				paramList[1] = new SqlParameter("@themeorder", ThemeOrder);
				paramList[2] = new SqlParameter("@themename", ThemeName);
				paramList[3] = new SqlParameter("@newid", -1) {Direction = ParameterDirection.Output};

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Edit_Thematic_Heading", paramList);

				return Convert.ToInt32(paramList[3].Value);
			}
			catch (Exception ee)
			{
				lastException = ee;
				return -1;
			}
		}

		/// <summary> Deletes a thematic heading from the database  </summary>
		/// <param name="ThematicHeadingID"> Primary key for the thematic heading to delete </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE</returns>
		/// <remarks> This calls the 'SobekCM_Delete_Thematic_Heading' stored procedure </remarks> 
		public static bool Delete_Thematic_Heading( int ThematicHeadingID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Delete_Thematic_Heading", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@thematicheadingid", ThematicHeadingID);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Delete_Thematic_Heading", paramList);
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		#endregion

		#region Methods related to the URL Portals into this library

		///// <summary> Saves a new thematic heading or updates an existing thematic heading </summary>
		///// <param name="ThematicHeadingID"> Primary key for the existing thematic heading, or -1 for a new heading </param>
		///// <param name="ThemeOrder"> Order of this thematic heading, within the rest of the headings </param>
		///// <param name="ThemeName"> Display name for this thematic heading</param>
		///// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		///// <returns> Thematic heading id, or -1 if there was an error </returns>
		///// <remarks> This calls the 'SobekCM_Edit_Thematic_Heading' stored procedure </remarks> 
		//public static int Edit_Thematic_Heading(int ThematicHeadingID, string ThemeOrder, string ThemeName, Custom_Tracer tracer)
		//{
		//    if (tracer != null)
		//    {
		//        tracer.Add_Trace("SobekCM_Database.Edit_Thematic_Heading", String.Empty);
		//    }

		//    try
		//    {
		//        // Execute this non-query stored procedure
		//        SqlParameter[] param_list = new SqlParameter[4];
		//        param_list[0] = new SqlParameter("@thematicheadingid", ThematicHeadingID);
		//        param_list[1] = new SqlParameter("@themeorder", ThemeOrder);
		//        param_list[2] = new SqlParameter("@themename", ThemeName);
		//        param_list[3] = new SqlParameter("@newid", -1);
		//        param_list[3].Direction = ParameterDirection.Output;

		//        SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Edit_Thematic_Heading", param_list);

		//        return Convert.ToInt32(param_list[3].Value);
		//    }
		//    catch (Exception ee)
		//    {
		//        last_exception = ee;
		//        return -1;
		//    }
		//}

		///// <summary> Deletes a thematic heading from the database  </summary>
		///// <param name="ThematicHeadingID"> Primary key for the thematic heading to delete </param>
		///// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		///// <returns> TRUE if successful, otherwise FALSE</returns>
		///// <remarks> This calls the 'SobekCM_Delete_Thematic_Heading' stored procedure </remarks> 
		//public static bool Delete_Thematic_Heading(int ThematicHeadingID, Custom_Tracer tracer)
		//{
		//    if (tracer != null)
		//    {
		//        tracer.Add_Trace("SobekCM_Database.Delete_Thematic_Heading", String.Empty);
		//    }

		//    try
		//    {
		//        // Execute this non-query stored procedure
		//        SqlParameter[] param_list = new SqlParameter[1];
		//        param_list[0] = new SqlParameter("@thematicheadingid", ThematicHeadingID);

		//        SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Delete_Thematic_Heading", param_list);
		//        return true;
		//    }
		//    catch (Exception ee)
		//    {
		//        last_exception = ee;
		//        return false;
		//    }
		//}

		#endregion

		/// <summary> Saves a item aggregation alias for future use </summary>
		/// <param name="Alias"> Alias string which will forward to a item aggregation </param>
		/// <param name="Aggregation_Code"> Code for the item aggregation to forward to </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE</returns>
		/// <remarks> This calls the 'SobekCM_Save_Item_Aggregation_Alias' stored procedure </remarks> 
		public static bool Save_Aggregation_Alias(string Alias, string Aggregation_Code, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Save_Aggregation_Alias", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@alias", Alias);
				paramList[1] = new SqlParameter("@aggregation_code", Aggregation_Code);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Save_Item_Aggregation_Alias", paramList);
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Deletes an item aggregation alias by alias code </summary>
		/// <param name="Alias"> Alias string which forwarded to a item aggregation </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE</returns>
		/// <remarks> This calls the 'SobekCM_Delete_Item_Aggregation_Alias' stored procedure </remarks> 
		public static bool Delete_Aggregation_Alias(string Alias, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Delete_Aggregation_Alias", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@alias", Alias);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Delete_Item_Aggregation_Alias", paramList);
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Saves a HTML skin to the database </summary>
		/// <param name="Skin_Code"> Code for this HTML skin </param>
		/// <param name="Base_Skin_Code"> Base skin code from which this html skin inherits </param>
		/// <param name="overrideBanner"> Flag indicates this skin overrides the default banner </param>
		/// <param name="overrideHeaderFooter"> Flag indicates this skin overrides the default header/footer</param>
		/// <param name="Banner_Link"> Link to which the banner sends the user </param>
		/// <param name="Notes"> Notes on this skin ( name, use, etc...) </param>
		/// <param name="Build_On_Launch"> Flag indicates if this skin should be built upon launch ( i.e., is this a heavily used web skin? )</param>
		/// <param name="Suppress_Top_Navigation"> Flag indicates if the top-level aggregation navigation should be suppressed for this web skin ( i.e., is the top-level navigation embedded into the header file already? )</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Add_Web_Skin' stored procedure </remarks> 
		public static bool Save_Web_Skin(string Skin_Code, string Base_Skin_Code, bool overrideBanner, bool overrideHeaderFooter, string Banner_Link, string Notes, bool Build_On_Launch, bool Suppress_Top_Navigation, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Save_Skin", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[8];
				paramList[0] = new SqlParameter("@webskincode", Skin_Code);
				paramList[1] = new SqlParameter("@basewebskin", Base_Skin_Code);
				paramList[2] = new SqlParameter("@overridebanner", overrideBanner);
				paramList[3] = new SqlParameter("@overrideheaderfooter", overrideHeaderFooter);
				paramList[4] = new SqlParameter("@bannerlink", Banner_Link);
				paramList[5] = new SqlParameter("@notes", Notes);
				paramList[6] = new SqlParameter("@build_on_launch", Build_On_Launch);
				paramList[7] = new SqlParameter("@suppress_top_nav", Suppress_Top_Navigation  );

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Add_Web_Skin", paramList);
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Deletes a HTML web skin fromo the database </summary>
		/// <param name="Skin_Code"> Code for the  HTML web skin to delete </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Delete_Web_Skin' stored procedure </remarks> 
		public static bool Delete_Web_Skin(string Skin_Code, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Delete_Web_Skin", String.Empty);
			}

			try
			{
				// Execute this non-query stored procedure
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@webskincode", Skin_Code);

				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Delete_Web_Skin", paramList);
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Saves information about a new icon/wordmark or modify an existing one </summary>
		/// <param name="Icon_Name"> Code identifier for this icon/wordmark</param>
		/// <param name="Icon_File"> Filename for this icon/wordmark</param>
		/// <param name="Icon_Link">  Link that clicking on this icon/wordmark will forward the user to</param>
		/// <param name="Icon_Title"> Title for this icon, which appears when you hover over the icon </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Primary key for this new icon (or wordmark), or -1 if this action failed</returns>
		/// <remarks> This calls the 'SobekCM_Save_Icon' stored procedure </remarks> 
		public static int Save_Icon(string Icon_Name, string Icon_File, string Icon_Link, string Icon_Title, Custom_Tracer tracer)
		{
			return Save_Icon( Icon_Name, Icon_File, Icon_Link, 80, Icon_Title, tracer);
		}

		/// <summary> Saves information about a new icon/wordmark or modify an existing one </summary>
		/// <param name="Icon_Name"> Code identifier for this icon/wordmark</param>
		/// <param name="Icon_File"> Filename for this icon/wordmark</param>
		/// <param name="Icon_Link">  Link that clicking on this icon/wordmark will forward the user to</param>
		/// <param name="Height"> Height for this icon/wordmark </param>
		/// <param name="Icon_Title"> Title for this icon, which appears when you hover over the icon </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Primary key for this new icon (or wordmark), or -1 if this action failed</returns>
		/// <remarks> This calls the 'SobekCM_Save_Icon' stored procedure </remarks> 
		public static int Save_Icon(string Icon_Name, string Icon_File, string Icon_Link, int Height, string Icon_Title, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Save_Icon", String.Empty);
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[7];
				paramList[0] = new SqlParameter("@iconid", -1 );
				paramList[1] = new SqlParameter("@icon_name", Icon_Name);
				paramList[2] = new SqlParameter("@icon_url", Icon_File);
				paramList[3] = new SqlParameter("@link", Icon_Link);
				paramList[4] = new SqlParameter("@height", Height);
				paramList[5] = new SqlParameter("@title", Icon_Title);
				paramList[6] = new SqlParameter("@new_iconid", -1) {Direction = ParameterDirection.Output};

				// Execute this query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Save_Icon", paramList);

				// Return the new icon id
				return Convert.ToInt32(paramList[6].Value);
			}
			catch (Exception ee)
			{
				lastException = ee;
				return -1;
			}
		}

		/// <summary> Deletes an existing wordmark/icon if it is not linked to any titles in the database </summary>
		/// <param name="Icon_Code"> Wordmark/icon code for the wordmark to delete </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successfully deleted, otherwise FALSE indicating the icon is linked to some titles and cannot be deleted </returns>
		/// <remarks> This calls the 'SobekCM_Delete_Icon' stored procedure </remarks> 
		public static bool Delete_Icon( string Icon_Code, Custom_Tracer tracer )
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Delete_Icon", String.Empty);
			}

			try
			{
				// Clear the last exception first
				lastException = null;

				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@icon_code", Icon_Code);
				paramList[1] = new SqlParameter("@links", -1) {Direction = ParameterDirection.Output};

				// Execute this query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Delete_Icon", paramList);

				if (Convert.ToInt32(paramList[1].Value) > 0)
				{
					return false;
				}
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Gets the datatable of all item aggregation codes </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataTable with list of all item aggregations' code, type, name, and mapping to Greenstone </returns>
		/// <remarks> This calls the 'SobekCM_Get_Codes' stored procedure </remarks> 
		public static DataTable Get_Codes_Item_Aggregations(Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_Codes_Item_Aggregations", String.Empty);
			}

			// Define a temporary dataset
			DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_Codes");
			return tempSet.Tables[0];
		}
			   
		/// <summary> Gets the datatable of all users from the mySobek / personalization database </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataTable with list of all users' id, full name, and username </returns>
		/// <remarks> This calls the 'mySobek_Get_All_Users' stored procedure</remarks> 
		public static DataTable Get_All_Users(Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_All_Users", String.Empty);
			}

			// Define a temporary dataset
			DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "mySobek_Get_All_Users");
			return tempSet.Tables[0];
		}

		/// <summary> Gets the datatable of all user groups from the mySobek / personalization database </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataTable with list of all user groups' id, name, descriptiont, etc.. </returns>
		/// <remarks> This calls the 'mySobek_Get_All_User_Groups' stored procedure</remarks> 
		public static DataTable Get_All_User_Groups(Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_All_User_Groups", String.Empty);
			}

			// Define a temporary dataset
			DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "mySobek_Get_All_User_Groups");
			return tempSet.Tables[0];
		}


		/// <summary> Gets the dataset with all projects and all templates </summary>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> DataSet with list of all projects and tables </returns>
		/// <remarks> This calls the 'mySobek_Get_All_Projects_Templates' stored procedure</remarks> 
		public static DataSet Get_All_Projects_Templates(Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Get_All_Projects_Templates", String.Empty);
			}

			// Define a temporary dataset
			DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "mySobek_Get_All_Projects_Templates");
			return tempSet;
		}

		/// <summary> Updates an existing item aggregation's data that appears in the basic edit aggregation form </summary>
		/// <param name="Code"> Code for this item aggregation </param>
		/// <param name="Name"> Name for this item aggregation </param>
		/// <param name="ShortName"> Short version of this item aggregation </param>
		/// <param name="isActive"> Flag indicates if this item aggregation is active</param>
		/// <param name="isHidden"> Flag indicates if this item is hidden</param>
		/// <param name="External_Link">External link for this item aggregation (usually used for institutional aggregations)</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Update_Item_Aggregation' stored procedure in the SobekCM database</remarks> 
		public static bool Update_Item_Aggregation(string Code, string Name, string ShortName, bool isActive, bool isHidden, string External_Link, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Update_Item_Aggregation", String.Empty);
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[6];
				paramList[0] = new SqlParameter("@code", Code);
				paramList[1] = new SqlParameter("@name", Name);
				paramList[2] = new SqlParameter("@shortname", ShortName);
				paramList[3] = new SqlParameter("@isActive", isActive);
				paramList[4] = new SqlParameter("@hidden", isHidden);
				paramList[5] = new SqlParameter("@externallink", External_Link);

				// Execute this query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Update_Item_Aggregation", paramList);

				// Succesful, so return true
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Save a new item aggregation with the basic details provided in the new aggregation form </summary>
		/// <param name="Code"> Code for this item aggregation </param>
		/// <param name="Name"> Name for this item aggregation </param>
		/// <param name="ShortName"> Short version of this item aggregation </param>
		/// <param name="Description"> Description of this item aggregation </param>
		/// <param name="Type"> Type of item aggregation (i.e., Collection Group, Institution, Exhibit, etc..)</param>
		/// <param name="isActive"> Flag indicates if this item aggregation is active</param>
		/// <param name="isHidden"> Flag indicates if this item is hidden</param>
		/// <param name="ParentID"> ID for the item aggregation parent</param>
		/// <param name="ExternalLink">External link for this item aggregation (used primarily for institutional item aggregations to provide a link back to the institution's actual home page)</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Save_Item_Aggregation' stored procedure in the SobekCM database</remarks> 
		public static bool Save_Item_Aggregation( string Code, string Name, string ShortName, string Description, string Type, bool isActive, bool isHidden, string ExternalLink, int ParentID, Custom_Tracer tracer)
		{
			return Save_Item_Aggregation( -1, Code, Name, ShortName, Description, -1, Type, isActive, isHidden, String.Empty, 0, 0, false, String.Empty, String.Empty,  String.Empty, ExternalLink, ParentID, tracer);
		}

		/// <summary> Save a new item aggregation or edit an existing item aggregation in the database </summary>
		/// <param name="AggregationID"> AggregationID if this is editing an existing one, otherwise -1 </param>
		/// <param name="Code"> Code for this item aggregation </param>
		/// <param name="Name"> Name for this item aggregation </param>
		/// <param name="ShortName"> Short version of this item aggregation </param>
		/// <param name="Description"> Description of this item aggregation </param>
		/// <param name="ThematicHeadingID"> Thematic heading id for this item aggregation (or -1)</param>
		/// <param name="Type"> Type of item aggregation (i.e., Collection Group, Institution, Exhibit, etc..)</param>
		/// <param name="isActive"> Flag indicates if this item aggregation is active</param>
		/// <param name="isHidden"> Flag indicates if this item is hidden</param>
		/// <param name="DisplayOptions"> Display options for this item aggregation </param>
		/// <param name="Map_Search"> Map Search value indicates if there is a map search, and the type of search </param>
		/// <param name="Map_Display"> Map Display value indicates if there is a map display option when looking at search results or browses </param>
		/// <param name="OAI_Flag"> Flag indicates if this item aggregation should be available via OAI-PMH </param>
		/// <param name="OAI_Metadata"> Additional metadata about this collection, to be included in the set information in OAI-PMH</param>
		/// <param name="ContactEmail"> Contact email for this item aggregation (can leave blank to use default)</param>
		/// <param name="DefaultInterface"> Default interface for this item aggregation (particularly useful for institutional aggregations)</param>
		/// <param name="ExternalLink">External link for this item aggregation (used primarily for institutional item aggregations to provide a link back to the institution's actual home page)</param>
		/// <param name="ParentID"> ID for the item aggregation parent</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Save_Item_Aggregation' stored procedure in the SobekCM database</remarks> 
		public static bool Save_Item_Aggregation(int AggregationID, string Code, string Name, string ShortName, string Description, int ThematicHeadingID, string Type, bool isActive, bool isHidden, string DisplayOptions, int Map_Search, int Map_Display, bool OAI_Flag, string OAI_Metadata, string ContactEmail, string DefaultInterface, string ExternalLink, int ParentID, Custom_Tracer tracer )
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Save_Item_Aggregation", String.Empty);
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[19];
				paramList[0] = new SqlParameter("@aggregationid", AggregationID);
				paramList[1] = new SqlParameter("@code", Code);
				paramList[2] = new SqlParameter("@name", Name);
				paramList[3] = new SqlParameter("@shortname", ShortName);
				paramList[4] = new SqlParameter("@description", Description);
				paramList[5] = new SqlParameter("@thematicHeadingId", ThematicHeadingID);
				paramList[6] = new SqlParameter("@type", Type);
				paramList[7] = new SqlParameter("@isActive", isActive);
				paramList[8] = new SqlParameter("@hidden", isHidden);
				paramList[9] = new SqlParameter("@display_options", DisplayOptions);
				paramList[10] = new SqlParameter("@map_search", Map_Search);
				paramList[11] = new SqlParameter("@map_display", Map_Display);
				paramList[12] = new SqlParameter("@oai_flag", OAI_Flag);
				paramList[13] = new SqlParameter("@oai_metadata", OAI_Metadata);
				paramList[14] = new SqlParameter("@contactemail", ContactEmail);
				paramList[15] = new SqlParameter("@defaultinterface", DefaultInterface);
				paramList[16] = new SqlParameter("@externallink", ExternalLink);
				paramList[17] = new SqlParameter("@parentid", ParentID);
				paramList[18] = new SqlParameter("@newaggregationid", 0) {Direction = ParameterDirection.InputOutput};


				// Execute this query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Save_Item_Aggregation", paramList);

				// Succesful, so return true
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Sets a user's password to the newly provided one </summary>
		/// <param name="UserID"> Primary key for this user from the database </param>
		/// <param name="New_Password"> New password (unencrypted) to set for this user </param>
		/// <param name="Is_Temporary_Password"> Flag indicates if this is a temporary password that must be reset the first time the user logs on</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwsie FALSE  </returns>
		/// <remarks> This calls the 'mySobek_Reset_User_Password' stored procedure</remarks> 
		public static bool Reset_User_Password(int UserID, string New_Password, bool Is_Temporary_Password, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Reset_User_Password", String.Empty);
			}

			const string salt = "This is my salt to add to the password";
			string encryptedPassword = SecurityInfo.SHA1_EncryptString(New_Password + salt);

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[3];
				paramList[0] = new SqlParameter("@userid", UserID);
				paramList[1] = new SqlParameter("@password", encryptedPassword);
				paramList[2] = new SqlParameter("@is_temporary", Is_Temporary_Password);

				// Execute this query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Reset_User_Password", paramList);

				// Succesful, so return true
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Sets some of the permissions values for a single user </summary>
		/// <param name="UserID"> Primary key for this user from the database </param>
		/// <param name="Can_Submit"> Flag indicates if this user can submit items </param>
		/// <param name="Is_Internal"> Flag indicates if this user is considered an 'internal user'</param>
		/// <param name="Can_Edit_All"> Flag indicates if this user is authorized to edit all items in the library</param>
		/// <param name="Is_Portal_Admin"> Flag indicates if this user is a portal Administrator </param>
		/// <param name="Is_System_Admin"> Flag indicates if this user is a system Administrator</param>
		/// <param name="Include_Tracking_Standard_Forms"> Flag indicates if this user should have tracking portions appear in their standard forms </param>
		/// <param name="Edit_Template"> Template name for editing non-MARC records </param>
		/// <param name="Edit_Template_MARC"> Template name for editing MARC-derived records </param>
		/// <param name="Clear_Projects_Templates"> Flag indicates whether to clear projects and templates for this user </param>
		/// <param name="Clear_Aggregation_Links"> Flag indicates whether to clear item aggregations linked to this user</param>
		/// <param name="Clear_User_Groups"> Flag indicates whether to clear user group membership for this user </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'mySobek_Update_UFDC_User' stored procedure</remarks> 
		public static bool Update_SobekCM_User(int UserID, bool Can_Submit, bool Is_Internal, bool Can_Edit_All, bool Is_System_Admin, bool Is_Portal_Admin, bool Include_Tracking_Standard_Forms, string Edit_Template, string Edit_Template_MARC, bool Clear_Projects_Templates, bool Clear_Aggregation_Links, bool Clear_User_Groups, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Update_UFDC_User", String.Empty);
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[12];
				paramList[0] = new SqlParameter("@userid", UserID);
				paramList[1] = new SqlParameter("@can_submit", Can_Submit);
				paramList[2] = new SqlParameter("@is_internal", Is_Internal);
				paramList[3] = new SqlParameter("@can_edit_all", Can_Edit_All);
				paramList[4] = new SqlParameter("@is_portal_admin", Is_Portal_Admin);
				paramList[5] = new SqlParameter("@is_system_admin", Is_System_Admin);
				paramList[6] = new SqlParameter("@include_tracking_standard_forms", Include_Tracking_Standard_Forms);
				paramList[7] = new SqlParameter("@edit_template", Edit_Template);
				paramList[8] = new SqlParameter("@edit_template_marc", Edit_Template_MARC);
				paramList[9] = new SqlParameter("@clear_projects_templates", Clear_Projects_Templates);
				paramList[10] = new SqlParameter("@clear_aggregation_links", Clear_Aggregation_Links);
				paramList[11] = new SqlParameter("@clear_user_groups", Clear_User_Groups);

				// Execute this query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Update_UFDC_User", paramList);

				// Succesful, so return true
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Sets the list of templates possible for a given user </summary>
		/// <param name="UserID"> Primary key for this user from the database </param>
		/// <param name="Templates"> List of templates to link to this user </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'mySobek_Add_User_Templates_Link' stored procedure</remarks> 
		public static bool Update_SobekCM_User_Templates(int UserID, ReadOnlyCollection<string> Templates, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Update_SobekCM_User_Templates", String.Empty);
			}

			// Call the routine
			try
			{
				// Build the parameter list for the first run
				SqlParameter[] paramList = new SqlParameter[6];
				paramList[0] = new SqlParameter("@userid", UserID);

				if (Templates.Count > 0)
					paramList[1] = new SqlParameter("@template_default", Templates[0]);
				else
					paramList[1] = new SqlParameter("@template_default", String.Empty);

				if (Templates.Count > 1)
					paramList[2] = new SqlParameter("@template2", Templates[1]);
				else
					paramList[2] = new SqlParameter("@template2", String.Empty);

				if (Templates.Count > 2)
					paramList[3] = new SqlParameter("@template3", Templates[2]);
				else
					paramList[3] = new SqlParameter("@template3", String.Empty);

				if (Templates.Count > 3)
					paramList[4] = new SqlParameter("@template4", Templates[3]);
				else
					paramList[4] = new SqlParameter("@template4", String.Empty);

				if (Templates.Count > 4)
					paramList[5] = new SqlParameter("@template5", Templates[4]);
				else
					paramList[5] = new SqlParameter("@template5", String.Empty);

				// Execute this query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Add_User_Templates_Link", paramList);

				int currentIndex = 5;
				while (Templates.Count > currentIndex)
				{
					paramList[0] = new SqlParameter("@userid", UserID);
					paramList[1] = new SqlParameter("@template_default", String.Empty);

					if (Templates.Count > currentIndex)
						paramList[2] = new SqlParameter("@template2", Templates[currentIndex]);
					else
						paramList[2] = new SqlParameter("@template2", String.Empty);

					if (Templates.Count > currentIndex + 1 )
						paramList[3] = new SqlParameter("@template3", Templates[currentIndex + 1]);
					else
						paramList[3] = new SqlParameter("@template3", String.Empty);

					if (Templates.Count > currentIndex + 2)
						paramList[4] = new SqlParameter("@template4", Templates[currentIndex + 2]);
					else
						paramList[4] = new SqlParameter("@template4", String.Empty);

					if (Templates.Count > currentIndex + 3)
						paramList[5] = new SqlParameter("@template5", Templates[currentIndex + 3]);
					else
						paramList[5] = new SqlParameter("@template5", String.Empty);

					// Execute this query stored procedure
					SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Add_User_Templates_Link", paramList);

					currentIndex += 4;
				} 

				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Sets the list of projects possible for a given user </summary>
		/// <param name="UserID"> Primary key for this user from the database </param>
		/// <param name="Projects"> List of projects to link to this user</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'mySobek_Add_User_Projects_Link' stored procedure</remarks> 
		public static bool Update_SobekCM_User_Projects(int UserID, ReadOnlyCollection<string> Projects, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Update_SobekCM_User_Projects", String.Empty);
			}

			// Call the routine
			try
			{
				// Build the parameter list for the first run
				SqlParameter[] paramList = new SqlParameter[6];
				paramList[0] = new SqlParameter("@userid", UserID);
				if (Projects.Count > 0)
					paramList[1] = new SqlParameter("@project_default", Projects[0]);
				else
					paramList[1] = new SqlParameter("@project_default", Projects[0]);

				if (Projects.Count > 1)
					paramList[2] = new SqlParameter("@project2", Projects[1]);
				else
					paramList[2] = new SqlParameter("@project2", String.Empty);

				if (Projects.Count > 2)
					paramList[3] = new SqlParameter("@project3", Projects[2]);
				else
					paramList[3] = new SqlParameter("@project3", String.Empty);

				if (Projects.Count > 3)
					paramList[4] = new SqlParameter("@project4", Projects[3]);
				else
					paramList[4] = new SqlParameter("@project4", String.Empty);

				if (Projects.Count > 4)
					paramList[5] = new SqlParameter("@project5", Projects[4]);
				else
					paramList[5] = new SqlParameter("@project5", String.Empty);

				// Execute this query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Add_User_Projects_Link", paramList);

				int currentIndex = 5;
				while (Projects.Count > currentIndex)
				{
					paramList[0] = new SqlParameter("@userid", UserID);
					paramList[1] = new SqlParameter("@project_default", String.Empty);

					if (Projects.Count > currentIndex)
						paramList[2] = new SqlParameter("@project2", Projects[currentIndex]);
					else
						paramList[2] = new SqlParameter("@project2", String.Empty);

					if (Projects.Count > currentIndex + 1)
						paramList[3] = new SqlParameter("@project3", Projects[currentIndex + 1]);
					else
						paramList[3] = new SqlParameter("@project3", String.Empty);

					if (Projects.Count > currentIndex + 2)
						paramList[4] = new SqlParameter("@project4", Projects[currentIndex + 2]);
					else
						paramList[4] = new SqlParameter("@project4", String.Empty);

					if (Projects.Count > currentIndex + 3)
						paramList[5] = new SqlParameter("@project5", Projects[currentIndex + 3]);
					else
						paramList[5] = new SqlParameter("@project5", String.Empty);

					// Execute this query stored procedure
					SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Add_User_Projects_Link", paramList);

					currentIndex += 4;
				}

				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Sets the list of aggregations and permissions tagged to a given user </summary>
		/// <param name="UserID"> Primary key for this user from the database </param>
		/// <param name="Aggregations"> List of aggregations and permissions to link to this user </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Add_User_Aggregations_Link' stored procedure</remarks> 
		public static bool Update_SobekCM_User_Aggregations(int UserID, ReadOnlyCollection<User_Editable_Aggregation> Aggregations, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Update_SobekCM_User_Aggregations", String.Empty);
			}

			// Call the routine
			try
			{
				// Build the parameter list for the first run
				SqlParameter[] paramList = new SqlParameter[16];
				paramList[0] = new SqlParameter("@userid", UserID);

				if (Aggregations.Count > 0)
				{
					paramList[1] = new SqlParameter("@aggregationcode1", Aggregations[0].Code);
					paramList[2] = new SqlParameter("@canselect1", Aggregations[0].CanSelect);
					paramList[3] = new SqlParameter("@canedit1", Aggregations[0].CanEditItems);
					paramList[4] = new SqlParameter("@iscurator1", Aggregations[0].IsCurator);
					paramList[5] = new SqlParameter("@onhomepage1", Aggregations[0].OnHomePage);
				}
				else
				{
					paramList[1] = new SqlParameter("@aggregationcode1", String.Empty);
					paramList[2] = new SqlParameter("@canselect1", false);
					paramList[3] = new SqlParameter("@canedit1", false);
					paramList[4] = new SqlParameter("@iscurator1", false);
					paramList[5] = new SqlParameter("@onhomepage1", false);
				}

				if (Aggregations.Count > 1)
				{
					paramList[6] = new SqlParameter("@aggregationcode2", Aggregations[1].Code);
					paramList[7] = new SqlParameter("@canselect2", Aggregations[1].CanSelect);
					paramList[8] = new SqlParameter("@canedit2", Aggregations[1].CanEditItems);
					paramList[9] = new SqlParameter("@iscurator2", Aggregations[1].IsCurator);
					paramList[10] = new SqlParameter("@onhomepage2", Aggregations[1].OnHomePage);
				}
				else
				{
					paramList[6] = new SqlParameter("@aggregationcode2", String.Empty);
					paramList[7] = new SqlParameter("@canselect2", false);
					paramList[8] = new SqlParameter("@canedit2", false);
					paramList[9] = new SqlParameter("@iscurator2", false);
					paramList[10] = new SqlParameter("@onhomepage2", false);
				}

				if (Aggregations.Count > 2)
				{
					paramList[11] = new SqlParameter("@aggregationcode3", Aggregations[2].Code);
					paramList[12] = new SqlParameter("@canselect3", Aggregations[2].CanSelect);
					paramList[13] = new SqlParameter("@canedit3", Aggregations[2].CanEditItems);
					paramList[14] = new SqlParameter("@iscurator3", Aggregations[2].IsCurator);
					paramList[15] = new SqlParameter("@onhomepage3", Aggregations[2].OnHomePage);
				}
				else
				{
					paramList[11] = new SqlParameter("@aggregationcode3", String.Empty);
					paramList[12] = new SqlParameter("@canselect3", false);
					paramList[13] = new SqlParameter("@canedit3", false);
					paramList[14] = new SqlParameter("@iscurator3", false);
					paramList[15] = new SqlParameter("@onhomepage3", false);
				}

				// Execute this query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Add_User_Aggregations_Link", paramList);

				int currentIndex = 3;
				while (Aggregations.Count > currentIndex)
				{
					// Build the parameter list for the first run
					paramList[0] = new SqlParameter("@userid", UserID);

					if (Aggregations.Count > currentIndex)
					{
						paramList[1] = new SqlParameter("@aggregationcode1", Aggregations[currentIndex].Code);
						paramList[2] = new SqlParameter("@canselect1", Aggregations[currentIndex].CanSelect);
						paramList[3] = new SqlParameter("@canedit1", Aggregations[currentIndex].CanEditItems);
						paramList[4] = new SqlParameter("@iscurator1", Aggregations[currentIndex].IsCurator);
						paramList[5] = new SqlParameter("@onhomepage1", Aggregations[currentIndex].OnHomePage);
					}
					else
					{
						paramList[1] = new SqlParameter("@aggregationcode1", String.Empty);
						paramList[2] = new SqlParameter("@canselect1", false);
						paramList[3] = new SqlParameter("@canedit1", false);
						paramList[4] = new SqlParameter("@iscurator1", false);
						paramList[5] = new SqlParameter("@onhomepage1", false);
					}

					if (Aggregations.Count > currentIndex + 1)
					{
						paramList[6] = new SqlParameter("@aggregationcode2", Aggregations[currentIndex + 1].Code);
						paramList[7] = new SqlParameter("@canselect2", Aggregations[currentIndex + 1].CanSelect);
						paramList[8] = new SqlParameter("@canedit2", Aggregations[currentIndex + 1].CanEditItems);
						paramList[9] = new SqlParameter("@iscurator2", Aggregations[currentIndex + 1].IsCurator);
						paramList[10] = new SqlParameter("@onhomepage2", Aggregations[currentIndex + 1].OnHomePage);
					}
					else
					{
						paramList[6] = new SqlParameter("@aggregationcode2", String.Empty);
						paramList[7] = new SqlParameter("@canselect2", false);
						paramList[8] = new SqlParameter("@canedit2", false);
						paramList[9] = new SqlParameter("@iscurator2", false);
						paramList[10] = new SqlParameter("@onhomepage2", false);
					}

					if (Aggregations.Count > currentIndex + 2)
					{
						paramList[11] = new SqlParameter("@aggregationcode3", Aggregations[currentIndex + 2].Code);
						paramList[12] = new SqlParameter("@canselect3", Aggregations[currentIndex + 2].CanSelect);
						paramList[13] = new SqlParameter("@canedit3", Aggregations[currentIndex + 2].CanEditItems);
						paramList[14] = new SqlParameter("@iscurator3", Aggregations[currentIndex + 2].IsCurator);
						paramList[15] = new SqlParameter("@onhomepage3", Aggregations[currentIndex + 2].OnHomePage);
					}
					else
					{
						paramList[11] = new SqlParameter("@aggregationcode3", String.Empty);
						paramList[12] = new SqlParameter("@canselect3", false);
						paramList[13] = new SqlParameter("@canedit3", false);
						paramList[14] = new SqlParameter("@iscurator3", false);
						paramList[15] = new SqlParameter("@onhomepage3", false);
					}
					 
					// Execute this query stored procedure
					SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Add_User_Aggregations_Link", paramList);

					currentIndex += 3;
				}

				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}


		/// <summary> Sets some of the basic information and global permissions values for a single user group </summary>
		/// <param name="UserGroupID"> Primary key for this user group from the database, or -1 for a new user group </param>
		/// <param name="GroupName"> Name of this user group </param>
		/// <param name="GroupDescription"> Basic description of this user group </param>
		/// <param name="Can_Submit"> Flag indicates if this user group can submit items </param>
		/// <param name="Is_Internal"> Flag indicates if this user group is considered an 'internal user'</param>
		/// <param name="Can_Edit_All"> Flag indicates if this user group is authorized to edit all items in the library</param>
		/// <param name="Is_System_Admin"> Flag indicates if this user group is a system Administrator</param>
        /// <param name="Is_Portal_Admin"> Flag indicated if this user group is a portal administrator </param>
		/// <param name="Clear_Projects_Templates"> Flag indicates whether to clear projects and templates for this user group </param>
		/// <param name="Clear_Aggregation_Links"> Flag indicates whether to clear item aggregations linked to this user group </param>
		/// <param name="Clear_Editable_Links"> Flag indicates whether to clear the link between this user group and editable regex expressions  </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> UserGroupId for a new user group, if this was to save a new one </returns>
		/// <remarks> This calls the 'mySobek_Save_User_Group' stored procedure</remarks> 
		public static int Save_User_Group(int UserGroupID, string GroupName, string GroupDescription, bool Can_Submit, bool Is_Internal, bool Can_Edit_All, bool Is_System_Admin, bool Is_Portal_Admin, bool Include_Tracking_Standard_Forms, bool Clear_Projects_Templates, bool Clear_Aggregation_Links, bool Clear_Editable_Links, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Save_User_Group", String.Empty);
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[14];
				paramList[0] = new SqlParameter("@usergroupid", UserGroupID);
				paramList[1] = new SqlParameter("@groupname", GroupName);
				paramList[2] = new SqlParameter("@groupdescription", GroupDescription);
				paramList[3] = new SqlParameter("@can_submit_items", Can_Submit);
				paramList[4] = new SqlParameter("@is_internal", Is_Internal);
				paramList[6] = new SqlParameter("@can_edit_all", Can_Edit_All);
				paramList[7] = new SqlParameter("@is_system_admin", Is_System_Admin);
                paramList[8] = new SqlParameter("@is_portal_admin", Is_Portal_Admin);
                paramList[9] = new SqlParameter("@include_tracking_standard_forms", Include_Tracking_Standard_Forms );
				paramList[10] = new SqlParameter("@clear_projects_templates", Clear_Projects_Templates);
				paramList[11] = new SqlParameter("@clear_aggregation_links", Clear_Aggregation_Links);
				paramList[12] = new SqlParameter("@clear_editable_links", Clear_Editable_Links);
				paramList[13] = new SqlParameter("@new_usergroupid", UserGroupID) {Direction = ParameterDirection.InputOutput};

				// Execute this query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Save_User_Group", paramList);

				// Succesful, so return new id, if there was one
				return Convert.ToInt32(paramList[13].Value);
			}
			catch (Exception ee)
			{
				lastException = ee;
				return -1;
			}
		}

		/// <summary> Sets the list of templates possible for a given user group </summary>
		/// <param name="UserGroupID"> Primary key for this user group from the database </param>
		/// <param name="Templates"> List of templates to link to this user group </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'mySobek_Add_User_Group_Templates_Link' stored procedure</remarks> 
		public static bool Update_SobekCM_User_Group_Templates(int UserGroupID, List<string> Templates, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Update_SobekCM_User_Group_Templates", String.Empty);
			}

			// Ensure five values
			while (Templates.Count < 5)
				Templates.Add(String.Empty);

			// Call the routine
			try
			{
				// Build the parameter list for the first run
				SqlParameter[] paramList = new SqlParameter[6];
				paramList[0] = new SqlParameter("@usergroupid", UserGroupID);
				paramList[1] = new SqlParameter("@template1", Templates[0]);
				paramList[2] = new SqlParameter("@template2", Templates[1]);
				paramList[3] = new SqlParameter("@template3", Templates[2]);
				paramList[4] = new SqlParameter("@template4", Templates[3]);
				paramList[5] = new SqlParameter("@template5", Templates[4]);

				// Execute this query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Add_User_Group_Templates_Link", paramList);

				int currentIndex = 5;
				while (Templates.Count > currentIndex)
				{
					while (Templates.Count < currentIndex + 4)
						Templates.Add(String.Empty);

					paramList[0] = new SqlParameter("@usergroupid", UserGroupID);
					paramList[1] = new SqlParameter("@template1", String.Empty);
					paramList[2] = new SqlParameter("@template2", Templates[currentIndex]);
					paramList[3] = new SqlParameter("@template3", Templates[currentIndex + 1]);
					paramList[4] = new SqlParameter("@template4", Templates[currentIndex + 2]);
					paramList[5] = new SqlParameter("@template5", Templates[currentIndex + 3]);

					// Execute this query stored procedure
					SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Add_User_Group_Templates_Link", paramList);

					currentIndex += 4;
				}

				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Sets the list of projects possible for a given user group </summary>
		/// <param name="UserGroupID"> Primary key for this user group from the database </param>
		/// <param name="Projects"> List of projects to link to this user group</param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'mySobek_Add_User_Group_Projects_Link' stored procedure</remarks> 
		public static bool Update_SobekCM_User_Group_Projects(int UserGroupID, List<string> Projects, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Update_SobekCM_User_Group_Projects", String.Empty);
			}

			// Ensure five values
			while (Projects.Count < 5)
				Projects.Add(String.Empty);

			// Call the routine
			try
			{
				// Build the parameter list for the first run
				SqlParameter[] paramList = new SqlParameter[6];
				paramList[0] = new SqlParameter("@usergroupid", UserGroupID);
				paramList[1] = new SqlParameter("@project1", Projects[0]);
				paramList[2] = new SqlParameter("@project2", Projects[1]);
				paramList[3] = new SqlParameter("@project3", Projects[2]);
				paramList[4] = new SqlParameter("@project4", Projects[3]);
				paramList[5] = new SqlParameter("@project5", Projects[4]);

				// Execute this query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Add_User_Group_Projects_Link", paramList);

				int currentIndex = 5;
				while (Projects.Count > currentIndex)
				{
					while (Projects.Count < currentIndex + 4)
						Projects.Add(String.Empty);

					paramList[0] = new SqlParameter("@usergroupid", UserGroupID);
					paramList[1] = new SqlParameter("@project1", String.Empty);
					paramList[2] = new SqlParameter("@project2", Projects[currentIndex]);
					paramList[3] = new SqlParameter("@project3", Projects[currentIndex + 1]);
					paramList[4] = new SqlParameter("@project4", Projects[currentIndex + 2]);
					paramList[5] = new SqlParameter("@project5", Projects[currentIndex + 3]);

					// Execute this query stored procedure
					SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Add_User_Group_Projects_Link", paramList);

					currentIndex += 4;
				}

				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Sets the list of aggregations and permissions tagged to a given user group</summary>
		/// <param name="UserGroupID"> Primary key for this user group from the database </param>
		/// <param name="Aggregations"> List of aggregations and permissions to link to this user group </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Add_User_Group_Aggregations_Link' stored procedure</remarks> 
		public static bool Update_SobekCM_User_Group_Aggregations(int UserGroupID, List<User_Editable_Aggregation> Aggregations, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Update_SobekCM_User_Group_Aggregations", String.Empty);
			}

			// Ensure five values
			while (Aggregations.Count < 3)
				Aggregations.Add(new User_Editable_Aggregation(String.Empty, String.Empty, false, false, false, false));


			// Call the routine
			try
			{
				// Build the parameter list for the first run
				SqlParameter[] paramList = new SqlParameter[13];
				paramList[0] = new SqlParameter("@usergroupid", UserGroupID);

				paramList[1] = new SqlParameter("@aggregationcode1", Aggregations[0].Code);
				paramList[2] = new SqlParameter("@canselect1", Aggregations[0].CanSelect);
				paramList[3] = new SqlParameter("@canedit1", Aggregations[0].CanEditItems);
				paramList[4] = new SqlParameter("@iscurator1", Aggregations[0].IsCurator);

				paramList[5] = new SqlParameter("@aggregationcode2", Aggregations[1].Code);
				paramList[6] = new SqlParameter("@canselect2", Aggregations[1].CanSelect);
				paramList[7] = new SqlParameter("@canedit2", Aggregations[1].CanEditItems);
				paramList[8] = new SqlParameter("@iscurator2", Aggregations[1].IsCurator);

				paramList[9] = new SqlParameter("@aggregationcode3", Aggregations[2].Code);
				paramList[10] = new SqlParameter("@canselect3", Aggregations[2].CanSelect);
				paramList[11] = new SqlParameter("@canedit3", Aggregations[2].CanEditItems);
				paramList[12] = new SqlParameter("@iscurator3", Aggregations[2].IsCurator);

				// Execute this query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Add_User_Group_Aggregations_Link", paramList);

				int currentIndex = 3;
				while (Aggregations.Count > currentIndex)
				{
					while (Aggregations.Count < currentIndex + 3)
						Aggregations.Add(new User_Editable_Aggregation(String.Empty, String.Empty, false, false, false, false));

					// Build the parameter list for the first run
					paramList[0] = new SqlParameter("@usergroupid", UserGroupID);

					paramList[1] = new SqlParameter("@aggregationcode1", Aggregations[currentIndex].Code);
					paramList[2] = new SqlParameter("@canselect1", Aggregations[currentIndex].CanSelect);
					paramList[3] = new SqlParameter("@canedit1", Aggregations[currentIndex].CanEditItems);
					paramList[4] = new SqlParameter("@iscurator1", Aggregations[currentIndex].IsCurator);

					paramList[5] = new SqlParameter("@aggregationcode2", Aggregations[currentIndex + 1].Code);
					paramList[6] = new SqlParameter("@canselect2", Aggregations[currentIndex + 1].CanSelect);
					paramList[7] = new SqlParameter("@canedit2", Aggregations[currentIndex + 1].CanEditItems);
					paramList[8] = new SqlParameter("@iscurator2", Aggregations[currentIndex + 1].IsCurator);

					paramList[9] = new SqlParameter("@aggregationcode3", Aggregations[currentIndex + 2].Code);
					paramList[10] = new SqlParameter("@canselect3", Aggregations[currentIndex + 2].CanSelect);
					paramList[11] = new SqlParameter("@canedit3", Aggregations[currentIndex + 2].CanEditItems);
					paramList[12] = new SqlParameter("@iscurator3", Aggregations[currentIndex + 2].IsCurator);

					// Execute this query stored procedure
					SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Add_User_Group_Aggregations_Link", paramList);

					currentIndex += 3;
				}

				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Saves a new project, or edits an existing project name </summary>
		/// <param name="Code"> Code for the new project, or project to edit </param>
		/// <param name="Name"> Descriptive name for this project </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'mySobek_Save_Project' stored procedure</remarks> 
		public static bool Save_Project(string Code, string Name, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Save_Project", String.Empty);
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@project_code", Code);
				paramList[1] = new SqlParameter("@project_name", Name);

				// Execute this query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Save_Project", paramList);

				// Succesful, so return true
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Saves a new template, or edits an existing template name </summary>
		/// <param name="Code"> Code for the new template, or template to edit </param>
		/// <param name="Name"> Descriptive name for this template </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'mySobek_Save_Template' stored procedure</remarks> 
		public static bool Save_Template(string Code, string Name, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Save_Template", String.Empty);
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@project_code", Code);
				paramList[1] = new SqlParameter("@project_name", Name);

				// Execute this query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "mySobek_Save_Template", paramList);

				// Succesful, so return true
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}


		/// <summary> Gets the build log for a particular aggregation </summary>
		/// <param name="AggregationID"> Primary key for this aggregation in the database </param>
		/// <returns> Aggregation build log table </returns>
		/// <remarks> This calls the 'SobekCM_Build_Log_Get' stored procedure </remarks> 
		public static DataTable Get_Aggregation_Build_Log(int AggregationID)
		{

			try
			{
				// build the parameter list
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@aggregationid", AggregationID);

				// Get the table
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Build_Log_Get", paramList);

				// Return true, since no exception caught
				return tempSet.Tables[0];

			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		#endregion

		#region Methods used by the SobekCM Manager Application

		#region Methods to get and edit information about the ITEM GROUP

		/// <summary> Saves the serial hierarchy and link between an item and an item group </summary>
		/// <param name="GroupID"> Group ID this item belongs to </param>
		/// <param name="ItemID"> Primary key for the item itself </param>
		/// <param name="Level1_Text"> Text for the FIRST level of serial hierarchy relating this item to the item group </param>
		/// <param name="Level1_Index"> Sorting index for the FIRST level of serial hierarchy relating this item to the item group </param>
		/// <param name="Level2_Text"> Text for the SECOND level of serial hierarchy relating this item to the item group </param>
		/// <param name="Level2_Index"> Sorting index for the SECOND level of serial hierarchy relating this item to the item group</param>
		/// <param name="Level3_Text"> Text for the THIRD level of serial hierarchy relating this item to the item group </param>
		/// <param name="Level3_Index"> Sorting index for the THIRD level of serial hierarchy relating this item to the item group</param>
		/// <param name="Level4_Text"> Text for the FOURTH level of serial hierarchy relating this item to the item group </param>
		/// <param name="Level4_Index"> Sorting index for the FOURTH level of serial hierarchy relating this item to the item group</param>
		/// <param name="Level5_Text"> Text for the FIFTH level of serial hierarchy relating this item to the item group </param>
		/// <param name="Level5_Index"> Sorting index for the FIFTH level of serial hierarchy relating this item to the item group</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Save_Serial_Hierarchy' stored procedure </remarks> 
		public static bool Save_Serial_Hierarchy(int GroupID, int ItemID, string Level1_Text, int Level1_Index,
												 string Level2_Text, int Level2_Index, string Level3_Text, int Level3_Index, string Level4_Text, 
												 int Level4_Index, string Level5_Text, int Level5_Index )
		{
			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[13];
				paramList[0] = new SqlParameter("@GroupID", GroupID);
				paramList[1] = new SqlParameter("@ItemID", ItemID);
				paramList[2] = new SqlParameter("@Level1_Text", Level1_Text);
				paramList[3] = new SqlParameter("@Level1_Index", Level1_Index);
				paramList[4] = new SqlParameter("@Level2_Text", Level2_Text);
				paramList[5] = new SqlParameter("@Level2_Index", Level2_Index);
				paramList[6] = new SqlParameter("@Level3_Text", Level3_Text);
				paramList[7] = new SqlParameter("@Level3_Index", Level3_Index);
				paramList[8] = new SqlParameter("@Level4_Text", Level4_Text);
				paramList[9] = new SqlParameter("@Level4_Index", Level4_Index);
				paramList[10] = new SqlParameter("@Level5_Text", Level5_Text);
				paramList[11] = new SqlParameter("@Level5_Index", Level5_Index);
				paramList[12] = new SqlParameter("@SerialHierarchy", String.Empty);


				// Execute this non-query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Save_Serial_Hierarchy", paramList);

				return true;
			}
			catch (Exception ee)
			{
				// Pass this exception onto the method to handle it
				lastException = ee;
				return false;
			}
		}

		#endregion

		/// <summary> Gets the list of items that were recently quality control accepted and still
		/// need to be set to PUBLIC or RESTRICTED online </summary>
		/// <remarks> This calls the 'Tracking_Items_Pending_Online_Complete' stored procedure </remarks> 
		public static DataTable Items_Pending_Online_Complete
		{
			get
			{
				try
				{
					// Create the connection
					SqlConnection connect = new SqlConnection(connectionString);

					// Create the command 
					SqlCommand executeCommand = new SqlCommand("Tracking_Items_Pending_Online_Complete", connect)
													{CommandType = CommandType.StoredProcedure};

					// Create the adapter
					SqlDataAdapter adapter = new SqlDataAdapter(executeCommand);

					// Create the dataset
					DataSet list = new DataSet();

					// Fill the dataset
					adapter.Fill(list);

					// Return the first table
					return list.Tables[0];

				}
				catch (Exception ee)
				{
					lastException = ee;
					return null;
				}
			}
		}

		/// <summary> Gets the report of all newspaper items which do not have serial information </summary>
		/// <remarks> This calls the 'SobekCM_Manager_Newspapers_Without_Serial_Info' stored procedure </remarks> 
		public static DataTable Newspapers_Without_Serial_Info
		{
			get
			{
				try
				{
					// Create the connection
					SqlConnection connect = new SqlConnection(connectionString);

					// Create the command 
					SqlCommand executeCommand = new SqlCommand("SobekCM_Manager_Newspapers_Without_Serial_Info", connect)
													{CommandType = CommandType.StoredProcedure};

					// Create the adapter
					SqlDataAdapter adapter = new SqlDataAdapter(executeCommand);

					// Create the dataset
					DataSet list = new DataSet();

					// Fill the dataset
					adapter.Fill(list);

					// Return the first table
					return list.Tables[0];

				}
				catch (Exception ee)
				{
					lastException = ee;
					return null;
				}
			}
		}




		/// <summary> Returns the primary key for this item group, identified by bibliographic identifier </summary>
		/// <param name="BibID"> Bibliographic identifier to pull the primary key from the database for </param>
		/// <returns> GroupID for this bibliographic identifier, or -1 if missing</returns>
		/// <remarks> This calls the 'SobekCM_Manager_GroupID_From_BibID' stored procedure </remarks> 
		public static int Get_GroupID_From_BibID(string BibID)
		{
			try
			{
				// Clear the last exception in this case
				lastException = null;

				// Create the connection
				SqlConnection connect = new SqlConnection(connectionString);

				// Create the command 
				SqlCommand executeCommand = new SqlCommand("SobekCM_Manager_GroupID_From_BibID", connect)
												{CommandType = CommandType.StoredProcedure};
				executeCommand.Parameters.AddWithValue("@bibid", BibID);

				// Create the adapter
				SqlDataAdapter adapter = new SqlDataAdapter(executeCommand);

				// Create the dataset
				DataSet list = new DataSet();

				// Fill the dataset
				adapter.Fill(list);

				// If there is a match return it
				return (list.Tables[0].Rows.Count > 0) ? Convert.ToInt32(list.Tables[0].Rows[0][0]) : -1;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return -1;
			}
		}

        /// <summary> Gets the size of the online files and the size of the archived files, by aggregation </summary>
        /// <param name="AggregationCode1"> Code for the primary aggregation  </param>
        /// <param name="AggregationCode2"> Code for the secondary aggregation </param>
        /// <param name="Online_Stats_Type"> Flag indicates if online content reporting should be included ( 0=skip, 1=summary, 2=details )</param>
        /// <param name="Archival_Stats_Type"> Flag indicates if locally archived reporting should be included ( 0=skip, 1=summary, 2=details )</param>
        /// <returns> Dataset with two tables, first is the online space, and second is the archived space </returns>
        /// <remarks> If two codes are passed in, then the values returned is the size of all items which exist
        ///  in both the provided aggregations.  Otherwise, it is just the size of all items in the primary 
        ///  aggregation. <br /><br /> This calls the 'SobekCM_Online_Archived_Space' stored procedure </remarks> 
        public static DataSet Online_Archived_Space(string AggregationCode1, string AggregationCode2, short Online_Stats_Type, short Archival_Stats_Type)
        {
            try
            {
                // Create the connection
                SqlConnection connect = new SqlConnection(connectionString);

                // Create the command 
                SqlCommand executeCommand = new SqlCommand("SobekCM_Online_Archived_Space", connect)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 120
                };
                executeCommand.Parameters.AddWithValue("@code1", AggregationCode1);
                executeCommand.Parameters.AddWithValue("@code2", AggregationCode2);
                executeCommand.Parameters.AddWithValue("@include_online", Online_Stats_Type);
                executeCommand.Parameters.AddWithValue("@include_archive", Archival_Stats_Type);

                // Create the adapter
                SqlDataAdapter adapter = new SqlDataAdapter(executeCommand);

                // Create the dataset
                DataSet list = new DataSet();

                // Fill the dataset
                adapter.Fill(list);

                return list;
            }
            catch (Exception ee)
            {
                lastException = ee;
                return null;
            }
        }

		#endregion

		#region Methods to interact with the TIVOLI archive file log in the database

		/// <summary> Get the list of all archived TIVOLI files by BibID and VID </summary>
		/// <param name="BibID"> Bibliographic identifier </param>
		/// <param name="VID"> Volume identifier </param>
		/// <returns> List of all the files archived for a particular digital resource </returns>
		/// <remarks> This calls the 'Tivoli_Get_File_By_Bib_VID' stored procedure </remarks> 
		public static DataTable Tivoli_Get_Archived_Files(string BibID, string VID)
		{
			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[2];
				paramList[0] = new SqlParameter("@BibID", BibID);
				paramList[1] = new SqlParameter("@VID", VID);

				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "Tivoli_Get_File_By_Bib_VID", paramList);
				return ((tempSet == null) || (tempSet.Tables.Count == 0) || (tempSet.Tables[0].Rows.Count == 0)) ? null : tempSet.Tables[0];
			}
			catch 
			{
				// Return null for this case
				return null;

			}
		}

		/// <summary> Add information about a single file to the archived TIVOLI </summary>
		/// <param name="BibID"> Bibliographic identifier </param>
		/// <param name="VID"> Volume identifier </param>
		/// <param name="Folder"> Name of the folder </param>
		/// <param name="FileName"> Name of the archived file </param>
		/// <param name="FileSize"> Size of the archived file </param>
		/// <param name="LastWriteDate"> Last modified write date of the archived file </param>
		/// <param name="ItemID"> Primary key for this item </param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'Tivoli_Add_File_Archive_Log' stored procedure </remarks> 
		public static bool Tivoli_Add_File_Archive_Log(string BibID, string VID, string Folder, string FileName, long FileSize, DateTime LastWriteDate, int ItemID )
		{
			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[7];
				paramList[0] = new SqlParameter("@BibID", BibID);
				paramList[1] = new SqlParameter("@VID", VID);
				paramList[2] = new SqlParameter("@Folder", Folder);
				paramList[3] = new SqlParameter("@FileName", FileName);
				paramList[4] = new SqlParameter("@Size", FileSize);
				paramList[5] = new SqlParameter("@LastWriteDate", LastWriteDate);
				paramList[6] = new SqlParameter("@ItemID", ItemID);

				// Define a temporary dataset
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "Tivoli_Add_File_Archive_Log", paramList);

				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Adds a worklog that items were archived (tivoli)'d for a specific item </summary>
		/// <param name="BibID"> Bibliographic identifier </param>
		/// <param name="VID"> Volume identifier </param>
		/// <param name="User"> User linked to this progress ( usually blank since this is performed by the Tivoli Processor ) </param>
		/// <param name="UserNotes"> Notes about this process worklog </param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'Tracking_Archive_Complete' stored procedure </remarks> 
		public static bool Tivoli_Archive_Complete(string BibID, string VID, string User, string UserNotes )
		{
			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[4];
				paramList[0] = new SqlParameter("@BibID", BibID);
				paramList[1] = new SqlParameter("@VID", VID);
				paramList[2] = new SqlParameter("@User", User);
				paramList[3] = new SqlParameter("@UserNotes", UserNotes);

				// Define a temporary dataset
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "Tracking_Archive_Complete", paramList);

				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Gets the list of outstanding archive (tivoli) file requests </summary>
		/// <returns> Table with all the outstanding archive (tivoli) file requests </returns>
		/// <remarks> This calls the 'Tivoli_Outstanding_File_Requests' stored procedure </remarks> 
		public static DataTable Tivoli_Outstanding_File_Requests()
		{
			try
			{
				// Define a temporary dataset
				DataSet returnSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "Tivoli_Outstanding_File_Requests");
				if (returnSet != null)
					return returnSet.Tables[0];

				return null;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

        /// <summary> Completes a given archive tivoli file request in the database </summary>
        /// <param name="TivoliRequestID">Primary key for the tivolie request which either completed or failed </param>
        /// <param name="Email_Body"> Body of the response email </param>
        /// <param name="Email_Subject">Subject line to use for the response email </param>
        /// <param name="isFailure"> Flag indicates if this represents a failure to retrieve the material from TIVOLI</param>
        /// <returns> TRUE if successful, otherwise FALSE </returns>
        /// <remarks> This calls the 'Tracking_Archive_Complete' stored procedure </remarks> 
        public static bool Tivoli_Complete_File_Request(int TivoliRequestID, string Email_Body, string Email_Subject, bool isFailure)
        {
            try
            {
                // Build the parameter list
                SqlParameter[] paramList = new SqlParameter[4];
                paramList[0] = new SqlParameter("@tivolirequestid", TivoliRequestID);
                paramList[1] = new SqlParameter("@email_body", Email_Body);
                paramList[2] = new SqlParameter("@email_subject", Email_Subject);
                paramList[3] = new SqlParameter("@isFailure", isFailure);

                // Define a temporary dataset
                SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "Tivoli_Complete_File_Request", paramList);

                return true;
            }
            catch (Exception ee)
            {
                lastException = ee;
                return false;
            }
        }

		/// <summary> Requests a package or file from the archives/tivoli </summary>
		/// <param name="BibID"> Bibliographic identifier (BibID) for the item to retrieve files for </param>
		/// <param name="VID"> Volume identifier (VID) for the item to retrieve files for </param>
		/// <param name="Files"> Files to retrieve from archives/tivoli </param>
		/// <param name="UserName"> Name of the user requesting the retrieval </param>
		/// <param name="EmailAddress"> Email address for the user requesting the retrieval </param>
		/// <param name="RequestNote"> Any custom request note, to be returned in the email once retrieval is complete </param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'Tivoli_Request_File' stored procedure </remarks> 
		public static bool Tivoli_Request_File( string BibID, string VID, string Files, string UserName, string EmailAddress, string RequestNote )
		{
			try
			{
				string folder = BibID + "\\" + VID;
				if (Files.Length == 0)
					Files = "*";

				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[5];
				paramList[0] = new SqlParameter("@folder", folder);
				paramList[1] = new SqlParameter("@filename", Files);
				paramList[2] = new SqlParameter("@username", UserName);
				paramList[3] = new SqlParameter("@emailaddress", EmailAddress);
				paramList[4] = new SqlParameter("@requestnote", RequestNote);

				// Define a temporary dataset
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "Tivoli_Request_File", paramList);

				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}


		#endregion

		#region Methods to pull lists of items for the SMaRT tracking application

		/// <summary> Gets the collection of all items linked to an item aggregation  </summary>
		/// <param name="AggregationCode"> Code for the item aggregation of interest </param>
		/// <returns> Table with all of the item and item group information </returns>
		/// <remarks> This calls the 'Tracking_Get_Aggregation_Browse' stored procedure.</remarks>
		public static DataSet Tracking_Get_Item_Aggregation_Browse(string AggregationCode )
		{
			// Create the connection
			SqlConnection connect = new SqlConnection(connectionString + "Connection Timeout=45");

			// Create the command 
			SqlCommand executeCommand = new SqlCommand("Tracking_Get_Aggregation_Browse", connect)
											{CommandTimeout = 45, CommandType = CommandType.StoredProcedure};

			executeCommand.Parameters.AddWithValue("@code", AggregationCode);

			// Create the adapter
			SqlDataAdapter adapter = new SqlDataAdapter(executeCommand);

			// Pull the raw data
			DataSet rawData = new DataSet();
			adapter.Fill(rawData);

			// Return the built results
			return rawData;
		}

		/// <summary> Gets the list of all private and dark items linked to an item aggregation  </summary>
		/// <param name="AggregationCode"> Code for the item aggregation of interest </param>
		/// <param name="ResultsPerPage"> Number of results to return per "page" of results </param>
		/// <param name="ResultsPage"> Which page of results to return ( one-based, so the first page is page number of one )</param>
		/// <param name="Sort"> Current sort to use ( 0 = default by search or browse, 1 = title, 10 = date asc, 11 = date desc )</param>
		/// <param name="Tracer"> Tracer object keeps track of all executions that take place while meeting a user's request </param>
		/// <returns> Table with all of the item and item group information </returns>
		/// <remarks> This calls the 'Tracking_Get_Aggregation_Privates' stored procedure.</remarks>
		public static Private_Items_List Tracking_Get_Aggregation_Private_Items(string AggregationCode, int ResultsPerPage, int ResultsPage, int Sort, Custom_Tracer Tracer )
		{
			if (Tracer != null)
				Tracer.Add_Trace("SobekCM_Database.Tracking_Get_Aggregation_Private_Items", "Pulling list of private items for this aggregation");

			// Create the connection
			SqlConnection connect = new SqlConnection(connectionString + "Connection Timeout=45");

			// Create the command 
			SqlCommand executeCommand = new SqlCommand("Tracking_Get_Aggregation_Privates", connect)
											{CommandTimeout = 45, CommandType = CommandType.StoredProcedure};

			executeCommand.Parameters.AddWithValue("@code", AggregationCode);
			executeCommand.Parameters.AddWithValue("@pagesize", ResultsPerPage);
			executeCommand.Parameters.AddWithValue("@pagenumber", ResultsPage);
			executeCommand.Parameters.AddWithValue("@sort", Sort);
			executeCommand.Parameters.AddWithValue("@minpagelookahead", 1);
			executeCommand.Parameters.AddWithValue("@maxpagelookahead", 1);
			executeCommand.Parameters.AddWithValue("@lookahead_factor", LOOKAHEAD_FACTOR);

			// Add parameters for total items and total titles
			SqlParameter totalItemsParameter = executeCommand.Parameters.AddWithValue("@total_items", 0);
			totalItemsParameter.Direction = ParameterDirection.InputOutput;

			SqlParameter totalTitlesParameter = executeCommand.Parameters.AddWithValue("@total_titles", 0);
			totalTitlesParameter.Direction = ParameterDirection.InputOutput;

			// Create the data reader
			connect.Open();
			Private_Items_List returnArgs;
			using (SqlDataReader reader = executeCommand.ExecuteReader())
			{

				// Create the return argument object
				returnArgs = new Private_Items_List {Title_Results = DataReader_To_Private_Items_List(reader)};

				// Close the reader
				reader.Close();

				// Store the total items/titles
				returnArgs.Total_Items = Convert.ToInt32(totalItemsParameter.Value);
				returnArgs.Total_Titles = Convert.ToInt32(totalTitlesParameter.Value);
			}
			connect.Close();

			if (Tracer != null)
				Tracer.Add_Trace("SobekCM_Database.Tracking_Get_Aggregation_Private_Items", "Done pulling list of private items");

			return returnArgs;
		}

		private static List<Private_Items_List_Title> DataReader_To_Private_Items_List(SqlDataReader reader)
		{
			// Create return list
			List<Private_Items_List_Title> returnValue = new List<Private_Items_List_Title>();

			Dictionary<int, int> lookup = new Dictionary<int, int>();

			// Get all the main title values first
			while (reader.Read())
			{
				// Create new database title object for this
				Private_Items_List_Title result = new Private_Items_List_Title
													  {
														  RowNumber = reader.GetInt32(0),
														  BibID = reader.GetString(1),
														  Group_Title = reader.GetString(2),
														  Type = reader.GetString(3),
														  ALEPH_Number = reader.GetInt32(4),
														  OCLC_Number = reader.GetInt64(5),
														  Last_Activity_Date = reader.GetDateTime(6),
														  Last_Milestone_Date = reader.GetDateTime(7),
														  Complete_Item_Count = reader.GetInt32(8),
														  Primary_Identifier_Type = reader.GetString(9),
														  Primary_Identifier = reader.GetString(10)
													  };

				returnValue.Add(result);

				lookup.Add(result.RowNumber, returnValue.Count - 1);
			}

			// Move to the item table
			reader.NextResult();

			// If there were no titles, then there are no results
			if (returnValue.Count == 0)
				return returnValue;


			// Step through all the item rows, build the item, and add to the title 
			Private_Items_List_Title titleResult = returnValue[0];
			int lastRownumber = titleResult.RowNumber;
			while (reader.Read())
			{
				// Ensure this is the right title for this item 
				int thisRownumber = reader.GetInt32(0);
				if (thisRownumber != lastRownumber)
				{
					titleResult = returnValue[lookup[thisRownumber]];
					lastRownumber = thisRownumber;
				}

				// Create new database item object for this
				Private_Items_List_Item result = new Private_Items_List_Item
													 {
														 VID = reader.GetString(1),
														 Title = reader.GetString(2),
														 Internal_Comments = reader.GetString(3),
														 PubDate = reader.GetString(4),
														 Locally_Archived = reader.GetBoolean(5),
														 Remotely_Archived = reader.GetBoolean(6),
														 Aggregation_Codes = reader.GetString(7),
														 Last_Activity_Date = reader.GetDateTime(8),
														 Last_Activity_Type = reader.GetString(9),
														 Last_Milestone = reader.GetInt32(10),
														 Last_Milestone_Date = reader.GetDateTime(11)
													 };

				// Add this to the title object
				titleResult.Add_Item_Result(result);
			}

			return returnValue;
		}




		/// <summary> Perform a metadata search against items in the database </summary>
		/// <param name="Term1"> First search term for this metadata search </param>
		/// <param name="Field1"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link2"> Link between the first and second terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term2"> Second search term for this metadata search </param>
		/// <param name="Field2"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link3">Link between the second and third search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term3"> Third search term for this metadata search </param>
		/// <param name="Field3"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link4">Link between the third and fourth search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term4"> Fourth search term for this metadata search </param>
		/// <param name="Field4"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link5">Link between the fourth and fifth search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term5"> Fifth search term for this metadata search </param>
		/// <param name="Field5"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link6">Link between the fifth and sixth search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term6"> Sixth search term for this metadata search </param>
		/// <param name="Field6"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link7">Link between the sixth and seventh search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term7"> Seventh search term for this metadata search </param>
		/// <param name="Field7"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link8">Link between the seventh and eighth search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term8"> Eighth search term for this metadata search </param>
		/// <param name="Field8"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="Link9">Link between the eighth and ninth search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term9"> Ninth search term for this metadata search </param>
		/// <param name="Field9"> FIeld number to search for (or -1 to search all fields)</param>
		/// <param name="Link10">Link between the ninth and tenth search terms ( 0=AND, 1=OR, 2=AND NOT )</param>
		/// <param name="Term10"> Tenth search term for this metadata search </param>
		/// <param name="Field10"> Field number to search for (or -1 to search all fields)</param>
		/// <param name="AggregationCode"> Code for the aggregation of interest ( or empty string to search all aggregations )</param>
		/// <returns> Table with all of the item and item group information which matches the metadata search </returns>
		/// <remarks> This calls the 'Tracking_Metadata_Search' stored procedure.</remarks>
		public static DataSet Tracking_Metadata_Search(string Term1, int Field1,
													   int Link2, string Term2, int Field2, int Link3, string Term3, int Field3, int Link4, string Term4, int Field4,
													   int Link5, string Term5, int Field5, int Link6, string Term6, int Field6, int Link7, string Term7, int Field7,
													   int Link8, string Term8, int Field8, int Link9, string Term9, int Field9, int Link10, string Term10, int Field10,
													   string AggregationCode )
		{
			// Create the connection
			SqlConnection connect = new SqlConnection(connectionString + "Connection Timeout=45");

			// Create the command 
			SqlCommand executeCommand = new SqlCommand("Tracking_Metadata_Search", connect)
											{CommandTimeout = 45, CommandType = CommandType.StoredProcedure};

			executeCommand.Parameters.AddWithValue("@term1", Term1);
			executeCommand.Parameters.AddWithValue("@field1", Field1);
			executeCommand.Parameters.AddWithValue("@link2", Link2);
			executeCommand.Parameters.AddWithValue("@term2", Term2);
			executeCommand.Parameters.AddWithValue("@field2", Field2);
			executeCommand.Parameters.AddWithValue("@link3", Link3);
			executeCommand.Parameters.AddWithValue("@term3", Term3);
			executeCommand.Parameters.AddWithValue("@field3", Field3);
			executeCommand.Parameters.AddWithValue("@link4", Link4);
			executeCommand.Parameters.AddWithValue("@term4", Term4);
			executeCommand.Parameters.AddWithValue("@field4", Field4);
			executeCommand.Parameters.AddWithValue("@link5", Link5);
			executeCommand.Parameters.AddWithValue("@term5", Term5);
			executeCommand.Parameters.AddWithValue("@field5", Field5);
			executeCommand.Parameters.AddWithValue("@link6", Link6);
			executeCommand.Parameters.AddWithValue("@term6", Term6);
			executeCommand.Parameters.AddWithValue("@field6", Field6);
			executeCommand.Parameters.AddWithValue("@link7", Link7);
			executeCommand.Parameters.AddWithValue("@term7", Term7);
			executeCommand.Parameters.AddWithValue("@field7", Field7);
			executeCommand.Parameters.AddWithValue("@link8", Link8);
			executeCommand.Parameters.AddWithValue("@term8", Term8);
			executeCommand.Parameters.AddWithValue("@field8", Field8);
			executeCommand.Parameters.AddWithValue("@link9", Link9);
			executeCommand.Parameters.AddWithValue("@term9", Term9);
			executeCommand.Parameters.AddWithValue("@field9", Field9);
			executeCommand.Parameters.AddWithValue("@link10", Link10);
			executeCommand.Parameters.AddWithValue("@term10", Term10);
			executeCommand.Parameters.AddWithValue("@field10", Field10);
			if (AggregationCode.ToUpper() == "ALL")
				AggregationCode = String.Empty;
			executeCommand.Parameters.AddWithValue("@aggregationcode", AggregationCode);

			// Create the adapter
			SqlDataAdapter adapter = new SqlDataAdapter(executeCommand);

			// Pull the raw data
			DataSet rawData = new DataSet();
			adapter.Fill(rawData);

			// Return the built results
			return rawData;
		}

		/// <summary> Performs a basic metadata search over the entire citation, given a search condition </summary>
		/// <param name="Search_Condition"> Search condition string to be run against the databasse </param>
		/// <param name="AggregationCode"> Code for the aggregation of interest ( or empty string to search all aggregations )</param>
		/// <returns> Table with all of the item and item group information which matches the metadata search </returns>
		/// <remarks> This calls the 'Tracking_Metadata_Basic_Search' stored procedure.</remarks>
		public static DataSet Tracking_Metadata_Search(string Search_Condition, string AggregationCode )
		{
			// Create the connection
			SqlConnection connect = new SqlConnection(connectionString + "Connection Timeout=45");

			// Create the command 
			SqlCommand executeCommand = new SqlCommand("Tracking_Metadata_Basic_Search", connect)
											{CommandTimeout = 45, CommandType = CommandType.StoredProcedure};

			executeCommand.Parameters.AddWithValue("@searchcondition", Search_Condition);
			executeCommand.Parameters.AddWithValue("@aggregationcode", AggregationCode);


			// Create the adapter
			SqlDataAdapter adapter = new SqlDataAdapter(executeCommand);

			// Pull the raw data
			DataSet rawData = new DataSet();
			adapter.Fill(rawData);

			// Return the built results
			return rawData;
		}

		/// <summary> Performs a metadata search for a piece of metadata that EXACTLY matches the provided search term </summary>
		/// <param name="Search_Term"> Search condition string to be run against the databasse </param>
		/// <param name="FieldID"> Primary key for the field to search in the database </param>
		/// <param name="AggregationCode"> Code for the aggregation of interest ( or empty string to search all aggregations )</param>
		/// <returns> Table with all of the item and item group information which matches the metadata search </returns>
		/// <remarks> This calls the 'Tracking_Metadata_Exact_Search' stored procedure.</remarks>
		public static DataSet Tracking_Metadata_Exact_Search(string Search_Term, int FieldID, string AggregationCode )
		{
			// Create the connection
			SqlConnection connect = new SqlConnection(connectionString + "Connection Timeout=45");

			// Create the command 
			SqlCommand executeCommand = new SqlCommand("Tracking_Metadata_Exact_Search", connect)
											{CommandTimeout = 45, CommandType = CommandType.StoredProcedure};

			executeCommand.Parameters.AddWithValue("@term1", Search_Term.Replace("''", "'"));
			executeCommand.Parameters.AddWithValue("@field1", FieldID);
			if (AggregationCode.ToUpper() == "ALL")
				AggregationCode = String.Empty;
			executeCommand.Parameters.AddWithValue("@aggregationcode", AggregationCode);

			// Create the adapter
			SqlDataAdapter adapter = new SqlDataAdapter(executeCommand);

			// Pull the raw data
			DataSet rawData = new DataSet();
			adapter.Fill(rawData);

			// Return the built results
			return rawData;
		}

		/// <summary> Returns the list of all items/titles which match a given OCLC number </summary>
		/// <param name="OCLC_Number"> OCLC number to look for matching items </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Table with all of the item and item group information which matches the OCLC number </returns>
		/// <remarks> This calls the 'Tracking_Items_By_OCLC' stored procedure <br /><br />
		/// This is very similar to the <see cref="SobekCM_Database.Items_By_OCLC_Number" /> method, except it returns more information, since
		/// the tracking application does not have basic information about each item/title in its cache, unlike the
		/// web server application, which does cache this information. </remarks>
		public static DataSet Tracking_Items_By_OCLC_Number(long OCLC_Number, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Items_By_OCLC_Number", "Searching by OCLC in the database");
			}

			// Build the parameter list
			SqlParameter[] paramList = new SqlParameter[1];
			paramList[0] = new SqlParameter("@oclc_number", OCLC_Number);

			// Get the matching set
			DataSet rawData = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "Tracking_Items_By_OCLC", paramList);

			// Return the built results
			return rawData;
		}

		/// <summary> Returns the list of all items/titles which match a given ALEPH number </summary>
		/// <param name="ALEPH_Number"> ALEPH number to look for matching items </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Table with all of the item and item group information which matches the ALEPH number </returns>
		/// <remarks> This calls the 'Tracking_Items_By_ALEPH' stored procedure. <br /><br />
		/// This is very similar to the <see cref="SobekCM_Database.Items_By_ALEPH_Number" /> method, except it returns more information, since
		/// the tracking application does not have basic information about each item/title in its cache, unlike the
		/// web server application, which does cache this information. </remarks>
		public static DataSet Tracking_Items_By_ALEPH_Number(int ALEPH_Number, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Items_By_ALEPH_Number", "Searching by ALEPH in the database");
			}

			// Build the parameter list
			SqlParameter[] paramList = new SqlParameter[1];
			paramList[0] = new SqlParameter("@aleph_number", ALEPH_Number);

			// Get the matching set
			DataSet rawData = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "Tracking_Items_By_ALEPH", paramList);

			// Return the built results
			return rawData;
		}

		/// <summary> Gets the list of all items within this item group, indicated by BibID, including additional information for the SMaRT tracking application </summary>
		/// <param name="BibID"> Bibliographic identifier for the title of interest </param>
		/// <param name="tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Strongly typed dataset with information about the title (item group), including volumes, icons, and skins</returns>
		/// <remarks> This calls the 'Tracking_Get_Multiple_Volumes' stored procedure <br /><br />
		/// This is very similar to the <see cref="SobekCM_Database.Get_Multiple_Volumes" /> method, except it returns more information, since
		/// the tracking application does not have basic information about each item/title in its cache, unlike the
		/// web server application, which does cache this information. </remarks>
		public static SobekCM_Items_In_Title Tracking_Multiple_Volumes(string BibID, Custom_Tracer tracer)
		{
			if (tracer != null)
			{
				tracer.Add_Trace("SobekCM_Database.Tracking_Multiple_Volumes", "List of volumes for " + BibID + " pulled from database");
			}

			try
			{
				// Create the connection
				SqlConnection connect = new SqlConnection(connectionString);

				// Create the command 
				SqlCommand executeCommand = new SqlCommand("Tracking_Get_Multiple_Volumes", connect)
												{CommandType = CommandType.StoredProcedure};
				executeCommand.Parameters.AddWithValue("@bibid", BibID);

				// Create the adapter
				SqlDataAdapter adapter = new SqlDataAdapter(executeCommand);

				// Get the datatable
				DataSet valueSet = new DataSet();
				adapter.Fill(valueSet);

				// If there was either no match, or more than one, return null
				if ((valueSet.Tables.Count == 0) || (valueSet.Tables[0] == null) || (valueSet.Tables[0].Rows.Count == 0))
				{
					return null;
				}

				// Create the object
				SobekCM_Items_In_Title returnValue = new SobekCM_Items_In_Title(valueSet.Tables[0]);

				// Return the fully built object
				return returnValue;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Gets the high level report of which items exist in which milestone for an aggregation </summary>
		/// <param name="AggregationCode"> Code for the item aggregation of interest </param>
		/// <param name="Tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Table with the milestone information </returns>
		/// <remarks> This calls the 'Tracking_Item_Milestone_Report' stored procedure.</remarks>
		public static DataTable Tracking_Get_Milestone_Report(string AggregationCode, Custom_Tracer Tracer)
		{
			if (Tracer != null)
			{
				Tracer.Add_Trace("SobekCM_Database.Tracking_Get_Milestone_Report", "");
			}

			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@aggregation_code", AggregationCode);

				// Define a temporary dataset
				DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "Tracking_Item_Milestone_Report", paramList);

				// Return the built argument set
				return tempSet.Tables[0];
			}
			catch
			{
				return null;
			}
		}

		#endregion

		#region Methods pulled over from old Tracking Database

		/// <summary> Gets the history and archive information about a single item from the tracking database</summary>
		/// <param name="ItemID"> Primary key for this item in the database </param>
		/// <param name="Tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns>Dataset which contains the history and archive information for this item</returns>
		/// <remarks> This calls the 'Tracking_Get_History_Archives' stored procedure. </remarks>
		public static DataSet Tracking_Get_History_Archives(int ItemID, Custom_Tracer Tracer)
		{
			if (Tracer != null)
			{
				Tracer.Add_Trace("SobekCM_Database.Tracking_Get_History_Archives", String.Empty);
			}

			try
			{
				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@itemid", ItemID);

				return SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "Tracking_Get_History_Archives", paramList);
			}
			catch (Exception ee)
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Gets the list of all items which have been modified in this library from the history/workflow information over the last week </summary>
		/// <param name="Tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Dataset which contains all items which have recently been modified in this library from the tracking database's history/workflow information</returns>
		/// <remarks> This calls the 'Tracking_Update_List' stored procedure. </remarks>
		public static DataTable Tracking_Update_List(Custom_Tracer Tracer)
		{
			Tracer.Add_Trace("SobekCM_Database.Tracking_Update_List", String.Empty);

			try
			{
				DateTime sinceDate = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0));

				SqlParameter[] paramList = new SqlParameter[1];
				paramList[0] = new SqlParameter("@sinceDate", sinceDate.ToShortDateString());

				return SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "Tracking_Update_List", paramList).Tables[0];
			}
			catch ( Exception ee )
			{
				lastException = ee;
				return null;
			}
		}

		/// <summary> Marks an item as been editing online through UFDC </summary>
		/// <param name="ItemID"> Primary key for the item having a progress/worklog entry added </param>
		/// <param name="User">User name who did the edit</param>
		/// <param name="UserNotes">Any user notes about this edit</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'Tracking_Online_Edit_Complete' stored procedure. </remarks>
		public static bool Tracking_Online_Edit_Complete(int ItemID, string User, string UserNotes)
		{
			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[3];
				paramList[0] = new SqlParameter("@itemid", ItemID);
				paramList[1] = new SqlParameter("@user", User);
				paramList[2] = new SqlParameter("@usernotes", UserNotes);

				// Execute this non-query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "Tracking_Online_Edit_Complete", paramList);

				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Marks an item as having been submitted online </summary>
		/// <param name="ItemID"> Primary key for the item having a progress/worklog entry added </param>
		/// <param name="User">User name who submitted this item</param>
		/// <param name="UserNotes">Any user notes about this new item</param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'Tracking_Online_Submit_Complete' stored procedure. </remarks>
		public static bool Tracking_Online_Submit_Complete(int ItemID, string User, string UserNotes)
		{
			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[3];
				paramList[0] = new SqlParameter("@itemid", ItemID);
				paramList[1] = new SqlParameter("@user", User);
				paramList[2] = new SqlParameter("@usernotes", UserNotes);

				// Execute this non-query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "Tracking_Online_Submit_Complete", paramList);

				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Marks an item as having been loaded as a new item by the bulk loader </summary>
		/// <param name="BibID"> Bibliographic identifier for the item to which to add the new history/worklog </param>
		/// <param name="VID"> Volume identifier for the item to which to add the new history/worklog </param>
		/// <param name="User"> User who performed this work or initiated this work </param>
		/// <param name="UserNotes"> Any notes generated during the work or by the work initiator </param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'Tracking_Load_New_Complete' stored procedure. </remarks>
		public static bool Tracking_Load_New_Complete(string BibID, string VID, string User, string UserNotes)
		{
			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[4];
				paramList[0] = new SqlParameter("@bibid", BibID);
				paramList[1] = new SqlParameter("@vid", VID);
				paramList[2] = new SqlParameter("@user", User);
				paramList[3] = new SqlParameter("@usernotes", UserNotes);

				// Execute this non-query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "Tracking_Load_New_Complete", paramList);
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Marks an item as having been loaded as a replacement item by the bulk loader </summary>
		/// <param name="BibID"> Bibliographic identifier for the item to which to add the new history/worklog </param>
		/// <param name="VID"> Volume identifier for the item to which to add the new history/worklog </param>
		/// <param name="User"> User who performed this work or initiated this work </param>
		/// <param name="UserNotes"> Any notes generated during the work or by the work initiator </param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'Tracking_Load_Replacement_Complete' stored procedure. </remarks>
		public static bool Tracking_Load_Replacement_Complete(string BibID, string VID, string User, string UserNotes)
		{
			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[4];
				paramList[0] = new SqlParameter("@bibid", BibID);
				paramList[1] = new SqlParameter("@vid", VID);
				paramList[2] = new SqlParameter("@user", User);
				paramList[3] = new SqlParameter("@usernotes", UserNotes);

				// Execute this non-query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "Tracking_Load_Replacement_Complete", paramList);
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Marks an item as having a metadata update loaded by the bulk loader </summary>
		/// <param name="BibID"> Bibliographic identifier for the item to which to add the new history/worklog </param>
		/// <param name="VID"> Volume identifier for the item to which to add the new history/worklog </param>
		/// <param name="User"> User who performed this work or initiated this work </param>
		/// <param name="UserNotes"> Any notes generated during the work or by the work initiator </param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'Tracking_Load_Metadata_Update_Complete' stored procedure. </remarks>
		public static bool Tracking_Load_Metadata_Update_Complete(string BibID, string VID, string User, string UserNotes)
		{
			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[4];
				paramList[0] = new SqlParameter("@bibid", BibID);
				paramList[1] = new SqlParameter("@vid", VID);
				paramList[2] = new SqlParameter("@user", User);
				paramList[3] = new SqlParameter("@usernotes", UserNotes);

				// Execute this non-query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "Tracking_Load_Metadata_Update_Complete", paramList);
				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Marks an item as been digitally acquired </summary>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'Tracking_Digital_Acquisition_Complete' stored procedure. </remarks>
		public static bool Tracking_Digital_Acquisition_Complete( string BibID, string VID, string User, string Location, DateTime Date ) 
		{
			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[5];
				paramList[0] = new SqlParameter("@bibid", BibID);
				paramList[1] = new SqlParameter("@vid", VID);
				paramList[2] = new SqlParameter("@user", User);
				paramList[3] = new SqlParameter("@storagelocation", Location);
				paramList[4] = new SqlParameter("@date", Date);

				// Execute this non-query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "Tracking_Digital_Acquisition_Complete", paramList);

				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		/// <summary> Marks an item as been image processed </summary>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'Tracking_Image_Processing_Complete' stored procedure. </remarks>
		public static bool Tracking_Image_Processing_Complete(string BibID, string VID, string User, string Location, DateTime Date)
		{
			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[5];
				paramList[0] = new SqlParameter("@bibid", BibID);
				paramList[1] = new SqlParameter("@vid", VID);
				paramList[2] = new SqlParameter("@user", User);
				paramList[3] = new SqlParameter("@storagelocation", Location);
				paramList[4] = new SqlParameter("@date", Date);

				// Execute this non-query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "Tracking_Image_Processing_Complete", paramList);

				return true;
			}
			catch (Exception ee)
			{
				lastException = ee;
				return false;
			}
		}

		#endregion

		#region Method to save a FDA report to the database

		/// <summary> Saves all the pertinent information from a received Florida Digital Archive (FDA) ingest report </summary>
		/// <param name="Report"> Object containing all the data from the received FDA report </param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		public static bool FDA_Report_Save(FDA_Report_Data Report)
		{
			// Try to get the bibid and vid from the package name
			string bibid = String.Empty;
			string vid = String.Empty;
			if ((Report.Package.Length == 16) && (Report.Package[10] == '_'))
			{
				bibid = Report.Package.Substring(0, 10);
				vid = Report.Package.Substring(11, 5);
			}

			// If the package name was bib id without VID
			if (Report.Package.Length == 10)
			{
				bibid = Report.Package;
			}

			// Save the report information to the database
			int reportid = FDA_Report_Save(Report.Package, Report.IEID, Report.Report_Type_String, Report.Date, Report.Account, Report.Project, Report.Warnings, Report.Message_Note, bibid, vid);

			// If no error, continue
			return reportid > 0;
		}

		/// <summary> Save the information about a FDA report to the database </summary>
		/// <param name="Package">ID of the submission package sent to FDA.  (End user's id)</param>
		/// <param name="IEID">Intellectual Entity ID assigned by FDA</param>
		/// <param name="FdaReportType">Type of FDA report received</param>
		/// <param name="Report_Date">Date FDA was generated</param>
		/// <param name="Account">Account information for the FDA submission package</param>
		/// <param name="Project">Project information for the FDA submission package</param>
		/// <param name="Warnings">Number of warnings in this package</param>
		/// <param name="BibID">Bibliographic Identifier</param>
		/// <param name="VID">Volume Identifier</param>
		/// <param name="Message"> Message included in the FDA report received </param>
		/// <returns>Primary key for the report in the database, or -1 on failure</returns>
		/// <remarks>This calls the FDA_Report_Save stored procedure in the database</remarks>
		public static int FDA_Report_Save(string Package, string IEID, string FdaReportType, DateTime Report_Date, string Account, string Project, int Warnings, string Message, string BibID, string VID)
		{
			// If there is no connection string, return -1
			if (connectionString.Length == 0)
				return -1;

			try
			{
				// Create the SQL Connection
				SqlConnection sqlConn = new SqlConnection(connectionString);

				// Create the SQL Command
				SqlCommand addReport = new SqlCommand("FDA_Report_Save", sqlConn)
										   {CommandType = CommandType.StoredProcedure};

				// Add all of the parameters
				addReport.Parameters.AddWithValue("@Package", Package);
				addReport.Parameters.AddWithValue("@IEID", IEID);
				addReport.Parameters.AddWithValue("@FdaReportType", FdaReportType);
				addReport.Parameters.AddWithValue("@Report_Date", Report_Date);
				addReport.Parameters.AddWithValue("@Account", Account);
				addReport.Parameters.AddWithValue("@Project", Project);
				addReport.Parameters.AddWithValue("@Warnings", Warnings);
				addReport.Parameters.AddWithValue("@Message", Message);
				addReport.Parameters.AddWithValue("@BibID", BibID);
				addReport.Parameters.AddWithValue("@VID", VID);

				// Add a final parameter to receive the primary key back from the database
				addReport.Parameters.AddWithValue("@FdaReportID", -1);
				addReport.Parameters["@FdaReportID"].Direction = ParameterDirection.InputOutput;

				// Open the SQL Connection and execute the stored procedure
				sqlConn.Open();
				addReport.ExecuteNonQuery();
				sqlConn.Close();

				// Get and return the primary key
				return Convert.ToInt32(addReport.Parameters["@FdaReportID"].Value);
			}
			catch
			{
				// In the case of an error, return -1
				return -1;
			}
		}

		#endregion

		#region Methods relating to sending emails from and logging emails in the database

		/// <summary> Send an email using databse mail through the SQL database </summary>
		/// <param name="Recipient_List"> List of recepients, seperated by a semi-colon </param>
		/// <param name="Subject_Line"> Subject line for the email to send </param>
		/// <param name="Email_Body"> Body of the email to send</param>
		/// <param name="isHTML"> Flag indicates if the email body is HTML-encoded, or plain text </param>
		/// <param name="isContactUs"> Flag indicates if this was sent from the 'Contact Us' feature of the library, rather than from a mySobek feature such as email your bookshelf </param>
		/// <param name="ReplyToEmailID"> Primary key of the previous email, if this is a reply to a previously logged email </param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Send_Email' stored procedure to send and log this email. </remarks>
		public static bool Send_Database_Email(string Recipient_List, string Subject_Line, string Email_Body, bool isHTML, bool isContactUs, int ReplyToEmailID )
		{
			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[6];
				paramList[0] = new SqlParameter("@recipients_list", Recipient_List);
				paramList[1] = new SqlParameter("@subject_line", Subject_Line);
				paramList[2] = new SqlParameter("@email_body", Email_Body);
				paramList[3] = new SqlParameter("@html_format", isHTML);
				paramList[4] = new SqlParameter("@contact_us", isContactUs);
				if (ReplyToEmailID > 0)
				{
					paramList[5] = new SqlParameter("@replytoemailid", ReplyToEmailID);
				}
				else
				{
					paramList[5] = new SqlParameter("@replytoemailid", DBNull.Value);
				}

				// Execute this non-query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Send_Email", paramList);

				return true;
			}
			catch (Exception ee)
			{
				// Pass this exception onto the method to handle it
				lastException = ee;
				return false;
			}
		}

		/// <summary> Log the fact an email was sent via a different system than the databse mail </summary>
		/// <param name="Sender"> Name of the sender indicated in the sent email </param>
		/// <param name="Recipient_List"> List of recepients, seperated by a semi-colon </param>
		/// <param name="Subject_Line"> Subject line for the email to log </param>
		/// <param name="Email_Body"> Body of the email to log</param>
		/// <param name="isHTML"> Flag indicates if the email body is HTML-encoded, or plain text </param>
		/// <param name="isContactUs"> Flag indicates if this was sent from the 'Contact Us' feature of the library, rather than from a mySobek feature such as email your bookshelf </param>
		/// <param name="ReplyToEmailID"> Primary key of the previous email, if this is a reply to a previously logged email </param>
		/// <returns> TRUE if successful, otherwise FALSE </returns>
		/// <remarks> This calls the 'SobekCM_Log_Email' stored procedure. </remarks>
		public static bool Log_Sent_Email( string Sender, string Recipient_List, string Subject_Line, string Email_Body, bool isHTML, bool isContactUs, int ReplyToEmailID)
		{
			try
			{
				// Build the parameter list
				SqlParameter[] paramList = new SqlParameter[7];
				paramList[0] = new SqlParameter("@sender", Sender);
				paramList[1] = new SqlParameter("@recipients_list", Recipient_List);
				paramList[2] = new SqlParameter("@subject_line", Subject_Line);
				paramList[3] = new SqlParameter("@email_body", Email_Body);
				paramList[4] = new SqlParameter("@html_format", isHTML);
				paramList[5] = new SqlParameter("@contact_us", isContactUs);
				if (ReplyToEmailID > 0)
				{
					paramList[6] = new SqlParameter("@replytoemailid", ReplyToEmailID);
				}
				else
				{
					paramList[6] = new SqlParameter("@replytoemailid", DBNull.Value);
				}

				// Execute this non-query stored procedure
				SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "SobekCM_Log_Email", paramList);

				return true;
			}
			catch (Exception ee)
			{
				// Pass this exception onto the method to handle it
				lastException = ee;
				return false;
			}
		}

		#endregion

		#region Methods related to OAI-PMH

		/// <summary> Gets the list of all OAI-enabled item aggregations </summary>
		/// <returns> DataTable with all the data about the OAI-enabled item aggregations, including code, name, description, last item added date, and any aggregation-level OAI_Metadata  </returns>
		/// <remarks> This calls the 'SobekCM_Get_OAI_Sets' stored procedure  <br /><br />
		/// This is called by the <see cref="Oai_MainWriter"/> class. </remarks> 
		public static DataTable Get_OAI_Sets()
		{
			// Define a temporary dataset
			DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_Get_OAI_Sets");

			// If there was no data for this collection and entry point, return null (an ERROR occurred)
			if ((tempSet.Tables.Count == 0) || (tempSet.Tables[0] == null) || (tempSet.Tables[0].Rows.Count == 0))
			{
				return null;
			}

			// Return the first table from the returned dataset
			return tempSet.Tables[0];
		}

		/// <summary> Returns a list of either identifiers or records for either the entire system or a single
		/// set, to be served through the OAI-PMH server  </summary>
		/// <param name="Set_Code"> Code the OAI-PMH set (which is really an aggregation code)</param>
		/// <param name="Data_Code"> Code for the metadata to be served ( usually oai_dc )</param>
		/// <param name="From_Date"> Date from which to pull records which have changed </param>
		/// <param name="Until_Date"> Date to pull up to by last modified date on the records </param>
		/// <param name="Page_Size"> Number of records to include in a single 'page' of OAI-PMH results </param>
		/// <param name="Page_Number"> Page number of the results to return </param>
		/// <param name="Include_Record"> Flag indicates whether the full records should be included, or just the identifier </param>
		/// <returns> DataTable of all the OAI-PMH record information </returns>
		/// <remarks> This calls the 'SobekCM_Get_OAI_Data' stored procedure  <br /><br />
		/// This is called by the <see cref="Oai_MainWriter"/> class. </remarks> 
		public static List<OAI.OAI_Record> Get_OAI_Data( string Set_Code, string Data_Code, DateTime From_Date, DateTime Until_Date, int Page_Size, int Page_Number, bool Include_Record )
		{
			// Create the connection
			SqlConnection connect = new SqlConnection(connectionString);

			// Create the command 
			SqlCommand executeCommand = new SqlCommand("SobekCM_Get_OAI_Data", connect) { CommandType = CommandType.StoredProcedure };

			executeCommand.Parameters.AddWithValue("@aggregationcode", Set_Code);
			executeCommand.Parameters.AddWithValue("@data_code", Data_Code);
			executeCommand.Parameters.AddWithValue("@from", From_Date);
			executeCommand.Parameters.AddWithValue("@until", Until_Date);
			executeCommand.Parameters.AddWithValue("@pagesize", Page_Size);
			executeCommand.Parameters.AddWithValue("@pagenumber", Page_Number);
			executeCommand.Parameters.AddWithValue("@include_data", Include_Record);

			// Determine the column for the date
			int date_column = 2;
			if ( Include_Record )
				date_column = 3;

			// Create the data reader
			connect.Open();
			List<OAI.OAI_Record> returnVal = new List<OAI.OAI_Record>();
			try
			{
				using (SqlDataReader reader = executeCommand.ExecuteReader())
				{
					// Read in each row
					while (reader.Read())
					{
						if (Include_Record)
							returnVal.Add(new OAI.OAI_Record(reader.GetString(1), reader.GetString(2), reader.GetDateTime(3)));
						else
							returnVal.Add(new OAI.OAI_Record(reader.GetString(1), reader.GetDateTime(2)));
					}

					// Close the reader
					reader.Close();
				}
			}
			catch (Exception ee )
			{
			    string message = ee.Message;
			    bool error = true;
			}
			connect.Close();

			return returnVal;
		}

		/// <summary> Returns a single OAI-PMH record, by identifier ( BibID ) </summary>
		/// <param name="Identifier"> Code the OAI-PMH record  (which is really the BibID)</param>
		/// <param name="Data_Code"> Code for the metadata to be served ( usually oai_dc )</param>
		/// <returns> Single OAI-PMH record </returns>
		/// <remarks> This calls the 'SobekCM_Get_OAI_Data_Item' stored procedure  <br /><br />
		/// This is called by the <see cref="Oai_MainWriter"/> class. </remarks> 
		public static OAI.OAI_Record Get_OAI_Record( string Identifier, string Data_Code )
		{
			// Create the connection
			SqlConnection connect = new SqlConnection(connectionString);

			// Create the command 
			SqlCommand executeCommand = new SqlCommand("SobekCM_Get_OAI_Data_Item", connect) { CommandType = CommandType.StoredProcedure };

			executeCommand.Parameters.AddWithValue("@bibid", Identifier);
			executeCommand.Parameters.AddWithValue("@data_code", Data_Code);

			// Create the data reader
			connect.Open();
			OAI.OAI_Record returnRecord = null;
			try
			{
				using (SqlDataReader reader = executeCommand.ExecuteReader())
				{
					// Read in the first row
					if (reader.Read())
					{
						returnRecord = new OAI.OAI_Record(reader.GetString(1), reader.GetString(2), reader.GetDateTime(3));
					}

					// Close the reader
					reader.Close();
				}
			}
			catch
			{
				
			}
			connect.Close();

			return returnRecord;
		}




		///// <summary> Returns the items and groups along with group creation date for inclusion in the OAI data source files </summary>
		///// <param name="AggregationCode"> Aggregation code for the item aggregation in question (or empty string) </param>
		///// <remarks> This calls the 'SobekCM_OAI_Item_List' stored procedure <br /><br />
		///// This is used during creation of the OAI fields at the group level</remarks>
		//public static DataSet Get_OAI_Item_List(string AggregationCode)
		//{
		//    try
		//    {
		//        // build the parameter list
		//        SqlParameter[] paramList = new SqlParameter[2];
		//        paramList[0] = new SqlParameter("@app_server_name", "lib-ufdcweb3");
		//        paramList[1] = new SqlParameter("@collection_code", AggregationCode);

		//        // Define a temporary dataset
		//        DataSet tempSet = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "SobekCM_OAI_Item_List", paramList);

		//        // Return the first table from the returned dataset
		//        return tempSet;
		//    }
		//    catch (Exception ee)
		//    {
		//        lastException = ee;
		//        return null;
		//    }
		//}

		#endregion

        #region Methods used by the Statistics Usage Reader

        /// <summary> Gets all the tables ued during the process of reading the statistics 
        /// from the web iis logs and creating the associated SQL commands  </summary>
        /// <returns> Large dataset with several tables ( all items, all titles, aggregations, etc.. )</returns>
        public static DataSet Get_Statistics_Lookup_Tables()
        {
            try
            {
                // Create the connection
                SqlConnection connect = new SqlConnection(connectionString);

                // Create the command 
                SqlCommand executeCommand = new SqlCommand("SobekCM_Statistics_Lookup_Tables", connect);
                executeCommand.CommandType = CommandType.StoredProcedure;

                // Create the adapter
                SqlDataAdapter adapter = new SqlDataAdapter(executeCommand);

                // Create the dataset
                DataSet returnValue = new DataSet();

                // Fill the dataset
                adapter.Fill(returnValue);

                // Return the results
                return returnValue;
            }
            catch (Exception ee)
            {
                throw ee;
            }
        }


        #endregion
	}

}
